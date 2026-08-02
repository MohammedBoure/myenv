# 💻 توثيق بيئة Command Prompt (CMD)

بيئة **CMD** متطورة ومخصصة داخل نظام `myenv` تجمع بين اختصارات **Doskey** وإكمال **Clink** والتنبؤ التفاعلي للسجل.

---

## 🚀 المميزات الرئيسية لـ CMD

1. **التنقل الطبيعي وتخصيص سجل الأوامر**:
   - **`→` / `←`**: تحرك طبيعي حرف بحرف داخل السطر لتعديل الأخطاء.
   - **`↑` / `↓`**: التنقل في سجل الأوامر السابقة المطابقة لما كتبته.
   - **`Tab`**: الإكمال التلقائي للملفات والمجلدات.
   - **ثيم Obsidian Dark**: قوائم إكمال داكنة وأنيقة تتناسب مع مظهر النظام.
2. **شريط الأوامر الملون**:
   - يعرض الوقت الحالي، اسم المستخدم، اسم الجهاز، والمسار الحالي بالألوان.
3. **التشغيل التلقائي (AutoRun)**:
   - مسجل تلقائياً عبر [scripts/cmd-init.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cmd-init.cmd).

---

## ⚡ الاختصارات المتاحة (Doskey Command Aliases)

| الاختصار | الأمر المنفذ | الوصف |
|---|---|---|
| `cd` / `chdir` | `cd /d <path> & ls` | الانتقال للمجلد الجديد وتطبيق أمر `ls` تلقائياً لعرض محتوياته |
| `ls` | `dir /b` | عرض قائمة الملفات بشكل مختصر |
| `ll` | `dir` | عرض التفاصيل الكاملة للملفات والمجلدات |
| `la` | `dir /a` | عرض جميع الملفات بما فيها الملفات المخفية والسيستم |
| `clear` | `cls` | مسح شاشة الـ CMD |
| `croot` | `cd /d "%USERPROFILE%" & ls` | الانتقال المباشر لمجلد المستخدم الرئيسي وثم تطبيق `ls` |
| `docs` | `docs [wm|translate|cmd|ps|scripts]` | مستكشف التوثيق السريع في التيرمينال |
| `gs` | `git status` | عرض حالة مستودع Git |
| `ga` | `git add` | إضافة الملفات للتجهيز في Git |
| `gc` | `git commit -m` | عمل Commit في Git مع إضافة الرسالة مباشرة |
| `gp` | `git push` | رفع التغييرات إلى السيرفر البعيد |
| `gl` | `git log -n 10` | عرض أحدث 10 سجلات Commit في Git |
| `sudo` | `RunAs Administrator` | تشغيل CMD كمسؤول أو تنفيذ أمر بصلحيات الأدمن |
| `cb` / `c` | `cb.cmd <command>` | تنفيذ أي أمر وعرض مخرجاته بالشاشة مع نسخها تلقائياً للحافظة |

---

## ⌨️ نظام التحكم بالملاحة المفصل (Clink Navigation)

| الاختصار | نوع التحكم والوظيفة |
|---|---|
| `→` / `←` | تحريك المؤشر وتعديل الكلمات |
| `↑` / `↓` | التنقل بين الأوامر السابقة المطابقة للبداية المكتوبة |
| `Tab` | فتح قائمة إكمال الملفات والمجلدات |
| `Ctrl + Space` / `F7` | فتح قائمة منبثقة تفاعلية بسجل الأوامر التاريخية كاملة |
| `Ctrl + L` | مسح الشاشة فوراً دون مسح السجل |
| `Alt + Enter` | فتح نافذة CMD جديدة عبر GlazeWM |

---

## 🛠️ الملفات المخصصة لـ CMD

- **ملف التهيأة والأوامر المختصرة**: [scripts/cmd-init.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cmd-init.cmd)
- **ملف إعدادات Clink**: [clink/clink_settings](file:///%USERPROFILE%/Documents/myenv/clink/clink_settings)
- **سكريبت ربط CMD بالـ Registry**: [scripts/set-cmd-autocompletion.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-cmd-autocompletion.ps1)
- **سكريبت تثبيت وتفعيل Clink**: [scripts/install-clink.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-clink.ps1)
