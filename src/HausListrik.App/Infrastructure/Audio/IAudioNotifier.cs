using HausListrik.App.Configuration;

namespace HausListrik.App.Infrastructure.Audio;

public interface IAudioNotifier
{
    string SpeakBatteryDrop(int percentage, bool isCritical);

    string SpeakChargingRestored();

    string PreviewBatteryDrop();

    string PreviewChargingBurst();

    VoicePackValidationResult ValidateActiveVoicePack();

    void UpdateOptions(AudioOptions options);
}
