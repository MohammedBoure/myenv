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
| `Alt + Ctrl + P` | Pause GlazeWM (revert to legacy Windows window behavior) |
| `Alt + Shift + Space` | Toggle window floating & center |
| `Alt + Ctrl + Shift + I` | Toggle permanent ignore rule for focused window (shrinks/centers dialog or removes rule to re-tile) |
| `Alt + Ctrl + I` | Temporarily unmanage focused window for current session, shrink to centered dialog size (`ignore`) |
| `Alt + Ctrl + T` | Return window to tiling state |
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
| `Alt + Ctrl + Left` / `Alt + Ctrl + U` | Decrease window width by 2% |
| `Alt + Ctrl + Right` / `Alt + Ctrl + P` | Increase window width by 2% |
| `Alt + Ctrl + Down` / `Alt + Ctrl + O` | Increase window height by 2% |
| `Alt + Ctrl + Up` / `Alt + Ctrl + K` | Decrease window height by 2% |
| `Alt + Ctrl + R` | Enter Interactive Resize mode (`Esc` to exit) |

---

### Workspaces & Multi-Monitor Setup
- **Screen 1 `DISPLAY1` (Monitor Index 1 - Center Primary: PLAYER MSG12)**: Workspaces `1` to `10`
- **Screen 2 `DISPLAY8` (Monitor Index 2 - Right: Fujitsu L20T-1 ECO)**: Workspaces `a`, `z`, `e`, `r`, `t`
- **Screen 3 `DISPLAY13` (Monitor Index 0 - Left Virtual: Deskreen VDD by MTT)**: Workspaces `y`, `u`, `i`, `o`, `p`

| Keybinding | Action / Description |
|---|---|
| `Alt + 1..0` | Focus workspace 1-10 (Screen 1: Primary Center) |
| `Alt + Shift + 1..0` | Move focused window to workspace 1-10 and follow focus |
| `Alt + A / Z / E / R / T` | Focus workspace a, z, e, r, t (Screen 2: Right Fujitsu) |
| `Alt + Shift + A / Z / E / R / T` | Move focused window to workspace a, z, e, r, t and follow focus |
| `Alt + Y / U / I / O / P` | Focus workspace y, u, i, o, p (Screen 3: Left Deskreen Virtual) |
| `Alt + Shift + Y / U / I / O / P` | Move focused window to workspace y, u, i, o, p and follow focus |
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
| `Win + Shift + X` / `Alt + Ctrl + E` | **Quick Type & Paste Translate** (Floating translator window) |
| `Alt + Shift + S` | **Instant Full Screenshot** (Auto-saves to Pictures/Screenshots & Clipboard) |
| `Alt + Shift + X` | **Open Task Manager** |
| `Alt + Shift + M` | **Toggle Master Audio Mute** |
| `Alt + Ctrl + Z` | **Toggle Window Transparency** (80% / 100%) |
| `Alt + Enter` | Open CMD at current File Explorer directory |
| `Alt + Ctrl + Enter` | Open PowerShell at current File Explorer directory |
| `Alt + Ctrl + Shift + R` | Reload GlazeWM configuration (`config.yaml`) |
| `Alt + Ctrl + Shift + E` | Exit GlazeWM safely |

