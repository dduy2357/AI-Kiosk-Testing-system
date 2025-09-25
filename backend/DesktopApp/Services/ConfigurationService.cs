using DesktopApp.Helpers;
using DesktopApp.Interops;
using DesktopApp.Models;
using System.Windows;

namespace DesktopApp.Services
{
    public static class ConfigurationService
    {
        private static MonitoringService? _monitoringService;
        private static ApiService _apiService;
        public static void Initialize(MonitoringService monitoringService, ApiService apiService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            KeyboardHook.Initialize(_monitoringService);
            ProcessMonitor.Initialize(_monitoringService);
            TouchGesture.Initialize(_monitoringService);
            WindowManager.Initialize(_monitoringService);
            ScreenLockManager.Initialize(_monitoringService);
            MouseHook.Initialize(_monitoringService);
            MonitorWatcher.Initialize(_monitoringService, _apiService);
            VirtualMachineDetector.Initialize(_monitoringService);
            _monitoringService.LogInfo("ConfigurationService initialized.");
        }

        public static async Task Apply(ConfigModel config)
        {
            if (_monitoringService == null)
            {
                throw new InvalidOperationException("MonitoringService not initialized. Call Initialize first.");
            }

            if (config == null)
            {
                _monitoringService.LogError("No configuration provided.");
                return;
            }

            try
            {
                //     ProcessMonitor.ListAllRunningProcesses();
                // 1. Chặn phím tắt
                if (config.ShortcutKeys != null && config.ShortcutKeys.Any())
                {
                    try
                    {
                        KeyboardHook.SetBlockedCombinations(config.ShortcutKeys);
                        KeyboardHook.Start();
                        _monitoringService.LogInfo("Keyboard hook enabled with blocked keys: " + string.Join(", ", config.ShortcutKeys));
                    }
                    catch (Exception ex)
                    {
                        _monitoringService.LogError($"Failed to enable keyboard hook: {ex.Message}");
                    }
                }
                else
                {
                    _monitoringService.LogWarning("No blocked keys specified in configuration.");
                }
                // 2. Chặn ứng dụng không mong muốn (blacklist)
                if (config.BlockedApps != null && config.BlockedApps.Any())
                {
                    await BlockProcessesAsync(config.BlockedApps, _monitoringService);
                }
                else
                {
                    _monitoringService.LogWarning("No blocked processes specified in configuration.");
                }

                // 3. Chỉ cho phép ứng dụng trong danh sách whitelist
                if (config.WhitelistApps != null && config.WhitelistApps.Any())
                {
                    //KillAllAppsExceptWhitelist(config.WhitelistApps);
                    _monitoringService.LogInfo("Whitelist applied: " + string.Join(", ", config.WhitelistApps));
                }
                else
                {
                    _monitoringService.LogWarning("No whitelist apps specified in configuration.");
                }

                // 4. Chặn touchpad
                if (config.DisableTouchpad)
                {
                    try
                    {
                        TouchGesture.DisablePrecisionTouchpadGestures(); // Disable touch gestures
                    }
                    catch (Exception ex)
                    {
                        _monitoringService.LogError($"Failed to disable touchpad, attempting gesture disable: {ex.Message}");
                    }
                }
                else
                {
                    _monitoringService.LogInfo("Touchpad disabling skipped per configuration.");
                }

                // 5. Lock window to foreground
                //try
                //{
                //    WindowManager.LockToForeground(window);
                //    _monitoringService.LogInfo("Window locked to foreground.");
                //}
                //catch (Exception ex)
                //{
                //    _monitoringService.LogError($"Failed to lock window: {ex.Message}");
                //}

                // 6. Ngăn khóa màn hình
                if (config.PreventScreenLock)
                {
                    try
                    {
                        ScreenLockManager.PreventScreenLock();
                    }
                    catch (Exception ex)
                    {
                        _monitoringService.LogError($"Failed to prevent screen lock: {ex.Message}");
                    }
                }

                // ... TODO: Cấu hình thời gian thi, thông báo, giao diện, v.v.
                _monitoringService.LogInfo("Configuration applied successfully.");
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to apply configuration: {ex.Message}");
            }
        }

        public static async Task Reset(Window window)
        {
            if (_monitoringService == null)
            {
                throw new InvalidOperationException("MonitoringService not initialized.");
            }

            try
            {
                KeyboardHook.Stop();
                ProcessMonitor.StopMonitoring();
                TouchGesture.EnablePrecisionTouchpadGestures(); // Enable touch gestures back
                WindowManager.Unlock(window);
                ScreenLockManager.AllowScreenLock();
                MouseHook.Stop();
                MonitorWatcher.Stop();
                _monitoringService.LogInfo("Configuration reset successfully.");
                await Task.Delay(500); // Allow WMI cleanup
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to reset configuration: {ex.Message}");
            }
        }

        public static async Task BlockProcessesAsync(List<string> blockedProcesses, MonitoringService monitoringService)
        {
            if (blockedProcesses == null || !blockedProcesses.Any())
            {
                monitoringService.LogWarning("No processes to block.");
                return;
            }
            foreach (var processName in blockedProcesses)
            {
                try
                {
                    // Kill ngay tiến trình nếu đang chạy (ghi log với context "Initial")
                    ProcessMonitor.BlockProcess(processName, context: "Initial");

                    // Theo dõi liên tục và chặn lại nếu tiến trình khởi động lại
                    ProcessMonitor.MonitorProcess(processName, () =>
                    {
                        ProcessMonitor.BlockProcess(processName, context: "Blocked");
                    });
                    //  monitoringService.LogInfo($"Monitoring enabled for process: {processName}");
                }
                catch (Exception ex)
                {
                    monitoringService.LogError($"Failed to set up monitoring for process {processName}: {ex.Message}");
                }
            }
            await Task.CompletedTask;
        }
    }
}
