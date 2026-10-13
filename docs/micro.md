# Micro Terminal Text Editor Guide (VS Code Parity)

The **Micro** terminal text editor is integrated into **MyEnv** as the absolute default, fast, keyboard-driven terminal editor. It provides an intuitive, Visual Studio Code-like pair programming and editing experience directly within PowerShell and Windows Terminal.

---

## 🚀 Key Features & Parity with VS Code

- **Line (I-Beam) Cursor**: Clean vertical line cursor (`"cursorstyle": "line"`).
- **Full Mouse Support**: Left click to place cursor, drag to highlight, double-click for words, scroll wheel support.
- **System Clipboard Integration**: True Windows clipboard sync (`Ctrl+C`, `Ctrl+V`, `Ctrl+X`).
- **Multi-Cursor Support**: `Ctrl+D` spawns multiple cursors at subsequent occurrences of the selected word.
- **VS Code Sidebar File Tree**: Toggleable file explorer tree view via `Ctrl+B` using the `filemanager` plugin.
- **Eye-Comfort Dark Theming**: Soft, solid-background dark themes that prevent wallpaper transparency bleed-through and reduce eye strain:
  - `catppuccin-mocha`: Celebrated soothing pastel dark palette (`#1E1E2E`).
  - `vscode-dark`: Official VS Code Dark Modern aesthetic (`#1E1E1E`).
  - `midnight-aurora`: Solid dark blue-gray (`#0D1117`) matching MyEnv's terminal palette.

---

## ⌨️ VS Code Keybindings Reference

| Shortcut | Action | VS Code Equivalent |
|---|---|---|
| `Ctrl + S` | Save current file | Save (`workbench.action.files.save`) |
| `Ctrl + C` | Copy selected text to Windows clipboard | Copy |
| `Ctrl + V` | Paste from Windows clipboard | Paste |
| `Ctrl + X` | Cut selected text to Windows clipboard | Cut |
| `Ctrl + Z` | Undo last edit | Undo |
| `Ctrl + Y` | Redo last undone edit | Redo |
| `Ctrl + F` | Interactive text search | Find |
| `Ctrl + B` | Toggle file tree sidebar (`filemanager`) | Toggle Primary Side Bar |
| `Enter` / `Tab` | Open file or expand/collapse folder in file tree | Open file / expand folder |
| `Ctrl + D` | Select next occurrence (multi-cursor) | Add Selection To Next Find Match |
| `Alt + Up` | Move current line or selection up | Move Line Up |
| `Alt + Down` | Move current line or selection down | Move Line Down |
| `Shift + Alt + Up` | Duplicate current line up | Copy Line Up |
| `Shift + Alt + Down` | Duplicate current line down | Copy Line Down |
| `Ctrl + A` | Select all text in buffer | Select All |
| `Ctrl + E` | Open command prompt (`> prompt`) | Command Palette |
| `Ctrl + Q` | Close buffer / quit editor | Close Editor |

---

## 🛠️ Shell & Git Integration

### 1. PowerShell Environment Variables
In [`powershell/profile.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/profile.ps1):
```powershell
$env:VISUAL = 'micro'
$env:EDITOR = 'micro'
```

### 2. Git Global Configuration
Git is automatically configured to use `micro` for interactive commit messages, rebases, and merge conflict resolution:
```bash
git config --global core.editor "micro"
```

### 3. Quick Aliases
PowerShell provides fast aliases:
- `m <file>`: Instantly open file in `micro`.
- `mc <file>`: Alternate alias for `micro`.

---

## 📂 Configuration Files

All configuration files are managed centrally in `myenv`:

- **Settings**: [`micro/settings.json`](file:///C:/Users/moham/Documents/myenv/micro/settings.json)
- **Keybindings**: [`micro/bindings.json`](file:///C:/Users/moham/Documents/myenv/micro/bindings.json)
- **Colorscheme**: [`micro/colorschemes/midnight-aurora.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/midnight-aurora.micro)

These files are linked to `%USERPROFILE%\.config\micro\` via [`scripts/setup-micro.ps1`](file:///C:/Users/moham/Documents/myenv/scripts/setup-micro.ps1).
