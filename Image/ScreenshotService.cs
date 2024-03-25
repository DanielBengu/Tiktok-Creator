using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;

namespace Reddit_scraper.ImageService
{
    public class ScreenshotService
    {
        static readonly string reddit_logo_path = "C:\\Users\\danie\\OneDrive\\Documents\\reddit.png";
        static readonly string metadata_path = "C:\\Users\\danie\\OneDrive\\Documents\\likescomments.png";
        public static string GenerateScreenshot(string path, string title, int videoWidth)
        {
            // Generate the image
            Bitmap image = GenerateImage(title, videoWidth);

            string imagePath = Path.Combine(path, "generated_image.png");

            // Save the image to a file
            image.Save(imagePath, ImageFormat.Png);

            // Dispose the image object
            image.Dispose();

            //todo: screenshot creator
            return imagePath;
        }

        static Bitmap GenerateImage(string title, int videoWidth)
        {
            string imgUsed = reddit_logo_path;
            // Load the logo image
            if (string.IsNullOrEmpty(imgUsed) || !File.Exists(imgUsed))
                throw new FileNotFoundException("Logo file not found", imgUsed);

            // Image dimensions
            int imgWidth = videoWidth - 10;
            int imgHeight = videoWidth / 3;

            // Create a bitmap with transparent background
            Bitmap bitmap = new(imgWidth, imgHeight, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // Make the background transparent
                graphics.Clear(Color.Transparent);

                DrawBackgroundPanel(graphics, imgWidth, imgHeight);
                DrawLogo(graphics, imgWidth, imgHeight, out int logoWidth, imgUsed);
                DrawTitle(graphics, title, imgWidth, imgHeight, logoWidth);

                // Add image representing likes and comments
                string likesCommentsImage = metadata_path; // Path to your likes and comments image
                if (File.Exists(likesCommentsImage))
                {
                    Image likesComments = Image.FromFile(likesCommentsImage);
                    // Determine position and size of the likes and comments image
                    int likesCommentsWidth = imgWidth / 2 - imgWidth / 8; // Adjust as needed
                    int likesCommentsHeight = (imgHeight / 4) - (imgHeight / 12); // Adjust as needed
                    int likesCommentsX = ((imgWidth - likesCommentsWidth) / 2) + (imgWidth / 4);
                    int likesCommentsY = imgHeight - likesCommentsHeight - 20; // Adjust as needed
                                                                               // Draw the likes and comments image
                    graphics.DrawImage(likesComments, likesCommentsX, likesCommentsY, likesCommentsWidth, likesCommentsHeight);
                    likesComments.Dispose();
                }
            }

            return bitmap;
        }

        static void DrawBackgroundPanel(Graphics graphics, int width, int height)
        {
            int cornerRadius = 20;
            using GraphicsPath path = RoundedRectangle(new Rectangle(10, 10, width - 20, height - 20), cornerRadius);

            Color color = Color.FromArgb(11, 20, 22);

            using Brush brush = new SolidBrush(color);
            graphics.FillPath(brush, path);
        }
        static void DrawTitle(Graphics graphics, string title, int imgWidth, int imgHeight, int logoWidth)
        {
            // Draw title
            Font titleFont = new("Arial", 30, FontStyle.Bold);

            // Calculate the available width for the title
            float availableWidthForTitle = imgWidth - 60 - logoWidth; // Adjusted padding

            // Split title into lines
            string[] lines = SplitTextIntoLines(title, titleFont, availableWidthForTitle, graphics);

            // Check if the text fits within 3 lines, otherwise reduce font size and try again
            int maxLines = 2;
            while (lines.Length > maxLines && titleFont.Size > 5)
            {
                // Reduce font size
                titleFont = new Font(titleFont.FontFamily, titleFont.Size - 1, titleFont.Style);
                // Try splitting again
                lines = SplitTextIntoLines(title, titleFont, availableWidthForTitle, graphics);
            }

            // Draw the title
            float titleY = (imgHeight - titleFont.GetHeight(graphics) * lines.Length) / 2;
            for (int i = 0; i < lines.Length; i++)
            {
                graphics.DrawString(lines[i], titleFont, Brushes.White, new PointF(20 + logoWidth + 30, titleY + i * titleFont.GetHeight(graphics)));
            }
        }

        static void DrawLogo(Graphics graphics, int imgWidth, int imgHeight, out int logoWidth, string imgUsed)
        {
            // Load and resize the logo image
            Image logoImage = Image.FromFile(imgUsed);
            int logoMaxWidth = imgWidth / 4; // Adjust as needed
            int logoMaxHeight = imgHeight - 80; // Adjust as needed
            Size logoSize = GetResizedImageSize(logoImage, logoMaxWidth, logoMaxHeight);

            // Load and resize the logo image with transparency preservation
            Bitmap resizedLogo = ResizeImageWithTransparency(logoImage, logoSize.Width, logoSize.Height);

            // Draw the logo on the left side of the rectangle
            graphics.DrawImage(resizedLogo, new Rectangle(20, (imgHeight - resizedLogo.Height) / 2, resizedLogo.Width, resizedLogo.Height));

            // Dispose the resized logo image
            resizedLogo.Dispose();

            logoWidth = logoSize.Width;
        }

        static string[] SplitTextIntoLines(string text, Font font, float maxWidth, Graphics graphics)
        {
            string[] words = text.Split(' ');
            List<string> lines = [];
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

            return [.. lines];
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
