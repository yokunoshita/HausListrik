namespace HausListrik.App.Configuration;

public sealed record AudioOptions
{
    public int DefaultVolume { get; init; } = 80;

    public int ChargingVolume { get; init; } = 100;

    public int SpeechRate { get; init; } = -1;

    public string VoiceName { get; init; } = string.Empty;

    public string VoicePackDirectory { get; init; } = "Assets\\VoicePack";

    public bool PreferAudioFiles { get; init; } = true;
}
