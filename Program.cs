using DeepAIImageGeneration;
using Reddit_scraper;
using Reddit_scraper.AI.RandomStuff;
using Reddit_scraper.ImageService;
using Reddit_scraper.VideoMixer;
using System.Drawing;
using System.Drawing.Imaging;
using TextToSpeechApp;

using HttpClient client = new();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

RedditScraper redditScraper = new(client, new());

while (true)
{
    string subtitlePath = string.Empty;
    int subtitleDuration = 0;
    Console.WriteLine("Enter Reddit post URL to scrape (or type 'exit' to quit):");
    string? url = Console.ReadLine();

    if (string.IsNullOrEmpty(url))
        continue;

    if (url.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // Validate the URL (you may want to add more robust URL validation)
    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        Console.WriteLine("Invalid URL. Please enter a valid Reddit post URL.");
        continue;
    }

    // Call the ScrapeRedditPost method
    var post = await redditScraper.ScrapeRedditPost(url); // Wait for the asynchronous method to complete
    if (post == null)
        continue;

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
    
    //Profound Woman: it-IT, en-US-Wavenet-C, Female
    //Text-to-speech
    string filePathAudio = Path.Combine(basePostPathContent, $"audio.mp3");
    await GoogleAPI.GenerateSpeech($"{post.Title}. {post.Content}", filePathAudio, "it-IT", "en-US-Wavenet-C", Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Female);

    try
    {
        //Create subtitles
        subtitlePath = Path.Combine(basePostPathContent, $"sub.srt");
        subtitleDuration = await GoogleAPI.GenerateSrtAndReturnEndTime(filePathAudio, subtitlePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during subtitle generation: {ex.Message}");
        continue;
    }

    //Create screenshot TODO
    string imagePath = ScreenshotService.GenerateScreenshot(basePostPathContent, post.Title);

    //Video
    string videoPath = Path.Combine(basePostPathOutput, $"video.mp4");
    string baseVideoFile = "C:\\Users\\danie\\Videos\\Downloader\\basic_minecraft.mp4";
    VideoMixing.GenerateVideo(baseVideoFile, filePathAudio, imagePath, subtitlePath, subtitleDuration, videoPath, "C:\\Users\\danie\\Downloads\\ffmpeg-2024-03-07-git-97beb63a66-full_build\\bin\\ffmpeg.exe");
}