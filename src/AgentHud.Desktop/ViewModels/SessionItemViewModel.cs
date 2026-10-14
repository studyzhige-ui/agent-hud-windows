using System.Windows.Media;
using AgentHud.Contracts;

namespace AgentHud.Desktop.ViewModels;

internal sealed class SessionItemViewModel
{
    private static readonly Brush ActiveBrush = CreateBrush("#2A9D72");
    private static readonly Brush AttentionBrush = CreateBrush("#C9832B");
    private static readonly Brush NeutralBrush = CreateBrush("#7A8491");
    private static readonly Brush DisconnectedBrush = CreateBrush("#9B5B66");
    private static readonly Brush CodexBrush = CreateBrush("#4E83D1");
    private static readonly Brush ClaudeBrush = CreateBrush("#C17C4C");
    private static readonly Brush DemoBrush = CreateBrush("#7C6FC2");

    public SessionItemViewModel(AgentSessionSnapshot snapshot)
    {
        ProviderName = snapshot.ProviderName;
        DisplayName = snapshot.DisplayName;
        StatusText = snapshot.StatusText;
        IsDemo = snapshot.IsDemo;
        ProviderInitial = snapshot.ProviderName.Length == 0
            ? "?"
            : snapshot.ProviderName[..1].ToUpperInvariant();
        UsageText = snapshot.UsagePercent is double usage
            ? $"Session usage · {usage:P0}"
            : "Usage unavailable";
        StateLabel = snapshot.State switch
        {
            AgentSessionState.RequiresAttention => "Needs attention",
            AgentSessionState.Active => "Active",
            AgentSessionState.Idle => "Idle",
            AgentSessionState.Completed => "Completed",
            AgentSessionState.Disconnected => "Disconnected",
            _ => "Unknown"
        };
        StatusBrush = snapshot.State switch
        {
            AgentSessionState.RequiresAttention => AttentionBrush,
            AgentSessionState.Active => ActiveBrush,
            AgentSessionState.Disconnected => DisconnectedBrush,
            _ => NeutralBrush
        };
        ProviderBrush = snapshot.ProviderName.Contains("Codex", StringComparison.OrdinalIgnoreCase)
            ? CodexBrush
            : snapshot.ProviderName.Contains("Claude", StringComparison.OrdinalIgnoreCase)
                ? ClaudeBrush
                : DemoBrush;
    }

    public string ProviderName { get; }

    public string ProviderInitial { get; }

    public string DisplayName { get; }

    public string StatusText { get; }

    public string StateLabel { get; }

    public string UsageText { get; }

    public bool IsDemo { get; }

    public Brush StatusBrush { get; }

    public Brush ProviderBrush { get; }

    private static Brush CreateBrush(string hex)
    {
        var brush = (Brush)new BrushConverter().ConvertFromString(hex)!;
        brush.Freeze();
        return brush;
    }
}
