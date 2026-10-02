using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NightPad.Services;

/// <summary>
/// Native high-performance service for parsing and rendering mathematical LaTeX formulas, equations, and symbols in WPF.
/// </summary>
public static class MathFormulaService
{
    private static readonly SolidColorBrush TextPrimaryBrush = new(Color.FromRgb(0xF0, 0xF6, 0xFC));
    private static readonly SolidColorBrush TextMutedBrush = new(Color.FromRgb(0x8B, 0x94, 0x9E));
    private static readonly SolidColorBrush AccentBlueBrush = new(Color.FromRgb(0x58, 0xA6, 0xFF));
    private static readonly SolidColorBrush CardBgBrush = new(Color.FromRgb(0x16, 0x1B, 0x22));
    private static readonly SolidColorBrush CardBorderBrush = new(Color.FromRgb(0x30, 0x36, 0x3D));
    private static readonly SolidColorBrush HeaderBgBrush = new(Color.FromRgb(0x21, 0x26, 0x2D));
    private static readonly FontFamily MathFontFamily = new("Cambria Math, Segoe UI Historic, Segoe UI Symbol, Times New Roman, Cascadia Code");

    private static readonly Dictionary<string, string> SymbolsMap = new(StringComparer.Ordinal)
    {
        // Greek lowercase
        ["\\alpha"] = "α",
        ["\\beta"] = "β",
        ["\\gamma"] = "γ",
        ["\\delta"] = "δ",
        ["\\epsilon"] = "ε",
        ["\\varepsilon"] = "ε",
        ["\\zeta"] = "ζ",
        ["\\eta"] = "η",
        ["\\theta"] = "θ",
        ["\\vartheta"] = "ϑ",
        ["\\iota"] = "ι",
        ["\\kappa"] = "κ",
        ["\\lambda"] = "λ",
        ["\\mu"] = "μ",
        ["\\nu"] = "ν",
        ["\\xi"] = "ξ",
        ["\\pi"] = "π",
        ["\\varpi"] = "ϖ",
        ["\\rho"] = "ρ",
        ["\\varrho"] = "ϱ",
        ["\\sigma"] = "σ",
        ["\\varsigma"] = "ς",
        ["\\tau"] = "τ",
        ["\\upsilon"] = "υ",
        ["\\phi"] = "φ",
        ["\\varphi"] = "ϕ",
        ["\\chi"] = "χ",
        ["\\psi"] = "ψ",
        ["\\omega"] = "ω",

        // Greek uppercase
        ["\\Gamma"] = "Γ",
        ["\\Delta"] = "Δ",
        ["\\Theta"] = "Θ",
        ["\\Lambda"] = "Λ",
        ["\\Xi"] = "Ξ",
        ["\\Pi"] = "Π",
        ["\\Sigma"] = "Σ",
        ["\\Upsilon"] = "Υ",
        ["\\Phi"] = "Φ",
        ["\\Psi"] = "Ψ",
        ["\\Omega"] = "Ω",

        // Operators & Relations
        ["\\times"] = "×",
        ["\\cdot"] = "·",
        ["\\div"] = "÷",
        ["\\pm"] = "±",
        ["\\mp"] = "∓",
        ["\\leq"] = "≤",
        ["\\le"] = "≤",
        ["\\geq"] = "≥",
        ["\\ge"] = "≥",
        ["\\neq"] = "≠",
        ["\\ne"] = "≠",
        ["\\approx"] = "≈",
        ["\\equiv"] = "≡",
        ["\\sim"] = "∼",
        ["\\propto"] = "∝",
        ["\\ll"] = "≪",
        ["\\gg"] = "≫",

        // Calculus, Sets, Logic
        ["\\infty"] = "∞",
        ["\\partial"] = "∂",
        ["\\nabla"] = "∇",
        ["\\hbar"] = "ℏ",
        ["\\in"] = "∈",
        ["\\notin"] = "∉",
        ["\\subset"] = "⊂",
        ["\\subseteq"] = "⊆",
        ["\\supset"] = "⊃",
        ["\\supseteq"] = "⊇",
        ["\\cup"] = "∪",
        ["\\cap"] = "∩",
        ["\\setminus"] = "∖",
        ["\\emptyset"] = "∅",
        ["\\forall"] = "∀",
        ["\\exists"] = "∃",
        ["\\nexists"] = "∄",
        ["\\neg"] = "¬",

        // Arrows
        ["\\to"] = "→",
        ["\\rightarrow"] = "→",
        ["\\leftarrow"] = "←",
        ["\\Rightarrow"] = "⇒",
        ["\\Leftarrow"] = "⇐",
        ["\\iff"] = "⇔",
        ["\\Leftrightarrow"] = "⇔",
        ["\\uparrow"] = "↑",
        ["\\downarrow"] = "↓"
    };

