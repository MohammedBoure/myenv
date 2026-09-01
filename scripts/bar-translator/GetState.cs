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
                Console.Write("{\"short\":\"ترجمة فورية\",\"full\":\"حدد أي نص بالإنجليزية في أي مكان ليتم ترجمته هنا تلقائياً\",\"original\":\"Select English text anywhere to translate\",\"has_data\":false}");
            } catch {
                Console.Write("{\"short\":\"ترجمة فورية\",\"full\":\"حدد أي نص بالإنجليزية في أي مكان ليتم ترجمته هنا تلقائياً\",\"original\":\"Select English text anywhere to translate\",\"has_data\":false}");
            }
        }
    }
}
