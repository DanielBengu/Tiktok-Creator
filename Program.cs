using Reddit_scraper;
using Reddit_scraper.ImageService;
using Reddit_scraper.Reddit;
using Reddit_scraper.VideoMixer;
using System.Data;
using TextToSpeechApp;

using HttpClient client = new();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

RedditScraper redditScraper = new(client, new());
bool keepLoop = true;
var skipGetCommand = false;
Tuple<Command, object> data = new(Command.Invalid, string.Empty);
while (keepLoop)
{
    if(!skipGetCommand)
        data = await GetCommand(redditScraper);

    switch (data.Item1)
    {
        case Command.Invalid:
        case Command.NotSupported:
        default:
            skipGetCommand = false;
            break;
        case Command.Exit:
            keepLoop = false;
            break;
        case Command.AlreadyProcessed:
            Console.WriteLine("Post already processed, do it anyway? y/n");
            var key = Console.ReadKey();
            Console.WriteLine();
            if(key.Key == ConsoleKey.Y)
            {
                skipGetCommand = true;
                data = new(Command.RedditPost, data.Item2);
            }
            break;
        case Command.RedditPost:
            RedditPost post = (RedditPost)data.Item2;
            await Reddit.GenerateRedditPost(post);
            skipGetCommand = false;
            break;
    }
}

static async Task<Tuple<Command, object>> GetCommand(RedditScraper redditScraper)
{
    Console.WriteLine("Enter Reddit post URL to scrape (or type 'exit' to quit):");
    string? url = Console.ReadLine();

    if (string.IsNullOrEmpty(url))
        return new(Command.Invalid, string.Empty);

    if (url.Equals("exit", StringComparison.OrdinalIgnoreCase))
        return new(Command.Exit, string.Empty);

    // Validate the URL (you may want to add more robust URL validation)
    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        Console.WriteLine("Invalid URL. Please enter a valid Reddit post URL.");
        return new(Command.Invalid, string.Empty);
    }

    // Call the ScrapeRedditPost method
    var post = await redditScraper.ScrapeRedditPost(url); // Wait for the asynchronous method to complete

    if(post == null)
       return new(Command.NotSupported, string.Empty);

    if (post.AlreadyProcessed == false)
        return new (Command.RedditPost, post);
    else
        return new(Command.AlreadyProcessed, post);
}

enum Command
{
    AlreadyProcessed,
    Invalid,
    Exit,
    NotSupported,
    RedditPost
}