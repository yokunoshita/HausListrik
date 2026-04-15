namespace BatteryBuddy.App.Configuration;

public sealed record BatteryMonitorOptions
{
    public int PollingIntervalSeconds { get; init; } = 5;

    public int MinimumBrightnessPercentage { get; init; } = 18;

    public int DefaultBrightnessPercentage { get; init; } = 70;

    public int LowBatteryThreshold { get; init; } = 20;

    public int CriticalBatteryThreshold { get; init; } = 10;

    public bool AutoDimEnabled { get; init; } = true;

    public bool ChaosModeEnabled { get; init; } = true;

    public bool ChargingBurstEnabled { get; init; } = true;

    public bool StartMinimizedToTray { get; init; } = true;

    public bool MinimizeToTrayOnClose { get; init; } = true;
}
