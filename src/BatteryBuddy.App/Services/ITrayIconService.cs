using BatteryBuddy.App.Presentation.ViewModels;

namespace BatteryBuddy.App.Services;

public interface ITrayIconService : IDisposable
{
    void Initialize(MainViewModel viewModel, Action showWindow, Action exitApplication);

    void Show();

    void Hide();

    void ShowInfo(string title, string message);
}
