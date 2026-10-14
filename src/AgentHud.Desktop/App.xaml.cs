using System.Windows;
using AgentHud.App;
using AgentHud.Desktop.Providers;
using AgentHud.Desktop.Services;
using AgentHud.Desktop.ViewModels;

namespace AgentHud.Desktop;

public partial class App : Application
{
    private TrayIconService? _trayIcon;
    private MainWindow? _window;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var providers = new[] { new DemoAgentProvider() };
        var aggregator = new SessionAggregator(providers);
        var viewModel = new MainViewModel(aggregator);

        _window = new MainWindow(viewModel);
        MainWindow = _window;

        _trayIcon = new TrayIconService(
            toggleWindow: _window.ToggleVisibility,
            exitApplication: Shutdown);

        _window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
