# 🔠 BarTranslator Source Code (`tools/bar-translator/`)

High-performance, ultra-fast background daemon and selection monitor that automatically translates highlighted English text to Arabic in real time and displays it on the top status bar (YASB / Zebar).

## 🚀 Features

- **Automatic Mouse Drag & Double-Click Detection**: Uses a low-level Win32 mouse hook (`WH_MOUSE_LL`) to detect text selection gestures across any application (browsers, IDEs, PDFs, terminals) with intelligent debouncing (< 75ms).
- **Clipboard Format Listener**: Also detects manual `Ctrl+C` copies without interfering with normal clipboard usage.
- **English-to-Arabic Real-Time Translation**: Free, high-speed translation engine via Google Translate API with in-memory caching for 0ms repeated lookups.
- **Smart Truncation**: Displays the first word / part of the translation on the bar by default, expanding to the full translated sentence upon clicking the widget.
- **Atomic State Persistence**: Writes to `scripts/bar-translator/state.json` atomically to prevent file lock contention.
- **Local HTTP API (`http://127.0.0.1:49876`)**: Exposes `/state`, `/copy`, `/clear`, and `/translate` endpoints for seamless integration with Zebar WebView2 and external scripts.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`BarTranslator.csproj`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/BarTranslator.csproj) | .NET 10 project definition targeting `net10.0-windows10.0.19041.0` (WinExe mode for zero window flash). |
| [`Program.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/Program.cs) | Application entry point, single-instance mutex (`MyEnv_BarTranslator_Daemon`), CLI dispatcher, and Win32 message loop. |
| [`SelectionMonitor.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/SelectionMonitor.cs) | Low-level mouse hook (`WH_MOUSE_LL`) tracking drag/double-click selection, window filtering, and safe text capture. |
| [`TranslationEngine.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/TranslationEngine.cs) | Async Google Translate client with fallback, caching, English detection, and smart Arabic truncation. |
| [`StateManager.cs`](file:///C:/Users/moham/Documents/myenv/tools/bar-translator/StateManager.cs) | Manages atomic writes to `state.json` and runs local HTTP server on port 49876 for Zebar. |

## 🛠️ Build & Publish

```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\bar-translator"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\bar-translator"
```
