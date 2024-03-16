using System.Diagnostics;

namespace Reddit_scraper.VideoMixer
{
    public class VideoMixing
    {
        public static void GenerateVideo(string basePostPath, string videoFile, string newAudioFile, string imagePath, string subtitleFile, int videoDuration, string outputVideoFile, string ffmpegPath = "ffmpeg")
        {

            string command = BuildCommand(basePostPath, videoFile, newAudioFile, imagePath, subtitleFile, videoDuration, outputVideoFile);

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

        static string BuildCommand(string basePostPath, string videoFile, string newAudioFile, string imagePath, string subtitleFile, int videoDuration, string outputVideoFile)
        {
            // Build the FFmpeg command to replace video audio and add subtitles
            string escapedSubtitleFile = subtitleFile.Replace(@"\", @"\\").Replace(@":", @"\:");

            //alignment 10 for center, 2 for bottom center
            string subtitlesStyle = "'Fontsize=30,Alignment=2,Fontname=Helvetica,BackColour=&H000000,Spacing=0.2,Outline=1,Shadow=0.75'";

            // Input files
            string videoInput = $"-i \"{videoFile}\""; // Input video file
            string audioInput = $"-i \"{newAudioFile}\""; // Input audio file
            string subtitleInput = $"-i \"{subtitleFile}\""; // Input subtitle file
            string imageInput = $"-i \"{imagePath}\""; // Input screenshot file

            // Mapping streams
            //string videoMapping = "-map 0:v"; // Map video stream from the first input
            string audioMapping = "-map 1:a"; // Map audio stream from the second input

            // Subtitle and initial screenshot filters
            string combinedFilter = $"-filter_complex \"[0:v]subtitles='{escapedSubtitleFile}':force_style={subtitlesStyle}[sub];[sub][3]overlay=x=(main_w-overlay_w)/2:y=(main_h-overlay_h)/2:enable='between(t,0,3)'\"";

            // Encoding options
            string videoCodec = "-c:v libx264"; // Video codec
            string audioCodec = "-c:a aac"; // Audio codec
            string strictOption = "-strict -2"; // Strictness level for AAC codec

            // Duration
            string durationOption = $"-t {videoDuration}"; // Duration of the output video

            // Output file
            string outputOption = $"\"{outputVideoFile}\""; // Output file path

            // Concatenate all parts to form the complete command
            string command = $"{videoInput} {audioInput} {subtitleInput} {imageInput} {audioMapping} {combinedFilter} {videoCodec} {audioCodec} {strictOption} {durationOption} {outputOption}";

            return command;
        }
    }
}