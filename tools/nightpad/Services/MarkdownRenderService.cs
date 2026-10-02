using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    [GeneratedRegex(@"^!\[(?<alt>[^\]]*)\]\((?<src>[^)\s]+)(?:\s+[""'][^""']*[""'])?\)\s*$")]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"\[([^\]]+)\]\(([^)]+)\)")]
    private static partial Regex LinkRegex();

    /// <summary>
    /// Renders raw Markdown text into a styled WPF FlowDocument with interactive table and image controls.
    /// </summary>
    public static FlowDocument Render(
        string markdownText,
        bool isRtl = false,
        string? baseDirectory = null,
        Action<string, string>? onCopyImage = null,
        Action<string, string>? onDeleteImage = null)
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
                    if (MathPlotService.IsPlotLanguage(codeBlockLang))
                    {
                        doc.Blocks.Add(MathPlotService.CreatePlotBlock(codeBlockLines));
                    }
                    else if (MathFormulaService.IsMathLanguage(codeBlockLang))
                    {
                        string mathText = string.Join(Environment.NewLine, codeBlockLines);
                        doc.Blocks.Add(MathFormulaService.CreateMathBlock(mathText, isRtl));
                    }
                    else
                    {
                        doc.Blocks.Add(CreateCodeBlock(codeBlockLines, codeBlockLang));
                    }
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

            // Block Math: $$ ... $$
            if (trimmed.StartsWith("$$"))
            {
                if (trimmed.EndsWith("$$") && trimmed.Length >= 4)
                {
                    string formula = trimmed[2..^2].Trim();
                    doc.Blocks.Add(MathFormulaService.CreateMathBlock(formula, isRtl));
                    continue;
                }
                else
                {
                    var mathLines = new List<string>();
                    string firstLineMath = trimmed[2..].Trim();
                    if (!string.IsNullOrEmpty(firstLineMath))
                        mathLines.Add(firstLineMath);

                    int mathIdx = i + 1;
                    while (mathIdx < lines.Length)
                    {
                        string mLine = lines[mathIdx].Trim();
                        if (mLine.EndsWith("$$"))
                        {
                            string lastMath = mLine[..^2].Trim();
                            if (!string.IsNullOrEmpty(lastMath))
                                mathLines.Add(lastMath);
                            break;
                        }
                        mathLines.Add(lines[mathIdx]);
                        mathIdx++;
                    }

                    string fullMath = string.Join(Environment.NewLine, mathLines);
                    doc.Blocks.Add(MathFormulaService.CreateMathBlock(fullMath, isRtl));
                    i = mathIdx;
                    continue;
                }
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

            // Image Block (![alt](url))
            var imgMatch = ImageRegex().Match(trimmed);
            if (imgMatch.Success)
            {
                string alt = imgMatch.Groups["alt"].Value;
                string src = imgMatch.Groups["src"].Value.Trim().Trim('<', '>');
                doc.Blocks.Add(CreateImageBlock(alt, src, baseDirectory, onCopyImage, onDeleteImage));
                continue;
            }

            // Table Block (| Col 1 | Col 2 | ... followed by |---|---|)
            if (trimmed.Contains('|') && i + 1 < lines.Length && IsTableDelimiter(lines[i + 1]))
            {
                var headerCells = SplitTableRow(trimmed);
                var delimiterCells = SplitTableRow(lines[i + 1]);
                var alignments = ParseTableAlignments(delimiterCells, isRtl);

                int colCount = Math.Max(headerCells.Count, alignments.Count);
                var dataRows = new List<List<string>>();

                int nextIdx = i + 2;
                while (nextIdx < lines.Length)
                {
                    string rowTrimmed = lines[nextIdx].Trim();
                    if (string.IsNullOrWhiteSpace(rowTrimmed) ||
                        rowTrimmed.StartsWith("```") ||
                        rowTrimmed.StartsWith('#'))
                    {
                        break;
                    }

                    if (rowTrimmed.Contains('|'))
                    {
                        var rowCells = SplitTableRow(rowTrimmed);
                        dataRows.Add(rowCells);
                        nextIdx++;
                    }
                    else
                    {
                        break;
                    }
                }

                doc.Blocks.Add(CreateTable(headerCells, alignments, dataRows, colCount, isRtl));
                i = nextIdx - 1;
                continue;
            }

            // Regular Paragraph
            doc.Blocks.Add(CreateParagraph(trimmed));
        }

        // Handle unterminated code block at EOF
        if (inCodeBlock && codeBlockLines.Count > 0)
        {
            if (MathPlotService.IsPlotLanguage(codeBlockLang))
            {
                doc.Blocks.Add(MathPlotService.CreatePlotBlock(codeBlockLines));
            }
            else if (MathFormulaService.IsMathLanguage(codeBlockLang))
            {
                string mathText = string.Join(Environment.NewLine, codeBlockLines);
                doc.Blocks.Add(MathFormulaService.CreateMathBlock(mathText, isRtl));
            }
            else
            {
                doc.Blocks.Add(CreateCodeBlock(codeBlockLines, codeBlockLang));
            }
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

    private static Block CreateImageBlock(
        string alt,
        string src,
        string? baseDirectory,
        Action<string, string>? onCopyImage = null,
        Action<string, string>? onDeleteImage = null)
    {
        string resolvedPath = src.Trim().Trim('"', '\'');
        bool isHttp = resolvedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                      resolvedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        if (!isHttp)
        {
            string localCandidate = resolvedPath.Replace('/', '\\');

            if (Path.IsPathRooted(localCandidate))
            {
                try
                {
                    resolvedPath = Path.GetFullPath(localCandidate);
                }
                catch { }
            }
            else
            {
                if (!string.IsNullOrEmpty(baseDirectory))
                {
                    try
                    {
                        string candidate = Path.GetFullPath(Path.Combine(baseDirectory, localCandidate));
                        if (File.Exists(candidate))
                        {
                            resolvedPath = candidate;
                        }
                        else
                        {
                            resolvedPath = candidate;
                        }
                    }
                    catch { }
                }

                if (!File.Exists(resolvedPath))
                {
                    try
                    {
                        string defaultAssetsParent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NightPad");
                        string candidate = Path.GetFullPath(Path.Combine(defaultAssetsParent, localCandidate));
                        if (File.Exists(candidate))
                        {
                            resolvedPath = candidate;
                        }
                    }
                    catch { }
                }
            }
        }

        bool fileExists = false;
        try
        {
            fileExists = !isHttp && File.Exists(resolvedPath);
        }
        catch { }

        BitmapSource? bitmap = null;
        if (fileExists || isHttp)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(resolvedPath, UriKind.Absolute);
                if (!isHttp)
                {
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                }
                bi.EndInit();
                bi.Freeze();
                bitmap = bi;
            }
            catch
            {
                bitmap = null;
            }
        }

        var outerCard = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x16, 0x1B, 0x22)),
            BorderBrush = CodeBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 8, 0, 10),
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        outerCard.Child = mainGrid;

        // Toolbar Header
        var toolbarBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x26, 0x2D)),
            BorderBrush = CodeBorderBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            CornerRadius = new CornerRadius(5, 5, 0, 0),
            Padding = new Thickness(8, 4, 8, 4)
        };

        var toolbarGrid = new Grid();
        toolbarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        toolbarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        toolbarBorder.Child = toolbarGrid;

        // File info
        var infoStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        infoStack.Children.Add(new TextBlock
        {
            Text = "🖼️ ",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        });

        string displayName = !string.IsNullOrWhiteSpace(alt)
            ? alt
            : (!isHttp ? Path.GetFileName(resolvedPath) : src);

        if (bitmap != null)
        {
            displayName += $" ({bitmap.PixelWidth}×{bitmap.PixelHeight})";
        }

        infoStack.Children.Add(new TextBlock
        {
            Text = displayName,
            Foreground = TextSecondaryBrush,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = resolvedPath
        });
        Grid.SetColumn(infoStack, 0);
        toolbarGrid.Children.Add(infoStack);

        // Action Buttons
        var btnStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (bitmap != null)
        {
            var btnCopy = new Button
            {
                Content = "📋 Copy",
                ToolTip = "Copy image to clipboard / نسخ الصورة",
                Background = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D)),
                Foreground = AccentBlueBrush,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 6, 0),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };

            btnCopy.Click += (s, e) =>
            {
                try
                {
                    var data = new DataObject();
                    data.SetImage(bitmap);
                    if (fileExists && File.Exists(resolvedPath))
                    {
                        var sc = new System.Collections.Specialized.StringCollection { resolvedPath };
                        data.SetFileDropList(sc);
                    }
                    Clipboard.SetDataObject(data, true);

                    btnCopy.Content = "✓ Copied!";
                    btnCopy.Foreground = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50));

                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1.5)
                    };
                    timer.Tick += (ts, te) =>
                    {
                        timer.Stop();
                        btnCopy.Content = "📋 Copy";
                        btnCopy.Foreground = AccentBlueBrush;
                    };
                    timer.Start();

                    onCopyImage?.Invoke(src, resolvedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not copy image:\n{ex.Message}", "Copy Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            btnStack.Children.Add(btnCopy);

            var btnOpen = new Button
            {
                Content = "↗ Open",
                ToolTip = "Open in external photo viewer",
                Background = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D)),
                Foreground = TextPrimaryBrush,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(0, 0, 6, 0),
                FontSize = 11,
                Cursor = Cursors.Hand
            };
            btnOpen.Click += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(resolvedPath) { UseShellExecute = true });
                }
                catch { }
            };
            btnStack.Children.Add(btnOpen);
        }

        var btnDelete = new Button
        {
            Content = "🗑️ Delete",
            ToolTip = "Remove image from document and disk / مسح الصورة",
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D)),
            Foreground = new SolidColorBrush(Color.FromRgb(0xF8, 0x51, 0x49)),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(6, 2, 6, 2),
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand
        };
        btnDelete.Click += (s, e) =>
        {
            onDeleteImage?.Invoke(src, resolvedPath);
        };
        btnStack.Children.Add(btnDelete);

        Grid.SetColumn(btnStack, 1);
        toolbarGrid.Children.Add(btnStack);

        Grid.SetRow(toolbarBorder, 0);
        mainGrid.Children.Add(toolbarBorder);

        if (bitmap != null)
        {
            var imgContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x0D, 0x11, 0x17)),
                CornerRadius = new CornerRadius(0, 0, 5, 5),
                Padding = new Thickness(6),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var img = new Image
            {
                Source = bitmap,
                MaxWidth = 680,
                MaxHeight = 520,
                Stretch = Stretch.Uniform,
                Cursor = Cursors.Hand,
                ToolTip = $"Click to open externally:\n{resolvedPath}"
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(resolvedPath) { UseShellExecute = true });
                }
                catch { }
            };

            imgContainer.Child = img;
            Grid.SetRow(imgContainer, 1);
            mainGrid.Children.Add(imgContainer);
        }
        else
        {
            var missingContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x1F, 0x16, 0x16)),
                CornerRadius = new CornerRadius(0, 0, 5, 5),
                Padding = new Thickness(12, 10, 12, 10)
            };

            var missingText = new TextBlock
            {
                Text = $"⚠️ Image not found or could not be loaded: {src}",
                Foreground = new SolidColorBrush(Color.FromRgb(0xF8, 0x51, 0x49)),
                FontStyle = FontStyles.Italic,
                FontSize = 12
            };
            missingContainer.Child = missingText;
            Grid.SetRow(missingContainer, 1);
            mainGrid.Children.Add(missingContainer);
        }

        return new BlockUIContainer(outerCard)
        {
            Margin = new Thickness(0, 4, 0, 8)
        };
    }

    private static Block CreateTable(
        List<string> headerCells,
        List<TextAlignment> alignments,
        List<List<string>> dataRows,
        int colCount,
        bool isRtl)
    {
        var table = new Table
        {
            CellSpacing = 0,
            Margin = new Thickness(0, 8, 0, 12),
            BorderBrush = CodeBorderBrush,
            BorderThickness = new Thickness(1),
            Background = CodeBgBrush
        };

        int[] maxColLengths = new int[colCount];
        for (int c = 0; c < colCount; c++)
        {
            maxColLengths[c] = c < headerCells.Count ? headerCells[c].Length : 5;
        }
        foreach (var row in dataRows)
        {
            for (int c = 0; c < colCount; c++)
            {
                if (c < row.Count)
                {
                    maxColLengths[c] = Math.Max(maxColLengths[c], row[c].Length);
                }
            }
        }

        for (int c = 0; c < colCount; c++)
        {
            double weight = Math.Max(1.0, Math.Min(5.0, maxColLengths[c] / 12.0));
            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(weight, GridUnitType.Star)
            });
        }

        var rowGroup = new TableRowGroup();
        table.RowGroups.Add(rowGroup);

        // Header Row
        var headerRow = new TableRow
        {
            Background = new SolidColorBrush(Color.FromRgb(0x1F, 0x24, 0x2C))
        };

        for (int c = 0; c < colCount; c++)
        {
            string cellText = c < headerCells.Count ? headerCells[c] : "";
            var align = c < alignments.Count ? alignments[c] : (isRtl ? TextAlignment.Right : TextAlignment.Left);

            var p = new Paragraph
            {
                TextAlignment = align,
                FontWeight = FontWeights.Bold,
                Foreground = AccentBlueBrush,
                Margin = new Thickness(0),
                LineHeight = 20
            };
            ApplyInlineFormatting(p, cellText);

            var cell = new TableCell(p)
            {
                Padding = new Thickness(10, 8, 10, 8),
                BorderBrush = CodeBorderBrush,
                BorderThickness = new Thickness(0, 0, c < colCount - 1 ? 1 : 0, 2)
            };
            headerRow.Cells.Add(cell);
        }
        rowGroup.Rows.Add(headerRow);

        // Data Rows
        for (int r = 0; r < dataRows.Count; r++)
        {
            var rowData = dataRows[r];
            var isEven = r % 2 == 0;
            var row = new TableRow
            {
                Background = isEven
                    ? new SolidColorBrush(Color.FromRgb(0x0D, 0x11, 0x17))
                    : new SolidColorBrush(Color.FromRgb(0x16, 0x1B, 0x22))
            };

            bool isLastRow = r == dataRows.Count - 1;

            for (int c = 0; c < colCount; c++)
            {
                string cellText = c < rowData.Count ? rowData[c] : "";
                var align = c < alignments.Count ? alignments[c] : (isRtl ? TextAlignment.Right : TextAlignment.Left);

                var p = new Paragraph
                {
                    TextAlignment = align,
                    Foreground = TextPrimaryBrush,
                    Margin = new Thickness(0),
                    LineHeight = 20
                };
                ApplyInlineFormatting(p, cellText);

                var cell = new TableCell(p)
                {
                    Padding = new Thickness(10, 6, 10, 6),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x2C, 0x35)),
                    BorderThickness = new Thickness(0, 0, c < colCount - 1 ? 1 : 0, isLastRow ? 0 : 1)
                };
                row.Cells.Add(cell);
            }
            rowGroup.Rows.Add(row);
        }

        return table;
    }

    private static bool IsTableDelimiter(string line)
    {
        string trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || !trimmed.Contains('-') || !trimmed.Contains('|'))
            return false;

        var cells = SplitTableRow(trimmed);
        if (cells.Count == 0)
            return false;

        foreach (var cell in cells)
        {
            string c = cell.Trim();
            if (c.Length == 0 || !Regex.IsMatch(c, @"^:?-+:?$"))
                return false;
        }

        return true;
    }

    private static List<string> SplitTableRow(string line)
    {
        var list = new List<string>();
        string trimmed = line.Trim();
        if (trimmed.StartsWith('|'))
            trimmed = trimmed[1..];
        if (trimmed.EndsWith('|'))
            trimmed = trimmed[..^1];

        var parts = Regex.Split(trimmed, @"(?<!\\)\|");
        foreach (var p in parts)
        {
            list.Add(p.Trim().Replace(@"\|", "|"));
        }
        return list;
    }

    private static List<TextAlignment> ParseTableAlignments(List<string> delimiterCells, bool isRtl)
    {
        var alignments = new List<TextAlignment>();
        foreach (var cell in delimiterCells)
        {
            string c = cell.Trim();
            bool left = c.StartsWith(':');
            bool right = c.EndsWith(':');

            if (left && right)
                alignments.Add(TextAlignment.Center);
            else if (right)
                alignments.Add(TextAlignment.Right);
            else if (left)
                alignments.Add(TextAlignment.Left);
            else
                alignments.Add(isRtl ? TextAlignment.Right : TextAlignment.Left);
        }
        return alignments;
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
    /// Parses inline Markdown (bold, italic, inline code, links, breaks) and populates inlines of a paragraph.
    /// </summary>
    private static void ApplyInlineFormatting(Paragraph paragraph, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        int index = 0;
        while (index < text.Length)
        {
            int codeStart = text.IndexOf('`', index);
            int boldStart1 = text.IndexOf("**", index, StringComparison.Ordinal);
            int boldStart2 = text.IndexOf("__", index, StringComparison.Ordinal);
            int linkStart = text.IndexOf('[', index);
            int brStart1 = text.IndexOf("<br>", index, StringComparison.OrdinalIgnoreCase);
            int brStart2 = text.IndexOf("<br/>", index, StringComparison.OrdinalIgnoreCase);
            int brStart3 = text.IndexOf("<br />", index, StringComparison.OrdinalIgnoreCase);

            int mathStart = -1;
            int dollarIdx = text.IndexOf('$', index);
            if (dollarIdx >= index && dollarIdx + 1 < text.Length && text[dollarIdx + 1] != '$' && !char.IsWhiteSpace(text[dollarIdx + 1]))
            {
                bool escaped = dollarIdx > 0 && text[dollarIdx - 1] == '\\';
                bool prevDollar = dollarIdx > 0 && text[dollarIdx - 1] == '$';
                if (!escaped && !prevDollar)
                {
                    int closingDollar = text.IndexOf('$', dollarIdx + 1);
                    if (closingDollar > dollarIdx + 1 && !char.IsWhiteSpace(text[closingDollar - 1]))
                    {
                        bool closeEscaped = text[closingDollar - 1] == '\\';
                        bool nextDollar = closingDollar + 1 < text.Length && text[closingDollar + 1] == '$';
                        if (!closeEscaped && !nextDollar)
                        {
                            mathStart = dollarIdx;
                        }
                    }
                }
            }

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
            CheckSpecial(mathStart, "math");
            CheckSpecial(brStart1, "br4");
            CheckSpecial(brStart2, "br5");
            CheckSpecial(brStart3, "br6");

            if (nextSpecial == -1)
            {
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
            else if (specialType == "math")
            {
                int mathClose = text.IndexOf('$', index + 1);
                if (mathClose > index + 1)
                {
                    string formula = text.Substring(index + 1, mathClose - index - 1);
                    paragraph.Inlines.Add(MathFormulaService.CreateInlineMath(formula));
                    index = mathClose + 1;
                    continue;
                }
            }
            else if (specialType == "br4")
            {
                paragraph.Inlines.Add(new LineBreak());
                index += 4;
                continue;
            }
            else if (specialType == "br5")
            {
                paragraph.Inlines.Add(new LineBreak());
                index += 5;
                continue;
            }
            else if (specialType == "br6")
            {
                paragraph.Inlines.Add(new LineBreak());
                index += 6;
                continue;
            }

            paragraph.Inlines.Add(new Run(text[index].ToString()));
            index++;
        }
    }
}
