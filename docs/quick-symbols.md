# Quick Symbols & Words: Global Keyboard-Driven Symbol Palette

A fast, lightweight, and keyboard-driven floating palette for quick access and instant auto-injection of frequently used symbols, mathematical operators, bullets, typography characters, code snippets, and frequent words into any active window across MyEnv and Windows.

---

## Overview

- **Global System-Wide Shortcut**: Activated instantly from any application using **`Win + Shift + W`** (or **`Alt + Shift + W`**).
- **Direct Unicode Auto-Injection**: Once a symbol or word is selected, it restores focus to the previously active window and injects the text via Win32 Unicode `SendInput` (with clipboard fallback).
- **Real-Time Keyboard Search**: Automatically focuses the search bar upon opening so you can type immediately to filter by symbol character, English/Arabic label, or category.
- **Customizable & Persistent Storage**: Add custom words or symbols on the fly using `Ctrl + Enter` (or click `[+] Add`), or delete existing ones using `Del` key or `[x]` button. Saved automatically to `%APPDATA%\NightPad\quick_symbols.json`.
- **Shared Dictionary**: Seamlessly synchronized between the standalone system-wide tool (`QuickSymbols.exe`) and the integrated Notepad (`NightPad.exe`) palette.

---

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Win + Shift + W` / `Alt + Shift + W` | Open Quick Symbols & Words palette from anywhere |
| `Up` / `Down` | Navigate through the symbol list |
| `PageUp` / `PageDown` | Jump 5 items up or down |
| `Enter` | Inject selected symbol/word into active application and close |
| `Ctrl + Enter` | Add search query as a new custom symbol/word |
| `Del` | Delete selected symbol/word from dictionary |
| `Esc` | Close palette without inserting and refocus previous window |

---

## Binaries & Source

- **Executable**: [`scripts/quick-symbols/QuickSymbols.exe`](file:///C:/Users/moham/Documents/myenv/scripts/quick-symbols/QuickSymbols.exe)
- **Source Code**: [`tools/quick-symbols/`](file:///C:/Users/moham/Documents/myenv/tools/quick-symbols)
- **Configuration**: Bound in [`glazewm/config.yaml`](file:///C:/Users/moham/Documents/myenv/glazewm/config.yaml)
