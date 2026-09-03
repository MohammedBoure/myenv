using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
            string port = "49876";

            if (action == "clear") {
                // 1. Try local HTTP API
                try {
                    var req = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + port + "/clear");
                    req.Timeout = 300;
                    using (var resp = req.GetResponse()) {
                        return;
                    }
                } catch {}

                // 2. Fallback to BarTranslator.exe --clear
                try {
                    string exe = Path.Combine(dir, "BarTranslator.exe");
                    if (File.Exists(exe)) {
                        var psi = new ProcessStartInfo {
                            FileName = exe,
                            Arguments = "--clear",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        var p = Process.Start(psi);
                        if (p != null) p.WaitForExit(1000);
                    }
                } catch {}
            } else if (action == "copy") {
                // 1. Try local HTTP API
                try {
                    var req = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + port + "/copy");
                    req.Timeout = 300;
                    using (var resp = req.GetResponse()) {
                        return;
                    }
                } catch {}

                // 2. Fallback: read state.json and copy full translation
                try {
                    string stateFile = Path.Combine(dir, "state.json");
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
