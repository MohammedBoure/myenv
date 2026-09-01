# BarTranslator Scripts & Published Binaries (`scripts/bar-translator/`)

Standalone binaries and automation helpers for the status bar real-time English-to-Arabic selection translator.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`BarTranslator.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/BarTranslator.exe) | Published background daemon that monitors text selection and performs instant translation. |
| [`get-state.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/get-state.exe) | Ultra-fast native state reader (< 5ms) invoked by YASB's custom widget to fetch current translation JSON. |
| [`GetState.cs`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/GetState.cs) | Source code for `get-state.exe` (compiled with .NET Framework `csc.exe`). |
| [`translator-action.exe`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/translator-action.exe) | Headless Win32 utility handling `copy` and `clear` callbacks without console flashes. |
| [`Actions.cs`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/Actions.cs) | Source code for `translator-action.exe`. |
| [`copy-state.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/copy-state.cmd) | Wrapper triggered on bar right-click to copy the full Arabic translation to the clipboard. |
| [`clear-state.cmd`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/clear-state.cmd) | Wrapper triggered on bar middle-click to clear the translation and hide the widget. |
| [`state.json`](file:///C:/Users/moham/Documents/myenv/scripts/bar-translator/state.json) | Shared state file containing the latest translated text, short preview, and original text. |

## 🕹️ Usage & Testing

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
