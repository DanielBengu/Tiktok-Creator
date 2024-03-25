using Google.Cloud.TextToSpeech.V1;
using Reddit_scraper.AI.RandomStuff;
using Reddit_scraper.Generic;
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

            string filePathAudio = Path.Combine(basePostPathContent, $"audio.mp3");
            string videoPath = Path.Combine(basePostPathOutput, $"video.mp4");

            //Generate video caption
            Console.WriteLine($"Title generated: {post.Title}\nDo you accept the title? y/n");
            ConsoleKeyInfo choice = Console.ReadKey();
            Console.WriteLine();
            if (choice.Key == ConsoleKey.N)
            {
                Console.WriteLine("Insert new title");
                string? newTitle = Console.ReadLine();
                if (!string.IsNullOrEmpty(newTitle))
                    post.Title = newTitle;
            }

            string captionFile = Path.Combine(basePostPathOutput, "caption.txt");
            string videoCaption = $"{post.Title} #reddit #storie #fyp #perte #redditita";

            File.WriteAllText(captionFile, videoCaption);
            GenericService.CalculateAndSaveBestTimeForPosting(basePostPathOutput);

            //Google Gemini AI (Chat)
            try
            {
                bool enabled = false;
                string prompt = $"rewrite this story, make it short and remove unnecessary parts: {post.Content}";
                string response = await GeminiQuickstart.GenerateContent(prompt, enabled);
                if(!string.IsNullOrEmpty(response))
                    Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Select gender: M/F");
            ConsoleKeyInfo gender = Console.ReadKey();
            Console.WriteLine();

            SsmlVoiceGender ssmlVoiceGender = SsmlVoiceGender.Male;
            string name = "it-IT-Standard-D";

            if (gender.Key == ConsoleKey.F)
            {
                Console.WriteLine("Female selected");
                ssmlVoiceGender = SsmlVoiceGender.Female;
                name = "it-IT-Standard-B";

            } else if(gender.Key == ConsoleKey.M) {
                Console.WriteLine("Male selected");
            }
            else
            {
                Console.WriteLine("Key not recognized, default to male");
            }
            //Profound Woman: it-IT, en-US-Wavenet-C, Female
            //Text-to-speech
            await GoogleAPI.GenerateSpeech($"{post.Title}. {post.Content}", filePathAudio, "it-IT", name, ssmlVoiceGender);

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

            //Video
            string baseVideoFile = GenericService.GetRandomBackgroundVideoPath(subtitleDuration, @"C:\Users\danie\Videos\Downloader\Background videos\Minecraft", out int width);

            //Create screenshot TODO
            string imagePath = ScreenshotService.GenerateScreenshot(basePostPathContent, post.Title, width);

            if (File.Exists(videoPath))
            {
                Console.WriteLine("Video already existing, removing it");
                File.Delete(videoPath);
            }

            VideoMixing.GenerateVideo(baseVideoFile, filePathAudio, imagePath, subtitlePath, subtitleDuration, videoPath, "C:\\Users\\danie\\Downloads\\ffmpeg-2024-03-07-git-97beb63a66-full_build\\bin\\ffmpeg.exe");

            Thread.Sleep(500);

            try
            {
                File.Move(Path.Combine(basePostPathOutput, $"video.mp4"), Path.Combine(basePostPathOutput, $"{videoCaption}.mp4"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during the renaming of the video: {ex.Message}");
            }
            
            return true;
        }
    }
}
