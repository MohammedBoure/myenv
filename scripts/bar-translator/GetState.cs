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

                    if (!string.IsNullOrWhiteSpace(content) && content.Contains("\"has_data\": true")) {
                        Console.Write(content);
                        return;
                    }
                }

                // Default idle state so the field is always visible on the bar
                Console.Write("{\"short\":\"العربية\",\"full\":\"حدد أي كلمة أو جملة بالإنجليزية ليتم ترجمتها هنا تلقائياً\",\"original\":\"English\",\"original_short\":\"English\",\"display_short\":\"English ➔ العربية\",\"display_full\":\"English ➔ العربية (حدد نصاً للترجمة)\",\"has_data\":false,\"timestamp\":0}");
            } catch {
                Console.Write("{\"short\":\"العربية\",\"full\":\"حدد أي كلمة أو جملة بالإنجليزية ليتم ترجمتها هنا تلقائياً\",\"original\":\"English\",\"original_short\":\"English\",\"display_short\":\"English ➔ العربية\",\"display_full\":\"English ➔ العربية (حدد نصاً للترجمة)\",\"has_data\":false,\"timestamp\":0}");
            }
        }
    }
}
