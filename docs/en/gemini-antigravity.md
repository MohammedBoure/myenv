# 🤖 Gemini Antigravity Environment Configuration

Centralized, version-controlled global configuration, guidelines, rules, custom skills, and MCP server integrations for **Google Antigravity (AGY)** within MyEnv.

---

## 📂 Architecture & Directory Junction

To ensure that your Antigravity agent guidelines, MCP tools, and skills are completely backed up in Git and automatically restored on new machine setups, MyEnv links the global configuration directory via an NTFS directory junction:

| System Path (Source) | MyEnv Repository Path (Target) | Purpose |
|---|---|---|
| `%USERPROFILE%\.gemini\config` | [`gemini/`](file:///%USERPROFILE%/Documents/myenv/gemini) | Global Antigravity configuration directory |

---

## 🛠️ Files & Configuration Hierarchy

```
myenv/
└── gemini/
    ├── README.md               # Directory overview & structure
    ├── GEMINI.md               # Global instructions, coding rules & Git workflow
    ├── mcp_config.json         # Model Context Protocol (MCP) server definitions
    ├── rules/                  # Granular contextual/global rule files (*.md)
    │   └── README.md
    └── skills/                 # Custom on-demand global skills (* / SKILL.md)
        └── README.md
```

### 1. `GEMINI.md` (Global Guidelines)
Contains persistent rules automatically loaded by Antigravity CLI (`agy`) and Antigravity IDE:
- **Language Alignment**: Communicating in the exact language of the user prompt.
- **Directory Architecture**: Modular sub-files, clean naming, and mandatory `README.md` files in every code directory.
- **Git Workflow**: Conventional Commits (`feat:`, `fix:`, `refactor:`, `docs:`) and initial branch named `main`.
- **Safety Guardrails**: Prohibition of destructive commands and protection of private credentials/secrets.

### 2. `mcp_config.json` (Model Context Protocol)
Configures external MCP servers to extend the AI agent with external APIs, databases, file system bridges, or CLI tools.

### 3. `rules/` & `skills/`
- **Rules**: Additional domain-specific or team-specific guidelines loaded contextually.
- **Skills**: Step-by-step procedures and tool workflows loaded on-demand via progressive disclosure.

---

## 🚀 Setup & Automation

To create or verify the Gemini Antigravity directory junction:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-gemini-config.ps1"
```

Or run the master setup script:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-all.ps1"
```
