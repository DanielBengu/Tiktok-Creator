using System.Text;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Google.Cloud.TextToSpeech.V1;
using Google.Cloud.Translation.V2;
using Google.Protobuf.Collections;

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

            List<string> subtitles = [];
            TimeSpan endTime = TimeSpan.Zero; // Initialize the end time

            // Process the transcription results and generate subtitles
            for (int i = 0; i < operation.Result.Results.Count; i++)
            {
                // Pass the subtitles list and update the end time
                endTime = DivideTextIntoLines(operation.Result.Results[i].Alternatives[0].Words, subtitles);
            }

            // Write subtitles to .srt file
            File.WriteAllLines(outputSrtFile, subtitles);
            Console.WriteLine("Subtitles generated");

            return (int)Math.Ceiling(endTime.TotalSeconds);
        }

        static TimeSpan DivideTextIntoLines(RepeatedField<WordInfo> content, List<string> subtitles)
        {
            int maxWordLimit = 8;
            StringBuilder subtitleTextBuilder = new(); // Use StringBuilder for efficient string concatenation
            TimeSpan endTime = TimeSpan.Zero;
            TimeSpan startTime = TimeSpan.Zero;
            foreach (var word in content)
            {
                if (string.IsNullOrEmpty(subtitleTextBuilder.ToString()))
                    startTime = word.StartTime.ToTimeSpan();

                subtitleTextBuilder.Append($"{word.Word.ToUpper()} ");

                // Check if the length of the concatenated string exceeds the limit
                if (subtitleTextBuilder.Length > maxWordLimit)
                {
                    // Add the subtitle to the list
                    subtitles.Add($"{subtitles.Count + 1}\n{ToSrtTime(startTime)} --> {ToSrtTime(word.EndTime.ToTimeSpan())}\n{subtitleTextBuilder.ToString().Trim()}\n");
                    subtitleTextBuilder.Clear();
                    endTime = word.EndTime.ToTimeSpan(); // Update the end time
                }
            }

            // Add the remaining words as a final subtitle if they didn't meet the word limit
            if (subtitleTextBuilder.Length > 0)
            {
                // You can use the last word's end time as the end time for the remaining words
                var lastWord = content.LastOrDefault();
                if (lastWord != null)
                {
                    // Add the last subtitle to the list
                    subtitles.Add($"{subtitles.Count + 1}\n{ToSrtTime(startTime)} --> {ToSrtTime(lastWord.EndTime.ToTimeSpan())}\n{subtitleTextBuilder.ToString().Trim()}\n");
                    endTime = lastWord.EndTime.ToTimeSpan(); // Update the end time
                }
            }

            return endTime; // Return the calculated end time
        }

        // Method to convert Duration to SRT time format
        private static string ToSrtTime(TimeSpan duration)
        {
            return $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00},{duration.Milliseconds:000}";
        }
    }
}