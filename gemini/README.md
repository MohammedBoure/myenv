# Gemini Antigravity Configuration Directory (`gemini/`)

Centralized, version-controlled global configuration, instructions, rules, skills, and Model Context Protocol (MCP) integrations for **Google Antigravity (AGY)**.

This directory serves as the single source of truth for the machine-local global configuration directory located at `~/.gemini/config/` (`%USERPROFILE%\.gemini\config`). It is automatically linked via directory junction by [`scripts/setup-all.ps1`](file:///C:/Users/moham/Documents/myenv/scripts/setup-all.ps1) and [`scripts/setup-gemini-config.ps1`](file:///C:/Users/moham/Documents/myenv/scripts/setup-gemini-config.ps1).

---

## 📂 Files & Structure

| File / Folder | Purpose |
|---|---|
| [`GEMINI.md`](file:///C:/Users/moham/Documents/myenv/gemini/GEMINI.md) | Global instructions, coding standards, Git workflow rules, and safety guardrails loaded automatically by Antigravity across all workspaces. |
| [`config.json`](file:///C:/Users/moham/Documents/myenv/gemini/config.json) | Global user settings and CLI telemetry/remote control preferences. |
| [`mcp_config.json`](file:///C:/Users/moham/Documents/myenv/gemini/mcp_config.json) | Central Model Context Protocol (MCP) servers configuration to integrate external tools, databases, and services with Antigravity agents. |
| [`projects/`](file:///C:/Users/moham/Documents/myenv/gemini/projects) | Project workspace tracking metadata for Antigravity sessions. |
| [`rules/`](file:///C:/Users/moham/Documents/myenv/gemini/rules) | Granular global rule definitions applied contextually or hierarchically across projects. |
| [`skills/`](file:///C:/Users/moham/Documents/myenv/gemini/skills) | Custom user-defined global skills available on-demand to Antigravity agents. |

---

## 🔗 Automated Environment Linking

To establish or verify the junction link on your machine:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-gemini-config.ps1"
```

This creates an NTFS directory junction:
```
%USERPROFILE%\.gemini\config  <====>  %USERPROFILE%\Documents\myenv\gemini
```
