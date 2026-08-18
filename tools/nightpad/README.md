# Notepad Source Directory (`tools/nightpad`)

Source code for **Notepad** (NightPad), the lightweight, simple, and high-performance keyboard-driven text and code editor built natively for the MyEnv desktop environment with real-time Markdown review and side-by-side live preview.

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`App.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml) | Application XAML defining clean dark brushes, menus, context menus, buttons, textboxes, and scrollbars. |
| [`App.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml.cs) | Application entry point handling command-line arguments (file paths) and window lifecycle. |
| [`MainWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml) | Minimalist window layout containing the title bar with Markdown Preview button, classic Menu Bar (`File`, `Edit`, `View`, `Tools`), Quick Save Bar, Search/Replace, Go-To-Line, AvalonEdit Text Editor with Arabic font fallbacks, split resizable Markdown Live Preview, and Status Bar. |
| [`MainWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml.cs) | Core editor logic managing document buffer, instant focus on start, fast terminal-style keyboard saving with Tab completion and directory presets (`F1`-`F4`), real-time side-by-side Markdown review panel, Arabic RTL/LTR bidirectional toggle, and robust shortcut bindings. |
| [`NightPad.csproj`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/NightPad.csproj) | .NET 10 WPF project configuration referencing AvalonEdit. |
| [`Services/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services) | Services for syntax highlighting (`SyntaxService.cs`), Markdown rendering (`MarkdownRenderService.cs`), path auto-completion (`PathCompletionService.cs`), Arabic language handling (`ArabicTextService.cs`), and text transformations (`TextTransformService.cs`). |
| [`Resources/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Resources) | Assets and resources. |
