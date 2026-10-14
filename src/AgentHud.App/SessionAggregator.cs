using AgentHud.Contracts;

namespace AgentHud.App;

public sealed class SessionAggregator
{
    private readonly IReadOnlyList<IAgentProvider> _providers;

    public SessionAggregator(IEnumerable<IAgentProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _providers = providers.ToArray();
    }

    public async Task<IReadOnlyList<AgentSessionSnapshot>> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        if (_providers.Count == 0)
        {
            return Array.Empty<AgentSessionSnapshot>();
        }

        var tasks = _providers.Select(provider => ReadProviderAsync(provider, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results
            .SelectMany(static sessions => sessions)
            .OrderByDescending(static session => Priority(session.State))
            .ThenByDescending(static session => session.ObservedAt)
            .ToArray();
    }

    private static async Task<IReadOnlyList<AgentSessionSnapshot>> ReadProviderAsync(
        IAgentProvider provider,
        CancellationToken cancellationToken)
    {
        try
        {
            return await provider.GetSessionsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return new[]
            {
                new AgentSessionSnapshot(
                    provider.Id,
                    provider.DisplayName,
                    $"{provider.Id}:unavailable",
                    provider.DisplayName,
                    AgentSessionState.Disconnected,
                    "Provider unavailable",
                    DateTimeOffset.UtcNow)
            };
        }
    }

    private static int Priority(AgentSessionState state) => state switch
    {
        AgentSessionState.RequiresAttention => 6,
        AgentSessionState.Active => 5,
        AgentSessionState.Idle => 4,
        AgentSessionState.Completed => 3,
        AgentSessionState.Unknown => 2,
        AgentSessionState.Disconnected => 1,
        _ => 0
    };
}
