# Quick Symbols and Words Tool Directory (`tools/quick-symbols`)

Source code for **QuickSymbols**, the system-wide keyboard-driven floating palette for quick access and auto-injection of frequently used symbols, mathematical characters, snippets, and words into any focused application across Windows and MyEnv.

## Files and Structure

| File | Purpose |
|---|---|
| [`App.xaml`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols/App.xaml) | Application XAML defining obsidian dark theme brushes, buttons, textboxes, and styling. |
| [`App.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols/App.xaml.cs) | Application entry point capturing the target active window before displaying the palette. |
| [`MainWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols/MainWindow.xaml) | Floating topmost borderless dialog with real-time search input, category filter chips, symbol list, and keyboard shortcuts footer. |
| [`MainWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols/MainWindow.xaml.cs) | Logic for searching, category filtering, adding/deleting custom symbols, and direct Unicode/SendInput text injection into the target application. |
| [`QuickSymbolService.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols/QuickSymbolService.cs) | Service managing persistent JSON storage at `%APPDATA%\NightPad\quick_symbols.json` shared with NightPad. |
| [`QuickSymbols.csproj`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols/QuickSymbols.csproj) | .NET 10 WPF project configuration. |
