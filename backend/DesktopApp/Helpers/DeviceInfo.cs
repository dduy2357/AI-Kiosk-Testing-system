using System;
using System.Security.Principal;

namespace DesktopApp.Helpers
{
    public static class DeviceInfoHelper
    {
        public static string GetDeviceName()
        {
            return Environment.MachineName;
        }

        public static string GetUserName()
        {
            return Environment.UserName;
        }

        public static string GetDeviceId()
        {
            try
            {
                return WindowsIdentity.GetCurrent().User.Value ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        public static string GetDeviceDescription()
        {
            try
            {
                var description = Environment.OSVersion.ToString();
                return description ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        public static string GetSystemUUID()
        {
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
                foreach (var obj in searcher.Get())
                {
                    return obj["UUID"]?.ToString() ?? "unknown";
                }
            }
            catch
            {
                // ignore
            }
            return "unknown";
        }
    }

}
