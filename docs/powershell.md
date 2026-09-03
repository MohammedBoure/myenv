# PowerShell Environment Documentation

High-performance, customized PowerShell profile featuring PSReadLine, Midnight Aurora theme, and 32% Console Transparency.

---

## Key Features

1. **Unified Profile (`$PROFILE`)**:
   - Single source of truth loaded automatically from [powershell/profile.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/profile.ps1).
2. **PSReadLine Enhancements**:
   - `ListView` history predictions and auto-suggestions.
   - `Ctrl+Backspace` for backward word deletion (`BackwardKillWord`).
3. **Midnight Aurora Theme & Transparency (Windows 10 & 11)**:
   - True black console background with **32% Transparency** (68% Opacity) synchronized automatically across classic Windows Console (`conhost.exe`) and Windows 11 Windows Terminal (`wt.exe`).
4. **Elevated `sudo` Command**:
   - `sudo`: Opens a new elevated PowerShell window at current working directory.
   - `sudo <command>`: Executes target command with Administrator privileges in current path.
5. **Dev Tools PATH Auto-Loader**:
   - Automatically loads `dotnet`, `flutter`, `jdk-17`, `Android SDK`, `kotlin`, `msys64`, `php`, `composer`, `nodejs`, `npm`.
6. **Auto-LS Navigation (`cd` / `chdir`)**:
   - Executing `cd <path>` automatically runs `Get-ChildItem` to list directory contents.

---

## Configuration Files

- **Main Profile**: [powershell/profile.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/profile.ps1)
- **Theme & Prompt Script**: [powershell/midnight-aurora.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/midnight-aurora.ps1)
- **Console Transparency Script**: [powershell/console-theme.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/console-theme.ps1)

---

## Enabled Shortcuts & Functions

| Shortcut | Description |
|---|---|
| `cd` / `chdir` | Navigate to directory and auto-list files (`Get-ChildItem`) |
| `cpf` | Interactively select single/multiple files via fzf (`Tab` multi-select, `Ctrl+P` preview, `-abs`, `-md`, `-s`, `-c`) and copy paths to Clipboard |
| `docs` | Terminal Documentation & Shortcut Navigator CLI |
| `cb` / `c` | Execute command, display output, and copy directly to Clipboard |
| `\| cb` | Pipe output to screen and Clipboard simultaneously |
| `sudo <command>` | Execute command with Administrator privileges at current directory |
| `Ctrl + Backspace` | Delete word backward |
| `Tab` | Open interactive menu completion |
| `Ctrl + R` | Interactive history search (fzf / PSReadLine) |
| `Alt + Ctrl + Enter` | Open new PowerShell window via GlazeWM |
