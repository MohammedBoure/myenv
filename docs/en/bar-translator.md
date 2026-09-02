# 🌐 BarTranslator (Status Bar Real-Time Selection Translator & Controls)

An ultra-fast, background selection monitor, translator, and status bar control suite built in **C# (.NET 10)** and integrated directly with **YASB** and **Zebar**.

---

## ⚡ Overview & Features

- **Consolidated Settings & Dropdown Menu**:
  - Clicking the settings dropdown button on the top bar triggers a custom dark-mode context menu with 0px sharp corners.
  - Contains all translation and language toggles:
    - **Auto-Translate Copied Text** (`Ctrl+C` clipboard listener toggle)
    - **Auto-Select Text Capture** (Mouse selection without `Ctrl+C` toggle)
    - **Translation Focus Mode**
    - **Show/Hide English text**
    - **Copy Arabic translation**
    - **Clear translation**
- **Dynamic Status Bar Container Visibility**:
  - Checkbox toggles inside the dropdown menu for all widgets on the bar (Workspaces, CPU, GPU, RAM, Network Traffic, Clock/Date, Audio Volume, Microphone, GitHub, Notifications, Home, Power Menu).
  - Toggling any item immediately updates `yasb/config.yaml`, causing YASB to dynamically show or hide that container in real time.
  - Quick presets: **Show All Containers** and **Minimalist Preset** (Workspaces & Clock only).
- **Live Background Daemon Synchronization**:
  - The background daemon uses `FileSystemWatcher` and local HTTP endpoints (`/reload_state`) to instantly synchronize setting changes made from the menu or CLI without restarting.
- **Continuous Bilingual Display**:
  - The top bar permanently displays both the English word/phrase and its Arabic translation side-by-side (e.g. `🌐 compiler ➔ مترجم`).
- **Ultra-Fast Translation Engine**:
  - Low-latency `SocketsHttpHandler` connection pooling and persistent in-memory caching for 0ms instantaneous lookups on repeated queries.
  - YASB polling interval optimized to **120ms** for immediate visual feedback.

---

## ⌨️ Mouse Controls on Top Bar

| Action | Result |
|---|---|
| **Left Click on Menu Button (`bar_menu`)** | Open unified settings dropdown menu (Translation controls & widget visibility toggles). |
| **Left Click on Translator Widget** | Toggle between short preview and full translated text (`display_short` ⟷ `display_full`). |
| **Right Click on Translator Widget** | Copy full Arabic translation directly to system clipboard. |
| **Middle Click on Translator Widget** | Clear current translation and reset field to default idle state (`English ➔ العربية`). |

---

## 📂 Project Architecture

| Component | Path | Description |
|---|---|---|
| **C# Source Code** | [`tools/bar-translator/`](file:///%USERPROFILE%/Documents/myenv/tools/bar-translator) | Background daemon source with Win32 hooks (`WH_MOUSE_LL`). |
| **Bar Config Manager** | [`tools/bar-translator/BarConfigManager.cs`](file:///%USERPROFILE%/Documents/myenv/tools/bar-translator/BarConfigManager.cs) | Dynamic manager for reading and modifying `yasb/config.yaml` widget lists. |
| **Published Daemon** | [`scripts/bar-translator/BarTranslator.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/BarTranslator.exe) | Compiled standalone background process. |
| **Fast CLI Reader** | [`scripts/bar-translator/get-state-reader.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/get-state-reader.exe) | Instant native state reader with continuous display defaults. |
| **Action Utility** | [`scripts/bar-translator/translator-action.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/translator-action.exe) | Headless utility executing copy and clear callbacks without console windows. |
| **Menu Wrapper** | [`scripts/bar-translator/show-menu.cmd`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/show-menu.cmd) | Spawns the dark dropdown settings menu at the cursor position. |
| **State File** | [`scripts/bar-translator/state.json`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/state.json) | Shared state file read by YASB and Zebar. |
| **YASB Config** | [`yasb/config.yaml`](file:///%USERPROFILE%/Documents/myenv/yasb/config.yaml) | Widget registration under `primary-bar.widgets`. |
| **YASB Styles** | [`yasb/styles.css`](file:///%USERPROFILE%/Documents/myenv/yasb/styles.css) | Sharp dark theme styling for widgets. |

---

## 🛠️ Compilation & Rebuild Guide

From project directory:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\bar-translator"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\bar-translator"
```
