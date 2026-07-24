# Zebar + GlazeWM + YASB على Windows

هذا الملف يوثق إعدادات الجهاز التي تم فحصها في **2026-07-24**، كما يوثق إعداد بدء التشغيل التلقائي عند تسجيل دخول مستخدم Windows الحالي.

## روابط ملفات الإعدادات

الروابط التالية تشير إلى الملفات الحية الموجودة على الجهاز؛ لم يتم نسخ الإعدادات أو استبدالها.

| المكوّن | ملف الإعداد | المسار |
|---|---|---|
| GlazeWM | `config.yaml` | [فتح إعداد GlazeWM](file:///C:/Users/moham/.glzr/glazewm/config.yaml) — `C:\Users\moham\.glzr\glazewm\config.yaml` |
| GlazeWM | `errors.log` | [فتح سجل GlazeWM](file:///C:/Users/moham/.glzr/glazewm/errors.log) — `C:\Users\moham\.glzr\glazewm\errors.log` |
| Zebar | `settings.json` | [فتح إعداد Zebar](file:///C:/Users/moham/.glzr/zebar/settings.json) — `C:\Users\moham\.glzr\zebar\settings.json` |
| Zebar | starter metadata | [فتح بيانات Zebar starter](file:///C:/Users/moham/.glzr/zebar/.marketplace/glzr-io.starter.json) — `C:\Users\moham\.glzr\zebar\.marketplace\glzr-io.starter.json` |
| YASB | `config.yaml` | [فتح إعداد YASB](file:///C:/Users/moham/.config/yasb/config.yaml) — `C:\Users\moham\.config\yasb\config.yaml` |
| YASB | `styles.css` | [فتح مظهر YASB](file:///C:/Users/moham/.config/yasb/styles.css) — `C:\Users\moham\.config\yasb\styles.css` |
| YASB | `yasb.log` | [فتح سجل YASB](file:///C:/Users/moham/.config/yasb/yasb.log) — `C:\Users\moham\.config\yasb\yasb.log` |

## البرامج التنفيذية والإصدارات

- GlazeWM 3.10.1: `C:\Program Files\glzr.io\GlazeWM\glazewm.exe`
- Zebar: `C:\Program Files\glzr.io\Zebar\zebar.exe`
- YASB 2.0.5: `C:\Program Files\YASB\yasb.exe`
- أداة التحكم في YASB: `C:\Program Files\YASB\yasbc.exe`

## فهم الإعداد الحالي

### GlazeWM

- تسع مساحات عمل بأسماء `1` إلى `9`.
- الإدارة بنمط tiling، مع `cloak` لإخفاء نوافذ المساحات غير النشطة.
- المسافة الداخلية بين النوافذ `20px`، والهامش العلوي `60px`، وباقي الهوامش `20px`.
- حدود النافذة المركّزة مفعلة بلون `#8dbcff`، وحدود النوافذ الأخرى بلون رمادي.
- اختصارات التركيز والنقل والتغيير تستخدم `Alt` مع H/J/K/L أو الأسهم؛ وإعادة التحميل `Alt+Shift+R` والخروج `Alt+Shift+E`.
- نوافذ Zebar مستثناة من tiling، وكذلك بعض نوافذ Picture-in-Picture وPowerToys وOffice.
- `startup_commands` و`shutdown_commands` فارغة؛ لذلك تم الاعتماد على Startup وRun في Windows دون تغيير ملف GlazeWM نفسه.

### Zebar

- `settings.json` يشغّل starter pack باسم `glzr-io.starter`.
- الـ widget المستخدم هو `with-glazewm` بالـ preset `default`.
- أمر الإقلاع الصحيح هو `zebar.exe startup`، وهو يفتح الـ widgets المعرفة في إعداد Zebar.

### YASB

- شريط علوي واحد اسمه `primary-bar`، مفعّل على كل الشاشات، بارتفاع `34px`.
- الشريط يستخدم `windows_app_bar` وblur مع زوايا مستديرة.
- يسار الشريط: Home، مساحات GlazeWM، والنافذة النشطة.
- الوسط: الساعة والتقويم.
- اليمين: CPU، الذاكرة، GitHub، الميكروفون، الصوت، الإشعارات، وقائمة الطاقة.
- `watch_stylesheet` و`watch_config` مفعّلان، لذلك يعاد تحميل التغييرات أثناء التطوير.
- `glazewm_workspaces` مذكور مرتين في YAML؛ تعريفه الأخير هو الذي يُستخدم عادةً، وهو يترك الشريط ظاهراً عند عدم اتصال GlazeWM (`hide_if_offline: false`) ويفعّل التبديل بالتمرير.
- المظهر الداكن والألوان الأساسية معرفة في `styles.css`، مع accent أزرق `#4cc2ff`.

## ما تم تجهيزه للإقلاع

تم التحقق من عدم وجود إعداد سابق لهذه البرامج في Startup أو Run، ثم تم تفعيل التالي لحساب Windows الحالي:

| البرنامج | آلية الإقلاع | الحالة |
|---|---|---|
| YASB | قيمة `YASB` في `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`، وقيمتها `C:\Program Files\YASB\yasb.exe` | مفعّل عبر `yasbc enable-autostart` |
| GlazeWM | الاختصار `GlazeWM.lnk` في [مجلد Startup](file:///C:/Users/moham/AppData/Roaming/Microsoft/Windows/Start%20Menu/Programs/Startup) | مفعّل |
| Zebar | الاختصار `Zebar.lnk` في مجلد Startup، مع الوسيط `startup` | مفعّل |

مجلد Startup الكامل هو:

`C:\Users\moham\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`

إذا بدأ YASB قبل GlazeWM فقد يظهر اتصال GlazeWM كـ offline لوقت قصير؛ هذا متوقع من الإعداد الحالي، وسيتمكن YASB من الاتصال بعد بدء GlazeWM.

## التحقق اليدوي

من PowerShell يمكن تشغيل المكونات يدوياً بهذه الأوامر:

```powershell
& 'C:\Program Files\glzr.io\GlazeWM\glazewm.exe'
& 'C:\Program Files\glzr.io\Zebar\zebar.exe' startup
& 'C:\Program Files\YASB\yasbc.exe' start
```

بعد تسجيل الخروج ثم الدخول مجدداً، يمكن التحقق من العمليات:

```powershell
Get-Process glazewm,zebar,yasb -ErrorAction SilentlyContinue
```

## التراجع

لإيقاف الإقلاع التلقائي:

```powershell
Remove-Item -LiteralPath 'C:\Users\moham\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\GlazeWM.lnk'
Remove-Item -LiteralPath 'C:\Users\moham\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\Zebar.lnk'
& 'C:\Program Files\YASB\yasbc.exe' disable-autostart
```

تم ترك ملفات إعدادات البرامج الأصلية وسجلاتها دون حذف أو إعادة توليد.
