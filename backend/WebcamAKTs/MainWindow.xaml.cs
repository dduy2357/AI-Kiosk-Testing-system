using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WebcamAKTs
{
    public partial class MainWindow : System.Windows.Window
    {
        private VideoCapture capture;
        private CancellationTokenSource cts;
        private string detectStatus = "Đang phân tích...";

        public MainWindow()
        {
            InitializeComponent();
            StartCamera();
        }

        private void StartCamera()
        {
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("Không thể mở webcam.");
                return;
            }

            cts = new CancellationTokenSource();
            Task.Run(() => CaptureLoop(cts.Token));
        }

        private async Task CaptureLoop(CancellationToken token)
        {
            using var frame = new Mat();
            OpenCvSharp.Rect? faceRect = null;
            int frameCounter = 0;

            while (!token.IsCancellationRequested)
            {
                capture.Read(frame);
                if (!frame.Empty())
                {
                    var clone = frame.Clone();

                    if (faceRect.HasValue)
                    {
                        Cv2.Rectangle(clone, faceRect.Value, Scalar.Red, 2);
                    }

                    var image = clone.ToBitmapSource();
                    image.Freeze();

                    Dispatcher.Invoke(() =>
                    {
                        WebcamImage.Source = image;
                        EmotionText.Text = detectStatus;
                    });

                    if (frameCounter++ % 30 == 0) // Gửi mỗi ~1 giây
                    {
                        _ = Task.Run(async () =>
                        {
                            var (result, emotionDetails, rect) = await SendFrameToApi(frame.Clone());
                            Dispatcher.Invoke(() =>
                            {
                                detectStatus = result;
                                faceRect = rect;
                                EmotionDetailText.Text = emotionDetails;
                            });
                        });
                    }
                }

                await Task.Delay(30); // ~33 FPS
            }
        }


        private async Task<(string, string, OpenCvSharp.Rect?)> SendFrameToApi(Mat frame)
        {
            using var ms = frame.ToMemoryStream(".jpg");
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(ms)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("image/jpeg") }
            }, "image", "frame.jpg");

            using var client = new HttpClient();

            try
            {
                // Detect face
                var detectResponse = await client.PostAsync("http://localhost:5000/detect-face", content);
                var detectJson = await detectResponse.Content.ReadAsStringAsync();

                OpenCvSharp.Rect? rect = null;
                bool faceDetected = false;

                using (var doc = JsonDocument.Parse(detectJson))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("result", out var resultProp) &&
                        resultProp.GetString() == "Detected" &&
                        root.TryGetProperty("region", out var region))
                    {
                        int x = region.GetProperty("x").GetInt32();
                        int y = region.GetProperty("y").GetInt32();
                        int w = region.GetProperty("w").GetInt32();
                        int h = region.GetProperty("h").GetInt32();

                        rect = new OpenCvSharp.Rect(x, y, w, h);
                        faceDetected = true;
                    }
                }

                // Emotion analysis
                ms.Position = 0;
                var emotionContent = new MultipartFormDataContent();
                emotionContent.Add(new StreamContent(ms)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("image/jpeg") }
                }, "image", "frame.jpg");

                string emotionText = "";
                string emotionDetails = "";

                if (faceDetected)
                {
                    var emoResponse = await client.PostAsync("http://localhost:5000/analyze-emotion", emotionContent);
                    var emoJson = await emoResponse.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(emoJson);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("dominant_emotion", out var emo))
                        emotionText = $"Cảm xúc: {emo.GetString()}";

                    if (root.TryGetProperty("emotions", out var detail))
                    {
                        foreach (var prop in detail.EnumerateObject())
                            emotionDetails += $"{prop.Name}: {prop.Value.GetDouble():F1}%\n";
                    }

                    return ($"✅ Phát hiện khuôn mặt\n{emotionText}", emotionDetails, rect);
                }

                return ("❌ Không phát hiện khuôn mặt", "", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gọi API: " + ex.Message);
                return ("🚫 Không kết nối được API", "", null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            cts?.Cancel();
            capture?.Release();
        }
    }

    public static class MatExtensions
    {
        public static MemoryStream ToMemoryStream(this Mat mat, string ext)
        {
            Cv2.ImEncode(ext, mat, out byte[] imageBytes);
            return new MemoryStream(imageBytes);
        }
    }
}
