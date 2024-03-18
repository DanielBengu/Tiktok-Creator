﻿using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;

namespace Reddit_scraper.ImageService
{
    public class ScreenshotService
    {
        static readonly string reddit_logo_path = "C:\\Users\\danie\\OneDrive\\Documents\\reddit.png";
        public static string GenerateScreenshot(string path, string title)
        {
            // Generate the image
            Bitmap image = GenerateImage(title, reddit_logo_path);

            string imagePath = Path.Combine(path, "generated_image.png");

            // Save the image to a file
            image.Save(imagePath, ImageFormat.Png);

            // Dispose the image object
            image.Dispose();

            //todo: screenshot creator
            return imagePath;
        }

        static Bitmap GenerateImage(string title, string logoPath)
        {
            // Load the logo image
            if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath))
                throw new FileNotFoundException("Logo file not found", logoPath);

            // Image dimensions
            int width = 600;
            int height = 200;

            // Create a bitmap with transparent background
            Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // Make the background transparent
                graphics.Clear(Color.Transparent);

                // Draw a rounded rectangle as background panel
                int cornerRadius = 20;
                using (GraphicsPath path = RoundedRectangle(new Rectangle(10, 10, width - 20, height - 20), cornerRadius))
                {
                    // Fill the rounded rectangle with white color
                    using Brush brush = new SolidBrush(Color.White);
                    graphics.FillPath(brush, path);
                }

                // Load and resize the logo image
                Image logoImage = Image.FromFile(logoPath);
                int logoMaxWidth = width / 3; // Adjust as needed
                int logoMaxHeight = height - 40; // Adjust as needed
                Size logoSize = GetResizedImageSize(logoImage, logoMaxWidth, logoMaxHeight);

                // Load and resize the logo image with transparency preservation
                Bitmap resizedLogo = ResizeImageWithTransparency(logoImage, logoSize.Width, logoSize.Height);

                // Draw the logo on the left side of the rectangle
                graphics.DrawImage(resizedLogo, new Rectangle(20, (height - resizedLogo.Height) / 2, resizedLogo.Width, resizedLogo.Height));

                // Dispose the resized logo image
                resizedLogo.Dispose();

                // Draw title
                Font titleFont = new("Arial", 24, FontStyle.Bold);

                // Calculate the available width for the title
                float availableWidthForTitle = width - 60 - logoSize.Width; // Adjusted padding

                // Split title into lines
                string[] lines = SplitTextIntoLines(title, titleFont, availableWidthForTitle, graphics);

                // Check if the text fits within 3 lines, otherwise reduce font size and try again
                int maxLines = 5;
                while (lines.Length > maxLines && titleFont.Size > 5)
                {
                    // Reduce font size
                    titleFont = new Font(titleFont.FontFamily, titleFont.Size - 1, titleFont.Style);
                    // Try splitting again
                    lines = SplitTextIntoLines(title, titleFont, availableWidthForTitle, graphics);
                }

                // Draw the title
                float titleY = (height - titleFont.GetHeight(graphics) * lines.Length) / 2;
                for (int i = 0; i < lines.Length; i++)
                {
                    graphics.DrawString(lines[i], titleFont, Brushes.Black, new PointF(20 + logoSize.Width + 30, titleY + i * titleFont.GetHeight(graphics)));
                }
            }

            return bitmap;
        }

        static string[] SplitTextIntoLines(string text, Font font, float maxWidth, Graphics graphics)
        {
            string[] words = text.Split(' ');
            List<string> lines = new List<string>();
            string currentLine = "";

            foreach (string word in words)
            {
                if (graphics.MeasureString(currentLine + word, font).Width > maxWidth)
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }
                currentLine += word + " ";
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines.ToArray();
        }

        // Method to resize the image while maintaining aspect ratio
        static Size GetResizedImageSize(Image image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            return new Size(newWidth, newHeight);
        }

        // Method to resize the image while maintaining aspect ratio and preserving transparency
        static Bitmap ResizeImageWithTransparency(Image image, int width, int height)
        {
            Bitmap resizedImage = new(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }



        static GraphicsPath RoundedRectangle(Rectangle rectangle, int cornerRadius)
        {
            GraphicsPath path = new();
            int diameter = 2 * cornerRadius;
            Rectangle arc = new(rectangle.Location, new Size(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = rectangle.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rectangle.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rectangle.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
