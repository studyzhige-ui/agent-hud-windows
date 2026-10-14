using System.Globalization;
using System.Text;
using System.Text.Json;
using AgentHud.Contracts;

namespace AgentHud.Providers.Codex;

internal static class CodexRolloutReader
{
    private const int HeadBytes = 192 * 1024;
    private const int TailBytes = 512 * 1024;
    private const int FullReadThreshold = HeadBytes + TailBytes;
    private static readonly TimeSpan ActiveFreshness = TimeSpan.FromMinutes(10);

    public static async Task<AgentSessionSnapshot?> TryReadAsync(
        string path,
        DateTime lastWriteTimeUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 16 * 1024,
                FileOptions.Asynchronous | FileOptions.RandomAccess);

            if (stream.Length <= 0)
            {
                return null;
            }

            var state = new ParseState();

            if (stream.Length <= FullReadThreshold)
            {
                var bytes = await ReadRangeAsync(stream, 0, checked((int)stream.Length), cancellationToken);
                ParseLines(bytes, discardFirstPartialLine: false, state);
            }
            else
            {
                var head = await ReadRangeAsync(
                    stream,
                    0,
                    (int)Math.Min(HeadBytes, stream.Length),
                    cancellationToken);
                ParseLines(head, discardFirstPartialLine: false, state);

                var tailOffset = Math.Max(0, stream.Length - TailBytes);
                var tail = await ReadRangeAsync(
                    stream,
                    tailOffset,
                    checked((int)(stream.Length - tailOffset)),
                    cancellationToken);
                ParseLines(tail, discardFirstPartialLine: tailOffset > 0, state);
            }

            if (string.IsNullOrWhiteSpace(state.SessionId))
            {
                return null;
            }

            var observedAt = state.LastActivityAt ??
                new DateTimeOffset(DateTime.SpecifyKind(lastWriteTimeUtc, DateTimeKind.Utc));

            var now = DateTimeOffset.UtcNow;
            var sessionState = ResolveState(state, observedAt, now);
            var title = state.Title
                ?? WorkspaceName(state.WorkingDirectory)
                ?? "Codex session";
            var status = ResolveStatus(state, sessionState);

            return new AgentSessionSnapshot(
                ProviderId: "codex",
                ProviderName: state.ClientName ?? "Codex",
                SessionId: state.SessionId,
                DisplayName: title,
                State: sessionState,
                StatusText: status,
                ObservedAt: observedAt);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static AgentSessionState ResolveState(
        ParseState state,
        DateTimeOffset observedAt,
        DateTimeOffset now)
    {
        if (state.TurnRunning == true)
        {
            return now - observedAt <= ActiveFreshness
                ? AgentSessionState.Active
                : AgentSessionState.Unknown;
        }

        if (state.LastTerminalWasCompleted == true)
        {
            return AgentSessionState.Completed;
        }

        if (state.LastTerminalWasCompleted == false)
        {
            return AgentSessionState.Idle;
        }

        return state.LastActivityAt is null
            ? AgentSessionState.Unknown
            : AgentSessionState.Idle;
    }

    private static string ResolveStatus(ParseState state, AgentSessionState sessionState)
    {
        var modelSuffix = string.IsNullOrWhiteSpace(state.Model)
            ? string.Empty
            : $" · {state.Model}";

        return sessionState switch
        {
            AgentSessionState.Active => $"Running{modelSuffix}",
            AgentSessionState.Completed => $"Last turn completed{modelSuffix}",
            AgentSessionState.Idle => $"Idle{modelSuffix}",
            AgentSessionState.Unknown when state.TurnRunning == true =>
                "Running turn is stale; waiting for fresh evidence",
            _ => "Session state unavailable"
        };
    }

    private static async Task<byte[]> ReadRangeAsync(
        FileStream stream,
        long offset,
        int count,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[count];
        stream.Seek(offset, SeekOrigin.Begin);

        var read = 0;
        while (read < count)
        {
            var chunk = await stream.ReadAsync(
                buffer.AsMemory(read, count - read),
                cancellationToken);
            if (chunk == 0)
            {
                break;
            }

            read += chunk;
        }

        if (read == buffer.Length)
        {
            return buffer;
        }

        Array.Resize(ref buffer, read);
        return buffer;
    }

    private static void ParseLines(
        byte[] bytes,
        bool discardFirstPartialLine,
        ParseState state)
    {
        if (bytes.Length == 0)
        {
            return;
        }

        var text = Encoding.UTF8.GetString(bytes);
        var lines = text.Split('\n');

        var startIndex = discardFirstPartialLine ? 1 : 0;
        for (var index = startIndex; index < lines.Length; index++)
        {
            ParseLine(lines[index], state);
        }
    }

