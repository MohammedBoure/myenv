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

                if (!File.Exists(stateFile)) {
                    Console.Write("{}");
                    return;
                }

                string content;
                using (var fs = new FileStream(stateFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                    content = sr.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(content) || !content.Contains("\"has_data\": true")) {
                    Console.Write("{}");
                } else {
                    Console.Write(content);
                }
            } catch {
                Console.Write("{}");
            }
        }
    }
}
