using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Helpers
{
    public static class TimeSetting
    {
        public static event Action<string, string>? TimeChanged; // Time, Date

        public static void Start()
        {
            // Use DispatcherTimer in BottomTabBar for UI updates
            // This class just provides the data
            UpdateTime();
        }

        public static void UpdateTime()
        {
            string time = DateTime.Now.ToString("h:mm tt");
            string date = DateTime.Now.ToString("M/d/yyyy");
            TimeChanged?.Invoke(time, date);
        }

        public static (string Time, string Date) GetTime()
        {
            return (DateTime.Now.ToString("h:mm tt"), DateTime.Now.ToString("M/d/yyyy"));
        }
    }
}
