using Microsoft.WindowsAPICodePack.Shell;

namespace Reddit_scraper.Generic
{
    internal class BackgroundVideo
    {
        public static string GetRandomBackgroundVideoPath(int videoDuration, string path, out int videoWidth)
        {
            // Get all files from the specified path
            string[] files = Directory.GetFiles(path);

            // Filter files based on their names and duration
            var filteredFiles = files.Where(file => {
                    // Extract the duration from the file name
                    if (int.TryParse(Path.GetFileNameWithoutExtension(file).Split('_')[0], out int duration))
                        return duration > videoDuration;

                    return false;
                }).ToList();

            if (filteredFiles.Count == 0)
                // No suitable video found
                throw new FileNotFoundException($"No video found longer than {videoDuration} seconds");

            // Choose a random video file from the filtered list
            Random random = new();
            int randomIndex = random.Next(filteredFiles.Count);
            string videoPath = filteredFiles[randomIndex];

            // Create a ShellFile object for the video file
            ShellFile videoFile = ShellFile.FromFilePath(videoPath);

            videoWidth = (int)videoFile.Properties.System.Video.FrameWidth.Value.GetValueOrDefault();

            return videoPath;
        }
    }
}
