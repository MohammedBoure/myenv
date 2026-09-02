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

        public static bool AutoCaptureEnabled { get; set; } = true;
        public static bool TranslationMode { get; set; } = false;
        public static bool ShowEnglish { get; set; } = true;

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

        public static void ToggleAutoCapture() {
            lock (fileLock) {
                AutoCaptureEnabled = !AutoCaptureEnabled;
                SaveToFile(currentData);
            }
        }

        public static void ToggleTranslationMode() {
            lock (fileLock) {
                TranslationMode = !TranslationMode;
                SaveToFile(currentData);
            }
        }

        public static void ToggleShowEnglish() {
            lock (fileLock) {
                ShowEnglish = !ShowEnglish;
                if (currentData.HasData) {
                    currentData.DisplayShort = ShowEnglish ? $"{currentData.OriginalShort} ➔ {currentData.Short}" : currentData.Short;
                    currentData.DisplayFull = ShowEnglish ? $"{currentData.Original} ➔ {currentData.Full}" : currentData.Full;
                }
                SaveToFile(currentData);
            }
        }

        public static string GetCurrentJson() {
            lock (fileLock) {
                return JsonSerializer.Serialize(new {
                    @short = currentData.Short,
                    full = currentData.Full,
                    original = currentData.Original,
                    original_short = currentData.OriginalShort,
                    display_short = currentData.DisplayShort,
                    display_full = currentData.DisplayFull,
                    has_data = currentData.HasData,
                    timestamp = currentData.Timestamp,
                    auto_capture = AutoCaptureEnabled,
                    translation_mode = TranslationMode,
                    show_english = ShowEnglish
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
                var payload = new {
                    @short = data.Short,
                    full = data.Full,
                    original = data.Original,
                    original_short = data.OriginalShort,
                    display_short = data.DisplayShort,
                    display_full = data.DisplayFull,
                    has_data = data.HasData,
                    timestamp = data.Timestamp,
                    auto_capture = AutoCaptureEnabled,
                    translation_mode = TranslationMode,
                    show_english = ShowEnglish
                };

                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

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
                            OriginalShort = root.TryGetProperty("original_short", out var os) ? os.GetString() ?? "" : "",
                            DisplayShort = root.TryGetProperty("display_short", out var ds) ? ds.GetString() ?? "" : "",
                            DisplayFull = root.TryGetProperty("display_full", out var df) ? df.GetString() ?? "" : "",
                            HasData = root.TryGetProperty("has_data", out var h) && h.GetBoolean(),
                            Timestamp = root.TryGetProperty("timestamp", out var t) ? t.GetInt64() : 0
                        };

                        if (root.TryGetProperty("auto_capture", out var ac)) {
                            AutoCaptureEnabled = ac.GetBoolean();
                        }
                        if (root.TryGetProperty("translation_mode", out var tm)) {
                            TranslationMode = tm.GetBoolean();
                        }
                        if (root.TryGetProperty("show_english", out var se)) {
                            ShowEnglish = se.GetBoolean();
                        }
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
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
            } else if (path == "/copy") {
                CopyCurrentToClipboard();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes("{\"copied\":true}");
            } else if (path == "/toggle_auto_capture") {
                ToggleAutoCapture();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
            } else if (path == "/toggle_translation_mode") {
                ToggleTranslationMode();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
            } else if (path == "/toggle_show_english") {
                ToggleShowEnglish();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
            } else if (path == "/get_widgets") {
                var enabled = BarConfigManager.GetEnabledWidgets();
                var list = BarConfigManager.AllWidgets.Select(w => new {
                    id = w.Id,
                    name = w.Name,
                    section = w.Section,
                    enabled = enabled.Contains(w.Id)
                });
                string json = JsonSerializer.Serialize(list);
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(json);
            } else if (path == "/toggle_widget") {
                string? name = request.QueryString["name"]?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(name)) {
                    bool state = BarConfigManager.ToggleWidget(name);
                    response.ContentType = "application/json; charset=utf-8";
                    responseBytes = Encoding.UTF8.GetBytes($"{{\"widget\":\"{name}\",\"enabled\":{state.ToString().ToLowerInvariant()}}}");
                } else {
                    response.StatusCode = 400;
                    responseBytes = Encoding.UTF8.GetBytes("{\"error\":\"missing_widget_name\"}");
                }
            } else if (path == "/set_widget") {
                string? name = request.QueryString["name"]?.ToLowerInvariant();
                string? valStr = request.QueryString["visible"]?.ToLowerInvariant();
                bool val = valStr == "true" || valStr == "1";
                if (!string.IsNullOrEmpty(name)) {
                    BarConfigManager.SetWidgetEnabled(name, val);
                    response.ContentType = "application/json; charset=utf-8";
                    responseBytes = Encoding.UTF8.GetBytes($"{{\"widget\":\"{name}\",\"enabled\":{val.ToString().ToLowerInvariant()}}}");
                } else {
                    response.StatusCode = 400;
                    responseBytes = Encoding.UTF8.GetBytes("{\"error\":\"missing_widget_name\"}");
                }
            } else if (path == "/set_setting") {
                string? key = request.QueryString["key"]?.ToLowerInvariant();
                string? valStr = request.QueryString["value"]?.ToLowerInvariant();
                bool val = valStr == "true" || valStr == "1";

                lock (fileLock) {
                    if (key == "auto_capture") AutoCaptureEnabled = val;
                    else if (key == "translation_mode") TranslationMode = val;
                    else if (key == "show_english") ShowEnglish = val;
                    SaveToFile(currentData);
                }
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
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
