namespace Reddit_scraper.Image
{
    public class ScreenshotService
    {
        public static string GenerateScreenshot()
        {
            string basePath = "C:\\Users\\danie\\Videos\\Downloader\\";
            //todo: screenshot creator
            return Path.Combine(basePath, "screenshot.png");
        }
    }
}
