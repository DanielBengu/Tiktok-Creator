using Microsoft.WindowsAPICodePack.Shell;

namespace Reddit_scraper.Generic
{
    internal class GenericService
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

        public static void CalculateAndSaveBestTimeForPosting(string bestTimePath)
        {
            DateTime now = DateTime.Now;

            var postingTimes = new[]
            {
            new { Day = DayOfWeek.Monday, Times = new[] { new TimeSpan(12, 0, 0), new TimeSpan(16, 0, 0) } },
            new { Day = DayOfWeek.Tuesday, Times = new[] { new TimeSpan(15, 0, 0) } },
            new { Day = DayOfWeek.Wednesday, Times = new[] { new TimeSpan(19, 0, 0), new TimeSpan(20, 0, 0), new TimeSpan(23, 0, 0) } },
            new { Day = DayOfWeek.Thursday, Times = new[] { new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0) } },
            new { Day = DayOfWeek.Friday, Times = new[] { new TimeSpan(11, 0, 0), new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0) } },
            new { Day = DayOfWeek.Saturday, Times = new[] { new TimeSpan(17, 0, 0) } },
            new { Day = DayOfWeek.Sunday, Times = new[] { new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0), new TimeSpan(22, 0, 0) } }
        };

            // Find the next best time for posting
            var nextBestTime = postingTimes.SelectMany(x => x.Times.Select(t => new DateTime(now.Year, now.Month, now.Day, t.Hours, t.Minutes, t.Seconds)))
                                           .Where(dt => dt > now)
                                           .OrderBy(dt => dt)
                                           .FirstOrDefault();

            // Return the next best time as a string
            SaveNextBestTimeToFile($"{nextBestTime.DayOfWeek}_{nextBestTime.Hour}", bestTimePath);
        }

        static void SaveNextBestTimeToFile(string nextBestTime, string bestTimePath)
        {
            // Save the next best time to a txt file
            string fileName = Path.Combine(bestTimePath, nextBestTime);
            File.WriteAllText(fileName, string.Empty);
            Console.WriteLine($"Next best time for posting has been saved to {fileName}");
        }
    }
}
