using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarTranslator {
    public static class StateManager {
        private static readonly object fileLock = new();
        private static TranslationData currentData = CreateDefaultData();
        private static HttpListener? httpListener;
        private static FileSystemWatcher? fileWatcher;
        private static readonly int port = 49876;
        private static volatile bool isInternalSaving = false;

        public static string StateFilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            @"Documents\myenv\scripts\bar-translator\state.json"
        );

        public static bool AutoCaptureEnabled { get; set; } = true;
        public static bool ClipboardTranslateEnabled { get; set; } = true;
        public static bool TranslationMode { get; set; } = false;
        public static bool ShowEnglish { get; set; } = true;

        private static TranslationData CreateDefaultData() {
            return new TranslationData {
                HasData = false,
                Short = "العربية",
                Full = "حدد أو انسخ أي نص بالإنجليزية ليتم ترجمته فوراً",
                Original = "English",
                OriginalShort = "English",
                English = "English",
                EnglishShort = "English",
                Arabic = "العربية",
                ArabicShort = "العربية",
                DisplayShort = ShowEnglish ? "English ➔ العربية" : "العربية",
                DisplayFull = ShowEnglish ? "English ➔ العربية" : "العربية",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public static void Initialize() {
            // Ensure directory exists
            string? dir = Path.GetDirectoryName(StateFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            // Load existing state if available
            LoadState();

            // Set up FileSystemWatcher so background daemon immediately reflects changes made by the menu process
            SetupFileWatcher();

            // Start HTTP listener for Zebar and local utilities
            StartHttpServer();
        }

        private static void SetupFileWatcher() {
            try {
                string? dir = Path.GetDirectoryName(StateFilePath);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) {
                    fileWatcher = new FileSystemWatcher(dir, Path.GetFileName(StateFilePath)) {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                        EnableRaisingEvents = true
                    };
                    fileWatcher.Changed += (s, e) => OnExternalStateFileModified();
                    fileWatcher.Created += (s, e) => OnExternalStateFileModified();
                }
            } catch {}
        }

        private static void OnExternalStateFileModified() {
            if (isInternalSaving) return;
            try {
                // Short sleep to allow file write to finish
                Thread.Sleep(30);
                LoadState();
            } catch {}
        }

        public static void UpdateState(TranslationData data) {
            lock (fileLock) {
                currentData = data;
                SaveToFile(data);
            }
        }

        public static void ClearState() {
            lock (fileLock) {
                currentData = CreateDefaultData();
                SaveToFile(currentData);
            }
            SelectionMonitor.ClearLastProcessed();
            NotifyDaemonReload();
        }

        public static void ToggleAutoCapture() {
            lock (fileLock) {
                AutoCaptureEnabled = !AutoCaptureEnabled;
                SaveToFile(currentData);
            }
            NotifyDaemonReload();
        }

        public static void ToggleClipboardTranslate() {
            lock (fileLock) {
                ClipboardTranslateEnabled = !ClipboardTranslateEnabled;
                SaveToFile(currentData);
            }
            NotifyDaemonReload();
        }

        public static void ToggleTranslationMode() {
            lock (fileLock) {
                TranslationMode = !TranslationMode;
                SaveToFile(currentData);
            }
            NotifyDaemonReload();
        }

        public static void ToggleShowEnglish() {
            lock (fileLock) {
                ShowEnglish = !ShowEnglish;
                if (currentData.HasData) {
                    string enShort = !string.IsNullOrEmpty(currentData.EnglishShort) ? currentData.EnglishShort : currentData.OriginalShort;
                    string arShort = !string.IsNullOrEmpty(currentData.ArabicShort) ? currentData.ArabicShort : currentData.Short;
                    string enFull = !string.IsNullOrEmpty(currentData.English) ? currentData.English : currentData.Original;
                    string arFull = !string.IsNullOrEmpty(currentData.Arabic) ? currentData.Arabic : currentData.Full;

                    currentData.DisplayShort = ShowEnglish ? $"{enShort} ➔ {arShort}" : arShort;
                    string balancedEn = TranslationEngine.TruncateAtWordBoundary(enFull, 42);
                    string balancedAr = TranslationEngine.TruncateAtWordBoundary(arFull, 42);
                    currentData.DisplayFull = ShowEnglish ? $"{balancedEn} ➔ {balancedAr}" : arFull;
                } else {
                    currentData.DisplayShort = ShowEnglish ? "English ➔ العربية" : "العربية";
                    currentData.DisplayFull = ShowEnglish ? "English ➔ العربية" : "العربية";
                }
                SaveToFile(currentData);
            }
            NotifyDaemonReload();
        }

        private static void NotifyDaemonReload() {
            // Send quick non-blocking request to daemon HTTP server to reload settings immediately
            Task.Run(async () => {
                try {
                    using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMilliseconds(200) };
                    await client.GetAsync($"http://127.0.0.1:{port}/reload_state");
                } catch {}
            });
        }

        public static string GetCurrentJson() {
            lock (fileLock) {
                return JsonSerializer.Serialize(new {
                    @short = currentData.Short,
                    full = currentData.Full,
                    original = currentData.Original,
                    original_short = currentData.OriginalShort,
                    english = currentData.English,
                    english_short = currentData.EnglishShort,
                    arabic = currentData.Arabic,
                    arabic_short = currentData.ArabicShort,
                    display_short = currentData.DisplayShort,
                    display_full = currentData.DisplayFull,
                    has_data = currentData.HasData,
                    timestamp = currentData.Timestamp,
                    auto_capture = AutoCaptureEnabled,
                    clipboard_translate = ClipboardTranslateEnabled,
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
                textToCopy = currentData.HasData ? currentData.Full : currentData.Short;
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
                isInternalSaving = true;
                var payload = new {
                    @short = data.Short,
                    full = data.Full,
                    original = data.Original,
                    original_short = data.OriginalShort,
                    english = data.English,
                    english_short = data.EnglishShort,
                    arabic = data.Arabic,
                    arabic_short = data.ArabicShort,
                    display_short = data.DisplayShort,
                    display_full = data.DisplayFull,
                    has_data = data.HasData,
                    timestamp = data.Timestamp,
                    auto_capture = AutoCaptureEnabled,
                    clipboard_translate = ClipboardTranslateEnabled,
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
            } catch {} finally {
                Task.Delay(50).ContinueWith(_ => { isInternalSaving = false; });
            }
        }

        public static void LoadState() {
            try {
                if (File.Exists(StateFilePath)) {
                    string json = File.ReadAllText(StateFilePath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "{}") {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        lock (fileLock) {
                            bool hasData = root.TryGetProperty("has_data", out var h) && h.GetBoolean();
                            string shortText = root.TryGetProperty("short", out var s) ? s.GetString() ?? "" : "";
                            string fullText = root.TryGetProperty("full", out var f) ? f.GetString() ?? "" : "";
                            string origText = root.TryGetProperty("original", out var o) ? o.GetString() ?? "" : "";
                            string origShort = root.TryGetProperty("original_short", out var os) ? os.GetString() ?? "" : "";
                            string enText = root.TryGetProperty("english", out var et) ? et.GetString() ?? "" : origText;
                            string enShort = root.TryGetProperty("english_short", out var es) ? es.GetString() ?? "" : origShort;
                            string arText = root.TryGetProperty("arabic", out var at) ? at.GetString() ?? "" : fullText;
                            string arShort = root.TryGetProperty("arabic_short", out var @as) ? @as.GetString() ?? "" : shortText;
                            string dispShort = root.TryGetProperty("display_short", out var ds) ? ds.GetString() ?? "" : "";
                            string dispFull = root.TryGetProperty("display_full", out var df) ? df.GetString() ?? "" : "";

                            if (root.TryGetProperty("auto_capture", out var ac)) {
                                AutoCaptureEnabled = ac.GetBoolean();
                            }
                            if (root.TryGetProperty("clipboard_translate", out var ct)) {
                                ClipboardTranslateEnabled = ct.GetBoolean();
                            }
                            if (root.TryGetProperty("translation_mode", out var tm)) {
                                TranslationMode = tm.GetBoolean();
                            }
                            if (root.TryGetProperty("show_english", out var se)) {
                                ShowEnglish = se.GetBoolean();
                            }

                            if (!hasData && string.IsNullOrWhiteSpace(dispShort)) {
                                dispShort = ShowEnglish ? "English ➔ العربية" : "العربية";
                                dispFull = ShowEnglish ? "English ➔ العربية" : "العربية";
                                shortText = "العربية";
                                fullText = "حدد أو انسخ أي نص بالإنجليزية ليتم ترجمته فوراً";
                                origText = "English";
                                origShort = "English";
                                enText = "English";
                                enShort = "English";
                                arText = "العربية";
                                arShort = "العربية";
                            }

                            currentData = new TranslationData {
                                Short = shortText,
                                Full = fullText,
                                Original = origText,
                                OriginalShort = origShort,
                                English = enText,
                                EnglishShort = enShort,
                                Arabic = arText,
                                ArabicShort = arShort,
                                DisplayShort = dispShort,
                                DisplayFull = dispFull,
                                HasData = hasData,
                                Timestamp = root.TryGetProperty("timestamp", out var t) ? t.GetInt64() : 0
                            };
                        }
                        return;
                    }
                }

                // If file doesn't exist, create it with defaults
                lock (fileLock) {
                    currentData = CreateDefaultData();
                    SaveToFile(currentData);
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
            } else if (path == "/reload_state") {
                LoadState();
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
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
            } else if (path == "/toggle_clipboard_translate") {
                ToggleClipboardTranslate();
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
                    else if (key == "clipboard_translate") ClipboardTranslateEnabled = val;
                    else if (key == "translation_mode") TranslationMode = val;
                    else if (key == "show_english") ShowEnglish = val;
                    SaveToFile(currentData);
                }
                response.ContentType = "application/json; charset=utf-8";
                responseBytes = Encoding.UTF8.GetBytes(GetCurrentJson());
            } else if (path == "/translate") {
                string? text = request.QueryString["text"];
                if (!string.IsNullOrWhiteSpace(text)) {
                    var data = await TranslationEngine.TranslateAsync(text);
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
