using System.Globalization;
using System.Media;
using System.Speech.Synthesis;
using HausListrik.App.Configuration;

namespace HausListrik.App.Infrastructure.Audio;

public sealed class PersonalityAudioNotifier : IAudioNotifier, IDisposable
{
    private readonly SpeechSynthesizer _speechSynthesizer;
    private readonly Random _random = new();
    private readonly string _baseDirectory;
    private AudioOptions _options;

    private static readonly string[] StandardBatteryLines =
    {
        "Capek banget. Energi aku makin tipis.",
        "Aduh, turun lagi. Aku pengen rebahan.",
        "Baterainya susut. Tolong jangan dipaksa terus.",
        "Aku mulai limbung. Tolong cari colokan."
    };

    private static readonly string[] CriticalBatteryLines =
    {
        "Serius, aku hampir pingsan.",
        "Satu langkah lagi aku tumbang total.",
        "Ini udah kritis. Aku butuh charger sekarang juga."
    };

    private static readonly string[] ChargingLines =
    {
        "AAH! Mantap! Seger!",
        "Yes! Dicas juga akhirnya!",
        "Masuk juga energinya. Hidup terasa indah lagi!"
    };

    public PersonalityAudioNotifier(AudioOptions options)
    {
        _speechSynthesizer = new SpeechSynthesizer();
        _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _options = options;
        ConfigureVoice();
    }

    public string SpeakBatteryDrop(int percentage, bool isCritical)
    {
        var pool = isCritical ? CriticalBatteryLines : StandardBatteryLines;
        var line = $"{pool[_random.Next(pool.Length)]} Tinggal {percentage} persen.";
        var category = isCritical ? "critical" : "drop";
        Play(category, line, _options.DefaultVolume);
        return line;
    }

    public string SpeakChargingRestored()
    {
        var line = ChargingLines[_random.Next(ChargingLines.Length)];
        Play("charging", line, _options.ChargingVolume);
        return line;
    }

    public void UpdateOptions(AudioOptions options)
    {
        _options = options;
        ConfigureVoice();
    }

    public void Dispose()
    {
        _speechSynthesizer.Dispose();
    }

    private void Play(string category, string fallbackText, int volume)
    {
        if (_options.PreferAudioFiles && TryPlayAudioFile(category))
        {
            return;
        }

        Speak(fallbackText, volume);
    }

    private bool TryPlayAudioFile(string category)
    {
        try
        {
            var voicePackDirectory = ResolveVoicePackDirectory();
            if (!Directory.Exists(voicePackDirectory))
            {
                return false;
            }

            var pattern = $"{category}-*.wav";
            var files = Directory.GetFiles(voicePackDirectory, pattern, SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                return false;
            }

            var selected = files[_random.Next(files.Length)];
            _ = Task.Run(() =>
            {
                using var player = new SoundPlayer(selected);
                player.PlaySync();
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ResolveVoicePackDirectory()
    {
        if (Path.IsPathRooted(_options.VoicePackDirectory))
        {
            return _options.VoicePackDirectory;
        }

        return Path.Combine(_baseDirectory, _options.VoicePackDirectory);
    }

    private void ConfigureVoice()
    {
        _speechSynthesizer.Rate = _options.SpeechRate;
        _speechSynthesizer.Volume = ClampVolume(_options.DefaultVolume);

        if (!string.IsNullOrWhiteSpace(_options.VoiceName))
        {
            _speechSynthesizer.SelectVoice(_options.VoiceName);
            return;
        }

        var installedVoice = _speechSynthesizer.GetInstalledVoices(CultureInfo.CurrentCulture)
            .Select(voice => voice.VoiceInfo.Name)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(installedVoice))
        {
            _speechSynthesizer.SelectVoice(installedVoice);
        }
    }

    private void Speak(string text, int volume)
    {
        _speechSynthesizer.Volume = ClampVolume(volume);
        _speechSynthesizer.SpeakAsyncCancelAll();
        _speechSynthesizer.SpeakAsync(text);
    }

    private static int ClampVolume(int volume) => Math.Clamp(volume, 0, 100);
}
