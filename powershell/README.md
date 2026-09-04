# PowerShell Configuration Directory

This directory contains the central configuration, themes, formatting definitions, and interactive helper functions for PowerShell in the `myenv` environment.

---

## Files & Purpose

| File | Purpose |
|---|---|
| [`profile.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/profile.ps1) | Single source of truth for PowerShell `$PROFILE`. Configures strict UTF-8 stream encoding (CodePage 65001), human-readable `size` table formatting via `FileSystem.format.ps1xml`, PSReadLine Predictive IntelliSense (`HistoryAndPlugin`), `CompletionPredictor` module integration, Arabic text reshaper (`Format-ArabicText` / `ar`), native GUI input dialog (`Get-ArabicInput`), AI CLI wrapper (`Invoke-ArabicCli` / `ask-ai`), development tools in `$env:Path`, environment variables, module loading, the `sudo` privilege escalation utility, and `np`/`nightpad`/`notepad` NightPad text editor functions. |
| [`FileSystem.format.ps1xml`](file:///C:/Users/moham/Documents/myenv/powershell/FileSystem.format.ps1xml) | Custom PowerShell table view format definition replacing the raw byte `Length` column with a human-readable `size` column (formatted as B, KB, MB, GB, TB) for `Get-ChildItem`, `ls`, and directory navigation. |
| [`midnight-aurora.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/midnight-aurora.ps1) | Core interactive environment script. Sets up PSReadLine Predictive IntelliSense (ghost suggestions & list view via `F2`), interactive menu Tab completion (`MenuComplete`), history prefix search (`UpArrow`/`DownArrow`), word completion shortcuts (`Ctrl+F`, `Ctrl+RightArrow`), custom prompt with git branch detection, keybindings (`Ctrl+R`, `Ctrl+T`), folder tab auto-completion, and helper functions: `cpf`, `cb`/`c`, `cd`/`chdir` (with human-readable auto-ls), `ll`, `la`, `gs`, `croot`, and `docs`. |
| [`cpf-ignore.txt`](file:///C:/Users/moham/Documents/myenv/powershell/cpf-ignore.txt) | Central ignore configuration file for `cpf` (defining ignored folders, files, and wildcard patterns like `.gitignore`). |
| [`console-theme.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/console-theme.ps1) | Console window and terminal theming script applying Cascadia Code font (for Arabic glyph shaping), 32% transparency (68% opacity), true black background, and Midnight Aurora palette across both classic Windows Console (`conhost.exe`) and Windows 11 Windows Terminal (`wt.exe`). |
