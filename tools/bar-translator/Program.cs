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
        public static void Main(string[] args) {
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

                if (command is "--toggle-auto-capture" or "-tac") {
                    StateManager.ToggleAutoCapture();
                    Console.WriteLine(StateManager.GetCurrentJson());
                    return;
                }

                if (command is "--toggle-translation-mode" or "-ttm") {
                    StateManager.ToggleTranslationMode();
                    Console.WriteLine(StateManager.GetCurrentJson());
                    return;
                }

                if (command is "--toggle-show-english" or "-tse") {
                    StateManager.ToggleShowEnglish();
                    Console.WriteLine(StateManager.GetCurrentJson());
                    return;
                }

                if (command is "--menu" or "-m") {
                    ShowSettingsMenu();
                    return;
                }

                if (command is "--translate" or "-t" or "translate") {
                    string query = string.Join(" ", args.Skip(1));
                    if (!string.IsNullOrWhiteSpace(query)) {
                        try {
                            var result = TranslationEngine.TranslateToEnglishArabicAsync(query).GetAwaiter().GetResult();
                            if (result != null) {
                                StateManager.UpdateState(result);
                                Console.WriteLine(StateManager.GetCurrentJson());
                                return;
                            }
                        } catch {}
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

        private static void ShowSettingsMenu() {
            var menu = new ContextMenuStrip();
            
            var itemAuto = new ToolStripMenuItem($"Auto-Select Capture: {(StateManager.AutoCaptureEnabled ? "ON" : "OFF")}") {
                Checked = StateManager.AutoCaptureEnabled
            };
            itemAuto.Click += (s, e) => StateManager.ToggleAutoCapture();
            menu.Items.Add(itemAuto);

            var itemMode = new ToolStripMenuItem($"Translation Focus Mode: {(StateManager.TranslationMode ? "ON" : "OFF")}") {
                Checked = StateManager.TranslationMode
            };
            itemMode.Click += (s, e) => StateManager.ToggleTranslationMode();
            menu.Items.Add(itemMode);

            var itemEn = new ToolStripMenuItem($"Show English Text: {(StateManager.ShowEnglish ? "ON" : "OFF")}") {
                Checked = StateManager.ShowEnglish
            };
            itemEn.Click += (s, e) => StateManager.ToggleShowEnglish();
            menu.Items.Add(itemEn);

            menu.Items.Add(new ToolStripSeparator());

            var itemCopy = new ToolStripMenuItem("Copy Translation");
            itemCopy.Click += (s, e) => StateManager.CopyCurrentToClipboard();
            menu.Items.Add(itemCopy);

            var itemClear = new ToolStripMenuItem("Clear");
            itemClear.Click += (s, e) => StateManager.ClearState();
            menu.Items.Add(itemClear);

            menu.Closed += (s, e) => Application.ExitThread();

            menu.Show(Cursor.Position);
            Application.Run();
        }
    }
}
