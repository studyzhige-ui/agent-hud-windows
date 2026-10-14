using AgentHud.Contracts;

namespace AgentHud.Desktop.Providers;

internal sealed class DemoAgentProvider : IAgentProvider
{
    public string Id => "demo";

    public string DisplayName => "Demo data";

    public ProviderCapability Capabilities =>
        ProviderCapability.Discovery |
        ProviderCapability.State |
        ProviderCapability.Usage;

    public Task<IReadOnlyList<AgentSessionSnapshot>> GetSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<AgentSessionSnapshot> sessions =
        [
            new(
                ProviderId: "codex-demo",
                ProviderName: "Codex",
                SessionId: "demo-codex-1",
                DisplayName: "Interview Copilot",
                State: AgentSessionState.Active,
                StatusText: "Editing provider architecture",
                ObservedAt: DateTimeOffset.UtcNow,
                UsagePercent: 0.64,
                IsDemo: true),
            new(
                ProviderId: "claude-demo",
                ProviderName: "Claude Code",
                SessionId: "demo-claude-1",
                DisplayName: "Agent HUD",
                State: AgentSessionState.RequiresAttention,
                StatusText: "Waiting for approval",
                ObservedAt: DateTimeOffset.UtcNow,
                UsagePercent: 0.31,
                IsDemo: true)
        ];

        return Task.FromResult(sessions);
    }
}
