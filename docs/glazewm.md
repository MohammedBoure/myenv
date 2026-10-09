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
| `Alt + Ctrl + Pause` / `Win + Pause` | Pause GlazeWM (revert to legacy Windows window behavior) |
| `Alt + Shift + Space` | Toggle window floating & center |
| `Win + Shift + I` | Toggle permanent ignore rule for focused window (shrinks/centers dialog or removes rule to re-tile) |
| `Win + Ctrl + I` | Temporarily unmanage focused window for current session, shrink to centered dialog size (`ignore`) |
| `Win + T` | Return window to tiling state |
| `Alt + F` | Toggle window fullscreen mode |
| `Alt + M` | Minimize window |
| `Alt + Q` | Close focused window (Strictly close, exempted from workspaces) |

---

### Tiling Direction & Resizing
| Keybinding | Action / Description |
|---|---|
| **Smart Split** | Auto-determines horizontal (right) or vertical (bottom) split based on window aspect ratio |
| `Alt + V` | Toggle split direction (Horizontal <-> Vertical) |
| `Alt + Shift + V` | **Force Vertical Split** (adds new window to the bottom) |
| `Alt + Ctrl + V` | **Force Horizontal Split** (adds new window to the right) |
| `Alt + Ctrl + Left` | Decrease window width by 2% |
| `Alt + Ctrl + Right` | Increase window width by 2% |
| `Alt + Ctrl + Down` | Increase window height by 2% |
| `Alt + Ctrl + Up` | Decrease window height by 2% |
| `Alt + Ctrl + Space` | Enter Interactive Resize mode (`Esc` to exit) |

---

### Workspaces & Multi-Monitor Setup
- **Screen 1 `DISPLAY1` (Monitor Index 1 - Center Primary: PLAYER MSG12)**: Workspaces `1` to `10`
- **Screen 2 `DISPLAY8` (Monitor Index 2 - Right: Fujitsu L20T-1 ECO)**: Workspaces `z`, `e`, `r`, `t`
- **Screen 3 `DISPLAY13` (Monitor Index 0 - Left Virtual: Deskreen VDD by MTT)**: Workspaces `y`, `u`, `i`, `o`, `p`

| Keybinding | Action / Description |
|---|---|
| `Alt + 1..0` | Focus workspace 1-10 (Screen 1: Primary Center) |
| `Alt + Shift + 1..0` / `Shift + Alt + 1..0` | Move focused window to workspace 1-10 and follow focus |
| `Alt + Z / E / R / T` | Focus workspace z, e, r, t (Screen 2: Right Fujitsu) |
| `Alt + Shift + Z / E / R / T` / `Shift + Alt + Z / E / R / T` | Move focused window to workspace z, e, r, t and follow focus |
| `Alt + Y / U / I / O / P` | Focus workspace y, u, i, o, p (Screen 3: Left Deskreen Virtual) |
| `Alt + Shift + Y / U / I / O / P` / `Shift + Alt + Y / U / I / O / P` | Move focused window to workspace y, u, i, o, p and follow focus |
| `Alt + PageUp` | Focus previous workspace |
| `Alt + PageDown` | Focus next workspace |
| `Alt + D` | Focus recently used workspace |
| `Alt + Ctrl + Shift + Left/Right/Up/Down` | Move active workspace to target display (Left/Right/Up/Down) |

---

### Applications & System Controls
| Keybinding | Action / Description |
|---|---|
| `Alt + Shift + Q` | Launch App Search Launcher Dialog (`app-launcher.ps1`) |
| `Win + Shift + C` / `Alt + Shift + C` | **Instant Selection Translate** (Auto `Ctrl+C` highlighted text & show translation) |
| `Win + Shift + Q` | **Screen Region OCR Translate** (Drag-select screen area to extract & translate) |
| `Win + Shift + X` | **Quick Type & Paste Translate** (Floating translator window) |
| `Alt + Shift + S` | **Instant Full Screenshot** (Auto-saves to Pictures/Screenshots & Clipboard) |
| `Alt + Shift + X` | **Open Task Manager** |
| `Alt + Shift + M` | **Toggle Master Audio Mute** |
| `Win + Shift + Z` | **Toggle Window Transparency** (80% / 100%) |
| `Alt + Enter` | Open CMD at current File Explorer directory |
| `Alt + Ctrl + Enter` | Open PowerShell at current File Explorer directory |
| `Win + Shift + R` / `Alt + Shift + F5` | Reload GlazeWM configuration (`config.yaml`) |
| `Win + Shift + Escape` / `Alt + Shift + Escape` | Exit GlazeWM safely |

