namespace BatteryBuddy.App.Configuration;

public interface ISettingsProvider
{
    AppSettings Load();

    void Save(AppSettings settings);
}
