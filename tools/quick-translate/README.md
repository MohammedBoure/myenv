# QuickTranslate Source Code Directory (`tools/quick-translate/`)

Source code for **QuickTranslate**, a fast screen region OCR and multi-tier Arabic translation tool for Windows 10/11 built with .NET 10 WPF and WinRT native OCR.

## 📂 Files & Structure

| File | Purpose |
|---|---|
| [`QuickTranslate.csproj`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/QuickTranslate.csproj) | .NET 10 project file configuring Windows Desktop SDK, WPF, and Windows 10 TFM (`net10.0-windows10.0.19041.0`). |
| [`App.xaml`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/App.xaml) / [`App.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/App.xaml.cs) | Application entry point handling CLI modes (`--clipboard`, `--type`, or screen selection). |
| [`OcrService.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/OcrService.cs) | High-speed offline OCR engine utilizing native `Windows.Media.Ocr.OcrEngine` (English and French). |
| [`TranslationService.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/TranslationService.cs) | Multi-tier fallback translation service querying Google Translate client endpoints with deep JSON parsing. |
| [`SelectionWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) / [`SelectionWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/SelectionWindow.xaml.cs) | Transparent crosshair drag-selection overlay window for screen capture. |
| [`ResultWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/ResultWindow.xaml) / [`ResultWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/ResultWindow.xaml.cs) | Floating popup window displaying original OCR text and translated Arabic result with one-click copy. |
| [`TypeTranslateWindow.xaml`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/TypeTranslateWindow.xaml) / [`TypeTranslateWindow.xaml.cs`](file:///C:/Users/moham/Documents/myenv/tools/quick-translate/TypeTranslateWindow.xaml.cs) | Interactive instant typing translation dialog with live translation and paste integration. |
