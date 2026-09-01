# 🌐 BarTranslator (Status Bar Real-Time Selection Translator)

An ultra-fast, background selection monitor and status bar translation widget built in **C# (.NET 10)** and integrated directly with **YASB** and **Zebar**.

---

## ⚡ Overview & Features

- **Instant Selection Capture (< 75ms)**: Automatically detects when you drag-select or double-click text in any application (web browser, code editor, terminal, PDF viewer, Office apps) without requiring any hotkey presses.
- **Smart Arabic Truncation**:
  - By default, only the first word / initial part appears on the top bar (e.g. `🌐 مرحبا…`).
  - Clicking on the widget toggles to display the full translated sentence.
  - Hovering over the widget displays a rich tooltip containing the original English text and the full Arabic translation.
- **Ultra-Fast Translation Engine**: Uses async Google Translate API with in-memory caching for 0ms instantaneous lookups on repeated phrases.
- **Bar Integration**:
  - **YASB**: Integrated as a native `yasb.custom.CustomWidget` querying `get-state.exe` (< 5ms execution). Auto-hides (`hide_empty: true`) when no text is active.
  - **Zebar**: Integrated into the starter pack with real-time state fetching over localhost HTTP (`http://127.0.0.1:49876/state`).

---

## ⌨️ Mouse Controls on Top Bar

| Action | Result |
|---|---|
| **Left Click** | Toggle between short preview (first word) and full translated text. |
| **Right Click** | Copy full Arabic translation directly to system clipboard. |
| **Middle Click** | Clear current translation and hide widget from the bar. |

---

## 📂 Project Architecture

| Component | Path | Description |
|---|---|---|
| **C# Source Code** | [`tools/bar-translator/`](file:///%USERPROFILE%/Documents/myenv/tools/bar-translator) | Background daemon source with Win32 hooks (`WH_MOUSE_LL`). |
| **Published Daemon** | [`scripts/bar-translator/BarTranslator.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/BarTranslator.exe) | Compiled standalone background process. |
| **Fast CLI Reader** | [`scripts/bar-translator/get-state.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/get-state.exe) | Instant native state reader for YASB. |
| **Action Utility** | [`scripts/bar-translator/translator-action.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/translator-action.exe) | Headless utility executing copy and clear callbacks. |
| **State File** | [`scripts/bar-translator/state.json`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/state.json) | Shared state file read by YASB and Zebar. |
| **YASB Config** | [`yasb/config.yaml`](file:///%USERPROFILE%/Documents/myenv/yasb/config.yaml) | Widget registration under `primary-bar.widgets.left`. |
| **YASB Styles** | [`yasb/styles.css`](file:///%USERPROFILE%/Documents/myenv/yasb/styles.css) | Sharp dark theme styling for `.translator-widget`. |
| **Zebar Widget** | [`zebar/packs/glzr-io.starter/`](file:///%USERPROFILE%/Documents/myenv/zebar/packs/glzr-io.starter) | Zebar HTML/React pack with live translation button. |

---

## 🛠️ Compilation & Rebuild Guide

From project directory:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\bar-translator"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\bar-translator"
```
