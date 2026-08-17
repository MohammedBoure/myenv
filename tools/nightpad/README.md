# NightPad Source Directory (`tools/nightpad`)

Source code for **NightPad**, the professional Obsidian Night Mode text and code editor built natively for the MyEnv desktop environment.

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`App.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml) | Application XAML defining obsidian dark brushes, menu styles, context menus, buttons, textboxes, and scrollbars. |
| [`App.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/App.xaml.cs) | Application entry point handling command-line arguments (file paths) and window lifecycle. |
| [`MainWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml) | Main window layout containing the custom title bar, document tabs, menu bar, quick action toolbar, search/replace bar, markdown preview panel, and status bar. |
| [`MainWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/MainWindow.xaml.cs) | Main window logic managing multi-document tabs, AvalonEdit instances, search/replace, line operations, and shortcuts. |
| [`NightPad.csproj`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/NightPad.csproj) | .NET 10 WPF project configuration referencing AvalonEdit. |
| [`Models/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Models) | Data models representing document state, statistics, and tab tracking. |
| [`Services/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services) | Services for syntax highlighting definitions, text transformation, and JSON/encoding utilities. |
| [`Resources/`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Resources) | Assets and resources for NightPad. |
