# 📊 YASB Status Bar Documentation / توثيق شريط المهام YASB

Lightweight, highly customized status bar with a **Classic Sharp Dark Aesthetic**.
شريط علوي خفيف الوزن ومخصص للغاية مصمم بهوية بصريّة حادة وشديدة التباين (Sharp Obsidian Dark Theme).

---

## 🎨 Design System / النظام البصري

- **Fully Transparent Bar / شريط شفاف بالكامل**: Background set to `transparent` with `0px top gap`.
- **Sharp Edge System / تصميم الحواف الحادة**: Global `border-radius: 0px !important` across all bar widgets, workspace buttons, popups, and tooltips.
- **Obsidian Dark Aesthetic / تباين عالي**: High-contrast silver-white text (`#f5f5f5`) on deep obsidian popups (`rgba(14, 14, 14, 0.96)`) with subtle rectangular borders (`1px solid #2a2a2a`).

---

## 🧩 Widgets / عناصر الشريط

1. **Workspaces Widget / مساحات العمل**:
   - **Focused Primary Screen Workspace / الشاشة الرئيسية النشطة**: Stark White (`#ffffff`).
   - **Focused Secondary Screen Workspace / الشاشة الثانوية النشطة**: Distinct Gray (`#555555`).
   - **Inactive Workspaces / مساحات العمل غير النشطة**: Dark Slate (`rgba(18, 18, 18, 0.65)`).
2. **System Widgets / أدوات النظام**:
   - 🌐 **Traffic Widget / شبكة الإنترنت**: Live download & upload speed (`⬇ Download ⬆ Upload`).
   - 💻 **CPU Widget / استهلاك المعالج**: Total usage percentage + current frequency (MHz).
   - 🎮 **GPU Widget / كرت الشاشة**: Usage percentage + temperature + VRAM (`mem_used / mem_total`).
   - 🧠 **RAM Memory Widget / الذاكرة العشوائية**: Usage percentage + GB used from total.
   - 🔊 **Audio, Mic, Notifications & Clock / الصوت والساعة والوسائط**: Controls & volume popups.

---

## 📁 Configuration Files / ملفات التهيأة

- **Widgets & Layout Config / ملف الـ Widgets والـ Layout**: [yasb/config.yaml](file:///c:/Users/moham/Documents/myenv/yasb/config.yaml)
- **Styles & CSS Theme / ملف التنسيقات والأنماط**: [yasb/styles.css](file:///c:/Users/moham/Documents/myenv/yasb/styles.css)
