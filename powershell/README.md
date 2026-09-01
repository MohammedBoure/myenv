# ⚡ PowerShell Configuration Directory

This directory contains the central configuration, themes, and interactive helper functions for PowerShell in the `myenv` environment.

---

## 📁 Files & Purpose

| File | Purpose |
|---|---|
| [`profile.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/profile.ps1) | Single source of truth for PowerShell `$PROFILE`. Configures UTF-8 encoding (CodePage 65001), development tools in `$env:Path`, environment variables, module loading, the `sudo` privilege escalation utility, and `np`/`nightpad`/`notepad` NightPad text editor functions. |
| [`midnight-aurora.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/midnight-aurora.ps1) | Core interactive environment script. Sets up PSReadLine syntax highlighting, custom prompt with git branch detection, keybindings (`Ctrl+R`, `Ctrl+T`), folder tab auto-completion, and helper functions: `cpf` (multi-path copy via fzf with Tab selection, preview toggle, absolute/relative/markdown formatting & shortcut guide), `cb`/`c` (copy command output to clipboard), `cd`/`chdir` (auto-listing navigation), `ll`, `la`, `gs`, `croot`, and `docs`. |
| [`cpf-ignore.txt`](file:///C:/Users/moham/Documents/myenv/powershell/cpf-ignore.txt) | Central ignore configuration file for `cpf` (defining ignored folders, files, and wildcard patterns like `.gitignore`). |
| [`console-theme.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/console-theme.ps1) | Console window and terminal theming script applying Cascadia Code font (for Arabic glyph shaping), 32% transparency (68% opacity), true black background, and Midnight Aurora palette across both classic Windows Console (`conhost.exe`) and Windows 11 Windows Terminal (`wt.exe`). |
