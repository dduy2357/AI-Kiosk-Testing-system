using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace DesktopApp.Services
{
    public class MonitoringService
    {
        private static MonitoringService _instance;
        private static readonly object _lock = new object();
        private readonly string _logDirectory = "Logs";
        private readonly string _logFilePath;
        private readonly ApiService _apiService;
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private bool _isLoggingEnabled = true;

        private MonitoringService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(_logDirectory, $"app_{timestamp}.log");

            EnsureLogDirectoryExists();
            Task.Run(ProcessLogQueueAsync);
        }

        // Public method to get the singleton instance
        public static MonitoringService GetInstance(ApiService apiService)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MonitoringService(apiService);
                    }
                }
            }
            return _instance;
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public void LogError(string message)
        {
            Log("ERROR", message);
        }

        public void LogDebug(string message)
        {
            Log("DEBUG", message);
        }

        private void Log(string level, string message)
        {
            if (!_isLoggingEnabled)
                return;

            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            _logQueue.Enqueue(logEntry);

            // Gửi log lên API (nếu cần)
            if (_apiService != null)
            {
                Task.Run(() => SendLogToApiAsync(logEntry));
            }
        }

        private async Task ProcessLogQueueAsync()
        {
            while (_isLoggingEnabled)
            {
                if (_logQueue.TryDequeue(out var logEntry))
                {
                    try
                    {
                        await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        // Lưu lỗi vào console nếu ghi file thất bại
                        Console.WriteLine($"Failed to write log to file {_logFilePath}: {ex.Message}");
                    }
                }
                await Task.Delay(100); // Giảm tải CPU
            }
        }

        private async Task SendLogToApiAsync(string logEntry)
        {
            try
            {
                // Giả sử API có endpoint POST /api/logs để gửi log
                var logData = new { LogEntry = logEntry };
                var content = new StringContent(JsonConvert.SerializeObject(logData), System.Text.Encoding.UTF8, "application/json");
                // await _apiService.PostAsync("api/logs", content);
                // Giả lập gửi thành công
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Lưu lỗi vào file log hiện tại
                await File.AppendAllTextAsync(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] Failed to send log to API: {ex.Message}{Environment.NewLine}");
            }
        }

        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void StopLogging()
        {
            _isLoggingEnabled = false;
            LogInfo("Logging stopped for file: " + _logFilePath);
        }

        // Hỗ trợ theo dõi hành vi bất thường (VD: nỗ lực truy cập tiến trình bị cấm)
        public void MonitorSuspiciousActivity(string activity, Action onDetected)
        {
            LogWarning($"Suspicious activity detected: {activity}");
            onDetected?.Invoke();
        }

        // Lấy log gần đây (cho mục đích debug hoặc hiển thị)
        public async Task<string[]> GetRecentLogsAsync(int count = 100)
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    var lines = await File.ReadAllLinesAsync(_logFilePath);
                    return lines.TakeLast(count).ToArray();
                }
                return Array.Empty<string>();
            }
            catch (Exception ex)
            {
                LogError($"Failed to read logs from {_logFilePath}: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }
}