    /// <summary>
    /// Checks whether a code block language corresponds to mathematical formula notation.
    /// </summary>
    public static bool IsMathLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language)) return false;
        string l = language.Trim().ToLowerInvariant();
        return l is "math" or "latex" or "tex" or "equation" or "formula" or "katex";
    }

    /// <summary>
    /// Renders a block-level mathematical formula card in WPF FlowDocument.
    /// </summary>
    public static Block CreateMathBlock(string rawLatex, bool isRtl = false)
    {
        string formula = rawLatex.Trim();
        if (formula.StartsWith("$$") && formula.EndsWith("$$") && formula.Length >= 4)
        {
            formula = formula[2..^2].Trim();
        }

        var outerCard = new Border
        {
            Background = CardBgBrush,
            BorderBrush = CardBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 8, 0, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        outerCard.Child = mainGrid;

        // Top Toolbar
        var headerBorder = new Border
        {
            Background = HeaderBgBrush,
            BorderBrush = CardBorderBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            CornerRadius = new CornerRadius(5, 5, 0, 0),
            Padding = new Thickness(10, 4, 10, 4)
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerBorder.Child = headerGrid;

        var titleBlock = new TextBlock
        {
            Text = "∑ Mathematical Formula",
            Foreground = AccentBlueBrush,
            FontWeight = FontWeights.Bold,
            FontSize = 11.5,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleBlock, 0);
        headerGrid.Children.Add(titleBlock);

        var btnCopyLatex = new Button
        {
            Content = "📋 Copy LaTeX",
            ToolTip = "Copy raw LaTeX code to clipboard / نسخ كود المعادلة",
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D)),
            Foreground = AccentBlueBrush,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(8, 2, 8, 2),
            FontSize = 11,
            Cursor = Cursors.Hand
        };
        btnCopyLatex.Click += (s, e) =>
        {
            try
            {
                Clipboard.SetText(formula);
                btnCopyLatex.Content = "✓ Copied!";
                btnCopyLatex.Foreground = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50));
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                timer.Tick += (ts, te) =>
                {
                    timer.Stop();
                    btnCopyLatex.Content = "📋 Copy LaTeX";
                    btnCopyLatex.Foreground = AccentBlueBrush;
                };
                timer.Start();
            }
            catch { }
        };
        Grid.SetColumn(btnCopyLatex, 1);
        headerGrid.Children.Add(btnCopyLatex);

        Grid.SetRow(headerBorder, 0);
        mainGrid.Children.Add(headerBorder);

        // Body: Rendered Equation
        var bodyContainer = new Border
        {
            Padding = new Thickness(16, 14, 16, 14),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var mathVisual = RenderFormulaToVisual(formula, fontSize: 18.0);
        bodyContainer.Child = mathVisual;

        Grid.SetRow(bodyContainer, 1);
        mainGrid.Children.Add(bodyContainer);

        return new BlockUIContainer(outerCard)
        {
            Margin = new Thickness(0, 6, 0, 10)
        };
    }

    /// <summary>
    /// Renders an inline mathematical formula ($...$) as an InlineUIContainer for embedding inside Paragraphs.
    /// </summary>
    public static Inline CreateInlineMath(string formula)
    {
        var visual = RenderFormulaToVisual(formula, fontSize: 13.5);
        visual.VerticalAlignment = VerticalAlignment.Center;

        var container = new Border
        {
            Padding = new Thickness(2, 0, 2, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = visual
        };

        return new InlineUIContainer(container)
        {
            BaselineAlignment = BaselineAlignment.Center
        };
    }

    /// <summary>
    /// Recursive parser and visual builder for LaTeX expressions.
    /// </summary>
    public static FrameworkElement RenderFormulaToVisual(string latex, double fontSize = 16.0)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        string text = latex.Trim();
        int i = 0;

        while (i < text.Length)
        {
            // Skip spaces
            if (char.IsWhiteSpace(text[i]))
            {
                panel.Children.Add(new TextBlock { Text = " ", FontSize = fontSize, FontFamily = MathFontFamily });
                i++;
                continue;
            }

            // Fraction \frac{num}{den}
            if (text[i..].StartsWith("\\frac", StringComparison.Ordinal))
            {
                i += 5;
                string num = ExtractBracedGroup(text, ref i);
                string den = ExtractBracedGroup(text, ref i);

                var fracVisual = CreateFractionVisual(num, den, fontSize);
                panel.Children.Add(fracVisual);
                continue;
            }

            // Square root \sqrt{arg} or \sqrt[n]{arg}
            if (text[i..].StartsWith("\\sqrt", StringComparison.Ordinal))
            {
                i += 5;
                string degree = "";
                if (i < text.Length && text[i] == '[')
                {
                    int closeIdx = text.IndexOf(']', i);
                    if (closeIdx > i)
                    {
                        degree = text.Substring(i + 1, closeIdx - i - 1);
                        i = closeIdx + 1;
                    }
                }

                string arg = ExtractBracedGroup(text, ref i);
                var rootVisual = CreateSquareRootVisual(arg, degree, fontSize);
                panel.Children.Add(rootVisual);
                continue;
            }

            // Big Operators: \sum, \int, \prod
            if (text[i..].StartsWith("\\sum", StringComparison.Ordinal) ||
                text[i..].StartsWith("\\int", StringComparison.Ordinal) ||
                text[i..].StartsWith("\\prod", StringComparison.Ordinal) ||
                text[i..].StartsWith("\\lim", StringComparison.Ordinal))
            {
                string opCmd = text[i..].StartsWith("\\sum") ? "\\sum" :
                               text[i..].StartsWith("\\int") ? "\\int" :
                               text[i..].StartsWith("\\prod") ? "\\prod" : "\\lim";
                i += opCmd.Length;

                string opChar = opCmd switch
                {
                    "\\sum" => "∑",
                    "\\int" => "∫",
                    "\\prod" => "∏",
                    _ => "lim"
                };

                string lower = "";
                string upper = "";

                // Check for limits: _{lower}^{upper} or ^{upper}_{lower}
                while (i < text.Length && (text[i] == '_' || text[i] == '^'))
                {
                    if (text[i] == '_')
                    {
                        i++;
                        lower = ExtractBracedOrSingle(text, ref i);
                    }
                    else if (text[i] == '^')
                    {
                        i++;
                        upper = ExtractBracedOrSingle(text, ref i);
                    }
                }

                var bigOpVisual = CreateBigOperatorVisual(opChar, lower, upper, fontSize);
                panel.Children.Add(bigOpVisual);
                continue;
            }

            // Superscript ^ or Subscript _ on previous or current element
            if (text[i] == '^' || text[i] == '_')
            {
                bool isSuper = text[i] == '^';
                i++;
                string script = ExtractBracedOrSingle(text, ref i);
                var scriptBlock = new TextBlock
                {
                    Text = ReplaceKnownSymbols(script),
                    FontSize = fontSize * 0.72,
                    Foreground = TextPrimaryBrush,
                    FontFamily = MathFontFamily,
                    Margin = isSuper ? new Thickness(1, -fontSize * 0.45, 2, 0) : new Thickness(1, fontSize * 0.35, 2, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                panel.Children.Add(scriptBlock);
                continue;
            }

            // Functions: \sin, \cos, \tan, \ln, \log, \exp, \det, \max, \min
            if (text[i] == '\\')
            {
                var matchFunc = Regex.Match(text[i..], @"^\\(sin|cos|tan|arcsin|arccos|arctan|sinh|cosh|tanh|ln|log|exp|det|max|min|deg)\b");
                if (matchFunc.Success)
                {
                    string fName = matchFunc.Groups[1].Value;
                    panel.Children.Add(new TextBlock
                    {
                        Text = fName + " ",
                        FontSize = fontSize,
                        Foreground = AccentBlueBrush,
                        FontFamily = MathFontFamily,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    i += matchFunc.Length;
                    continue;
                }

                // Symbols (e.g. \alpha, \times, \leq, \infty)
                var matchSym = Regex.Match(text[i..], @"^\\[a-zA-Z]+");
                if (matchSym.Success)
                {
                    string cmd = matchSym.Value;
                    if (SymbolsMap.TryGetValue(cmd, out string? unicodeChar))
                    {
                        panel.Children.Add(new TextBlock
                        {
                            Text = unicodeChar,
                            FontSize = fontSize * 1.05,
                            Foreground = TextPrimaryBrush,
                            FontFamily = MathFontFamily,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(1, 0, 1, 0)
                        });
                    }
                    else
                    {
                        // Fallback: render raw command name
                        panel.Children.Add(new TextBlock
                        {
                            Text = cmd[1..],
                            FontSize = fontSize,
                            Foreground = TextPrimaryBrush,
                            FontFamily = MathFontFamily,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                    }
                    i += matchSym.Length;
                    continue;
                }
            }

            // Text block \text{...} or \mathrm{...}
            if (text[i..].StartsWith("\\text{", StringComparison.Ordinal) ||
                text[i..].StartsWith("\\mathrm{", StringComparison.Ordinal))
            {
                i = text.IndexOf('{', i);
                string textVal = ExtractBracedGroup(text, ref i);
                panel.Children.Add(new TextBlock
                {
                    Text = textVal,
                    FontSize = fontSize,
                    Foreground = TextPrimaryBrush,
                    FontFamily = MathFontFamily,
                    VerticalAlignment = VerticalAlignment.Center
                });
                continue;
            }

            // Normal Character / Variable / Number / Operator
            char c = text[i];
            bool isLetter = char.IsLetter(c);
            panel.Children.Add(new TextBlock
            {
                Text = c.ToString(),
                FontSize = fontSize,
                FontStyle = isLetter ? FontStyles.Italic : FontStyles.Normal,
                FontWeight = "+-*/=<>()[]{}".Contains(c) ? FontWeights.Normal : (isLetter ? FontWeights.SemiBold : FontWeights.Normal),
                Foreground = "=+-*/".Contains(c) ? AccentBlueBrush : TextPrimaryBrush,
                FontFamily = MathFontFamily,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = "=+-*/".Contains(c) ? new Thickness(3, 0, 3, 0) : new Thickness(0)
            });
            i++;
        }

        return panel;
    }

    private static FrameworkElement CreateFractionVisual(string num, string den, double fontSize)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 3, 0)
        };

        var numVisual = RenderFormulaToVisual(num, fontSize * 0.88);
        numVisual.HorizontalAlignment = HorizontalAlignment.Center;

        var bar = new Rectangle
        {
            Height = 1.4,
            Fill = TextPrimaryBrush,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 1.5, 0, 1.5)
        };

        var denVisual = RenderFormulaToVisual(den, fontSize * 0.88);
        denVisual.HorizontalAlignment = HorizontalAlignment.Center;

        stack.Children.Add(numVisual);
        stack.Children.Add(bar);
        stack.Children.Add(denVisual);

        return stack;
    }

    private static FrameworkElement CreateSquareRootVisual(string arg, string degree, double fontSize)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2, 0, 2, 0)
        };

        if (!string.IsNullOrEmpty(degree))
        {
            var degBlock = new TextBlock
            {
                Text = degree,
                FontSize = fontSize * 0.65,
                Foreground = TextMutedBrush,
                FontFamily = MathFontFamily,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, -2, 0)
            };
            panel.Children.Add(degBlock);
        }

        var radical = new TextBlock
        {
            Text = "√",
            FontSize = fontSize * 1.25,
            Foreground = TextPrimaryBrush,
            FontFamily = MathFontFamily,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, -1, 1, 0)
        };
        panel.Children.Add(radical);

        var contentBorder = new Border
        {
            BorderBrush = TextPrimaryBrush,
            BorderThickness = new Thickness(0, 1.4, 0, 0),
            Padding = new Thickness(2, 1, 3, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = RenderFormulaToVisual(arg, fontSize * 0.95)
        };
        panel.Children.Add(contentBorder);

        return panel;
    }

    private static FrameworkElement CreateBigOperatorVisual(string op, string lower, string upper, double fontSize)
    {
        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 3, 0)
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Upper
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Symbol
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Lower

        if (!string.IsNullOrEmpty(upper))
        {
            var upVisual = RenderFormulaToVisual(upper, fontSize * 0.68);
            upVisual.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetRow(upVisual, 0);
            grid.Children.Add(upVisual);
        }

        var opText = new TextBlock
        {
            Text = op,
            FontSize = fontSize * 1.5,
            Foreground = AccentBlueBrush,
            FontFamily = MathFontFamily,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(opText, 1);
        grid.Children.Add(opText);

        if (!string.IsNullOrEmpty(lower))
        {
            var lowVisual = RenderFormulaToVisual(lower, fontSize * 0.68);
            lowVisual.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetRow(lowVisual, 2);
            grid.Children.Add(lowVisual);
        }

        return grid;
    }

    private static string ExtractBracedGroup(string text, ref int index)
    {
        while (index < text.Length && text[index] != '{')
            index++;

        if (index >= text.Length) return "";

        int start = index + 1;
        int depth = 1;
        index++;

        while (index < text.Length && depth > 0)
        {
            if (text[index] == '{') depth++;
            else if (text[index] == '}') depth--;
            index++;
        }

        return depth == 0 ? text[start..(index - 1)] : text[start..];
    }

    private static string ExtractBracedOrSingle(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;

        if (index >= text.Length) return "";

        if (text[index] == '{')
        {
            return ExtractBracedGroup(text, ref index);
        }

        // Single character or single LaTeX command e.g. \infty
        if (text[index] == '\\')
        {
            var match = Regex.Match(text[index..], @"^\\[a-zA-Z]+");
            if (match.Success)
            {
                index += match.Length;
                return match.Value;
            }
        }

        char c = text[index++];
        return c.ToString();
    }

    private static string ReplaceKnownSymbols(string s)
    {
        string result = s;
        foreach (var (k, v) in SymbolsMap)
        {
            if (result.Contains(k))
            {
                result = result.Replace(k, v);
            }
        }
        return result;
    }
}
