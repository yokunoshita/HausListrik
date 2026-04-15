using System.Windows.Forms;
using HausListrik.App.Domain;

namespace HausListrik.App.Infrastructure.Battery;

public sealed class WindowsBatteryInfoProvider : IBatteryInfoProvider
{
    public BatterySnapshot GetCurrentSnapshot()
    {
        var powerStatus = SystemInformation.PowerStatus;
        var percentage = (int)Math.Round(powerStatus.BatteryLifePercent * 100, MidpointRounding.AwayFromZero);
        var remainingMinutes = powerStatus.BatteryLifeRemaining >= 0
            ? (int?)TimeSpan.FromSeconds(powerStatus.BatteryLifeRemaining).TotalMinutes
            : null;

        return new BatterySnapshot(
            percentage,
            MapChargeState(powerStatus.PowerLineStatus, powerStatus.BatteryChargeStatus),
            powerStatus.PowerLineStatus == PowerLineStatus.Online,
            remainingMinutes);
    }

    private static BatteryChargeState MapChargeState(
        PowerLineStatus powerLineStatus,
        BatteryChargeStatus batteryChargeStatus)
    {
        if (batteryChargeStatus.HasFlag(BatteryChargeStatus.NoSystemBattery))
        {
            return BatteryChargeState.NoBattery;
        }

        if (batteryChargeStatus.HasFlag(BatteryChargeStatus.Charging))
        {
            return BatteryChargeState.Charging;
        }

        if (powerLineStatus == PowerLineStatus.Online)
        {
            return BatteryChargeState.Full;
        }

        if (powerLineStatus == PowerLineStatus.Offline)
        {
            return BatteryChargeState.Discharging;
        }

        return BatteryChargeState.Unknown;
    }
}
