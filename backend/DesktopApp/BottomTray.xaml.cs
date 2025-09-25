using DesktopApp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for BottomTray.xaml
    /// </summary>
    public partial class BottomTray : UserControl, INotifyPropertyChanged
    {
        // Define the routed event
        public static readonly RoutedEvent ExitButtonClickEvent =
            EventManager.RegisterRoutedEvent(
                "ExitButtonClick",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(BottomTray));

        // CLR event wrapper
        public event RoutedEventHandler ExitButtonClick
        {
            add { AddHandler(ExitButtonClickEvent, value); }
            remove { RemoveHandler(ExitButtonClickEvent, value); }
        }
        private void BottomTray_Loaded(object sender, RoutedEventArgs e)
        {
            // xử lý nếu cần
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise the routed event
            RaiseEvent(new RoutedEventArgs(ExitButtonClickEvent));
        }
        private readonly DispatcherTimer _timeTimer;
        private float _volumeLevel;
        public float VolumeLevel
        {
            get => _volumeLevel;
            set 
            {
                _volumeLevel = value;
                OnPropertyChanged(nameof(VolumeLevel));
            }
        }

        public BottomTray()
        { 
            InitializeComponent();
            DataContext = this;

            // Initialize Wi-Fi
            // WifiSetting.InitializeAsync();

            //// Subscribe to Setting events
            BatterySetting.BatteryStatusChanged += (icon, percentage) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BatteryIcon.Text = icon;
                    BatteryText.Text = percentage;
                });
            };

            TimeSetting.TimeChanged += (time, date) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ClockTimeText.Text = time;
                    ClockDateText.Text = date;
                });
            };

            WifiSetting.WiFiStatusChanged += (status) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (status == "No Wi-Fi" || status == "Wi-Fi Error")
                    {
                        WiFiIcon.Text = "❌📶";
                        WiFiComboBox.Text = status;
                    }
                    else
                    {
                        WiFiIcon.Text = "📶";
                        WiFiComboBox.Text = status; 
                    }
                });
            };


            WifiSetting.AvailableNetworksChanged += (networks) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WiFiComboBox.ItemsSource = networks;
                });
            };

            //  Start time updates(every second)
            _timeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timeTimer.Tick += (s, e) => TimeSetting.UpdateTime();
            _timeTimer.Tick += (s, e) => BatterySetting.UpdateBatteryStatus();
            //_timeTimer.Tick += (s, e) => WifiSetting.GetListWiFiAsync();
            _timeTimer.Tick += async (s, e) => await WifiSetting.UpdateWiFiStatusAsync();
            _timeTimer.Start();

            // Initial updates
            var (batteryIcon, batteryPercentage) = BatterySetting.GetBatteryStatus();
            Application.Current.Dispatcher.Invoke(() =>
            {
                BatteryIcon.Text = batteryIcon; 
                BatteryText.Text = batteryPercentage;
            });

            var (time, date) = TimeSetting.GetTime();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ClockTimeText.Text = time;
                ClockDateText.Text = date;
            });

        }
        private async void WiFiComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // update this to input password wifi
        {
            if (WiFiComboBox.SelectedItem is string ssid)
            {
                // Prompt for password
                string password = null;
                var result = MessageBox.Show($"Connect to {ssid}? If a password is required, enter it in the next prompt.",
                    "Connect to Wi-Fi", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // In a real app, use a custom dialog to securely input the password
                    var inputDialog = new Window
                    {
                        Title = $"Enter Password for {ssid}",
                        Width = 300,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    var stackPanel = new StackPanel { Margin = new Thickness(10) };
                    var passwordBox = new PasswordBox { Width = 200 };
                    var connectButton = new Button { Content = "Connect", Width = 80, Margin = new Thickness(0, 10, 0, 0) };
                    connectButton.Click += (s, e) => { password = passwordBox.Password; inputDialog.Close(); };
                    stackPanel.Children.Add(new TextBlock { Text = "Password:" });
                    stackPanel.Children.Add(passwordBox);
                    stackPanel.Children.Add(connectButton);
                    inputDialog.Content = stackPanel;

                    inputDialog.ShowDialog();

                    await WifiSetting.ConnectToNetworkAsync(ssid, password);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
