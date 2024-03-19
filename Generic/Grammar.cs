using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reddit_scraper.Generic
{
    internal class Grammar
    {
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

            // Split content into sentences
            string[] sentences = content.Split(new char[] { '.' });

            // Correct capitalization for each sentence
            for (int i = 0; i < sentences.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(sentences[i]))
                {
                    // Trim whitespace and capitalize first letter
                    sentences[i] = char.ToUpper(sentences[i][0]) + sentences[i].Substring(1).ToLower();
                    // Fix standalone I
                    sentences[i] = Regex.Replace(sentences[i], @"\bi\b", "I");
                }
            }

            // Join sentences back into content
            content = string.Join(". ", sentences);

            return content.Trim();
        }
    }
}
