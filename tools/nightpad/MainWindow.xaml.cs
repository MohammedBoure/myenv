using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using NightPad.Models;
using NightPad.Services;

namespace NightPad;

public partial class MainWindow : Window
{
    private double _zoomFactor = 1.0;
    private const double BaseFontSize = 14.0;
    private int _untitledCounter = 1;
    private EditorDocument? _activeDocument;
    private bool _isMarkdownPreviewActive = false;

    public ObservableCollection<EditorDocument> Documents { get; } = new();

    public EditorDocument? ActiveDocument
    {
        get => _activeDocument;
        set
        {
            if (_activeDocument != value)
            {
                _activeDocument = value;
                OnActiveDocumentChanged();
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        InitializeSyntaxMenu();

        // Create initial document
        CreateNewTab();
    }

    #region Tab & Document Management

    public EditorDocument CreateNewTab(string? filePath = null, string? content = null, string? title = null)
    {
        string tabTitle = title ?? (string.IsNullOrEmpty(filePath) ? $"Untitled-{_untitledCounter++}" : Path.GetFileName(filePath));
        var doc = new EditorDocument(filePath, content, tabTitle);

        var editor = CreateConfiguredEditor(doc);
        doc.Editor = editor;

        if (!string.IsNullOrEmpty(filePath))
        {
            doc.SyntaxName = SyntaxService.GetLanguageByExtension(filePath);
            ApplySyntax(editor, doc.SyntaxName);
        }

        Documents.Add(doc);
        ActiveDocument = doc;
        return doc;
    }

    private TextEditor CreateConfiguredEditor(EditorDocument doc)
    {
        var editor = new TextEditor
        {
            Document = doc.Document,
            Background = (SolidColorBrush)FindResource("BgEditorBrush"),
            Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush"),
            FontFamily = new FontFamily("Cascadia Code, JetBrains Mono, Consolas, Courier New"),
            FontSize = BaseFontSize * _zoomFactor,
            ShowLineNumbers = MenuLineNumbers?.IsChecked ?? true,
            WordWrap = MenuWordWrap?.IsChecked ?? false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(6, 4, 6, 4)
        };

        // Editor Options
        editor.Options.ConvertTabsToSpaces = true;
        editor.Options.IndentationSize = 4;
        editor.Options.EnableHyperlinks = false;
        editor.Options.HighlightCurrentLine = true;
        editor.Options.EnableRectangularSelection = true;
        editor.Options.EnableTextDragDrop = true;

        // Custom Gutter & Styling
        editor.LineNumbersForeground = (SolidColorBrush)FindResource("TextSecondaryBrush");
        editor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(90, 31, 111, 235));
        editor.TextArea.SelectionBorder = null;
        editor.TextArea.SelectionCornerRadius = 0;

        // Event handlers
        editor.TextChanged += (s, e) =>
        {
            doc.IsModified = true;
            doc.UpdateStatistics();
            UpdateStatusBar();
            if (_isMarkdownPreviewActive) UpdateMarkdownPreview();
        };

        editor.TextArea.Caret.PositionChanged += (s, e) =>
        {
            doc.CaretLine = editor.TextArea.Caret.Line;
            doc.CaretColumn = editor.TextArea.Caret.Column;
            UpdateStatusBar();
        };

        editor.TextArea.SelectionChanged += (s, e) =>
        {
            doc.SelectionLength = editor.SelectionLength;
            doc.SelectionLines = string.IsNullOrEmpty(editor.SelectedText) ? 0 : editor.SelectedText.Split('\n').Length;
            UpdateStatusBar();
        };

        editor.PreviewMouseWheel += (s, e) =>
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (e.Delta > 0) ZoomIn();
                else if (e.Delta < 0) ZoomOut();
            }
        };

