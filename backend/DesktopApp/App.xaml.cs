using DesktopApp.Interops;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Hook tạm thời để chặn 1 số tổ hợp mặc định như Alt+Tab
            //KeyboardHook.SetBlockedCombinations(new[] { "Ctrl+Shift+Esc" });
            //Hook bàn phím
            //KeyboardHook.Start();
        }
    }

}
