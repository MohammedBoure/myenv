# Notepad: Lightweight Professional Text Editor

A fast, lightweight, and clean text and code editor built natively for the **MyEnv** Windows desktop environment. Designed with a dark interface, 0px sharp rectangular aesthetics, classic uncluttered menu bar (`File`, `Edit`, `View`, `Tools`), bottom taskbar/status bar, and accurate multi-language syntax support.

---

## 🚀 Overview & Key Highlights

- **Clean & Uncluttered**: Zero distraction, instant launch in <20ms, clean layout without sidebars, bloated tabs, or decorative badges.
- **Classic Menu Bar**: Clean and responsive menus for `File`, `Edit`, `View`, and `Tools`.
- **Accurate Python 3 & Language Syntax Highlighting**:
  - **Python**: Full coloring for keywords, built-ins (`print`, `len`, `range`, `isinstance`, `enumerate`, `dict`, `list`...), magic methods (`__init__`, `__str__`...), decorators (`@decorator`), type annotations, `self`, `cls`, f-strings, docstrings, and smart auto-indentation after `:` colons.
  - **20+ Supported Languages**: Python, PowerShell, JavaScript, TypeScript, JSON, YAML, Markdown, C#, C/C++, HTML, XML, CSS, PHP, SQL, Batch/CMD, INI/Config, Java, Rust, and Go.
- **Search & Replace (`Ctrl + F` / `Ctrl + H`)**: Sleek search panel supporting Match Case, Whole Word, Regular Expressions, and instant match counter.
- **Developer Tools**: JSON formatter & minifier (`Ctrl + Shift + J`), Base64 and URL encoding/decoding, case transformation (`Ctrl + Shift + U` / `Ctrl + U`), line sorting, duplicate line removal, and timestamp insertion (`F5`).
- **Clean Taskbar / Status Bar**: Real-time cursor location (`Ln`, `Col`), character/word/line counters, encoding (`UTF-8`), line ending (`Windows CRLF` / `Unix LF`), and language selector.
- **Desktop & CLI Integration**: Bound to `Alt + N` in GlazeWM and runnable via `np [file]` or `nightpad` from CMD and PowerShell.

---

## ⚡ Launching Notepad

| Launch Method | Command / Shortcut | Description |
|---|---|---|
| **GlazeWM Hotkey** | `Alt + N` | Instant launch from anywhere on any workspace |
| **PowerShell CLI** | `np [file]` / `nightpad [file]` | Opens specified file or blank editor from PowerShell |
| **Command Prompt (CMD)** | `np [file]` / `nightpad [file]` | Opens specified file or blank editor from CMD |
| **Windows App Launcher** | `Alt + Shift + Q` -> Type `NightPad` | Searchable via MyEnv WPF application launcher |
| **File Explorer Drag & Drop** | Drag any file into Notepad window | Instantly opens the file |

---

## ⌨️ Keyboard Shortcuts Reference

### File Management
| Shortcut | Action |
|---|---|
| `Ctrl + N` | Create new document |
| `Ctrl + O` | Open file dialog |
| `Ctrl + S` | Save current document |
| `Ctrl + Shift + S` | Save As dialog |
| `Alt + F4` | Exit Notepad (prompts to save if modified) |

### Text Editing & Formatting
| Shortcut | Action |
|---|---|
| `Ctrl + Z` / `Ctrl + Y` | Undo / Redo |
| `Ctrl + X` / `Ctrl + C` / `Ctrl + V` | Cut / Copy / Paste |
| `Ctrl + A` | Select All |
| `Ctrl + D` | Duplicate current line |
| `Ctrl + Shift + K` | Delete current line |
| `Alt + ↑` / `Alt + ↓` | Move active line Up / Down |
| `Ctrl + /` | Toggle single-line comment |
| `Ctrl + Shift + J` | Format / Beautify JSON |
| `Ctrl + Shift + U` / `Ctrl + U` | Transform selection to UPPERCASE / lowercase |
| `F5` | Insert current timestamp |

### Search & Navigation
| Shortcut | Action |
|---|---|
| `Ctrl + F` | Open Find panel |
| `Ctrl + H` | Open Find & Replace panel |
| `F3` / `Enter` | Find Next match |
| `Shift + F3` | Find Previous match |
| `Ctrl + G` | Go to line |
| `Esc` | Close search / Go-To bar |

### View & Zoom
| Shortcut | Action |
|---|---|
| `Alt + Z` | Toggle Word Wrap |
| `Ctrl + Shift + L` | Toggle line numbers |
| `Ctrl + +` / `Ctrl + -` | Zoom In / Zoom Out |
| `Ctrl + MouseWheel` | Zoom In / Zoom Out with mouse wheel |
| `Ctrl + 0` | Restore Default Zoom (100%) |

---

## 🛠️ Source & Executable

- **Source Directory**: [`tools/nightpad/`](file:///%USERPROFILE%/Documents/myenv/tools/nightpad)
- **Executable**: [`scripts/nightpad/NightPad.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/nightpad/NightPad.exe)
