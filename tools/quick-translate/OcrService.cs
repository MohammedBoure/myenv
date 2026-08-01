using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace QuickTranslate {
    public static class OcrService {
        public static async Task<string> RecognizeTextAsync(Bitmap rawBitmap) {
            if (rawBitmap == null) return string.Empty;

            // Preprocess bitmap: upscale small single words & add white padding margin
            using (Bitmap processedBitmap = PrepareBitmapForOcr(rawBitmap))
            using (var stream = new InMemoryRandomAccessStream()) {
                // Save bitmap to memory stream as PNG
                using (var ms = new MemoryStream()) {
                    processedBitmap.Save(ms, ImageFormat.Png);
                    byte[] bytes = ms.ToArray();
                    await stream.WriteAsync(bytes.AsBuffer());
                    stream.Seek(0);
                }

                // Decode SoftwareBitmap from stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // Try user profile languages first, fallback to English
                OcrEngine? engine = OcrEngine.TryCreateFromUserProfileLanguages();
                if (engine == null) {
                    engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
                }

                if (engine == null) {
                    throw new InvalidOperationException("Could not initialize Windows OCR engine.");
                }

                OcrResult result = await engine.RecognizeAsync(softwareBitmap);
                
                if (result == null || string.IsNullOrWhiteSpace(result.Text)) {
                    return string.Empty;
                }

                return result.Text.Trim();
            }
        }

        private static Bitmap PrepareBitmapForOcr(Bitmap original) {
            // Determine scale factor for small selections (single words)
            double scale = 1.0;
            if (original.Height < 100 || original.Width < 250) {
                scale = 3.0; // Scale up 3x for small text / single words
            } else if (original.Height < 200) {
                scale = 2.0;
            }

            int padding = 30; // Add 30px quiet white margin around text
            int newWidth = (int)(original.Width * scale) + (padding * 2);
            int newHeight = (int)(original.Height * scale) + (padding * 2);

            Bitmap processed = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(processed)) {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.DrawImage(original, new Rectangle(padding, padding, (int)(original.Width * scale), (int)(original.Height * scale)));
            }

            return processed;
        }
    }
}
