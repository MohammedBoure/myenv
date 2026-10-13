# Micro Configuration Directory (`micro/`)

This directory contains the central configuration, keybindings, and custom color themes for the `micro` terminal text editor in `myenv`, configured for seamless parity with Visual Studio Code.

---

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`settings.json`](file:///C:/Users/moham/Documents/myenv/micro/settings.json) | Central settings for `micro`. Configures line (I-beam) cursor, full mouse interaction, system clipboard sync, 4-space tab indentation, word & soft wrapping, bracket auto-close, LSP autocompletion, diagnostic error recognition, and eye-friendly dark themes. |
| [`bindings.json`](file:///C:/Users/moham/Documents/myenv/micro/bindings.json) | Keybinding overrides matching VS Code shortcuts: standard copy/paste/cut/undo/redo/save/find, `Ctrl+D` for multi-cursor next occurrence, `Ctrl+B` for file tree toggle, `Ctrl+Space` for LSP autocomplete, `Alt+K` for hover, `Alt+D` for definition, `Alt+F` for formatting, `Shift+Alt+Up/Down` for line duplication, and `Alt+Up/Down` for line moving. |
| [`init.lua`](file:///C:/Users/moham/Documents/myenv/micro/init.lua) | Micro Lua startup script that emits DECSCUSR `\027[6 q` on initialization and buffer changes to enforce a vertical line / I-beam cursor across all terminal sessions. |
| [`colorschemes/`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes) | Directory containing eye-friendly custom dark themes with solid non-transparent backgrounds. |
| [`colorschemes/soft-dark.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/soft-dark.micro) | Specially designed eye-comfort matte dark theme (`#191A24`) with soft pastel syntax colors, zero glare, and high clarity. |
| [`colorschemes/catppuccin-mocha.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/catppuccin-mocha.micro) | Soothing, eye-friendly pastel dark theme (`#1E1E2E`) designed for low contrast glare and fatigue-free long coding sessions. |
| [`colorschemes/vscode-dark.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/vscode-dark.micro) | VS Code Dark Modern theme with `#1E1E1E` background, calm dark slate statusline, `#264F78` selection, and familiar VS Code syntax colors. |
| [`colorschemes/midnight-aurora.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/midnight-aurora.micro) | Solid dark blue-gray (`#0D1117`) syntax highlighting colorscheme tailored to MyEnv's terminal palette. |
| [`plug/filemanager/`](file:///C:/Users/moham/Documents/myenv/micro/plug/filemanager) | Enhanced filemanager sidebar plugin with native `Enter` key file opening and recursive directory expanding/collapsing. |
| [`plug/lsp/`](file:///C:/Users/moham/Documents/myenv/micro/plug/lsp) | Micro LSP plugin powering intelligent code autocompletion, real-time diagnostic syntax error highlighting, hover documentation (`Alt+K`), and definition jumps (`Alt+D`). |

---

## 🔗 Target Symlink / Config Location

These configuration files are deployed and linked to the user's active configuration directory via [`scripts/setup-micro.ps1`](file:///C:/Users/moham/Documents/myenv/scripts/setup-micro.ps1):

- **Target Path**: `%USERPROFILE%\.config\micro\` (`$env:USERPROFILE\.config\micro\`)
