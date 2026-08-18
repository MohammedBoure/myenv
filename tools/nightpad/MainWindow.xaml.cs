using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using NightPad.Services;

namespace NightPad;

public partial class MainWindow : Window
{
    private double _zoomFactor = 1.0;
    private const double BaseFontSize = 14.0;
    private string? _currentFilePath;
    private bool _isModified;
    private string _currentSyntaxName = "Plain Text";

    public MainWindow()
    {
        InitializeComponent();
        ConfigureEditor();
        InitializeSyntaxMenu();
        UpdateTitle();
        UpdateStatusBar();
    }

    private void ConfigureEditor()
    {
        MainEditor.Options.ConvertTabsToSpaces = true;
        MainEditor.Options.IndentationSize = 4;
        MainEditor.Options.EnableHyperlinks = false;
        MainEditor.Options.HighlightCurrentLine = true;
        MainEditor.Options.EnableRectangularSelection = true;
        MainEditor.Options.EnableTextDragDrop = true;

        MainEditor.LineNumbersForeground = (SolidColorBrush)FindResource("TextSecondaryBrush");
        MainEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(90, 31, 111, 235));
        MainEditor.TextArea.SelectionBorder = null;
        MainEditor.TextArea.SelectionCornerRadius = 0;

        MainEditor.TextChanged += (s, e) =>
        {
            if (!_isModified)
            {
                _isModified = true;
                UpdateTitle();
            }
            UpdateStatusBar();
        };

        MainEditor.TextArea.Caret.PositionChanged += (s, e) => UpdateStatusBar();
        MainEditor.TextArea.SelectionChanged += (s, e) => UpdateStatusBar();

