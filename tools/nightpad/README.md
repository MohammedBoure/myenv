# Notepad Source Directory (`tools/nightpad`)

Source code for **Notepad** (NightPad), the lightweight, simple, and high-performance keyboard-driven text and code editor built natively for the MyEnv desktop environment with real-time Markdown review and side-by-side live preview.

## Files and Structure

| File / Folder | Purpose |
|---|---|
| [`App.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml) | Application XAML defining clean dark brushes, menus, context menus, buttons, textboxes, and scrollbars. |
| [`App.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml.cs) | Application entry point handling command-line arguments (file paths) and window lifecycle. |
| [`MainWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml) | Minimalist window layout containing the unified single-row top bar (title, branding, menus `File`/`Edit`/`View`/`Tools`, Markdown preview toggle, window controls), external modification notification banner, Quick Save bar, Search/Replace, Go-To-Line, AvalonEdit text editor with word wrapping and Arabic typography, split resizable Markdown Live Preview, and status bar. |
| [`MainWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml.cs) | Core editor logic managing document buffer, instant focus on launch, lightweight kernel-level external file watching via `FileSystemWatcher`, image pasting with automatic `./assets/` saving and Markdown reference generation, Ctrl+Click / Alt+O external image opening, smooth line navigation (`Ctrl+Enter` / `Ctrl+Shift+Enter`), fast keyboard saving, and Arabic RTL support. |
| [`NightPad.csproj`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/NightPad.csproj) | .NET 10 WPF project configuration referencing AvalonEdit. |
| [`Services/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services) | Services for syntax highlighting (`SyntaxService.cs`), Markdown and image rendering (`MarkdownRenderService.cs`), path auto-completion (`PathCompletionService.cs`), Arabic language handling (`ArabicTextService.cs`), and text transformations (`TextTransformService.cs`). |
| [`Resources/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Resources) | Assets and resources. |
