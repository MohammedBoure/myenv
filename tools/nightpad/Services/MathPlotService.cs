using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NightPad.Services;

/// <summary>
/// Native high-performance service for parsing and rendering mathematical function graphs and Cartesian coordinate plots in WPF.
/// </summary>
public static class MathPlotService
{
    private static readonly SolidColorBrush CardBgBrush = new(Color.FromRgb(0x16, 0x1B, 0x22));
    private static readonly SolidColorBrush CanvasBgBrush = new(Color.FromRgb(0x0D, 0x11, 0x17));
    private static readonly SolidColorBrush BorderDarkBrush = new(Color.FromRgb(0x30, 0x36, 0x3D));
    private static readonly SolidColorBrush HeaderBgBrush = new(Color.FromRgb(0x21, 0x26, 0x2D));
    private static readonly SolidColorBrush GridLineBrush = new(Color.FromRgb(0x21, 0x26, 0x2D));
    private static readonly SolidColorBrush AxisLineBrush = new(Color.FromRgb(0x48, 0x4F, 0x58));
    private static readonly SolidColorBrush TextMutedBrush = new(Color.FromRgb(0x8B, 0x94, 0x9E));
    private static readonly SolidColorBrush AccentBlueBrush = new(Color.FromRgb(0x58, 0xA6, 0xFF));
    private static readonly FontFamily CodeFontFamily = new("Cascadia Code, Consolas, Courier New");

    private static readonly Color[] FunctionColors =
    [
        Color.FromRgb(0x58, 0xA6, 0xFF), // Neon Blue
        Color.FromRgb(0x3F, 0xB9, 0x50), // Mint Green
        Color.FromRgb(0xFF, 0xA6, 0x57), // Warm Amber
        Color.FromRgb(0xBC, 0x8C, 0xFF), // Lavender Purple
        Color.FromRgb(0xF7, 0x78, 0xBA), // Rose Pink
        Color.FromRgb(0x39, 0xC5, 0xCF)  // Cyan
    ];

    public record PlotFunction(string Name, string RawExpression, Func<double, double> Evaluator, Color Color);

