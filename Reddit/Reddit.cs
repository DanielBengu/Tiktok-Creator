using Reddit_scraper.ImageService;
using Reddit_scraper.VideoMixer;
using TextToSpeechApp;

namespace Reddit_scraper.Reddit
{
    internal class Reddit
    {
        public static async Task<bool> GenerateRedditPost(RedditPost post)
        {
            string subtitlePath;
            int subtitleDuration;

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePostPathOutput = $"{documentsPath}\\RedditScraper\\{post.Id}\\";
            string basePostPathContent = $"{basePostPathOutput}\\Content\\";
            if (!Directory.Exists(basePostPathContent))
                Directory.CreateDirectory(basePostPathContent);

            /*
            //Google Gemini AI (Chat)
            try
            {
                string response = await GeminiQuickstart.GenerateContent("prompt");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            */
            //AI Image Generator 
            /*
            EdenAIImageGenerator edenAIImageGenerator = new();
            Image? ai_image = await edenAIImageGenerator.GenerateImageAsync(post.Title);
            EdenAIImageGenerator.SaveImage(ai_image, post);
            */

            //Generate video caption
            string captionFile = Path.Combine(basePostPathOutput, "caption.txt");
            File.WriteAllText(captionFile, $"{post.Title} #minecraft #reddit #redditstories #redditstorytime #minecraftrun #storytime #story #fyp #fypシ");

            //Profound Woman: it-IT, en-US-Wavenet-C, Female
            //Text-to-speech
            string filePathAudio = Path.Combine(basePostPathContent, $"audio.mp3");
            await GoogleAPI.GenerateSpeech($"{post.Title}. {post.Content}", filePathAudio, "it-IT", "en-US-Wavenet-C", Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Female);

            try
            {
                //Create subtitles
                subtitlePath = Path.Combine(basePostPathContent, $"sub.srt");
                subtitleDuration = await GoogleAPI.GenerateSrtAndReturnEndTime(post.Id,filePathAudio, subtitlePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during subtitle generation: {ex.Message}");
                return false;
            }

            //Create screenshot TODO
            string imagePath = ScreenshotService.GenerateScreenshot(basePostPathContent, post.Title);

            //Video
            string videoPath = Path.Combine(basePostPathOutput, $"video.mp4");
            string baseVideoFile = "C:\\Users\\danie\\Videos\\Downloader\\Background videos\\Minecraft\\basic_minecraft.mp4";
            if (subtitleDuration > 80)
                baseVideoFile = "C:\\Users\\danie\\Videos\\Downloader\\Background videos\\Minecraft\\4min.mp4";
            VideoMixing.GenerateVideo(baseVideoFile, filePathAudio, imagePath, subtitlePath, subtitleDuration, videoPath, "C:\\Users\\danie\\Downloads\\ffmpeg-2024-03-07-git-97beb63a66-full_build\\bin\\ffmpeg.exe");

            return true;
        }
    }
}
