using DesktopApp.Services;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace DesktopApp.Interops
{
    /// <summary>
    /// Ngăn máy tính tự động khóa màn hình hoặc bật chế độ ngủ/screen saver
    /// </summary>
    public static class ScreenLockManager
    {
        private static MonitoringService? _monitoringService;
        private static bool _isScreenLockPrevented;
        private static string? _originalScreenSaverTimeout;
        private static string? _originalScreenSaverActive;

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _isScreenLockPrevented = false;
        }

        public static void PreventScreenLock()
        {
            if (_isScreenLockPrevented)
            {
                _monitoringService?.LogWarning("Screen lock prevention is already active.");
                return;
            }

            try
            {
                // Disable screen saver
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        _originalScreenSaverTimeout = key.GetValue("ScreenSaveTimeOut")?.ToString();
                        _originalScreenSaverActive = key.GetValue("ScreenSaveActive")?.ToString();
                        key.SetValue("ScreenSaveTimeOut", "0");
                        key.SetValue("ScreenSaveActive", "0");
                        _monitoringService?.LogInfo("Screen saver disabled.");
                    }
                }

                // Prevent monitor power-off
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
                _monitoringService?.LogInfo("Monitor power-off prevention enabled.");

                _isScreenLockPrevented = true;
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to prevent screen lock: {ex.Message}");
            }
        }

        public static void AllowScreenLock()
        {
            if (!_isScreenLockPrevented)
            {
                _monitoringService?.LogWarning("Screen lock prevention is not active.");
                return;
            }

            try
            {
                // Restore screen saver settings
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        if (_originalScreenSaverTimeout != null)
                            key.SetValue("ScreenSaveTimeOut", _originalScreenSaverTimeout);
                        if (_originalScreenSaverActive != null)
                            key.SetValue("ScreenSaveActive", _originalScreenSaverActive);
                        _monitoringService?.LogInfo("Screen saver settings restored.");
                    }
                }

                // Allow monitor power-off
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                _monitoringService?.LogInfo("Monitor power-off prevention disabled.");

                _isScreenLockPrevented = false;
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to allow screen lock: {ex.Message}");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
        }
    }
}
