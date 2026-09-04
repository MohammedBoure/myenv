# Notepad: Lightweight Professional Text Editor

A fast, lightweight, and keyboard-driven text and code editor built natively for the MyEnv Windows desktop environment. Designed with an obsidian dark interface, 0px sharp rectangular aesthetics, classic uncluttered menu bar (`File`, `Edit`, `View`, `Tools`), bottom status bar, instant launch readiness, terminal-style fast saving, and robust Arabic RTL support.

---

## Overview and Key Highlights

- **Instant Launch & Direct Typing**: Ready to type immediately upon opening (<20ms) - keyboard focus is acquired instantly with zero mouse interaction required.
- **Unified Single-Row Top Bar**: Combines window title, document branding, integrated menus (`File`, `Edit`, `View`, `Tools`), Markdown Live Preview button, and window controls onto a single row, saving vertical space for the text editor and live preview.
- **Smooth Line Movement & Long Line Control**:
  - **Default Word Wrap (`Alt + Z`)**: Wraps long lines neatly within the editor window with automatic block indent alignment.
  - **Scroll Below Document**: Allows scrolling past the last line for comfortable breathing room.
  - **New Line Below (`Ctrl + Enter`)**: Inserts a new line below from anywhere within a long line without breaking existing text.
  - **New Line Above (`Ctrl + Shift + Enter`)**: Inserts a new indented line above the current line.
- **Zero-Strain External File Watcher**:
  - Monitors open files using kernel-level `FileSystemWatcher` event hooks (zero polling, 0% CPU strain).
  - Automatically prompts via a non-intrusive warning banner when an external program updates, renames, or deletes the active file.
  - One-click `[Reload]` (or `F5`) updates buffer while preserving caret position; internal saves are automatically de-duplicated.
- **Clipboard Image Pasting & External Viewer Integration**:
  - Paste images directly from the clipboard (`Ctrl + V`) or copied from File Explorer.
  - Automatically saves the image into `./assets/image_YYYYMMDD_HHMMSS.png` relative to the open document.
  - Generates standard Markdown syntax `![image](assets/filename.png)` so images copy cleanly with the text.
  - Embedded image rendering in the split Markdown Live Preview with click-to-open capability.
  - Quick external viewing directly from the editor buffer via `Alt + O`, `Ctrl + Click`, or right-click context menu (`Open Externally` / `Reveal in File Explorer`).
- **Quick Symbols & Frequent Words Palette (`Ctrl + .` / `F4`)**:
  - Instant modal palette for fast access to symbols, arrows, bullets, math characters, markdown snippets, and frequent Arabic phrases.
  - 100% keyboard-driven: type to filter in real-time, `Up`/`Down` arrows to navigate, and `Enter` to insert directly at current cursor position.
  - Fully customizable: press `Ctrl + Enter` (or click `[+] Add`) to add new custom words and symbols directly to persistent JSON storage (`%APPDATA%/NightPad/quick_symbols.json`).
  - Delete any item directly from the palette using `Del` key or `[x]` button.
- **Terminal-Style Fast Keyboard Saving (`Ctrl + S` / `Ctrl + Shift + S` / `F2`)**:
  - Interactive keyboard path input bar embedded directly in the window (no slow OS dialog lag).
  - **Tab Auto-Completion**: Autocompletes folder and file paths interactively.
  - **Quick Preset Jump**: Instant shortcuts for key directories: `F1` (Current Dir), `F2` (Documents), `F3` (Desktop), `F4` (MyEnv).
  - **Auto Directory Creation**: Automatically creates missing parent directories on save.
  - **Classic Dialog Fallback**: Press `Alt + B` or `Ctrl + Alt + S` for the traditional Windows File Dialog.
- **Arabic Language & Bidirectional (RTL) Support**:
  - **RTL / LTR Toggle (`Ctrl + Shift + R`)**: Switch writing flow between Left-to-Right and Right-to-Left instantly.
  - **Smart Arabic Auto-Detection**: Automatically aligns text to Right-to-Left when Arabic characters are detected.
  - **Enhanced Typography**: Cascadia Code with Cairo, Segoe UI, and Tahoma font fallbacks for Arabic script and ligatures.
  - **Multilingual Word Counter**: Accurately counts Arabic and Latin words using Unicode word boundaries.
- **Classic Menu Bar**: Clean and responsive menus for `File`, `Edit`, `View`, and `Tools`.
- **Accurate Python 3 & Language Syntax Highlighting**:
  - **Python**: Full coloring for keywords, built-ins (`print`, `len`, `range`, `isinstance`, `enumerate`, `dict`, `list`...), magic methods (`__init__`, `__str__`...), decorators (`@decorator`), type annotations, `self`, `cls`, f-strings, docstrings, and smart auto-indentation after `:` colons.
  - **20+ Supported Languages**: Python, PowerShell, JavaScript, TypeScript, JSON, YAML, Markdown, C#, C/C++, HTML, XML, CSS, PHP, SQL, Batch/CMD, INI/Config, Java, Rust, and Go.
