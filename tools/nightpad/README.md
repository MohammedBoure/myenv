# Notepad Source Directory (`tools/nightpad`)

Source code for **Notepad** (NightPad), the lightweight, simple, and high-performance text and code editor built natively for the MyEnv desktop environment.

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`App.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml) | Application XAML defining clean dark brushes, menus, context menus, buttons, textboxes, and scrollbars. |
| [`App.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml.cs) | Application entry point handling command-line arguments (file paths) and window lifecycle. |
| [`MainWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml) | Clean minimalist window layout containing the title bar, classic Menu Bar (`File`, `Edit`, `View`, `Tools`), Search/Replace, Go-To-Line, Text Editor, and bottom Taskbar/Status Bar. |
| [`MainWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml.cs) | Core editor logic managing document buffer, file I/O, search/replace, line operations, Python smart indentation, and shortcuts. |
| [`NightPad.csproj`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/NightPad.csproj) | .NET 10 WPF project configuration referencing AvalonEdit. |
| [`Services/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services) | Services for syntax highlighting definitions (`SyntaxService.cs`) and text transformation utilities (`TextTransformService.cs`). |
| [`Resources/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Resources) | Assets and resources. |
