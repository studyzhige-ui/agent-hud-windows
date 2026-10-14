namespace AgentHud.Contracts;

public interface IAgentProvider
{
    string Id { get; }

    string DisplayName { get; }

    ProviderCapability Capabilities { get; }

    Task<IReadOnlyList<AgentSessionSnapshot>> GetSessionsAsync(
        CancellationToken cancellationToken = default);
}
