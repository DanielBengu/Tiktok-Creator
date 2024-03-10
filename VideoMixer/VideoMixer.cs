using System.Diagnostics;

namespace Reddit_scraper.VideoMixer
{
    public class VideoMixing
    {
        public static void ReplaceAudio(string videoFile, string newAudioFile, string subtitleFile, string outputVideoFile, string ffmpegPath = "ffmpeg")
        {
            // Build the FFmpeg command to replace video audio and add subtitles
            string escapedSubtitleFile = subtitleFile.Replace(@"\", @"\\");
            escapedSubtitleFile = escapedSubtitleFile.Replace(@":", @"\:");
            string command = $"-i \"{videoFile}\" -i \"{newAudioFile}\" -i \"{subtitleFile}\" -map 0:v -map 1:a -vf \"subtitles='{escapedSubtitleFile}':force_style='Fontsize=30'\" -c:v libx264 -c:a aac -strict -2 \"{outputVideoFile}\"";


            // Run FFmpeg command
            ProcessStartInfo processStartInfo = new()
            {
                FileName = ffmpegPath,
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}