    private static void ParseLine(string line, ParseState state)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeNode) ||
                !root.TryGetProperty("payload", out var payload) ||
                payload.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            var type = typeNode.GetString();
            var timestamp = ReadTimestamp(root);

            switch (type)
            {
                case "session_meta":
                    ReadSessionMetadata(payload, state);
                    break;
                case "turn_context":
                    if (payload.TryGetProperty("model", out var modelNode))
                    {
                        state.Model = modelNode.GetString() ?? state.Model;
                    }
                    break;
                case "event_msg":
                    ReadEvent(payload, timestamp, state);
                    break;
            }
        }
        catch (JsonException)
        {
            // A rollout can be concurrently appended. Ignore malformed/partial lines
            // and retry them on the next refresh.
        }
    }

    private static void ReadSessionMetadata(JsonElement payload, ParseState state)
    {
        if (state.SessionId is null &&
            payload.TryGetProperty("id", out var idNode))
        {
            state.SessionId = idNode.GetString();
        }

        if (state.WorkingDirectory is null &&
            payload.TryGetProperty("cwd", out var cwdNode))
        {
            state.WorkingDirectory = cwdNode.GetString();
        }

        if (payload.TryGetProperty("source", out var sourceNode) &&
            sourceNode.ValueKind == JsonValueKind.String)
        {
            state.ClientName = sourceNode.GetString() switch
            {
                "cli" => "Codex CLI",
                "exec" => "Codex CLI · exec",
                "vscode" => "Codex IDE",
                _ => state.ClientName
            };
        }

        if (state.ClientName is null &&
            payload.TryGetProperty("originator", out var originatorNode))
        {
            var originator = originatorNode.GetString();
            if (!string.IsNullOrWhiteSpace(originator))
            {
                state.ClientName = $"Codex · {TrimSingleLine(originator, 28)}";
            }
        }
    }

    private static void ReadEvent(
        JsonElement payload,
        DateTimeOffset? timestamp,
        ParseState state)
    {
        if (!payload.TryGetProperty("type", out var kindNode))
        {
            return;
        }

        var kind = kindNode.GetString();
        if (timestamp is { } observedAt)
        {
            state.LastActivityAt = Max(state.LastActivityAt, observedAt);
        }

        switch (kind)
        {
            case "task_started":
                state.TurnRunning = true;
                state.LastTerminalWasCompleted = null;
                break;
            case "task_complete":
                state.TurnRunning = false;
                state.LastTerminalWasCompleted =
                    !payload.TryGetProperty("error", out var error) ||
                    IsEmptyError(error);
                break;
            case "turn_aborted":
                state.TurnRunning = false;
                state.LastTerminalWasCompleted = false;
                break;
            case "user_message":
                if (state.Title is null &&
                    payload.TryGetProperty("message", out var messageNode))
                {
                    state.Title = TitleFromText(messageNode.GetString());
                }
                break;
            case "item_completed":
                ReadCompletedItem(payload, state);
                break;
        }
    }

    private static void ReadCompletedItem(JsonElement payload, ParseState state)
    {
        if (state.Title is not null ||
            !payload.TryGetProperty("item", out var item) ||
            item.ValueKind != JsonValueKind.Object ||
            !item.TryGetProperty("type", out var typeNode) ||
            typeNode.GetString() != "UserMessage" ||
            !item.TryGetProperty("content", out var content) ||
            content.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var part in content.EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object ||
                !part.TryGetProperty("type", out var partType) ||
                !string.Equals(partType.GetString(), "text", StringComparison.OrdinalIgnoreCase) ||
                !part.TryGetProperty("text", out var textNode))
            {
                continue;
            }

            state.Title = TitleFromText(textNode.GetString());
            if (state.Title is not null)
            {
                return;
            }
        }
    }

    private static DateTimeOffset? ReadTimestamp(JsonElement root)
    {
        if (!root.TryGetProperty("timestamp", out var node))
        {
            return null;
        }

        var text = node.GetString();
        return DateTimeOffset.TryParse(
            text,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var timestamp)
            ? timestamp
            : null;
    }

    private static bool IsEmptyError(JsonElement error) =>
        error.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
        (error.ValueKind == JsonValueKind.String &&
         string.IsNullOrEmpty(error.GetString()));

    private static string? TitleFromText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return TrimSingleLine(value, 60);
    }

    private static string TrimSingleLine(string value, int maxLength)
    {
        var line = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        return line.Length <= maxLength
            ? line
            : line[..maxLength];
    }

    private static string? WorkspaceName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var trimmed = path.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        var name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static DateTimeOffset Max(DateTimeOffset? left, DateTimeOffset right) =>
        left is null || right > left ? right : left.Value;

    private sealed class ParseState
    {
        public string? SessionId { get; set; }

        public string? WorkingDirectory { get; set; }

        public string? Title { get; set; }

        public string? Model { get; set; }

        public string? ClientName { get; set; }

        public DateTimeOffset? LastActivityAt { get; set; }

        public bool? TurnRunning { get; set; }

        public bool? LastTerminalWasCompleted { get; set; }
    }
}
