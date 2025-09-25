using DesktopApp.Constants;
using DesktopApp.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DesktopApp.Interops
{
    /// <summary>
    /// Triển khai hook bàn phím để chặn phím
    /// </summary>
    public static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104; // Xử lý phím hệ thống (VD: Alt, Win)
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static HashSet<string> _blockedCombinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static MonitoringService? _monitoringService;

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _monitoringService.LogInfo("KeyboardHook initialized.");
        }

        public static void SetBlockedCombinations(IEnumerable<string> combinations)
        {
            if (combinations == null)
                throw new ArgumentNullException(nameof(combinations));

            _blockedCombinations = new HashSet<string>(combinations, StringComparer.OrdinalIgnoreCase);
            _monitoringService?.LogInfo($"Blocked combinations updated: {string.Join(", ", _blockedCombinations)}");
        }

        public static void Start()
        {
            if (_hookID != IntPtr.Zero)
            {
                _monitoringService?.LogWarning("Keyboard hook is already running.");
                return;
            }

            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            if (_hookID == IntPtr.Zero)
            {
                _monitoringService?.LogError("Failed to set keyboard hook.");
                throw new InvalidOperationException("Failed to set keyboard hook.");
            }
            _monitoringService?.LogInfo("Keyboard hook started.");
        }

        public static void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
                _monitoringService?.LogInfo("Keyboard hook stopped.");
            }
        }
         
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam); //là Virtual Key Code(mã số của phím được nhấn), ví dụ VK_LWIN = 0x5B (phím Windows trái)
                var key = KeyInterop.KeyFromVirtualKey(vkCode); //Chuyển mã vkCode sang đối tượng Key của WPF — giúp xử lý tên phím dễ hơn (Key.Escape, Key.A,)

                //Bit 15 (giá trị 0x8000)	Pressed state – cho biết phím hiện đang được nhấn giữ hay không.
                //Bit 0 (giá trị 0x0001)	Toggle state – cho biết phím như Caps Lock, Num Lock, Scroll Lock đang bật hay không.
                // Kiểm tra trạng thái các phím modifier
                bool isCtrl = (GetKeyState(KeyCodes.VK_CONTROL) & 0x8000) != 0 ||
                              (GetKeyState(KeyCodes.VK_LCONTROL) & 0x8000) != 0 ||
                              (GetKeyState(KeyCodes.VK_RCONTROL) & 0x8000) != 0;
                bool isAlt = (GetKeyState(KeyCodes.VK_MENU) & 0x8000) != 0 ||
                             (GetKeyState(KeyCodes.VK_LMENU) & 0x8000) != 0 ||
                             (GetKeyState(KeyCodes.VK_RMENU) & 0x8000) != 0;
                bool isShift = (GetKeyState(KeyCodes.VK_SHIFT) & 0x8000) != 0 ||
                               (GetKeyState(KeyCodes.VK_LSHIFT) & 0x8000) != 0 ||
                               (GetKeyState(KeyCodes.VK_RSHIFT) & 0x8000) != 0;
                bool isWin = (GetKeyState(KeyCodes.VK_LWIN) & 0x8000) != 0 ||
                             (GetKeyState(KeyCodes.VK_RWIN) & 0x8000) != 0;

                // Xây dựng tổ hợp phím
                var combo = BuildComboString(isCtrl, isAlt, isShift, isWin, key);

                // Kiểm tra tổ hợp phím có bị chặn không
                if (IsBlockedCombination(combo))
                {
                    _monitoringService?.LogWarning($"Blocked key combination: {combo}");
                    return (IntPtr)1; // Chặn phím
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string BuildComboString(bool ctrl, bool alt, bool shift, bool win, Key key)
        {
            var parts = new List<string>();
            if (ctrl) parts.Add("Ctrl");
            if (alt) parts.Add("Alt");
            if (shift) parts.Add("Shift");
            if (win) parts.Add("Win");
            parts.Add(key.ToString());
            return string.Join("+", parts);
        }

        private static bool IsBlockedCombination(string combo)
        {
            if (_blockedCombinations.Contains(combo, StringComparer.OrdinalIgnoreCase))
                return true;

            // Kiểm tra các tổ hợp phím đặc biệt hoặc phím đơn
            foreach (var blockedCombo in _blockedCombinations)
            {
                // Nếu tổ hợp phím bị chặn là phím đơn (VD: "Escape")
                if (blockedCombo.Equals(combo.Split('+').Last(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Kiểm tra tổ hợp phím đầy đủ (VD: "Ctrl+Alt+Del")
                if (blockedCombo.Equals(combo, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // Delegates and WinAPI imports
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

}
