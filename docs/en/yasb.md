# 📊 YASB (Yet Another Status Bar) Documentation

A lightweight, customized top status bar designed with a **Classic Sharp Obsidian Dark Theme**.

---

## 🎨 Design System

- **Fully Transparent Bar**: Background set to `transparent` with `0px top gap`.
- **Sharp Edge System**: Global `border-radius: 0px !important` across all bar widgets, workspace buttons, popups, and tooltips.
- **Obsidian Dark Aesthetic**: High-contrast silver-white text (`#f5f5f5`) on deep obsidian popups (`rgba(14, 14, 14, 0.96)`) with subtle rectangular borders (`1px solid #2a2a2a`).

---

## 🧩 Bar Widgets

1. **Workspaces Widget**:
   - **Focused Primary Screen Workspace**: Stark White (`#ffffff`, bold black text `#000000`).
   - **Focused Secondary Screen Workspace**: Distinct Gray (`#555555`, white text `#ffffff`).
   - **Inactive Workspaces**: Dark Slate (`rgba(18, 18, 18, 0.65)`).
2. **System Widgets**:
   - 🌐 **Traffic Widget**: Live download and upload speed (`⬇ Download ⬆ Upload`).
   - 💻 **CPU Widget**: Total CPU usage percentage + current frequency (MHz).
   - 🎮 **GPU Widget**: GPU usage percentage + temperature + VRAM (`mem_used / mem_total`).
   - 🧠 **RAM Memory Widget**: RAM usage percentage + GB used from total (`virtual_mem_percent%`).
   - 🚫 **Battery Widget**: Removed completely.
   - 🔊 **Audio, Mic, Notifications & Clock**: Master volume controls, media popups, date & clock.

---

## 📁 Configuration Files

- **Widgets & Layout Config**: [yasb/config.yaml](file:///%USERPROFILE%/Documents/myenv/yasb/config.yaml)
- **Styles & CSS Theme**: [yasb/styles.css](file:///%USERPROFILE%/Documents/myenv/yasb/styles.css)
