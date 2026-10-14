using AgentHud.Contracts;

namespace AgentHud.Providers.Codex;

public sealed class CodexNativeProvider : IAgentProvider
{
    private const int MaxSessions = 24;
    private readonly string _codexHome;

    public CodexNativeProvider(string? codexHome = null)
    {
        _codexHome = ResolveCodexHome(codexHome);
    }

    public string Id => "codex";

    public string DisplayName => "Codex";

    public ProviderCapability Capabilities =>
        ProviderCapability.Discovery |
        ProviderCapability.State;

    public async Task<IReadOnlyList<AgentSessionSnapshot>> GetSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var candidates = EnumerateRollouts()
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .Take(MaxSessions * 2)
            .ToArray();

        if (candidates.Length == 0)
        {
            return Array.Empty<AgentSessionSnapshot>();
        }

        var bySession = new Dictionary<string, AgentSessionSnapshot>(StringComparer.Ordinal);

        foreach (var file in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var snapshot = await CodexRolloutReader.TryReadAsync(
                file.FullName,
                file.LastWriteTimeUtc,
                cancellationToken);

            if (snapshot is null)
            {
                continue;
            }

            if (!bySession.TryGetValue(snapshot.SessionId, out var existing) ||
                snapshot.ObservedAt > existing.ObservedAt)
            {
                bySession[snapshot.SessionId] = snapshot;
            }
        }

        return bySession.Values
            .OrderByDescending(static session => session.State == AgentSessionState.Active)
            .ThenByDescending(static session => session.ObservedAt)
            .Take(MaxSessions)
            .ToArray();
    }

    private IEnumerable<FileInfo> EnumerateRollouts()
    {
        var options = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false,
            MatchCasing = MatchCasing.CaseInsensitive,
            MaxRecursionDepth = 8
        };

        foreach (var directoryName in new[] { "sessions", "archived_sessions" })
        {
            var directory = Path.Combine(_codexHome, directoryName);
            if (!Directory.Exists(directory))
            {
                continue;
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directory, "rollout-*.jsonl", options);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            using var enumerator = files.GetEnumerator();
            while (true)
            {
                string path;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    path = enumerator.Current;
                }
                catch (IOException)
                {
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }

                FileInfo info;
                try
                {
                    info = new FileInfo(path);
                    _ = info.Length;
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                yield return info;
            }
        }
    }

    private static string ResolveCodexHome(string? overridePath)
    {
        var configured = overridePath;
        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = Environment.GetEnvironmentVariable("CODEX_HOME");
        }

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(
                Environment.ExpandEnvironmentVariables(configured.Trim()));
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".codex");
    }
}