    /// <summary>
    /// Determines whether a code block language corresponds to mathematical plotting.
    /// </summary>
    public static bool IsPlotLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language)) return false;
        string l = language.Trim().ToLowerInvariant();
        return l is "plot" or "graph" or "math-plot" or "mathgraph" or "mathplot" or "function";
    }

    /// <summary>
    /// Parses plot specification lines and generates a WPF visual block container.
    /// </summary>
    public static Block CreatePlotBlock(List<string> lines)
    {
        string title = "Mathematical Plot";
        double xMin = -10.0;
        double xMax = 10.0;
        double? customYMin = null;
        double? customYMax = null;

        var functions = new List<PlotFunction>();
        int colorIdx = 0;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith("//"))
                continue;

            // Title
            if (line.StartsWith("title:", StringComparison.OrdinalIgnoreCase))
            {
                title = line[6..].Trim();
                continue;
            }

            // X Range: range: -10, 10 or x: -5, 5 or x-range: -5 to 5
            if (line.StartsWith("range:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("x-range:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("x:", StringComparison.OrdinalIgnoreCase))
            {
                int colonIdx = line.IndexOf(':');
                string rangeStr = line[(colonIdx + 1)..].Trim();
                var parts = Regex.Split(rangeStr, @"[,\s+to]+");
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double min) &&
                    double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double max) &&
                    max > min)
                {
                    xMin = min;
                    xMax = max;
                }
                continue;
            }

            // Y Range: y-range: -5, 5 or y: -2 to 2
            if (line.StartsWith("y-range:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("y:", StringComparison.OrdinalIgnoreCase))
            {
                int colonIdx = line.IndexOf(':');
                string rangeStr = line[(colonIdx + 1)..].Trim();
                var parts = Regex.Split(rangeStr, @"[,\s+to]+");
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double ymin) &&
                    double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double ymax) &&
                    ymax > ymin)
                {
                    customYMin = ymin;
                    customYMax = ymax;
                }
                continue;
            }

            // Function expression: y = sin(x) or f(x) = x^2 - 4 or sin(x)
            string funcName = $"f{functions.Count + 1}(x)";
            string exprStr = line;

            var matchEq = Regex.Match(line, @"^([a-zA-Z][a-zA-Z0-9]*(?:\s*\(\s*x\s*\))?|[yY])\s*=\s*(.+)$");
            if (matchEq.Success)
            {
                funcName = matchEq.Groups[1].Value.Trim();
                exprStr = matchEq.Groups[2].Value.Trim();
            }

            try
            {
                var evaluator = MathExpressionParser.Compile(exprStr);
                var color = FunctionColors[colorIdx % FunctionColors.Length];
                colorIdx++;
                functions.Add(new PlotFunction(funcName, exprStr, evaluator, color));
            }
            catch
            {
                // Skip invalid function expressions
            }
        }

        // If no functions provided, provide a default wave demo
        if (functions.Count == 0)
        {
            functions.Add(new PlotFunction("f(x)", "sin(x)", Math.Sin, FunctionColors[0]));
        }

        var plotControl = BuildPlotVisual(title, functions, xMin, xMax, customYMin, customYMax);
        return new BlockUIContainer(plotControl)
        {
            Margin = new Thickness(0, 8, 0, 12)
        };
    }

    private static FrameworkElement BuildPlotVisual(
        string title,
        List<PlotFunction> functions,
        double xMin,
        double xMax,
        double? customYMin,
        double? customYMax)
    {
        const double Width = 600;
        const double Height = 340;
        const double MarginLeft = 45;
        const double MarginRight = 20;
        const double MarginTop = 20;
        const double MarginBottom = 35;

        double plotW = Width - MarginLeft - MarginRight;
        double plotH = Height - MarginTop - MarginBottom;

        // Auto-scale Y if not explicitly set
        double yMin = customYMin ?? -5.0;
        double yMax = customYMax ?? 5.0;

        if (!customYMin.HasValue || !customYMax.HasValue)
        {
            double calculatedMin = double.MaxValue;
            double calculatedMax = double.MinValue;
            const int sampleCount = 200;

            foreach (var fn in functions)
            {
                for (int i = 0; i <= sampleCount; i++)
                {
                    double x = xMin + (xMax - xMin) * i / sampleCount;
                    try
                    {
                        double y = fn.Evaluator(x);
                        if (!double.IsNaN(y) && !double.IsInfinity(y) && Math.Abs(y) < 1000)
                        {
                            if (y < calculatedMin) calculatedMin = y;
                            if (y > calculatedMax) calculatedMax = y;
                        }
                    }
                    catch { }
                }
            }

            if (calculatedMin < calculatedMax)
            {
                double pad = (calculatedMax - calculatedMin) * 0.15;
                if (pad < 0.5) pad = 1.0;
                if (!customYMin.HasValue) yMin = Math.Floor((calculatedMin - pad) * 2) / 2;
                if (!customYMax.HasValue) yMax = Math.Ceiling((calculatedMax + pad) * 2) / 2;
            }
        }

        if (yMax <= yMin)
        {
            yMax = yMin + 2.0;
        }

        double XToScreen(double x) => MarginLeft + (x - xMin) / (xMax - xMin) * plotW;
        double YToScreen(double y) => MarginTop + (yMax - y) / (yMax - yMin) * plotH;
        double ScreenToX(double px) => xMin + (px - MarginLeft) / plotW * (xMax - xMin);
        double ScreenToY(double py) => yMax - (py - MarginTop) / plotH * (yMax - yMin);

        // Root Card
        var outerCard = new Border
        {
            Background = CardBgBrush,
            BorderBrush = BorderDarkBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            MaxWidth = Width + 20,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var mainLayout = new Grid();
        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Canvas plot
        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Legend
        outerCard.Child = mainLayout;

        // Toolbar Header
        var headerBorder = new Border
        {
            Background = HeaderBgBrush,
            BorderBrush = BorderDarkBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            CornerRadius = new CornerRadius(5, 5, 0, 0),
            Padding = new Thickness(10, 5, 10, 5)
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerBorder.Child = headerGrid;

        var titleBlock = new TextBlock
        {
            Text = $"📈 {title}",
            Foreground = AccentBlueBrush,
            FontWeight = FontWeights.Bold,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleBlock, 0);
        headerGrid.Children.Add(titleBlock);

        var rightHeaderStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var coordDisplay = new TextBlock
        {
            Text = "X: 0.00, Y: 0.00",
            Foreground = TextMutedBrush,
            FontFamily = CodeFontFamily,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        rightHeaderStack.Children.Add(coordDisplay);

        var btnCopyPlot = new Button
        {
            Content = "📋 Copy Plot",
            ToolTip = "Copy plot image to clipboard / نسخ الرسم البياني",
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D)),
            Foreground = AccentBlueBrush,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(8, 2, 8, 2),
            FontSize = 11,
            Cursor = Cursors.Hand
        };
        rightHeaderStack.Children.Add(btnCopyPlot);

        Grid.SetColumn(rightHeaderStack, 1);
        headerGrid.Children.Add(rightHeaderStack);

        Grid.SetRow(headerBorder, 0);
        mainLayout.Children.Add(headerBorder);

        // Plot Canvas
        var canvas = new Canvas
        {
            Width = Width,
            Height = Height,
            Background = CanvasBgBrush,
            ClipToBounds = true,
            Cursor = Cursors.Cross
        };

        // Grid Lines and Ticks
        DrawGridAndAxes(canvas, Width, Height, MarginLeft, MarginRight, MarginTop, MarginBottom, xMin, xMax, yMin, yMax, XToScreen, YToScreen);

        // Plot Function Curves
        foreach (var fn in functions)
        {
            DrawCurve(canvas, fn, xMin, xMax, yMin, yMax, XToScreen, YToScreen);
        }

        // Crosshair cursor tracking
        var crosshairX = new Line
        {
            Stroke = new SolidColorBrush(Color.FromArgb(90, 88, 166, 255)),
            StrokeThickness = 1,
            StrokeDashArray = [2, 2],
            Visibility = Visibility.Collapsed
        };
        var crosshairY = new Line
        {
            Stroke = new SolidColorBrush(Color.FromArgb(90, 88, 166, 255)),
            StrokeThickness = 1,
            StrokeDashArray = [2, 2],
            Visibility = Visibility.Collapsed
        };
        canvas.Children.Add(crosshairX);
        canvas.Children.Add(crosshairY);

        canvas.MouseMove += (s, e) =>
        {
            var pos = e.GetPosition(canvas);
            if (pos.X >= MarginLeft && pos.X <= Width - MarginRight &&
                pos.Y >= MarginTop && pos.Y <= Height - MarginBottom)
            {
                double mathX = ScreenToX(pos.X);
                double mathY = ScreenToY(pos.Y);
                coordDisplay.Text = $"X: {mathX:F2}, Y: {mathY:F2}";

                crosshairX.X1 = pos.X;
                crosshairX.Y1 = MarginTop;
                crosshairX.X2 = pos.X;
                crosshairX.Y2 = Height - MarginBottom;
                crosshairX.Visibility = Visibility.Visible;

                crosshairY.X1 = MarginLeft;
                crosshairY.Y1 = pos.Y;
                crosshairY.X2 = Width - MarginRight;
                crosshairY.Y2 = pos.Y;
                crosshairY.Visibility = Visibility.Visible;
            }
            else
            {
                crosshairX.Visibility = Visibility.Collapsed;
                crosshairY.Visibility = Visibility.Collapsed;
            }
        };

        canvas.MouseLeave += (s, e) =>
        {
            crosshairX.Visibility = Visibility.Collapsed;
            crosshairY.Visibility = Visibility.Collapsed;
            coordDisplay.Text = "X: 0.00, Y: 0.00";
        };

        // Copy Plot Button Handler
        btnCopyPlot.Click += (s, e) =>
        {
            try
            {
                var rtb = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32);
                crosshairX.Visibility = Visibility.Collapsed;
                crosshairY.Visibility = Visibility.Collapsed;
                rtb.Render(canvas);
                Clipboard.SetImage(rtb);

                btnCopyPlot.Content = "✓ Copied!";
                btnCopyPlot.Foreground = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50));
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                timer.Tick += (ts, te) =>
                {
                    timer.Stop();
                    btnCopyPlot.Content = "📋 Copy Plot";
                    btnCopyPlot.Foreground = AccentBlueBrush;
                };
                timer.Start();
            }
            catch { }
        };

        Grid.SetRow(canvas, 1);
        mainLayout.Children.Add(canvas);

        // Legend Bottom Bar
        var legendPanel = new WrapPanel
        {
            Margin = new Thickness(10, 8, 10, 8),
            Orientation = Orientation.Horizontal
        };

        foreach (var fn in functions)
        {
            var badge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x21, 0x26, 0x2D)),
                BorderBrush = new SolidColorBrush(fn.Color),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 8, 2),
                Margin = new Thickness(0, 0, 8, 4)
            };

            var badgeStack = new StackPanel { Orientation = Orientation.Horizontal };
            var colorSquare = new Rectangle
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(fn.Color),
                RadiusX = 2,
                RadiusY = 2,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var badgeText = new TextBlock
            {
                Text = $"{fn.Name} = {fn.RawExpression}",
                Foreground = new SolidColorBrush(Color.FromRgb(0xF0, 0xF6, 0xFC)),
                FontFamily = CodeFontFamily,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };

            badgeStack.Children.Add(colorSquare);
            badgeStack.Children.Add(badgeText);
            badge.Child = badgeStack;
            legendPanel.Children.Add(badge);
        }

        Grid.SetRow(legendPanel, 2);
        mainLayout.Children.Add(legendPanel);

        return outerCard;
    }

    private static void DrawGridAndAxes(
        Canvas canvas,
        double width,
        double height,
        double mLeft,
        double mRight,
        double mTop,
        double mBottom,
        double xMin,
        double xMax,
        double yMin,
        double yMax,
        Func<double, double> XToScreen,
        Func<double, double> YToScreen)
    {
        double plotW = width - mLeft - mRight;
        double plotH = height - mTop - mBottom;

        // Compute sensible step increments for grid
        double xStep = ComputeStep(xMax - xMin, 8);
        double yStep = ComputeStep(yMax - yMin, 6);

        // Vertical grid lines and X labels
        double firstX = Math.Ceiling(xMin / xStep) * xStep;
        for (double x = firstX; x <= xMax + 1e-9; x += xStep)
        {
            double sx = XToScreen(x);
            if (sx < mLeft || sx > width - mRight) continue;

            var gLine = new Line
            {
                X1 = sx,
                Y1 = mTop,
                X2 = sx,
                Y2 = height - mBottom,
                Stroke = GridLineBrush,
                StrokeThickness = 1
            };
            canvas.Children.Add(gLine);

            var lbl = new TextBlock
            {
                Text = Math.Abs(x) < 1e-9 ? "0" : x.ToString("G4", CultureInfo.InvariantCulture),
                Foreground = TextMutedBrush,
                FontSize = 9.5,
                FontFamily = CodeFontFamily
            };
            lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(lbl, sx - lbl.DesiredSize.Width / 2);
            Canvas.SetTop(lbl, height - mBottom + 4);
            canvas.Children.Add(lbl);
        }

        // Horizontal grid lines and Y labels
        double firstY = Math.Ceiling(yMin / yStep) * yStep;
        for (double y = firstY; y <= yMax + 1e-9; y += yStep)
        {
            double sy = YToScreen(y);
            if (sy < mTop || sy > height - mBottom) continue;

            var gLine = new Line
            {
                X1 = mLeft,
                Y1 = sy,
                X2 = width - mRight,
                Y2 = sy,
                Stroke = GridLineBrush,
                StrokeThickness = 1
            };
            canvas.Children.Add(gLine);

            var lbl = new TextBlock
            {
                Text = Math.Abs(y) < 1e-9 ? "0" : y.ToString("G4", CultureInfo.InvariantCulture),
                Foreground = TextMutedBrush,
                FontSize = 9.5,
                FontFamily = CodeFontFamily,
                TextAlignment = TextAlignment.Right
            };
            lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(lbl, mLeft - lbl.DesiredSize.Width - 5);
            Canvas.SetTop(lbl, sy - lbl.DesiredSize.Height / 2);
            canvas.Children.Add(lbl);
        }

        // Main X-Axis (y = 0)
        double zeroY = YToScreen(0);
        if (zeroY >= mTop && zeroY <= height - mBottom)
        {
            var axisX = new Line
            {
                X1 = mLeft,
                Y1 = zeroY,
                X2 = width - mRight,
                Y2 = zeroY,
                Stroke = AxisLineBrush,
                StrokeThickness = 1.5
            };
            canvas.Children.Add(axisX);
        }

        // Main Y-Axis (x = 0)
        double zeroX = XToScreen(0);
        if (zeroX >= mLeft && zeroX <= width - mRight)
        {
            var axisY = new Line
            {
                X1 = zeroX,
                Y1 = mTop,
                X2 = zeroX,
                Y2 = height - mBottom,
                Stroke = AxisLineBrush,
                StrokeThickness = 1.5
            };
            canvas.Children.Add(axisY);
        }

        // Border rectangle around plotting area
        var plotBorder = new Rectangle
        {
            Width = plotW,
            Height = plotH,
            Stroke = BorderDarkBrush,
            StrokeThickness = 1
        };
        Canvas.SetLeft(plotBorder, mLeft);
        Canvas.SetTop(plotBorder, mTop);
        canvas.Children.Add(plotBorder);
    }

    private static void DrawCurve(
        Canvas canvas,
        PlotFunction fn,
        double xMin,
        double xMax,
        double yMin,
        double yMax,
        Func<double, double> XToScreen,
        Func<double, double> YToScreen)
    {
        const int Steps = 450;
        double yRange = yMax - yMin;

        var pathFigureCollection = new PathFigureCollection();
        PathFigure? currentFigure = null;
        double? prevY = null;

        for (int i = 0; i <= Steps; i++)
        {
            double x = xMin + (xMax - xMin) * i / Steps;
            double y;
            try
            {
                y = fn.Evaluator(x);
            }
            catch
            {
                y = double.NaN;
            }

            if (double.IsNaN(y) || double.IsInfinity(y) || y < yMin - yRange * 3 || y > yMax + yRange * 3)
            {
                currentFigure = null;
                prevY = null;
                continue;
            }

            // Check for large asymptotic jumps (e.g. tan(x))
            if (prevY.HasValue && Math.Abs(y - prevY.Value) > yRange * 0.75 && (y * prevY.Value < 0))
            {
                currentFigure = null;
            }

            double sx = XToScreen(x);
            double sy = YToScreen(y);
            var pt = new Point(sx, sy);

            if (currentFigure == null)
            {
                currentFigure = new PathFigure
                {
                    StartPoint = pt,
                    IsClosed = false
                };
                pathFigureCollection.Add(currentFigure);
            }
            else
            {
                currentFigure.Segments.Add(new LineSegment(pt, true));
            }

            prevY = y;
        }

        var pathGeometry = new PathGeometry(pathFigureCollection);
        var path = new Path
        {
            Data = pathGeometry,
            Stroke = new SolidColorBrush(fn.Color),
            StrokeThickness = 2.2,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round
        };
        canvas.Children.Add(path);
    }

    private static double ComputeStep(double range, int targetSteps)
    {
        double roughStep = range / targetSteps;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughStep)));
        double normalized = roughStep / magnitude;

        double step;
        if (normalized < 1.5) step = 1 * magnitude;
        else if (normalized < 3.5) step = 2 * magnitude;
        else if (normalized < 7.5) step = 5 * magnitude;
        else step = 10 * magnitude;

        return Math.Max(step, 0.0001);
    }
}

