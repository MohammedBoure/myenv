# 🌐 BarTranslator (Status Bar Real-Time Selection Translator)

An ultra-fast, background selection monitor and status bar translation widget built in **C# (.NET 10)** and integrated directly with **YASB** and **Zebar**.

---

## ⚡ Overview & Features

- **Instant Selection Capture (< 25ms)**: Automatically detects when you drag-select or double-click text anywhere (browser, code editor, terminal, PDF viewer, Office apps) without requiring any manual copy (`Ctrl+C`) or hotkey presses.
- **Continuous Bilingual Display**:
  - The top bar permanently displays both the English word/phrase and its Arabic translation side-by-side (e.g. `🌐 compiler ➔ مترجم` or `🌐 deep learning ➔ تعلم…`).
  - Remains continuously visible on the bar until a new valid text is selected/copied.
- **10-Word Safety Filter**:
  - If selected or copied text exceeds **10 words**, it is automatically ignored.
  - The previous valid translation **remains continuously displayed** on the bar without interruption.
- **Smart Arabic & English Truncation**:
  - For phrases longer than 2–3 words, shows the essential English words and first Arabic word (e.g. `🌐 Artificial intelligence… ➔ ذكاء…`).
  - Left-clicking on the display location toggles to show the full English sentence and full Arabic translation.
  - Hovering displays a detailed tooltip with the complete bilingual text.
- **Ultra-Fast Translation Engine**:
  - Low-latency `SocketsHttpHandler` connection pooling and persistent in-memory caching for 0ms instantaneous lookups on repeated queries.
  - YASB polling interval optimized to **120ms** for immediate visual feedback.
- **Bar Integration**:
  - **YASB**: Placed prominently in `primary-bar.widgets.center` next to the clock; uses native `get-state-reader.exe` (< 3ms execution).
  - **Zebar**: Integrated into the starter pack with real-time state fetching over localhost HTTP (`http://127.0.0.1:49876/state`).

---

## ⌨️ Mouse Controls on Top Bar

| Action | Result |
|---|---|
| **Left Click** | Toggle between short preview and full translated text (`display_short` ⟷ `display_full`). |
| **Right Click** | Copy full Arabic translation directly to system clipboard. |
| **Middle Click** | Clear current translation and reset field to default idle state (`English ➔ العربية`). |

---

## 📂 Project Architecture

| Component | Path | Description |
|---|---|---|
| **C# Source Code** | [`tools/bar-translator/`](file:///%USERPROFILE%/Documents/myenv/tools/bar-translator) | Background daemon source with Win32 hooks (`WH_MOUSE_LL`). |
| **Published Daemon** | [`scripts/bar-translator/BarTranslator.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/BarTranslator.exe) | Compiled standalone background process. |
| **Fast CLI Reader** | [`scripts/bar-translator/get-state-reader.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/get-state-reader.exe) | Instant native state reader with continuous display defaults. |
| **Action Utility** | [`scripts/bar-translator/translator-action.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/translator-action.exe) | Headless utility executing copy and clear callbacks without console windows. |
| **State File** | [`scripts/bar-translator/state.json`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/state.json) | Shared state file read by YASB and Zebar. |
| **YASB Config** | [`yasb/config.yaml`](file:///%USERPROFILE%/Documents/myenv/yasb/config.yaml) | Widget registration under `primary-bar.widgets.center`. |
| **YASB Styles** | [`yasb/styles.css`](file:///%USERPROFILE%/Documents/myenv/yasb/styles.css) | Sharp dark theme styling for `.translator-widget`. |
| **Zebar Widget** | [`zebar/packs/glzr-io.starter/`](file:///%USERPROFILE%/Documents/myenv/zebar/packs/glzr-io.starter) | Zebar HTML/React pack with live translation button. |

---

## 🛠️ Compilation & Rebuild Guide

From project directory:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\bar-translator"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\bar-translator"
```
