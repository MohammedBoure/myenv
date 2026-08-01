using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuickTranslate {
    public class TranslationResult {
        public string OriginalText { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = "AUTO";
        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class TranslationService {
        private static readonly HttpClient client = new HttpClient {
            Timeout = TimeSpan.FromSeconds(5)
        };

        public static async Task<TranslationResult> TranslateToArabicAsync(string text) {
            var result = new TranslationResult {
                OriginalText = text
            };

            if (string.IsNullOrWhiteSpace(text)) {
                result.ErrorMessage = "لا يوجد نص محدد للترجمة.";
                return result;
            }

            try {
                // Endpoint for Google Translate free API
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ar&dt=t&q={Uri.EscapeDataString(text)}";
                
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                    var root = doc.RootElement;
                    
                    // Parse translation chunks from json array root[0]
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

                        // Try to read detected source language from root[2]
                        if (root.GetArrayLength() > 2) {
                            string? detectedLang = root[2].GetString();
                            if (!string.IsNullOrEmpty(detectedLang)) {
                                result.SourceLanguage = detectedLang.ToUpper();
                            }
                        }

                        result.TranslatedText = fullTranslation.Trim();
                        result.IsSuccess = true;
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
