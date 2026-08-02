# 🔠 QuickTranslate Documentation / دليل أداة الترجمة الفورية

Instant screen selection & region OCR translation tool built with **C# (WPF)** using **WinRT OCR** and **Google Translate API**.
أداة تظليل وترجمة فورية للشاشة بلغة **C# (WPF)** تعتمد على محرك **WinRT OCR** المدمج في Windows ومحرك **Google Translate API**.

---

## ⚡ Execution Modes & Hotkeys / أنماط التشغيل والاختصارات

| Hotkey / الاختصار | Mode / النمط | Mechanism / آلية العمل (English) | آلية العمل (العربية) |
|---|---|---|---|
| `Win + Shift + C` / `Alt + Shift + C` | **Instant Selection Translate** | Auto-triggers `Ctrl+C` on highlighted text & opens translation | نسخ تلقائي `Ctrl+C` للنص المظلل وفتحه في الترججمة مباشرة |
| `Win + Shift + Q` / `Alt + Shift + T` | **Screen Region OCR Translate** | Drag-select region (+) & extract text via WinRT OCR | تظليل أي منطقة بالسحب (+) واستخراج النصوص بـ OCR لترجمتها |

---

## 📂 Project Structure / هيكلية المشروع (`tools/quick-translate/`)

| File / الملف | Description / الوظيفة (English) | الوظيفة (العربية) |
|---|---|---|
| [App.xaml](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/App.xaml) / [App.xaml.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/App.xaml.cs) | App entry point & hotkey mode dispatcher | نقطة الانطلاق والتحكم بنمط التطبيق والوضع |
| [SelectionWindow.xaml](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) | Screen region selection overlay (+) | نافذة تحديد المنطقة على الشاشة بالسحب (+) |
| [ResultWindow.xaml](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/ResultWindow.xaml) | Translation result window & real-time editor | نافذة عرض النص والترجمة مع النسخ المباشر |
| [OcrService.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/OcrService.cs) | Text extraction via `Windows.Media.Ocr` | استخراج النصوص عبر WinRT OCR |
| [TranslationService.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/TranslationService.cs) | Free Google Translate API fetcher | إرسال النص لـ Google Translate واستلام الترجمة |
| [scripts/quick-translate.ps1](file:///c:/Users/moham/Documents/myenv/scripts/quick-translate.ps1) | Background launcher script | سكريبت التشغيل المباشر بالخلفية |

---

## 🛠️ Build & Development Guide / كيفية البناء والتعديل

### 1. Source Code Modifications / التعديل البرمجي:
- **Theme & Styling / تغيير الثيم**: Edit `*.xaml` files (uses sharp dark theme).
- **Target Language / لغة الترجمة**: Edit `targetLang = "ar"` in [TranslationService.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/TranslationService.cs).

### 2. Rebuild Commands / أوامر إعادة التجميع:
From the project directory:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\quick-translate"
dotnet build -c Release
```
Or publish directly to binary output path:
```powershell
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\quick-translate"
```
