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
                Timeout = TimeSpan.FromSeconds(3.0)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
        }

        public static bool ContainsValidText(string text) {
            if (string.IsNullOrWhiteSpace(text)) return false;
            // Must contain at least one letter (Unicode letter category)
            return Regex.IsMatch(text, @"[\p{L}]");
        }

        public static bool ContainsEnglish(string text) {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return Regex.IsMatch(text, @"[a-zA-Z]");
        }

        public static bool ContainsArabic(string text) {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return Regex.IsMatch(text, @"[\u0600-\u06FF]");
        }

        public static string SanitizeText(string text) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static async Task<TranslationData?> TranslateToEnglishArabicAsync(string rawText) {
            return await TranslateAsync(rawText);
        }

        public static async Task<TranslationData?> TranslateAsync(string rawText) {
            string text = SanitizeText(rawText);
            if (string.IsNullOrWhiteSpace(text) || !ContainsValidText(text)) {
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

            // Determine translation direction
            bool isSourceEnglish = ContainsEnglish(text);
            string targetLang = isSourceEnglish ? "ar" : (ContainsArabic(text) ? "en" : "ar");
            string sourceLang = isSourceEnglish ? "en" : "auto";

            string translatedText = string.Empty;

            // Engine 1: Google Dict Client API (clients5.google.com) - Ultra reliable & fast
            try {
                string url = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex&sl={sourceLang}&tl={targetLang}&q={Uri.EscapeDataString(text)}";
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    translatedText = ParseGoogleClientResponse(json);
                }
            } catch {}

            // Engine 2: Google Translate API (translate.googleapis.com) with dict-chrome-ex client
            if (string.IsNullOrWhiteSpace(translatedText)) {
                try {
                    string url = $"https://translate.googleapis.com/translate_a/t?client=dict-chrome-ex&sl={sourceLang}&tl={targetLang}&q={Uri.EscapeDataString(text)}";
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        string json = await response.Content.ReadAsStringAsync();
                        translatedText = ParseGoogleClientResponse(json);
                    }
                } catch {}
            }

            // Engine 3: MyMemory API Fallback
            if (string.IsNullOrWhiteSpace(translatedText)) {
                try {
                    string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={(isSourceEnglish ? "en" : "auto")}|{targetLang}";
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        string json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("responseData", out var respData) &&
                            respData.TryGetProperty("translatedText", out var transTextProp)) {
                            translatedText = transTextProp.GetString() ?? string.Empty;
                        }
                    }
                } catch {}
            }

            if (string.IsNullOrWhiteSpace(translatedText)) {
                return null;
            }

            // Clean translated text
            translatedText = WebUtility.HtmlDecode(translatedText).Trim();
            translatedText = Regex.Replace(translatedText, @"\s+", " ");

            // Generate short form for translated text
            string shortTranslated = GenerateShortText(translatedText, targetLang == "ar");

            // Generate short form for original text
            string shortOriginal = GenerateShortText(text, !isSourceEnglish);

            // Display formats respecting ShowEnglish setting
            string displayShort = StateManager.ShowEnglish ? $"{shortOriginal} ➔ {shortTranslated}" : shortTranslated;
            string displayFull = StateManager.ShowEnglish ? $"{text} ➔ {translatedText}" : translatedText;

            var result = new TranslationData {
                Original = text,
                OriginalShort = shortOriginal,
                Full = translatedText,
                Short = shortTranslated,
                DisplayShort = displayShort,
                DisplayFull = displayFull,
                HasData = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Cache up to 1000 entries
            if (cache.Count > 1000) {
                cache.Clear();
            }
            cache[text] = result;

            return result;
        }

        private static string ParseGoogleClientResponse(string json) {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;
            try {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Format 1: ["Translated text"]
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) {
                    var first = root[0];
                    if (first.ValueKind == JsonValueKind.String) {
                        return first.GetString() ?? string.Empty;
                    }
                    // Format 2: [["Translated text", "en"]]
                    if (first.ValueKind == JsonValueKind.Array && first.GetArrayLength() > 0) {
                        var nested = first[0];
                        if (nested.ValueKind == JsonValueKind.String) {
                            return nested.GetString() ?? string.Empty;
                        }
                    }
                } else if (root.ValueKind == JsonValueKind.String) {
                    return root.GetString() ?? string.Empty;
                }
            } catch {}
            return string.Empty;
        }

        private static string GenerateShortText(string text, bool isArabic) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1) {
                return text;
            }

            if (isArabic) {
                // If the first word is a short particle/preposition (e.g. في, من, لا, عن), keep first two words
                if (words[0].Length <= 3 && words.Length >= 2) {
                    return $"{words[0]} {words[1]}…";
                }
                return $"{words[0]}…";
            }

            if (words.Length <= 2) {
                return string.Join(" ", words);
            }
            return $"{words[0]} {words[1]}…";
        }
    }
}
