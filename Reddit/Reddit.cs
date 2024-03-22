﻿using Google.Cloud.TextToSpeech.V1;
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
            string videoCaption = $"{post.Title} #reddit #redditstories #fyp #perte";
            File.WriteAllText(captionFile, videoCaption);

            Console.WriteLine("Select gender: M/F");
            ConsoleKeyInfo gender = Console.ReadKey();
            Console.ReadLine();

            SsmlVoiceGender ssmlVoiceGender = SsmlVoiceGender.Male;
            string name = "it-IT-Neural2-C";

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
            string filePathAudio = Path.Combine(basePostPathContent, $"audio.mp3");
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
            string videoPath = Path.Combine(basePostPathOutput, $"video.mp4");
            string baseVideoFile = BackgroundVideo.GetRandomBackgroundVideoPath(subtitleDuration, @"C:\Users\danie\Videos\Downloader\Background videos\Minecraft", out int width);

            //Create screenshot TODO
            string imagePath = ScreenshotService.GenerateScreenshot(basePostPathContent, post.Title, width);

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
