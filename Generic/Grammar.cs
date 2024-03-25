using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Reddit_scraper.Generic
{
    internal class Grammar
    {
        private static readonly Dictionary<string, string> _censorDictionary = new()
        {
            { "aita", "sono infame" },
            { "sex", "andare a letto" },
            { "dollar", "euro" },
            { "dollars", "euro" }
        };

        public static string RemoveEdits(string content)
        {
            return Regex.Replace(content, @"(?i)EDIT:.*$", "");
        }

        public static string ReplaceAgeGender(string input)
        {
            // Define the regular expression pattern
            string pattern = @"\b(\d+)([MF])\b";

            // Perform the replacement
            string replacedText = Regex.Replace(input, pattern, match =>
            {
                int age = int.Parse(match.Groups[1].Value);
                string gender = match.Groups[2].Value;

                // Define the replacement string format based on gender
                string replacementText = gender.Equals("F", StringComparison.OrdinalIgnoreCase) ? "donna di {0} anni" : "uomo di {0} anni";

                return string.Format(replacementText, age);
            });

            return replacedText;
        }

        public static string ReplaceDollarsToEuros(string input)
        {
            // Define the regular expression pattern to match sums in dollars
            string pattern = @"\$(\d+(?:\.\d{2})?)";

            // Perform the replacement
            string replacedText = Regex.Replace(input, pattern, match =>
            {
                decimal dollars = decimal.Parse(match.Groups[1].Value);

                // Format the replacement string with euros
                return $"{Math.Floor(dollars)} euro";
            });

            return replacedText;
        }

        public static string CorrectGrammar(string content)
        {
            // Remove extra whitespaces
            content = Regex.Replace(content, @"\s+", " ");

            // Correct common punctuation errors
            content = Regex.Replace(content, @"\s+,", ",");
            content = Regex.Replace(content, @"\s+\.(\s*\.)*", ".");
            content = Regex.Replace(content, @"\s+!", "!");
            content = Regex.Replace(content, @"\s+\?", "?");
            content = Regex.Replace(content, @"\s+;", ";");
            content = Regex.Replace(content, @"\s+:", ":");

            content = ReplaceAgeGender(content);
            content = ReplaceDollarsToEuros(content);

            // Split content into sentences
            string[] sentences = content.Split(['.']);

            // Correct capitalization for each sentence
            for (int i = 0; i < sentences.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(sentences[i]))
                {
                    // Trim whitespace and capitalize first letter
                    sentences[i] = char.ToUpper(sentences[i][0]) + sentences[i][1..].ToLower();
                    // Fix standalone I
                    sentences[i] = Regex.Replace(sentences[i], @"\bi\b", "I");
                }
            }

            // Join sentences back into content
            content = string.Join(". ", sentences);

            return content.Trim();
        }

        public static string CensorContent(string content)
        {
            string censoredContent = content;

            foreach (var pair in _censorDictionary)
            {
                string originalWord = pair.Key;
                string censoredWord = pair.Value;
                censoredContent = Regex.Replace(censoredContent, @"\b" + originalWord + @"\b", censoredWord, RegexOptions.IgnoreCase);
            }

            return censoredContent;
        }
    }
}
