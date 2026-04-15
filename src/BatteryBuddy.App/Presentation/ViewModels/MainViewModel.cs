using System.Windows;
using System.Windows.Input;
using BatteryBuddy.App.Configuration;
using BatteryBuddy.App.Domain;
using BatteryBuddy.App.Presentation.Commands;
using BatteryBuddy.App.Services;

namespace BatteryBuddy.App.Presentation.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly BatteryExperienceCoordinator _coordinator;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IStartupRegistrationService _startupRegistrationService;

    private AppSettings _settings;
    private string _batteryPercentageLabel = "--%";
    private string _powerSourceLabel = "Unknown";
    private string _brightnessLabel = "Brightness: --";
    private string _lastSpokenLine = "Belum ada suara.";
    private string _diagnostics = "Menunggu monitor berjalan.";
    private bool _isMonitoring;

    public MainViewModel(
        BatteryExperienceCoordinator coordinator,
        ISettingsProvider settingsProvider,
        IStartupRegistrationService startupRegistrationService,
        AppSettings settings)
    {
        _coordinator = coordinator;
        _settingsProvider = settingsProvider;
        _startupRegistrationService = startupRegistrationService;
        _settings = settings with
        {
            Startup = settings.Startup with
            {
                LaunchOnWindowsStartup = startupRegistrationService.IsEnabled()
            }
        };

        StartCommand = new RelayCommand(StartMonitoring, () => !_isMonitoring);
        StopCommand = new RelayCommand(StopMonitoring, () => _isMonitoring);
        SaveSettingsCommand = new RelayCommand(SaveSettings);
        OpenVoicePackFolderCommand = new RelayCommand(OpenVoicePackFolder);

        _coordinator.StateChanged += OnStateChanged;
        StartMonitoring();
    }

    public string Headline => "Your battery now complains for attention.";

    public string Subheadline => "MVP ini dibuat untuk Windows dengan struktur yang enak di-scale: service layer terpisah, UI tipis, dan semua perilaku gimmick dibungkus dalam coordinator.";

    public string BatteryPercentageLabel
    {
        get => _batteryPercentageLabel;
        private set => SetProperty(ref _batteryPercentageLabel, value);
    }

    public string PowerSourceLabel
    {
        get => _powerSourceLabel;
        private set => SetProperty(ref _powerSourceLabel, value);
    }

    public string BrightnessLabel
    {
        get => _brightnessLabel;
        private set => SetProperty(ref _brightnessLabel, value);
    }

    public string LastSpokenLine
    {
        get => _lastSpokenLine;
        private set => SetProperty(ref _lastSpokenLine, value);
    }

    public string Diagnostics
    {
        get => _diagnostics;
        private set => SetProperty(ref _diagnostics, value);
    }

    public bool IsLaunchOnStartupEnabled
    {
        get => _settings.Startup.LaunchOnWindowsStartup;
        set => UpdateStartupOptions(_settings.Startup with { LaunchOnWindowsStartup = value });
    }

    public bool PreferAudioFiles
    {
        get => _settings.Audio.PreferAudioFiles;
        set => UpdateAudioOptions(_settings.Audio with { PreferAudioFiles = value });
    }

    public bool IsMinimizeToTrayEnabled
    {
        get => _settings.BatteryMonitor.MinimizeToTrayOnClose;
        set => UpdateBatteryOptions(_settings.BatteryMonitor with { MinimizeToTrayOnClose = value });
    }

    public string VoicePackDirectoryLabel => $"Voice pack folder: {_settings.Audio.VoicePackDirectory}";

    public bool IsChaosModeEnabled
    {
        get => _settings.BatteryMonitor.ChaosModeEnabled;
        set => UpdateBatteryOptions(_settings.BatteryMonitor with { ChaosModeEnabled = value });
    }

    public bool IsAutoDimEnabled
    {
        get => _settings.BatteryMonitor.AutoDimEnabled;
        set => UpdateBatteryOptions(_settings.BatteryMonitor with { AutoDimEnabled = value });
    }

    public bool IsChargingBurstEnabled
    {
        get => _settings.BatteryMonitor.ChargingBurstEnabled;
        set => UpdateBatteryOptions(_settings.BatteryMonitor with { ChargingBurstEnabled = value });
    }

    public double PollingIntervalSeconds
    {
        get => _settings.BatteryMonitor.PollingIntervalSeconds;
        set
        {
            var normalized = (int)Math.Round(value, MidpointRounding.AwayFromZero);
            UpdateBatteryOptions(_settings.BatteryMonitor with { PollingIntervalSeconds = normalized });
            RaisePropertyChanged(nameof(PollingIntervalLabel));
        }
    }

    public string PollingIntervalLabel => $"{_settings.BatteryMonitor.PollingIntervalSeconds} seconds";

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

    public ICommand SaveSettingsCommand { get; }

    public ICommand OpenVoicePackFolderCommand { get; }

    private void StartMonitoring()
    {
        _coordinator.UpdateSettings(_settings.BatteryMonitor, _settings.Audio);
        _coordinator.Start();
        _isMonitoring = true;
        RaiseCommandStates();
    }

    private void StopMonitoring()
    {
        _coordinator.Stop();
        _isMonitoring = false;
        Diagnostics = "Battery monitor dihentikan.";
        RaiseCommandStates();
    }

    private void SaveSettings()
    {
        _startupRegistrationService.SetEnabled(_settings.Startup.LaunchOnWindowsStartup);
        _settingsProvider.Save(_settings);
        Diagnostics = "Pengaturan berhasil disimpan ke appsettings.json.";
    }

    private void UpdateBatteryOptions(BatteryMonitorOptions options)
    {
        _settings = _settings with { BatteryMonitor = options };
        _coordinator.UpdateSettings(options, _settings.Audio);
        RaisePropertyChanged(nameof(IsChaosModeEnabled));
        RaisePropertyChanged(nameof(IsAutoDimEnabled));
        RaisePropertyChanged(nameof(IsChargingBurstEnabled));
        RaisePropertyChanged(nameof(PollingIntervalSeconds));
        RaisePropertyChanged(nameof(PollingIntervalLabel));
        RaisePropertyChanged(nameof(IsMinimizeToTrayEnabled));
    }

    private void UpdateAudioOptions(AudioOptions options)
    {
        _settings = _settings with { Audio = options };
        _coordinator.UpdateSettings(_settings.BatteryMonitor, options);
        RaisePropertyChanged(nameof(PreferAudioFiles));
        RaisePropertyChanged(nameof(VoicePackDirectoryLabel));
    }

    private void UpdateStartupOptions(StartupOptions options)
    {
        _settings = _settings with { Startup = options };
        RaisePropertyChanged(nameof(IsLaunchOnStartupEnabled));
    }

    public void HandleWindowShown()
    {
        Diagnostics = "Window dibuka dari tray.";
    }

    private void OpenVoicePackFolder()
    {
        var fullPath = Path.IsPathRooted(_settings.Audio.VoicePackDirectory)
            ? _settings.Audio.VoicePackDirectory
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.Audio.VoicePackDirectory);

        Directory.CreateDirectory(fullPath);

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }
        catch
        {
            Diagnostics = $"Folder voice pack siap dipakai di: {fullPath}";
        }
    }

    private void OnStateChanged(object? sender, BatteryExperienceState state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            BatteryPercentageLabel = $"{state.Snapshot.Percentage}%";
            PowerSourceLabel = state.Snapshot.ChargeState switch
            {
                BatteryChargeState.Charging => "Charging",
                BatteryChargeState.Discharging => "On battery",
                BatteryChargeState.Full => "Connected to power",
                BatteryChargeState.NoBattery => "No battery detected",
                _ => "Unknown power state"
            };

            BrightnessLabel = $"Brightness: {state.AppliedBrightness}%";
            LastSpokenLine = state.LastVoiceLine;
            Diagnostics = state.Diagnostics;
        });
    }

    private void RaiseCommandStates()
    {
        ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
        ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
    }
}
