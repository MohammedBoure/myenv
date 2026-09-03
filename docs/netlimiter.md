# NetLimiter Network Control Documentation

A stable and powerful bandwidth management tool for Windows designed to set download and upload speed limits on a per-application basis.

---

## Key Features in `myenv`

| Feature | Description |
|---|---|
| **Per-App Bandwidth Limits** | Set strict Download (**DL Limit**) and Upload (**UL Limit**) caps for any process (e.g., `chrome.exe`, `steam.exe`). |
| **System Stability** | Runs cleanly as a background Windows Service with minimal resource footprint. |
| **GlazeWM Integration** | Configured in `glazewm/config.yaml` to open as a **Floating Centered** window. |
| **WinGet Automation** | Added to `winget-packages.json` (`LocktimeSoftware.NetLimiter`) for automated restoration. |

---

## Quick Guide

1. **Launch**:
   Open App Launcher (`Alt + Q`), type `NetLimiter`, press `Enter`.
2. **Set Limit**:
   - Locate target app in the main list.
   - Click **DL Limit** or **UL Limit** column.
   - Enable checkbox `[x]` and specify speed limit (e.g., `2 MB/s` or `500 KB/s`).

---

## PowerShell Installation

```powershell
winget install --id LocktimeSoftware.NetLimiter --source winget
```
