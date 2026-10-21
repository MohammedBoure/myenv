# RtlTerminal Directory (`tools/rtlterminal/`)

Binaries and dependencies for **RtlTerminal**, an open-source Windows terminal emulator with native Arabic and Right-to-Left (RTL) text rendering, ConPTY integration, and tabbed sessions.

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`RtlTerminal.exe`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.exe) | Main entry executable for RtlTerminal. Launches the terminal window, handles RTL layout, and initiates ConPTY sessions. |
| [`RtlTerminal.dll`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.dll) | Core managed assembly containing WPF UI views, theme engines, and terminal state management. |
| [`RtlTerminal.runtimeconfig.json`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.runtimeconfig.json) | .NET runtime configuration specifying target framework (`net8.0-windows`) and framework options. |
| [`RtlTerminal.deps.json`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/RtlTerminal.deps.json) | Application dependency graph for NuGet packages and assemblies. |
| [`ConPty/`](file:///C:/Users/moham/Documents/myenv/tools/rtlterminal/ConPty) | Architecture-specific native ConPTY (`conpty.dll`) and `OpenConsole.exe` binaries powering pseudo-terminal sessions on Windows. |
