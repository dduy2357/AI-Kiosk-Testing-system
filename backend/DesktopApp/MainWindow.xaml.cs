using DesktopApp.Helpers;
using DesktopApp.Interops;
using DesktopApp.Models;
using DesktopApp.Services;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [SupportedOSPlatform("windows")]
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly MonitoringService _monitoringService;
        private ConfigModel _config;
        private readonly ExamEventHandler _examEventHandler;

        public MainWindow()
        {
            _apiService = new ApiService();
            _monitoringService = MonitoringService.GetInstance(_apiService);
            _config = new ConfigModel();
            _examEventHandler = new ExamEventHandler(_apiService, _monitoringService);
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainView_Closing;
            WebView.NavigationStarting += WebView_NavigationStarting;
            TopNavBarControl.ControlledWebView = WebView;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var (msg, _config) = await _apiService.GetConfigurationAsync();
                if (msg.Length > 0)
                {
                    MessageBox.Show(msg, "Error load configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                _monitoringService.LogInfo("MainWindow: Configuration loaded from API.");

                // Đảm bảo WebView2 đã sẵn sàng
                await WebView.EnsureCoreWebView2Async();

                // Kiểm tra WebGL nếu BlockVirtualMachines = true
                if (_config.BlockVirtualMachines)
                {
                    bool isVmWebGL = await CheckWebGLForVMAsync();
                    if (isVmWebGL)
                    {
                        _monitoringService.LogError("Virtual machine detected via WebGL. Closing application.");
                        MessageBox.Show(
                            "This application cannot run on a virtual machine due to security restrictions.",
                            "Security Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        WebView?.Dispose();
                        await ConfigurationService.Reset(this);
                        Close();
                        Application.Current.Shutdown();
                        return;
                    }
                }
                // Gán sự kiện để inject JS sau khi trang load
                WebView.CoreWebView2.NavigationCompleted += async (s, e2) =>
                {
                    await WebView.ExecuteScriptAsync(@"
                        document.addEventListener('contextmenu', event => event.preventDefault());
                        const style = document.createElement('style');
                        style.innerHTML = '* { user-select: none !important; }';
                        document.head.appendChild(style);
                   ");
                };
                WebView.CoreWebView2.WebMessageReceived += async (sender, e) => await _examEventHandler.HandleWebMessageAsync(sender, e);
                WebView.Source = new Uri(_config!.ProtectedUrl);

                WindowManager.LockToForeground(this);
                _monitoringService.LogInfo("Application initialized and configuration applied.");
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to initialize application: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        public async Task<bool> CheckWebGLForVMAsync()
        {
            try
            {
                const string script = @"
                    (function() {
                        try {
                            const canvas = document.createElement('canvas');
                            const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
                            if (!gl) return JSON.stringify({ isVm: false });

                            const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
                            if (!debugInfo) return JSON.stringify({ isVm: false });

                            const vendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL);
                            const renderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);

                            const isVm = vendor.toLowerCase().includes('vmware') || 
                                         vendor.toLowerCase().includes('virtualbox') || 
                                         renderer.toLowerCase().includes('vmware') || 
                                         renderer.toLowerCase().includes('virtualbox');

                            return JSON.stringify({ isVm, vendor, renderer });
                        } catch (e) {
                            return JSON.stringify({ isVm: false, error: e.message });
                        }
                    })();
                ";
                var resultJson = await WebView.CoreWebView2.ExecuteScriptAsync(script);
                resultJson = resultJson.Trim('"').Replace("\\\"", "\""); // Unescape chuỗi JSON

                var resultObj = JsonConvert.DeserializeObject<dynamic>(resultJson);
                bool isVm = resultObj?.isVm ?? false;

                string vendor = resultObj?.vendor ?? "unknown";
                string renderer = resultObj?.renderer ?? "unknown";
                string error = resultObj?.error ?? "";

                _monitoringService?.LogInfo($"WebGL VM Check: isVm={isVm}, vendor={vendor}, renderer={renderer}, error={error}");

                return isVm;
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"[WebGL VM Check] Exception: {ex.Message}");
                return false;
            }
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            try
            {
                if (_config?.ProtectedUrl != null && !args.Uri.StartsWith(_config.ProtectedUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _monitoringService.LogWarning($"Blocked navigation to unauthorized URL: {args.Uri}");
                    args.Cancel = true;
                    MessageBox.Show("Không được phép truy cập URL ngoài hệ thống thi.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Error restricting URL: {ex.Message}");
            }
        }
        private async void MainView_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                WebView?.Dispose();
                await ConfigurationService.Reset(this);
                Application.Current.Shutdown();
                _monitoringService.LogInfo("Application closed and configuration reset.");
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to reset configuration on close: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void BottomTray_ExitButtonClick(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog before closing
            var result = MessageBox.Show("Bạn có chắc chắn muốn thoát ứng dụng?",
                                       "Xác nhận thoát",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (WebView != null && WebView.CoreWebView2 != null)
                    {
                        // Xóa cookie và storage
                        WebView.CoreWebView2.CookieManager.DeleteAllCookies();
                        await WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.AllProfile);
                        DataStorage.AccessToken = null;
                    }
                    await ConfigurationService.Reset(this);
                    WebView?.Dispose();
                }
                catch (Exception ex)
                {
                    _monitoringService?.LogError("Error clearing web data: " + ex.Message);
                }
                Application.Current.Shutdown();
            }
        }
    }
}
