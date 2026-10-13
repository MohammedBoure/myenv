# Micro Configuration Directory (`micro/`)

This directory contains the central configuration, keybindings, and custom color themes for the `micro` terminal text editor in `myenv`, configured for seamless parity with Visual Studio Code.

---

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`settings.json`](file:///C:/Users/moham/Documents/myenv/micro/settings.json) | Central settings for `micro`. Configures line (I-beam) cursor, full mouse interaction, system clipboard sync, 4-space tab indentation, word & soft wrapping, bracket auto-close, and dark themes. |
| [`bindings.json`](file:///C:/Users/moham/Documents/myenv/micro/bindings.json) | Keybinding overrides matching VS Code shortcuts: standard copy/paste/cut/undo/redo/save/find, `Ctrl+D` for multi-cursor next occurrence, `Ctrl+B` for file tree toggle, `Shift+Alt+Up/Down` for line duplication, and `Alt+Up/Down` for line moving. |
| [`colorschemes/`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes) | Directory containing eye-friendly custom dark themes with solid non-transparent backgrounds. |
| [`colorschemes/catppuccin-mocha.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/catppuccin-mocha.micro) | Soothing, eye-friendly pastel dark theme (`#1E1E2E`) designed for zero glare and fatigue-free long coding sessions. |
| [`colorschemes/vscode-dark.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/vscode-dark.micro) | Exact VS Code Dark Modern theme with `#1E1E1E` background, `#264F78` selection, and familiar VS Code syntax colors. |
| [`colorschemes/midnight-aurora.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/midnight-aurora.micro) | Solid dark blue-gray (`#0D1117`) syntax highlighting colorscheme tailored to MyEnv's terminal palette. |
| [`plug/filemanager/`](file:///C:/Users/moham/Documents/myenv/micro/plug/filemanager) | Enhanced filemanager sidebar plugin with native `Enter` key file opening and recursive directory expanding/collapsing. |

---

## 🔗 Target Symlink / Config Location

These configuration files are deployed and linked to the user's active configuration directory via [`scripts/setup-micro.ps1`](file:///C:/Users/moham/Documents/myenv/scripts/setup-micro.ps1):

- **Target Path**: `%USERPROFILE%\.config\micro\` (`$env:USERPROFILE\.config\micro\`)
