namespace BatteryBuddy.App.Infrastructure.Audio;

public interface IAudioNotifier
{
    string SpeakBatteryDrop(int percentage, bool isCritical);

    string SpeakChargingRestored();

    void UpdateOptions(AudioOptions options);
}
