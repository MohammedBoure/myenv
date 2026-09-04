# 🔠 BarTranslator Source Code (`tools/bar-translator/`)

High-performance, ultra-fast background daemon and selection monitor that automatically translates highlighted English text to Arabic in real time and provides unified status bar controls for YASB and Zebar.

## 🚀 Features

- **Consolidated Settings & Dropdown Menu**: Spawns a sleek dark-themed context menu containing all translation controls (Auto-Translate Copied Text, Auto-Select Capture, Focus Mode, Bilingual display, Copy, Clear) and dynamic container/widget visibility toggles for the status bar.
- **Dynamic Container Visibility Management**: Reads and updates `yasb/config.yaml` in real time, instantly showing/hiding bar containers (Workspaces, CPU, GPU, RAM, Network, Clock/Date, Audio, Mic, GitHub, Notifications, Home, Power Menu) via hot-reload.
- **Live State Synchronization**: Background daemon uses `FileSystemWatcher` and IPC endpoints to immediately sync setting changes across processes without restarting.
- **Automatic Mouse Drag & Double-Click Detection**: Uses a low-level Win32 mouse hook (`WH_MOUSE_LL`) to detect text selection gestures across any application (browsers, IDEs, PDFs, terminals) with intelligent debouncing (< 75ms).
- **Clipboard Format Listener**: Detects manual `Ctrl+C` copies and automatically translates copied English text when enabled (`ClipboardTranslateEnabled`).
- **English-to-Arabic Real-Time Translation**: Free, high-speed translation engine via Google Translate API with in-memory caching for 0ms repeated lookups and sentences up to 60 words.
- **Atomic State Persistence**: Writes to `scripts/bar-translator/state.json` atomically to persist translation text and user settings (`auto_capture`, `clipboard_translate`, `translation_mode`, `show_english`).
- **Local HTTP API (`http://127.0.0.1:49876`)**: Exposes `/state`, `/reload_state`, `/copy`, `/clear`, `/translate`, `/toggle_clipboard_translate`, `/toggle_auto_capture`, `/toggle_translation_mode`, `/toggle_show_english`, `/get_widgets`, `/toggle_widget`, and `/set_widget` endpoints.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`BarTranslator.csproj`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/BarTranslator.csproj) | .NET 10 project definition targeting `net10.0-windows` (WinExe mode with Windows Forms for zero window flash). |
| [`Program.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/Program.cs) | Application entry point, single-instance mutex (`MyEnv_BarTranslator_Daemon`), CLI dispatcher, dark context menu renderer, and Win32 message loop. |
| [`BarConfigManager.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/BarConfigManager.cs) | Status bar configuration manager reading and dynamically updating `yasb/config.yaml` widget sections (`left`, `center`, `right`). |
| [`SelectionMonitor.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/SelectionMonitor.cs) | Low-level mouse hook (`WH_MOUSE_LL`) tracking drag/double-click selection, window filtering, safe text capture, and clipboard translation gating. |
| [`TranslationEngine.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/TranslationEngine.cs) | Async Google Translate client with fallback, caching, bilingual detection, sentence support up to 60 words, and equal divided priority text truncation. |
| [`StateManager.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/StateManager.cs) | Manages atomic state & settings persistence to `state.json`, FileSystemWatcher sync, and local HTTP server on port 49876 with REST API. |

## 🛠️ Build & Publish

```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\bar-translator"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\bar-translator"
```
