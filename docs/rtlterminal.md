# RtlTerminal (Arabic & RTL Windows Terminal) Documentation

**RtlTerminal** is an open-source, high-performance terminal emulator designed for Windows with native Right-to-Left (RTL) text shaping and bidirectional rendering for Arabic, Persian, and mixed Unicode command-line workflows.

---

## 🌟 Overview & Architecture

In `myenv`, **RtlTerminal** is configured as the primary terminal emulator bound to `Alt + Enter`:
- **Location**: [`tools/rtlterminal/RtlTerminal.exe`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.exe)
- **Engine**: Windows ConPTY API + custom WPF hardware-accelerated cell renderer.
- **Profiles Supported**: Command Prompt (`cmd.exe`), PowerShell (`powershell.exe` / `pwsh`), and WSL distributions.
- **GlazeWM Integration**: Bound to `Alt + Enter` via the fast native launcher [`scripts/open-terminal-here.exe`](file:///C:/Users/moham/Documents/myenv/scripts/open-terminal-here.exe) with automatic tiling matching rules.

---

## 🚀 Key Features

1. **Smart RTL Text Shaping**:
   - Automatically detects Arabic and Persian words and renders them in correct Right-to-Left order while keeping Latin text, paths, commands, flags, and numbers oriented Left-to-Right.
   - Preserves ANSI colors, interactive terminal applications (fzf, micro, git), progress bars, and Unicode symbols.
2. **Tabbed & Split Panes**:
   - Chrome-style tabs and multiple split panes (horizontal & vertical).
   - Instant switching between tabs and panes.
3. **Seamless UTF-8 & Windows Integration**:
   - Automatically launches shells with CodePage 65001 (`chcp 65001`).
   - Integrated with `myenv` environment variables (`PYTHONIOENCODING=utf-8`, `LANG=en_US.UTF-8`).
4. **Shell Launchers & Shims**:
   - Native shims provided in `scripts/`: [`rtlterminal.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/rtlterminal.cmd) and [`rtl.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/rtl.cmd).
   - Registered in user `PATH`, PowerShell functions/aliases (`rtl`, `rtlterminal`), and CMD doskey macros.

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action / Function |
|---|---|
| `Alt + Enter` | Launch **RtlTerminal** at the current active File Explorer path (or Home) |
| `Ctrl + Shift + T` | Open a new terminal tab |
| `Ctrl + Tab` / `Ctrl + Shift + Tab` | Switch between tabs |
| `Ctrl + Shift + W` | Close the active pane or tab |
| `Ctrl + Shift + D` | Split active tab into multiple panes |
| `Ctrl + Shift + Left` / `Right` | Switch focus between split panes |
| `Ctrl + Shift + C` / `Ctrl + C` (with selection) | Copy selected text |
| `Ctrl + Shift + V` / `Ctrl + V` | Paste text or clipboard file path |
| `Ctrl + Shift + A` | Select all text in buffer |
| `Ctrl + Mouse Click` | Open hyperlink under cursor |

---

## ⚙️ Customization & Settings

RtlTerminal stores its user configuration under the Windows Registry key:
`HKCU:\Software\RtlTerminal`

### Registry Configuration Values

| Value Name | Type | Description & Recommended Values |
|---|---|---|
| `FontFamily` | `String` | Terminal font (e.g., `Cascadia Code`, `Segoe UI`, `Cairo`) |
| `FontSize` | `String` | Terminal font size (Default: `14`) |
| `FontBold` | `DWord` | `1` for Bold, `0` for Regular font weight |
| `Theme` | `String` | Window UI theme (`Dark` or `Light`) |
| `ColorScheme` | `String` | Color palette (`Campbell`, `Catppuccin`, `Dracula`, `Tokyo Night`, `Nord`, `One Half`) |
| `TerminalProfile` | `String` | Default startup profile: `Command Prompt`, `PowerShell`, or `WSL` |
| `HistorySize` | `DWord` | Scrollback buffer lines (`2000`, `5000`, or `10000`) |

### Automated Setup Script

To reinstall, update, or configure RtlTerminal settings and system bindings at any time, run:
```powershell
& "$env:USERPROFILE\Documents\myenv\scripts\setup-arabic-terminal.ps1"
```
Or execute the master environment setup:
```powershell
& "$env:USERPROFILE\Documents\myenv\scripts\setup-all.ps1"
```
