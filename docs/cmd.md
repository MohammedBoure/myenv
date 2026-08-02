# 💻 CMD Environment Documentation / توثيق بيئة Command Prompt

An enhanced **Command Prompt (CMD)** environment using **Doskey aliases** and **Clink** auto-suggestions.
بيئة **CMD** متطورة ومخصصة داخل نظام `myenv` تجمع بين اختصارات **Doskey** وإكمال **Clink**.

---

## 🚀 Key Features / المميزات الرئيسية لـ CMD

1. **Natural Text Editing & History Search / إعدادات التنقل والسجل**:
   - **`→` / `←`**: Move cursor character by character / تحرك طبيعي داخل السطر.
   - **`↑` / `↓`**: History search matching typed prefix / التنقل بين الأوامر المطابقة للمكتوب.
   - **`Tab`**: Auto-complete files & directories / الإكمال التلقائي للملفات والمجلدات.
   - **Obsidian Dark Popup**: Dark themed completion menus / قوائم إكمال داكنة وأنيقة.
2. **Colored Prompt / شريط الأوامر الملون**:
   - Displays timestamp, username, host, and path in colors / يعرض الوقت واسم الجهاز والمسار بالألوان.
3. **AutoRun Integration / التشغيل التلقائي**:
   - Registered in Registry AutoRun via [scripts/cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd).

---

## ⚡ Doskey Command Aliases / الاختصارات المتاحة

| Alias / الاختصار | Executed Command / الأمر المنفذ | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|---|
| `cd` / `chdir` | `cd /d <path> & ls` | Change directory and auto-list files (Auto-LS) | الانتقال للمجلد الجديد وتطبيق أمر `ls` تلقائياً |
| `ls` | `dir /b` | Brief file listing | عرض قائمة الملفات بشكل مختصر |
| `ll` | `dir` | Detailed file listing | عرض التفاصيل الكاملة للملفات والمجلدات |
| `la` | `dir /a` | List all files including hidden | عرض جميع الملفات بما فيها المخفية |
| `clear` | `cls` | Clear console screen | مسح شاشة الـ CMD |
| `croot` | `cd /d "%USERPROFILE%" & ls` | Jump to Home directory + Auto-LS | الانتقال المباشر لمجلد المستخدم الرئيسي |
| `docs` | `docs [wm|translate|cmd|ps]` | Terminal documentation navigator | مستكشف التوثيق السريع في التيرمينال |
| `gs` | `git status` | Git status | عرض حالة مستودع Git |
| `ga` | `git add` | Git add | إضافة الملفات للتجهيز في Git |
| `gc` | `git commit -m` | Git commit with message | عمل Commit في Git مع إضافة الرسالة مباشرة |
| `gp` | `git push` | Git push | رفع التغييرات إلى السيرفر البعيد |
| `gl` | `git log -n 10` | Show latest 10 git commits | عرض أحدث 10 سجلات Commit في Git |
| `sudo` | `RunAs Administrator` | Run CMD or command as Admin | تشغيل CMD كمسؤول أو تنفيذ أمر بصلحيات الأدمن |
| `cb` / `c` | `cb.cmd <command>` | Run command & copy output to Clipboard | تنفيذ أي أمر وتوجيه النتيجة للتيرمينال والحافظة |

---

## ⌨️ Clink & Hotkey Navigation / نظام التحكم بالملاحة

| Shortcut / الاختصار | Function / الوظيفة (English) | الوظيفة (العربية) |
|---|---|---|
| `→` / `←` | Move cursor character by character | تحريك المؤشر وتعديل الكلمات |
| `↑` / `↓` | History search matching typed text | التنقل بين الأوامر السابقة المطابقة |
| `Tab` | Popup file/folder completion | قائمة إكمال الملفات والمجلدات |
| `Ctrl + Space` / `F7` | Interactive history search popup | قائمة منبثقة تفاعلية بسجل الأوامر |
| `Ctrl + L` | Clear screen buffer | مسح الشاشة فوراً دون مسح السجل |
| `Alt + Enter` | Open new CMD window | فتح نافذة CMD جديدة عبر GlazeWM |

---

## 🛠️ Associated Files / الملفات المخصصة لـ CMD

- **CMD Init Script / ملف التهيأة**: [scripts/cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd)
- **Clink Settings / إعدادات Clink**: [clink/clink_settings](file:///c:/Users/moham/Documents/myenv/clink/clink_settings)
- **Registry Script / سكريبت سجل النظام**: [scripts/set-cmd-autocompletion.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-cmd-autocompletion.ps1)
- **Clink Installer / سكريبت تثبيت Clink**: [scripts/install-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-clink.ps1)
