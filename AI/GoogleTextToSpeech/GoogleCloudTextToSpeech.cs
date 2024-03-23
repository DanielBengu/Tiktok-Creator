using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Google.Cloud.TextToSpeech.V1;
using Google.Cloud.Translation.V2;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Reddit_scraper.Generic;
using Duration = Google.Protobuf.WellKnownTypes.Duration;

namespace TextToSpeechApp
{
    public class GoogleAPI
    {
        static readonly string _credentialPath = "C:\\Users\\danie\\Downloads\\august-period-416720-ed316595a52b.json";
        public static async Task GenerateSpeech(string text, string outputFile, string languageCode = "en-US", string name = "en-US-Wavenet-C",
                                                SsmlVoiceGender voiceGender = SsmlVoiceGender.Neutral)
        {
            // Set up the TextToSpeechClient using the API key
            TextToSpeechClientBuilder builder = new()
            {
                CredentialsPath = _credentialPath
            };
            TextToSpeechClient client = await builder.BuildAsync();

            // Construct the synthesis input
            SynthesisInput input = new()
            {
                Text = text
            };

            // Construct the voice request
            VoiceSelectionParams voiceSelection = new()
            {
                LanguageCode = languageCode,
                Name = name,
                SsmlGender = voiceGender
            };

            // Construct the audio config
            AudioConfig audioConfig = new()
            {
                AudioEncoding = AudioEncoding.Linear16
            };

            // Perform the text-to-speech synthesis
            SynthesizeSpeechResponse response = await client.SynthesizeSpeechAsync(
                input, voiceSelection, audioConfig);

            // Write the audio content to a file
            using (Stream output = File.Create(outputFile))
            {
                response.AudioContent.WriteTo(output);
            }

            Console.WriteLine($"Audio saved to: {outputFile}");
        }

        public static async Task<string> TranslateText(string text, string targetLanguageCode)
        {
            // Initialize the TranslationClientBuilder
            TranslationClientBuilder builder = new()
            {
                CredentialsPath = _credentialPath
            };

            // Build the TranslationClient
            TranslationClient translationClient = await builder.BuildAsync();

            // Perform the translation
            try
            {
                // Perform the translation
                TranslationResult result = await translationClient.TranslateTextAsync(
                    text, targetLanguageCode);

                // Return the translated text
                return result.TranslatedText;
            }
            catch (Google.GoogleApiException ex)
            {
                // Handle the Google API exception
                Console.WriteLine($"Google API Exception: {ex.Message}");
                throw; // Rethrow the exception for handling at the caller level
            }
        }

        public static async Task<int> GenerateSrtAndReturnEndTime(string audioId, string audioFile, string outputSrtFile)
        {
            Console.WriteLine("Generating subtitles...");

            // Initialize the SpeechClient with Google Cloud credentials
            SpeechClientBuilder builder = new() { CredentialsPath = _credentialPath };
            SpeechClient speechClient = await builder.BuildAsync();

            // Create a StorageClient to work with Google Cloud Storage
            StorageClientBuilder storageClientBuilder = new() { CredentialsPath = _credentialPath };
            StorageClient storageClient = await storageClientBuilder.BuildAsync();

            // Upload the audio file to Google Cloud Storage
            string bucketName = "web_stories";
            using (FileStream fileStream = File.OpenRead(audioFile))
            {
                await storageClient.UploadObjectAsync(bucketName, audioId, null, fileStream);
            }

            // Construct the GCS URI for the audio file
            string gcsUri = $"gs://{bucketName}/{audioId}";

            // Perform speech-to-text transcription asynchronously
            var operation = await speechClient.LongRunningRecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                LanguageCode = "it-IT",
                EnableWordTimeOffsets = true
            }, RecognitionAudio.FromStorageUri(gcsUri));

            // Wait for the transcription operation to complete
            operation = await operation.PollUntilCompletedAsync();

            double timeSubsEnd = 0;
            List<string> subtitles = [];
            string content = string.Empty;
            DivideTextIntoLines(operation.Result.Results[0].Alternatives[0].Words);
            foreach (var result in operation.Result.Results)
                foreach (var alternative in result.Alternatives)
                    foreach (var wordInfo in alternative.Words)
                    {
                        string startTime = ToSrtTime(wordInfo.StartTime.ToTimeSpan());
                        string endTime = ToSrtTime(wordInfo.EndTime.ToTimeSpan());
                        timeSubsEnd = wordInfo.EndTime.ToTimeSpan().TotalSeconds;
                        string subtitleText = wordInfo.Word; // Use word instead of alternative.Transcript
                        subtitles.Add($"{subtitles.Count + 1}\n{startTime} --> {endTime}\n{subtitleText}\n");
                    }

            // Write subtitles to .srt file
            File.WriteAllLines(outputSrtFile, subtitles);
            Console.WriteLine("Subtitles generated");
            return (int)Math.Ceiling(timeSubsEnd);
        }

        static List<string> DivideTextIntoLines(RepeatedField<WordInfo> content)
        {
            List<string> subtitles = []; // Initialize the list using new List<string>()
            int maxWordLimit = 6;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;
            StringBuilder subtitleTextBuilder = new(); // Use StringBuilder for efficient string concatenation

            foreach (var word in content)
            {
                subtitleTextBuilder.Append($"{word} "); // Use Append method of StringBuilder to concatenate strings

                // Check if the length of the concatenated string exceeds the limit
                if (subtitleTextBuilder.Length > maxWordLimit)
                {
                    endTime = word.EndTime.ToTimeSpan();
                    subtitles.Add($"{subtitles.Count + 1}\n{startTime} --> {endTime}\n{subtitleTextBuilder}\n");
                    startTime = endTime.Add(TimeSpan.FromMilliseconds(1)); // Use endTime to calculate the new startTime
                    subtitleTextBuilder.Clear();
                }
            }

            // Add the remaining words as a final subtitle if they didn't meet the word limit
            if (subtitleTextBuilder.Length > 0)
            {
                endTime = content.Last().EndTime.ToTimeSpan(); // Use the end time of the last word
                subtitles.Add($"{subtitles.Count + 1}\n{startTime} --> {endTime}\n{subtitleTextBuilder}\n");
            }

            return subtitles;
        }

        // Method to convert Duration to SRT time format
        private static string ToSrtTime(TimeSpan duration)
        {
            return $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00},{duration.Milliseconds:000}";
        }
    }
}