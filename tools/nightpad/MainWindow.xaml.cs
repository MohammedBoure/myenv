using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    private bool _isAutoDetectMode = true;
    private string _currentSyntaxName = "Plain Text";

    // Auto-completion state for quick keyboard save
    private List<string>? _currentCompletions;
    private int _completionIndex = -1;
    private string _completionOriginalInput = string.Empty;
    private bool _suppressTextChange;

    // Lightweight external file change monitoring (FileSystemWatcher)
    private FileSystemWatcher? _fileWatcher;
    private DateTime _lastKnownWriteTimeUtc = DateTime.MinValue;
    private bool _isSavingInternal;
    private bool _hasPendingExternalChange;

    // Quick Symbols & Frequent Words palette state
    private List<QuickSymbolItem> _allSymbols = new();
    private List<QuickSymbolItem> _filteredSymbols = new();
    private string _selectedCategoryFilter = "All";

    public MainWindow()
    {
        InitializeComponent();
        ConfigureEditor();
        InitializeSyntaxMenu();
        UpdateTitle();
        UpdateStatusBar();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        FocusEditor();
    }

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        if (_hasPendingExternalChange)
        {
            ShowExternalChangeNotification();
        }
        else if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
        {
            try
            {
                var currentWrite = File.GetLastWriteTimeUtc(_currentFilePath);
                if (currentWrite > _lastKnownWriteTimeUtc.AddMilliseconds(200))
                {
                    _lastKnownWriteTimeUtc = currentWrite;
                    _hasPendingExternalChange = true;
                    ShowExternalChangeNotification();
                }
            }
            catch { }
        }

        if (QuickSavePanel.Visibility != Visibility.Visible &&
            SearchPanel.Visibility != Visibility.Visible &&
            GoToLinePanel.Visibility != Visibility.Visible &&
            QuickSymbolsOverlay.Visibility != Visibility.Visible)
        {
            FocusEditor();
        }
    }

    private void MainEditor_Loaded(object sender, RoutedEventArgs e)
    {
        FocusEditor();
    }

    /// <summary>
    /// Instantly focuses the AvalonEdit editor buffer so the user can type immediately upon launch without mouse interaction.
    /// </summary>
    private void FocusEditor()
    {
        Dispatcher.InvokeAsync(() =>
        {
            MainEditor.Focus();
            if (MainEditor.TextArea != null)
            {
                Keyboard.Focus(MainEditor.TextArea);
            }
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void ConfigureEditor()
    {
        MainEditor.Options.ConvertTabsToSpaces = true;
        MainEditor.Options.IndentationSize = 4;
        MainEditor.Options.EnableHyperlinks = false;
        MainEditor.Options.HighlightCurrentLine = true;
        MainEditor.Options.EnableRectangularSelection = true;
        MainEditor.Options.EnableTextDragDrop = true;
        MainEditor.Options.AllowScrollBelowDocument = true;
        MainEditor.Options.InheritWordWrapIndentation = true;

        MainEditor.WordWrap = true;
        MenuWordWrap.IsChecked = true;

        MainEditor.LineNumbersForeground = (SolidColorBrush)FindResource("TextSecondaryBrush");
        MainEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(90, 31, 111, 235));
        MainEditor.TextArea.SelectionBorder = null;
        MainEditor.TextArea.SelectionCornerRadius = 0;

        DataObject.AddPastingHandler(MainEditor, OnEditorPasting);
        MainEditor.ContextMenuOpening += MainEditor_ContextMenuOpening;
        MainEditor.TextArea.PreviewMouseLeftButtonDown += TextArea_PreviewMouseLeftButtonDown;

        MainEditor.TextChanged += (s, e) =>
        {
            if (!_isModified)
            {
                _isModified = true;
                UpdateTitle();
            }

            if (_isAutoDetectMode)
            {
                AutoDetectAndApplySyntaxFromContent();
            }

            CheckAutoArabicDetection();
            UpdateStatusBar();

            if (PreviewContainer.Visibility == Visibility.Visible)
            {
                UpdateMarkdownPreview();
            }
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

        // Smart Line Insertion (Ctrl+Enter, Ctrl+Shift+Enter) & Python indentation
        MainEditor.TextArea.PreviewKeyDown += (s, e) =>
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var modifiers = Keyboard.Modifiers;

            if (key == Key.Enter)
            {
                // Ctrl+Shift+Enter: insert new line ABOVE
                if (modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Shift))
                {
                    e.Handled = true;
                    var curLine = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
                    string indent = GetLineIndentation(curLine);
                    MainEditor.Document.Insert(curLine.Offset, indent + Environment.NewLine);
                    MainEditor.CaretOffset = curLine.Offset + indent.Length;
                    return;
                }

                // Ctrl+Enter: insert new line BELOW without splitting current line
                if (modifiers == ModifierKeys.Control && !modifiers.HasFlag(ModifierKeys.Shift))
                {
                    e.Handled = true;
                    var curLine = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
                    string indent = GetLineIndentation(curLine);
                    string lineText = MainEditor.Document.GetText(curLine.Offset, curLine.Length);
                    if (lineText.TrimEnd().EndsWith(':'))
                    {
                        indent += "    ";
                    }

                    string toInsert = Environment.NewLine + indent;
                    MainEditor.Document.Insert(curLine.EndOffset, toInsert);
                    MainEditor.CaretOffset = curLine.EndOffset + toInsert.Length;
                    return;
                }

                // Normal Enter: preserve auto-indent and python block indent
                if (modifiers == ModifierKeys.None)
                {
                    var curLine = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
                    string lineText = MainEditor.Document.GetText(curLine.Offset, MainEditor.CaretOffset - curLine.Offset);

                    int leadingSpaces = 0;
                    while (leadingSpaces < lineText.Length && lineText[leadingSpaces] == ' ')
                    {
                        leadingSpaces++;
                    }

                    if (lineText.TrimEnd().EndsWith(':'))
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
            }
        };
    }

    private string GetLineIndentation(DocumentLine line)
    {
        string text = MainEditor.Document.GetText(line.Offset, line.Length);
        int spaces = 0;
        while (spaces < text.Length && (text[spaces] == ' ' || text[spaces] == '\t'))
        {
            spaces++;
        }
        return text[..spaces];
    }

    private void UpdateTitle()
    {
        string fileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
        string prefix = _isModified ? "*" : "";
        string titleText = $"{prefix}{fileName} - Notepad";
        Title = titleText;
        TxtWindowTitle.Text = titleText;
    }

    #region File Operations & Fast Terminal Saving

    public void OpenFile(string filePath)
    {
        OpenOrCreateFile(filePath);
    }

    /// <summary>
    /// Loads a file if it exists, or initializes a new buffer bound to the specified target file path.
    /// Automatically applies syntax highlighting based on the file extension and updates the document title.
    /// </summary>
    public void OpenOrCreateFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        if (_isModified && !PromptSaveBeforeAction())
            return;

        try
        {
            string fullPath = Path.GetFullPath(filePath);

            if (Directory.Exists(fullPath))
            {
                _currentFilePath = null;
                MainEditor.Document.Text = string.Empty;
                _isModified = false;
                _currentSyntaxName = "Plain Text";
                ApplySyntax(_currentSyntaxName);
                UpdateTitle();
                UpdateStatusBar();
                ShowQuickSave();
                ApplyPresetDirectory(fullPath);
                return;
            }

            _currentFilePath = fullPath;

            if (File.Exists(fullPath))
            {
                string content = File.ReadAllText(fullPath, Encoding.UTF8);
                MainEditor.Document.Text = content;
            }
            else
            {
                MainEditor.Document.Text = string.Empty;
            }

            _isModified = false;
            string extLang = SyntaxService.GetLanguageByExtension(_currentFilePath);
            if (extLang != "Plain Text")
            {
                _isAutoDetectMode = false;
                _currentSyntaxName = extLang;
            }
            else
            {
                _isAutoDetectMode = true;
                string contentLang = SyntaxService.DetectLanguageFromContent(MainEditor.Document.Text);
                _currentSyntaxName = contentLang;
            }

            ApplySyntax(_currentSyntaxName);
            UpdateSyntaxMenuSelection();

            CheckAutoArabicDetection();
            UpdateTitle();
            UpdateStatusBar();

            SetupFileWatcher(_currentFilePath);
            _hasPendingExternalChange = false;
            ExternalChangeBanner.Visibility = Visibility.Collapsed;

            if (PreviewContainer.Visibility == Visibility.Visible)
            {
                UpdateMarkdownPreview();
            }

            FocusEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open/initialize file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NewFile()
    {
        if (_isModified && !PromptSaveBeforeAction())
            return;

        MainEditor.Document.Text = string.Empty;
        _currentFilePath = null;
        _isModified = false;
        _isAutoDetectMode = true;
        _currentSyntaxName = "Plain Text";
        ApplySyntax(_currentSyntaxName);
        UpdateSyntaxMenuSelection();

        DisposeFileWatcher();
        _hasPendingExternalChange = false;
        ExternalChangeBanner.Visibility = Visibility.Collapsed;

        SetTextDirection(FlowDirection.LeftToRight);
        UpdateTitle();
        UpdateStatusBar();

        if (PreviewContainer.Visibility == Visibility.Visible)
        {
            UpdateMarkdownPreview();
        }

        FocusEditor();
    }

    /// <summary>
    /// Fast save: saves instantly if file has path, otherwise opens keyboard Quick Save bar.
    /// </summary>
    public bool SaveFile()
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            ShowQuickSave();
            return false;
        }

        _isSavingInternal = true;
        try
        {
            string? dir = Path.GetDirectoryName(_currentFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_currentFilePath, MainEditor.Document.Text, new UTF8Encoding(false));
            _isModified = false;
            _lastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(_currentFilePath);
            _hasPendingExternalChange = false;
            ExternalChangeBanner.Visibility = Visibility.Collapsed;
            SetupFileWatcher(_currentFilePath);

            UpdateTitle();
            UpdateStatusBar();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        finally
        {
            Task.Delay(300).ContinueWith(_ => _isSavingInternal = false);
        }
    }

    /// <summary>
    /// Opens the fast keyboard-driven Quick Save bar.
    /// </summary>
    public void ShowQuickSave()
    {
        SearchPanel.Visibility = Visibility.Collapsed;
        GoToLinePanel.Visibility = Visibility.Collapsed;
        QuickSavePanel.Visibility = Visibility.Visible;

        string defaultDir = !string.IsNullOrEmpty(_currentFilePath)
            ? Path.GetDirectoryName(_currentFilePath) ?? Environment.CurrentDirectory
            : Environment.CurrentDirectory;

        string defaultFileName = !string.IsNullOrEmpty(_currentFilePath)
            ? Path.GetFileName(_currentFilePath)
            : SuggestDefaultFileName();

        string initialPath = Path.Combine(defaultDir, defaultFileName);
        _suppressTextChange = true;
        TxtQuickSavePath.Text = initialPath;
        _suppressTextChange = false;

        ResetCompletion();
        UpdateQuickSaveStatus(initialPath);

        TxtQuickSavePath.Focus();

        // Select the filename portion for instant overwrite/editing
        int lastSep = initialPath.LastIndexOfAny(new[] { '\\', '/' });
        if (lastSep >= 0 && lastSep + 1 < initialPath.Length)
        {
            TxtQuickSavePath.Select(lastSep + 1, initialPath.Length - lastSep - 1);
        }
        else
        {
            TxtQuickSavePath.SelectAll();
        }
    }

    private string SuggestDefaultFileName()
    {
        string ext = _currentSyntaxName switch
        {
            "Python" => ".py",
            "PowerShell" => ".ps1",
            "JavaScript" => ".js",
            "TypeScript" => ".ts",
            "JSON" => ".json",
            "Markdown" => ".md",
            "C#" => ".cs",
            "HTML" => ".html",
            "CSS" => ".css",
            "Batch" => ".cmd",
            "SQL" => ".sql",
            _ => ".txt"
        };

        return $"untitled{ext}";
    }

    public void CloseQuickSave()
    {
        QuickSavePanel.Visibility = Visibility.Collapsed;
        ResetCompletion();
        FocusEditor();
    }

    private void ResetCompletion()
    {
        _currentCompletions = null;
        _completionIndex = -1;
        _completionOriginalInput = string.Empty;
    }

    private void TxtQuickSavePath_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Tab)
        {
            e.Handled = true;
            HandlePathTabCompletion();
        }
        else if (key == Key.Escape)
        {
            e.Handled = true;
            CloseQuickSave();
        }
        else if (key == Key.Enter)
        {
            e.Handled = true;
            ConfirmQuickSave();
        }
        else if (key == Key.F1)
        {
            e.Handled = true;
            ApplyPresetDirectory(Environment.CurrentDirectory);
        }
        else if (key == Key.F2)
        {
            e.Handled = true;
            ApplyPresetDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }
        else if (key == Key.F3)
        {
            e.Handled = true;
            ApplyPresetDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }
        else if (key == Key.F4)
        {
            e.Handled = true;
            ApplyPresetDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "myenv"));
        }
        else if (key != Key.LeftShift && key != Key.RightShift && key != Key.LeftCtrl && key != Key.RightCtrl)
        {
            ResetCompletion();
        }
    }

    private void HandlePathTabCompletion()
    {
        string input = TxtQuickSavePath.Text;

        if (_currentCompletions == null || _currentCompletions.Count == 0)
        {
            _completionOriginalInput = input;
            _currentCompletions = PathCompletionService.GetCompletions(input);
            _completionIndex = -1;
        }

        if (_currentCompletions != null && _currentCompletions.Count > 0)
        {
            _completionIndex = (_completionIndex + 1) % _currentCompletions.Count;
            string match = _currentCompletions[_completionIndex];

            _suppressTextChange = true;
            TxtQuickSavePath.Text = match;
            TxtQuickSavePath.CaretIndex = match.Length;
            _suppressTextChange = false;

            LblQuickSaveStatus.Text = $"[{_completionIndex + 1}/{_currentCompletions.Count}] {match}";
        }
        else
        {
            LblQuickSaveStatus.Text = "No path matches found";
        }
    }

    private void ApplyPresetDirectory(string dir)
    {
        if (string.IsNullOrWhiteSpace(dir)) return;

        string currentVal = TxtQuickSavePath.Text;
        string fileName = Path.GetFileName(currentVal);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = SuggestDefaultFileName();
        }

        string newPath = Path.Combine(dir, fileName);
        _suppressTextChange = true;
        TxtQuickSavePath.Text = newPath;
        _suppressTextChange = false;

        ResetCompletion();
        UpdateQuickSaveStatus(newPath);
        TxtQuickSavePath.Focus();
        TxtQuickSavePath.CaretIndex = newPath.Length;
    }

    private void TxtQuickSavePath_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChange) return;
        ResetCompletion();
        UpdateQuickSaveStatus(TxtQuickSavePath.Text);
    }

    private void TxtQuickSavePath_KeyDown(object sender, KeyEventArgs e)
    {
        // Handled in PreviewKeyDown
    }

    private void UpdateQuickSaveStatus(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            LblQuickSaveStatus.Text = "Enter a file path";
            return;
        }

        string resolved = PathCompletionService.ResolvePath(input);
        string? dir = Path.GetDirectoryName(resolved);

        if (string.IsNullOrEmpty(dir))
        {
            LblQuickSaveStatus.Text = "Invalid directory path";
        }
        else if (Directory.Exists(dir))
        {
            LblQuickSaveStatus.Text = "Directory exists ✓ | Press Enter to save";
        }
        else
        {
            LblQuickSaveStatus.Text = "📁 New directory: will create on save | Press Enter to save";
        }
    }

    private void ConfirmQuickSave()
    {
        string raw = TxtQuickSavePath.Text;
        if (string.IsNullOrWhiteSpace(raw))
        {
            MessageBox.Show("Please enter a valid file path.", "Save", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string targetPath = PathCompletionService.ResolvePath(raw);

        // If target is a directory path, append default filename
        if (Directory.Exists(targetPath) || targetPath.EndsWith('\\') || targetPath.EndsWith('/'))
        {
            targetPath = Path.Combine(targetPath, SuggestDefaultFileName());
        }

        _isSavingInternal = true;
        try
        {
            PathCompletionService.EnsureDirectoryExists(targetPath);
            File.WriteAllText(targetPath, MainEditor.Document.Text, new UTF8Encoding(false));

            _currentFilePath = Path.GetFullPath(targetPath);
            _isModified = false;
            _lastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(_currentFilePath);
            _hasPendingExternalChange = false;
            ExternalChangeBanner.Visibility = Visibility.Collapsed;
            SetupFileWatcher(_currentFilePath);

            _currentSyntaxName = SyntaxService.GetLanguageByExtension(_currentFilePath);
            _isAutoDetectMode = (_currentSyntaxName == "Plain Text");
            ApplySyntax(_currentSyntaxName);
            UpdateSyntaxMenuSelection();

            UpdateTitle();
            UpdateStatusBar();
            CloseQuickSave();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save file to '{targetPath}':\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Task.Delay(300).ContinueWith(_ => _isSavingInternal = false);
        }
    }

    private void BtnQuickSaveConfirm_Click(object sender, RoutedEventArgs e) => ConfirmQuickSave();
    private void BtnQuickSaveCancel_Click(object sender, RoutedEventArgs e) => CloseQuickSave();
    private void BtnQuickSaveBrowse_Click(object sender, RoutedEventArgs e)
    {
        CloseQuickSave();
        SaveAsDialog();
    }

    private void BtnPresetCurrent_Click(object sender, RoutedEventArgs e) => ApplyPresetDirectory(Environment.CurrentDirectory);
    private void BtnPresetDocuments_Click(object sender, RoutedEventArgs e) => ApplyPresetDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    private void BtnPresetDesktop_Click(object sender, RoutedEventArgs e) => ApplyPresetDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    private void BtnPresetMyEnv_Click(object sender, RoutedEventArgs e) => ApplyPresetDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "myenv"));

    /// <summary>
    /// Classic Windows Save As Dialog fallback.
    /// </summary>
    public bool SaveAsDialog()
    {
        var dlg = new SaveFileDialog
        {
            FileName = string.IsNullOrEmpty(_currentFilePath) ? SuggestDefaultFileName() : Path.GetFileName(_currentFilePath),
            Filter = "All Files (*.*)|*.*|Python (*.py)|*.py|Text Documents (*.txt)|*.txt|PowerShell (*.ps1)|*.ps1|JSON (*.json)|*.json|Markdown (*.md)|*.md|C# (*.cs)|*.cs"
        };

        if (dlg.ShowDialog(this) == true)
        {
            _isSavingInternal = true;
            try
            {
                string? dir = Path.GetDirectoryName(dlg.FileName);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(dlg.FileName, MainEditor.Document.Text, new UTF8Encoding(false));
                _currentFilePath = dlg.FileName;
                _isModified = false;
                _lastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(_currentFilePath);
                _hasPendingExternalChange = false;
                ExternalChangeBanner.Visibility = Visibility.Collapsed;
                SetupFileWatcher(_currentFilePath);

                _currentSyntaxName = SyntaxService.GetLanguageByExtension(_currentFilePath);
                _isAutoDetectMode = (_currentSyntaxName == "Plain Text");
                ApplySyntax(_currentSyntaxName);
                UpdateSyntaxMenuSelection();

                UpdateTitle();
                UpdateStatusBar();
                FocusEditor();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                Task.Delay(300).ContinueWith(_ => _isSavingInternal = false);
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
    private void MenuSaveAsQuick_Click(object sender, RoutedEventArgs e) => ShowQuickSave();
    private void MenuSaveAsDialog_Click(object sender, RoutedEventArgs e) => SaveAsDialog();
    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    #endregion

    #region Markdown Live Preview

    public void ToggleMarkdownPreview()
    {
        bool isCurrentlyVisible = PreviewContainer.Visibility == Visibility.Visible;
        if (isCurrentlyVisible)
        {
            PreviewContainer.Visibility = Visibility.Collapsed;
            PreviewSplitter.Visibility = Visibility.Collapsed;
            ColPreview.Width = new GridLength(0);
            MenuMarkdownPreview.IsChecked = false;
            BtnHeaderMarkdownPreview.Content = "👁️ Markdown Preview";
            BtnStatusMarkdownPreview.Foreground = (SolidColorBrush)FindResource("AccentBlueBrush");
            FocusEditor();
        }
        else
        {
            PreviewContainer.Visibility = Visibility.Visible;
            PreviewSplitter.Visibility = Visibility.Visible;
            ColPreview.Width = new GridLength(1, GridUnitType.Star);
            MenuMarkdownPreview.IsChecked = true;
            BtnHeaderMarkdownPreview.Content = "✕ Close Preview";
            BtnStatusMarkdownPreview.Foreground = (SolidColorBrush)FindResource("AccentRedBrush");
            UpdateMarkdownPreview();
        }
    }

    private void UpdateMarkdownPreview()
    {
        bool isRtl = MainEditor.FlowDirection == FlowDirection.RightToLeft;
        string? baseDir = !string.IsNullOrEmpty(_currentFilePath) ? Path.GetDirectoryName(_currentFilePath) : null;
        MarkdownViewer.Document = MarkdownRenderService.Render(MainEditor.Document.Text, isRtl, baseDir);
    }

    private void BtnToggleMarkdownPreview_Click(object sender, RoutedEventArgs e) => ToggleMarkdownPreview();
    private void MenuMarkdownPreview_Click(object sender, RoutedEventArgs e) => ToggleMarkdownPreview();
    private void BtnClosePreview_Click(object sender, RoutedEventArgs e) => ToggleMarkdownPreview();

    #endregion

    #region Arabic & Text Direction Support

    private void SetTextDirection(FlowDirection direction)
    {
        MainEditor.FlowDirection = direction;
        if (MainEditor.TextArea != null)
        {
            MainEditor.TextArea.FlowDirection = direction;
        }

        bool isRtl = direction == FlowDirection.RightToLeft;
        MenuRtlDirection.IsChecked = isRtl;
        BtnDirectionToggle.Content = isRtl ? "RTL" : "LTR";
        BtnDirectionToggle.ToolTip = isRtl
            ? "Text Direction: Right-To-Left (عربي) - Click or Ctrl+Shift+R to toggle LTR"
            : "Text Direction: Left-To-Right - Click or Ctrl+Shift+R to toggle RTL / عربي";

        if (PreviewContainer.Visibility == Visibility.Visible)
        {
            UpdateMarkdownPreview();
        }
    }

    public void ToggleTextDirection()
    {
        var next = MainEditor.FlowDirection == FlowDirection.RightToLeft
            ? FlowDirection.LeftToRight
            : FlowDirection.RightToLeft;
        SetTextDirection(next);
    }

    private void CheckAutoArabicDetection()
    {
        if (MenuAutoArabicDetect?.IsChecked != true)
            return;

        string text = MainEditor.Document.Text;
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (ArabicTextService.ShouldBeRightToLeft(text))
        {
            if (MainEditor.FlowDirection != FlowDirection.RightToLeft)
            {
                SetTextDirection(FlowDirection.RightToLeft);
            }
        }
    }

    private void MenuRtlDirection_Click(object sender, RoutedEventArgs e)
    {
        SetTextDirection(MenuRtlDirection.IsChecked ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
    }

    private void MenuAutoArabicDetect_Click(object sender, RoutedEventArgs e)
    {
        CheckAutoArabicDetection();
    }

    private void BtnToggleDirection_Click(object sender, RoutedEventArgs e)
    {
        ToggleTextDirection();
    }

    #endregion

    #region Syntax Highlighting

    private void InitializeSyntaxMenu()
    {
        MenuSyntaxParent.Items.Clear();

        var autoDetectItem = new MenuItem
        {
            Header = "Auto-Detect Language",
            IsCheckable = true,
            IsChecked = _isAutoDetectMode
        };
        autoDetectItem.Click += (s, e) => SetAutoDetectMode(true);
        MenuSyntaxParent.Items.Add(autoDetectItem);
        MenuSyntaxParent.Items.Add(new Separator());

        foreach (var lang in SyntaxService.SupportedLanguages)
        {
            var item = new MenuItem
            {
                Header = lang,
                IsCheckable = true,
                IsChecked = (lang == _currentSyntaxName)
            };
            item.Click += (s, e) => SetManualLanguage(lang);
            MenuSyntaxParent.Items.Add(item);
        }
    }

    private void SetAutoDetectMode(bool autoDetect)
    {
        _isAutoDetectMode = autoDetect;
        if (_isAutoDetectMode)
        {
            string detected = "Plain Text";
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                string extLang = SyntaxService.GetLanguageByExtension(_currentFilePath);
                if (extLang != "Plain Text")
                {
                    detected = extLang;
                }
            }

            if (detected == "Plain Text")
            {
                detected = SyntaxService.DetectLanguageFromContent(MainEditor.Document.Text);
            }

            _currentSyntaxName = detected;
            ApplySyntax(_currentSyntaxName);
        }

        UpdateSyntaxMenuSelection();
        UpdateStatusBar();
    }

    private void SetManualLanguage(string lang)
    {
        _isAutoDetectMode = false;
        _currentSyntaxName = lang;
        ApplySyntax(lang);
        UpdateSyntaxMenuSelection();
        UpdateStatusBar();
    }

    private void AutoDetectAndApplySyntaxFromContent()
    {
        string detected = "Plain Text";
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            string extLang = SyntaxService.GetLanguageByExtension(_currentFilePath);
            if (extLang != "Plain Text")
            {
                detected = extLang;
            }
        }

        if (detected == "Plain Text")
        {
            detected = SyntaxService.DetectLanguageFromContent(MainEditor.Document.Text);
        }

        if (detected != _currentSyntaxName)
        {
            _currentSyntaxName = detected;
            ApplySyntax(_currentSyntaxName);
            UpdateSyntaxMenuSelection();
        }
    }

    private void UpdateSyntaxMenuSelection()
    {
        foreach (var itemObj in MenuSyntaxParent.Items)
        {
            if (itemObj is MenuItem item)
            {
                if (item.Header?.ToString() == "Auto-Detect Language")
                {
                    item.IsChecked = _isAutoDetectMode;
                }
                else if (item.Header is string headerStr)
                {
                    item.IsChecked = (headerStr == _currentSyntaxName);
                }
            }
        }
    }

    private void ApplySyntax(string syntaxName)
    {
        MainEditor.SyntaxHighlighting = SyntaxService.GetDefinition(syntaxName);
    }

    private void BtnLanguageSelector_Click(object sender, RoutedEventArgs e)
    {
        var contextMenu = new ContextMenu();

        var autoDetectItem = new MenuItem
        {
            Header = "Auto-Detect Language",
            IsCheckable = true,
            IsChecked = _isAutoDetectMode
        };
        autoDetectItem.Click += (s, ev) => SetAutoDetectMode(true);
        contextMenu.Items.Add(autoDetectItem);
        contextMenu.Items.Add(new Separator());

        foreach (var lang in SyntaxService.SupportedLanguages)
        {
            var item = new MenuItem
            {
                Header = lang,
                IsCheckable = true,
                IsChecked = (lang == _currentSyntaxName)
            };
            item.Click += (s, ev) => SetManualLanguage(lang);
            contextMenu.Items.Add(item);
        }
        contextMenu.PlacementTarget = BtnLanguageSelector;
        contextMenu.IsOpen = true;
    }

    #endregion

    #region Search & Replace

    private void MenuFind_Click(object sender, RoutedEventArgs e)
    {
        QuickSavePanel.Visibility = Visibility.Collapsed;
        GoToLinePanel.Visibility = Visibility.Collapsed;
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
        QuickSavePanel.Visibility = Visibility.Collapsed;
        GoToLinePanel.Visibility = Visibility.Collapsed;
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
        FocusEditor();
    }

    private void SearchOption_Click(object sender, RoutedEventArgs e) => UpdateMatchCount();
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateMatchCount();

    private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Enter)
        {
            e.Handled = true;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                FindPrevious();
            else
                FindNext();
        }
        else if (key == Key.Escape)
        {
            e.Handled = true;
            BtnCloseSearch_Click(sender, e);
        }
    }

    private void TxtReplace_KeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Enter)
        {
            e.Handled = true;
            ReplaceCurrent();
        }
        else if (key == Key.Escape)
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
        QuickSavePanel.Visibility = Visibility.Collapsed;
        SearchPanel.Visibility = Visibility.Collapsed;
        GoToLinePanel.Visibility = Visibility.Visible;
        TxtGoToLine.Text = MainEditor.TextArea.Caret.Line.ToString();
        TxtGoToLine.SelectAll();
        TxtGoToLine.Focus();
    }

    private void BtnCloseGoToLine_Click(object sender, RoutedEventArgs e)
    {
        GoToLinePanel.Visibility = Visibility.Collapsed;
        FocusEditor();
    }

    private void BtnGoToLine_Click(object sender, RoutedEventArgs e) => ExecuteGoToLine();

    private void TxtGoToLine_KeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Enter)
        {
            e.Handled = true;
            ExecuteGoToLine();
        }
        else if (key == Key.Escape)
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
            FocusEditor();
        }
    }

    #endregion

    #region Edit Actions & Shortcuts

    private void MenuUndo_Click(object sender, RoutedEventArgs e) => MainEditor.Undo();
    private void MenuRedo_Click(object sender, RoutedEventArgs e) => MainEditor.Redo();
    private void MenuCut_Click(object sender, RoutedEventArgs e) => MainEditor.Cut();
    private void MenuCopy_Click(object sender, RoutedEventArgs e) => MainEditor.Copy();
    private void MenuPaste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsImage() || HasImageFileInClipboard())
        {
            ExecuteImagePaste();
        }
        else
        {
            MainEditor.Paste();
        }
    }

    #region Image Pasting & External Resource Opening

    private static bool HasImageFileInClipboard()
    {
        try
        {
            if (!Clipboard.ContainsFileDropList()) return false;
            var files = Clipboard.GetFileDropList();
            if (files == null || files.Count == 0) return false;
            string first = files[0] ?? "";
            string ext = Path.GetExtension(first).ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp" or ".svg";
        }
        catch
        {
            return false;
        }
    }

    private void OnEditorPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (Clipboard.ContainsImage() || HasImageFileInClipboard())
        {
            e.CancelCommand();
            ExecuteImagePaste();
        }
    }

    private void ExecuteImagePaste()
    {
        try
        {
            BitmapSource? bitmap = null;

            if (Clipboard.ContainsImage())
            {
                bitmap = Clipboard.GetImage();
            }
            else if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                if (files != null && files.Count > 0)
                {
                    string filePath = files[0]!;
                    if (File.Exists(filePath))
                    {
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(filePath, UriKind.Absolute);
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        bi.Freeze();
                        bitmap = bi;
                    }
                }
            }

            if (bitmap == null)
            {
                MainEditor.Paste();
                return;
            }

            string assetsDir;
            string relativeDir;

            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                string docDir = Path.GetDirectoryName(_currentFilePath)!;
                assetsDir = Path.Combine(docDir, "assets");
                relativeDir = "assets";
            }
            else
            {
                assetsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NightPad", "assets");
                relativeDir = assetsDir.Replace('\\', '/');
            }

            if (!Directory.Exists(assetsDir))
            {
                Directory.CreateDirectory(assetsDir);
            }

            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"image_{timeStamp}.png";
            string targetFilePath = Path.Combine(assetsDir, fileName);

            int counter = 1;
            while (File.Exists(targetFilePath))
            {
                fileName = $"image_{timeStamp}_{counter}.png";
                targetFilePath = Path.Combine(assetsDir, fileName);
                counter++;
            }

            using (var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);
            }

            string markdownRef = $"![image]({relativeDir}/{fileName})";

            int caret = MainEditor.CaretOffset;
            MainEditor.Document.Insert(caret, markdownRef);
            MainEditor.CaretOffset = caret + markdownRef.Length;

            UpdateStatusBar();
            if (PreviewContainer.Visibility == Visibility.Visible)
            {
                UpdateMarkdownPreview();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not paste image:\n{ex.Message}", "Image Paste Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public void OpenResolvedPathExternally(string path)
    {
        try
        {
            string target = path.Trim().Trim('"', '\'');
            if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                return;
            }

            string fullPath = target;
            if (!Path.IsPathRooted(target) && !string.IsNullOrEmpty(_currentFilePath))
            {
                string docDir = Path.GetDirectoryName(_currentFilePath)!;
                fullPath = Path.GetFullPath(Path.Combine(docDir, target));
            }

            if (File.Exists(fullPath) || Directory.Exists(fullPath))
            {
                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show($"File or directory not found:\n{fullPath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open externally:\n{ex.Message}", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string? GetLinkOrImagePathAtOffset(int offset)
    {
        if (offset < 0 || offset > MainEditor.Document.TextLength) return null;
        var line = MainEditor.Document.GetLineByOffset(offset);
        string lineText = MainEditor.Document.GetText(line.Offset, line.Length);
        int relOffset = offset - line.Offset;

        // 1. Check for markdown image: ![alt](url)
        var imgMatches = Regex.Matches(lineText, @"!\[.*?\]\((.*?)\)");
        foreach (Match m in imgMatches)
        {
            if (relOffset >= m.Index && relOffset <= m.Index + m.Length)
            {
                return m.Groups[1].Value.Trim();
            }
        }

        // 2. Check for markdown link: [text](url)
        var linkMatches = Regex.Matches(lineText, @"\[.*?\]\((.*?)\)");
        foreach (Match m in linkMatches)
        {
            if (relOffset >= m.Index && relOffset <= m.Index + m.Length)
            {
                return m.Groups[1].Value.Trim();
            }
        }

        // 3. Check for URLs: https?://...
        var urlMatches = Regex.Matches(lineText, @"https?://[^\s<>""']+");
        foreach (Match m in urlMatches)
        {
            if (relOffset >= m.Index && relOffset <= m.Index + m.Length)
            {
                return m.Value.Trim();
            }
        }

        return null;
    }

    private string? GetLinkOrImagePathAtCaret()
    {
        return GetLinkOrImagePathAtOffset(MainEditor.CaretOffset);
    }

    private void MenuOpenLink_Click(object sender, RoutedEventArgs e)
    {
        string? link = GetLinkOrImagePathAtCaret();
        if (string.IsNullOrEmpty(link))
        {
            // Try entire line
            var line = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
            string lineText = MainEditor.Document.GetText(line.Offset, line.Length);
            var m = Regex.Match(lineText, @"!?\[.*?\]\((.*?)\)");
            if (m.Success)
            {
                link = m.Groups[1].Value.Trim();
            }
            else
            {
                var u = Regex.Match(lineText, @"https?://[^\s<>""']+");
                if (u.Success) link = u.Value.Trim();
            }
        }

        if (!string.IsNullOrEmpty(link))
        {
            OpenResolvedPathExternally(link);
        }
        else
        {
            MessageBox.Show("No image or link found at current caret position or line.\nMove cursor over an image or link like ![image](path.png) or [link](url).", "Open Link / Image", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void TextArea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            var pos = MainEditor.GetPositionFromPoint(e.GetPosition(MainEditor));
            if (pos.HasValue)
            {
                int offset = MainEditor.Document.GetOffset(pos.Value.Line, pos.Value.Column);
                string? link = GetLinkOrImagePathAtOffset(offset);
                if (!string.IsNullOrEmpty(link))
                {
                    e.Handled = true;
                    OpenResolvedPathExternally(link);
                }
            }
        }
    }

    private void MainEditor_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        var menu = new ContextMenu();

        string? linkUnderCaret = GetLinkOrImagePathAtCaret();
        if (string.IsNullOrEmpty(linkUnderCaret))
        {
            var line = MainEditor.Document.GetLineByNumber(MainEditor.TextArea.Caret.Line);
            string lineText = MainEditor.Document.GetText(line.Offset, line.Length);
            var m = Regex.Match(lineText, @"!?\[.*?\]\((.*?)\)");
            if (m.Success) linkUnderCaret = m.Groups[1].Value.Trim();
        }

        if (!string.IsNullOrEmpty(linkUnderCaret))
        {
            string cleanTarget = linkUnderCaret.Trim('"', '\'');
            string fileName = Path.GetFileName(cleanTarget);
            var openItem = new MenuItem
            {
                Header = $"🖼️ Open Externally: {(string.IsNullOrEmpty(fileName) ? cleanTarget : fileName)}",
                FontWeight = FontWeights.SemiBold
            };
            openItem.Click += (s, ev) => OpenResolvedPathExternally(cleanTarget);
            menu.Items.Add(openItem);

            string fullPath = cleanTarget;
            if (!Path.IsPathRooted(cleanTarget) && !string.IsNullOrEmpty(_currentFilePath))
            {
                fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_currentFilePath)!, cleanTarget));
            }

            if (File.Exists(fullPath))
            {
                var revealItem = new MenuItem { Header = "📁 Reveal in File Explorer" };
                revealItem.Click += (s, ev) =>
                {
                    try { Process.Start("explorer.exe", $"/select,\"{fullPath}\""); } catch { }
                };
                menu.Items.Add(revealItem);
            }

            menu.Items.Add(new Separator());
        }

        var undoItem = new MenuItem { Header = "Undo", InputGestureText = "Ctrl+Z" };
        undoItem.Click += (s, ev) => MainEditor.Undo();
        var redoItem = new MenuItem { Header = "Redo", InputGestureText = "Ctrl+Y" };
        redoItem.Click += (s, ev) => MainEditor.Redo();
        menu.Items.Add(undoItem);
        menu.Items.Add(redoItem);
        menu.Items.Add(new Separator());

        var cutItem = new MenuItem { Header = "Cut", InputGestureText = "Ctrl+X" };
        cutItem.Click += (s, ev) => MainEditor.Cut();
        var copyItem = new MenuItem { Header = "Copy", InputGestureText = "Ctrl+C" };
        copyItem.Click += (s, ev) => MainEditor.Copy();

        var pasteItem = new MenuItem
        {
            Header = (Clipboard.ContainsImage() || HasImageFileInClipboard()) ? "📋 Paste (Image)" : "📋 Paste",
            InputGestureText = "Ctrl+V"
        };
        pasteItem.Click += (s, ev) => MenuPaste_Click(s, ev);

        menu.Items.Add(cutItem);
        menu.Items.Add(copyItem);
        menu.Items.Add(pasteItem);

        var selectAllItem = new MenuItem { Header = "Select All", InputGestureText = "Ctrl+A" };
        selectAllItem.Click += (s, ev) => MainEditor.SelectAll();
        menu.Items.Add(selectAllItem);

        menu.Items.Add(new Separator());

        var wordWrapItem = new MenuItem { Header = "Word Wrap", InputGestureText = "Alt+Z", IsCheckable = true, IsChecked = MainEditor.WordWrap };
        wordWrapItem.Click += (s, ev) =>
        {
            MainEditor.WordWrap = !MainEditor.WordWrap;
            MenuWordWrap.IsChecked = MainEditor.WordWrap;
        };
        menu.Items.Add(wordWrapItem);

        MainEditor.ContextMenu = menu;
    }

    #endregion

    #region Lightweight External File Watcher

    private void SetupFileWatcher(string? filePath)
    {
        DisposeFileWatcher();
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;

        try
        {
            string? dir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

            _lastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);

            _fileWatcher = new FileSystemWatcher(dir, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileWatcherChanged;
            _fileWatcher.Renamed += OnFileWatcherRenamed;
            _fileWatcher.Deleted += OnFileWatcherDeleted;
        }
        catch
        {
            // Resilient against system folder / security restrictions
        }
    }

    private void DisposeFileWatcher()
    {
        if (_fileWatcher != null)
        {
            try
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Changed -= OnFileWatcherChanged;
                _fileWatcher.Renamed -= OnFileWatcherRenamed;
                _fileWatcher.Deleted -= OnFileWatcherDeleted;
                _fileWatcher.Dispose();
            }
            catch { }
            _fileWatcher = null;
        }
    }

    private void OnFileWatcherChanged(object sender, FileSystemEventArgs e)
    {
        if (_isSavingInternal) return;
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

        try
        {
            var currentWrite = File.GetLastWriteTimeUtc(_currentFilePath);
            if (currentWrite <= _lastKnownWriteTimeUtc.AddMilliseconds(100)) return;
            _lastKnownWriteTimeUtc = currentWrite;
            _hasPendingExternalChange = true;

            Dispatcher.InvokeAsync(() =>
            {
                ShowExternalChangeNotification();
            });
        }
        catch { }
    }

    private void OnFileWatcherRenamed(object sender, RenamedEventArgs e)
    {
        if (_isSavingInternal) return;
        _hasPendingExternalChange = true;
        Dispatcher.InvokeAsync(() =>
        {
            TxtExternalChangeMessage.Text = $"File was renamed to '{e.Name}' outside Notepad.";
            ExternalChangeBanner.Visibility = Visibility.Visible;
        });
    }

    private void OnFileWatcherDeleted(object sender, FileSystemEventArgs e)
    {
        if (_isSavingInternal) return;
        _hasPendingExternalChange = true;
        Dispatcher.InvokeAsync(() =>
        {
            TxtExternalChangeMessage.Text = "File was deleted or moved outside Notepad.";
            ExternalChangeBanner.Visibility = Visibility.Visible;
        });
    }

    private void ShowExternalChangeNotification()
    {
        if (string.IsNullOrEmpty(_currentFilePath)) return;
        string fileName = Path.GetFileName(_currentFilePath);
        TxtExternalChangeMessage.Text = $"'{fileName}' has been modified outside Notepad.";
        ExternalChangeBanner.Visibility = Visibility.Visible;
    }

    private void BtnReloadExternal_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
        {
            ExternalChangeBanner.Visibility = Visibility.Collapsed;
            _hasPendingExternalChange = false;
            return;
        }

        try
        {
            int savedCaret = MainEditor.CaretOffset;
            string newContent = File.ReadAllText(_currentFilePath, Encoding.UTF8);
            MainEditor.Document.Text = newContent;
            MainEditor.CaretOffset = Math.Clamp(savedCaret, 0, newContent.Length);

            _isModified = false;
            _hasPendingExternalChange = false;
            _lastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(_currentFilePath);
            ExternalChangeBanner.Visibility = Visibility.Collapsed;

            UpdateTitle();
            UpdateStatusBar();
            if (PreviewContainer.Visibility == Visibility.Visible)
            {
                UpdateMarkdownPreview();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not reload file:\n{ex.Message}", "Reload Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnIgnoreExternal_Click(object sender, RoutedEventArgs e)
    {
        _hasPendingExternalChange = false;
        if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
        {
            try { _lastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(_currentFilePath); } catch { }
        }
        ExternalChangeBanner.Visibility = Visibility.Collapsed;
    }

    #endregion
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
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var modifiers = Keyboard.Modifiers;

        // Check Control + Alt combinations
        if (modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Alt))
        {
            if (key == Key.S) { e.Handled = true; SaveAsDialog(); return; }
        }

        // Check Control + Shift combinations
        if (modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Shift))
        {
            if (key == Key.S) { e.Handled = true; ShowQuickSave(); return; }
            if (key == Key.M) { e.Handled = true; ToggleMarkdownPreview(); return; }
            if (key == Key.R) { e.Handled = true; ToggleTextDirection(); return; }
            if (key == Key.L) { e.Handled = true; MenuLineNumbers.IsChecked = !MenuLineNumbers.IsChecked; MenuLineNumbers_Click(sender, e); return; }
            if (key == Key.K) { e.Handled = true; MenuDeleteLine_Click(sender, e); return; }
            if (key == Key.J) { e.Handled = true; MenuFormatJson_Click(sender, e); return; }
            if (key == Key.U) { e.Handled = true; MenuUpper_Click(sender, e); return; }
        }

        // Check Control combinations
        if (modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Shift) && !modifiers.HasFlag(ModifierKeys.Alt))
        {
            if (key == Key.N) { e.Handled = true; NewFile(); return; }
            if (key == Key.O) { e.Handled = true; MenuOpen_Click(sender, e); return; }
            if (key == Key.S) { e.Handled = true; SaveFile(); return; }
            if (key == Key.F) { e.Handled = true; MenuFind_Click(sender, e); return; }
            if (key == Key.H) { e.Handled = true; MenuReplace_Click(sender, e); return; }
            if (key == Key.G) { e.Handled = true; MenuGoToLine_Click(sender, e); return; }
            if (key == Key.D) { e.Handled = true; MenuDuplicateLine_Click(sender, e); return; }
            if (key is Key.OemQuestion or Key.Divide) { e.Handled = true; MenuToggleComment_Click(sender, e); return; }
            if (key is Key.Add or Key.OemPlus) { e.Handled = true; ZoomIn(); return; }
            if (key is Key.Subtract or Key.OemMinus) { e.Handled = true; ZoomOut(); return; }
            if (key is Key.D0 or Key.NumPad0) { e.Handled = true; ZoomReset(); return; }
            if (key is Key.OemPeriod or Key.Decimal) { e.Handled = true; ShowQuickSymbols(); return; }
        }

        // Check Windows + Shift combinations (e.g. Win + Shift + W)
        bool isWinPressed = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
        if (isWinPressed && modifiers.HasFlag(ModifierKeys.Shift))
        {
            if (key == Key.W)
            {
                e.Handled = true;
                ShowQuickSymbols();
                return;
            }
        }

        // Check Alt combinations (using resolved key)
        if (modifiers.HasFlag(ModifierKeys.Alt) && !modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Shift))
        {
            if (key == Key.Z) { e.Handled = true; MenuWordWrap.IsChecked = !MenuWordWrap.IsChecked; MenuWordWrap_Click(sender, e); return; }
            if (key == Key.M) { e.Handled = true; ToggleMarkdownPreview(); return; }
            if (key == Key.O) { e.Handled = true; MenuOpenLink_Click(sender, e); return; }
            if (key == Key.Up) { e.Handled = true; MenuMoveLineUp_Click(sender, e); return; }
            if (key == Key.Down) { e.Handled = true; MenuMoveLineDown_Click(sender, e); return; }
            if (key == Key.S) { e.Handled = true; ShowQuickSave(); return; }
        }

        // Single key shortcuts
        if (modifiers == ModifierKeys.None)
        {
            if (key == Key.F2)
            {
                if (QuickSavePanel.Visibility != Visibility.Visible)
                {
                    e.Handled = true;
                    ShowQuickSave();
                    return;
                }
            }
            else if (key == Key.F3)
            {
                e.Handled = true;
                FindNext();
                return;
            }
            else if (key == Key.F4)
            {
                if (QuickSymbolsOverlay.Visibility != Visibility.Visible)
                {
                    e.Handled = true;
                    ShowQuickSymbols();
                    return;
                }
            }
            else if (key == Key.F5)
            {
                if (ExternalChangeBanner.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    BtnReloadExternal_Click(sender, e);
                    return;
                }
                e.Handled = true;
                MenuInsertDateTime_Click(sender, e);
                return;
            }
            else if (key == Key.Escape)
            {
                if (QuickSymbolsOverlay.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    CloseQuickSymbols();
                    return;
                }
                if (ExternalChangeBanner.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    BtnIgnoreExternal_Click(sender, e);
                    return;
                }
                if (QuickSavePanel.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    CloseQuickSave();
                    return;
                }
                if (SearchPanel.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    BtnCloseSearch_Click(sender, e);
                    return;
                }
                if (GoToLinePanel.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    BtnCloseGoToLine_Click(sender, e);
                    return;
                }
                if (PreviewContainer.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    ToggleMarkdownPreview();
                    return;
                }
            }
        }
        else if (modifiers == ModifierKeys.Shift && key == Key.F3)
        {
            e.Handled = true;
            FindPrevious();
            return;
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
        int words = ArabicTextService.CountWords(text);

        StatusDocStats.Text = $"{lineCount:N0} line{(lineCount == 1 ? "" : "s")}, {words:N0} words, {charCount:N0} chars";

        if (text.Contains("\r\n")) StatusEol.Text = "Windows (CRLF)";
        else if (text.Contains('\n')) StatusEol.Text = "Unix (LF)";
        else StatusEol.Text = "Windows (CRLF)";

        BtnLanguageSelector.Content = _currentSyntaxName;
        BtnLanguageSelector.ToolTip = $"Syntax Highlighting: {_currentSyntaxName} {(_isAutoDetectMode ? "(Auto-Detected)" : "(Manual)")} - Click to switch language";
    }

    #region Quick Symbols & Words Palette

    public void ShowQuickSymbols()
    {
        // Collapse other panels to keep workspace clean
        QuickSavePanel.Visibility = Visibility.Collapsed;
        SearchPanel.Visibility = Visibility.Collapsed;
        GoToLinePanel.Visibility = Visibility.Collapsed;

        if (_allSymbols.Count == 0)
        {
            _allSymbols = QuickSymbolService.LoadItems();
        }

        QuickSymbolsOverlay.Visibility = Visibility.Visible;
        TxtQuickSymbolSearch.Text = string.Empty;
        _selectedCategoryFilter = "All";
        BuildCategoryChips();
        RefreshQuickSymbolsList();

        Dispatcher.InvokeAsync(() =>
        {
            TxtQuickSymbolSearch.Focus();
            Keyboard.Focus(TxtQuickSymbolSearch);
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    public void CloseQuickSymbols()
    {
        QuickSymbolsOverlay.Visibility = Visibility.Collapsed;
        FocusEditor();
    }

    private void QuickSymbolsOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        CloseQuickSymbols();
    }

    private void QuickSymbolsCard_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void BtnCloseQuickSymbols_Click(object sender, RoutedEventArgs e)
    {
        CloseQuickSymbols();
    }

    private void MenuQuickSymbols_Click(object sender, RoutedEventArgs e)
    {
        ShowQuickSymbols();
    }

    private void BuildCategoryChips()
    {
        CategoryFilterPanel.Children.Clear();

        var categories = new List<string> { "All" };
        var customCats = _allSymbols
            .Select(s => s.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c);
        categories.AddRange(customCats);

        foreach (var cat in categories)
        {
            var btn = new Button
            {
                Content = cat,
                Style = (Style)FindResource("NightButton"),
                Padding = new Thickness(7, 2, 7, 2),
                Margin = new Thickness(0, 0, 4, 4),
                FontSize = 11,
                Tag = cat
            };

            UpdateCategoryChipStyle(btn, cat.Equals(_selectedCategoryFilter, StringComparison.OrdinalIgnoreCase));

            btn.Click += (s, e) =>
            {
                if (s is Button clickedBtn && clickedBtn.Tag is string catTag)
                {
                    _selectedCategoryFilter = catTag;
                    foreach (UIElement child in CategoryFilterPanel.Children)
                    {
                        if (child is Button childBtn && childBtn.Tag is string childTag)
                        {
                            UpdateCategoryChipStyle(childBtn, childTag.Equals(_selectedCategoryFilter, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                    RefreshQuickSymbolsList();
                    TxtQuickSymbolSearch.Focus();
                }
            };

            CategoryFilterPanel.Children.Add(btn);
        }
    }

    private void UpdateCategoryChipStyle(Button btn, bool isSelected)
    {
        if (isSelected)
        {
            btn.Background = (SolidColorBrush)FindResource("AccentBlueBrush");
            btn.Foreground = (SolidColorBrush)FindResource("BgDarkBrush");
            btn.BorderBrush = (SolidColorBrush)FindResource("AccentBlueBrush");
            btn.FontWeight = FontWeights.Bold;
        }
        else
        {
            btn.Background = (SolidColorBrush)FindResource("BgSurfaceBrush");
            btn.Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush");
            btn.BorderBrush = (SolidColorBrush)FindResource("BorderDarkBrush");
            btn.FontWeight = FontWeights.Normal;
        }
    }

    private void TxtQuickSymbolSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshQuickSymbolsList();
    }

    private void RefreshQuickSymbolsList()
    {
        string query = TxtQuickSymbolSearch.Text.Trim();

        IEnumerable<QuickSymbolItem> queryable = _allSymbols;

        if (!string.Equals(_selectedCategoryFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            queryable = queryable.Where(s => string.Equals(s.Category, _selectedCategoryFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query))
        {
            queryable = queryable
                .Where(s => s.Text.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || s.Label.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || s.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s =>
                {
                    if (string.Equals(s.Text, query, StringComparison.OrdinalIgnoreCase)) return 0;
                    if (string.Equals(s.Label, query, StringComparison.OrdinalIgnoreCase)) return 1;
                    if (s.Text.StartsWith(query, StringComparison.OrdinalIgnoreCase)) return 2;
                    if (s.Label.StartsWith(query, StringComparison.OrdinalIgnoreCase)) return 3;
                    return 4;
                });
        }

        _filteredSymbols = queryable.ToList();
        LstQuickSymbols.ItemsSource = null;
        LstQuickSymbols.ItemsSource = _filteredSymbols;

        if (_filteredSymbols.Count > 0)
        {
            LstQuickSymbols.SelectedIndex = 0;
            LstQuickSymbols.ScrollIntoView(LstQuickSymbols.SelectedItem);
        }

        TxtQuickSymbolsCount.Text = $"{_filteredSymbols.Count} item{(_filteredSymbols.Count == 1 ? "" : "s")}";
    }

    private void InsertQuickSymbol(QuickSymbolItem? item)
    {
        if (item == null) return;

        CloseQuickSymbols();

        if (!string.IsNullOrEmpty(MainEditor.SelectedText))
        {
            int start = MainEditor.SelectionStart;
            MainEditor.Document.Replace(start, MainEditor.SelectionLength, item.Text);
            MainEditor.CaretOffset = start + item.Text.Length;
        }
        else
        {
            int offset = MainEditor.CaretOffset;
            MainEditor.Document.Insert(offset, item.Text);
            MainEditor.CaretOffset = offset + item.Text.Length;
        }

        FocusEditor();
    }

    private void TxtQuickSymbolSearch_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Down)
        {
            if (_filteredSymbols.Count > 0)
            {
                int next = Math.Min(_filteredSymbols.Count - 1, LstQuickSymbols.SelectedIndex + 1);
                LstQuickSymbols.SelectedIndex = next;
                LstQuickSymbols.ScrollIntoView(LstQuickSymbols.SelectedItem);
            }
            e.Handled = true;
        }
        else if (key == Key.Up)
        {
            if (_filteredSymbols.Count > 0)
            {
                int prev = Math.Max(0, LstQuickSymbols.SelectedIndex - 1);
                LstQuickSymbols.SelectedIndex = prev;
                LstQuickSymbols.ScrollIntoView(LstQuickSymbols.SelectedItem);
            }
            e.Handled = true;
        }
        else if (key == Key.PageDown)
        {
            if (_filteredSymbols.Count > 0)
            {
                int next = Math.Min(_filteredSymbols.Count - 1, LstQuickSymbols.SelectedIndex + 5);
                LstQuickSymbols.SelectedIndex = next;
                LstQuickSymbols.ScrollIntoView(LstQuickSymbols.SelectedItem);
            }
            e.Handled = true;
        }
        else if (key == Key.PageUp)
        {
            if (_filteredSymbols.Count > 0)
            {
                int prev = Math.Max(0, LstQuickSymbols.SelectedIndex - 5);
                LstQuickSymbols.SelectedIndex = prev;
                LstQuickSymbols.ScrollIntoView(LstQuickSymbols.SelectedItem);
            }
            e.Handled = true;
        }
        else if (key == Key.Enter)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                AddNewQuickSymbolFromSearch();
                e.Handled = true;
            }
            else
            {
                if (LstQuickSymbols.SelectedItem is QuickSymbolItem selected)
                {
                    InsertQuickSymbol(selected);
                }
                else if (!string.IsNullOrWhiteSpace(TxtQuickSymbolSearch.Text))
                {
                    AddNewQuickSymbolFromSearch();
                }
                e.Handled = true;
            }
        }
        else if (key == Key.Escape)
        {
            CloseQuickSymbols();
            e.Handled = true;
        }
    }

    private void LstQuickSymbols_KeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Enter)
        {
            if (LstQuickSymbols.SelectedItem is QuickSymbolItem selected)
            {
                InsertQuickSymbol(selected);
            }
            e.Handled = true;
        }
        else if (key == Key.Delete)
        {
            if (LstQuickSymbols.SelectedItem is QuickSymbolItem selected)
            {
                DeleteQuickSymbol(selected);
            }
            e.Handled = true;
        }
        else if (key == Key.Escape)
        {
            CloseQuickSymbols();
            e.Handled = true;
        }
    }

    private void LstQuickSymbols_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LstQuickSymbols.SelectedItem is QuickSymbolItem selected)
        {
            InsertQuickSymbol(selected);
        }
    }

    private void BtnAddQuickSymbol_Click(object sender, RoutedEventArgs e)
    {
        AddNewQuickSymbolFromSearch();
    }

    private void AddNewQuickSymbolFromSearch()
    {
        string text = TxtQuickSymbolSearch.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var existing = _allSymbols.FirstOrDefault(s => string.Equals(s.Text, text, StringComparison.Ordinal));
        if (existing != null)
        {
            InsertQuickSymbol(existing);
            return;
        }

        string cat = _selectedCategoryFilter == "All" ? "Custom" : _selectedCategoryFilter;
        var newItem = new QuickSymbolItem(text, text, cat, true);

        _allSymbols.Insert(0, newItem);
        QuickSymbolService.SaveItems(_allSymbols);

        BuildCategoryChips();
        RefreshQuickSymbolsList();

        LstQuickSymbols.SelectedItem = newItem;
        LstQuickSymbols.ScrollIntoView(newItem);
        TxtQuickSymbolSearch.Focus();
    }

    private void BtnDeleteQuickSymbol_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is QuickSymbolItem item)
        {
            DeleteQuickSymbol(item);
        }
    }

    private void DeleteQuickSymbol(QuickSymbolItem? item)
    {
        if (item == null) return;

        int currentIndex = LstQuickSymbols.SelectedIndex;
        _allSymbols.Remove(item);
        QuickSymbolService.SaveItems(_allSymbols);

        BuildCategoryChips();
        RefreshQuickSymbolsList();

        if (_filteredSymbols.Count > 0)
        {
            int nextIndex = Math.Min(currentIndex, _filteredSymbols.Count - 1);
            LstQuickSymbols.SelectedIndex = nextIndex;
            LstQuickSymbols.ScrollIntoView(LstQuickSymbols.SelectedItem);
        }

        TxtQuickSymbolSearch.Focus();
    }

    #endregion

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_isModified && !PromptSaveBeforeAction())
        {
            e.Cancel = true;
            return;
        }

        DisposeFileWatcher();
        base.OnClosing(e);
    }

    #endregion
}