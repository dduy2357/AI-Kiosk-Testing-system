using DesktopApp.Constants;
using DesktopApp.Helpers;
using DesktopApp.Models;
using DesktopApp.Services;
using System.Timers;
using System.Windows.Forms;

namespace DesktopApp.Interops
{
    public static class MonitorWatcher
    {
        private static MonitoringService _monitoringService;
        private static ApiService _apiService;
        private static System.Timers.Timer _timer;
        private static int _lastMonitorCount;

        public static void Initialize(MonitoringService monitoringService, ApiService apiService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _monitoringService.LogInfo("MonitorWatcher initialized.");
        }
        public static void Start()
        {
            _lastMonitorCount = Screen.AllScreens.Length;
            _monitoringService.LogInfo($"Initial monitor count: {_lastMonitorCount}");

            _timer = new System.Timers.Timer(15000); // Kiểm tra mỗi 2 giây
            _timer.Elapsed += CheckMonitors;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public static void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        public static bool HasMultipleMonitors()
        {
            try
            {
                int monitorCount = Screen.AllScreens.Length;
                _monitoringService?.LogInfo($"Detected {monitorCount} monitor(s) connected.");
                if (monitorCount > 1)
                {
                    _monitoringService?.LogWarning("Multiple monitors detected.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Error while checking monitor count: {ex.Message}");
            }
            return false;
        }

        private static async void CheckMonitors(object sender, ElapsedEventArgs e)
        {
            int currentCount = Screen.AllScreens.Length;
            if (currentCount != _lastMonitorCount)
            {
                _monitoringService.LogWarning($"Monitor count changed: {_lastMonitorCount} → {currentCount}");
                _lastMonitorCount = currentCount;

                if (currentCount > 1)
                {
                    _monitoringService.LogWarning("Extra monitor connected during exam!");

                    if (DataStorage.UserId == null || DataStorage.StudentExamId == null)
                    {
                        _monitoringService.LogError("UserId or StudentExamId is null. Cannot log monitor change.");
                        return;
                    }
                    var screenshot = ScreenCaptureHelper.CaptureScreenAsJpeg();
                    if (screenshot == null)
                    {
                        _monitoringService.LogError("Failed to capture screenshot.");
                        return;
                    }
                    var log = new AddExamLogVM
                    {
                        ActionType = "MultipleMonitors",
                        Description = "Detected multiple monitors. Captured screenshot from desktop app\"",
                        LogType = LogType.Violation,
                        StudentExamId = DataStorage.StudentExamId!,
                        UserId = DataStorage.UserId!,
                        ScreenshotPath = screenshot,
                    };

                    var logResult = await _apiService.SendLogToServer(log);
                    _monitoringService.LogInfo(logResult?.ToString() ?? "No log result returned.");
                }
            }
        }

    }
}
