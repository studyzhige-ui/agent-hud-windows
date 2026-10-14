using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AgentHud.Desktop.ViewModels;

namespace AgentHud.Desktop;

public partial class MainWindow : Window
{
    private const double CollapsedHeight = 78;
    private const double ExpandedHeight = 500;
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _refreshTimer;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _refreshTimer.Tick += RefreshTimer_OnTick;

        Loaded += OnLoaded;
        Closed += (_, _) => _refreshTimer.Stop();
    }

    public void ToggleVisibility()
    {
        if (IsVisible)
        {
            Hide();
            return;
        }

        Show();
        PositionAtTopCenter();
        Activate();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        PositionAtTopCenter();
        await RefreshAsync();
        _refreshTimer.Start();
    }

    private async void RefreshTimer_OnTick(object? sender, EventArgs e) =>
        await RefreshAsync();

    private async Task RefreshAsync()
    {
        try
        {
            await _viewModel.RefreshAsync();
        }
        catch
        {
            // UI stays usable even if a provider refresh fails.
            // Structured diagnostics will replace this in a later milestone.
        }
    }

    private void ExpandButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.IsExpanded = !_viewModel.IsExpanded;

        if (_viewModel.IsExpanded)
        {
            DetailsPanel.Visibility = Visibility.Visible;
            ExpandButton.Content = "⌃";
            ExpandButton.ToolTip = "Collapse agent sessions";
            AnimateHeight(ExpandedHeight);
        }
        else
        {
            DetailsPanel.Visibility = Visibility.Collapsed;
            ExpandButton.Content = "⌄";
            ExpandButton.ToolTip = "Expand agent sessions";
            AnimateHeight(CollapsedHeight);
        }

        PositionAtTopCenter();
    }

    private void Shell_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed &&
            e.OriginalSource is not System.Windows.Controls.Button)
        {
            DragMove();
        }
    }

    private void AnimateHeight(double destination)
    {
        var animation = new DoubleAnimation
        {
            To = destination,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        BeginAnimation(HeightProperty, animation);
    }

    private void PositionAtTopCenter()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + Math.Max(12, (workArea.Width - Width) / 2);
        Top = workArea.Top + 12;
    }
}
