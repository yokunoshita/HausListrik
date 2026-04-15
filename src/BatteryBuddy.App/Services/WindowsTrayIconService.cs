using System.Drawing;
using System.Windows.Forms;
using BatteryBuddy.App.Presentation.ViewModels;

namespace BatteryBuddy.App.Services;

public sealed class WindowsTrayIconService : ITrayIconService
{
    private NotifyIcon? _notifyIcon;

    public void Initialize(MainViewModel viewModel, Action showWindow, Action exitApplication)
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "Haus Listrik",
            Visible = false,
            ContextMenuStrip = BuildMenu(viewModel, showWindow, exitApplication)
        };

        _notifyIcon.DoubleClick += (_, _) => showWindow();
    }

    public void Show()
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = true;
        }
    }

    public void Hide()
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
        }
    }

    public void ShowInfo(string title, string message)
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(2500);
    }

    public void Dispose()
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _notifyIcon = null;
    }

    private static ContextMenuStrip BuildMenu(MainViewModel viewModel, Action showWindow, Action exitApplication)
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add("Open Haus Listrik", null, (_, _) => showWindow());
        menu.Items.Add("Save Settings", null, (_, _) => viewModel.SaveSettingsCommand.Execute(null));
        menu.Items.Add("Start Monitor", null, (_, _) => viewModel.StartCommand.Execute(null));
        menu.Items.Add("Stop Monitor", null, (_, _) => viewModel.StopCommand.Execute(null));
        menu.Items.Add("Open Voice Pack Folder", null, (_, _) => viewModel.OpenVoicePackFolderCommand.Execute(null));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exitApplication());

        return menu;
    }
}
