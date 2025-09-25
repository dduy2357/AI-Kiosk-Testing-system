using DesktopApp.Constants;
using DesktopApp.Interops;
using DesktopApp.Models;
using DesktopApp.Services;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;

namespace DesktopApp.Helpers
{
    public class ExamEventHandler
    {
        private readonly ApiService _apiService;
        private readonly MonitoringService _monitoringService;

        public ExamEventHandler(ApiService apiService, MonitoringService monitoringService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        }

        public async Task HandleWebMessageAsync(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.WebMessageAsJson))
                {
                    _monitoringService.LogError("Received empty WebMessageAsJson.");
                    return;
                }
                var eventData = System.Text.Json.JsonSerializer.Deserialize<ExamEventMessage>(e.WebMessageAsJson);
                if (eventData == null || string.IsNullOrEmpty(eventData.Type))
                {
                    _monitoringService.LogError("Received invalid or unknown event data.");
                    return;
                }
                switch (eventData.Type)
                {
                    case ExamEventType.Token:
                        HandleTokenEvent(eventData);
                        break;
                    case ExamEventType.StartExam:
                        await HandleStartExamAsync(eventData);
                        break;
                    case ExamEventType.CaptureScreenshot:
                       // await HandleCaptureScreenshotAsync(eventData);
                        break;
                    default:
                        _monitoringService.LogWarning($"Received unknown event type: {eventData.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Error handling web message: {ex.Message}; {ex.InnerException}; {ex.StackTrace}");
                return;
            }
        }

        private void HandleTokenEvent(ExamEventMessage eventData)
        {
            if (string.IsNullOrEmpty(eventData.Token))
            {
                _monitoringService.LogError("Token message received but token is null or empty.");
                return;
            }
            DataStorage.AccessToken = eventData.Token;
            _monitoringService.LogInfo($"AccessToken received: {DataStorage.AccessToken}");
        }

        private async Task HandleStartExamAsync(ExamEventMessage eventData)
        {
            if (string.IsNullOrEmpty(eventData.ExamId) || string.IsNullOrEmpty(eventData.StudentExamId))
            {
                _monitoringService.LogError("startExam event missing ExamId or StudentExamId");
                return;
            }
            DataStorage.UserId = eventData.UserId;
            DataStorage.ExamId = eventData.ExamId;
            DataStorage.StudentExamId = eventData.StudentExamId;
            DataStorage.AccessToken = eventData.Token;

            _monitoringService.LogInfo($"Exam started - ExamId: {eventData.ExamId}, StudentExamId: {eventData.StudentExamId}");

            var runningProcesses = ProcessMonitor.ListAllProcessesForServer();
            var log = new AddExamLogVM
            {
                ActionType = "ProcessList",
                Description = "List of running processes at application startup",
                LogType = LogType.Info,
                StudentExamId = eventData.StudentExamId,
                UserId = eventData.UserId ?? string.Empty,
                ScreenshotPath = ScreenCaptureHelper.CaptureScreenAsJpeg(),
                Metadata = JsonConvert.SerializeObject(new { RunningProcesses = runningProcesses })
            };

            var logResult = await _apiService.SendLogToServer(log);
            _monitoringService.LogInfo(logResult?.ToString() ?? "No log result returned.");
        }

        public async Task HandleCaptureScreenshotAsync(ExamEventMessage eventData)
        {
            var screenshot = ScreenCaptureHelper.CaptureScreenAsJpeg();
            if (screenshot == null)
            {
                _monitoringService.LogError("Failed to capture screenshot.");
                return;
            }
            if (eventData.StudentExamId == null && DataStorage.StudentExamId == null)
            {
                _monitoringService.LogError("Error sent screenshot: Not found studentExamId");
                return; 
            }
            eventData.StudentExamId ??= DataStorage.StudentExamId;

            var capture = new CaptureRequest
            {
                StudentExamId = eventData.StudentExamId!,
                ImageCapture = screenshot,
                IsDetected = false,
                LogType = LogType.Info,
                Description = "Captured screenshot from desktop app",
                Emotions = null,
                DominantEmotion = null
            };

            var fileName = $"{Guid.NewGuid()}_screenshot.jpg";
            var msg = await _apiService.UploadScreenshot(capture, fileName);
            _monitoringService.LogInfo(string.IsNullOrEmpty(msg) || !msg.StartsWith("Failed")
                ? $"Capture sent successfully: {msg}"
                : $"Failed to upload screenshot: {msg}");
        }
    }
}