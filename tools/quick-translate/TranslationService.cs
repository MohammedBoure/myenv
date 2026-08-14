using System;
using System.Collections.Concurrent;
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
        private static readonly HttpClient client = new HttpClient {
            Timeout = TimeSpan.FromSeconds(5)
        };

        // Fast in-memory cache to make typing/backspacing instant and avoid redundant network calls
        private static readonly ConcurrentDictionary<string, TranslationResult> cache = new(StringComparer.OrdinalIgnoreCase);
        private const int MaxCacheSize = 250;

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

            try {
                // Free Google Translate single endpoint
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={Uri.EscapeDataString(sourceLang)}&tl={Uri.EscapeDataString(targetLang)}&dt=t&q={Uri.EscapeDataString(text)}";
                
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                    var root = doc.RootElement;
                    
                    // Parse translation sentences from json array root[0]
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) {
                        var sentences = root[0];
                        string fullTranslation = "";
                        
                        if (sentences.ValueKind == JsonValueKind.Array) {
                            foreach (var item in sentences.EnumerateArray()) {
                                if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0) {
                                    fullTranslation += item[0].GetString();
                                }
                            }
                        }

                        // Read detected source language from root[2]
                        if (root.GetArrayLength() > 2) {
                            string? detectedLang = root[2].GetString();
                            if (!string.IsNullOrEmpty(detectedLang)) {
                                result.SourceLanguage = detectedLang.ToUpperInvariant();
                            }
                        }

                        result.TranslatedText = fullTranslation.Trim();
                        result.IsSuccess = true;

                        // Cache result
                        if (cache.Count >= MaxCacheSize) {
                            cache.Clear();
                        }
                        cache[cacheKey] = result;

                        return result;
                    }
                }

                result.ErrorMessage = "فشل في فك تشفير استجابة الترجمة.";
            } catch (Exception ex) {
                result.ErrorMessage = $"خطأ في الاتصال بالترجمة: {ex.Message}";
                result.TranslatedText = text; // fallback to original text
            }

            return result;
        }
    }
}
