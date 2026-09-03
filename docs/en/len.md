# 🌳 len (ProjectLens) CLI Tool Documentation

**len (ProjectLens)** is a fast, lightweight, and customizable command-line tool designed to generate clean directory trees, manage persistent file/folder blocklists, control traversal depth, analyze detailed code statistics, and quickly copy results to your clipboard.

---

## 🚀 Key Features

- 🌳 **Visual Directory Tree**: Instant hierarchical visualization of projects and subdirectories.
- 🔢 **Customizable Depth**: Limit traversal depth easily (e.g. `len 3`, `len ./src 2`, or `len -d 3`).
- 🚫 **Persistent Blocklist Management**:
  - Add blocked files: `len -f [file_or_pattern]`
  - Add blocked folders: `len -F [folder_or_pattern]`
  - Remove blocked files: `len -rf [file_or_pattern]`
  - Remove blocked folders: `len -rF [folder_or_pattern]`
  - List blocked items: `len -l` or `len --list-blocked`
- 📋 **Copy to Clipboard (`-c`)**: Direct clipboard copy of clean tree output for pasting into LLMs or docs.
- 📊 **Code & File Statistics (`-s`, `-e`)**: Inline line counts, file sizes, extensions summary, and test/doc coverage.
- 🔄 **Automated Remote Installation & Updates**: Managed directly via `scripts/install-len.ps1` from the remote repository [`https://github.com/MohammedBoure/len.git`](https://github.com/MohammedBoure/len.git).

---

## 📦 Installation & Automated Updates

The `myenv` environment manages `len` automatically from the remote GitHub repository:

### 1. Run the Installer / Updater
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\install-len.ps1"
```

### 2. Smart Update Detection
- Queries the remote GitHub repository for the latest `HEAD` commit.
- Checks the currently installed package (`projectlens`) in Python.
- If already installed at the latest commit, it performs **no action** and exits immediately.
- If not installed, outdated, or in local editable mode, it automatically upgrades to the latest remote version.

### 3. Master Setup Integration
`scripts/setup-all.ps1` automatically executes `install-len.ps1` during environment setup.

---

## 💻 Usage & Examples

### 1. Basic Tree View
```bash
# Scan current directory (full depth)
len

# Scan a specific directory
len D:\my-project
len ./src
```

### 2. Controlling Depth
```bash
# Scan current directory up to depth 3
len 3

# Scan specific folder up to depth 2
len ./src 2
```

### 3. Copying Tree to Clipboard (`-c`)
```bash
# Scan up to depth 3 and copy output to clipboard
len 3 -c

# Scan specific path and copy to clipboard
len ./src 2 -c
```

### 4. Blocklist Management
Rules are stored persistently in `~/.lenconfig.json`:
```bash
# Block temporary files or folders
len -f "*.log"
len -f secret.env
len -F node_modules
len -F ".git"

# Remove from blocklist
len -rf "*.log"
len -rF node_modules

# List all blocked patterns
len -l
```

### 5. Project Statistics
```bash
# Show line counts and file sizes inline
len -s

# Show extended project analysis (largest files, test coverage)
len -s -e
```

---

## 🛠️ CLI Options Reference

| Option / Flag | Alias | Description |
|---|---|---|
| `[PATH]` | | Project directory path (defaults to current directory `.`) |
| `[DEPTH]` | `-d`, `--depth` | Maximum directory traversal depth (e.g., `1`, `2`, `3`) |
| `-c` | `--copy` | Copy output tree to system clipboard |
| `-f <pattern>` | `--add-file` | Add file name or pattern to blocklist |
| `-F <pattern>` | `--add-folder` | Add folder name or pattern to blocklist |
| `-rf <pattern>` | `--remove-file` | Remove file name or pattern from blocklist |
| `-rF <pattern>` | `--remove-folder` | Remove folder name or pattern from blocklist |
| `-l` | `--list-blocked` | Display current blocklist rules |
| `-s` | `--show-stats` | Show file lines and sizes inline |
| `-e` | `--show-extended-stats` | Show extended code statistics |
| `-h` | `--help` | Display help manual |
