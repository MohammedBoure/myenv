# 🔠 توثيق أداة الترجمة الفورية QuickTranslate

أداة ترجمة فورية وكتابة ولصق مباشر للشاشة بلغة **C# (WPF)** تعتمد على محرك **WinRT OCR** المدمج في Windows ومحرك **Google Translate API** فائق السرعة.

---

## ⚡ أنماط التشغيل والاختصارات

| الاختصار | النمط | آلية العمل |
|---|---|---|
| `Win + Shift + X` / `Alt + Shift + E` | **اكتب وترجم وطبق فوراً (Type & Paste)** | يفتح نافذة حوارية عائمة سريعة للكتابة المباشرة، مع ترجمة فورية أثناء الكتابة. عند ضغط `Enter` يتم إغلاق النافذة وتطبيق النص المترجم ولصقه مباشرة في التطبيق السابق. |
| `Win + Shift + C` / `Alt + Shift + C` | **ترجمة النص المحدد فوراً (Selection)** | يقوم بعمل نسخ تلقائي (`Ctrl+C`) للنص المظلل في أي تطبيق وفتحه مباشرة في نافذة الترجمة (< 15ms). |
| `Win + Shift + Q` / `Alt + Shift + T` | **ترجمة منطقة من الشاشة (OCR)** | تظليل أي منطقة بالسحب (+) واستخراج النصوص منها عبر WinRT OCR وترجمتها فوراً. |

---

## ⌨️ اختصارات نافذة الكتابة والترجمة (`TypeTranslateWindow`)

- `Enter`: تطبيق ولصق النص المترجم في البرنامج النشط فوراً.
- `Tab`: التبديل بين اللغات (تلقائي AR ⇄ EN، AR ➔ EN، EN ➔ AR، AR ➔ FR، AR ➔ DE، AR ➔ ES، AR ➔ TR).
- `Shift + Enter`: إضافة سطر جديد داخل حقل الإدخال.
- `Esc`: إغلاق النافذة دون إجراء أي لصق أو تعديل.

---

## 📂 هيكلية المشروع (`tools/quick-translate/`)

| الملف | الوظيفة |
|---|---|
| [App.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml) / [App.xaml.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/App.xaml.cs) | نقطة الانطلاق والتقاط مقبض النافذة النشطة وتوجيه الأنماط (`--type`, `--clipboard`, OCR) |
| [TypeTranslateWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TypeTranslateWindow.xaml) / [.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TypeTranslateWindow.xaml.cs) | نافذة إدخال النصوص والترجمة الفورية أثناء الكتابة واللصق المباشر عند ضغط Enter |
| [SelectionWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/SelectionWindow.xaml) | نافذة تحديد المنطقة على الشاشة بالسحب (+) |
| [ResultWindow.xaml](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/ResultWindow.xaml) | نافذة عرض النص المستخرج والترجمة العربية مع خيار النسخ والتعديل المباشر |
| [OcrService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/OcrService.cs) | استخراج النصوص عبر `Windows.Media.Ocr` |
| [TranslationService.cs](file:///%USERPROFILE%/Documents/myenv/tools/quick-translate/TranslationService.cs) | إرسال النص لـ Google Translate واستلام الترجمة مع كاش ذاكرة فائق السرعة |
| [scripts/quick-translate.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/quick-translate.ps1) | سكريبت التشغيل المباشر بالخلفية وإخفاء نافذة الكونسول |

---

## 🛠️ كيفية البناء والتعديل (Build & Modify)

### 1. أوامر إعادة التجميع والنشر (Rebuild & Publish):
من داخل المجلد الرئيسي للتطبيق:
```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\quick-translate"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\quick-translate"
```
