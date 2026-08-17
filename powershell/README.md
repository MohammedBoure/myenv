# ⚡ PowerShell Configuration Directory

This directory contains the central configuration, themes, and interactive helper functions for PowerShell in the `myenv` environment.

---

## 📁 Files & Purpose

| File | Purpose |
|---|---|
| [`profile.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/profile.ps1) | Single source of truth for PowerShell `$PROFILE`. Configures UTF-8 encoding, development tools in `$env:Path`, environment variables, module loading, the `sudo` privilege escalation utility, and `np`/`nightpad`/`notepad` NightPad text editor functions. |
| [`midnight-aurora.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/midnight-aurora.ps1) | Core interactive environment script. Sets up PSReadLine syntax highlighting, custom prompt with git branch detection, keybindings (`Ctrl+R`, `Ctrl+T`), folder tab auto-completion, and helper functions: `cpf` (copy relative path via fzf with folder completion & shortcut guide), `cb`/`c` (copy command output to clipboard), `cd`/`chdir` (auto-listing navigation), `ll`, `la`, `gs`, `croot`, and `docs`. |
| [`console-theme.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/console-theme.ps1) | Console window and terminal theming script applying dark background and transparency settings. |
