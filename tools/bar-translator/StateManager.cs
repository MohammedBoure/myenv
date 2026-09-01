using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarTranslator {
    public static class StateManager {
        private static readonly object fileLock = new();
        private static TranslationData currentData = new();
        private static HttpListener? httpListener;
        private static readonly int port = 49876;

        public static string StateFilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            @"Documents\myenv\scripts\bar-translator\state.json"
        );

        public static void Initialize() {
            // Ensure directory exists
            string? dir = Path.GetDirectoryName(StateFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            // Load existing state if available
            LoadState();

            // Start HTTP listener for Zebar and local utilities
            StartHttpServer();
        }

        public static void UpdateState(TranslationData data) {
            lock (fileLock) {
                currentData = data;
                SaveToFile(data);
            }
        }

        public static void ClearState() {
            lock (fileLock) {
                currentData = new TranslationData {
                    HasData = false,
                    Short = "",
                    Full = "",
                    Original = "",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                SaveToFile(currentData);
            }
        }

        public static string GetCurrentJson() {
            lock (fileLock) {
                if (!currentData.HasData) {
                    return "{}";
                }
                return JsonSerializer.Serialize(new {
                    @short = currentData.Short,
                    full = currentData.Full,
                    original = currentData.Original,
                    has_data = currentData.HasData,
                    timestamp = currentData.Timestamp
                }, new JsonSerializerOptions {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
        }

        public static void CopyCurrentToClipboard() {
            string textToCopy;
            lock (fileLock) {
                textToCopy = currentData.Full;
            }

            if (!string.IsNullOrWhiteSpace(textToCopy)) {
                Thread t = new Thread(() => {
                    try {
                        Clipboard.SetText(textToCopy);
                    } catch {}
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join(500);
            }
        }

        private static void SaveToFile(TranslationData data) {
            try {
                string json;
                if (!data.HasData) {
                    json = "{}";
                } else {
                    json = JsonSerializer.Serialize(new {
                        @short = data.Short,
                        full = data.Full,
                        original = data.Original,
                        has_data = data.HasData,
                        timestamp = data.Timestamp
                    }, new JsonSerializerOptions {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                }

                string tmpFile = StateFilePath + ".tmp";
                File.WriteAllText(tmpFile, json, Encoding.UTF8);
                File.Copy(tmpFile, StateFilePath, true);
                try { File.Delete(tmpFile); } catch {}
            } catch {}
        }

        private static void LoadState() {
            try {
                if (File.Exists(StateFilePath)) {
                    string json = File.ReadAllText(StateFilePath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "{}") {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        currentData = new TranslationData {
                            Short = root.TryGetProperty("short", out var s) ? s.GetString() ?? "" : "",
                            Full = root.TryGetProperty("full", out var f) ? f.GetString() ?? "" : "",
                            Original = root.TryGetProperty("original", out var o) ? o.GetString() ?? "" : "",
                            HasData = root.TryGetProperty("has_data", out var h) && h.GetBoolean(),
                            Timestamp = root.TryGetProperty("timestamp", out var t) ? t.GetInt64() : 0
                        };
                    }
                }
            } catch {}
        }

        private static void StartHttpServer() {
            Task.Run(async () => {
                try {
                    httpListener = new HttpListener();
                    httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
                    httpListener.Prefixes.Add($"http://localhost:{port}/");
                    httpListener.Start();

                    while (httpListener.IsListening) {
                        try {
                            var context = await httpListener.GetContextAsync();
                            _ = ProcessHttpRequestAsync(context);
                        } catch {
                            if (httpListener == null || !httpListener.IsListening) break;
                        }
                    }
                } catch {
                    // Port might be in use or access denied; continue silently
                }
            });
        }

        private static async Task ProcessHttpRequestAsync(HttpListenerContext context) {
            var request = context.Request;
            var response = context.Response;

            // Enable CORS for Zebar WebView
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase)) {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            string path = request.Url?.AbsolutePath.ToLowerInvariant() ?? "/";
            byte[] responseBytes = Array.Empty<byte>();

            if (path == "/" || path == "/state") {
                response.ContentType = "application/json; charset=utf-8";
                string json = GetCurrentJson();
                responseBytes = Encoding.UTF8.GetBytes(json);
            } else if (path == "/clear") {
                ClearState();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes("{\"cleared\":true}");
            } else if (path == "/copy") {
                CopyCurrentToClipboard();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes("{\"copied\":true}");
            } else if (path == "/translate") {
                string? text = request.QueryString["text"];
                if (!string.IsNullOrWhiteSpace(text)) {
                    var data = await TranslationEngine.TranslateToEnglishArabicAsync(text);
                    if (data != null) {
                        UpdateState(data);
                        response.ContentType = "application/json; charset=utf-8";
                        responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
                    } else {
                        response.StatusCode = 400;
                        responseBytes = Encoding.UTF8.GetBytes("{\"error\":\"translation_failed\"}");
                    }
                } else {
                    response.StatusCode = 400;
                    responseBytes = Encoding.UTF8.GetBytes("{\"error\":\"missing_text\"}");
                }
            } else {
                response.StatusCode = 404;
                responseBytes = Encoding.UTF8.GetBytes("{\"error\":\"not_found\"}");
            }

            response.ContentLength64 = responseBytes.Length;
            try {
                await response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            } catch {} finally {
                try { response.Close(); } catch {}
            }
        }
    }
}
