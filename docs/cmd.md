# 💻 توثيق بيئة Command Prompt (CMD)

بيئة **CMD** متطورة ومخصصة داخل نظام `myenv` لتوفر تجربة كتابة أوامر سريعة وذكية تشبه التيرمينال في أنظمة Linux، بفضل الدمج بين اختصارات **Doskey** وأداة **Clink**.

---

## 🚀 المميزات الرئيسية لـ CMD

1. **الإكمال التلقائي الذكي والقائمة التفاعلية أسفل السطر (Interactive Popup List & Auto-Suggestions)**:
   - تعرض لك الأداة قائمة تفاعلية بجميع الأوامر السابقة والأوصاف **تحت سطر الأوامر مباشرة** (مثل قائمة ListView في PowerShell).
   - تضغط على **`Tab`** أو **`Ctrl+R`** أو **`F7`** لتنفتح القائمة المنبثقة تحت سطر الكتابة، وتستخدم الأسهم (`↑` / `↓`) لاختيار الأمر واختياره بـ `Enter`.
   - اقتراحات فورية رمادية للأوامر السابقة، وتضغط على **السهم الأيمن `→`** للقبول السريع.
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

## ⌨️ نظام الإكمال المزدوج واختصارات المفاتيح (Dual Completion System)

| الاختصار | نوع الإكمال والوظيفة |
|---|---|
| **`Tab`** | 📁 **إكمال الملفات والمسارات والأوامر**: فتح قائمة تفاعلية أسفل سطر الأوامر لاختيار الملفات أو المجلدات |
| **`Ctrl + Space`** / **`Shift + Tab`** | 📜 **قائمة سجل الأوامر السابقة**: فتح قائمة تفاعلية منبثقة بسجل الأوامر السابقة لاختيار أي أمر قديم |
| **`Ctrl + R`** / **`F7`** | 🔍 **البحث في سجل التاريخ**: فتح قائمة منبثقة للبحث السريع في السجل |
| **`F8`** | 🎯 **مطابقة السجل بالنص الحالي**: البحث في السجل عن الأوامر التي تبدأ بالحروف المكتوبة حالياً |
| **`→` (السهم الأيمن)** | ⚡ **قبول التلميح الرمادي**: قبول الاقتراح التلقائي المعروض على السطر باللون الرمادي |
| **`Ctrl + L`** | 🧹 **مسح الشاشة** فوراً دون مسح السجل |
| **`Alt + Enter`** | 🪟 فتح نافذة CMD جديدة عبر مدير النوافذ |

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
