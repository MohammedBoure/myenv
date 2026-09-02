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
        public string OriginalShort { get; set; } = string.Empty;
        public string Full { get; set; } = string.Empty;
        public string Short { get; set; } = string.Empty;
        public string DisplayShort { get; set; } = string.Empty;
        public string DisplayFull { get; set; } = string.Empty;
        public bool HasData { get; set; } = false;
        public long Timestamp { get; set; } = 0;
    }

    public static class TranslationEngine {
        private static readonly HttpClient client;
        private static readonly ConcurrentDictionary<string, TranslationData> cache = new(StringComparer.OrdinalIgnoreCase);

        static TranslationEngine() {
            var handler = new SocketsHttpHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            };
            client = new HttpClient(handler) {
                Timeout = TimeSpan.FromSeconds(2.5)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        }

        public static bool ContainsEnglish(string text) {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return Regex.IsMatch(text, @"[a-zA-Z]");
        }

        public static string SanitizeText(string text) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static async Task<TranslationData?> TranslateToEnglishArabicAsync(string rawText) {
            string text = SanitizeText(rawText);
            if (string.IsNullOrWhiteSpace(text) || !ContainsEnglish(text)) {
                return null;
            }

            // Word count limit: Up to 60 words for quick sentence translation
            string[] origWords = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (origWords.Length > 60) {
                return null;
            }

            // In-memory instant cache lookup (0ms)
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

            // Clean Arabic text
            translatedArabic = WebUtility.HtmlDecode(translatedArabic).Trim();
            translatedArabic = Regex.Replace(translatedArabic, @"\s+", " ");

            // Generate short form for Arabic (first word / part)
            string shortArabic = GenerateShortArabic(translatedArabic);

            // Generate short form for original English
            string shortEnglish = GenerateShortEnglish(origWords);

            // Continuous bilingual display formats (respecting ShowEnglish setting)
            string displayShort = StateManager.ShowEnglish ? $"{shortEnglish} ➔ {shortArabic}" : shortArabic;
            string displayFull = StateManager.ShowEnglish ? $"{text} ➔ {translatedArabic}" : translatedArabic;

            var result = new TranslationData {
                Original = text,
                OriginalShort = shortEnglish,
                Full = translatedArabic,
                Short = shortArabic,
                DisplayShort = displayShort,
                DisplayFull = displayFull,
                HasData = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Cache up to 600 entries
            if (cache.Count > 600) {
                cache.Clear();
            }
            cache[text] = result;

            return result;
        }

        private static string GenerateShortArabic(string arabicText) {
            if (string.IsNullOrWhiteSpace(arabicText)) return string.Empty;

            string[] words = arabicText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1) {
                return arabicText;
            }

            // If the first word is a short particle/preposition (e.g. في, من, لا), keep first two words
            if (words[0].Length <= 3 && words.Length >= 2) {
                return $"{words[0]} {words[1]}…";
            }

            return $"{words[0]}…";
        }

        private static string GenerateShortEnglish(string[] origWords) {
            if (origWords.Length <= 2) {
                return string.Join(" ", origWords);
            }
            return $"{origWords[0]} {origWords[1]}…";
        }
    }
}
