using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for TopNavBar.xaml
    /// </summary>
    public partial class TopNavBar : UserControl
    {
        public WebView2? ControlledWebView { get; set; }

        public TopNavBar()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (ControlledWebView?.CanGoBack == true)
                ControlledWebView.GoBack();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (ControlledWebView?.CanGoForward == true)
                ControlledWebView.GoForward();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ControlledWebView?.Reload();
        }
    }

}
