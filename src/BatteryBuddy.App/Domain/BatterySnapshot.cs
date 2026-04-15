namespace BatteryBuddy.App.Domain;

public sealed record BatterySnapshot(
    int Percentage,
    BatteryChargeState ChargeState,
    bool IsPowerConnected,
    int? RemainingMinutes);
