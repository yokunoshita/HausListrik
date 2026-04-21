namespace HausListrik.App.Infrastructure.Audio;

public sealed record VoicePackValidationResult(
    bool IsValid,
    bool IsUsingAudioFiles,
    string StatusMessage,
    int DropClipCount,
    int CriticalClipCount,
    int ChargingClipCount);
