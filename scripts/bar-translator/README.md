# BarTranslator Scripts & Published Binaries (`scripts/bar-translator/`)

Standalone binaries and automation helpers for the status bar real-time English-to-Arabic selection translator and status bar controls.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`BarTranslator.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/BarTranslator.exe) | Published background daemon that monitors text selection, manages status bar widget visibility, and renders the settings dropdown menu. |
| [`get-state.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/get-state.exe) | Ultra-fast native state reader (< 5ms) invoked by YASB's custom widget to fetch current translation JSON. |
| [`get-state-reader.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/get-state-reader.exe) | Live state reader with default idle placeholder for continuous top bar visibility. |
| [`GetState.cs`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/GetState.cs) | Source code for `get-state.exe` and `get-state-reader.exe` (compiled with .NET Framework `csc.exe`). |
| [`translator-action.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/translator-action.exe) | Headless Win32 utility handling `copy` and `clear` callbacks without console flashes. |
| [`Actions.cs`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/Actions.cs) | Source code for `translator-action.exe`. |
| [`copy-state.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/copy-state.cmd) | Wrapper triggered on bar right-click to copy the full Arabic translation to the clipboard. |
| [`clear-state.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/clear-state.cmd) | Wrapper triggered on bar middle-click to clear the translation and hide the widget. |
| [`toggle-clipboard-translate.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/toggle-clipboard-translate.cmd) | Helper script to toggle automatic translation of copied text on or off. |
| [`toggle-auto-capture.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/toggle-auto-capture.cmd) | Helper script to toggle automatic mouse text selection capture on or off. |
| [`toggle-translation-mode.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/toggle-translation-mode.cmd) | Helper script to enter or exit dedicated full sentence reading Translation Focus Mode. |
| [`toggle-show-english.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/toggle-show-english.cmd) | Helper script to toggle bilingual display vs Arabic translation only. |
| [`show-menu.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/show-menu.cmd) | Opens the native desktop settings popup menu with all translator controls and bar widget visibility toggles. |
| [`state.json`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/state.json) | Shared state file containing the latest translated text, short preview, and original text. |

## 🕹️ Usage & Testing

- **Open settings dropdown menu**:
  ```powershell
  .\show-menu.cmd
  ```
- **Toggle copied text translation**:
  ```powershell
  .\BarTranslator.exe --toggle-clipboard-translate
  ```
- **List and toggle bar widgets**:
  ```powershell
  .\BarTranslator.exe --list-widgets
  .\BarTranslator.exe --toggle-widget cpu
  ```
- **Fetch current state**:
  ```powershell
  .\get-state.exe
  ```
- **Copy translation to clipboard**:
  ```powershell
  .\copy-state.cmd
  ```
- **Clear translation**:
  ```powershell
  .\clear-state.cmd
  ```
