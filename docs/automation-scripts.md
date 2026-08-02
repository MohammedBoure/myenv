# ⚙️ توثيق أدوات الأتمتة (Automation Scripts)

تحتوي بيئة `myenv` على مجموعة سكريبتات PowerShell مخصصة لأتمتة ربط الإعدادات وتثبيت الأدوات وإعداد سجل النظام (Registry).

---

## 📜 السكريبتات المتاحة في مجلد `scripts/`

| السكريبت | المسار | الوظيفة والدور |
|---|---|---|
| **`setup-all.ps1`** | [scripts/setup-all.ps1](file:///c:/Users/moham/Documents/myenv/scripts/setup-all.ps1) | **السكريبت الشامل**؛ يقوم بإنشاء الـ Junctions، تفعيل إخفاء شريط المهام تلقائياً، إعداد PSReadLine، تثبيت وتفعيل Clink، استعادة حزم Winget، ربط CMD AutoRun، وتعطيل اختصار `Alt+Shift` اللغوي. |
| **`install-packages.ps1`** | [scripts/install-packages.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-packages.ps1) | استعادة وتثبيت جميع حزم وأدوات التطوير المسجلة في `winget-packages.json` تلقائياً عبر Winget. |
| **`app-launcher.ps1`** | [scripts/app-launcher.ps1](file:///c:/Users/moham/Documents/myenv/scripts/app-launcher.ps1) | مشغل التطبيقات السريع (`Alt + Q`) المكتوب بـ WPF مع واجهة بحث سريعة. |
| **`install-clink.ps1`** | [scripts/install-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-clink.ps1) | فحص وتثبيت أداة Clink عبر WinGet وتنشيط الخيارات وتفعيل AutoRun. |
| **`set-cmd-autocompletion.ps1`** | [scripts/set-cmd-autocompletion.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-cmd-autocompletion.ps1) | تفعيل الإكمال بنقر `Tab` لـ CMD وتمرير [cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd) لسجل النظام. |
| **`set-taskbar-autohide.ps1`** | [scripts/set-taskbar-autohide.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-taskbar-autohide.ps1) | تفعيل الإخفاء التلقائي لشريط مهام Windows وإعادة تشغيل Explorer. |
| **`set-ctrl-backspace.ps1`** | [scripts/set-ctrl-backspace.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-ctrl-backspace.ps1) | ربط اختصار `Ctrl+Backspace` في PSReadLine لـ PowerShell. |
| **`disable-alt-shift-lang.ps1`** | [scripts/disable-alt-shift-lang.ps1](file:///c:/Users/moham/Documents/myenv/scripts/disable-alt-shift-lang.ps1) | تعطيل اختصار `Alt+Shift` لتغيير اللغة وإبقاء `Win+Space` الاختصار الوحيد لمنع التغيير العشوائي أثناء البرمجة. |
| **`focused-window-border.ps1`** | [scripts/focused-window-border.ps1](file:///c:/Users/moham/Documents/myenv/scripts/focused-window-border.ps1) | سكريبت PowerShell خفيف يعتمد على Win32/WPF لرسم إطار أبيض ناصع وتفاعلي (100% Click-through) حول النافذة المحددة في Windows 10. |
| **`smart-tiling-direction.ps1`** | [scripts/smart-tiling-direction.ps1](file:///c:/Users/moham/Documents/myenv/scripts/smart-tiling-direction.ps1) | الاستماع لأحداث GlazeWM وتحديد اتجاه التقسيم تلقائياً (أفقي/عمودي) حسب أبعاد النافذة المحددة. |
| **`OpenTerminalHere.cs` / `.exe`** | [scripts/OpenTerminalHere.cs](file:///c:/Users/moham/Documents/myenv/scripts/OpenTerminalHere.cs) | كود مصدري ومشغل C# فوري لتحديد مسار File Explorer المفتوح وفتح CMD أو PowerShell فيه بسرعة فائقة (<10ms). |
| **`open-terminal-here.ps1`** | [scripts/open-terminal-here.ps1](file:///c:/Users/moham/Documents/myenv/scripts/open-terminal-here.ps1) | سكريبت PowerShell المساعد لفتح التيرمينال في المسار الحالي لمتصفح الملفات Explorer. |
| **`quick-translate.ps1`** | [scripts/quick-translate.ps1](file:///c:/Users/moham/Documents/myenv/scripts/quick-translate.ps1) | سكريبت تشغيل مشغل الترجمة الفورية بالخلفية وإخفاء نافذة الأوامر. |
| **`download-clink.ps1`** | [scripts/download-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/download-clink.ps1) | تحميل أحدث نسخة محمولة من Clink من GitHub وتثبيتها في حال تعذر WinGet. |
| **`toggle-window-transparency.ps1`** | [scripts/toggle-window-transparency.ps1](file:///c:/Users/moham/Documents/myenv/scripts/toggle-window-transparency.ps1) | سكريبت التبديل الفوري لشفافية النافذة النشطة بين 80% و 100% (`Alt+Shift+Z`). |
| **`capture-screenshot.ps1`** | [scripts/capture-screenshot.ps1](file:///c:/Users/moham/Documents/myenv/scripts/capture-screenshot.ps1) | سكريبت التقاط الشاشة المباشر والحفظ التلقائي في `Pictures/Screenshots` والنسخ للحافظة (`Alt+Shift+S`). |
| **`toggle-mute.ps1`** | [scripts/toggle-mute.ps1](file:///c:/Users/moham/Documents/myenv/scripts/toggle-mute.ps1) | سكريبت كتم/تفعيل صوت الجهاز المباشر عبر Win32 API (`Alt+Shift+M`). |
| **`set-windows10-border.ps1`** | [scripts/set-windows10-border.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-windows10-border.ps1) | سكريبت ضبط إعدادات سجل النظام DWM لإطار النافذة النشطة باللون الأبيض في Windows 10. |
| **`sudo.cmd`** | [scripts/sudo.cmd](file:///c:/Users/moham/Documents/myenv/scripts/sudo.cmd) | أداة تشغيل الأوامر بصلحيات المسؤول (Administrator) مباشرة في CMD. |
| **`cb.cmd`** | [scripts/cb.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cb.cmd) | أداة تنفيذ أي أمر مع إخراج النتيجة في التيرمينال ونسخها مباشرة للحافظة Clipboard. |

---

## 🛠️ تشغيل الأتمتة الشاملة

لتطبيق أو إعادة أتمتة النظام بالكامل في أي وقت:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-all.ps1"
```
