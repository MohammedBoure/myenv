# 💻 توثيق بيئة Command Prompt (CMD)

بيئة **CMD** متطورة ومخصصة داخل نظام `myenv` لتوفر تجربة كتابة أوامر سريعة وذكية تشبه التيرمينال في أنظمة Linux، بفضل الدمج بين اختصارات **Doskey** وأداة **Clink**.

---

## 🚀 المميزات الرئيسية لـ CMD

1. **الإكمال التلقائي الذكي والاقتراحات السابقة (Auto-Suggestions)**:
   - تعرض لك الأداة اقتراحات بالأوامر السابقة بلون رمادي داكن أثناء الكتابة.
   - تضغط على **السهم الأيمن `→`** لقبول الاقتراح بالكامل واستكماله.
2. **شريط الأوامر الملون (Colored Prompt)**:
   - يعرض الوقت الحالي، اسم المستخدم، اسم الجهاز، والمسار الحالي بالألوان.
3. **التشغيل التلقائي (AutoRun)**:
   - يتم تسجيل سكريبت [cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd) تلقائياً في سجل النظام (`HKCU:\Software\Microsoft\Command Processor\AutoRun`).

---

## ⚡ الاختصارات المتاحة (Doskey Command Aliases)

تم تعريف الأوامر المختصرة التالية لزيادة سرعة وسلاسة العمل:

| الاختصار | الأمر المنفذ | الوصف |
|---|---|---|
| `ls` | `dir /b` | عرض قائمة الملفات بشكل مختصر |
| `ll` | `dir` | عرض التفاصيل الكاملة للملفات والمجلدات |
| `la` | `dir /a` | عرض جميع الملفات بما فيها الملفات المخفية والسيستم |
| `clear` | `cls` | مسح شاشة الـ CMD |
| `croot` | `cd /d "%USERPROFILE%"` | الانتقال المباشر لمجلد المستخدم الرئيسي (`C:\Users\moham`) |
| `gs` | `git status` | عرض حالة مستودع Git |
| `ga` | `git add` | إضافة الملفات للتجهيز في Git |
| `gc` | `git commit -m` | عمل Commit في Git مع إضافة الرسالة مباشرة |
| `gp` | `git push` | رفع التغييرات إلى السيرفر البعيد (Remote) |
| `gl` | `git log -n 10` | عرض أحدث 10 سجلات Commit في Git |

---

## ⌨️ اختصارات لوحة المفاتيح (Clink & Terminal Hotkeys)

| الاختصار | الوظيفة |
|---|---|
| `→` (السهم الأيمن) | قبول الاقتراح التلقائي الرمادي المعروض أثناء الكتابة |
| `Tab` | الإكمال التلقائي الذكي للمسارات والملفات والأوامر |
| `Shift + Tab` | التنقل العكسي في قائمة الإكمال التلقائي |
| `Ctrl + R` | البحث التفاعلي في سجل الأوامر السابقة |
| `F8` | البحث في سجل الأوامر السابقة بناءً على الأحرف التي كتبت |
| `F7` | فتح نافذة منبثقة تفاعلية لاختيار أي أمر سابق |
| `Ctrl + L` | مسح الشاشة فوراً دون مسح سجل التاريخ |
| `Alt + Enter` | فتح نافذة CMD جديدة (عبر مدير النوافذ GlazeWM) |

---

## 🛠️ الملفات المخصصة لـ CMD

- **ملف التهيأة والأوامر المختصرة**: [scripts/cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd)
- **ملف إعدادات Clink**: [clink/clink_settings](file:///c:/Users/moham/Documents/myenv/clink/clink_settings)
- **سكريبت ربط CMD بالـ Registry**: [scripts/set-cmd-autocompletion.ps1](file:///c:/Users/moham/Documents/myenv/scripts/set-cmd-autocompletion.ps1)
- **سكريبت تثبيت وتفعيل Clink**: [scripts/install-clink.ps1](file:///c:/Users/moham/Documents/myenv/scripts/install-clink.ps1)

---

## 💡 كيفية إضافة اختصارات أو تعديل الإعدادات

1. **إضافة أمر مختصر جديد (Doskey)**:
   افتح ملف [scripts/cmd-init.cmd](file:///c:/Users/moham/Documents/myenv/scripts/cmd-init.cmd) وأضف السطر التالي:
   ```cmd
   doskey myalias=command_to_run $1
   ```
2. **تعديل إعدادات Clink**:
   قم بتعديل ملف [clink/clink_settings](file:///c:/Users/moham/Documents/myenv/clink/clink_settings) لحفظ أي خيارات تخص الاقتراحات التلقائية أو الألوان.
