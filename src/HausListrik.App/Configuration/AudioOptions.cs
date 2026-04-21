namespace HausListrik.App.Configuration;

public sealed record AudioOptions
{
    public int DefaultVolume { get; init; } = 80;

    public int ChargingVolume { get; init; } = 100;

    public int SpeechRate { get; init; } = -1;

    public string VoiceName { get; init; } = string.Empty;

    public string VoicePackRootDirectory { get; init; } = "Assets\\VoicePack";

    public string VoicePackDirectory { get; init; } = "Assets\\VoicePack\\Default";

    public bool PreferAudioFiles { get; init; } = true;

    public int BatteryDropCooldownSeconds { get; init; } = 12;

    public int ChargingBurstCooldownSeconds { get; init; } = 8;
}
