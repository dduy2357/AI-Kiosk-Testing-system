using DesktopApp.Helpers;
using DesktopApp.Models;
using DesktopApp.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DesktopApp.Interops
{
    public static class ProcessMonitor
    {
        private static readonly Dictionary<string, Action> _monitoredProcesses = new();
        private static System.Timers.Timer? _monitorTimer;
        private static MonitoringService? _monitoringService;
        private static readonly HashSet<string> _knownProcesses = new HashSet<string>();
        private static ApiService _apiService;

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _apiService = new ApiService();
        }

        public static List<object> ListAllProcessesForServer()
        {
            if (_monitoringService == null)
                throw new InvalidOperationException("MonitoringService not initialized.");

            var processList = new List<object>();
            try
            {
                var processes = Process.GetProcesses();
                // Group by process name để loại trùng
                var grouped = processes
                 .GroupBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase)
                 .Select(g => new
                 {
                     Process = g.First(),
                     Count = g.Count()
                 }).OrderBy(x => x.Process.ProcessName);
                foreach (var item in grouped)
                {
                    var process = item.Process;

                    if (process.Id == 0 || process.Id == 4) continue;

                    if (!IsSystemProcess(process))
                    {
                        string filePath = process.MainModule?.FileName ?? string.Empty;
                        processList.Add(new
                        {
                            Name = process.ProcessName,
                            FilePath = filePath,
                            InstanceCount = item.Count
                        });
                        //_monitoringService.LogInfo($"Process: {process.ProcessName} (PID: {process.Id}) - Count: {item.Count} - Path: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to list running processes: {ex.Message}");
            }
            return processList;
        }

        public static List<string> GetRunningBlockedProcesses(List<string> blockedApps)
        {
            if (_monitoringService == null)
                throw new InvalidOperationException("MonitoringService not initialized.");

            var runningBlocked = new List<string>();

            if (blockedApps == null || blockedApps.Count == 0)
                return runningBlocked;

            try
            {
                foreach (var blockedName in blockedApps)
                {
                    var processes = Process.GetProcessesByName(blockedName);
                    foreach (var process in processes)
                    {
                        runningBlocked.Add(blockedName);
                        _monitoringService.LogWarning(
                            $"Detected blocked process running: {blockedName} (PID: {process.Id}) Path: {process.MainModule?.FileName ?? "N/A"}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to check blocked processes: {ex.Message}");
            }

            return runningBlocked.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static bool BlockProcess(string processName, string context = "Blocked")
        {
            if (string.IsNullOrEmpty(processName)) return true;
            bool allSuccessful = true;
            foreach (var process in Process.GetProcessesByName(processName.ToLowerInvariant()))
            {
                try
                {
                    process.Kill();
                    _monitoringService?.LogInfo($"{context} process terminated: {processName} (PID: {process.Id})");
                }
                catch (Exception ex)
                {
                    _monitoringService?.LogError($"Failed to terminate {context.ToLower()} process {processName} (PID: {process.Id}): {ex.Message}");
                    allSuccessful = false;
                }
            }
            return allSuccessful;
        }

        public static void MonitorProcess(string processName, Action onProcessDetected)
        {
            if (!_monitoredProcesses.ContainsKey(processName.ToLowerInvariant()))
            {
                _monitoredProcesses.Add(processName, onProcessDetected);
            }

            if (_monitorTimer == null)
            {
                _monitorTimer = new System.Timers.Timer(10000);
                _monitorTimer.Elapsed += (s, e) =>
                {
                    foreach (var kvp in _monitoredProcesses)
                    {
                        if (Process.GetProcessesByName(kvp.Key).Any())
                        {
                            kvp.Value?.Invoke();
                        }
                    }
                };
                _monitorTimer.AutoReset = true;
                _monitorTimer.Start();
            }
        }
        public static bool IsSystemProcess(Process process)
        {
            try
            {
                string path = process.MainModule?.FileName ?? string.Empty;
                if (string.IsNullOrEmpty(path)) return true;

                bool inSystemDir = path.StartsWith(Environment.SystemDirectory, StringComparison.OrdinalIgnoreCase);
                if (inSystemDir)
                {
                    try
                    {
                        var cert = X509Certificate.CreateFromSignedFile(path);
                        var cert2 = new X509Certificate2(cert);
                        bool isTrusted = cert2.Verify();

                        var trustedPublishers = new[]
                        {
                            "Microsoft Windows",
                            "Intel Corporation",
                            "Rivet Networks",
                            "NVIDIA Corporation",
                             "Rivet Networks LLC",
                            "Realtek Semiconductor",
                            "Advanced Micro Devices"
                        };
                        if (!isTrusted || !trustedPublishers.Any(pub => cert2.Subject.Contains(pub)))
                        {
                            return false; // Chữ ký không hợp lệ hoặc không phải nhà phát hành tin cậy
                                _monitoringService.LogWarning( $"Process {process.ProcessName} (PID: {process.Id}) at {path} has invalid or unknown digital signature. May be spoofed!");
                        }
                        return true;
                    }
                    catch (Exception certEx)
                    {
                        _monitoringService.LogWarning($"Failed to verify digital signature for process {process.ProcessName} (PID: {process.Id}): {certEx.Message}");
                        return true;
                    }
                }
                return false; // Không phải ở system folder thì không phải system process
            }
            catch (Exception ex)
            {
                //_monitoringService.LogWarning($"Unable to determine if process {process.ProcessName} (PID: {process.Id}) is system process: {ex.Message}");
                return true; // Không truy cập được → xử lý như system process
            }
        }

        public static bool KillAllAppsExceptWhitelist(IEnumerable<string> whitelist)
        {
            if (_monitoringService == null)
                throw new InvalidOperationException("MonitoringService not initialized.");

            bool allSuccessful = true;

            // Tách whitelist thành 2 nhóm:
            // - Tên process
            // - Đường dẫn tuyệt đối
            var whitelistNames = new HashSet<string>(whitelist
                .Where(item => !item.Contains('\\')).Select(item => Path.GetFileNameWithoutExtension(item)),
                StringComparer.OrdinalIgnoreCase
            );
            var whitelistPaths = new HashSet<string>(whitelist
                .Where(item => item.Contains('\\')).Select(item => item.TrimEnd('\\')),
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string procName = process.ProcessName;
                    string filePath = string.Empty;
                    try { filePath = process.MainModule?.FileName ?? string.Empty; }
                    catch { filePath = string.Empty; }

                    bool isWhitelisted = whitelistNames.Contains(procName) ||
                        (!string.IsNullOrEmpty(filePath) && whitelistPaths.Contains(filePath));
                    if (isWhitelisted) continue;

                    bool isSystem = IsSystemProcess(process);
                    if (!isSystem)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception killEx)
                        {
                            _monitoringService.LogError($"Failed to terminate process {procName} (PID: {process.Id}): {killEx.Message}");
                            allSuccessful = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _monitoringService.LogError($"Error processing processError processing process {process.ProcessName} (PID: {process.Id}): {ex.Message}");
                    allSuccessful = false;
                }
            }
            return allSuccessful;
        }

        public static void MonitorNewProcesses(IEnumerable<string> whitelist, Action<string> onNewProcessDetected)
        {
            if (_monitoringService == null)
                throw new InvalidOperationException("MonitoringService not initialized.");

            var whitelistNames = new HashSet<string>(whitelist
                .Where(item => !item.Contains('\\')).Select(item => Path.GetFileNameWithoutExtension(item)),
                StringComparer.OrdinalIgnoreCase
            );
            var whitelistPaths = new HashSet<string>(whitelist
                .Where(item => item.Contains('\\')).Select(item => item.TrimEnd('\\')),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var process in Process.GetProcesses())
            {
                try { _knownProcesses.Add(process.ProcessName.ToLowerInvariant()); }
                catch { /* Bỏ qua các lỗi quyền truy cập */ }
            }
            // Tạo timer để kiểm tra tiến trình mới
            var newProcessTimer = new System.Timers.Timer(10000);
            newProcessTimer.Elapsed += async (s, e) =>
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (IsSystemProcess(process)) continue;
                        string processName = process.ProcessName.ToLowerInvariant();

                        string procName = process.ProcessName;
                        string filePath = string.Empty;
                        try { filePath = process.MainModule?.FileName ?? string.Empty; }
                        catch { filePath = string.Empty; }

                        bool isWhitelisted = whitelistNames.Contains(procName) ||
                            (!string.IsNullOrEmpty(filePath) && whitelistPaths.Contains(filePath));
                        if (isWhitelisted) continue;

                        if (!_knownProcesses.Contains(processName))
                        {
                            _knownProcesses.Add(processName);
                            _monitoringService?.LogInfo($"New process detected during exam: {processName} (PID: {process.Id})");
                            onNewProcessDetected?.Invoke(processName);
                            await ReportNewProcessToApi(process, processName);
                            if (!_monitoredProcesses.ContainsKey(processName)) // Chặn process nếu chưa được giám sát
                            {
                                MonitorProcess(processName, () =>
                                {
                                    BlockProcess(processName, context: "DetectBlocked");
                                });
                            }
                        }
                    }
                    catch
                    {
                        _monitoringService?.LogError($"Failed to access process {process.ProcessName} (PID: {process.Id}). It may have exited or insufficient permissions.");
                    }
                }
            };
            newProcessTimer.AutoReset = true;
            newProcessTimer.Start();
        }

        public static void StopMonitoring()
        {
            if (_monitorTimer != null)
            {
                _monitorTimer.Stop();
                _monitorTimer.Dispose();
                _monitorTimer = null;
                _monitoringService?.LogInfo("Process monitoring stopped.");
            }
            _monitoredProcesses.Clear();
        }

        private static async Task ReportNewProcessToApi(Process process, string processName)
        {
            if (string.IsNullOrEmpty(DataStorage.StudentExamId) || string.IsNullOrEmpty(DataStorage.UserId))
            {
                _monitoringService?.LogWarning("StudentExamId or UserId is null.");
                return;
            }

            var screenshot = ScreenCaptureHelper.CaptureScreenAsJpeg();
            if (screenshot == null)
                _monitoringService?.LogError("Failed to capture screenshot.");

            var msg = await _apiService.SendProcessDetected(new AddExamLogVM
            {
                ActionType = "ProcessDetected",
                Description = $"New process detected: {processName} (PID: {process.Id})",
                LogType = Constants.LogType.Warning,
                Metadata = JsonConvert.SerializeObject(new
                {
                    ProcessName = processName,
                    ProcessId = process.Id,
                    FilePath = process.MainModule?.FileName ?? "N/A"
                }),
                StudentExamId = DataStorage.StudentExamId!,
                UserId = DataStorage.UserId!,
                ScreenshotPath = screenshot,
            });
            _monitoringService?.LogInfo(msg);
        }
    }
}
