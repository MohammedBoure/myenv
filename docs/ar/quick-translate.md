# 🔠 توثيق أداة الترجمة الفورية QuickTranslate

أداة تظليل وترجمة فورية للشاشة بلغة **C# (WPF)** تعتمد على محرك **WinRT OCR** المدمج في Windows ومحرك **Google Translate API**.

---

## ⚡ أنماط التشغيل والاختصارات

| الاختصار | النمط | آلية العمل |
|---|---|---|
| `Win + Shift + C` / `Alt + Shift + C` | **ترجمة النص المحدد فوراً** | يقوم بعمل نسخ تلقائي (`Ctrl+C`) للنص المظلل في أي تطبيق وفتحه مباشرة في نافذة الترجمة (< 15ms). |
| `Win + Shift + Q` / `Alt + Shift + T` | **ترجمة منطقة من الشاشة (OCR)** | تظليل أي منطقة بالسحب (+) واستخراج النصوص منها عبر OCR لترجمتها. |

---

## 📂 هيكلية المشروع (`tools/quick-translate/`)

| الملف | الوظيفة |
|---|---|
| [App.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml) / [App.xaml.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml.cs) | نقطة الانطلاق والتحكم بنمط التطبيق واختصار التشغيل والظهور المباشر |
| [SelectionWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) | نافذة تحديد المنطقة على الشاشة بالسحب (+) |
| [ResultWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/ResultWindow.xaml) | نافذة عرض النص المستخرج والترجمة العربية مع خيار النسخ والتعديل المباشر |
| [OcrService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/OcrService.cs) | استخراج النصوص عبر `Windows.Media.Ocr` |
| [TranslationService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TranslationService.cs) | إرسال النص لـ Google Translate واستلام الترجمة |
| [scripts/quick-translate.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/quick-translate.ps1) | سكريبت التشغيل المباشر بالخلفية وإخفاء نافذة الكونسول |

---

## 🛠️ كيفية البناء والتعديل (Build & Modify)

### 1. التعديل البرمجي:
- **تغيير الثيم أو التصميم**: تعديل ملفات `*.xaml` (تعتمد ثيم Catppuccin الداكن).
- **تغيير لغة الترجمة**: تعديل متغير اللغة في [TranslationService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TranslationService.cs) (`targetLang = "ar"`).

### 2. أوامر إعادة التجميع (Rebuild):
من داخل المجلد الرئيسي للتطبيق:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\quick-translate"
dotnet build -c Release
```
أو لإصدار ملف تنفيذي موحد للمشروع:
```powershell
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\quick-translate"
```
