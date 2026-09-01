using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BarTranslator {
    public class TranslationData {
        public string Original { get; set; } = string.Empty;
        public string Full { get; set; } = string.Empty;
        public string Short { get; set; } = string.Empty;
        public bool HasData { get; set; } = false;
        public long Timestamp { get; set; } = 0;
    }

    public static class TranslationEngine {
        private static readonly HttpClient client;
        private static readonly ConcurrentDictionary<string, TranslationData> cache = new(StringComparer.OrdinalIgnoreCase);

        static TranslationEngine() {
            var handler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            client = new HttpClient(handler) {
                Timeout = TimeSpan.FromSeconds(3)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        }

        public static bool ContainsEnglish(string text) {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return Regex.IsMatch(text, @"[a-zA-Z]");
        }

        public static string SanitizeText(string text) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // Normalize spaces, tabs, and newlines into a single space
            string cleaned = Regex.Replace(text, @"\s+", " ").Trim();
            return cleaned;
        }

        public static async Task<TranslationData?> TranslateToEnglishArabicAsync(string rawText) {
            string text = SanitizeText(rawText);
            if (string.IsNullOrWhiteSpace(text) || !ContainsEnglish(text)) {
                return null;
            }

            // Cap length to 800 chars to ensure instant translation and avoid massive selections
            if (text.Length > 800) {
                text = text.Substring(0, 800).Trim();
            }

            if (cache.TryGetValue(text, out var cachedData)) {
                return cachedData;
            }

            string translatedArabic = string.Empty;

            // Engine 1: Google Free Single API (translate.googleapis.com)
            try {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=ar&dt=t&q={Uri.EscapeDataString(text)}";
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) {
                        var sentences = root[0];
                        if (sentences.ValueKind == JsonValueKind.Array) {
                            foreach (var item in sentences.EnumerateArray()) {
                                if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0 && item[0].ValueKind == JsonValueKind.String) {
                                    translatedArabic += item[0].GetString();
                                }
                            }
                        }
                    }
                }
            } catch {}

            // Engine 2: Google Dict API Fallback (clients5.google.com)
            if (string.IsNullOrWhiteSpace(translatedArabic)) {
                try {
                    string url = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex&sl=en&tl=ar&q={Uri.EscapeDataString(text)}";
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        string json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.String) {
                            translatedArabic = root[0].GetString() ?? string.Empty;
                        }
                    }
                } catch {}
            }

            if (string.IsNullOrWhiteSpace(translatedArabic)) {
                return null;
            }

            // Decode HTML entities and clean spaces
            translatedArabic = WebUtility.HtmlDecode(translatedArabic).Trim();
            translatedArabic = Regex.Replace(translatedArabic, @"\s+", " ");

            // Create smart short form (first word / first part)
            string shortForm = GenerateShortForm(translatedArabic);

            var result = new TranslationData {
                Original = text,
                Full = translatedArabic,
                Short = shortForm,
                HasData = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Cache up to 400 entries
            if (cache.Count > 400) {
                cache.Clear();
            }
            cache[text] = result;

            return result;
        }

        private static string GenerateShortForm(string arabicText) {
            if (string.IsNullOrWhiteSpace(arabicText)) return string.Empty;

            string[] words = arabicText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1) {
                return arabicText;
            }

            // If the first word is very short (e.g. preposition like في, من, على, لا), keep first two words
            if (words[0].Length <= 3 && words.Length >= 2) {
                return $"{words[0]} {words[1]}…";
            }

            return $"{words[0]}…";
        }
    }
}
