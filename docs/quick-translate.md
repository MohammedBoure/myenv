# 🔠 QuickTranslate Documentation / دليل أداة الترجمة الفورية

أداة تظليل وترجمة فورية للشاشة بلغة **C# (WPF)** تعتمد على محرك **WinRT OCR** المدمج في Windows ومحرك **Google Translate API**.

---

## ⚡ أنماط التشغيل والاختصارات

| الاختصار | النمط | آلية العمل |
|---|---|---|
| `Win + Shift + C` / `Alt + Shift + C` | **ترجمة النص المحدد فوراً** | يقوم بعمل نسخ تلقائي (`Ctrl+C`) للنص المظلل في أي تطبيق وفتحه مباشرة في نافذة الترجمة. |
| `Win + Shift + Q` / `Alt + Shift + T` | **ترجمة منطقة من الشاشة (OCR)** | تظليل أي منطقة بالسحب (+) واستخراج النصوص منها عبر OCR لترجمتها. |

---

## 📂 هيكلية المشروع (`tools/quick-translate/`)

| الملف | الوظيفة |
|---|---|
| [App.xaml](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/App.xaml) / [App.xaml.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/App.xaml.cs) | نقطة الانطلاق والتحكم بنمط التطبيق واختصار التشغيل |
| [SelectionWindow.xaml](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) | نافذة تحديد المنطقة على الشاشة بالسحب (+) |
| [ResultWindow.xaml](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/ResultWindow.xaml) | نافذة عرض النص المستخرج والترجمة العربية مع خيار النسخ |
| [OcrService.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/OcrService.cs) | استخراج النصوص عبر `Windows.Media.Ocr` |
| [TranslationService.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/TranslationService.cs) | إرسال النص لـ Google Translate واستلام الترجمة |
| [scripts/quick-translate.ps1](file:///c:/Users/moham/Documents/myenv/scripts/quick-translate.ps1) | سكريبت التشغيل المباشر وإخفاء نافذة الكونسول |

---

## 🛠️ كيفية البناء والتعديل (Build & Modify)

### 1. التعديل البرمجي:
- **تغيير الثيم أو التصميم**: تعديل ملفات `*.xaml` (تعتمد ثيم Catppuccin الداكن).
- **تغيير لغة الترجمة**: تعديل متغير اللغة في [TranslationService.cs](file:///c:/Users/moham/Documents/myenv/tools/quick-translate/TranslationService.cs) (`targetLang = "ar"`).

### 2. أوامر إعادة التجميع (Rebuild):
من داخل المجلد الرئيسي للتطبيق:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\quick-translate"
dotnet build -c Release
```
أو لإصدار ملف تنفيذي موحد:
```powershell
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\tools\quick-translate\bin\Publish"
```
