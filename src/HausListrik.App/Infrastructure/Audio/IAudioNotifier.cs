using HausListrik.App.Configuration;

namespace HausListrik.App.Infrastructure.Audio;

public interface IAudioNotifier
{
    string SpeakBatteryDrop(int percentage, bool isCritical);

    string SpeakChargingRestored();

    void UpdateOptions(AudioOptions options);
}
