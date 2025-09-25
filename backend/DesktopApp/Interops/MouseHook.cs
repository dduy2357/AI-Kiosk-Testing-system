using DesktopApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Interops
{
    public static class MouseHook
    {
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static MonitoringService _monitoringService;
        private static bool _isStarted;

        public static bool IsStarted => _isStarted;

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _monitoringService.LogInfo("MouseHook initialized.");
        }

        public static void Start()
        {
            if (_isStarted)
            {
                _monitoringService?.LogWarning("MouseHook already started.");
                return;
            }

            try
            {
                _hookID = SetHook(_proc);
                if (_hookID == IntPtr.Zero)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to set mouse hook.");
                }
                _isStarted = true;
                _monitoringService?.LogInfo("MouseHook started successfully.");
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to start MouseHook: {ex.Message}");
            }
        }

        public static void Stop()
        {
            if (!_isStarted)
            {
                _monitoringService?.LogWarning("MouseHook not started.");
                return;
            }

            try
            {
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }
                _isStarted = false;
                _monitoringService?.LogInfo("MouseHook stopped successfully.");
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Failed to stop MouseHook: {ex.Message}");
            }
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    int button = (int)(hookStruct.mouseData >> 16) & 0xffff;

                    // Log tất cả sự kiện chuột để debug
                    //_monitoringService?.LogDebug($"Mouse event: wParam={wParam:X}, button={button}, x={hookStruct.pt.x}, y={hookStruct.pt.y}");

                    // Chặn MB4 (Back) và MB5 (Forward)
                    if (wParam == (IntPtr)WM_XBUTTONDOWN || wParam == (IntPtr)WM_XBUTTONUP)
                    {
                        if (button == 1 || button == 2) // XButton1 (Back) hoặc XButton2 (Forward)
                        {
                            _monitoringService?.LogInfo($"Blocked mouse button: {(button == 1 ? "Back" : "Forward")} (wParam={wParam:X})");
                            return (IntPtr)1; // Chặn sự kiện
                        }
                    }
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Error in MouseHook callback: {ex.Message}");
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
