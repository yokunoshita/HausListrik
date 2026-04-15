using System.Windows;
using HausListrik.App.Presentation.ViewModels;
using HausListrik.App.Services;

namespace HausListrik.App;

public partial class MainWindow : Window
{
    private bool _forceExit;
    private MainViewModel? _viewModel;
    private ITrayIconService? _trayIconService;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void Attach(MainViewModel viewModel, ITrayIconService trayIconService)
    {
        _viewModel = viewModel;
        _trayIconService = trayIconService;
    }

    public void HideToTray(bool notifyUser = true)
    {
        Hide();
        ShowInTaskbar = false;

        if (notifyUser)
        {
            _trayIconService?.ShowInfo("Haus Listrik", "App disembunyikan ke tray.");
        }
    }

    public void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (WindowState == WindowState.Minimized && _viewModel?.IsMinimizeToTrayEnabled == true)
        {
            HideToTray();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_forceExit)
        {
            return;
        }

        if (_viewModel?.IsMinimizeToTrayEnabled == true)
        {
            e.Cancel = true;
            HideToTray();
        }
    }

    public void ForceExit()
    {
        _forceExit = true;
        Close();
    }
}
