# MyEnv: Professional Windows Desktop Environment (GlazeWM + YASB)

A high-performance, keyboard-driven tiling desktop environment for Windows developers. Centralized, fully version-controlled, and automated.

- **Workspace Directory**: `%USERPROFILE%\Documents\myenv` (or `$env:USERPROFILE\Documents\myenv`)
- **Updated Date**: `2026-07-27`

---

## 📚 Documentation Hub / مركز التوثيق الشامل

يمكنك الوصول لكافة أدلة التوثيق الخاصة بمكونات البيئة من الجدول التالي:

| التوثيق | المكون | الوصف | رابط الملف |
|---|---|---|---|
| 💻 **CMD Documentation** | Command Prompt | دليل اختصارات Doskey وأداة Clink والإكمال التلقائي | [docs/cmd.md](file:///%USERPROFILE%/Documents/myenv/docs/cmd.md) |
| ⚡ **PowerShell Documentation** | PowerShell Profile | إعدادات PSReadLine وثيم Midnight Aurora الشفاف | [docs/powershell.md](file:///%USERPROFILE%/Documents/myenv/docs/powershell.md) |
| 🪟 **GlazeWM Documentation** | Tiling WM | دليل كافة اختصارات لوحة المفاتيح والتقسيم ومساحات العمل | [docs/glazewm.md](file:///%USERPROFILE%/Documents/myenv/docs/glazewm.md) |
| 📊 **YASB Documentation** | Status Bar | تفاصيل الثيم الداكن الشفاف والأزرار والـ Widgets | [docs/yasb.md](file:///%USERPROFILE%/Documents/myenv/docs/yasb.md) |
| ⚙️ **Automation Scripts** | PowerShell Scripts | دليل جميع سكريبتات الأتمتة وإعادة التهيئة المباشرة | [docs/automation-scripts.md](file:///%USERPROFILE%/Documents/myenv/docs/automation-scripts.md) |

---

## 🚀 Key Environment Features

### 1. YASB (Yet Another Status Bar) - Classic Sharp Dark Theme
- **Fully Transparent Bar**: Background set to `transparent` (`0px` top gap).
- **Sharp Edge Design System**: Global `border-radius: 0px !important` across all bar widgets, workspace buttons, popups, and tooltips.
- **Obsidian Dark Aesthetic**: High-contrast silver-white text (`#f5f5f5`) on deep obsidian popups (`rgba(14, 14, 14, 0.96)`) with subtle rectangular borders (`1px solid #2a2a2a`).
- **Date & Time Widget**: Displays complete date and time format (`Fri, 24 Jul • 21:33`).
- **Focus-Aware Workspace Highlighting**:
  - **Focused Screen Active Workspace**: Stark White (`#ffffff`, bold black text `#000000`).
  - **Secondary Screen Active Workspace**: Distinct Gray (`#555555`, white text `#ffffff`).
  - **Inactive Workspaces**: Dark Slate (`rgba(18, 18, 18, 0.65)`).
- **Vertical Centering**: Explicit Qt `qproperty-alignment: 'AlignVCenter'` for icon and text alignment.
- **Zero Background Conflicts**: Zebar background process disabled and removed from Startup.

### 2. Windows Taskbar Auto-Hide
- **Automated Auto-Hide**: Primary (`StuckRects3`) and Multi-Monitor (`MMStuckRects3`) registry settings set to `3` (Auto-Hide Enabled).
- **Screen Real Estate**: Maximum vertical height reserved for IDEs, code editors, and terminal windows.

### 3. Application Launcher (`Alt + Q`)
- **Centered Search Dialog**: Custom WPF search interface (`scripts/app-launcher.ps1`).
- **Sharp Dark Theme**: `0px` corner radius, `#0e0e0e` background, `#373737` border.
- **24x24 App Icons**: Robust icon extraction via `[System.Windows.Media.Imaging.BitmapSizeOptions]::FromEmptyOptions()`.
- **Instant Floating Rule**: GlazeWM rule set to `set-floating --centered` & `ignore` on `.*myenv-app-launcher.*` to float instantly over all windows without layout jitter.

### 4. PowerShell Environment & Console Theme
- **Centralized Profile**: `powershell/profile.ps1` automatically loaded by `$PROFILE` (uses dynamic `$env:USERPROFILE`).
- **Dev Tools PATH**: `dotnet`, `flutter`, `jdk-17`, `Android SDK`, `kotlin`, `msys64`, `php`, `composer`, `nodejs`, `npm`.
- **True Black Console Theme**: `#000000` background with **32% Transparency** (`WindowAlpha = 173` / 68% opacity).
- **PSReadLine**: `Ctrl+Backspace` for backward word deletion (`BackwardKillWord`), history predictions (`ListView`), and Tab menu completion.

### 5. CMD (Command Prompt) Auto-Completion & Clink History Predictions
- **Clink History Auto-Suggestions**: Built-in real-time command prediction as you type (`autosuggest.enable = true`) matching previously executed commands in dark gray text. Press `→` (Right Arrow) to accept.
- **Tab & Hotkeys**: `Tab` for completion, `F8` for history search matching typed prefix, `F7` for history list popup, `Ctrl+R` for interactive history search.
- **AutoRun Initialization**: `scripts/cmd-init.cmd` registered under `HKCU:\Software\Microsoft\Command Processor\AutoRun`.
- **Doskey Aliases**: `ls`, `ll`, `la`, `clear`, `croot`, `gs`, `ga`, `gc`, `gp`, `gl`.
- **Colored Prompt**: Displays timestamp, username, computer name, and current path.

### 6. Package Manifest & Auto Restoration (`winget-packages.json`)
- **Package Manifest**: Central `winget-packages.json` storing developer tool IDs & versions for seamless system bootstrapping.
- **Automated Restore**: `scripts/install-packages.ps1` restores all tools via `winget import`.

### 7. Windows Language Switcher Hotkeys
- **Alt+Shift Disabled**: Registry settings (`HKCU:\Keyboard Layout\Toggle`) disable `Alt+Shift` and `Ctrl+Shift` to prevent accidental language changes during coding.
- **Win+Space Only**: `Win+Space` is configured as the sole input language switcher.

---

## 📂 Project Structure & Central Files

| Component | File / Path | Action |
|---|---|---|
| **GlazeWM Config** | [glazewm/config.yaml](file:///%USERPROFILE%/Documents/myenv/glazewm/config.yaml) | Main tiling WM rules, keybindings, workspaces |
| **YASB Config** | [yasb/config.yaml](file:///%USERPROFILE%/Documents/myenv/yasb/config.yaml) | Bar widgets, date/time format, alignment |
| **YASB Styles** | [yasb/styles.css](file:///%USERPROFILE%/Documents/myenv/yasb/styles.css) | Sharp dark theme, transparent bar, workspace colors |
| **PowerShell Profile** | [powershell/profile.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/profile.ps1) | Single source of truth for PowerShell `$PROFILE` |
| **PowerShell Theme** | [powershell/midnight-aurora.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/midnight-aurora.ps1) | PSReadLine options, colors, custom prompt |
| **Console Theme** | [powershell/console-theme.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/console-theme.ps1) | Black console background & 32% transparency |
| **CMD Macro Script** | [scripts/cmd-init.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cmd-init.cmd) | Doskey aliases, Clink injection, and prompt styling for `cmd.exe` |
| **Clink Settings** | [clink/clink_settings](file:///%USERPROFILE%/Documents/myenv/clink/clink_settings) | Clink history auto-suggestions configuration |
| **Package Manifest** | [winget-packages.json](file:///%USERPROFILE%/Documents/myenv/winget-packages.json) | Exported Winget package manifest for developer tools |

---

## 🛠️ Automation Scripts (`scripts/`)

| Script | Path | Description |
|---|---|---|
| **`setup-all.ps1`** | [scripts/setup-all.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/setup-all.ps1) | Master setup script: applies junctions, taskbar autohide, PSReadLine, Clink, Winget packages, CMD autorun, Alt+Shift disable, and reloads YASB. |
| **`install-packages.ps1`** | [scripts/install-packages.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-packages.ps1) | Restores/installs all developer packages from `winget-packages.json`. |
| **`set-taskbar-autohide.ps1`** | [scripts/set-taskbar-autohide.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-taskbar-autohide.ps1) | Toggles Windows Taskbar auto-hide in Registry and restarts Explorer. |
| **`set-ctrl-backspace.ps1`** | [scripts/set-ctrl-backspace.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-ctrl-backspace.ps1) | Binds `Ctrl+Backspace` for word deletion in PSReadLine and saves to `$PROFILE`. |
| **`disable-alt-shift-lang.ps1`** | [scripts/disable-alt-shift-lang.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/disable-alt-shift-lang.ps1) | Disables `Alt+Shift` language switching in Registry, leaving `Win+Space` as primary. |
| **`set-cmd-autocompletion.ps1`** | [scripts/set-cmd-autocompletion.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-cmd-autocompletion.ps1) | Enables CMD Tab auto-completion, triggers `install-clink.ps1`, and registers `cmd-init.cmd` AutoRun. |
| **`install-clink.ps1`** | [scripts/install-clink.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-clink.ps1) | Installs Clink via `winget`, enables Clink AutoRun, and applies `clink_settings` from `myenv`. |
| **`app-launcher.ps1`** | [scripts/app-launcher.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/app-launcher.ps1) | Fast WPF application launcher search dialog (`Alt + Q`). |
| **`cb.cmd`** | [scripts/cb.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cb.cmd) | Runs a command, outputs results directly to terminal, and copies output to system clipboard. |
| **`open-terminal-here.exe`** | [scripts/open-terminal-here.exe](file:///%USERPROFILE%/Documents/myenv/scripts/open-terminal-here.exe) | Instant native launcher (< 10ms, 0% window flash) to open CMD or PS at active File Explorer path. |




---

## ⌨️ GlazeWM Keybindings Cheat Sheet

### Window & Focus Controls
| Keybinding | Action |
|---|---|
| `Alt + H / ←` | Focus left window |
| `Alt + L / →` | Focus right window |
| `Alt + K / ↑` | Focus top window |
| `Alt + J / ↓` | Focus bottom window |
| `Alt + Shift + H/L/K/J` | Move focused window in direction |
| `Alt + Space` | Cycle window focus (tiling -> floating -> fullscreen) |
| `Alt + Shift + Space` | Toggle window floating (centered) |
| `Alt + T` | Return window to tiling state |
| `Alt + F` | Toggle window fullscreen |
| `Alt + M` | Toggle window minimize |
| `Alt + Q` | Close focused window |

### Tiling Layout Direction Controls
| Keybinding | Action |
|---|---|
| `Alt + V` | Toggle tiling split direction (Horizontal <-> Vertical) |
| `Alt + Shift + V` | **Force Vertical Split** (stack new window underneath) |
| `Alt + Ctrl + V` | **Force Horizontal Split** (place new window side-by-side) |
| `Alt + Shift + W` | Redraw all windows layout tree |

### Resizing Windows
| Keybinding | Action |
|---|---|
| `Alt + U` / `Alt + P` | Decrease / Increase window width (2%) |
| `Alt + I` / `Alt + O` | Decrease / Increase window height (2%) |
| `Alt + R` | Enter Interactive Resize Mode (Use HJKL / Arrows, `Esc`/`Enter` to exit) |

### Workspaces & Multi-Monitor Navigation
- **Left Display `DISPLAY1` (Monitor Index 0)**: Workspaces `1` to `5`
- **Right Display `DISPLAY8` (Monitor Index 1)**: Workspaces `6` to `10`

| Keybinding | Action |
|---|---|
| `Alt + 1..5` | Focus workspace 1-5 (Left Monitor) |
| `Alt + 6..0` | Focus workspace 6-10 (Right Monitor, `Alt+0` = 10) |
| `Alt + Shift + 1..0` | Move window to workspace 1-10 and focus it |
| `Alt + PageUp` / `Alt + A` | Focus previous active workspace |
| `Alt + PageDown` / `Alt + S` | Focus next active workspace |
| `Alt + D` | Focus recent workspace |
| `Alt + Shift + A/F/D/S` | Move workspace to Left / Right / Top / Bottom monitor |

### Applications & Management
| Keybinding | Action |
|---|---|
| `Alt + Shift + Q` | Launch App Launcher Search Dialog (`app-launcher.ps1`) |
| `Alt + Enter` | Launch Command Prompt (`cmd.exe`) |
| `Alt + Ctrl + Enter` | Launch Windows PowerShell (`powershell.exe`) |
| `Alt + Ctrl + T` | Open PowerShell at current File Explorer directory (`open-terminal-here.ps1`) |
| `Alt + Shift + T` | Open CMD at current File Explorer directory (`open-terminal-here.ps1`) |
| `Alt + Shift + R` | Reload GlazeWM configuration (`config.yaml`) |
| `Alt + Shift + E` | Exit GlazeWM safely |
| `Alt + Shift + P` | Toggle Pause GlazeWM window management |

---

## 🔗 Directory Junctions & Startup

Central environment directory junctions:
```powershell
C:\Users\moham\.config\yasb  -> C:\Users\moham\Documents\myenv\yasb
C:\Users\moham\.glzr\glazewm -> C:\Users\moham\Documents\myenv\glazewm
C:\Users\moham\.glzr\zebar   -> C:\Users\moham\Documents\myenv\zebar
```

Startup configuration:
- **GlazeWM**: `GlazeWM.lnk` in Startup folder pointing to `myenv/glazewm/config.yaml`.
- **YASB**: `YASB` registry entry in `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.

To run master setup at any time:
```powershell
powershell -ExecutionPolicy Bypass -File "C:\Users\moham\Documents\myenv\scripts\setup-all.ps1"
```