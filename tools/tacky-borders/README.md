# Tacky-Borders Binary Directory (`tools/tacky-borders/`)

Contains the compiled binary for **Tacky-Borders**, a lightweight native window border manager written in Rust.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`tacky-borders.exe`](file:///C:/Users/moham/Documents/myenv/tools/tacky-borders/tacky-borders.exe) | Compiled standalone native 64-bit executable using Direct3D / DirectX and Win32 event hooks (`SetWinEventHook`) for 100% click-through active window borders. |

## 🚀 Execution & GlazeWM Integration
Launched automatically by GlazeWM on startup via [`glazewm/config.yaml`](file:///C:/Users/moham/Documents/myenv/glazewm/config.yaml):
```yaml
startup_commands:
  - 'shell-exec %USERPROFILE%\Documents\myenv\tools\tacky-borders\tacky-borders.exe'
```
Configuration is loaded from `%USERPROFILE%\.config\tacky-borders\config.yaml` ([`tacky-borders/config.yaml`](file:///C:/Users/moham/Documents/myenv/tacky-borders/config.yaml)).
