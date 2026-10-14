namespace AgentHud.Contracts;

public enum AgentSessionState
{
    Unknown = 0,
    Idle = 1,
    Active = 2,
    RequiresAttention = 3,
    Completed = 4,
    Disconnected = 5
}
