using DeepAIImageGeneration;
using Reddit_scraper;
using Reddit_scraper.VideoMixer;
using System.Drawing;
using System.Drawing.Imaging;
using TextToSpeechApp;

using HttpClient client = new();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

RedditScraper redditScraper = new(client, new());

while (true)
{
    
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
    string basePostPath = $"{documentsPath}\\RedditScraper\\{post.Id}";

    if (!Directory.Exists(basePostPath))
        Directory.CreateDirectory(basePostPath);

    //AI Image Generator
    /*
    EdenAIImageGenerator edenAIImageGenerator = new();
    Image? ai_image = await edenAIImageGenerator.GenerateImageAsync(post.Title);
    EdenAIImageGenerator.SaveImage(ai_image, post);
    */

    //Profound Woman: it-IT, en-US-Wavenet-C, Female
    //Text-to-speech
    string filePathTitle = Path.Combine(basePostPath, $"title.mp3");
    string filePathDesc = Path.Combine(basePostPath, $"description.mp3");
    await GoogleAPI.GenerateSpeech(post.Title, filePathTitle, "it-IT", "en-US-Wavenet-C", Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Female);
    await GoogleAPI.GenerateSpeech(post.Content, filePathDesc, "it-IT", "en-US-Wavenet-C", Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Female);

    //Create subtitles
    string subtitlePath = Path.Combine(basePostPath, $"sub.srt");
    await GoogleAPI.GenerateSrt(filePathDesc, subtitlePath);

    //Video
    string videoPath = Path.Combine(basePostPath, $"video.mp4");
    string baseVideoFile = "C:\\Users\\danie\\Videos\\Downloader\\basic_minecraft.mp4";
    VideoMixing.ReplaceAudio(baseVideoFile, filePathDesc, subtitlePath, videoPath, "C:\\Users\\danie\\Downloads\\ffmpeg-2024-03-07-git-97beb63a66-full_build\\bin\\ffmpeg.exe");
}