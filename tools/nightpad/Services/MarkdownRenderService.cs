using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NightPad.Services;

/// <summary>
/// High-performance native service to convert Markdown into clean dark-themed WPF FlowDocument.
/// </summary>
public static partial class MarkdownRenderService
{
    private static readonly SolidColorBrush TextPrimaryBrush = new(Color.FromRgb(0xF0, 0xF6, 0xFC));
    private static readonly SolidColorBrush TextSecondaryBrush = new(Color.FromRgb(0x8B, 0x94, 0x9E));
    private static readonly SolidColorBrush AccentBlueBrush = new(Color.FromRgb(0x58, 0xA6, 0xFF));
    private static readonly SolidColorBrush CodeBgBrush = new(Color.FromRgb(0x16, 0x1B, 0x22));
    private static readonly SolidColorBrush CodeBorderBrush = new(Color.FromRgb(0x30, 0x36, 0x3D));
    private static readonly SolidColorBrush BlockquoteBorderBrush = new(Color.FromRgb(0x58, 0xA6, 0xFF));
    private static readonly FontFamily CodeFontFamily = new("Cascadia Code, Consolas, Courier New");
    private static readonly FontFamily BodyFontFamily = new("Segoe UI, Cairo, Tahoma, Arial");

    [GeneratedRegex(@"^#{1,6}\s+")]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"\*\*(.+?)\*\*|__(.+?)__")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(.+?)(?<!_)_(?!_)")]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex InlineCodeRegex();

    [GeneratedRegex(@"\[([^\]]+)\]\(([^)]+)\)")]
    private static partial Regex LinkRegex();

    /// <summary>
    /// Renders raw Markdown text into a styled WPF FlowDocument.
    /// </summary>
    public static FlowDocument Render(string markdownText, bool isRtl = false)
    {
        var doc = new FlowDocument
        {
            Background = new SolidColorBrush(Color.FromRgb(0x0D, 0x11, 0x17)),
            Foreground = TextPrimaryBrush,
            FontFamily = BodyFontFamily,
            FontSize = 13.5,
            LineHeight = 22,
            PagePadding = new Thickness(16, 12, 16, 16),
            FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
        };

        if (string.IsNullOrWhiteSpace(markdownText))
        {
            var placeholder = new Paragraph(new Run("Type Markdown to see live preview..."))
            {
                Foreground = TextSecondaryBrush,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 10, 0, 10)
            };
            doc.Blocks.Add(placeholder);
            return doc;
        }

        string[] lines = markdownText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool inCodeBlock = false;
        var codeBlockLines = new List<string>();
        string codeBlockLang = "";

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            // Code Block start/end
            if (trimmed.StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    inCodeBlock = true;
                    codeBlockLang = trimmed.Length > 3 ? trimmed[3..].Trim() : "";
                    codeBlockLines.Clear();
                }
                else
                {
                    inCodeBlock = false;
                    doc.Blocks.Add(CreateCodeBlock(codeBlockLines, codeBlockLang));
                    codeBlockLines.Clear();
                }
                continue;
            }

            if (inCodeBlock)
            {
                codeBlockLines.Add(line);
                continue;
            }

            // Blank line
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            // Horizontal Rule
            if (trimmed is "---" or "***" or "___" || (trimmed.Length >= 3 && Regex.IsMatch(trimmed, @"^[-*_]{3,}$")))
            {
                doc.Blocks.Add(CreateHorizontalRule());
                continue;
            }

            // Headers
            if (trimmed.StartsWith('#'))
            {
                var match = HeaderRegex().Match(trimmed);
                if (match.Success)
                {
                    int level = match.Value.Trim().Length;
                    string headerText = trimmed[match.Length..];
                    doc.Blocks.Add(CreateHeader(headerText, level));
                    continue;
                }
            }

            // Blockquotes
            if (trimmed.StartsWith('>'))
            {
                string quoteText = trimmed.Length > 1 ? trimmed[1..].Trim() : "";
                doc.Blocks.Add(CreateBlockquote(quoteText));
                continue;
            }

            // Unordered List Items (- or * or +)
            if (Regex.IsMatch(trimmed, @"^[-*+]\s+"))
            {
                string itemText = Regex.Replace(trimmed, @"^[-*+]\s+", "");
                doc.Blocks.Add(CreateListItem("•", itemText));
                continue;
            }

            // Ordered List Items (1. or 2.)
            if (Regex.IsMatch(trimmed, @"^\d+\.\s+"))
            {
                var prefixMatch = Regex.Match(trimmed, @"^\d+\.");
                string prefix = prefixMatch.Value;
                string itemText = trimmed[prefixMatch.Length..].Trim();
                doc.Blocks.Add(CreateListItem(prefix, itemText));
                continue;
            }

            // Regular Paragraph
            doc.Blocks.Add(CreateParagraph(trimmed));
        }

        // Handle unterminated code block at EOF
        if (inCodeBlock && codeBlockLines.Count > 0)
        {
            doc.Blocks.Add(CreateCodeBlock(codeBlockLines, codeBlockLang));
        }

        return doc;
    }

    private static Block CreateHeader(string text, int level)
    {
        double fontSize = level switch
        {
            1 => 22,
            2 => 18,
            3 => 16,
            4 => 14.5,
            _ => 13.5
        };

        var p = new Paragraph
        {
            FontSize = fontSize,
            FontWeight = FontWeights.Bold,
            Foreground = TextPrimaryBrush,
            Margin = new Thickness(0, level == 1 ? 16 : 12, 0, 6)
        };

        ApplyInlineFormatting(p, text);

        if (level <= 2)
        {
            var section = new Section { Margin = new Thickness(0, 0, 0, 6) };
            section.Blocks.Add(p);
            section.Blocks.Add(CreateHorizontalRule(0.5));
            return section;
        }

        return p;
    }

    private static Block CreateParagraph(string text)
    {
        var p = new Paragraph
        {
            Margin = new Thickness(0, 3, 0, 6),
            Foreground = TextPrimaryBrush
        };
        ApplyInlineFormatting(p, text);
        return p;
    }

    private static Block CreateListItem(string bullet, string text)
    {
        var p = new Paragraph
        {
            Margin = new Thickness(14, 2, 0, 2),
            Foreground = TextPrimaryBrush
        };

        var bulletRun = new Run($"{bullet} ")
        {
            Foreground = AccentBlueBrush,
            FontWeight = FontWeights.Bold
        };
        p.Inlines.Add(bulletRun);

        ApplyInlineFormatting(p, text);
        return p;
    }

    private static Block CreateBlockquote(string text)
    {
        var p = new Paragraph
        {
            Margin = new Thickness(8, 4, 0, 4),
            Padding = new Thickness(10, 4, 4, 4),
            BorderBrush = BlockquoteBorderBrush,
            BorderThickness = new Thickness(3, 0, 0, 0),
            Foreground = TextSecondaryBrush,
            FontStyle = FontStyles.Italic
        };

        ApplyInlineFormatting(p, text);
        return p;
    }

    private static Block CreateCodeBlock(List<string> lines, string language)
    {
        string codeContent = string.Join(Environment.NewLine, lines);

        var codeRun = new Run(codeContent)
        {
            FontFamily = CodeFontFamily,
            FontSize = 12.5,
            Foreground = new SolidColorBrush(Color.FromRgb(0x7E, 0xE7, 0x87)) // Soft light green syntax color
        };

        var p = new Paragraph(codeRun)
        {
            Margin = new Thickness(0),
            LineHeight = 18
        };

        var section = new Section
        {
            Background = CodeBgBrush,
            BorderBrush = CodeBorderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 8, 10, 8),
            Margin = new Thickness(0, 8, 0, 8)
        };

        if (!string.IsNullOrEmpty(language))
        {
            var langHeader = new Paragraph(new Run(language.ToUpperInvariant())
            {
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = TextSecondaryBrush
            })
            {
                Margin = new Thickness(0, 0, 0, 4)
            };
            section.Blocks.Add(langHeader);
        }

        section.Blocks.Add(p);
        return section;
    }

    private static Block CreateHorizontalRule(double thickness = 1.0)
    {
        return new Paragraph
        {
            Margin = new Thickness(0, 8, 0, 8),
            BorderBrush = CodeBorderBrush,
            BorderThickness = new Thickness(0, 0, 0, thickness),
            LineHeight = 1
        };
    }

    /// <summary>
    /// Parses inline Markdown (bold, italic, inline code, links) and populates inlines of a paragraph.
    /// </summary>
    private static void ApplyInlineFormatting(Paragraph paragraph, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // Tokenize and parse inline elements (code, bold, italic, links)
        int index = 0;
        while (index < text.Length)
        {
            // Inline Code `...`
            int codeStart = text.IndexOf('`', index);
            int boldStart1 = text.IndexOf("**", index, StringComparison.Ordinal);
            int boldStart2 = text.IndexOf("__", index, StringComparison.Ordinal);
            int linkStart = text.IndexOf('[', index);

            int nextSpecial = -1;
            string specialType = "";

            void CheckSpecial(int pos, string type)
            {
                if (pos >= 0 && (nextSpecial == -1 || pos < nextSpecial))
                {
                    nextSpecial = pos;
                    specialType = type;
                }
            }

            CheckSpecial(codeStart, "code");
            CheckSpecial(boldStart1, "bold**");
            CheckSpecial(boldStart2, "bold__");
            CheckSpecial(linkStart, "link");

            if (nextSpecial == -1)
            {
                // Remaining plain text
                paragraph.Inlines.Add(new Run(text[index..]));
                break;
            }

            if (nextSpecial > index)
            {
                paragraph.Inlines.Add(new Run(text[index..nextSpecial]));
                index = nextSpecial;
            }

            if (specialType == "code")
            {
                int codeEnd = text.IndexOf('`', index + 1);
                if (codeEnd > index)
                {
                    string code = text.Substring(index + 1, codeEnd - index - 1);
                    var span = new Span(new Run(code))
                    {
                        FontFamily = CodeFontFamily,
                        FontSize = 12,
                        Background = CodeBgBrush,
                        Foreground = AccentBlueBrush
                    };
                    paragraph.Inlines.Add(span);
                    index = codeEnd + 1;
                    continue;
                }
            }
            else if (specialType is "bold**" or "bold__")
            {
                string tag = specialType == "bold**" ? "**" : "__";
                int boldEnd = text.IndexOf(tag, index + 2, StringComparison.Ordinal);
                if (boldEnd > index)
                {
                    string boldText = text.Substring(index + 2, boldEnd - index - 2);
                    var boldRun = new Run(boldText) { FontWeight = FontWeights.Bold };
                    paragraph.Inlines.Add(boldRun);
                    index = boldEnd + 2;
                    continue;
                }
            }
            else if (specialType == "link")
            {
                var match = LinkRegex().Match(text, index);
                if (match.Success && match.Index == index)
                {
                    string label = match.Groups[1].Value;
                    string url = match.Groups[2].Value;

                    var linkRun = new Run(label)
                    {
                        Foreground = AccentBlueBrush,
                        TextDecorations = TextDecorations.Underline
                    };
                    paragraph.Inlines.Add(linkRun);
                    index += match.Length;
                    continue;
                }
            }

            // If not matching a pattern, output single character and advance
            paragraph.Inlines.Add(new Run(text[index].ToString()));
            index++;
        }
    }
}
