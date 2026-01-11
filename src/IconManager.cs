using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using Runner.Models;

namespace Runner
{
    public class IconManager
    {
        public static Icon CreateIcon(IconConfig? iconConfig)
        {
            // Try to load from external file first
            if (iconConfig?.Path != null)
            {
                try
                {
                    // Resolve path relative to executable directory
                    var iconPath = Path.IsPathRooted(iconConfig.Path)
                        ? iconConfig.Path
                        : Path.Combine(AppContext.BaseDirectory, iconConfig.Path);

                    if (File.Exists(iconPath))
                    {
                        var extension = Path.GetExtension(iconPath).ToLower();

                        if (extension == ".ico")
                        {
                            return new Icon(iconPath);
                        }
                        else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp")
                        {
                            using var bitmap = new Bitmap(iconPath);
                            return ConvertToIcon(bitmap);
                        }
                    }
                }
                catch
                {
                    // Fall through to embedded resource
                }
            }

            // Try to load embedded icon.png resource
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Runner.icon.png";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var bitmap = new Bitmap(stream);
                    return ConvertToIcon(bitmap);
                }
            }
            catch
            {
                // Fall through to generated icon
            }

            // Generate icon from text and color
            var text = iconConfig?.Text ?? "R";
            var bgColor = ParseColor(iconConfig?.Color) ?? Color.FromArgb(73, 109, 137);
            var textColor = ParseColor(iconConfig?.TextColor) ?? Color.White;

            return GenerateTextIcon(text, bgColor, textColor);
        }

        private static Icon ConvertToIcon(Bitmap bitmap)
        {
            // Resize to 32x32 for tray icon
            using var resized = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.Clear(Color.Transparent);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.DrawImage(bitmap, 0, 0, 32, 32);
            }

            // Create a proper ICO file with BMP data (not PNG)
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms))
            {
                // ICO Header
                writer.Write((short)0);  // Reserved
                writer.Write((short)1);  // Type (1 = icon)
                writer.Write((short)1);  // Number of images

                // Create BMP data for the icon
                using var bmpStream = new MemoryStream();
                using (var bmpWriter = new BinaryWriter(bmpStream))
                {
                    // BITMAPINFOHEADER
                    bmpWriter.Write(40); // Header size
                    bmpWriter.Write(32); // Width
                    bmpWriter.Write(64); // Height (double for icon - includes AND mask)
                    bmpWriter.Write((short)1); // Planes
                    bmpWriter.Write((short)32); // Bits per pixel
                    bmpWriter.Write(0); // Compression (BI_RGB)
                    bmpWriter.Write(0); // Image size (can be 0 for BI_RGB)
                    bmpWriter.Write(0); // X pixels per meter
                    bmpWriter.Write(0); // Y pixels per meter
                    bmpWriter.Write(0); // Colors used
                    bmpWriter.Write(0); // Important colors

                    // Write pixel data (bottom-up, BGRA format)
                    for (int y = 31; y >= 0; y--)
                    {
                        for (int x = 0; x < 32; x++)
                        {
                            Color pixel = resized.GetPixel(x, y);
                            bmpWriter.Write(pixel.B);
                            bmpWriter.Write(pixel.G);
                            bmpWriter.Write(pixel.R);
                            bmpWriter.Write(pixel.A);
                        }
                    }

                    // AND mask (all zeros for 32bpp with alpha)
                    for (int i = 0; i < 32 * 4; i++)
                    {
                        bmpWriter.Write((byte)0);
                    }
                }

                var bmpData = bmpStream.ToArray();

                // ICONDIRENTRY
                writer.Write((byte)32);  // Width
                writer.Write((byte)32);  // Height
                writer.Write((byte)0);   // Color count
                writer.Write((byte)0);   // Reserved
                writer.Write((short)1);  // Color planes
                writer.Write((short)32); // Bits per pixel
                writer.Write(bmpData.Length); // Size of image data
                writer.Write(22);        // Offset (6 + 16)

                // Write BMP data
                writer.Write(bmpData);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return new Icon(ms);
        }

        private static Icon GenerateTextIcon(string text, Color bgColor, Color textColor)
        {
            using var bitmap = new Bitmap(32, 32);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Fill background
            using (var brush = new SolidBrush(bgColor))
            {
                graphics.FillRectangle(brush, 0, 0, 32, 32);
            }

            // Draw text
            using (var font = new Font("Segoe UI", 16, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(textColor))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString(text, font, brush,
                    new RectangleF(0, 0, 32, 32), format);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        private static Color? ParseColor(int[]? color)
        {
            if (color == null || color.Length != 3)
                return null;

            return Color.FromArgb(
                Math.Clamp(color[0], 0, 255),
                Math.Clamp(color[1], 0, 255),
                Math.Clamp(color[2], 0, 255)
            );
        }
    }
}
