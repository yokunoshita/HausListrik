namespace HausListrik.App.Infrastructure.Brightness;

public interface IBrightnessController
{
    bool IsSupported { get; }

    int GetCurrentBrightness();

    bool TrySetBrightness(int percentage);
}
