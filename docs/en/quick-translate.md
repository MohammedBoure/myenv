# 🔠 QuickTranslate Documentation

An instant screen selection & region OCR translation tool built in **C# (WPF)** using native **WinRT OCR** and **Google Translate API**.

---

## ⚡ Execution Modes & Keybindings

| Keybinding | Mode | Mechanism & Behavior |
|---|---|---|
| `Win + Shift + C` / `Alt + Shift + C` | **Instant Selection Translate** | Auto-triggers `Ctrl+C` on currently highlighted text in any app & opens translation instantly (< 15ms) |
| `Win + Shift + Q` / `Alt + Shift + T` | **Screen Region OCR Translate** | Drag-select any region on screen (+) to extract text via WinRT OCR and translate |

---

## 📂 Project Structure (`tools/quick-translate/`)

| File | Purpose / Role |
|---|---|
| [App.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml) / [App.xaml.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml.cs) | App entry point, hotkey dispatcher, and non-blocking window launch |
| [SelectionWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) | Screen region selection overlay (+) |
| [ResultWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/ResultWindow.xaml) | Translation display window with real-time text editor & auto-copy |
| [OcrService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/OcrService.cs) | Text extraction via native `Windows.Media.Ocr` |
| [TranslationService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TranslationService.cs) | Free Google Translate API fetcher |
| [scripts/quick-translate.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/quick-translate.ps1) | Background process launcher script |

---

## 🛠️ Build & Modification Guide

### 1. Source Code Editing:
- **UI Theme & Layout**: Edit `*.xaml` files (Catppuccin sharp dark theme).
- **Target Language**: Edit `targetLang = "ar"` in [TranslationService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TranslationService.cs).

### 2. Rebuild Commands:
From project directory:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\quick-translate"
dotnet build -c Release
```
Or publish directly to binary output folder:
```powershell
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\quick-translate"
```
