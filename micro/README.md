# Micro Configuration Directory (`micro/`)

This directory contains the central configuration, keybindings, and custom color themes for the `micro` terminal text editor in `myenv`, configured for seamless parity with Visual Studio Code.

---

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`settings.json`](file:///C:/Users/moham/Documents/myenv/micro/settings.json) | Central settings for `micro`. Configures line (I-beam) cursor, full mouse interaction, system clipboard sync, 4-space tab indentation, word & soft wrapping, bracket auto-close, and dark themes. |
| [`bindings.json`](file:///C:/Users/moham/Documents/myenv/micro/bindings.json) | Keybinding overrides matching VS Code shortcuts: standard copy/paste/cut/undo/redo/save/find, `Ctrl+D` for multi-cursor next occurrence, `Ctrl+B` for file tree toggle, `Shift+Alt+Up/Down` for line duplication, and `Alt+Up/Down` for line moving. |
| [`colorschemes/`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes) | Directory containing custom themes. Includes `midnight-aurora.micro` matching the terminal and PowerShell palette. |
| [`colorschemes/midnight-aurora.micro`](file:///C:/Users/moham/Documents/myenv/micro/colorschemes/midnight-aurora.micro) | True black / dark blue syntax highlighting colorscheme tailored to MyEnv's visual style. |

---

## 🔗 Target Symlink / Config Location

These configuration files are deployed and linked to the user's active configuration directory via [`scripts/setup-micro.ps1`](file:///C:/Users/moham/Documents/myenv/scripts/setup-micro.ps1):

- **Target Path**: `%USERPROFILE%\.config\micro\` (`$env:USERPROFILE\.config\micro\`)
