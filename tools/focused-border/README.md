# Focused Window Border Service (`tools/focused-border/`)

Ultra-fast, native Win32/C# active window border overlay service designed specifically for Windows 10 and GlazeWM.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`FocusedBorder.cs`](file:///C:/Users/moham/Documents/myenv/tools/focused-border/FocusedBorder.cs) | High-performance C# source code utilizing `SetWinEventHook` (`EVENT_SYSTEM_FOREGROUND`, `EVENT_OBJECT_LOCATIONCHANGE`, `EVENT_OBJECT_DESTROY`), `DWMWA_EXTENDED_FRAME_BOUNDS`, `DWMWA_CLOAKED`, and kernel-level `HTTRANSPARENT` click-through. |
| [`FocusedBorder.exe`](file:///C:/Users/moham/Documents/myenv/tools/focused-border/FocusedBorder.exe) | Compiled standalone 64-bit native executable (<20KB, ~0.00% CPU) with single-instance mutex protection. |

## 🚀 Compilation & Execution
Compiled via standard .NET Framework C# compiler:
```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /optimize+ /out:"FocusedBorder.exe" "FocusedBorder.cs"
```
Automatically started by GlazeWM via [`glazewm/config.yaml`](file:///C:/Users/moham/Documents/myenv/glazewm/config.yaml):
```yaml
startup_commands:
  - 'shell-exec %USERPROFILE%\Documents\myenv\tools\focused-border\FocusedBorder.exe'
```
