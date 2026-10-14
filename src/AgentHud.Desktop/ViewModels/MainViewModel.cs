using System.Collections.ObjectModel;
using AgentHud.App;
using AgentHud.Contracts;

namespace AgentHud.Desktop.ViewModels;

internal sealed class MainViewModel : ObservableObject
{
    private readonly SessionAggregator _aggregator;
    private bool _isExpanded;
    private string _summaryText = "Starting local monitor…";
    private int _activeCount;
    private int _attentionCount;

    public MainViewModel(SessionAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public ObservableCollection<SessionItemViewModel> Sessions { get; } = [];

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public string SummaryText
    {
        get => _summaryText;
        private set => SetProperty(ref _summaryText, value);
    }

    public int ActiveCount
    {
        get => _activeCount;
        private set => SetProperty(ref _activeCount, value);
    }

    public int AttentionCount
    {
        get => _attentionCount;
        private set
        {
            if (SetProperty(ref _attentionCount, value))
            {
                OnPropertyChanged(nameof(AttentionText));
            }
        }
    }

    public string AttentionText => AttentionCount == 0 ? "No alerts" : $"{AttentionCount} need attention";

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var snapshots = await _aggregator.RefreshAsync(cancellationToken);

        Sessions.Clear();
        foreach (var snapshot in snapshots)
        {
            Sessions.Add(new SessionItemViewModel(snapshot));
        }

        ActiveCount = snapshots.Count(session =>
            session.State is AgentSessionState.Active or AgentSessionState.RequiresAttention);
        AttentionCount = snapshots.Count(session =>
            session.State is AgentSessionState.RequiresAttention);

        SummaryText = snapshots.Count switch
        {
            0 => "No agent sessions detected",
            _ when AttentionCount > 0 => AttentionText,
            _ => $"{ActiveCount} active · {snapshots.Count} sessions"
        };
    }
}
