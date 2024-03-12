using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reddit_scraper.AI.RandomStuff
{
    public static class GeminiQuickstart
    {
        static readonly string _credentialPath = "C:\\Users\\danie\\Downloads\\august-period-416720-ed316595a52b.json";
        public static async Task<string> GenerateContent(
            string prompt,
            string projectId = "august-period-416720",
            string location = "us-central1",
            string publisher = "google",
            string model = "gemini-1.0-pro-vision"
        )
        {
            // Create client
            var predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{location}-aiplatform.googleapis.com",
                CredentialsPath = _credentialPath
            }.Build();

            //string imageUri = "gs://generativeai-downloads/images/scones.jpg";

            // Initialize request argument(s)
            var content = new Content
            {
                Role = "USER"
            };
            content.Parts.AddRange(new List<Part>()
            {
                new() {
                    Text = prompt
                }
                /*
                new() {
                    FileData = new() {
                        MimeType = "image/png",
                        FileUri = imageUri
                    }
                }
                */
            });

            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.4f,
                    TopP = 1,
                    TopK = 32,
                    MaxOutputTokens = 2048
                }
            };
            generateContentRequest.Contents.Add(content);

            // Make the request, returning a streaming response
            using PredictionServiceClient.StreamGenerateContentStream response = predictionServiceClient.StreamGenerateContent(generateContentRequest);

            StringBuilder fullText = new();

            // Read streaming responses from server until complete
            AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
            await foreach (GenerateContentResponse responseItem in responseStream)
            {
                fullText.Append(responseItem.Candidates[0].Content.Parts[0].Text);
            }

            return fullText.ToString();
        }
    }
}