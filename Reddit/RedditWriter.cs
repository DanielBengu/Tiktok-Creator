using System;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Reddit_scraper.Reddit
{
    internal class RedditPostWriter
    {
        public static bool AppendToJsonFile(RedditPost post)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(documentsPath, "reddit_posts.json");

            RedditPostData? existingData = new();

            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                existingData = JsonConvert.DeserializeObject<RedditPostData>(existingJson);
            }

            // Check if the post ID already exists in the existing data
            if (existingData != null && existingData.Posts.Exists(p => p.Id == post.Id))
            {
                Console.WriteLine($"Post with ID '{post.Id}' already exists. Skipping appending.");
                post.AlreadyProcessed = true;
                return false;
            }

            existingData.Posts.Add(post);

            string json = JsonConvert.SerializeObject(existingData, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Console.WriteLine($"Post data appended to: {filePath}");

            return true;
        }
    }
}
