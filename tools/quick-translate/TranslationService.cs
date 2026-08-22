using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickTranslate {
    public class TranslationResult {
        public string OriginalText { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = "AUTO";
        public string TargetLanguage { get; set; } = "EN";
        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class TranslationService {
        private static readonly HttpClient client;

        // Fast in-memory cache to make typing/backspacing instant and avoid redundant network calls
        private static readonly ConcurrentDictionary<string, TranslationResult> cache = new(StringComparer.OrdinalIgnoreCase);
        private const int MaxCacheSize = 300;

        static TranslationService() {
            var handler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            client = new HttpClient(handler) {
                Timeout = TimeSpan.FromSeconds(4)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        }

        public static bool ContainsArabic(string text) {
            if (string.IsNullOrEmpty(text)) return false;
            return Regex.IsMatch(text, @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFE]");
        }

        public static async Task<TranslationResult> TranslateToArabicAsync(string text) {
            return await TranslateAsync(text, targetLang: "ar", sourceLang: "auto");
        }

        public static async Task<TranslationResult> TranslateAsync(string text, string targetLang = "en", string sourceLang = "auto") {
            var result = new TranslationResult {
                OriginalText = text,
                TargetLanguage = targetLang.ToUpperInvariant()
            };

            if (string.IsNullOrWhiteSpace(text)) {
                result.ErrorMessage = "لا يوجد نص محدد للترجمة.";
                return result;
            }

            string cacheKey = $"{sourceLang}_{targetLang}_{text.Trim()}";
            if (cache.TryGetValue(cacheKey, out var cachedResult)) {
                return new TranslationResult {
                    OriginalText = text,
                    TranslatedText = cachedResult.TranslatedText,
                    SourceLanguage = cachedResult.SourceLanguage,
                    TargetLanguage = cachedResult.TargetLanguage,
                    IsSuccess = cachedResult.IsSuccess,
                    ErrorMessage = cachedResult.ErrorMessage
                };
            }

            // -----------------------------------------------------------------
            // Engine 1: Google Dict Client (clients5.google.com)
            // -----------------------------------------------------------------
            try {
                string url = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex&sl={Uri.EscapeDataString(sourceLang)}&tl={Uri.EscapeDataString(targetLang)}&q={Uri.EscapeDataString(text)}";
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    string? parsedText = ExtractTextFromArray(root, out string? detectedLang);
                    if (!string.IsNullOrWhiteSpace(parsedText)) {
                        result.TranslatedText = WebUtility.HtmlDecode(parsedText.Trim());
                        result.SourceLanguage = !string.IsNullOrEmpty(detectedLang) ? detectedLang.ToUpperInvariant() : sourceLang.ToUpperInvariant();
                        result.IsSuccess = true;
                        SaveToCache(cacheKey, result);
                        return result;
                    }
                }
            } catch {
                // Failover to next engine
            }

            // -----------------------------------------------------------------
            // Engine 2: Google Free Web Single API (translate.googleapis.com)
            // -----------------------------------------------------------------
            try {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={Uri.EscapeDataString(sourceLang)}&tl={Uri.EscapeDataString(targetLang)}&dt=t&q={Uri.EscapeDataString(text)}";
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) {
                        var sentences = root[0];
                        string fullTranslation = "";
                        if (sentences.ValueKind == JsonValueKind.Array) {
                            foreach (var item in sentences.EnumerateArray()) {
                                if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0 && item[0].ValueKind == JsonValueKind.String) {
                                    fullTranslation += item[0].GetString();
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(fullTranslation)) {
                            result.TranslatedText = WebUtility.HtmlDecode(fullTranslation.Trim());
                            if (root.GetArrayLength() > 2 && root[2].ValueKind == JsonValueKind.String) {
                                string? detected = root[2].GetString();
                                if (!string.IsNullOrEmpty(detected)) {
                                    result.SourceLanguage = detected.ToUpperInvariant();
                                }
                            }
                            result.IsSuccess = true;
                            SaveToCache(cacheKey, result);
                            return result;
                        }
                    }
                }
            } catch {
                // Failover to next engine
            }

            // -----------------------------------------------------------------
            // Engine 3: MyMemory Translation API (Independent Fallback)
            // -----------------------------------------------------------------
            try {
                string src = sourceLang == "auto" ? (ContainsArabic(text) ? "ar" : "en") : sourceLang;
                string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={Uri.EscapeDataString(src)}|{Uri.EscapeDataString(targetLang)}";
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("responseData", out var respData) &&
                        respData.TryGetProperty("translatedText", out var transProp)) {
                        string? translated = transProp.GetString();
                        if (!string.IsNullOrWhiteSpace(translated) && !translated.StartsWith("MYMEMORY WARNING")) {
                            result.TranslatedText = WebUtility.HtmlDecode(translated.Trim());
                            result.SourceLanguage = src.ToUpperInvariant();
                            result.IsSuccess = true;
                            SaveToCache(cacheKey, result);
                            return result;
                        }
                    }
                }
            } catch {
                // Failover
            }

            result.ErrorMessage = "تعذر الاتصال بخدمات الترجمة. يرجى التحقق من اتصال الإنترنت.";
            result.TranslatedText = text; // fallback
            return result;
        }

        private static string? ExtractTextFromArray(JsonElement element, out string? detectedLang) {
            detectedLang = null;
            if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() == 0) {
                return null;
            }

            // Case A: ["translated text", "detected_lang"]
            if (element[0].ValueKind == JsonValueKind.String) {
                if (element.GetArrayLength() > 1 && element[1].ValueKind == JsonValueKind.String) {
                    detectedLang = element[1].GetString();
                }
                return element[0].GetString();
            }

            // Case B: [["translated text", "detected_lang"]]
            if (element[0].ValueKind == JsonValueKind.Array && element[0].GetArrayLength() > 0) {
                var inner = element[0];
                if (inner[0].ValueKind == JsonValueKind.String) {
                    if (inner.GetArrayLength() > 1 && inner[1].ValueKind == JsonValueKind.String) {
                        detectedLang = inner[1].GetString();
                    }
                    return inner[0].GetString();
                }

                // Case C: [[["translated text", ...]]]
                if (inner[0].ValueKind == JsonValueKind.Array && inner[0].GetArrayLength() > 0 && inner[0][0].ValueKind == JsonValueKind.String) {
                    return inner[0][0].GetString();
                }
            }

            return null;
        }

        private static void SaveToCache(string key, TranslationResult result) {
            if (cache.Count >= MaxCacheSize) {
                cache.Clear();
            }
            cache[key] = result;
        }
    }
}
