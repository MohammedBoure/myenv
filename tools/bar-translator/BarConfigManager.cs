using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BarTranslator {
    public class BarWidgetInfo {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Section { get; set; } = "right"; // "left", "center", "right"
        public int DefaultOrder { get; set; }
    }

    public static class BarConfigManager {
        public static string ConfigPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            @"Documents\myenv\yasb\config.yaml"
        );

        public static readonly List<BarWidgetInfo> AllWidgets = new() {
            new BarWidgetInfo { Id = "home", Name = "Home Menu (User Folders)", Icon = "🏠", Section = "left", DefaultOrder = 0 },
            new BarWidgetInfo { Id = "glazewm_workspaces", Name = "GlazeWM Workspaces", Icon = "🪟", Section = "left", DefaultOrder = 1 },
            new BarWidgetInfo { Id = "bar_menu", Name = "Settings Dropdown Button", Icon = "⚙️", Section = "left", DefaultOrder = 2 },
            new BarWidgetInfo { Id = "translator", Name = "Translator Widget", Icon = "🌐", Section = "center", DefaultOrder = 0 },
            new BarWidgetInfo { Id = "clock", Name = "Clock & Date", Icon = "🕒", Section = "center", DefaultOrder = 1 },
            new BarWidgetInfo { Id = "traffic", Name = "Network Traffic (Download/Upload)", Icon = "📶", Section = "right", DefaultOrder = 0 },
            new BarWidgetInfo { Id = "cpu", Name = "CPU Performance & Load", Icon = "💻", Section = "right", DefaultOrder = 1 },
            new BarWidgetInfo { Id = "gpu", Name = "GPU Performance & Temp", Icon = "🎮", Section = "right", DefaultOrder = 2 },
            new BarWidgetInfo { Id = "memory", Name = "RAM Memory Usage", Icon = "🧠", Section = "right", DefaultOrder = 3 },
            new BarWidgetInfo { Id = "volume", Name = "Audio Volume & Mixer", Icon = "🔊", Section = "right", DefaultOrder = 4 },
            new BarWidgetInfo { Id = "microphone", Name = "Microphone Status", Icon = "🎙️", Section = "right", DefaultOrder = 5 },
            new BarWidgetInfo { Id = "github", Name = "GitHub Notifications", Icon = "🐙", Section = "right", DefaultOrder = 6 },
            new BarWidgetInfo { Id = "notifications", Name = "System Notifications", Icon = "🔔", Section = "right", DefaultOrder = 7 },
            new BarWidgetInfo { Id = "power_menu", Name = "Power Menu (Shutdown/Restart)", Icon = "⏻", Section = "right", DefaultOrder = 8 }
        };

        public static HashSet<string> GetEnabledWidgets() {
            var enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try {
                if (!File.Exists(ConfigPath)) return enabled;
                string yaml = File.ReadAllText(ConfigPath, Encoding.UTF8);

                // Locate the widgets: section under primary-bar
                int barsIdx = yaml.IndexOf("bars:", StringComparison.OrdinalIgnoreCase);
                if (barsIdx < 0) return enabled;

                int primaryBarIdx = yaml.IndexOf("primary-bar:", barsIdx, StringComparison.OrdinalIgnoreCase);
                if (primaryBarIdx < 0) return enabled;

                int barWidgetsIdx = yaml.IndexOf("widgets:", primaryBarIdx, StringComparison.OrdinalIgnoreCase);
                if (barWidgetsIdx < 0) return enabled;

                // Stop at root widgets: definition (which starts at column 0)
                int rootWidgetsIdx = yaml.IndexOf("\nwidgets:", barWidgetsIdx, StringComparison.OrdinalIgnoreCase);
                string barWidgetsBlock = rootWidgetsIdx > 0 
                    ? yaml.Substring(barWidgetsIdx, rootWidgetsIdx - barWidgetsIdx) 
                    : yaml.Substring(barWidgetsIdx);

                var matches = Regex.Matches(barWidgetsBlock, @"^\s*-\s*([a-zA-Z0-9_\-]+)", RegexOptions.Multiline);
                foreach (Match match in matches) {
                    string wid = match.Groups[1].Value.Trim();
                    enabled.Add(wid);
                }
            } catch {}

            return enabled;
        }

        public static bool IsWidgetEnabled(string widgetId) {
            return GetEnabledWidgets().Contains(widgetId);
        }

        public static bool ToggleWidget(string widgetId) {
            bool current = IsWidgetEnabled(widgetId);
            SetWidgetEnabled(widgetId, !current);
            return !current;
        }

        public static void SetWidgetEnabled(string widgetId, bool enable) {
            var enabled = GetEnabledWidgets();
            if (enable) {
                enabled.Add(widgetId);
            } else {
                enabled.Remove(widgetId);
            }
            SaveEnabledWidgets(enabled);
        }

        public static void SetAllWidgets(bool enable) {
            var enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (enable) {
                foreach (var w in AllWidgets) {
                    enabled.Add(w.Id);
                }
            } else {
                // Minimalist mode: Keep Workspaces, Settings Menu Button, and Clock
                enabled.Add("glazewm_workspaces");
                enabled.Add("bar_menu");
                enabled.Add("clock");
            }
            SaveEnabledWidgets(enabled);
        }

        public static void SaveEnabledWidgets(HashSet<string> enabled) {
            try {
                if (!File.Exists(ConfigPath)) return;
                string yaml = File.ReadAllText(ConfigPath, Encoding.UTF8);

                var leftWidgets = AllWidgets
                    .Where(w => w.Section == "left" && enabled.Contains(w.Id))
                    .OrderBy(w => w.DefaultOrder)
                    .Select(w => $"      - {w.Id}")
                    .ToList();

                var centerWidgets = AllWidgets
                    .Where(w => w.Section == "center" && enabled.Contains(w.Id))
                    .OrderBy(w => w.DefaultOrder)
                    .Select(w => $"      - {w.Id}")
                    .ToList();

                var rightWidgets = AllWidgets
                    .Where(w => w.Section == "right" && enabled.Contains(w.Id))
                    .OrderBy(w => w.DefaultOrder)
                    .Select(w => $"      - {w.Id}")
                    .ToList();

                var sb = new StringBuilder();
                sb.AppendLine("    widgets:");
                
                sb.AppendLine("      left:");
                if (leftWidgets.Count > 0) {
                    foreach (var line in leftWidgets) sb.AppendLine(line);
                } else {
                    sb.AppendLine("      - bar_menu"); // Always keep at least the settings button accessible
                }

                sb.AppendLine("      center:");
                if (centerWidgets.Count > 0) {
                    foreach (var line in centerWidgets) sb.AppendLine(line);
                }

                sb.AppendLine("      right:");
                if (rightWidgets.Count > 0) {
                    foreach (var line in rightWidgets) sb.AppendLine(line);
                }

                string newWidgetsBlock = sb.ToString().TrimEnd();

                // Replace the widgets block inside primary-bar in config.yaml
                string pattern = @"(    widgets:[\s\S]*?)(?=\nwidgets:|\Z)";
                var regex = new Regex(pattern);
                if (regex.IsMatch(yaml)) {
                    string updatedYaml = regex.Replace(yaml, newWidgetsBlock, 1);
                    File.WriteAllText(ConfigPath, updatedYaml, Encoding.UTF8);
                }
            } catch {}
        }
    }
}
