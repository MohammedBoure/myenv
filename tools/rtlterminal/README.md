# RtlTerminal Directory (`tools/rtlterminal/`)

Binaries and dependencies for **MyRtlTerminal** (`RtlTerminal`), a custom Windows terminal emulator built from [`MohammedBoure/MyRtlTerminal`](https://github.com/MohammedBoure/MyRtlTerminal) with native Arabic, Persian, and Right-to-Left (RTL) text rendering, ConPTY integration, and GlazeWM tiling window manager optimizations.

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`RtlTerminal.exe`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.exe) | Main entry executable for RtlTerminal. Built without top title, tab, or menu bars for clean borderless tiling, prelaunches the initial shell without startup banner delays, and defaults to PowerShell 7 (`pwsh`). |
| [`RtlTerminal.dll`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.dll) | Core managed assembly containing WPF UI views, theme engines, RTL buffer processors, command-line help/settings commands, and terminal state management. |
| [`RtlTerminal.runtimeconfig.json`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.runtimeconfig.json) | .NET runtime configuration specifying target framework (`net10.0`) and WindowsDesktop framework options. |
| [`RtlTerminal.deps.json`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.deps.json) | Application dependency graph for NuGet packages and assemblies. |
| [`Assets/`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/Assets) | Bundled font licenses and metadata for the built-in terminal font assets. |
| [`ConPty/`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/ConPty) | Architecture-specific native ConPTY (`conpty.dll`) and `OpenConsole.exe` binaries (`x64`, `x86`, `arm64`) powering pseudo-terminal sessions on Windows. |
