# GlazeWM Tiling Window Manager Documentation

GlazeWM is a high-performance, keyboard-driven tiling window manager for Windows developers.

---

## Configuration File

- **Main Config Path**: [glazewm/config.yaml](file:///%USERPROFILE%/Documents/myenv/glazewm/config.yaml)

---

## Complete Keybindings Cheat Sheet

### Focus & Window Navigation
| Keybinding | Action / Description |
|---|---|
| `Alt + H` / `Alt + Left` | Focus window on the left |
| `Alt + L` / `Alt + Right` | Focus window on the right |
| `Alt + K` / `Alt + Up` | Focus window above |
| `Alt + J` / `Alt + Down` | Focus window below |
| `Alt + Shift + H/L/K/J` | Move focused window in specified direction |
| `Alt + Space` / `Alt + W` | Cycle window state |
| `Alt + Shift + P` | Pause GlazeWM (revert to legacy Windows window behavior) |
| `Alt + Shift + Space` | Toggle window floating & center |
| `Alt + T` | Return window to tiling state |
| `Alt + F` | Toggle window fullscreen mode |
| `Alt + M` | Minimize window |
| `Alt + Q` | Close focused window |

---

### Tiling Direction & Resizing
| Keybinding | Action / Description |
|---|---|
| **Smart Split** | Auto-determines horizontal (right) or vertical (bottom) split based on window aspect ratio |
| `Alt + V` | Toggle split direction (Horizontal <-> Vertical) |
| `Alt + Shift + V` | **Force Vertical Split** (adds new window to the bottom) |
| `Alt + Ctrl + V` | **Force Horizontal Split** (adds new window to the right) |
| `Alt + U` / `Alt + P` | Decrease / Increase window width by 2% |
| `Alt + I` / `Alt + O` | Decrease / Increase window height by 2% |
| `Alt + R` | Enter Interactive Resize mode (`Esc` to exit) |

---

### Workspaces & Multi-Monitor Setup
- **Left Display `DISPLAY1` (Monitor Index 0)**: Workspaces `1` to `8`
- **Right Display `DISPLAY8` (Monitor Index 1)**: Workspaces `9` to `10`

| Keybinding | Action / Description |
|---|---|
| `Alt + 1..8` | Focus workspace 1-8 (Left Display) |
| `Alt + 9..0` | Focus workspace 9-10 (Right Display) |
| `Alt + Shift + 1..0` | Move focused window to target workspace and follow focus |
| `Alt + PageUp` | Focus previous workspace |
| `Alt + PageDown` | Focus next workspace |
| `Alt + D` | Focus recently used workspace |
| `Alt + Shift + A/F/D/S` | Move active workspace to target display (Left/Right/Up/Down) |

---

### Applications & System Controls
| Keybinding | Action / Description |
|---|---|
| `Alt + Shift + Q` | Launch App Search Launcher Dialog (`app-launcher.ps1`) |
| `Win + Shift + C` / `Alt + Shift + C` | **Instant Selection Translate** (Auto `Ctrl+C` highlighted text & show translation) |
| `Win + Shift + Q` / `Alt + Shift + T` | **Screen Region OCR Translate** (Drag-select screen area to extract & translate) |
| `Alt + Shift + S` | **Instant Full Screenshot** (Auto-saves to Pictures/Screenshots & Clipboard) |
| `Alt + Shift + X` | **Open Task Manager** |
| `Alt + Shift + M` | **Toggle Master Audio Mute** |
| `Alt + Shift + Z` | **Toggle Window Transparency** (80% / 100%) |
| `Alt + Enter` | Open CMD at current File Explorer directory |
| `Alt + Ctrl + Enter` | Open PowerShell at current File Explorer directory |
| `Alt + Shift + R` | Reload GlazeWM configuration (`config.yaml`) |
| `Alt + Shift + E` | Exit GlazeWM safely |
