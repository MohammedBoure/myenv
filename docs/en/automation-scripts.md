# ⚙️ Automation Scripts Documentation

PowerShell and batch scripts automating environment setup, registry options, developer tool restoration, and hotkey actions.

---

## 📜 Available Scripts (`scripts/`)

| Script | Path | Description |
|---|---|---|
| **`setup-all.ps1`** | [scripts/setup-all.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/setup-all.ps1) | **Master Setup Script**: Applies directory junctions, taskbar autohide, PSReadLine, Clink, Winget packages, CMD AutoRun, and Alt+Shift disable |
| **`docs.ps1`** | [scripts/docs.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/docs.ps1) | **CLI Documentation Navigator**: Interactive shortcut and documentation helper command `docs` |
| **`install-packages.ps1`** | [scripts/install-packages.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-packages.ps1) | Restores/installs developer packages from `winget-packages.json` |
| **`app-launcher.ps1`** | [scripts/app-launcher.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/app-launcher.ps1) | Centered WPF app launcher dialog (`Alt + Q`) |
| **`install-clink.ps1`** | [scripts/install-clink.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-clink.ps1) | Installs Clink via WinGet & configures CMD AutoRun |
| **`set-cmd-autocompletion.ps1`** | [scripts/set-cmd-autocompletion.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-cmd-autocompletion.ps1) | Configures CMD Tab completion & AutoRun script |
| **`set-taskbar-autohide.ps1`** | [scripts/set-taskbar-autohide.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-taskbar-autohide.ps1) | Toggles Windows Taskbar autohide in Registry & restarts Explorer |
| **`set-ctrl-backspace.ps1`** | [scripts/set-ctrl-backspace.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-ctrl-backspace.ps1) | Binds `Ctrl+Backspace` for PSReadLine word deletion |
| **`disable-alt-shift-lang.ps1`** | [scripts/disable-alt-shift-lang.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/disable-alt-shift-lang.ps1) | Disables `Alt+Shift` key toggle, keeping `Win+Space` as primary |
| **`focused-window-border.ps1`** | [scripts/focused-window-border.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/focused-window-border.ps1) | Legacy Win32/WPF active window focus border (superseded by native `tools/focused-border/FocusedBorder.exe`) |
| **`smart-tiling-direction.ps1`** | [scripts/smart-tiling-direction.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/smart-tiling-direction.ps1) | Listens to GlazeWM events & auto-sets tiling split direction |
| **`setup-arabic-terminal.ps1`** | [scripts/setup-arabic-terminal.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/setup-arabic-terminal.ps1) | **Arabic Terminal & UTF-8 Setup**: Centralizes Windows Terminal settings, enforces UTF-8 CodePage 65001, Cascadia Mono font, and compiles `open-terminal-here.exe` |
| **`setup-gemini-config.ps1`** | [scripts/setup-gemini-config.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/setup-gemini-config.ps1) | **Gemini Antigravity Setup**: Sets up central directory junction linking `%USERPROFILE%\.gemini\config` to `%USERPROFILE%\Documents\myenv\gemini` |
| **`install-len.ps1`** | [scripts/install-len.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-len.ps1) | **len (ProjectLens) Auto-Installer**: Smart automated installer and updater from remote GitHub repository with commit checking |
| **`OpenTerminalHere.cs` / `.exe`** | [scripts/OpenTerminalHere.cs](file:///%USERPROFILE%/Documents/myenv/scripts/OpenTerminalHere.cs) | Instant C# launcher to open Windows Terminal / CMD / PowerShell at active Explorer path (<10ms) with full Arabic support |
| **`open-terminal-here.ps1`** | [scripts/open-terminal-here.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/open-terminal-here.ps1) | PowerShell helper script for opening terminal at active Explorer folder with Windows Terminal preference |
| **`quick-translate.ps1`** | [scripts/quick-translate.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/quick-translate.ps1) | Background process launcher script for QuickTranslate tool (`--type`, `--clipboard`, `--ocr`) |
| **`download-clink.ps1`** | [scripts/download-clink.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/download-clink.ps1) | Downloads & installs portable Clink zip release if WinGet fails |
| **`toggle-window-transparency.ps1`** | [scripts/toggle-window-transparency.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/toggle-window-transparency.ps1) | Toggles active window transparency 80% / 100% (`Alt+Shift+Z`) |
| **`capture-screenshot.ps1`** | [scripts/capture-screenshot.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/capture-screenshot.ps1) | Direct full screenshot capture to file & Clipboard (`Alt+Shift+S`) |
| **`toggle-mute.ps1`** | [scripts/toggle-mute.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/toggle-mute.ps1) | Master audio mute toggle via Win32 API (`Alt+Shift+M`) |
| **`set-windows10-border.ps1`** | [scripts/set-windows10-border.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-windows10-border.ps1) | DWM registry border color configurator |
| **`sudo.cmd`** | [scripts/sudo.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/sudo.cmd) | Command elevation helper for CMD |
| **`cb.cmd`** | [scripts/cb.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cb.cmd) | Output display & clipboard copy wrapper script |

---

## 🛠️ Master Environment Setup

To run master setup at any time:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-all.ps1"
```
