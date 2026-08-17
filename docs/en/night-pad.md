# 🌙 NightPad: Professional Night Mode Text Editor

A lightweight, ultra-responsive, keyboard-driven text and code editor built natively for the **MyEnv** Windows desktop environment. Designed with an Obsidian Night Dark theme, 0px sharp rectangular aesthetics, a built-in workspace file explorer sidebar, auto-save engine, and instant startup latency.

---

## 🚀 Overview & Key Highlights

- **Obsidian Dark Aesthetic**: Deep obsidian background (`#0d1117`), surface panels (`#161b22`), high-contrast text (`#f0f6fc`), electric cyan accents (`#58a6ff`), and 0px rectangular sharp edges.
- **Workspace File Explorer Sidebar (`Ctrl + B`)**: Full recursive, lazy-loaded tree view of project files and directories. Double-click or press Enter on any file to open it in a tab.
- **Automatic Auto-Save (`Ctrl + Shift + A`)**: Background auto-save engine (runs every 3 seconds) silently saving dirty files without interrupting your workflow, with a live status indicator (`💾 Auto-Save: ON/OFF`).
- **Multi-Document Tab System**: Seamless tab management, drag-and-drop file opening, dirty-state indicators (`*`), and instant middle-click closing.
- **Rich Python 3 & Multi-Language Support**:
  - **Python**: Full coloring for keywords, built-ins (`print`, `len`, `range`, etc.), magic methods (`__init__`, `__str__`), decorators (`@decorator`), f-strings, docstrings, and smart indentation (pressing Enter after `:` auto-indents 4 spaces).
  - **20+ Languages**: PowerShell, JavaScript, TypeScript, JSON, YAML, Markdown, C#, C/C++, HTML, XML, CSS, PHP, SQL, Batch/CMD, INI/Config, Java, Rust, and Go.
- **Live Markdown Preview (`Ctrl + Shift + M`)**: Split-panel real-time rendering of headers, code blocks, bold/italics, and list items.
- **Advanced Find & Replace (`Ctrl + F` / `Ctrl + H`)**: Regex support, case sensitivity, whole word matching, and live match counters.
- **Developer Productivity Toolkit**: JSON formatter & minifier (`Ctrl + Shift + J`), Base64/URL encoding, case conversion, line deduplication, line sorting, and date/time stamping (`F5`).
- **Deep Desktop Integration**: Native GlazeWM keybinding (`Alt + N`), PowerShell & CMD CLI aliases (`np`, `nightpad`, `notepad`), and automatic tiling rules.

---

## ⚡ Launching NightPad

| Launch Method | Command / Shortcut | Description |
|---|---|---|
| **GlazeWM Hotkey** | `Alt + N` | Instant launch from anywhere on any workspace |
| **PowerShell CLI** | `np [file or folder]` / `nightpad` | Opens specified file/folder or active directory in sidebar |
| **Command Prompt (CMD)** | `np [file or folder]` / `nightpad` | Opens specified file/folder or active directory in sidebar |
| **Windows App Launcher** | `Alt + Shift + Q` -> Type `NightPad` | Searchable via MyEnv WPF application launcher |
| **File Explorer Drag & Drop** | Drag file(s) or folder into NightPad window | Automatically opens files in new tabs or sets workspace |

---

## ⌨️ Keyboard Shortcuts Reference

### File & Workspace Management
| Shortcut | Action |
|---|---|
| `Ctrl + N` | Open a new blank document tab |
| `Ctrl + O` | Open file dialog |
| `Ctrl + Shift + O` | Open Workspace Folder dialog |
| `Ctrl + S` | Save current document |
| `Ctrl + Shift + S` | Save As dialog |
| `Ctrl + Alt + S` | Save all open dirty documents |
| `Ctrl + Shift + A` | Toggle Auto-Save On / Off |
| `Ctrl + W` | Close active document tab |
| `Alt + F4` | Exit NightPad (prompts to save unsaved files) |

### Sidebar Explorer & View
| Shortcut | Action |
|---|---|
| `Ctrl + B` | Toggle File Explorer Sidebar |
| `Alt + Z` | Toggle Word Wrap |
| `Ctrl + Shift + M` | Toggle Markdown Live Preview split pane |
| `Ctrl + Shift + L` | Toggle line number gutter |
| `Ctrl + +` / `Ctrl + -` | Zoom In / Zoom Out |
| `Ctrl + MouseWheel` | Zoom In / Zoom Out with mouse wheel |
| `Ctrl + 0` | Reset Zoom to 100% |

### Text Editing & Navigation
| Shortcut | Action |
|---|---|
| `Ctrl + Z` / `Ctrl + Y` | Undo / Redo |
| `Ctrl + D` | Duplicate current line |
| `Ctrl + Shift + K` | Delete current line |
| `Alt + ↑` / `Alt + ↓` | Move active line Up / Down |
| `Ctrl + /` | Toggle single-line comment (language-aware) |
| `Ctrl + Shift + J` | Format / Beautify JSON |
| `Ctrl + Shift + U` / `Ctrl + U` | Transform selection to UPPERCASE / lowercase |
| `F5` | Insert current timestamp (`YYYY-MM-DD HH:MM:SS`) |
| `Ctrl + G` | Open "Go to Line" navigation bar |

### Search & Replace
| Shortcut | Action |
|---|---|
| `Ctrl + F` | Open Find panel |
| `Ctrl + H` | Open Find & Replace panel |
| `F3` / `Enter` | Jump to next match |
| `Shift + F3` | Jump to previous match |
| `Alt + R` | Replace current match |
| `Alt + A` | Replace all matches |
| `Esc` | Close search / navigation panel |

---

## 🛠️ Architecture & Source Code

- **Source Code**: [`tools/nightpad/`](file:///%USERPROFILE%/Documents/myenv/tools/nightpad)
  - `MainWindow.xaml` & `MainWindow.xaml.cs`: Core UI, Sidebar Explorer, AvalonEdit host, and event dispatching.
  - `Models/FileNode.cs`: Recursive file & directory tree node model for sidebar navigation.
  - `Models/EditorDocument.cs`: Document state management, encoding, and statistics.
  - `Services/SyntaxService.cs`: Embedded XSHD syntax definitions with Python 3 keywords, docstrings, and language recognition.
  - `Services/TextTransformService.cs`: Text formatting, JSON parsing, and encoding tools.
- **Published Binary**: [`scripts/nightpad/NightPad.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/nightpad/NightPad.exe)
