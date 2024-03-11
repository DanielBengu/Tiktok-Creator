using System.Diagnostics;

namespace Reddit_scraper.VideoMixer
{
    public class VideoMixing
    {
        public static void ReplaceAudio(string videoFile, string newAudioFile, string subtitleFile, int videoDuration, string outputVideoFile, string ffmpegPath = "ffmpeg")
        {
            // Build the FFmpeg command to replace video audio and add subtitles
            string escapedSubtitleFile = subtitleFile.Replace(@"\", @"\\").Replace(@":", @"\:");
            string subtitlesStyle = "'Fontsize=30,Alignment=10,Fontname=Helvetica,BackColour=&H000000,Spacing=0.2,Outline=1,Shadow=0.75'";

            string command = $"-i \"{videoFile}\" -i \"{newAudioFile}\" -i \"{subtitleFile}\" -map 0:v -map 1:a -vf \"subtitles='{escapedSubtitleFile}':force_style={subtitlesStyle}\" -c:v libx264 -c:a aac -strict -2 -t {videoDuration} \"{outputVideoFile}\"";



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