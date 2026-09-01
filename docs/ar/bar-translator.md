# 🌐 المترجم الفوري لشريط الحالة العلوي (`BarTranslator`)

أداة خلفية فائقة السرعة لمراقبة النصوص المحددة وترجمتها فورياً من الإنجليزية إلى العربية وعرضها مباشرة في الشريط العلوي (**YASB** أو **Zebar**).

---

## ⚡ المميزات الرئيسية

- **التقاط فوري للنص المحدد (< 75ms)**: بمجرد تحديد أي نص بالماوس (سحب بالزر الأيسر أو النقر المزدوج على أي كلمة) في أي تطبيق (المتصفح، محرر الأكواد، الطرفية، ملفات PDF)، يتم استخراج النص وترجمته تلقائياً دون الحاجة لأي اختصارات لوحة مفاتيح.
- **اختصار ذكي للنصوص الطويلة**:
  - يظهر في الشريط العلوي افتراضياً الجزء الأول / الكلمة الأولى فقط لتجنب تشويه مظهر الشريط (مثال: `🌐 ترجمة…`).
  - عند النقر على مكان العرض في الشريط، يتبدل النص فوراً ليظهر النص المترجم كاملاً.
  - عند تمرير مؤشر الماوس فوق الأداة، تظهر نافذة تلميح (Tooltip) تعرض النص الإنجليزي الأصلي والترجمة العربية الكاملة.
- **محرك ترجمة عالي السرعة**: يعتمد على واجهة Google Translate السريعة مع ذاكرة تخزين مؤقت (In-Memory Cache) للترجمة في 0ms للكلمات المكررة.
- **التكامل التام مع YASB و Zebar**:
  - في **YASB**: مسجل كودجت `CustomWidget` فائق السرعة عبر `get-state.exe`، ويختفي تلقائياً عند عدم وجود نص مترجم (`hide_empty: true`).
  - في **Zebar**: مدمج داخل حزمة الودجات عبر خادم محلي خفيف على المنفذ `49876`.

---

## 🖱️ التحكم عبر الماوس في الشريط العلوي

| الحركة | الوظيفة |
|---|---|
| **النقر بالزر الأيسر (Left Click)** | التبديل بين العرض المختصر (الكلمة الأولى) والنص الكامل. |
| **النقر بالزر الأيمن (Right Click)** | نسخ النص المترجم بالكامل إلى الحافظة فوراً. |
| **النقر بالزر الأوسط (Middle Click)** | مسح الترجمة الحالية وإخفاء الودجت من الشريط. |

---

## 📂 بنية الملفات والمسارات

| المكون | المسار | الوصف |
|---|---|---|
| **الكود المصدري** | [`tools/bar-translator/`](file:///%USERPROFILE%/Documents/myenv/tools/bar-translator) | مشروع C# (.NET 10) مع خطاف الماوس المنخفض `WH_MOUSE_LL`. |
| **الملف التنفيذي** | [`scripts/bar-translator/BarTranslator.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/BarTranslator.exe) | المعالج الخلفي المستقل الذي يعمل بصمت. |
| **قارئ الحالة الفوري** | [`scripts/bar-translator/get-state.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/get-state.exe) | قارئ حالة فائق السرعة (< 5ms) يستدعيه YASB. |
| **أداة الإجراءات** | [`scripts/bar-translator/translator-action.exe`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/translator-action.exe) | أداة تنفيذ عمليات النسخ والمسح دون وميض نوافذ CMD. |
| **ملف الحالة المشترك** | [`scripts/bar-translator/state.json`](file:///%USERPROFILE%/Documents/myenv/scripts/bar-translator/state.json) | ملف الحالة الذي تقرأ منه أشرطة المهام. |

---

## 🛠️ دليل إعادة التجميع (Build Guide)

```powershell
cd "$env:USERPROFILE\Documents\myenv\tools\bar-translator"
dotnet publish -c Release -o "$env:USERPROFILE\Documents\myenv\scripts\bar-translator"
```
