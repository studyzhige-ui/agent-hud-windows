using System.Drawing;
using Forms = System.Windows.Forms;

namespace AgentHud.Desktop.Services;

internal sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Forms.ContextMenuStrip _menu;

    public TrayIconService(Action toggleWindow, Action exitApplication)
    {
        ArgumentNullException.ThrowIfNull(toggleWindow);
        ArgumentNullException.ThrowIfNull(exitApplication);

        _menu = new Forms.ContextMenuStrip();
        _menu.Items.Add("Show / Hide", null, (_, _) => toggleWindow());
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add("Exit Agent HUD", null, (_, _) => exitApplication());

        _icon = new Forms.NotifyIcon
        {
            Text = "Agent HUD",
            Icon = SystemIcons.Application,
            ContextMenuStrip = _menu,
            Visible = true
        };

        _icon.DoubleClick += (_, _) => toggleWindow();
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _menu.Dispose();
    }
}
