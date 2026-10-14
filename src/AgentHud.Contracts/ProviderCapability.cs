namespace AgentHud.Contracts;

[Flags]
public enum ProviderCapability
{
    None = 0,
    Discovery = 1 << 0,
    State = 1 << 1,
    Usage = 1 << 2,
    DeepLink = 1 << 3,
    Approval = 1 << 4
}
