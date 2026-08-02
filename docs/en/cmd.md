# 💻 Command Prompt (CMD) Environment Documentation

An enhanced **Command Prompt (CMD)** environment using **Doskey aliases** and **Clink** real-time history predictions & auto-suggestions.

---

## 🚀 Key Features

1. **Natural Text Editing & History Search**:
   - `→` / `←`: Character-by-character line editing.
   - `↑` / `↓`: Prefix-matching history search.
   - `Tab`: File and folder completion.
   - **Obsidian Dark Theme**: Custom dark popup menus matching environment aesthetic.
2. **Colored Prompt**:
   - Displays timestamp, username, hostname, and current directory in high-contrast colors.
3. **AutoRun Registry Integration**:
   - Registered under `HKCU:\Software\Microsoft\Command Processor\AutoRun` pointing to [scripts/cmd-init.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cmd-init.cmd).

---

## ⚡ Doskey Command Aliases

| Alias | Executed Command | Description |
|---|---|---|
| `cd` / `chdir` | `cd /d <path> & ls` | Change directory and auto-list contents (Auto-LS) |
| `ls` | `dir /b` | Brief file listing |
| `ll` | `dir` | Detailed file listing |
| `la` | `dir /a` | List all files including hidden and system files |
| `clear` | `cls` | Clear console screen |
| `croot` | `cd /d "%USERPROFILE%" & ls` | Jump directly to User Home directory + Auto-LS |
| `docs` | `docs [wm|translate|cmd|ps|scripts]` | CLI Documentation & Shortcut Navigator |
| `gs` | `git status` | Git status |
| `ga` | `git add` | Git add |
| `gc` | `git commit -m` | Git commit with message |
| `gp` | `git push` | Git push to remote |
| `gl` | `git log -n 10` | Display latest 10 git commits |
| `sudo` | `RunAs Administrator` | Run CMD or command with Administrator privileges |
| `cb` / `c` | `cb.cmd <command>` | Execute command, display output, and copy to Clipboard |

---

## ⌨️ Clink Navigation & Hotkeys

| Keybinding | Action / Function |
|---|---|
| `→` / `←` | Move cursor character-by-character |
| `↑` / `↓` | Navigate history matching typed command prefix |
| `Tab` | Open interactive file & folder completion menu |
| `Ctrl + Space` / `F7` | Open interactive history search popup dialog |
| `Ctrl + L` | Clear screen buffer without clearing command history |
| `Alt + Enter` | Launch new CMD window via GlazeWM |

---

## 🛠️ Associated Files

- **CMD Initialization Script**: [scripts/cmd-init.cmd](file:///%USERPROFILE%/Documents/myenv/scripts/cmd-init.cmd)
- **Clink Settings**: [clink/clink_settings](file:///%USERPROFILE%/Documents/myenv/clink/clink_settings)
- **Registry Integration Script**: [scripts/set-cmd-autocompletion.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/set-cmd-autocompletion.ps1)
- **Clink Installer Script**: [scripts/install-clink.ps1](file:///%USERPROFILE%/Documents/myenv/scripts/install-clink.ps1)
