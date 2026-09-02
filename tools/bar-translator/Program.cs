using System;
using System.Drawing;
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

                if (command is "--toggle-clipboard-translate" or "-tct") {
                    StateManager.ToggleClipboardTranslate();
                    Console.WriteLine(StateManager.GetCurrentJson());
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

                if (command is "--toggle-widget" or "-tw") {
                    if (args.Length > 1) {
                        string wid = args[1];
                        bool state = BarConfigManager.ToggleWidget(wid);
                        Console.WriteLine($"{{\"widget\":\"{wid}\",\"enabled\":{state.ToString().ToLowerInvariant()}}}");
                        return;
                    }
                }

                if (command is "--show-all-widgets") {
                    BarConfigManager.SetAllWidgets(true);
                    Console.WriteLine("{\"all_widgets_enabled\":true}");
                    return;
                }

                if (command is "--minimal-widgets") {
                    BarConfigManager.SetAllWidgets(false);
                    Console.WriteLine("{\"minimal_widgets_enabled\":true}");
                    return;
                }

                if (command is "--list-widgets") {
                    var enabled = BarConfigManager.GetEnabledWidgets();
                    foreach (var w in BarConfigManager.AllWidgets) {
                        Console.WriteLine($"[{ (enabled.Contains(w.Id) ? "X" : " ") }] {w.Id} - {w.Name} ({w.Section})");
                    }
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
            var menu = new ContextMenuStrip {
                Renderer = new DarkMenuRenderer(),
                ShowImageMargin = true,
                ShowCheckMargin = false,
                Font = new Font("Segoe UI Variable Text", 9.5f, FontStyle.Regular),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(240, 240, 240)
            };

            // -------------------------------------------------------------
            // Section 1: Translation Controls
            // -------------------------------------------------------------
            var headerTrans = new ToolStripMenuItem("🌐  Translation & Language Settings") {
                Enabled = false,
                ForeColor = Color.FromArgb(88, 166, 255),
                Font = new Font("Segoe UI Variable Text", 9.5f, FontStyle.Bold)
            };
            menu.Items.Add(headerTrans);

            // 1. Auto-Translate Copied Text
            var itemClip = new ToolStripMenuItem($"Auto-Translate Copied Text: {(StateManager.ClipboardTranslateEnabled ? "ON" : "OFF")}") {
                Checked = StateManager.ClipboardTranslateEnabled
            };
            itemClip.Click += (s, e) => {
                StateManager.ToggleClipboardTranslate();
                itemClip.Checked = StateManager.ClipboardTranslateEnabled;
                itemClip.Text = $"Auto-Translate Copied Text: {(StateManager.ClipboardTranslateEnabled ? "ON" : "OFF")}";
            };
            menu.Items.Add(itemClip);

            // 2. Auto-Select Text Capture (Mouse Selection)
            var itemAuto = new ToolStripMenuItem($"Auto-Select Text Capture: {(StateManager.AutoCaptureEnabled ? "ON" : "OFF")}") {
                Checked = StateManager.AutoCaptureEnabled
            };
            itemAuto.Click += (s, e) => {
                StateManager.ToggleAutoCapture();
                itemAuto.Checked = StateManager.AutoCaptureEnabled;
                itemAuto.Text = $"Auto-Select Text Capture: {(StateManager.AutoCaptureEnabled ? "ON" : "OFF")}";
            };
            menu.Items.Add(itemAuto);

            // 3. Translation Focus Mode
            var itemMode = new ToolStripMenuItem($"Translation Focus Mode: {(StateManager.TranslationMode ? "ON" : "OFF")}") {
                Checked = StateManager.TranslationMode
            };
            itemMode.Click += (s, e) => {
                StateManager.ToggleTranslationMode();
                itemMode.Checked = StateManager.TranslationMode;
                itemMode.Text = $"Translation Focus Mode: {(StateManager.TranslationMode ? "ON" : "OFF")}";
            };
            menu.Items.Add(itemMode);

            // 4. Show English Text
            var itemEn = new ToolStripMenuItem($"Show English Text: {(StateManager.ShowEnglish ? "ON" : "OFF")}") {
                Checked = StateManager.ShowEnglish
            };
            itemEn.Click += (s, e) => {
                StateManager.ToggleShowEnglish();
                itemEn.Checked = StateManager.ShowEnglish;
                itemEn.Text = $"Show English Text: {(StateManager.ShowEnglish ? "ON" : "OFF")}";
            };
            menu.Items.Add(itemEn);

            var itemCopy = new ToolStripMenuItem("📋  Copy Arabic Translation");
            itemCopy.Click += (s, e) => StateManager.CopyCurrentToClipboard();
            menu.Items.Add(itemCopy);

            var itemClear = new ToolStripMenuItem("🧹  Clear Translation");
            itemClear.Click += (s, e) => StateManager.ClearState();
            menu.Items.Add(itemClear);

            menu.Items.Add(new ToolStripSeparator());

            // -------------------------------------------------------------
            // Section 2: Dynamic Bar Containers & Widgets Visibility
            // -------------------------------------------------------------
            var headerWidgets = new ToolStripMenuItem("📊  Bar Containers & Widgets Visibility") {
                Enabled = false,
                ForeColor = Color.FromArgb(88, 166, 255),
                Font = new Font("Segoe UI Variable Text", 9.5f, FontStyle.Bold)
            };
            menu.Items.Add(headerWidgets);

            var enabledWidgets = BarConfigManager.GetEnabledWidgets();

            foreach (var widget in BarConfigManager.AllWidgets) {
                bool isEnabled = enabledWidgets.Contains(widget.Id);
                var itemWidget = new ToolStripMenuItem($"{widget.Icon}  {widget.Name}") {
                    Checked = isEnabled
                };
                string wid = widget.Id;
                itemWidget.Click += (s, e) => {
                    bool newState = BarConfigManager.ToggleWidget(wid);
                    itemWidget.Checked = newState;
                };
                menu.Items.Add(itemWidget);
            }

            menu.Items.Add(new ToolStripSeparator());

            // Presets
            var itemShowAll = new ToolStripMenuItem("👁️  Show All Containers");
            itemShowAll.Click += (s, e) => BarConfigManager.SetAllWidgets(true);
            menu.Items.Add(itemShowAll);

            var itemMinimal = new ToolStripMenuItem("🕶️  Minimalist Preset (Workspaces & Clock)");
            itemMinimal.Click += (s, e) => BarConfigManager.SetAllWidgets(false);
            menu.Items.Add(itemMinimal);

            menu.Closed += (s, e) => Application.ExitThread();

            // Display at current cursor position
            menu.Show(Cursor.Position);
            Application.Run();
        }
    }

    public class DarkMenuRenderer : ToolStripProfessionalRenderer {
        public DarkMenuRenderer() : base(new DarkColorTable()) {}

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if (e.Item.Selected || e.Item.Pressed) {
                using var brush = new SolidBrush(Color.FromArgb(40, 40, 40));
                e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                using var pen = new Pen(Color.FromArgb(88, 166, 255), 1);
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1));
            } else {
                using var brush = new SolidBrush(Color.FromArgb(18, 18, 18));
                e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
            var rect = new Rectangle(e.ImageRectangle.X, e.ImageRectangle.Y, 16, 16);
            using var bgBrush = new SolidBrush(Color.FromArgb(28, 28, 28));
            e.Graphics.FillRectangle(bgBrush, rect);
            using var borderPen = new Pen(Color.FromArgb(88, 166, 255), 1);
            e.Graphics.DrawRectangle(borderPen, rect);

            using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(88, 166, 255));
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("✓", font, textBrush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), format);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            using var pen = new Pen(Color.FromArgb(42, 42, 42), 1);
            int y = e.Item.Height / 2;
            e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
        }
    }

    public class DarkColorTable : ProfessionalColorTable {
        public override Color ToolStripDropDownBackground => Color.FromArgb(18, 18, 18);
        public override Color MenuBorder => Color.FromArgb(45, 45, 45);
        public override Color MenuItemBorder => Color.FromArgb(88, 166, 255);
        public override Color MenuItemSelected => Color.FromArgb(40, 40, 40);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(40, 40, 40);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(40, 40, 40);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(32, 32, 32);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(32, 32, 32);
        public override Color CheckBackground => Color.FromArgb(28, 28, 28);
        public override Color CheckSelectedBackground => Color.FromArgb(40, 40, 40);
        public override Color CheckPressedBackground => Color.FromArgb(32, 32, 32);
        public override Color ImageMarginGradientBegin => Color.FromArgb(18, 18, 18);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(18, 18, 18);
        public override Color ImageMarginGradientEnd => Color.FromArgb(18, 18, 18);
        public override Color SeparatorDark => Color.FromArgb(42, 42, 42);
        public override Color SeparatorLight => Color.FromArgb(24, 24, 24);
    }
}
