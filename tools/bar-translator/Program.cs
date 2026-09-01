using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BarTranslator {
    public static class Program {
        private const string MutexName = @"Global\MyEnv_BarTranslator_Daemon";
        private static Mutex? singleInstanceMutex;

        [STAThread]
        public static async Task Main(string[] args) {
            Console.OutputEncoding = Encoding.UTF8;
            StateManager.Initialize();

            // Handle CLI flags
            if (args.Length > 0) {
                string command = args[0].ToLowerInvariant();

                if (command is "--get" or "-g" or "get") {
                    string json = StateManager.GetCurrentJson();
                    Console.WriteLine(json);
                    return;
                }

                if (command is "--clear" or "-c" or "clear") {
                    StateManager.ClearState();
                    Console.WriteLine("{\"cleared\":true}");
                    return;
                }

                if (command is "--copy" or "copy") {
                    StateManager.CopyCurrentToClipboard();
                    Console.WriteLine("{\"copied\":true}");
                    return;
                }

                if (command is "--translate" or "-t" or "translate") {
                    string query = string.Join(" ", args.Skip(1));
                    if (!string.IsNullOrWhiteSpace(query)) {
                        var result = await TranslationEngine.TranslateToEnglishArabicAsync(query);
                        if (result != null) {
                            StateManager.UpdateState(result);
                            Console.WriteLine(StateManager.GetCurrentJson());
                            return;
                        }
                    }
                    Console.WriteLine("{}");
                    return;
                }
            }

            // Daemon mode: ensure single instance
            singleInstanceMutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew) {
                // Already running
                return;
            }

            try {
                SelectionMonitor.Start();

                // Run application message loop to receive Windows hooks and messages
                Application.Run();
            } finally {
                SelectionMonitor.Stop();
                singleInstanceMutex?.ReleaseMutex();
            }
        }
    }
}
