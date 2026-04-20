using HausListrik.App.Configuration;
using HausListrik.App.Domain;
using HausListrik.App.Infrastructure.Audio;
using HausListrik.App.Infrastructure.Battery;
using HausListrik.App.Infrastructure.Brightness;

namespace HausListrik.App.Services;

public sealed class BatteryExperienceCoordinator : IDisposable
{
    private readonly IBatteryInfoProvider _batteryInfoProvider;
    private readonly IBrightnessController _brightnessController;
    private readonly IAudioNotifier _audioNotifier;
    private readonly object _syncRoot = new();

    private BatteryMonitorOptions _options;
    private System.Timers.Timer? _timer;
    private BatterySnapshot? _previousSnapshot;
    private string _lastVoiceLine = "Belum ada suara.";

    public BatteryExperienceCoordinator(
        IBatteryInfoProvider batteryInfoProvider,
        IBrightnessController brightnessController,
        IAudioNotifier audioNotifier,
        BatteryMonitorOptions options,
        AudioOptions audioOptions)
    {
        _batteryInfoProvider = batteryInfoProvider;
        _brightnessController = brightnessController;
        _audioNotifier = audioNotifier;
        _options = options;
        _audioNotifier.UpdateOptions(audioOptions);
    }

    public event EventHandler<BatteryExperienceState>? StateChanged;

    public void Start()
    {
        lock (_syncRoot)
        {
            if (_timer is not null)
            {
                return;
            }

            _timer = CreateTimer();
            _timer.Start();
        }

        PollBatteryState();
    }

    public void Stop()
    {
        lock (_syncRoot)
        {
            if (_timer is null)
            {
                return;
            }

            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
            _timer = null;
        }
    }

    public void UpdateSettings(BatteryMonitorOptions options, AudioOptions audioOptions)
    {
        lock (_syncRoot)
        {
            _options = options;
            _audioNotifier.UpdateOptions(audioOptions);

            if (_timer is not null)
            {
                _timer.Interval = TimeSpan.FromSeconds(_options.PollingIntervalSeconds).TotalMilliseconds;
            }
        }
    }

    public void Dispose()
    {
        Stop();

        if (_audioNotifier is IDisposable disposableAudio)
        {
            disposableAudio.Dispose();
        }
    }

    private System.Timers.Timer CreateTimer()
    {
        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(_options.PollingIntervalSeconds).TotalMilliseconds)
        {
            AutoReset = true
        };

        timer.Elapsed += OnTimerElapsed;
        return timer;
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        PollBatteryState();
    }

    private void PollBatteryState()
    {
        BatterySnapshot snapshot;
        BatteryMonitorOptions options;

        lock (_syncRoot)
        {
            snapshot = _batteryInfoProvider.GetCurrentSnapshot();
            options = _options;
        }

        var appliedBrightness = ApplyBrightnessIfNeeded(snapshot, options);
        var voiceLine = ResolveVoiceLine(snapshot, options);

        var diagnostics = BuildDiagnostics(snapshot, options, appliedBrightness);
        var state = new BatteryExperienceState(snapshot, appliedBrightness, voiceLine, diagnostics);

        _previousSnapshot = snapshot;
        StateChanged?.Invoke(this, state);
    }

    private int ApplyBrightnessIfNeeded(BatterySnapshot snapshot, BatteryMonitorOptions options)
    {
        if (!options.AutoDimEnabled || !_brightnessController.IsSupported)
        {
            return _brightnessController.GetCurrentBrightness();
        }

        var targetBrightness = snapshot.IsPowerConnected
            ? options.DefaultBrightnessPercentage
            : CalculateBrightness(snapshot.Percentage, options);

        _brightnessController.TrySetBrightness(targetBrightness);
        return targetBrightness;
    }

    private string ResolveVoiceLine(BatterySnapshot snapshot, BatteryMonitorOptions options)
    {
        if (_previousSnapshot is null)
        {
            return _lastVoiceLine;
        }

        if (options.ChargingBurstEnabled &&
            snapshot.IsPowerConnected &&
            !_previousSnapshot.IsPowerConnected)
        {
            _lastVoiceLine = _audioNotifier.SpeakChargingRestored();
            return _lastVoiceLine;
        }

        if (!options.ChaosModeEnabled)
        {
            return _lastVoiceLine;
        }

        if (snapshot.Percentage < _previousSnapshot.Percentage)
        {
            var isCritical = snapshot.Percentage <= options.CriticalBatteryThreshold;
            _lastVoiceLine = _audioNotifier.SpeakBatteryDrop(snapshot.Percentage, isCritical);
        }

        return _lastVoiceLine;
    }

    private string BuildDiagnostics(BatterySnapshot snapshot, BatteryMonitorOptions options, int appliedBrightness)
    {
        var remainingMinutes = snapshot.RemainingMinutes.HasValue
            ? $"{snapshot.RemainingMinutes.Value} mins remaining"
            : "Remaining time unavailable";

        var brightnessSupport = _brightnessController.IsSupported
            ? $"Brightness control active at {appliedBrightness}%."
            : "Brightness control unsupported on this device.";

        return $"{remainingMinutes} Polling every {options.PollingIntervalSeconds}s. {brightnessSupport}";
    }

    private static int CalculateBrightness(int batteryPercentage, BatteryMonitorOptions options)
    {
        var clampedBattery = Math.Clamp(batteryPercentage, 0, 100);
        var target = (int)Math.Round(
            options.MinimumBrightnessPercentage +
            ((options.DefaultBrightnessPercentage - options.MinimumBrightnessPercentage) * (clampedBattery / 100d)),
            MidpointRounding.AwayFromZero);

        return Math.Clamp(target, options.MinimumBrightnessPercentage, 100);
    }
}
