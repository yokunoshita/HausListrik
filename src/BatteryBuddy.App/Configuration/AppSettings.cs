namespace BatteryBuddy.App.Configuration;

public sealed record AppSettings
{
    public BatteryMonitorOptions BatteryMonitor { get; init; } = new();

    public AudioOptions Audio { get; init; } = new();

    public StartupOptions Startup { get; init; } = new();
}
