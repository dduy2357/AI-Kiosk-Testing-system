using DesktopApp.Services;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopApp.Interops
{
    public static class WindowManager
    {
        private static MonitoringService _monitoringService;

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        }

        public static void LockToForeground(Window window)
        {
            if (window == null)
            {
                _monitoringService?.LogError("Window parameter is null.");
                throw new ArgumentNullException(nameof(window));
            }

            try
            {
                window.Topmost = true;
                window.WindowState = WindowState.Maximized;
                window.ResizeMode = ResizeMode.NoResize;

                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                _monitoringService?.LogInfo("Window set to topmost and maximized.");
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to lock window to foreground: {ex.Message}");
                throw;
            }
        }

        public static void Unlock(Window window)
        {
            if (window == null)
            {
                _monitoringService?.LogError("Window parameter is null.");
                throw new ArgumentNullException(nameof(window));
            }

            try
            {
                window.Topmost = false;
                window.WindowState = WindowState.Normal; // Hoặc trạng thái mong muốn
                window.ResizeMode = ResizeMode.CanResize; // Khôi phục khả năng thay đổi kích thước
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                _monitoringService?.LogInfo("Window unlocked.");
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to unlock window: {ex.Message}");
                throw;
            }
        }

        // Win32 imports
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2); 
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
    }
}
