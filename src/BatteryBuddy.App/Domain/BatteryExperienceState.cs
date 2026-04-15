namespace BatteryBuddy.App.Domain;

public sealed record BatteryExperienceState(
    BatterySnapshot Snapshot,
    int AppliedBrightness,
    string LastVoiceLine,
    string Diagnostics);
