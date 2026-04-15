using BatteryBuddy.App.Domain;

namespace BatteryBuddy.App.Infrastructure.Battery;

public interface IBatteryInfoProvider
{
    BatterySnapshot GetCurrentSnapshot();
}