/// <summary>
/// Lightweight, recursive mathematical expression compiler supporting variables, functions, and standard operators.
/// </summary>
public static class MathExpressionParser
{
    public static Func<double, double> Compile(string expression)
    {
        string sanitized = Preprocess(expression);
        var tokens = Tokenize(sanitized);
        int pos = 0;
        var node = ParseExpression(tokens, ref pos);
        return x => node.Evaluate(x);
    }

    private static string Preprocess(string expr)
    {
        string s = expr.Trim();
        // Insert implicit multiplication: e.g. 2x -> 2*x, 3(x) -> 3*(x), x(x) -> x*(x)
        s = Regex.Replace(s, @"(?<=\d)(?=[a-zA-Z\(])", "*");
        s = Regex.Replace(s, @"(?<=\))(?=[a-zA-Z0-9\(])", "*");
        s = Regex.Replace(s, @"(?<=[xX])(?=[a-zA-Z\(])", "*");
        return s;
    }

    private static List<string> Tokenize(string expr)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < expr.Length)
        {
            char c = expr[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                    i++;
                tokens.Add(expr[start..i]);
                continue;
            }

            if (char.IsLetter(c))
            {
                int start = i;
                while (i < expr.Length && (char.IsLetterOrDigit(expr[i]) || expr[i] == '_'))
                    i++;
                tokens.Add(expr[start..i]);
                continue;
            }

            if ("+-*/^%()".Contains(c))
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            i++;
        }
        return tokens;
    }

    private abstract class Node
    {
        public abstract double Evaluate(double x);
    }

    private class NumberNode(double val) : Node
    {
        public override double Evaluate(double x) => val;
    }

    private class VariableNode : Node
    {
        public override double Evaluate(double x) => x;
    }

    private class UnaryNode(string op, Node child) : Node
    {
        public override double Evaluate(double x)
        {
            double v = child.Evaluate(x);
            return op == "-" ? -v : v;
        }
    }

    private class BinaryNode(Node left, string op, Node right) : Node
    {
        public override double Evaluate(double x)
        {
            double a = left.Evaluate(x);
            double b = right.Evaluate(x);
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => Math.Abs(b) < 1e-12 ? double.NaN : a / b,
                "%" => a % b,
                "^" => Math.Pow(a, b),
                _ => 0
            };
        }
    }

    private class FunctionNode(string name, Node arg) : Node
    {
        public override double Evaluate(double x)
        {
            double v = arg.Evaluate(x);
            return name.ToLowerInvariant() switch
            {
                "sin" => Math.Sin(v),
                "cos" => Math.Cos(v),
                "tan" => Math.Tan(v),
                "asin" => Math.Asin(v),
                "acos" => Math.Acos(v),
                "atan" => Math.Atan(v),
                "sinh" => Math.Sinh(v),
                "cosh" => Math.Cosh(v),
                "tanh" => Math.Tanh(v),
                "sqrt" => v < 0 ? double.NaN : Math.Sqrt(v),
                "cbrt" => Math.Cbrt(v),
                "abs" => Math.Abs(v),
                "exp" => Math.Exp(v),
                "ln" or "log" => v <= 0 ? double.NaN : Math.Log(v),
                "log10" => v <= 0 ? double.NaN : Math.Log10(v),
                "log2" => v <= 0 ? double.NaN : Math.Log2(v),
                "floor" => Math.Floor(v),
                "ceil" => Math.Ceiling(v),
                "round" => Math.Round(v),
                "sign" => Math.Sign(v),
                _ => v
            };
        }
    }

    private static Node ParseExpression(List<string> tokens, ref int pos)
    {
        var node = ParseTerm(tokens, ref pos);
        while (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-"))
        {
            string op = tokens[pos++];
            var right = ParseTerm(tokens, ref pos);
            node = new BinaryNode(node, op, right);
        }
        return node;
    }

    private static Node ParseTerm(List<string> tokens, ref int pos)
    {
        var node = ParsePower(tokens, ref pos);
        while (pos < tokens.Count && (tokens[pos] == "*" || tokens[pos] == "/" || tokens[pos] == "%"))
        {
            string op = tokens[pos++];
            var right = ParsePower(tokens, ref pos);
            node = new BinaryNode(node, op, right);
        }
        return node;
    }

    private static Node ParsePower(List<string> tokens, ref int pos)
    {
        var node = ParseUnary(tokens, ref pos);
        if (pos < tokens.Count && tokens[pos] == "^")
        {
            pos++;
            var right = ParsePower(tokens, ref pos); // Right associative
            node = new BinaryNode(node, "^", right);
        }
        return node;
    }

    private static Node ParseUnary(List<string> tokens, ref int pos)
    {
        if (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-"))
        {
            string op = tokens[pos++];
            var child = ParseUnary(tokens, ref pos);
            return new UnaryNode(op, child);
        }
        return ParsePrimary(tokens, ref pos);
    }

    private static Node ParsePrimary(List<string> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
            return new NumberNode(0);

        string token = tokens[pos++];

        if (token == "(")
        {
            var node = ParseExpression(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == ")")
                pos++;
            return node;
        }

        if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
        {
            return new NumberNode(num);
        }

        string lower = token.ToLowerInvariant();
        if (lower is "x")
        {
            return new VariableNode();
        }
        if (lower is "pi")
        {
            return new NumberNode(Math.PI);
        }
        if (lower is "e")
        {
            return new NumberNode(Math.E);
        }
        if (lower is "tau")
        {
            return new NumberNode(Math.PI * 2);
        }

        // Functions: sin(x), sqrt(x), etc.
        if (pos < tokens.Count && tokens[pos] == "(")
        {
            pos++; // consume '('
            var arg = ParseExpression(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == ")")
                pos++; // consume ')'
            return new FunctionNode(lower, arg);
        }

        return new NumberNode(0);
    }
}
