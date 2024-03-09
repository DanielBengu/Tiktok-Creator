using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeepAIImageGeneration
{
    public class EdenAIImageGenerator
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _apiUrl = "https://api.edenai.run/v2/image/generation";
        private readonly string _apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiODRmZDAzOGYtNDA1OC00OGM3LWI5NjYtZGNiZTA2YTE4NWVhIiwidHlwZSI6ImFwaV90b2tlbiJ9.57K-dJGEYukQWz3BzY7oM9JKjW26uzb2T0DvLlrJR7k";

        public async Task<Image?> GenerateImageAsync(string prompt)
        {
            try
            {
                var payload = new
                {
                    providers = "openai",
                    text = prompt,
                    resolution = "512x512",
                    fallback_providers = ""
                };

                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);

                var requestBody = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(_apiUrl, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Deserialize JSON response into C# objects
                    var responseObject = System.Text.Json.JsonSerializer.Deserialize<OpenAI.OpenAiResponse>(responseContent);

                    // Return the image URL
                    return ConvertAndSaveImage(responseObject?.openai?.items?[0].image);
                }
                else
                {
                    Console.WriteLine($"Failed to generate image. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating image: {ex.Message}");
                return null;
            }
        }

        public static Image? ConvertAndSaveImage(string base64Image)
        {
            try
            {
                // Convert base64 string to byte array
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Create MemoryStream from byte array
                using MemoryStream ms = new(imageBytes);
                // Create Image from MemoryStream
                Image image = Image.FromStream(ms);

                return image;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting and saving image: {ex.Message}");
                return null;
            }
        }
    }
}
