namespace HausListrik.App.Configuration;

public sealed record StartupOptions
{
    public bool LaunchOnWindowsStartup { get; init; }
}
