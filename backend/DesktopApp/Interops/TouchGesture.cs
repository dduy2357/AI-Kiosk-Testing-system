using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DesktopApp.Services;

namespace DesktopApp.Interops
{
    public static class TouchGesture
    {
        private static MonitoringService _monitoringService;
        private const string SubKeyPath = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad";
        private const int HWND_BROADCAST = 0xffff;
        private const int WM_SETTINGCHANGE = 0x001A;
        private const int SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            int Msg,
            IntPtr wParam,
            string lParam,
            int fuFlags,
            int uTimeout,
            out IntPtr lpdwResult);

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _monitoringService.LogInfo("TouchGesture initialized.");
        }

        public static void DisablePrecisionTouchpadGestures(bool autoRestartExplorer = true)
        {
            try
            {
                // Mở hoặc tạo registry key
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SubKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        _monitoringService?.LogError($"Failed to create or open registry key: {SubKeyPath}");
                        return;
                    }
                    // Cập nhật hoặc tạo các giá trị
                    SetOrCreateRegistryValue(key, "ThreeFingerSlideEnabled", 0);
                    SetOrCreateRegistryValue(key, "ThreeFingerTapEnabled", 0);
                    SetOrCreateRegistryValue(key, "FourFingerSlideEnabled", 0);
                    SetOrCreateRegistryValue(key, "FourFingerTapEnabled", 0);
                    SetOrCreateRegistryValue(key, "EdgeSwipe", 0);

                    _monitoringService?.LogInfo("Precision touchpad gestures disabled successfully.");
                }
                BroadcastTouchpadSettingsChange();
                if (autoRestartExplorer)
                {
                    RestartExplorer();
                }
                else
                {
                    _monitoringService?.LogWarning("Vui lòng đăng xuất hoặc khởi động lại Explorer để áp dụng thay đổi.");
                }
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to disable precision touchpad gestures: {ex.Message}");
            }
        }

        public static void EnablePrecisionTouchpadGestures(bool autoRestartExplorer = true)
        {
            try
            {
                // Mở hoặc tạo registry key
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SubKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        _monitoringService?.LogError($"Failed to create or open registry key: {SubKeyPath}");
                        return;
                    }

                    // Cập nhật hoặc tạo các giá trị
                    SetOrCreateRegistryValue(key, "ThreeFingerSlideEnabled", 1);
                    SetOrCreateRegistryValue(key, "ThreeFingerTapEnabled", 1);
                    SetOrCreateRegistryValue(key, "FourFingerSlideEnabled", 1);
                    SetOrCreateRegistryValue(key, "FourFingerTapEnabled", 1);
                    SetOrCreateRegistryValue(key, "EdgeSwipe", 1);

                    _monitoringService?.LogInfo("Precision touchpad gestures enabled successfully.");
                }

                BroadcastTouchpadSettingsChange();
                if (autoRestartExplorer)
                {
                    RestartExplorer();
                }
                else
                {
                    _monitoringService?.LogWarning("Vui lòng đăng xuất hoặc khởi động lại Explorer để áp dụng thay đổi.");
                }
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to enable precision touchpad gestures: {ex.Message}");
            }
        }

        private static void SetOrCreateRegistryValue(RegistryKey key, string valueName, int value)
        {
            try
            {
                key.SetValue(valueName, value, RegistryValueKind.DWord);
                //_monitoringService?.LogDebug($"Registry value set: {valueName} = {value}");
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to set or create registry value {valueName}: {ex.Message}");
            }
        }

        private static void BroadcastTouchpadSettingsChange()
        {
            try
            {
                IntPtr result;
                SendMessageTimeout(
                    (IntPtr)HWND_BROADCAST,
                    WM_SETTINGCHANGE,
                    IntPtr.Zero,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\PrecisionTouchPad",
                    SMTO_ABORTIFHUNG,
                    100,
                    out result);
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to broadcast touchpad settings change: {ex.Message}");
            }
        }

        private static void RestartExplorer()
        {
            try
            {
                Process.Start("taskkill", "/F /IM explorer.exe").WaitForExit();
                System.Threading.Thread.Sleep(1000);
                Process.Start("explorer.exe");
                _monitoringService?.LogInfo("Đã khởi động lại Explorer.");
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Lỗi khi khởi động lại Explorer: {ex.Message}");
            }
        }
    }
}