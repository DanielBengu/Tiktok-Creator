using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Google.Cloud.TextToSpeech.V1;
using Google.Cloud.Translation.V2;
using Reddit_scraper.Generic;

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
            return (int)Math.Ceiling(timeSubsEnd);
        }

        // Method to convert Duration to SRT time format
        private static string ToSrtTime(TimeSpan duration)
        {
            return $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00},{duration.Milliseconds:000}";
        }
    }
}