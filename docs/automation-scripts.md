# ⚙️ Automation Scripts Documentation / توثيق أدوات الأتمتة

PowerShell & batch scripts automating environment setup, registry settings, tool restoration, and hotkey actions.
تحتوي بيئة `myenv` على مجموعة سكريبتات PowerShell مخصصة لأتمتة ربط الإعدادات وتثبيت الأدوات وإعداد سجل النظام (Registry).

---

## 📜 Available Scripts / السكريبتات المتاحة في مجلد `scripts/`

| Script / السكريبت | Path / المسار | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|---|
| **`setup-all.ps1`** | [scripts/setup-all.ps1](file:///c:/Users/moham/Documents/myenv/scripts/setup-all.ps1) | **Master Setup Script**: Applies directory junctions, taskbar autohide, PSReadLine, Clink, Winget packages, CMD AutoRun, and Alt+Shift disable | **السكريبت الشامل**؛ إنشاء الـ Junctions، إخفاء شريط المهام، إعداد PSReadLine، Clink، Winget، و CMD AutoRun |
| **`docs.ps1`** | [scripts/docs.ps1](file:///c:/Users/moham/Documents/myenv/scripts/docs.ps1) | **CLI Documentation Navigator**: Interactive shortcut and documentation helper command `docs` | **مستكشف التوثيق التفاعلي السريع** في التيرمينال عبر أمر `docs` |
| **`install-packages.ps1`** | [scripts/install-packages.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-packages.ps1) | Restores/installs developer packages from `winget-packages.json` | استعادة وتثبيت حزم وأدوات التطوير عبر Winget |
| **`app-launcher.ps1`** | [scripts/app-launcher.ps1](file:///c:/Users/moham/Documents/myenv/scripts/app-launcher.ps1) | Centered WPF app launcher dialog (`Alt + Q`) | مشغل التطبيقات السريع (`Alt + Q`) المكتوب بـ WPF |
| **`install-clink.ps1`** | [scripts/install-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-clink.ps1) | Installs Clink via WinGet & configures CMD AutoRun | فحص وتثبيت أداة Clink وتفعيل AutoRun |
| **`set-cmd-autocompletion.ps1`** | [scripts/set-cmd-autocompletion.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-cmd-autocompletion.ps1) | Configures CMD Tab completion & AutoRun script | تفعيل الإكمال بنقر `Tab` لـ CMD وتمرير `cmd-init.cmd` |
| **`set-taskbar-autohide.ps1`** | [scripts/set-taskbar-autohide.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-taskbar-autohide.ps1) | Toggles Windows Taskbar autohide in Registry & restarts Explorer | تفعيل الإخفاء التلقائي لشريط المهام وإعادة تشغيل Explorer |
| **`set-ctrl-backspace.ps1`** | [scripts/set-ctrl-backspace.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-ctrl-backspace.ps1) | Binds `Ctrl+Backspace` for PSReadLine word deletion | ربط اختصار `Ctrl+Backspace` في PSReadLine |
| **`disable-alt-shift-lang.ps1`** | [scripts/disable-alt-shift-lang.ps1](file:///c:/Users/moham/Documents/myenv/scripts/disable-alt-shift-lang.ps1) | Disables `Alt+Shift` key toggle, keeping `Win+Space` as primary | تعطيل `Alt+Shift` وإبقاء `Win+Space` الاختصار الوحيد للغة |
| **`focused-window-border.ps1`** | [scripts/focused-window-border.ps1](file:///c:/Users/moham/Documents/myenv/scripts/focused-window-border.ps1) | Win32/WPF active window focus border overlay | رسم إطار أبيض ناصع حول النافذة المحددة |
| **`smart-tiling-direction.ps1`** | [scripts/smart-tiling-direction.ps1](file:///c:/Users/moham/Documents/myenv/scripts/smart-tiling-direction.ps1) | Listens to GlazeWM events & auto-sets tiling split direction | تحديد اتجاه التقسيم تلقائياً (أفقي/عمودي) حسب أبعاد النافذة |
| **`OpenTerminalHere.cs` / `.exe`** | [scripts/OpenTerminalHere.cs](file:///c:/Users/moham/Documents/myenv/scripts/OpenTerminalHere.cs) | Instant C# launcher to open CMD/PowerShell at active Explorer path (<10ms) | فتح CMD أو PowerShell فوراً في مسار متصفح الملفات Explorer |
| **`open-terminal-here.ps1`** | [scripts/open-terminal-here.ps1](file:///c:/Users/moham/Documents/myenv/scripts/open-terminal-here.ps1) | PowerShell helper script for opening terminal at active Explorer folder | سكريبت PowerShell المساعد لفتح التيرمينال في مسار Explorer |
| **`quick-translate.ps1`** | [scripts/quick-translate.ps1](file:///c:/Users/moham/Documents/myenv/scripts/quick-translate.ps1) | Background launcher for QuickTranslate OCR & Selection tool | سكريبت تشغيل مشغل الترجمة الفورية بالخلفية |
| **`download-clink.ps1`** | [scripts/download-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/download-clink.ps1) | Downloads & installs portable Clink zip release if WinGet fails | تحميل نسخة محمولة من Clink من GitHub وتثبيتها |
| **`toggle-window-transparency.ps1`** | [scripts/toggle-window-transparency.ps1](file:///c:/Users/moham/Documents/myenv/scripts/toggle-window-transparency.ps1) | Toggles active window transparency 80% / 100% (`Alt+Shift+Z`) | تبديل شفافية النافذة النشطة بين 80% و 100% (`Alt+Shift+Z`) |
| **`capture-screenshot.ps1`** | [scripts/capture-screenshot.ps1](file:///c:/Users/moham/Documents/myenv/scripts/capture-screenshot.ps1) | Direct full screenshot capture to file & Clipboard (`Alt+Shift+S`) | التقاط الشاشة المباشر والحفظ في الملفات والحافظة (`Alt+Shift+S`) |
| **`toggle-mute.ps1`** | [scripts/toggle-mute.ps1](file:///c:/Users/moham/Documents/myenv/scripts/toggle-mute.ps1) | Master audio mute toggle via Win32 API (`Alt+Shift+M`) | كتم/تفعيل صوت الجهاز المباشر عبر Win32 API (`Alt+Shift+M`) |
| **`set-windows10-border.ps1`** | [scripts/set-windows10-border.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-windows10-border.ps1) | DWM registry border color configurator | ضبط إعدادات سجل النظام DWM لإطار النافذة النشطة باللون الأبيض |
| **`sudo.cmd`** | [scripts/sudo.cmd](file:///c:/Users/moham/Documents/myenv/scripts/sudo.cmd) | Command elevation helper for CMD | تشغيل الأوامر بصلاحيات المسؤول (Administrator) في CMD |
| **`cb.cmd`** | [scripts/cb.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cb.cmd) | Output display & clipboard copy wrapper script | تنفيذ الأوامر مع إخراج النتيجة للتيرمينال ونسخها للحافظة |

---

## 🛠️ Master Environment Setup / تشغيل الأتمتة الشاملة

To run master setup at any time:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-all.ps1"
```
