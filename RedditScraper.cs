using DeepAIImageGeneration;
using HtmlAgilityPack;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reddit_scraper
{
    internal partial class RedditScraper
    {
        private readonly HttpClient _client;
        private readonly RedditPostWriter _postWriter;

        public RedditScraper(HttpClient httpClient, RedditPostWriter postWriter)
        {
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _postWriter = postWriter ?? throw new ArgumentNullException(nameof(postWriter));
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        }

        public async Task<RedditPost?> ScrapeRedditPost(string url)
        {
            HttpResponseMessage response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to retrieve data from Reddit. Status code: {response.StatusCode}");
                return null;
            }

            string html = await response.Content.ReadAsStringAsync();

            RedditPost? redditPost = ExtractRedditPost(html, url);

            if (redditPost != null)
            {
                RedditPostWriter.AppendToJsonFile(redditPost);
            }
            else
            {
                Console.WriteLine("No <shreddit-post> element found in the HTML.");
            }

            return redditPost;
        }

        private static RedditPost? ExtractRedditPost(string html, string url)
        {
            // Use Regex to capture the <shreddit-post> element
            Regex regex = MyRegex();
            Match match = regex.Match(html);

            if (match.Success)
            {
                // Extract the captured HTML
                string shredditPostHtml = match.Value;

                // Load the extracted HTML into another HtmlDocument
                HtmlDocument postDoc = new();
                postDoc.LoadHtml(shredditPostHtml);

                // Select the post node
                HtmlNode postNode = postDoc.DocumentNode.SelectSingleNode("//shreddit-post");

                if (postNode == null)
                    return null;

                // Get the post type (text or image)
                string? postType = postNode?.Attributes["post-type"]?.Value;

                // Get the post title
                string? postTitle = postNode?.Attributes["post-title"]?.Value;
                string? decodedPostTitle = WebUtility.HtmlDecode(postTitle);

                // Get the post ID from the URL
                string postId = ExtractPostIdFromUrl(url);

                //Removed the possible null reference since we already check
#pragma warning disable CS8602
                // Get the post content
                HtmlNode contentNode = postNode.SelectSingleNode($".//div[@slot='text-body']//div[@id='t3_{postId}-post-rtjson-content']//p");
                string postContent = contentNode != null ? HtmlEntity.DeEntitize(contentNode.InnerText.Trim()) : "";
#pragma warning restore CS8602

                if (string.IsNullOrEmpty(postType) || string.IsNullOrEmpty(decodedPostTitle))
                    return null;

                // If it's an image post, get the image link
                string? imageUrl = postType == "image" || postType == "multi_media" ? postNode?.Attributes["content-href"]?.Value : "";

                return new RedditPost
                {
                    Id = postId,
                    Type = postType,
                    Title = decodedPostTitle,
                    Content = postContent,
                    ImageUrl = imageUrl ?? string.Empty,
                };
            }

            return null;
        }

        private static string ExtractPostIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                return string.Empty;

            string[] segments = uri.Segments;
            if (segments.Length < 3)
                return string.Empty;

            return segments[^2].TrimEnd('/');
        }


        private void PrintPostInfo(RedditPost redditPost)
        {
            Console.WriteLine($"Post Type: {redditPost.Type}");
            Console.WriteLine($"Post Title: {redditPost.Title}");
            Console.WriteLine($"Post Content: {redditPost.Content}");

            if (!string.IsNullOrEmpty(redditPost.ImageUrl))
            {
                Console.WriteLine($"Image URL: {redditPost.ImageUrl}");
            }
        }

        [GeneratedRegex(@"<shreddit-post.*?</shreddit-post>", RegexOptions.Singleline)]
        private static partial Regex MyRegex();
    }

    public class RedditPost
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}