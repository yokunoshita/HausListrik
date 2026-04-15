using System.Windows;
using BatteryBuddy.App.Configuration;
using BatteryBuddy.App.Infrastructure.Audio;
using BatteryBuddy.App.Infrastructure.Battery;
using BatteryBuddy.App.Infrastructure.Brightness;
using BatteryBuddy.App.Presentation.ViewModels;
using BatteryBuddy.App.Services;

namespace BatteryBuddy.App;

public partial class App : Application
{
    private BatteryExperienceCoordinator? _coordinator;
    private ITrayIconService? _trayIconService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsProvider = JsonSettingsProvider.CreateDefault();
        var settings = settingsProvider.Load();

        var batteryProvider = new WindowsBatteryInfoProvider();
        var brightnessController = new WindowsBrightnessController();
        var audioNotifier = new PersonalityAudioNotifier(settings.Audio);
        var startupRegistrationService = new WindowsStartupRegistrationService();
        _trayIconService = new WindowsTrayIconService();

        _coordinator = new BatteryExperienceCoordinator(
            batteryProvider,
            brightnessController,
            audioNotifier,
            settings.BatteryMonitor,
            settings.Audio);

        var mainViewModel = new MainViewModel(
            _coordinator,
            settingsProvider,
            startupRegistrationService,
            settings);
        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };

        _trayIconService.Initialize(mainViewModel, () =>
        {
            mainWindow.ShowFromTray();
            mainViewModel.HandleWindowShown();
        }, () =>
        {
            mainWindow.ForceExit();
            Shutdown();
        });
        _trayIconService.Show();

        mainWindow.Attach(mainViewModel, _trayIconService);
        MainWindow = mainWindow;

        if (settings.BatteryMonitor.StartMinimizedToTray)
        {
            mainWindow.HideToTray(notifyUser: false);
            _trayIconService.ShowInfo("Haus Listrik", "Haus Listrik is running in the system tray.");
            return;
        }

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        _coordinator?.Dispose();
        base.OnExit(e);
    }
}
