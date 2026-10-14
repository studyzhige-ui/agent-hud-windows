namespace AgentHud.Contracts;

public sealed record AgentSessionSnapshot(
    string ProviderId,
    string ProviderName,
    string SessionId,
    string DisplayName,
    AgentSessionState State,
    string StatusText,
    DateTimeOffset ObservedAt,
    double? UsagePercent = null,
    bool IsDemo = false);
