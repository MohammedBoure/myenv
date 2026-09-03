# QuickTranslate Documentation

An instant screen selection, clipboard, and interactive type-and-paste translation tool built in C# (WPF) using native WinRT OCR and Google Translate API.

---

## Execution Modes & Keybindings

| Keybinding | Mode | Mechanism & Behavior |
|---|---|---|
| `Win + Shift + X` / `Alt + Shift + E` | **Type & Paste Translate** | Opens a centered floating dialog to type text with real-time translation preview. Pressing `Enter` closes the dialog and directly pastes the translated text into the active window. |
| `Win + Shift + C` / `Alt + Shift + C` | **Instant Selection Translate** | Auto-triggers `Ctrl+C` on currently highlighted text in any app & opens translation instantly (< 15ms) |
| `Win + Shift + Q` / `Alt + Shift + T` | **Screen Region OCR Translate** | Drag-select any region on screen (+) to extract text via WinRT OCR and translate |

---

## TypeTranslate Dialog Keybindings

- `Enter`: Apply and directly paste translated text into target active control.
- `Tab`: Cycle language pair (Auto AR <-> EN, AR -> EN, EN -> AR, AR -> FR, AR -> DE, AR -> ES, AR -> TR).
- `Shift + Enter`: Insert newline into the input box.
- `Esc`: Cancel and close without pasting.

---

## Project Structure (`tools/quick-translate/`)

| File | Purpose / Role |
|---|---|
| [App.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml) / [App.xaml.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml.cs) | App entry point, previous foreground HWND capture, and mode dispatcher (`--type`, `--clipboard`, OCR) |
| [TypeTranslateWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TypeTranslateWindow.xaml) / [.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TypeTranslateWindow.xaml.cs) | Interactive type-to-translate dialog with real-time preview and direct keystroke injection |
| [SelectionWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) | Screen region selection overlay (+) |
| [ResultWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/ResultWindow.xaml) | Translation display window with real-time text editor & auto-copy |
| [OcrService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/OcrService.cs) | Text extraction via native `Windows.Media.Ocr` |
| [TranslationService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TranslationService.cs) | Free Google Translate API fetcher with in-memory caching and bidirectional support |
| [scripts/quick-translate.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/quick-translate.ps1) | Background process launcher script |

---

## Build & Modification Guide

### 1. Rebuild & Publish Commands:
From project directory:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\quick-translate"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\quick-translate"
```
