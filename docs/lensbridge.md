# LensBridge Server Documentation

**LensBridge** is a high-performance desktop server and reverse stream proxy hub that ingests real-time video streams from mobile phone back cameras over Wi-Fi and converts them into standard formats for AI models, computer vision applications, and web dashboards.

---

## Architecture & Isolation Guarantee

In compliance with the `myenv` environment guidelines:
- **Zero In-Tree Source Code**: The LensBridge source code is cloned into `D:\git\LensBridge` and is never checked into or stored within the `myenv` directory.
- **Isolated Virtual Environment**: Dependencies (`fastapi`, `uvicorn`, `websockets`, `PySide6`, etc.) are installed in a dedicated Python virtual environment located at `D:\git\LensBridge\server\venv`, keeping your system Python clean.
- **Automated Workflow**: Installation, updates, execution, and background service management are driven by `scripts/install-lensbridge.ps1` and integrated PowerShell shortcuts.

---

## Installation & Remote Updates

### Automated Installation Command
```powershell
# Run the automated installer script:
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\install-lensbridge.ps1"

# Or use the built-in PowerShell helper:
lensbridge-install
```

### What the Installer Does
1. **Verifies Prerequisites**: Checks for Git and Python 3.10+.
2. **Repository Provisioning**: Clones `https://github.com/MohammedBoure/LensBridge.git` into `D:\git\LensBridge` (or runs `git pull` if it already exists).
3. **Isolated Python Venv**: Initializes `D:\git\LensBridge\server\venv` and upgrades `pip`.
4. **Dependency Restoration**: Installs all requirements from `server/requirements.txt`.
5. **Background Service Registration**: Calls `service_manager.ps1 install` to configure 24/7 background execution on Windows login.

---

## PowerShell Helper Commands

The following helper functions are registered in [`powershell/midnight-aurora.ps1`](file:///C:/Users/moham/Documents/myenv/powershell/midnight-aurora.ps1):

| Command | Description |
|---|---|
| `lensbridge-install [action]` | Runs `scripts/install-lensbridge.ps1` (`install`, `start`, `status`, or `skip`). |
| `lensbridge-run [args]` | Launches LensBridge Server interactively (`main.py`) using `D:\git\LensBridge\server\venv\Scripts\python.exe`. Pass `--no-gui` to run headless. |
| `lensbridge-service [action]` | Controls background service via `service_manager.ps1` (`status`, `start`, `stop`, `restart`, `install`, `uninstall`). |

---

## Service Management

You can inspect or control the 24/7 background service at any time:

```powershell
# View service running status, memory usage, and endpoints:
lensbridge-service status

# Start background service:
lensbridge-service start

# Stop background service:
lensbridge-service stop

# Restart background service:
lensbridge-service restart
```

---

## Endpoints Reference

When the server is active:
- **Web Dashboard**: `http://127.0.0.1:8765/`
- **MJPEG Video Stream**: `http://127.0.0.1:8765/stream/video` (or `/video_feed`)
- **WebSocket Binary Proxy**: `ws://127.0.0.1:8765/ws/proxy`
- **REST Snapshot**: `http://127.0.0.1:8765/snapshot`
- **UDP Auto-Discovery**: Port `45454`