        return editor;
    }

    private void OnActiveDocumentChanged()
    {
        if (ActiveDocument?.Editor != null)
        {
            EditorHost.Content = ActiveDocument.Editor;
            ActiveDocument.Editor.Focus();
            UpdateStatusBar();

            if (_isMarkdownPreviewActive)
            {
                UpdateMarkdownPreview();
            }
        }
        else
        {
            EditorHost.Content = null;
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private void Tab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.DataContext is EditorDocument doc)
        {
            ActiveDocument = doc;
        }
    }

    private void Tab_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.DataContext is EditorDocument doc)
        {
            ActiveDocument = doc;
        }
    }

    private void TabCloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.DataContext is EditorDocument doc)
        {
            CloseDocument(doc);
        }
    }

    private void NewTabButton_Click(object sender, RoutedEventArgs e)
    {
        CreateNewTab();
    }

    public bool CloseDocument(EditorDocument doc)
    {
        if (doc.IsModified)
        {
            var result = MessageBox.Show(
                $"Do you want to save changes to '{doc.FileName}'?",
                "NightPad",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.Yes)
            {
                if (!SaveDocument(doc))
                    return false;
            }
        }

        int index = Documents.IndexOf(doc);
        Documents.Remove(doc);

        if (Documents.Count == 0)
        {
            CreateNewTab();
        }
        else if (ActiveDocument == doc)
        {
            int newIndex = Math.Min(index, Documents.Count - 1);
            ActiveDocument = Documents[newIndex];
        }

        return true;
    }

    #endregion

    #region File Operations

    public void OpenFile(string filePath)
    {
        // Check if already open
        var existing = Documents.FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            ActiveDocument = existing;
            return;
        }

        try
        {
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            
            // If current tab is single untouched Untitled-1, replace it
            if (Documents.Count == 1 && Documents[0].FilePath == null && !Documents[0].IsModified && Documents[0].CharCount == 0)
            {
                var doc = Documents[0];
                doc.FilePath = filePath;
                doc.Document.Text = content;
                doc.IsModified = false;
                doc.SyntaxName = SyntaxService.GetLanguageByExtension(filePath);
                ApplySyntax(doc.Editor, doc.SyntaxName);
                doc.UpdateStatistics();
                UpdateStatusBar();
                ActiveDocument = doc;
            }
            else
            {
                CreateNewTab(filePath, content);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MenuOpen_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "All Supported Files (*.*)|*.*|Text Files (*.txt)|*.txt|PowerShell (*.ps1;*.psm1)|*.ps1;*.psm1|Python (*.py)|*.py|JSON (*.json)|*.json|Markdown (*.md)|*.md|C# (*.cs)|*.cs|Web Files (*.html;*.css;*.js)|*.html;*.css;*.js",
            Multiselect = true
        };

        if (dlg.ShowDialog(this) == true)
        {
            foreach (string file in dlg.FileNames)
            {
                OpenFile(file);
            }
        }
    }

    private void MenuNew_Click(object sender, RoutedEventArgs e) => CreateNewTab();

    public bool SaveDocument(EditorDocument? doc)
    {
        if (doc == null) return false;

        if (string.IsNullOrEmpty(doc.FilePath))
        {
            return SaveAsDocument(doc);
        }

        try
        {
            File.WriteAllText(doc.FilePath, doc.Document.Text, new UTF8Encoding(false));
            doc.IsModified = false;
            doc.UpdateTitle();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save file:\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public bool SaveAsDocument(EditorDocument? doc)
    {
        if (doc == null) return false;

        var dlg = new SaveFileDialog
        {
            FileName = doc.FileName.Replace(" *", ""),
            Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt|PowerShell (*.ps1)|*.ps1|Python (*.py)|*.py|JSON (*.json)|*.json|Markdown (*.md)|*.md|C# (*.cs)|*.cs"
        };

        if (dlg.ShowDialog(this) == true)
        {
            try
            {
                File.WriteAllText(dlg.FileName, doc.Document.Text, new UTF8Encoding(false));
                doc.FilePath = dlg.FileName;
                doc.IsModified = false;
                doc.SyntaxName = SyntaxService.GetLanguageByExtension(dlg.FileName);
                ApplySyntax(doc.Editor, doc.SyntaxName);
                doc.UpdateTitle();
                UpdateStatusBar();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save file:\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        return false;
    }

    private void MenuSave_Click(object sender, RoutedEventArgs e) => SaveDocument(ActiveDocument);

    private void MenuSaveAs_Click(object sender, RoutedEventArgs e) => SaveAsDocument(ActiveDocument);

    private void MenuSaveAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var doc in Documents.Where(d => d.IsModified))
        {
            SaveDocument(doc);
        }
    }

    private void MenuCloseTab_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveDocument != null) CloseDocument(ActiveDocument);
    }

    private void MenuCloseOthers_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveDocument == null) return;
        var others = Documents.Where(d => d != ActiveDocument).ToList();
        foreach (var doc in others)
        {
            CloseDocument(doc);
        }
    }

    private void MenuCloseAll_Click(object sender, RoutedEventArgs e)
    {
        var all = Documents.ToList();
        foreach (var doc in all)
        {
            CloseDocument(doc);
        }
    }

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
                if (ActiveDocument != null)
                {
                    ActiveDocument.SyntaxName = lang;
                    ApplySyntax(ActiveDocument.Editor, lang);
                    UpdateStatusBar();
                }
            };
            MenuSyntaxParent.Items.Add(item);
        }
    }

    private void ApplySyntax(TextEditor? editor, string syntaxName)
    {
        if (editor == null) return;
        editor.SyntaxHighlighting = SyntaxService.GetDefinition(syntaxName);
    }

    private void BtnLanguageSelector_Click(object sender, RoutedEventArgs e)
    {
        var contextMenu = new ContextMenu();
        foreach (var lang in SyntaxService.SupportedLanguages)
        {
            var item = new MenuItem { Header = lang };
            item.Click += (s, ev) =>
            {
                if (ActiveDocument != null)
                {
                    ActiveDocument.SyntaxName = lang;
                    ApplySyntax(ActiveDocument.Editor, lang);
                    UpdateStatusBar();
                }
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
        if (ActiveDocument?.Editor != null && !string.IsNullOrEmpty(ActiveDocument.Editor.SelectedText))
        {
            TxtSearch.Text = ActiveDocument.Editor.SelectedText;
            TxtSearch.SelectAll();
        }
        UpdateMatchCount();
    }

    private void MenuReplace_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Visible;
        ReplaceRow.Visibility = Visibility.Visible;
        TxtSearch.Focus();
        if (ActiveDocument?.Editor != null && !string.IsNullOrEmpty(ActiveDocument.Editor.SelectedText))
        {
            TxtSearch.Text = ActiveDocument.Editor.SelectedText;
            TxtSearch.SelectAll();
        }
        UpdateMatchCount();
    }

    private void BtnCloseSearch_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Collapsed;
        ActiveDocument?.Editor?.Focus();
    }

    private void SearchOption_Click(object sender, RoutedEventArgs e)
    {
        UpdateMatchCount();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateMatchCount();
    }

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
        if (ActiveDocument?.Editor == null || string.IsNullOrEmpty(TxtSearch.Text)) return;

        var editor = ActiveDocument.Editor;
        string pattern = TxtSearch.Text;
        string text = editor.Document.Text;
        int startIndex = editor.CaretOffset;

        var regex = GetSearchRegex(pattern);
        if (regex == null) return;

        var match = regex.Match(text, startIndex);
        if (!match.Success)
        {
            // Wrap around from beginning
            match = regex.Match(text, 0);
        }

        if (match.Success)
        {
            editor.Select(match.Index, match.Length);
            editor.ScrollTo(editor.TextArea.Caret.Line, editor.TextArea.Caret.Column);
        }
        UpdateMatchCount();
    }

    private void FindPrevious()
    {
        if (ActiveDocument?.Editor == null || string.IsNullOrEmpty(TxtSearch.Text)) return;

        var editor = ActiveDocument.Editor;
        string pattern = TxtSearch.Text;
        string text = editor.Document.Text;
        int selectionStart = editor.SelectionStart;

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
            targetMatch = matches[matches.Count - 1]; // Wrap around to end
        }

        if (targetMatch != null)
        {
            editor.Select(targetMatch.Index, targetMatch.Length);
            editor.ScrollTo(editor.TextArea.Caret.Line, editor.TextArea.Caret.Column);
        }
        UpdateMatchCount();
    }

    private void ReplaceCurrent()
    {
        if (ActiveDocument?.Editor == null || string.IsNullOrEmpty(TxtSearch.Text)) return;

        var editor = ActiveDocument.Editor;
        if (editor.SelectionLength > 0)
        {
            editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, TxtReplace.Text);
        }
        FindNext();
    }

    private void ReplaceAll()
    {
        if (ActiveDocument?.Editor == null || string.IsNullOrEmpty(TxtSearch.Text)) return;

        var editor = ActiveDocument.Editor;
        var regex = GetSearchRegex(TxtSearch.Text);
        if (regex == null) return;

        string original = editor.Document.Text;
        string replaced = regex.Replace(original, TxtReplace.Text);
        int count = regex.Matches(original).Count;

        if (count > 0)
        {
            editor.Document.Text = replaced;
            MessageBox.Show($"Replaced {count} occurrence(s).", "Replace All", MessageBoxButton.OK, MessageBoxImage.Information);
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
        if (ActiveDocument?.Editor == null || string.IsNullOrEmpty(TxtSearch.Text))
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

        int count = regex.Matches(ActiveDocument.Editor.Document.Text).Count;
        LblMatchCount.Text = $"{count} match{(count == 1 ? "" : "es")}";
    }

    #endregion

    #region Go To Line

    private void MenuGoToLine_Click(object sender, RoutedEventArgs e)
    {
        GoToLinePanel.Visibility = Visibility.Visible;
        TxtGoToLine.Text = ActiveDocument?.CaretLine.ToString() ?? "1";
        TxtGoToLine.SelectAll();
        TxtGoToLine.Focus();
    }

    private void BtnCloseGoToLine_Click(object sender, RoutedEventArgs e)
    {
        GoToLinePanel.Visibility = Visibility.Collapsed;
        ActiveDocument?.Editor?.Focus();
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
        if (ActiveDocument?.Editor == null) return;

        if (int.TryParse(TxtGoToLine.Text.Trim(), out int targetLine))
        {
            int clamped = Math.Clamp(targetLine, 1, ActiveDocument.Document.LineCount);
            ActiveDocument.Editor.ScrollToLine(clamped);
            ActiveDocument.Editor.TextArea.Caret.Line = clamped;
            ActiveDocument.Editor.TextArea.Caret.Column = 1;
            GoToLinePanel.Visibility = Visibility.Collapsed;
            ActiveDocument.Editor.Focus();
        }
    }

    #endregion

    #region Edit Actions & Keyboard Manipulations

    private void MenuUndo_Click(object sender, RoutedEventArgs e) => ActiveDocument?.Editor?.Undo();
    private void MenuRedo_Click(object sender, RoutedEventArgs e) => ActiveDocument?.Editor?.Redo();
    private void MenuCut_Click(object sender, RoutedEventArgs e) => ActiveDocument?.Editor?.Cut();
    private void MenuCopy_Click(object sender, RoutedEventArgs e) => ActiveDocument?.Editor?.Copy();
    private void MenuPaste_Click(object sender, RoutedEventArgs e) => ActiveDocument?.Editor?.Paste();
    private void MenuSelectAll_Click(object sender, RoutedEventArgs e) => ActiveDocument?.Editor?.SelectAll();

    private void MenuDuplicateLine_Click(object sender, RoutedEventArgs e)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null) return;

        var line = editor.Document.GetLineByNumber(editor.TextArea.Caret.Line);
        string text = editor.Document.GetText(line.Offset, line.Length);
        editor.Document.Insert(line.EndOffset, Environment.NewLine + text);
    }

    private void MenuDeleteLine_Click(object sender, RoutedEventArgs e)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null) return;

        var line = editor.Document.GetLineByNumber(editor.TextArea.Caret.Line);
        int offset = line.Offset;
        int length = line.TotalLength;
        editor.Document.Remove(offset, length);
    }

    private void MenuMoveLineUp_Click(object sender, RoutedEventArgs e)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null || editor.TextArea.Caret.Line <= 1) return;

        int curNum = editor.TextArea.Caret.Line;
        var curLine = editor.Document.GetLineByNumber(curNum);
        var prevLine = editor.Document.GetLineByNumber(curNum - 1);

        string curText = editor.Document.GetText(curLine.Offset, curLine.Length);
        string prevText = editor.Document.GetText(prevLine.Offset, prevLine.Length);

        using (editor.Document.RunUpdate())
        {
            editor.Document.Replace(curLine.Offset, curLine.Length, prevText);
            editor.Document.Replace(prevLine.Offset, prevLine.Length, curText);
        }
        editor.TextArea.Caret.Line = curNum - 1;
    }

    private void MenuMoveLineDown_Click(object sender, RoutedEventArgs e)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null || editor.TextArea.Caret.Line >= editor.Document.LineCount) return;

        int curNum = editor.TextArea.Caret.Line;
        var curLine = editor.Document.GetLineByNumber(curNum);
        var nextLine = editor.Document.GetLineByNumber(curNum + 1);

        string curText = editor.Document.GetText(curLine.Offset, curLine.Length);
        string nextText = editor.Document.GetText(nextLine.Offset, nextLine.Length);

        using (editor.Document.RunUpdate())
        {
            editor.Document.Replace(nextLine.Offset, nextLine.Length, curText);
            editor.Document.Replace(curLine.Offset, curLine.Length, nextText);
        }
        editor.TextArea.Caret.Line = curNum + 1;
    }

    private void MenuToggleComment_Click(object sender, RoutedEventArgs e)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null) return;

        string commentPrefix = (ActiveDocument?.SyntaxName) switch
        {
            "PowerShell" or "Python" or "YAML" or "INI / Config" => "# ",
            "SQL" => "-- ",
            "Batch" => ":: ",
            _ => "// "
        };

        var line = editor.Document.GetLineByNumber(editor.TextArea.Caret.Line);
        string text = editor.Document.GetText(line.Offset, line.Length);

        if (text.TrimStart().StartsWith(commentPrefix.Trim()))
        {
            int index = text.IndexOf(commentPrefix.Trim());
            editor.Document.Remove(line.Offset + index, commentPrefix.Length);
        }
        else
        {
            editor.Document.Insert(line.Offset, commentPrefix);
        }
    }

    private void MenuInsertDateTime_Click(object sender, RoutedEventArgs e)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null) return;
        string stamp = TextTransformService.GetCurrentTimestamp();
        editor.Document.Insert(editor.CaretOffset, stamp);
    }

    #endregion

    #region View & Formatting

    private void MenuWordWrap_Click(object sender, RoutedEventArgs e)
    {
        bool wrap = MenuWordWrap.IsChecked;
        foreach (var doc in Documents)
        {
            if (doc.Editor != null) doc.Editor.WordWrap = wrap;
        }
    }

    private void MenuLineNumbers_Click(object sender, RoutedEventArgs e)
    {
        bool show = MenuLineNumbers.IsChecked;
        foreach (var doc in Documents)
        {
            if (doc.Editor != null) doc.Editor.ShowLineNumbers = show;
        }
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
        foreach (var doc in Documents)
        {
            if (doc.Editor != null) doc.Editor.FontSize = BaseFontSize * _zoomFactor;
        }
        int percent = (int)(_zoomFactor * 100);
        StatusZoom.Text = $"{percent}%";
        BtnZoomLabel.Content = $"{percent}%";
    }

    private void MenuZoomIn_Click(object sender, RoutedEventArgs e) => ZoomIn();
    private void MenuZoomOut_Click(object sender, RoutedEventArgs e) => ZoomOut();
    private void MenuZoomReset_Click(object sender, RoutedEventArgs e) => ZoomReset();
    private void StatusZoom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ZoomReset();

    #endregion

    #region Tools & Transforms

    private void TransformSelectionOrAll(Func<string, string> transformFunc)
    {
        var editor = ActiveDocument?.Editor;
        if (editor == null) return;

        try
        {
            if (editor.SelectionLength > 0)
            {
                string transformed = transformFunc(editor.SelectedText);
                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, transformed);
            }
            else
            {
                string transformed = transformFunc(editor.Document.Text);
                editor.Document.Text = transformed;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Transform Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void MenuUpper_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToUpperCase);
    private void MenuLower_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToLowerCase);
    private void MenuTitleCase_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToTitleCase);
    private void MenuInvertCase_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToInvertCase);
    private void MenuSortAsc_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(s => TextTransformService.SortLines(s, false));
    private void MenuSortDesc_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(s => TextTransformService.SortLines(s, true));
    private void MenuRemoveDuplicates_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.RemoveDuplicateLines);
    private void MenuRemoveEmptyLines_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.RemoveEmptyLines);
    private void MenuTrimWhitespace_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.TrimTrailingWhitespace);
    private void MenuFormatJson_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.FormatJson);
    private void MenuMinifyJson_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.MinifyJson);
    private void MenuBase64Encode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToBase64);
    private void MenuBase64Decode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.FromBase64);
    private void MenuUrlEncode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.ToUrlEncoded);
    private void MenuUrlDecode_Click(object sender, RoutedEventArgs e) => TransformSelectionOrAll(TextTransformService.FromUrlEncoded);

    private void MenuCopyPath_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ActiveDocument?.FilePath))
        {
            Clipboard.SetText(ActiveDocument.FilePath);
        }
    }

    private void MenuOpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ActiveDocument?.FilePath))
        {
            Process.Start("explorer.exe", $"/select,\"{ActiveDocument.FilePath}\"");
        }
    }

    #endregion

    #region Markdown Live Preview

    private void MenuMarkdownPreview_Click(object sender, RoutedEventArgs e)
    {
        _isMarkdownPreviewActive = !_isMarkdownPreviewActive;

        if (_isMarkdownPreviewActive)
        {
            ColSplitter.Width = new GridLength(4);
            ColPreview.Width = new GridLength(1, GridUnitType.Star);
            MarkdownSplitter.Visibility = Visibility.Visible;
            MarkdownPreviewPane.Visibility = Visibility.Visible;
            BtnMarkdownPreview.Foreground = (SolidColorBrush)FindResource("AccentBlueBrush");
            UpdateMarkdownPreview();
        }
        else
        {
            ColSplitter.Width = new GridLength(0);
            ColPreview.Width = new GridLength(0);
            MarkdownSplitter.Visibility = Visibility.Collapsed;
            MarkdownPreviewPane.Visibility = Visibility.Collapsed;
            BtnMarkdownPreview.Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush");
        }
    }

    private void UpdateMarkdownPreview()
    {
        if (!_isMarkdownPreviewActive || ActiveDocument?.Editor == null) return;

        var flowDoc = new FlowDocument
        {
            Background = (SolidColorBrush)FindResource("BgDarkBrush"),
            Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush"),
            FontFamily = new FontFamily("Segoe UI, Arial"),
            FontSize = 13,
            PagePadding = new Thickness(16)
        };

        string[] lines = ActiveDocument.Document.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool inCodeBlock = false;
        StringBuilder codeContent = new();

        foreach (string line in lines)
        {
            if (line.TrimStart().StartsWith("```"))
            {
                if (inCodeBlock)
                {
                    // Finish code block
                    var codeP = new Paragraph(new Run(codeContent.ToString()))
                    {
                        FontFamily = new FontFamily("Cascadia Code, Consolas"),
                        Background = (SolidColorBrush)FindResource("BgSurfaceBrush"),
                        Foreground = (SolidColorBrush)FindResource("AccentBlueBrush"),
                        Padding = new Thickness(10),
                        Margin = new Thickness(0, 4, 0, 8)
                    };
                    flowDoc.Blocks.Add(codeP);
                    codeContent.Clear();
                    inCodeBlock = false;
                }
                else
                {
                    inCodeBlock = true;
                }
                continue;
            }

            if (inCodeBlock)
            {
                codeContent.AppendLine(line);
                continue;
            }

            if (line.StartsWith("# "))
            {
                var p = new Paragraph(new Bold(new Run(line[2..])))
                {
                    FontSize = 20,
                    Foreground = (SolidColorBrush)FindResource("AccentBlueBrush"),
                    Margin = new Thickness(0, 12, 0, 4)
                };
                flowDoc.Blocks.Add(p);
            }
            else if (line.StartsWith("## "))
            {
                var p = new Paragraph(new Bold(new Run(line[3..])))
                {
                    FontSize = 16,
                    Foreground = (SolidColorBrush)FindResource("AccentBlueBrush"),
                    Margin = new Thickness(0, 10, 0, 4)
                };
                flowDoc.Blocks.Add(p);
            }
            else if (line.StartsWith("### "))
            {
                var p = new Paragraph(new Bold(new Run(line[4..])))
                {
                    FontSize = 14,
                    Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush"),
                    Margin = new Thickness(0, 8, 0, 2)
                };
                flowDoc.Blocks.Add(p);
            }
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                var p = new Paragraph(new Run("• " + line[2..]))
                {
                    Margin = new Thickness(16, 2, 0, 2)
                };
                flowDoc.Blocks.Add(p);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                flowDoc.Blocks.Add(new Paragraph(new Run(line)) { Margin = new Thickness(0, 2, 0, 4) });
            }
        }

        MarkdownViewer.Document = flowDoc;
    }

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
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    OpenFile(file);
                }
            }
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Global Hotkeys
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.N) { e.Handled = true; CreateNewTab(); }
            else if (e.Key == Key.O) { e.Handled = true; MenuOpen_Click(sender, e); }
            else if (e.Key == Key.S) { e.Handled = true; SaveDocument(ActiveDocument); }
            else if (e.Key == Key.W) { e.Handled = true; if (ActiveDocument != null) CloseDocument(ActiveDocument); }
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
            if (e.Key == Key.S) { e.Handled = true; SaveAsDocument(ActiveDocument); }
            else if (e.Key == Key.M) { e.Handled = true; MenuMarkdownPreview_Click(sender, e); }
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
        else if (e.Key == Key.F1)
        {
            e.Handled = true;
            MenuShortcuts_Click(sender, e);
        }
    }

    private void UpdateStatusBar()
    {
        if (ActiveDocument == null) return;

        StatusCaretPos.Text = $"Ln {ActiveDocument.CaretLine}, Col {ActiveDocument.CaretColumn}";

        if (ActiveDocument.SelectionLength > 0)
        {
            StatusSelection.Text = $"Sel: {ActiveDocument.SelectionLength} chars ({ActiveDocument.SelectionLines} lines)";
            StatusSelection.Visibility = Visibility.Visible;
        }
        else
        {
            StatusSelection.Visibility = Visibility.Collapsed;
        }

        StatusDocStats.Text = $"{ActiveDocument.CharCount:N0} chars, {ActiveDocument.WordCount:N0} words, {ActiveDocument.LineCount:N0} lines";
        StatusEncoding.Text = ActiveDocument.EncodingName;
        StatusEol.Text = ActiveDocument.EolName;
        BtnLanguageSelector.Content = $"Language: {ActiveDocument.SyntaxName} ▼";
    }

    private void MenuShortcuts_Click(object sender, RoutedEventArgs e)
    {
        string shortcuts =
            "🌙 NightPad Keyboard Shortcuts:\n\n" +
            "File:\n" +
            "  Ctrl + N : New Tab\n" +
            "  Ctrl + O : Open File\n" +
            "  Ctrl + S : Save File\n" +
            "  Ctrl + Shift + S : Save As\n" +
            "  Ctrl + W : Close Tab\n\n" +
            "Editing:\n" +
            "  Ctrl + D : Duplicate Line\n" +
            "  Ctrl + Shift + K : Delete Line\n" +
            "  Alt + Up/Down : Move Line Up/Down\n" +
            "  Ctrl + / : Toggle Comment\n" +
            "  F5 : Insert Date/Time\n\n" +
            "Search & Navigation:\n" +
            "  Ctrl + F : Find\n" +
            "  Ctrl + H : Replace\n" +
            "  F3 / Shift + F3 : Find Next / Prev\n" +
            "  Ctrl + G : Go to Line\n\n" +
            "View & Tools:\n" +
            "  Alt + Z : Toggle Word Wrap\n" +
            "  Ctrl + Shift + M : Markdown Live Preview\n" +
            "  Ctrl + Shift + J : Format JSON\n" +
            "  Ctrl + Plus/Minus/0 : Zoom In/Out/Reset\n" +
            "  Ctrl + Shift + L : Toggle Line Numbers";

        MessageBox.Show(shortcuts, "NightPad Shortcuts", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "🌙 NightPad - Professional Night Mode Text Editor\n" +
            "Version 1.0.0 (x64 Native)\n\n" +
            "Designed exclusively for the MyEnv Windows Desktop Environment.\n" +
            "Crafted with deep Obsidian Night Dark Theme & Sharp Aesthetics.",
            "About NightPad",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        var modified = Documents.Where(d => d.IsModified).ToList();
        if (modified.Count > 0)
        {
            var result = MessageBox.Show(
                $"You have {modified.Count} unsaved document(s). Do you want to review and save before exiting?",
                "NightPad",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                foreach (var doc in modified)
                {
                    if (!SaveDocument(doc))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        base.OnClosing(e);
    }

    #endregion
}