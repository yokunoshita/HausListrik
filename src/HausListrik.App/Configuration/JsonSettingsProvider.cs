using System.IO;
using System.Text.Json;

namespace HausListrik.App.Configuration;

public sealed class JsonSettingsProvider : ISettingsProvider
{
    private readonly string _settingsPath;
    private readonly string _defaultSettingsPath;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonSettingsProvider(string settingsPath, string defaultSettingsPath)
    {
        _settingsPath = settingsPath;
        _defaultSettingsPath = defaultSettingsPath;
    }

    public static JsonSettingsProvider CreateDefault()
    {
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HausListrik");
        Directory.CreateDirectory(appDataDirectory);

        var settingsPath = Path.Combine(appDataDirectory, "appsettings.json");
        var defaultSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        return new JsonSettingsProvider(settingsPath, defaultSettingsPath);
    }

    public AppSettings Load()
    {
        var sourcePath = ResolveSourcePath();
        if (sourcePath is null)
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(sourcePath);
        return JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private string? ResolveSourcePath()
    {
        if (File.Exists(_settingsPath))
        {
            return _settingsPath;
        }

        if (File.Exists(_defaultSettingsPath))
        {
            return _defaultSettingsPath;
        }

        return null;
    }
}
