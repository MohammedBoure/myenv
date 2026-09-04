using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace QuickSymbols;

public partial class MainWindow : Window
{
    private readonly IntPtr targetHwnd;
    private List<QuickSymbolItem> _allSymbols = new();
    private List<QuickSymbolItem> _filteredSymbols = new();
    private string _selectedCategoryFilter = "All";
    private bool isPasting = false;

    #region Win32 API Imports & Structs
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;

    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;
    private const int VK_RETURN = 0x0D;
    private const int VK_V = 0x56;
    private const int VK_INSERT = 0x2D;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }
    #endregion

    public MainWindow(IntPtr previousForegroundHwnd)
    {
        InitializeComponent();
        targetHwnd = previousForegroundHwnd;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _allSymbols = QuickSymbolService.LoadItems();
        BuildCategoryChips();
        RefreshQuickSymbolsList();

        Dispatcher.InvokeAsync(() =>
        {
            TxtQuickSymbolSearch.Focus();
            Keyboard.Focus(TxtQuickSymbolSearch);
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            this.Close();
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
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
                    _ = InsertAndPasteToTargetAsync(selected.Text);
                }
                else if (!string.IsNullOrWhiteSpace(TxtQuickSymbolSearch.Text))
                {
                    string text = TxtQuickSymbolSearch.Text.Trim();
                    AddNewQuickSymbolFromSearch();
                    _ = InsertAndPasteToTargetAsync(text);
                }
                e.Handled = true;
            }
        }
        else if (key == Key.Escape)
        {
            this.Close();
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
                _ = InsertAndPasteToTargetAsync(selected.Text);
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
            this.Close();
            e.Handled = true;
        }
    }

    private void LstQuickSymbols_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LstQuickSymbols.SelectedItem is QuickSymbolItem selected)
        {
            _ = InsertAndPasteToTargetAsync(selected.Text);
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
            _ = InsertAndPasteToTargetAsync(existing.Text);
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

    public async Task InsertAndPasteToTargetAsync(string textToPaste)
    {
        if (isPasting) return;
        isPasting = true;

        if (string.IsNullOrEmpty(textToPaste))
        {
            this.Close();
            return;
        }

        // 1. Hide UI immediately for instant perception
        this.Hide();

        // 2. Put text into Windows Clipboard (as reliable backup)
        try
        {
            Clipboard.SetText(textToPaste);
        }
        catch { }

        // 3. Restore target window focus cleanly
        RestoreTargetFocus(targetHwnd);

        // 4. Short delay for active window focus switch
        await Task.Delay(50);

        // 5. Release modifier keys only if physically pressed
        ReleasePhysicallyPressedModifiers();

        // 6. Direct Unicode Text Injection
        bool typedViaUnicode = TypeTextUnicode(textToPaste);

        // 7. Fallback to smart keystrokes if Unicode injection was not sent
        if (!typedViaUnicode)
        {
            try
            {
                SendCtrlV();
            }
            catch { }
        }

        await Task.Delay(30);
        this.Close();
    }

    #region Non-Invasive Focus & Direct Typing Helpers
    private static void RestoreTargetFocus(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) return;

        try
        {
            IntPtr foregroundWnd = GetForegroundWindow();
            if (foregroundWnd == hWnd) return;

            uint targetThread = GetWindowThreadProcessId(hWnd, out _);
            uint currentThread = GetCurrentThreadId();

            if (targetThread != currentThread && targetThread != 0)
            {
                AttachThreadInput(currentThread, targetThread, true);
                BringWindowToTop(hWnd);
                SetForegroundWindow(hWnd);
                SetFocus(hWnd);
                AttachThreadInput(currentThread, targetThread, false);
            }
            else
            {
                BringWindowToTop(hWnd);
                SetForegroundWindow(hWnd);
                SetFocus(hWnd);
            }
        }
        catch { }
    }

    private static bool TypeTextUnicode(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        try
        {
            var inputs = new List<INPUT>(text.Length * 2);
            int structSize = Marshal.SizeOf<INPUT>();

            foreach (char c in text)
            {
                if (c == '\r') continue;

                // Key Down
                inputs.Add(new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = c,
                            dwFlags = KEYEVENTF_UNICODE,
                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                });

                // Key Up
                inputs.Add(new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = c,
                            dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                });
            }

            uint result = SendInput((uint)inputs.Count, inputs.ToArray(), structSize);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void ReleasePhysicallyPressedModifiers()
    {
        try
        {
            if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if ((GetAsyncKeyState(VK_LWIN) & 0x8000) != 0)
                keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if ((GetAsyncKeyState(VK_RWIN) & 0x8000) != 0)
                keybd_event(VK_RWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if ((GetAsyncKeyState(VK_RETURN) & 0x8000) != 0)
                keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
        catch { }
    }

    private static void SendCtrlV()
    {
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        keybd_event(VK_V, 0, 0, UIntPtr.Zero);
        keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
    #endregion
}
