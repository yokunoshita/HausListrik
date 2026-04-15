using HausListrik.App.Domain;

namespace HausListrik.App.Infrastructure.Battery;

public interface IBatteryInfoProvider
{
    BatterySnapshot GetCurrentSnapshot();
}
