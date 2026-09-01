using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BarTranslator {
    class Actions {
        [STAThread]
        static void Main(string[] args) {
            if (args.Length == 0) return;
            string action = args[0].ToLowerInvariant();
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string stateFile = Path.Combine(dir, "state.json");

            if (action == "clear") {
                try {
                    File.WriteAllText(stateFile, "{}", Encoding.UTF8);
                } catch {}
            } else if (action == "copy") {
                try {
                    if (File.Exists(stateFile)) {
                        string json = File.ReadAllText(stateFile, Encoding.UTF8);
                        var match = Regex.Match(json, "\"full\"\\s*:\\s*\"([^\"]+)\"");
                        if (match.Success) {
                            string fullText = Regex.Unescape(match.Groups[1].Value);
                            if (!string.IsNullOrWhiteSpace(fullText)) {
                                Clipboard.SetText(fullText);
                            }
                        }
                    }
                } catch {}
            }
        }
    }
}