- **Search & Replace (`Ctrl + F` / `Ctrl + H`)**: Sleek search panel supporting Match Case, Whole Word, Regular Expressions, and instant match counter.
- **Developer Tools**: JSON formatter & minifier (`Ctrl + Shift + J`), Base64 and URL encoding/decoding, case transformation (`Ctrl + Shift + U` / `Ctrl + U`), line sorting, duplicate line removal, and timestamp insertion (`F5`).
- **Status Bar**: Real-time cursor position (`Ln`, `Col`), text direction (`LTR` / `RTL`), character/word/line counters, encoding (`UTF-8`), line ending (`Windows CRLF` / `Unix LF`), and language selector.
- **Desktop & CLI Integration**: Bound to `Alt + N` in GlazeWM and runnable via `np [file]` or `nightpad` from CMD and PowerShell.

---

## Launching Notepad

| Launch Method | Command / Shortcut | Description |
|---|---|---|
| **GlazeWM Hotkey** | `Alt + N` | Instant launch from anywhere on any workspace |
| **PowerShell CLI** | `np [file]` / `nightpad [file]` | Opens specified file or blank editor from PowerShell |
| **Command Prompt (CMD)** | `np [file]` / `nightpad [file]` | Opens specified file or blank editor from CMD |
| **Windows App Launcher** | `Alt + Shift + Q` -> Type `NightPad` | Searchable via MyEnv WPF application launcher |
| **File Explorer Drag & Drop** | Drag any file into Notepad window | Instantly opens the file |

---

## Keyboard Shortcuts Reference

### File Management & Fast Saving
| Shortcut | Action |
|---|---|
| `Ctrl + N` | Create new document |
| `Ctrl + O` | Open file dialog |
| `Ctrl + S` | Save current file (opens Quick Save bar if untitled) |
| `Ctrl + Shift + S` / `F2` / `Alt + S` | Open Quick Save bar with Tab completion & presets |
| `Ctrl + Alt + S` | Open classic Windows Save Dialog |
| `F1` / `F2` / `F3` / `F4` | Jump to Current, Documents, Desktop, MyEnv directory in Quick Save |
| `Tab` | Autocomplete directory and file paths in Quick Save |
| `Alt + F4` | Exit Notepad (prompts to save if modified) |

### Arabic & Direction Controls
| Shortcut | Action |
|---|---|
| `Ctrl + Shift + R` | Toggle Right-to-Left (RTL) and Left-to-Right (LTR) text direction |
| Click `[LTR]` / `[RTL]` in Status Bar | Switch text flow direction |

### Text Editing & Formatting
| Shortcut | Action |
|---|---|
| `Ctrl + Z` / `Ctrl + Y` | Undo / Redo |
| `Ctrl + X` / `Ctrl + C` / `Ctrl + V` | Cut / Copy / Paste (Supports Image Pasting into `./assets/`) |
| `Ctrl + Enter` | Insert new line below current line without breaking text |
| `Ctrl + Shift + Enter` | Insert new line above current line |
| `Alt + O` | Open image or link under cursor in external viewer |
| `Ctrl + Click` | Follow link or open image under mouse in external viewer |
| `Ctrl + A` | Select All |
| `Ctrl + D` | Duplicate current line |
| `Ctrl + Shift + K` | Delete current line |
| `Alt + Up` / `Alt + Down` | Move active line Up / Down |
| `Ctrl + /` | Toggle single-line comment |
| `Ctrl + Shift + J` | Format / Beautify JSON |
| `Ctrl + Shift + U` / `Ctrl + U` | Transform selection to UPPERCASE / lowercase |
| `F5` | Insert timestamp (or Reload file when external modification banner is active) |

### Quick Symbols & Frequent Words
| Shortcut | Action |
|---|---|
| `Ctrl + .` / `F4` | Open Quick Symbols & Frequent Words palette |
| `Up` / `Down` | Navigate through symbols and words |
| `PageUp` / `PageDown` | Jump 5 items up or down |
| `Enter` | Insert selected symbol/word into editor and close palette |
| `Ctrl + Enter` | Add typed search query as a new symbol/word |
| `Del` | Delete selected symbol/word from list and persistent storage |
| `Esc` | Close symbols palette and refocus editor |

### Search & Navigation
| Shortcut | Action |
|---|---|
| `Ctrl + F` | Open Find panel |
| `Ctrl + H` | Open Find & Replace panel |
| `F3` / `Enter` | Find Next match |
| `Shift + F3` | Find Previous match |
| `Ctrl + G` | Go to line |
| `Esc` | Close Quick Save / Search / Go-To bar and return to editor |

### View & Zoom
| Shortcut | Action |
|---|---|
| `Alt + Z` | Toggle Word Wrap |
| `Ctrl + Shift + L` | Toggle line numbers |
| `Ctrl + +` / `Ctrl + -` | Zoom In / Zoom Out |
| `Ctrl + MouseWheel` | Zoom In / Zoom Out with mouse wheel |
| `Ctrl + 0` | Restore Default Zoom (100%) |

---

## Source & Executable

- **Source Directory**: [`tools/nightpad/`](file:///%USERPROFILE%/Documents/myenv/tools/nightpad)
- **Executable**: [`scripts/nightpad/NightPad.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/nightpad/NightPad.exe)
