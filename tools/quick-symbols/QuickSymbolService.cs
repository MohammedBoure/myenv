using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace QuickSymbols;

public class QuickSymbolItem
{
    public string Text { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsCustom { get; set; }

    public QuickSymbolItem() { }

    public QuickSymbolItem(string text, string label = "", string category = "General", bool isCustom = false)
    {
        Text = text;
        Label = string.IsNullOrWhiteSpace(label) ? text : label;
        Category = string.IsNullOrWhiteSpace(category) ? "General" : category;
        IsCustom = isCustom;
    }
}

public static class QuickSymbolService
{
    private static readonly string StoragePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NightPad",
        "quick_symbols.json"
    );

    public static List<QuickSymbolItem> LoadItems()
    {
        try
        {
            if (File.Exists(StoragePath))
            {
                string json = File.ReadAllText(StoragePath);
                var items = JsonSerializer.Deserialize<List<QuickSymbolItem>>(json);
                if (items != null && items.Count > 0)
                {
                    return items;
                }
            }
        }
        catch { }

        var defaults = GetDefaultItems();
        SaveItems(defaults);
        return defaults;
    }

    public static void SaveItems(List<QuickSymbolItem> items)
    {
        try
        {
            string? dir = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(StoragePath, json);
        }
        catch { }
    }

    public static List<QuickSymbolItem> GetDefaultItems()
    {
        return new List<QuickSymbolItem>
        {
            // Arrows
            new("→", "Right Arrow", "Arrows"),
            new("←", "Left Arrow", "Arrows"),
            new("↑", "Up Arrow", "Arrows"),
            new("↓", "Down Arrow", "Arrows"),
            new("⇒", "Implies (Double Right)", "Arrows"),
            new("⇔", "Equivalent (Double Left-Right)", "Arrows"),
            new("➜", "Heavy Right Arrow", "Arrows"),
            new("➔", "Bold Right Arrow", "Arrows"),

            // Bullets & Checkmarks
            new("✓", "Checkmark", "Symbols"),
            new("✗", "Cross Mark", "Symbols"),
            new("★", "Star", "Symbols"),
            new("●", "Bullet Point", "Symbols"),
            new("◆", "Diamond", "Symbols"),
            new("■", "Black Square", "Symbols"),
            new("⚡", "Lightning", "Symbols"),
            new("⚠️", "Warning Sign", "Symbols"),

            // Math & Logic
            new("≈", "Approximately Equal", "Math"),
            new("≠", "Not Equal", "Math"),
            new("≤", "Less Than or Equal", "Math"),
            new("≥", "Greater Than or Equal", "Math"),
            new("±", "Plus-Minus", "Math"),
            new("×", "Multiplication Sign", "Math"),
            new("÷", "Division Sign", "Math"),
            new("∞", "Infinity", "Math"),
            new("√", "Square Root", "Math"),
            new("π", "Pi", "Math"),
            new("∑", "Summation", "Math"),
            new("°", "Degree Sign", "Math"),

            // Typography & Punctuation
            new("—", "Em Dash", "Typography"),
            new("–", "En Dash", "Typography"),
            new("…", "Ellipsis", "Typography"),
            new("«", "Left Guillemet", "Typography"),
            new("»", "Right Guillemet", "Typography"),
            new("§", "Section Sign", "Typography"),
            new("©", "Copyright", "Typography"),
            new("®", "Registered Trademark", "Typography"),
            new("™", "Trademark", "Typography"),
            new("•", "Bullet Dot", "Typography"),

            // Markdown & Code Snippets
            new("# TODO: ", "TODO Comment", "Snippets"),
            new("# NOTE: ", "NOTE Comment", "Snippets"),
            new("# FIXME: ", "FIXME Comment", "Snippets"),
            new("```\n\n```", "Code Block", "Snippets"),
            new("<!--  -->", "HTML/Markdown Comment", "Snippets"),
            new("- [ ] ", "Markdown Task Unchecked", "Snippets"),
            new("- [x] ", "Markdown Task Checked", "Snippets"),

            // Frequent Arabic Expressions
            new("بسم الله الرحمن الرحيم", "Basmala", "Arabic"),
            new("السلام عليكم ورحمة الله وبركاته", "Islamic Greeting", "Arabic"),
            new("الحمد لله", "Praise be to God", "Arabic"),
            new("إن شاء الله", "God Willing", "Arabic"),
            new("ملاحظة:", "Note in Arabic", "Arabic"),
            new("تنبيه:", "Warning in Arabic", "Arabic"),
            new("هام:", "Important in Arabic", "Arabic")
        };
    }
}
