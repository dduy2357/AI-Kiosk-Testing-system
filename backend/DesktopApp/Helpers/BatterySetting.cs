using System.Timers;
using System.Windows.Forms;

namespace DesktopApp.Helpers
{
    public static class BatterySetting
    {
        private static readonly System.Timers.Timer _timer;
        public static event Action<string, string>? BatteryStatusChanged; // Icon, Percentage

        static BatterySetting()
        {
            //_timer = new System.Timers.Timer(1000); // Update every 5 seconds
            //_timer.Elapsed += (s, e) => UpdateBatteryStatus();
            //_timer.AutoReset = true;
            //_timer.Start();
            //UpdateBatteryStatus();
        }

        public static void UpdateBatteryStatus()
        {
            try
            {
                var powerStatus = SystemInformation.PowerStatus;
                string icon = powerStatus.PowerLineStatus == PowerLineStatus.Online ? "🔌" : "🔋";
                int percentage = (int)(powerStatus.BatteryLifePercent * 100);

                // Low battery warning
                if (powerStatus.BatteryLifePercent <= 0.2f && powerStatus.PowerLineStatus != PowerLineStatus.Online)
                {
                    icon = "🪫"; // Low battery icon
                }

                BatteryStatusChanged?.Invoke(icon, $"{percentage}%");
            }
            catch
            {
                BatteryStatusChanged?.Invoke("🔋", "N/A");
            }
        }

        public static (string Icon, string Percentage) GetBatteryStatus()
        {
            var powerStatus = SystemInformation.PowerStatus;
            string icon = powerStatus.PowerLineStatus == PowerLineStatus.Online ? "🔌" : "🔋";
            int percentage = (int)(powerStatus.BatteryLifePercent * 100);
            if (powerStatus.BatteryLifePercent <= 0.2f && powerStatus.PowerLineStatus != PowerLineStatus.Online)
            {
                icon = "🪫";
            }
            return (icon, $"{percentage}%");
        }
    }
}
