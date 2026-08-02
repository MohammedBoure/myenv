# ⚡ توثيق بيئة PowerShell

بيئة **PowerShell** مبنية ومخصصة في مشروع `myenv` لتوفر أداءً عالياً وشفافية أنيقة وتجربة كتابة متطورة عبر **PSReadLine**.

---

## 🚀 المميزات الرئيسية لـ PowerShell

1. **البروفايل الموحد (`$PROFILE`)**:
   - يتم تحميل البروفايل تلقائياً عبر الملف الموحد [powershell/profile.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/profile.ps1).
2. **تحسينات PSReadLine**:
   - اقتراحات التنبؤ التاريخي بناءً على الأوامر السابقة (ListView).
   - اختصار `Ctrl+Backspace` لحذف كلمة كاملة للخلف (`BackwardKillWord`).
3. **مظهر Midnight Aurora والشفافية**:
   - خلفية سوداء ناصعة بنسبة شفافية 32% (68% Opacity) مريحة للعين.
4. **أمر `sudo` لرفع الصلاحيات (RunAs Administrator)**:
   - كتابة `sudo` تفتح نافذة PowerShell جديدة بصلاحيات المسؤول في نفس المسار الحالي مباشرة.
   - كتابة `sudo command` تنفذ الأمر المحدد بصلاحيات المسؤول في نفس المسار الحالي.
5. **مسارات أدوات التطوير (PATH Auto-Loader)**:
   - تحميل تلقائي لمسارات الأدوات البرمجية: `dotnet`, `flutter`, `jdk-17`, `Android SDK`, `kotlin`, `msys64`, `php`, `composer`, `nodejs`, `npm`.
6. **ميزة العرض التلقائي عند التنقل (`cd` / `chdir` Auto-LS)**:
   - بمجرد الدخول إلى أي مجلد جديد باستخدام `cd path` (أو العودة `cd ..`), يتم تنفيذ `Get-ChildItem` (أمر `ls`) تلقائياً لعرض المحتويات.

---

## 📁 ملفات التهيأة المخصصة

- **ملف البروفايل الرئيسي**: [powershell/profile.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/profile.ps1)
- **ملف الثيم والـ Prompt**: [powershell/midnight-aurora.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/midnight-aurora.ps1)
- **ملف الشفافية وألوان الكونسول**: [powershell/console-theme.ps1](file:///%USERPROFILE%/Documents/myenv/powershell/console-theme.ps1)

---

## ⌨️ الاختصارات والمفاتيح المفعلة في PowerShell

| الاختصار | الوظيفة |
|---|---|
| `cd` / `chdir` | تغيير المسار مع إظهار قائمة الملفات والمجلدات `ls` تلقائياً في المسار الجديد |
| `docs` | مستكشف التوثيق السريع في التيرمينال عبر أمر `docs` |
| `cb` / `c` | تنفيذ الأمر وعرض النتائج بالشاشة مع نسخ المخرجات تلقائياً للحافظة |
| `| cb` | توجيه مخرجات أي أمر للشاشة وللحافظة في آن واحد |
| `sudo <cmd>` | تنفيذ الأمر بصلاحيات المسؤول بنفس المسار الحالي |
| `Ctrl + Backspace` | حذف الكلمة السابقة بالكامل |
| `Tab` | فتح قائمة الإكمال التلقائي التفاعلية |
| `Ctrl + R` | البحث في سجل الأوامر السابقة عبر fzf / PSReadLine |
| `Alt + Ctrl + Enter` | فتح نافذة PowerShell جديدة عبر GlazeWM |
