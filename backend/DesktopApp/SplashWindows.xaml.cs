using System.Windows;
using DesktopApp.Services;
using DesktopApp.Interops;
using DesktopApp.Helpers;

namespace DesktopApp
{
    public partial class SplashWindows : Window
    {
        private readonly ApiService _apiService;
        private readonly MonitoringService _monitoringService;

        public SplashWindows()
        {
            _apiService = new ApiService();
            _monitoringService = MonitoringService.GetInstance(_apiService);
            InitializeComponent();
            Loaded += async (s, e) => await RunConfigurations();
        }

        private async Task RunConfigurations()
        {
            try
            {
                StatusText.Text = "Checking environment...";
                _monitoringService.LogInfo("SplashWindow: Checking for virtual machine.");
                ConfigurationService.Initialize(_monitoringService, _apiService);
                // Check virtual machine
                if (VirtualMachineDetector.IsRunningInVirtualMachine())
                {
                    _monitoringService.LogWarning("Virtual machine detected. Application shut down.");

                    MessageBox.Show(
                        "Virtual machine detected.\nPlease ensure you are not running in a virtual environment before launching the application.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    await Task.Delay(200); 
                    Application.Current.Shutdown();
                    return;
                }
                LoadingBar.Value = 5;
                await Task.Delay(300);
                StatusText.Text = "Checking external displays...";
                _monitoringService.LogInfo("Checking for external displays.");
                MonitorWatcher.Start();
                // Check duplicate monitors
                if (MonitorWatcher.HasMultipleMonitors())
                {
                    _monitoringService.LogWarning("External displays detected. Application shut down.");
                    MessageBox.Show(
                        "External displays detected.\nPlease disconnect external displays before launching the application.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    await Task.Delay(200);
                    Application.Current.Shutdown();
                    return;
                }
                LoadingBar.Value = 10;
                await Task.Delay(300);
                StatusText.Text = "Initializing configuration...";
                _monitoringService.LogInfo("Initializing configuration.");

                // Fetch configuration
                StatusText.Text = "Loading configuration from server...";
                var (msg, config) = await _apiService.GetConfigurationAsync();
                if (msg.Length > 0)
                {
                    MessageBox.Show(msg, "Error load configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
                _monitoringService.LogInfo("Configuration loaded from API." + config.ProtectedUrl + "\n" + string.Join("", config.ShortcutKeys.ToString()) + "\n" + string.Join("", config.BlockedApps.ToString()));
                _monitoringService.LogInfo("WhiteList loaded from API: " + (config.WhitelistApps != null ? string.Join(", ", config.WhitelistApps) : "Empty"));
                _monitoringService.LogInfo("BlackList loaded from API: " + (config.BlockedApps != null ? string.Join(", ", config.BlockedApps) : "Empty"));

                var runningProcesses = ProcessMonitor.ListAllProcessesForServer();
                _monitoringService.LogInfo("Running processes: " + string.Join(", ", runningProcesses));

                // Check for blocked processes
                StatusText.Text = "Checking prohibited process...";
                var runningBlocked = ProcessMonitor.GetRunningBlockedProcesses(config.BlockedApps);
                if (runningBlocked.Any())
                {
                    var message = $"The following processes are running and need to be terminated to continue:\n{string.Join("\n", runningBlocked)}\n\nDo you want to terminate these processes?";
                    var result = MessageBox.Show(message, "Blocked process warning.", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        var failedProcesses = new List<string>();
                        foreach (var processName in runningBlocked)
                        {
                            if (!ProcessMonitor.BlockProcess(processName, "UserConfirmed"))
                                failedProcesses.Add(processName);
                            else
                            {
                                _monitoringService.LogInfo($"Process terminated: {processName}");
                                StatusText.Text = $"Process terminated: {processName}";
                            }
                        }
                        if (failedProcesses.Any())
                        {
                            var errorMessage = $"The following processes could not be terminated:\n{string.Join("\n", failedProcesses)}\n\nPlease kill these processes yourself in Task Manager and try again.\nPress OK to exit the application.";
                            MessageBox.Show(errorMessage, "Process termination error", MessageBoxButton.OK, MessageBoxImage.Error);
                            _monitoringService.LogError($"Failed to terminate processes: {string.Join(", ", failedProcesses)}");
                            await ConfigurationService.Reset(this);
                            Application.Current.Shutdown();
                            return;
                        }
                        _monitoringService.LogInfo("User confirmed termination of blocked processes.");
                    }
                    else
                    {
                        _monitoringService.LogInfo("User cancelled due to blocked processes.");
                        await ConfigurationService.Reset(this);
                        Application.Current.Shutdown();
                        return;
                    }
                }
                else
                {
                    _monitoringService.LogInfo("No blocked processes are currently running.");
                }
                await Task.Delay(400);
                LoadingBar.Value = 30;

                //// Terminate non-whitelisted processes
                StatusText.Text = "Terminating non-whitelisted processes...";
                if (config.WhitelistApps != null && config.WhitelistApps.Any())
                {
                    bool success = ProcessMonitor.KillAllAppsExceptWhitelist(config.WhitelistApps);
                    if (!success)
                    {
                        _monitoringService.LogWarning("Some non-whitelisted processes could not be terminated.");
                        StatusText.Text = "Warning: Some non-whitelisted processes could not be terminated.";
                    }
                    else
                    {
                        _monitoringService.LogInfo("All non-whitelisted processes terminated successfully.");
                    }
                }
                await Task.Delay(400);
                LoadingBar.Value = 40;

                // Block and monitor processes
                StatusText.Text = "Blocking and monitoring progress...";
                if (config.BlockedApps != null && config.BlockedApps.Any())
                {
                    await ConfigurationService.BlockProcessesAsync(config.BlockedApps, _monitoringService);
                }
                await Task.Delay(400);
                LoadingBar.Value = 50;

                // Theo dõi tiến trình mới và tự động chặn
                StatusText.Text = "Detecting and monitoring new progress...";
                ProcessMonitor.MonitorNewProcesses(config.WhitelistApps, processName =>
                {
                    //  _monitoringService.LogInfo($"New process detected and added to block list: {processName}");
                });
                await Task.Delay(400);
                LoadingBar.Value = 60;

                // Disable keyboard shortcuts and touchpad gestures
                StatusText.Text = "Disabling keyboard shortcuts and touchpad gestures...";
                if (config.ShortcutKeys != null && config.ShortcutKeys.Any())
                {
                    KeyboardHook.SetBlockedCombinations(config.ShortcutKeys);
                    KeyboardHook.Start();
                    _monitoringService.LogInfo("Keyboard shortcuts blocked: " + string.Join(", ", config.ShortcutKeys));
                }
                // block mouse click back/forward
                MouseHook.Start();
                if (config.DisableTouchpad)
                {
                    TouchGesture.DisablePrecisionTouchpadGestures();
                    _monitoringService.LogInfo("Touchpad gestures disabled.");
                }
                await Task.Delay(400);
                LoadingBar.Value = 80;

                // Prevent screen lock
                StatusText.Text = "Applying security configuration...";
                if (config.PreventScreenLock)
                {
                    ScreenLockManager.PreventScreenLock();
                    _monitoringService.LogInfo("Screen lock prevention enabled.");
                }
                _monitoringService.LogInfo("All configurations applied successfully.");
                await Task.Delay(200);
                LoadingBar.Value = 100;

                StatusText.Text = "Configuration complete. Please confirm to continue..";
                ConfirmButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Error during configuration: {ex.Message}");
                StatusText.Text = $"Error: {ex.Message}";
                ConfirmButton.IsEnabled = false;
            }
            finally
            {
                CancelButton.IsEnabled = true;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                _monitoringService.LogInfo("Transitioned to MainWindow.");
                this.Close();
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to open MainWindow: {ex.Message}");
                //  _configurationDetails.Add($"Lỗi khi mở MainWindow: {ex.Message}");
                StatusText.Text = $"Lỗi: {ex.Message}";
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ConfigurationService.Reset(this);
                _monitoringService.LogInfo("Configuration reset due to cancellation.");
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _monitoringService.LogError($"Failed to reset configuration on cancel: {ex.Message}");
                await ConfigurationService.Reset(this);
                Application.Current.Shutdown();
            }
        }

    }
}
