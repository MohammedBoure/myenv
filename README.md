# MyEnv: GlazeWM + Zebar + YASB

هذه هي بيئة سطح المكتب المخصصة لهذا الجهاز. الملفات الحية موجودة داخل:

`C:\Users\moham\Documents\myenv`

تم إعداد هذه النسخة في **2026-07-24**.

## السكربتات والأتمتة (Scripts)

توجد جميع سكربتات الأتمتة الخاصة بالبيئة داخل مجلد `scripts/`:

| السكربت | الوظيفة |
|---|---|
| [set-taskbar-autohide.ps1](file:///C:/Users/moham/Documents/myenv/scripts/set-taskbar-autohide.ps1) | تفعيل/إلغاء الإخفاء التلقائي لشريط مهام Windows عبر السجل وإعادة تشغيل Explorer. |
| [set-ctrl-backspace.ps1](file:///C:/Users/moham/Documents/myenv/scripts/set-ctrl-backspace.ps1) | ربط اختصار `Ctrl+Backspace` لحذف الكلمة كاملة في PowerShell وتثبيته داخل ملف `$PROFILE`. |
| [disable-alt-shift-lang.ps1](file:///C:/Users/moham/Documents/myenv/scripts/disable-alt-shift-lang.ps1) | إيقاف اختصار `Shift+Alt` لتغيير اللغة والإبقاء فقط على `Win+Space` لتجنب التغيير العشوائي للغة. |
| [setup-all.ps1](file:///C:/Users/moham/Documents/myenv/scripts/setup-all.ps1) | السكربت الرئيسي لتطبيق كل إعدادات البيئة بضغطة واحدة (Junctions, Auto-Hide, PSReadLine, Alt+Shift Disable, Stop Zebar, Reload YASB). |

## الملفات الحية

| المكوّن | الملف | المسار |
|---|---|---|
| GlazeWM | config.yaml | [فتح الملف](file:///C:/Users/moham/Documents/myenv/glazewm/config.yaml) — `C:\Users\moham\Documents\myenv\glazewm\config.yaml` |
| GlazeWM | errors.log | [فتح السجل](file:///C:/Users/moham/Documents/myenv/glazewm/errors.log) — `C:\Users\moham\Documents\myenv\glazewm\errors.log` |
| Zebar | settings.json | [فتح الملف](file:///C:/Users/moham/Documents/myenv/zebar/settings.json) — `C:\Users\moham\Documents\myenv\zebar\settings.json` |
| Zebar | starter metadata | [فتح الملف](file:///C:/Users/moham/Documents/myenv/zebar/.marketplace/glzr-io.starter.json) — `C:\Users\moham\Documents\myenv\zebar\.marketplace\glzr-io.starter.json` |
| Zebar | errors.log | [فتح السجل](file:///C:/Users/moham/Documents/myenv/zebar/errors.log) — `C:\Users\moham\Documents\myenv\zebar\errors.log` |
| YASB | config.yaml | [فتح الملف](file:///C:/Users/moham/Documents/myenv/yasb/config.yaml) — `C:\Users\moham\Documents\myenv\yasb\config.yaml` |
| YASB | styles.css | [فتح المظهر](file:///C:/Users/moham/Documents/myenv/yasb/styles.css) — `C:\Users\moham\Documents\myenv\yasb\styles.css` |
| YASB | yasb.log | [فتح السجل](file:///C:/Users/moham/Documents/myenv/yasb/yasb.log) — `C:\Users\moham\Documents\myenv\yasb\yasb.log` |

النسخ القديمة محفوظة داخل:

`C:\Users\moham\Documents\myenv\_legacy_originals`

## مسارات التشغيل

- GlazeWM: المسار القديم `C:\Users\moham\.glzr\glazewm` أصبح junction إلى `myenv\\glazewm`، ومتغير المستخدم `GLAZEWM_CONFIG_PATH` واختصار Startup موجهان إلى نفس الملف المركزي.
- Zebar: `C:\Program Files\glzr.io\Zebar\zebar.exe`
- YASB: `C:\Program Files\YASB\yasb.exe`
- أداة YASB: `C:\Program Files\YASB\yasbc.exe`

## الحالة المركزية للإعدادات

- Zebar: المسار القديم `C:\Users\moham\.glzr\zebar` أصبح junction إلى `myenv\zebar`.
- YASB: المسار القديم `C:\Users\moham\.config\yasb` أصبح junction إلى `myenv\yasb`.
- GlazeWM: المسار القديم `C:\Users\moham\.glzr\glazewm` أصبح junction إلى `myenv\\glazewm`، ومتغير المستخدم `GLAZEWM_CONFIG_PATH` واختصار Startup موجهان إلى نفس الملف المركزي.

مرجع GlazeWM الرسمي يدعم تحديد ملف إعداد مخصص عبر `start --config="..."` أو عبر `GLAZEWM_CONFIG_PATH`.

## الشريط العلوي والمسافات

- ارتفاع شريط YASB: `34px`.
- `padding.top` في YASB: `0`، حتى يلتصق الشريط بأعلى الشاشة.
- `outer_gap.top` في GlazeWM: `0px`؛ لأن `windows_app_bar` يحجز ارتفاع YASB مرة واحدة، وأي قيمة إضافية كانت تصنع فجوة ثانية.
- `inner_gap`: `0px`، لذلك لا توجد مسافة بين النوافذ.
- الهوامش العلوية والجانبية والسفلية: `0px` في GlazeWM؛ تبدأ النوافذ عند `y=34px`، أي مباشرة بعد شريط YASB دون فراغ إضافي.
- YASB يستخدم `windows_app_bar: true`، لذلك يحتفظ Windows بمساحة الشريط.
- أثناء تبديل مساحات العمل: GlazeWM يستخدم `hide_method: cloak` للانتقال الفوري، مع إيقاف YASB animation وblur لتقليل الوميض وإعادة الرسم.
- launcher البرامج: `Alt+Q` يفتح dialog مركزية من `scripts/app-launcher.ps1`؛ اكتب اسم البرنامج ثم اضغط Enter، وسيُفتح داخل workspace الحالي.
- ثيم YASB الحالي: `Midnight Aurora`، بألوان navy/cyan، أزرار workspace واضحة، وقوائم منبثقة متناسقة.
- Zebar ما زال مفعلاً عبر starter `glzr-io.starter` وwidget `with-glazewm` بالـpreset `default`.

## شريط Windows السفلي

تم تثبيت الإخفاء التلقائي في Windows (`StuckRects3.Settings[8] = 2`) وكذلك سجلات كل شاشة في `MMStuckRects3`، ثم أُعيد تشغيل Explorer لتطبيقه. لذلك لا يظهر الشريط السفلي إلا عند تحريك المؤشر إلى الحافة السفلية للشاشة. إذا بقي ظاهراً، راجع: Settings > Personalization > Taskbar > Taskbar behaviors > Automatically hide the taskbar.

## الاختصارات الأساسية في GlazeWM

### التركيز والتحكم في النوافذ

| الاختصار | الوظيفة |
|---|---|
| `Alt+H` أو `Alt+←` | التركيز على النافذة اليسرى |
| `Alt+L` أو `Alt+→` | التركيز على النافذة اليمنى |
| `Alt+K` أو `Alt+↑` | التركيز على النافذة العلوية |
| `Alt+J` أو `Alt+↓` | التركيز على النافذة السفلية |
| `Alt+Shift+H/L/K/J` أو الأسهم | نقل النافذة المركّزة في الاتجاه المحدد |
| `Alt+U` | تصغير العرض بنسبة 2% |
| `Alt+P` | زيادة العرض بنسبة 2% |
| `Alt+O` | زيادة الارتفاع بنسبة 2% |
| `Alt+I` | تصغير الارتفاع بنسبة 2% |
| `Alt+R` ثم H/J/K/L أو الأسهم | الدخول إلى وضع تغيير الحجم |
| `Enter` أو `Esc` داخل وضع الحجم | الخروج من وضع تغيير الحجم |
| `Alt+V` | تبديل اتجاه tiling الأفقي/العمودي |
| `Alt+Space` | التبديل بين tiling وfloating وfullscreen |
| `Alt+Shift+Space` | جعل النافذة floating ومتمركزة |
| `Alt+T` | إعادة النافذة إلى tiling |
| `Alt+F` | fullscreen |
| `Alt+M` | تصغير النافذة |
| `Alt+Shift+Q` | إغلاق النافذة |
| `Alt+Shift+P` | إيقاف تحكم GlazeWM مؤقتاً؛ اضغطه مرة أخرى للعودة |

الحالة الافتراضية للنوافذ الجديدة هي tiling. لا يوجد في هذه النسخة اختصار مستقل لـmaximize؛ `Alt+F` هو اختصار fullscreen، وإعداد fullscreen الحالي لا يفرض maximize قبل fullscreen.

### مساحات العمل والشاشات

التوزيع الحالي المثبت حسب شاشات الجهاز:

- الشاشة اليسرى `DISPLAY1` (monitor index `0`): المساحات `1–5`.
- الشاشة اليمنى `DISPLAY8` (monitor index `1`): المساحات `6–10`.
- `Alt+PageUp` يستخدم `focus --prev-workspace`، و`Alt+PageDown` يستخدم `focus --next-workspace`؛ التنقل مباشر بين كل المساحات بالترتيب، بما فيها المساحات الفارغة.

| الاختصار | الوظيفة |
|---|---|
| `Alt+1` إلى `Alt+5` | الانتقال إلى مساحة على الشاشة اليسرى |
| `Alt+6` إلى `Alt+0` | الانتقال إلى مساحة على الشاشة اليمنى؛ `Alt+0` تعني workspace 10 |
| `Alt+Shift+1` إلى `Alt+Shift+5` | نقل النافذة إلى مساحة 1–5 ثم الانتقال إليها |
| `Alt+Shift+6` إلى `Alt+Shift+0` | نقل النافذة إلى مساحة 6–10 ثم الانتقال إليها |
| `Alt+PageDown` | المساحة النشطة التالية |
| `Alt+PageUp` | المساحة النشطة السابقة |
| `Alt+S` | المساحة النشطة التالية |
| `Alt+A` | المساحة النشطة السابقة |
| `Alt+D` | العودة إلى المساحة الأخيرة |
| `Alt+Shift+A` | نقل مساحة العمل كاملة إلى الشاشة اليسرى |
| `Alt+Shift+F` | نقل مساحة العمل كاملة إلى الشاشة اليمنى |
| `Alt+Shift+D` | نقل مساحة العمل إلى الشاشة العلوية |
| `Alt+Shift+S` | نقل مساحة العمل إلى الشاشة السفلية |

`Alt+Shift+7` ينقل النافذة إلى workspace 7 على الشاشة اليمنى، لأن workspace 7 مربوط صراحةً بـmonitor index 1.

### الأوامر والصيانة

| الاختصار | الوظيفة |
|---|---|
| `Alt+Enter` | فتح CMD |
| `Alt+Ctrl+Enter` | فتح Windows PowerShell |
| `Alt+Shift+R` | إعادة تحميل config.yaml |
| `Alt+Shift+W` | إعادة رسم النوافذ |
| `Alt+Shift+E` | الخروج الآمن من GlazeWM |

Windows Terminal (`wt.exe`) غير مثبت حالياً، لذلك لم أسجل له اختصاراً غير عامل. بعد تثبيته يمكن إضافة `shell-exec wt` إلى `config.yaml`.

## الإقلاع التلقائي

| البرنامج | آلية التشغيل |
|---|---|
| GlazeWM | `GlazeWM.lnk` داخل مجلد Startup، ويستخدم `start --config="C:\Users\moham\Documents\myenv\glazewm\config.yaml"` |
| Zebar | `Zebar.lnk` داخل Startup مع الوسيط `startup` |
| YASB | قيمة `YASB` في `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` |

مجلد Startup:

`C:\Users\moham\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`

## التشغيل والاختبار اليدوي

```powershell
& 'C:\Program Files\glzr.io\GlazeWM\glazewm.exe' start --config='C:\Users\moham\Documents\myenv\glazewm\config.yaml'
& 'C:\Program Files\glzr.io\Zebar\zebar.exe' startup
& 'C:\Program Files\YASB\yasbc.exe' start
```

فحص العمليات:

```powershell
Get-Process glazewm,zebar,yasb -ErrorAction SilentlyContinue
```

فحص junctions:

```powershell
Get-Item 'C:\Users\moham\.glzr\zebar','C:\Users\moham\.config\yasb' |
  Select-Object FullName,LinkType,Target
```

## التراجع

- احذف `GlazeWM.lnk` و`Zebar.lnk` من مجلد Startup.
- شغّل `& 'C:\Program Files\YASB\yasbc.exe' disable-autostart`.
- أعد قيمة `GLAZEWM_CONFIG_PATH` أو احذفها من User Environment Variables.
- لا تحذف `_legacy_originals` قبل التأكد من أن البيئة الجديدة تعمل.

## مراجع

- [GlazeWM configuration and custom config path](https://github.com/glzr-io/glazewm#config-documentation)
- [Microsoft: automatically hide the taskbar](https://support.microsoft.com/en-us/windows/experience/personalization/customize-the-taskbar-in-windows)


`Ctrl+Super+←/→` اختصار خاص بـWindows للتنقل بين Virtual Desktops، وليس اختصار GlazeWM لمساحات العمل. وقد يتدخل Windows أو يغيّر الشاشة/السطح الافتراضي. كذلك الضغط على `Super` وحده يفتح قائمة Start؛ لذلك تستخدم هذه البيئة اختصارات `Alt` ولا تعتمد على Super.