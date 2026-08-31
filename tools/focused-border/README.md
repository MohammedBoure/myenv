# Focused Window Border Source Code (`tools/focused-border/`)

High-performance .NET 10 WPF active window border service designed specifically for Windows 10 and GlazeWM.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`FocusedBorder.csproj`](file:///C:/Users/moham/Documents/myenv/tools/focused-border/FocusedBorder.csproj) | .NET 10 project file with WPF enabled (`net10.0-windows`). |
| [`Program.cs`](file:///C:/Users/moham/Documents/myenv/tools/focused-border/Program.cs) | Application entry point and `BorderService` utilizing WinEventHooks (`EVENT_SYSTEM_FOREGROUND`, `EVENT_OBJECT_LOCATIONCHANGE`, `EVENT_OBJECT_DESTROY`), `DWMWA_EXTENDED_FRAME_BOUNDS`, `DWMWA_CLOAKED`, and kernel-level `HTTRANSPARENT` click-through. |

## 🚀 Compilation & Execution
Compiled via .NET CLI:
```powershell
dotnet publish -c Release -o "%USERPROFILE%\Documents\myenv\scripts\focused-border"
```
Automatically started by GlazeWM via [`glazewm/config.yaml`](file:///C:/Users/moham/Documents/myenv/glazewm/config.yaml):
```yaml
startup_commands:
  - 'shell-exec %USERPROFILE%\Documents\myenv\scripts\focused-border\FocusedBorder.exe'
```
