using System.Management;

namespace HausListrik.App.Infrastructure.Brightness;

public sealed class WindowsBrightnessController : IBrightnessController
{
    public bool IsSupported => GetBrightnessObjects().Any();

    public int GetCurrentBrightness()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
            var brightness = searcher.Get()
                .Cast<ManagementObject>()
                .Select(item => Convert.ToInt32(item["CurrentBrightness"]))
                .FirstOrDefault();

            return brightness;
        }
        catch
        {
            return 0;
        }
    }

    public bool TrySetBrightness(int percentage)
    {
        try
        {
            var normalized = Math.Clamp(percentage, 0, 100);

            foreach (var monitor in GetBrightnessObjects())
            {
                monitor.InvokeMethod("WmiSetBrightness", new object[] { 1, normalized });
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<ManagementObject> GetBrightnessObjects()
    {
        using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
        return searcher.Get().Cast<ManagementObject>().ToArray();
    }
}
