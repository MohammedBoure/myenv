# ⚡ PowerShell Environment Documentation / توثيق بيئة PowerShell

High-performance, customized **PowerShell** environment featuring **PSReadLine**, Midnight Aurora theme, and **32% Console Transparency**.
بيئة **PowerShell** مبنية ومخصصة لتوفر أداءً عالياً وشفافية أنيقة وتجربة كتابة متطورة عبر **PSReadLine**.

---

## 🚀 Key Features / المميزات الرئيسية لـ PowerShell

1. **Unified Profile (`$PROFILE`) / البروفايل الموحد**:
   - Automatically loaded via single source of truth [powershell/profile.ps1](file:///c:/Users/moham/Documents/myenv/powershell/profile.ps1).
2. **PSReadLine Enhancements / تحسينات PSReadLine**:
   - `ListView` history auto-suggestions & predictions.
   - `Ctrl+Backspace` for backward word deletion (`BackwardKillWord`).
3. **Midnight Aurora Theme & Transparency / الشفافية والثيم**:
   - True black background with **32% Transparency** (68% Opacity).
4. **Elevated `sudo` Command / أمر رفع الصلاحيات**:
   - `sudo`: Opens new elevated PowerShell window at current directory.
   - `sudo command`: Executes command with Administrator privileges in current directory.
5. **Dev Tools PATH Auto-Loader / تحميل مسارات الأدوات البرمجية**:
   - Auto-loads `dotnet`, `flutter`, `jdk-17`, `Android SDK`, `kotlin`, `msys64`, `php`, `composer`, `nodejs`, `npm`.
6. **Auto-LS Navigation (`cd` / `chdir`) / العرض التلقائي لـ `cd`**:
   - Executing `cd path` automatically runs `Get-ChildItem` to show directory contents.

---

## 📁 Configuration Files / ملفات التهيأة المخصصة

- **Main Profile File / البروفايل الرئيسي**: [powershell/profile.ps1](file:///c:/Users/moham/Documents/myenv/powershell/profile.ps1)
- **Theme & Prompt File / الثيم والـ Prompt**: [powershell/midnight-aurora.ps1](file:///c:/Users/moham/Documents/myenv/powershell/midnight-aurora.ps1)
- **Console Transparency File / ملف الشفافية**: [powershell/console-theme.ps1](file:///c:/Users/moham/Documents/myenv/powershell/console-theme.ps1)

---

## ⌨️ Shortcuts & Hotkeys / الاختصارات والمفاتيح المفعلة

| Shortcut / الاختصار | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|
| `cd` / `chdir` | Navigate to path & auto-list files (`ls`) | تغيير المسار مع إظهار قائمة الملفات تلقائياً |
| `docs` | Terminal Documentation Navigator CLI | مستكشف التوثيق السريع في التيرمينال |
| `cb` / `c` | Run command & copy output to Clipboard | تنفيذ الأمر وعرض النتائج مع نسسخها للحافظة |
| `| cb` | Pipe output to screen and Clipboard | توجيه مخرجات أي أمر للشاشة وللحافظة معاً |
| `sudo <cmd>` | Run command as Administrator in current path | تنفيذ الأمر بصلاحيات المسؤول بنفس المسار |
| `Ctrl + Backspace` | Delete word backward | حذف الكلمة السابقة بالكامل |
| `Tab` | Interactive menu completion | فتح قائمة الإكمال التلقائي التفاعلية |
| `Ctrl + R` | Interactive history search (fzf / PSReadLine) | البحث التفاعلي في سجل الأوامر السابقة |
| `Alt + Ctrl + Enter` | Open new PowerShell window via GlazeWM | فتح نافذة PowerShell جديدة عبر GlazeWM |
