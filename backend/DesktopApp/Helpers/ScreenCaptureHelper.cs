using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Versioning;

namespace DesktopApp.Helpers
{
    [SupportedOSPlatform("windows")]
    public static class ScreenCaptureHelper
    {
        public static byte[] CaptureScreenAsJpeg()
        {
            var screenBounds = Screen.AllScreens    // Xác định kích thước toàn màn hình (kể cả nhiều màn hình)
                .Select(s => s.Bounds)
                .Aggregate(Rectangle.Union);

            using (var bmp = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, bmp.Size);
                }

                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
        }
    }
}
