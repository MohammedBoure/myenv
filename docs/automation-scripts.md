# ⚙️ توثيق أدوات الأتمتة (Automation Scripts)

تحتوي بيئة `myenv` على مجموعة سكريبتات PowerShell مخصصة لأتمتة ربط الإعدادات وتثبيت الأدوات وإعداد سجل النظام (Registry).

---

## 📜 السكريبتات المتاحة في مجلد `scripts/`

| السكريبت | المسار | الوظيفة والدور |
|---|---|---|
| **`setup-all.ps1`** | [scripts/setup-all.ps1](file:///c:/Users/moham/Documents/myenv/scripts/setup-all.ps1) | **السكريبت الشامل**؛ يقوم بإنشاء الـ Junctions، تفعيل إخفاء شريط المهام تلقائياً، إعداد PSReadLine، تثبيت وتفعيل Clink، ربط CMD AutoRun، وتعطيل اختصار `Alt+Shift` اللغوي. |
| **`app-launcher.ps1`** | [scripts/app-launcher.ps1](file:///c:/Users/moham/Documents/myenv/scripts/app-launcher.ps1) | مشغل التطبيقات السريع (`Alt + Q`) المكتوب بـ WPF مع واجهة بحث سريعة. |
| **`install-clink.ps1`** | [scripts/install-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-clink.ps1) | فحص وتثبيت أداة Clink عبر WinGet وتنشيط الخيارات وتفعيل AutoRun. |
| **`set-cmd-autocompletion.ps1`** | [scripts/set-cmd-autocompletion.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-cmd-autocompletion.ps1) | تفعيل الإكمال بنقر `Tab` لـ CMD وتمرير [cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd) لسجل النظام. |
| **`set-taskbar-autohide.ps1`** | [scripts/set-taskbar-autohide.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-taskbar-autohide.ps1) | تفعيل الإخفاء التلقائي لشريط مهام Windows وإعادة تشغيل Explorer. |
| **`set-ctrl-backspace.ps1`** | [scripts/set-ctrl-backspace.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-ctrl-backspace.ps1) | ربط اختصار `Ctrl+Backspace` في PSReadLine لـ PowerShell. |
| **`disable-alt-shift-lang.ps1`** | [scripts/disable-alt-shift-lang.ps1](file:///c:/Users/moham/Documents/myenv/scripts/disable-alt-shift-lang.ps1) | تعطيل اختصار `Alt+Shift` لتغيير اللغة وإبقاء `Win+Space` الاختصار الوحيد لمنع التغيير العشوائي أثناء البرمجة. |

---

## 🛠️ تشغيل الأتمتة الشاملة

لتطبيق أو إعادة أتمتة النظام بالكامل في أي وقت:
```powershell
powershell -ExecutionPolicy Bypass -File "C:\Users\moham\Documents\myenv\scripts\setup-all.ps1"
```
