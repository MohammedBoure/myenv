# 🪟 GlazeWM Documentation / توثيق مدير النوافذ GlazeWM

**GlazeWM** is a keyboard-driven tiling window manager for Windows.
**GlazeWM** هو مدير نوافذ مقسمة لنظام Windows يتيح التحكم بالكامل بالنوافذ عبر لوحة المفاتيح.

---

## 📂 Configuration File / ملف التهيأة

- **Main Config File / ملف الإعدادات الرئيسي**: [glazewm/config.yaml](file:///c:/Users/moham/Documents/myenv/glazewm/config.yaml)

---

## ⌨️ Complete Keybindings Cheat Sheet / جدول اختصارات لوحة المفاتيح

### 🎯 Focus & Navigation Controls / التنقل وتحديد التركيز
| Keybinding / الاختصار | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|
| `Alt + H` / `Alt + ←` | Focus left window | نقل التركيز للنافذة اليسرى |
| `Alt + L` / `Alt + →` | Focus right window | نقل التركيز للنافذة اليمنى |
| `Alt + K` / `Alt + ↑` | Focus top window | نقل التركيز للنافذة العلوية |
| `Alt + J` / `Alt + ↓` | Focus bottom window | نقل التركيز للنافذة السفلية |
| `Alt + Shift + H/L/K/J` | Move window in direction | تحريك النافذة الحالية في الاتجاه المحدد |
| `Alt + Space` / `Alt + W` | Cycle window state | التبديل بين حالات النافذة والتركيز |
| `Alt + Shift + P` | Pause GlazeWM tiling | التوقف المؤقت لـ GlazeWM والعودة لسلوك الويندوز |
| `Alt + Shift + Space` | Toggle window floating | تحويل النافذة إلى عائمة وموسّطة (Floating) |
| `Alt + T` | Return window to tiling state | إعادة النافذة لحالة التقسيم (Tiling) |
| `Alt + F` | Toggle fullscreen mode | تبديل وضع ملء الشاشة (Fullscreen) |
| `Alt + M` | Minimize window | تصغير النافذة (Minimize) |
| `Alt + Q` | Close focused window | إغلاق النافذة الحالية (Close Window) |

---

### 📐 Splitting & Resizing / اتجاهات التقسيم وتغيير الأحجام
| Keybinding / الاختصار | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|
| **Smart Split** | Auto split by aspect ratio | تقسيم تلقائي (أفقي للعرَض، عمودي للارتفاع) |
| `Alt + V` | Toggle split direction | تبديل اتجاه التقسيم (أفقي <-> عمودي) |
| `Alt + Shift + V` | Force vertical split | تقسيم عمودي إجباري (أسفل النافذة الحالية) |
| `Alt + Ctrl + V` | Force horizontal split | تقسيم أفقي إجباري (بجانب النافذة الحالية) |
| `Alt + U` / `Alt + P` | Decrease / Increase width (2%) | تصغير / زيادة عرض النافذة بمقدار 2% |
| `Alt + I` / `Alt + O` | Decrease / Increase height (2%) | تصغير / زيادة ارتفاع النافذة بمقدار 2% |
| `Alt + R` | Interactive resize mode | وضع التكبير والتصغير التفاعلي (`Esc` للخروج) |

---

### 🖥️ Workspaces & Displays / مساحات العمل والتنقل بين الشاشات
* **Left Display `DISPLAY1`**: Workspaces `1` to `8`.
* **Right Display `DISPLAY8`**: Workspaces `9` to `10`.

| Keybinding / الاختصار | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|
| `Alt + 1..8` | Focus Workspaces 1-8 (Left) | الانتقال لمساحات العمل 1-8 (الشاشة اليسرى) |
| `Alt + 9..0` | Focus Workspaces 9-10 (Right) | الانتقال لمساحات العمل 9-10 (الشاشة اليمنى) |
| `Alt + Shift + 1..0` | Move window to workspace & focus | نقل النافذة الحالية لمساحة العمل والذهاب إليها |
| `Alt + PageUp` / `Alt + A` | Focus previous active workspace | الانتقال لمساحة العمل النشطة السابقة |
| `Alt + PageDown` / `Alt + S` | Focus next active workspace | الانتقال لمساحة العمل النشطة التالية |
| `Alt + D` | Focus recent workspace | الانتقال لآخر مساحة عمل تم استخدامها |
| `Alt + Shift + A/F/D/S` | Move workspace to display | نقل مساحة العمل للشاشة (يسار/يمين/أعلى/أسفل) |

---

### 🚀 Applications & Management / تشغيل البرامج والتحكم
| Keybinding / الاختصار | Description / الوصف (English) | الوصف (العربية) |
|---|---|---|
| `Alt + Shift + Q` | App Launcher WPF Search Dialog | تشغيل مشغل التطبيقات السريع (`app-launcher.ps1`) |
| `Win + Shift + C` / `Alt + Shift + C` | Instant Selection Translate | ترجمة النص المحدد فوراً (نسخ تلقائي `Ctrl+C`) |
| `Win + Shift + Q` / `Alt + Shift + T` | Screen Region OCR Translate | ترجمة منطقة الشاشة عبر OCR (تظليل صورة) |
| `Alt + Shift + S` | Instant Full Screenshot | التقاط كامل الشاشة والحفظ المباشر للحافظة وللملفات |
| `Alt + Shift + X` | Launch Task Manager | فتح مدير المهام الفوري (Task Manager) |
| `Alt + Shift + M` | Toggle Master Mute Audio | كتم/تفعيل صوت الجهاز المباشر |
| `Alt + Shift + Z` | Window Transparency 80%/100% | تبديل شفافية النافذة النشطة 80% / 100% |
| `Alt + Enter` | Open CMD at Explorer Path | فتح نافذة `cmd.exe` في مسار متصفح الملفات الحالي |
| `Alt + Ctrl + Enter` | Open PowerShell at Explorer Path | فتح نافذة `powershell.exe` في مسار Explorer |
| `Alt + Shift + R` | Reload GlazeWM Configuration | إعادة تحميل إعدادات GlazeWM |
| `Alt + Shift + E` | Exit GlazeWM safely | الخروج الآمن من GlazeWM |
