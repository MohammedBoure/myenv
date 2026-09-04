using System;
using System.IO;
using System.Text;

namespace BarTranslator {
    class Program {
        static void Main() {
            try {
                Console.OutputEncoding = Encoding.UTF8;
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                string stateFile = Path.Combine(dir, "state.json");

                if (File.Exists(stateFile)) {
                    string content;
                    using (var fs = new FileStream(stateFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                        content = sr.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(content) && content.Trim().StartsWith("{") && content.Trim().EndsWith("}")) {
                        Console.Write(content);
                        return;
                    }
                }

                // Default idle state fallback
                Console.Write("{\"short\":\"العربية\",\"full\":\"حدد أو انسخ أي نص بالإنجليزية ليتم ترجمته فوراً\",\"original\":\"English\",\"original_short\":\"English\",\"english\":\"English\",\"english_short\":\"English\",\"arabic\":\"العربية\",\"arabic_short\":\"العربية\",\"display_short\":\"English ➔ العربية\",\"display_full\":\"English ➔ العربية\",\"has_data\":false,\"timestamp\":0,\"auto_capture\":true,\"clipboard_translate\":true,\"translation_mode\":false,\"show_english\":true}");
            } catch {
                Console.Write("{\"short\":\"العربية\",\"full\":\"حدد أو انسخ أي نص بالإنجليزية ليتم ترجمته فوراً\",\"original\":\"English\",\"original_short\":\"English\",\"english\":\"English\",\"english_short\":\"English\",\"arabic\":\"العربية\",\"arabic_short\":\"العربية\",\"display_short\":\"English ➔ العربية\",\"display_full\":\"English ➔ العربية\",\"has_data\":false,\"timestamp\":0,\"auto_capture\":true,\"clipboard_translate\":true,\"translation_mode\":false,\"show_english\":true}");
            }
        }
    }
}
