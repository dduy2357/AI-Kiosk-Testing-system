using DesktopApp.Helpers;
using DesktopApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using DesktopApp.Constants;

namespace DesktopApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(Common.API_BASE) };
        }

        public async Task<(string?, ConfigModel?)> GetConfigurationAsync()
        {
            #region Commented Code
            //string json = @"
            //{
            //    ""ShortcutKeys"": [
            //      ""Apps"",
            //        ""Escape"",
            //        ""F1"", ""F2"", ""F3"", ""F4"", ""F5"", ""F6"", ""F7"", ""F8"", ""F9"", ""F10"", ""F11"", ""F12"",
            //        ""LWin"", ""RWin"",
            //        ""PrintScreen"", ""PrtSc"",
            //        ""Alt+Escape"", ""Alt+F4"",""Alt+Space"", ""Alt+Tab"",
            //        ""Ctrl+C"", ""Ctrl+Esc"", ""Ctrl+V"", ""Ctrl+X"",
            //        ""Ctrl+Shift+I"",
            //        ""Ctrl+Shift+N"",
            //        ""Ctrl+Shift+Delete"",
            //        ""Ctrl+Alt+Del"",
            //        ""Ctrl+Shift+Esc"",
            //        ""Ctrl+Alt+Q"", ""Ctrl+Alt+C"", ""Ctrl+Alt+T"", ""Ctrl+Alt+X"",
            //        ""Alt+F7"", ""Alt+F9"",
            //        ""Win+Tab"",
            //        ""Win+R"", ""Win+D"", ""Win+E"", ""Win+X""
            //    ],
            //      ""BlockedApps"": [
            //        ""browser"",
            //        ""Taskmgr"",
            //        ""AutoHotkey"",
            //        //""chrome"",
            //        //""Code"",
            //        ""DeskIn"",
            //        ""DeskIn_Host"",
            //        ""deskIn"",
            //        ""TouchpadBlocker"",
            //        ""firefox"",
            //        ""msedge"",
            //        ""powershell"",
            //        ""cmd"",
            //        ""terminal"",
            //        ""teamviewer"",
            //        ""ultraviewer"",
            //        ""anydesk"",
            //        ""obs64"",
            //        ""xsplit"",
            //        ""cheatengine"",
            //        ""ida"",
            //        ""sandboxie"",
            //        ""AnyDesk"",
            //        ""dwagent"",
            //        ""vncserver"",
            //        ""winvnc"",
            //        ""LogMeIn"",
            //        ""Supremo"",
            //        ""opera"",
            //        ""remoting_host"",
            //        ""safari"",
            //        ""regedit"",
            //        ""pwsh"",
            //        ""chatgpt"",
            //        ""googledesktop"",
            //        ""sublime_text"",
            //        ""vnc"",
            //        ""vmware"",
            //        ""vmtoolsd""],
            //      ""WhitelistApps"": [""notepad"", ""Code"", ""DesktopApp""],
            //      ""ProtectedUrl"": ""https://g77-sep490-su25-ab4781.gitlab.io"",
            //    ""DisableTouchpad"": true,
            //      ""PreventScreenLock"": true,
            //    }
            //";
            #endregion

            var response = await _httpClient.GetAsync("api/ConfigDesktop/get-configs");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ApiResponse<ConfigModelVM>>(json);
            if (result != null && !result.Success) return (result.Message, null);
            var partial = result.Data ?? new ConfigModelVM();

            var whitelist = new List<string>
            {
                "notepad", "Code", "DesktopApp", "devenv", "browser", "explorer",
                 "searchhost",
                "startmenuexperiencehost",
                "textinputhost",
                "widgets",
                "widgetservice",
                "msedgewebview2",
                "dllhost",
                "runtimebroker",
                "sihost",
                "taskhostw",
                "csrss",
                "dwm",
                "wininit",
                "lsass",
                "services",
                "winlogon",
                "fontdrvhost",
                "smss",
                "pushnotificationslongrunningtask",
                "audiodg",
            };

            return ("", new ConfigModel
            {
                ShortcutKeys = partial.ShortcutKeys ?? new List<string>(),
                BlockedApps = partial.BlockedApps ?? new List<string>(),
                ProtectedUrl = partial.ProtectedUrl,

                WhitelistApps = partial.WhiteListApps ?? whitelist,
                MaxDurationMinutes = 60,
                EnableFullscreen = true,
                DisableTouchpad = true,
                PreventScreenLock = true,
                BlockVirtualMachines = true
            });
        }
        public async Task<string> SendLogToServer(AddExamLogVM log)
        {
            try
            {
                if (!string.IsNullOrEmpty(DataStorage.AccessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization
                        = new AuthenticationHeaderValue("Bearer", DataStorage.AccessToken);
                }
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(log.StudentExamId), "StudentExamId");
                form.Add(new StringContent(log.UserId), "UserId");
                form.Add(new StringContent(log.ActionType), "ActionType");
                form.Add(new StringContent(log.Description), "Description");
                form.Add(new StringContent(log.LogType.ToString()), "LogType");
                form.Add(new StringContent(DeviceInfoHelper.GetDeviceId()), "DeviceId");
                form.Add(new StringContent(DeviceInfoHelper.GetDeviceName()), "DeviceUsername");
                form.Add(new StringContent(log.Metadata ?? ""), "Metadata");

                if (log.ScreenshotPath != null && log.ScreenshotPath.Length > 0)
                {
                    var fileContent = new ByteArrayContent(log.ScreenshotPath);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

                    form.Add(fileContent, "ScreenshotPath", $"Screenshot_{DateTime.UtcNow.ToString("F")}");
                }
                var response = await _httpClient.PostAsync("api/Log/write-exam-activity", form);
                var respContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"Failed to send log: {respContent}";

                return "Log sent successfully.";
            }
            catch (Exception ex)
            {
                return $"Error sending log: {ex.Message}";
            }
        }
        public async Task<string> UploadScreenshot(CaptureRequest capture, string fileName)
        {
            if (!string.IsNullOrEmpty(DataStorage.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", DataStorage.AccessToken);
            }
            using (var content = new MultipartFormDataContent())
            {
                // File content
                var fileContent = new ByteArrayContent(capture.ImageCapture);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

                content.Add(fileContent, "ImageCapture", fileName);
                content.Add(new StringContent(capture.StudentExamId), "StudentExamId");
                content.Add(new StringContent(capture.IsDetected.ToString()), "IsDetected");
                content.Add(new StringContent(capture.LogType.ToString()), "LogType");

                if (!string.IsNullOrEmpty(capture.Description))
                    content.Add(new StringContent(capture.Description), "Description");

                if (!string.IsNullOrEmpty(capture.Emotions))
                    content.Add(new StringContent(capture.Emotions), "Emotions");

                if (!string.IsNullOrEmpty(capture.DominantEmotion))
                    content.Add(new StringContent(capture.DominantEmotion), "DominantEmotion");

                var response = await _httpClient.PostAsync("api/FaceCapture/add", content);
                var respContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Failed to upload screenshot: {respContent}";
                }

                return respContent;
            }
        }

        public async Task<string> SendProcessDetected(AddExamLogVM log)
        {
            try
            {
                if (!string.IsNullOrEmpty(DataStorage.AccessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization
                        = new AuthenticationHeaderValue("Bearer", DataStorage.AccessToken);
                }
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(log.StudentExamId), "StudentExamId");
                form.Add(new StringContent(log.UserId), "UserId");
                form.Add(new StringContent(log.ActionType), "ActionType");
                form.Add(new StringContent(log.Description), "Description");
                form.Add(new StringContent(log.LogType.ToString()), "LogType");
                form.Add(new StringContent(DeviceInfoHelper.GetDeviceId()), "DeviceId");
                form.Add(new StringContent(DeviceInfoHelper.GetDeviceName()), "DeviceUsername");
                form.Add(new StringContent(log.Metadata ?? ""), "Metadata");

                if (log.ScreenshotPath != null && log.ScreenshotPath.Length > 0)
                {
                    var fileContent = new ByteArrayContent(log.ScreenshotPath);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

                    form.Add(fileContent, "ScreenshotPath", $"Screenshot_{Guid.NewGuid()}");
                }
                var response = await _httpClient.PostAsync("api/Log/write-exam-activity", form);
                var respContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"Failed to send log: {respContent}";

                return "Log sent successfully.";
            }
            catch (Exception ex)
            {
                return $"Error sending log: {ex.Message}";
            }
        }

    }
}