        MainEditor.PreviewMouseWheel += (s, e) =>
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (e.Delta > 0) ZoomIn();
                else if (e.Delta < 0) ZoomOut();
            }
        };

        // Smart Python indentation & Enter handling
        MainEditor.TextArea.PreviewKeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                var curLine = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
                string lineText = MainEditor.Document.GetText(curLine.Offset, MainEditor.CaretOffset - curLine.Offset);

                int leadingSpaces = 0;
                while (leadingSpaces < lineText.Length && lineText[leadingSpaces] == ' ')
                {
                    leadingSpaces++;
                }

                // If line ends in colon (Python block), indent extra 4 spaces
                if (lineText.TrimEnd().EndsWith(":"))
                {
                    leadingSpaces += 4;
                }

                if (leadingSpaces > 0)
                {
                    e.Handled = true;
                    string indent = Environment.NewLine + new string(' ', leadingSpaces);
                    MainEditor.Document.Insert(MainEditor.CaretOffset, indent);
                }
            }
        };
    }

    private void UpdateTitle()
    {
        string fileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
        string prefix = _isModified ? "*" : "";
        string titleText = $"{prefix}{fileName} - Notepad";
        Title = titleText;
        TxtWindowTitle.Text = titleText;
    }

    #region File Operations

    public void OpenFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        if (_isModified && !PromptSaveBeforeAction())
            return;

        try
        {
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            MainEditor.Document.Text = content;
            _currentFilePath = Path.GetFullPath(filePath);
            _isModified = false;

            _currentSyntaxName = SyntaxService.GetLanguageByExtension(_currentFilePath);
            ApplySyntax(_currentSyntaxName);

            UpdateTitle();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NewFile()
    {
        if (_isModified && !PromptSaveBeforeAction())
            return;

        MainEditor.Document.Text = string.Empty;
        _currentFilePath = null;
        _isModified = false;
        _currentSyntaxName = "Plain Text";
        ApplySyntax(_currentSyntaxName);

        UpdateTitle();
        UpdateStatusBar();
    }

    public bool SaveFile()
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            return SaveAsFile();
        }

        try
        {
            File.WriteAllText(_currentFilePath, MainEditor.Document.Text, new UTF8Encoding(false));
            _isModified = false;
            UpdateTitle();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public bool SaveAsFile()
    {
        var dlg = new SaveFileDialog
        {
            FileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath),
            Filter = "All Files (*.*)|*.*|Python (*.py)|*.py|Text Documents (*.txt)|*.txt|PowerShell (*.ps1)|*.ps1|JSON (*.json)|*.json|Markdown (*.md)|*.md|C# (*.cs)|*.cs"
        };

        if (dlg.ShowDialog(this) == true)
        {
            try
            {
                File.WriteAllText(dlg.FileName, MainEditor.Document.Text, new UTF8Encoding(false));
                _currentFilePath = dlg.FileName;
                _isModified = false;

                _currentSyntaxName = SyntaxService.GetLanguageByExtension(_currentFilePath);
                ApplySyntax(_currentSyntaxName);

                UpdateTitle();
                UpdateStatusBar();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        return false;
    }

    private bool PromptSaveBeforeAction()
    {
        string fileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
        var result = MessageBox.Show(
            $"Do you want to save changes to {fileName}?",
            "Notepad",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel)
            return false;

        if (result == MessageBoxResult.Yes)
            return SaveFile();

        return true;
    }

    private void MenuNew_Click(object sender, RoutedEventArgs e) => NewFile();
    private void MenuOpen_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "All Supported Files (*.*)|*.*|Python (*.py;*.pyw)|*.py;*.pyw|Text Documents (*.txt)|*.txt|PowerShell (*.ps1)|*.ps1|JSON (*.json)|*.json|Markdown (*.md)|*.md|C# (*.cs)|*.cs|Web Files (*.html;*.css;*.js;*.ts)|*.html;*.css;*.js;*.ts"
        };

        if (dlg.ShowDialog(this) == true)
        {
            OpenFile(dlg.FileName);
        }
    }

    private void MenuSave_Click(object sender, RoutedEventArgs e) => SaveFile();
    private void MenuSaveAs_Click(object sender, RoutedEventArgs e) => SaveAsFile();
    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    #endregion

    #region Syntax Highlighting

    private void InitializeSyntaxMenu()
    {
        MenuSyntaxParent.Items.Clear();
        foreach (var lang in SyntaxService.SupportedLanguages)
        {
            var item = new MenuItem { Header = lang };
            item.Click += (s, e) =>
            {
                _currentSyntaxName = lang;
                ApplySyntax(lang);
                UpdateStatusBar();
            };
            MenuSyntaxParent.Items.Add(item);
        }
    }

    private void ApplySyntax(string syntaxName)
    {
        MainEditor.SyntaxHighlighting = SyntaxService.GetDefinition(syntaxName);
    }

    private void BtnLanguageSelector_Click(object sender, RoutedEventArgs e)
    {
        var contextMenu = new ContextMenu();
        foreach (var lang in SyntaxService.SupportedLanguages)
        {
            var item = new MenuItem { Header = lang };
            item.Click += (s, ev) =>
            {
                _currentSyntaxName = lang;
                ApplySyntax(lang);
                UpdateStatusBar();
            };
            contextMenu.Items.Add(item);
        }
        contextMenu.PlacementTarget = BtnLanguageSelector;
        contextMenu.IsOpen = true;
    }

    #endregion

    #region Search & Replace

    private void MenuFind_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Visible;
        ReplaceRow.Visibility = Visibility.Collapsed;
        TxtSearch.Focus();
        if (!string.IsNullOrEmpty(MainEditor.SelectedText))
        {
            TxtSearch.Text = MainEditor.SelectedText;
            TxtSearch.SelectAll();
        }
        UpdateMatchCount();
    }

    private void MenuReplace_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Visible;
        ReplaceRow.Visibility = Visibility.Visible;
        TxtSearch.Focus();
        if (!string.IsNullOrEmpty(MainEditor.SelectedText))
        {
            TxtSearch.Text = MainEditor.SelectedText;
            TxtSearch.SelectAll();
        }
        UpdateMatchCount();
    }

    private void BtnCloseSearch_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Collapsed;
        MainEditor.Focus();
    }

    private void SearchOption_Click(object sender, RoutedEventArgs e) => UpdateMatchCount();
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateMatchCount();

    private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                FindPrevious();
            else
                FindNext();
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            BtnCloseSearch_Click(sender, e);
        }
    }

    private void TxtReplace_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ReplaceCurrent();
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            BtnCloseSearch_Click(sender, e);
        }
    }

    private void BtnFindNext_Click(object sender, RoutedEventArgs e) => FindNext();
    private void BtnFindPrev_Click(object sender, RoutedEventArgs e) => FindPrevious();
    private void MenuFindNext_Click(object sender, RoutedEventArgs e) => FindNext();
    private void MenuFindPrev_Click(object sender, RoutedEventArgs e) => FindPrevious();
    private void BtnReplace_Click(object sender, RoutedEventArgs e) => ReplaceCurrent();
    private void BtnReplaceAll_Click(object sender, RoutedEventArgs e) => ReplaceAll();

    private void FindNext()
    {
        if (string.IsNullOrEmpty(TxtSearch.Text)) return;

        string pattern = TxtSearch.Text;
        string text = MainEditor.Document.Text;
        int startIndex = MainEditor.CaretOffset;

        var regex = GetSearchRegex(pattern);
        if (regex == null) return;

        var match = regex.Match(text, startIndex);
        if (!match.Success)
        {
            match = regex.Match(text, 0); // Wrap around
        }

        if (match.Success)
        {
            MainEditor.Select(match.Index, match.Length);
            MainEditor.ScrollTo(MainEditor.TextArea.Caret.Line, MainEditor.TextArea.Caret.Column);
        }
        UpdateMatchCount();
    }

    private void FindPrevious()
    {
        if (string.IsNullOrEmpty(TxtSearch.Text)) return;

        string pattern = TxtSearch.Text;
        string text = MainEditor.Document.Text;
        int selectionStart = MainEditor.SelectionStart;

        var regex = GetSearchRegex(pattern);
        if (regex == null) return;

        var matches = regex.Matches(text);
        Match? targetMatch = null;

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            if (matches[i].Index < selectionStart)
            {
                targetMatch = matches[i];
                break;
            }
        }

        if (targetMatch == null && matches.Count > 0)
        {
            targetMatch = matches[matches.Count - 1]; // Wrap around
        }

        if (targetMatch != null)
        {
            MainEditor.Select(targetMatch.Index, targetMatch.Length);
            MainEditor.ScrollTo(MainEditor.TextArea.Caret.Line, MainEditor.TextArea.Caret.Column);
        }
        UpdateMatchCount();
    }

    private void ReplaceCurrent()
    {
        if (string.IsNullOrEmpty(TxtSearch.Text)) return;

        if (MainEditor.SelectionLength > 0)
        {
            MainEditor.Document.Replace(MainEditor.SelectionStart, MainEditor.SelectionLength, TxtReplace.Text);
        }
        FindNext();
    }

    private void ReplaceAll()
    {
        if (string.IsNullOrEmpty(TxtSearch.Text)) return;

        var regex = GetSearchRegex(TxtSearch.Text);
        if (regex == null) return;

        string original = MainEditor.Document.Text;
        string replaced = regex.Replace(original, TxtReplace.Text);
        int count = regex.Matches(original).Count;

        if (count > 0)
        {
            MainEditor.Document.Text = replaced;
            MessageBox.Show($"Replaced {count} occurrence(s).", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        UpdateMatchCount();
    }

    private Regex? GetSearchRegex(string pattern)
    {
        try
        {
            RegexOptions options = RegexOptions.None;
            if (ChkMatchCase.IsChecked != true) options |= RegexOptions.IgnoreCase;

            string regexPattern = ChkRegex.IsChecked == true ? pattern : Regex.Escape(pattern);
            if (ChkWholeWord.IsChecked == true)
            {
                regexPattern = $@"\b{regexPattern}\b";
            }
            return new Regex(regexPattern, options);
        }
        catch
        {
            return null;
        }
    }

    private void UpdateMatchCount()
    {
        if (string.IsNullOrEmpty(TxtSearch.Text))
        {
            LblMatchCount.Text = "0 matches";
            return;
        }

        var regex = GetSearchRegex(TxtSearch.Text);
        if (regex == null)
        {
            LblMatchCount.Text = "Invalid regex";
            return;
        }

        int count = regex.Matches(MainEditor.Document.Text).Count;
        LblMatchCount.Text = $"{count} match{(count == 1 ? "" : "es")}";
    }

    #endregion

    #region Go To Line

    private void MenuGoToLine_Click(object sender, RoutedEventArgs e)
    {
        GoToLinePanel.Visibility = Visibility.Visible;
        TxtGoToLine.Text = MainEditor.TextArea.Caret.Line.ToString();
        TxtGoToLine.SelectAll();
        TxtGoToLine.Focus();
    }

    private void BtnCloseGoToLine_Click(object sender, RoutedEventArgs e)
    {
        GoToLinePanel.Visibility = Visibility.Collapsed;
        MainEditor.Focus();
    }

    private void BtnGoToLine_Click(object sender, RoutedEventArgs e) => ExecuteGoToLine();

    private void TxtGoToLine_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ExecuteGoToLine();
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            BtnCloseGoToLine_Click(sender, e);
        }
    }

    private void ExecuteGoToLine()
    {
        if (int.TryParse(TxtGoToLine.Text.Trim(), out int targetLine))
        {
            int clamped = Math.Clamp(targetLine, 1, MainEditor.Document.LineCount);
            MainEditor.ScrollToLine(clamped);
            MainEditor.TextArea.Caret.Line = clamped;
            MainEditor.TextArea.Caret.Column = 1;
            GoToLinePanel.Visibility = Visibility.Collapsed;
            MainEditor.Focus();
        }
    }

    #endregion

    #region Edit Actions & Shortcuts

    private void MenuUndo_Click(object sender, RoutedEventArgs e) => MainEditor.Undo();
    private void MenuRedo_Click(object sender, RoutedEventArgs e) => MainEditor.Redo();
    private void MenuCut_Click(object sender, RoutedEventArgs e) => MainEditor.Cut();
    private void MenuCopy_Click(object sender, RoutedEventArgs e) => MainEditor.Copy();
    private void MenuPaste_Click(object sender, RoutedEventArgs e) => MainEditor.Paste();
    private void MenuDelete_Click(object sender, RoutedEventArgs e) => MainEditor.Delete();
    private void MenuSelectAll_Click(object sender, RoutedEventArgs e) => MainEditor.SelectAll();

    private void MenuDuplicateLine_Click(object sender, RoutedEventArgs e)
    {
        var line = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
        string text = MainEditor.Document.GetText(line.Offset, line.Length);
        MainEditor.Document.Insert(line.EndOffset, Environment.NewLine + text);
    }

    private void MenuDeleteLine_Click(object sender, RoutedEventArgs e)
    {
        var line = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
        MainEditor.Document.Remove(line.Offset, line.TotalLength);
    }

    private void MenuMoveLineUp_Click(object sender, RoutedEventArgs e)
    {
        if (MainEditor.TextArea.Caret.Line <= 1) return;

        int curNum = MainEditor.TextArea.Caret.Line;
        var curLine = MainEditor.Document.GetLineByNumber(curNum);
        var prevLine = MainEditor.Document.GetLineByNumber(curNum - 1);

        string curText = MainEditor.Document.GetText(curLine.Offset, curLine.Length);
        string prevText = MainEditor.Document.GetText(prevLine.Offset, prevLine.Length);

        using (MainEditor.Document.RunUpdate())
        {
            MainEditor.Document.Replace(curLine.Offset, curLine.Length, prevText);
            MainEditor.Document.Replace(prevLine.Offset, prevLine.Length, curText);
        }
        MainEditor.TextArea.Caret.Line = curNum - 1;
    }

    private void MenuMoveLineDown_Click(object sender, RoutedEventArgs e)
    {
        if (MainEditor.TextArea.Caret.Line >= MainEditor.Document.LineCount) return;

        int curNum = MainEditor.TextArea.Caret.Line;
        var curLine = MainEditor.Document.GetLineByNumber(curNum);
        var nextLine = MainEditor.Document.GetLineByNumber(curNum + 1);

        string curText = MainEditor.Document.GetText(curLine.Offset, curLine.Length);
        string nextText = MainEditor.Document.GetText(nextLine.Offset, nextLine.Length);

        using (MainEditor.Document.RunUpdate())
        {
            MainEditor.Document.Replace(nextLine.Offset, nextLine.Length, curText);
            MainEditor.Document.Replace(curLine.Offset, curLine.Length, nextText);
        }
        MainEditor.TextArea.Caret.Line = curNum + 1;
    }

    private void MenuToggleComment_Click(object sender, RoutedEventArgs e)
    {
        string commentPrefix = _currentSyntaxName switch
        {
            "Python" or "PowerShell" or "YAML" or "INI / Config" => "# ",
            "SQL" => "-- ",
            "Batch" => ":: ",
            _ => "// "
        };

        var line = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
        string text = MainEditor.Document.GetText(line.Offset, line.Length);

        if (text.TrimStart().StartsWith(commentPrefix.Trim()))
        {
            int index = text.IndexOf(commentPrefix.Trim());
            MainEditor.Document.Remove(line.Offset + index, commentPrefix.Length);
        }
        else
        {
            MainEditor.Document.Insert(line.Offset, commentPrefix);
        }
    }

    private void MenuInsertDateTime_Click(object sender, RoutedEventArgs e)
    {
        string stamp = TextTransformService.GetCurrentTimestamp();
        MainEditor.Document.Insert(MainEditor.CaretOffset, stamp);
    }

    #endregion

    #region View & Zoom

    private void MenuWordWrap_Click(object sender, RoutedEventArgs e)
    {
        MainEditor.WordWrap = MenuWordWrap.IsChecked;
    }

    private void MenuLineNumbers_Click(object sender, RoutedEventArgs e)
    {
        MainEditor.ShowLineNumbers = MenuLineNumbers.IsChecked;
    }

    private void MenuStatusBarToggle_Click(object sender, RoutedEventArgs e)
    {
        StatusBarBorder.Visibility = MenuStatusBarToggle.IsChecked ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ZoomIn()
    {
        _zoomFactor = Math.Min(3.0, _zoomFactor + 0.1);
        ApplyZoom();
    }

    private void ZoomOut()
    {
        _zoomFactor = Math.Max(0.5, _zoomFactor - 0.1);
        ApplyZoom();
    }

    private void ZoomReset()
    {
        _zoomFactor = 1.0;
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        MainEditor.FontSize = BaseFontSize * _zoomFactor;
        int percent = (int)(_zoomFactor * 100);
        StatusZoom.Text = $"{percent}%";
    }

    private void MenuZoomIn_Click(object sender, RoutedEventArgs e) => ZoomIn();
    private void MenuZoomOut_Click(object sender, RoutedEventArgs e) => ZoomOut();
    private void MenuZoomReset_Click(object sender, RoutedEventArgs e) => ZoomReset();
    private void StatusZoom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ZoomReset();

    #endregion

    #region Tools & Transforms

    private void TransformSelectionOrAll(Func<string, string> transformFunc)
    {
        try
        {
            if (MainEditor.SelectionLength > 0)
            {
                string transformed = transformFunc(MainEditor.SelectedText);
                MainEditor.Document.Replace(MainEditor.SelectionStart, MainEditor.SelectionLength, transformed);
            }
            else
            {
                string transformed = transformFunc(MainEditor.Document.Text);
                MainEditor.Document.Text = transformed;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void MenuUpper_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToUpperCase);
    private void MenuLower_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToLowerCase);
    private void MenuTitleCase_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToTitleCase);
    private void MenuFormatJson_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.FormatJson);
    private void MenuMinifyJson_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.MinifyJson);
    private void MenuSortAsc_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(s => TextTransformService.SortLines(s, false));
    private void MenuSortDesc_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(s => TextTransformService.SortLines(s, true));
    private void MenuRemoveDuplicates_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.RemoveDuplicateLines);
    private void MenuRemoveEmptyLines_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.RemoveEmptyLines);
    private void MenuTrimWhitespace_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.TrimTrailingWhitespace);
    private void MenuBase64Encode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToBase64);
    private void MenuBase64Decode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.FromBase64);
    private void MenuUrlEncode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToUrlEncoded);
    private void MenuUrlDecode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.FromUrlEncoded);

    #endregion

    #region Window Drag, Controls, Shortcuts & Status Bar

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
        }
        else
        {
            DragMove();
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void BtnMaximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            BtnMaximize.Content = "□";
        }
        else
        {
            WindowState = WindowState.Maximized;
            BtnMaximize.Content = "❐";
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && File.Exists(files[0]))
            {
                OpenFile(files[0]);
            }
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.N) { e.Handled = true; NewFile(); }
            else if (e.Key == Key.O) { e.Handled = true; MenuOpen_Click(sender, e); }
            else if (e.Key == Key.S) { e.Handled = true; SaveFile(); }
            else if (e.Key == Key.F) { e.Handled = true; MenuFind_Click(sender, e); }
            else if (e.Key == Key.H) { e.Handled = true; MenuReplace_Click(sender, e); }
            else if (e.Key == Key.G) { e.Handled = true; MenuGoToLine_Click(sender, e); }
            else if (e.Key == Key.D) { e.Handled = true; MenuDuplicateLine_Click(sender, e); }
            else if (e.Key == Key.OemQuestion) { e.Handled = true; MenuToggleComment_Click(sender, e); }
            else if (e.Key == Key.Add || e.Key == Key.OemPlus) { e.Handled = true; ZoomIn(); }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus) { e.Handled = true; ZoomOut(); }
            else if (e.Key == Key.D0 || e.Key == Key.NumPad0) { e.Handled = true; ZoomReset(); }
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            if (e.Key == Key.S) { e.Handled = true; SaveAsFile(); }
            else if (e.Key == Key.L) { e.Handled = true; MenuLineNumbers.IsChecked = !MenuLineNumbers.IsChecked; MenuLineNumbers_Click(sender, e); }
            else if (e.Key == Key.K) { e.Handled = true; MenuDeleteLine_Click(sender, e); }
            else if (e.Key == Key.J) { e.Handled = true; MenuFormatJson_Click(sender, e); }
            else if (e.Key == Key.U) { e.Handled = true; MenuUpper_Click(sender, e); }
        }
        else if (Keyboard.Modifiers == ModifierKeys.Alt)
        {
            if (e.Key == Key.Z) { e.Handled = true; MenuWordWrap.IsChecked = !MenuWordWrap.IsChecked; MenuWordWrap_Click(sender, e); }
            else if (e.Key == Key.Up) { e.Handled = true; MenuMoveLineUp_Click(sender, e); }
            else if (e.Key == Key.Down) { e.Handled = true; MenuMoveLineDown_Click(sender, e); }
        }
        else if (e.Key == Key.F3)
        {
            e.Handled = true;
            if (Keyboard.Modifiers == ModifierKeys.Shift) FindPrevious();
            else FindNext();
        }
        else if (e.Key == Key.F5)
        {
            e.Handled = true;
            MenuInsertDateTime_Click(sender, e);
        }
    }

    private void UpdateStatusBar()
    {
        int line = MainEditor.TextArea.Caret.Line;
        int col = MainEditor.TextArea.Caret.Column;
        StatusCaretPos.Text = $"Ln {line}, Col {col}";

        if (MainEditor.SelectionLength > 0)
        {
            int selLines = string.IsNullOrEmpty(MainEditor.SelectedText) ? 0 : MainEditor.SelectedText.Split('\n').Length;
            StatusSelection.Text = $"Sel: {MainEditor.SelectionLength} chars ({selLines} lines)";
            StatusSelection.Visibility = Visibility.Visible;
        }
        else
        {
            StatusSelection.Visibility = Visibility.Collapsed;
        }

        string text = MainEditor.Document.Text;
        int lineCount = MainEditor.Document.LineCount;
        int charCount = text.Length;

        int words = 0;
        bool inWord = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i])) inWord = false;
            else if (!inWord) { inWord = true; words++; }
        }

        StatusDocStats.Text = $"{lineCount:N0} line{(lineCount == 1 ? "" : "s")}, {words:N0} words, {charCount:N0} chars";

        if (text.Contains("\r\n")) StatusEol.Text = "Windows (CRLF)";
        else if (text.Contains("\n")) StatusEol.Text = "Unix (LF)";
        else StatusEol.Text = "Windows (CRLF)";

        BtnLanguageSelector.Content = _currentSyntaxName;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_isModified && !PromptSaveBeforeAction())
        {
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }

    #endregion
}