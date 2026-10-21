# Gemini Antigravity Environment Configuration

Centralized, version-controlled global configuration, guidelines, rules, custom skills, and MCP server integrations for Google Antigravity (AGY) within MyEnv.

---

## Architecture & Directory Junction

To ensure that your Antigravity agent guidelines, MCP tools, and skills are completely backed up in Git and automatically restored on new machine setups, MyEnv links the global configuration directory via an NTFS directory junction:

| System Path (Source) | MyEnv Repository Path (Target) | Purpose |
|---|---|---|
| `%USERPROFILE%\.gemini\config` | [`gemini/`](file:///%USERPROFILE%/Documents/myenv/gemini) | Global Antigravity configuration directory |

---

## Files & Configuration Hierarchy

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

### 4. Multi-Instance & Account Manager (`agys`, `agy1` .. `agyN`)
Integrated via [`powershell/antigravity-switcher.ps1`](file:///%USERPROFILE%/Documents/myenv/powershell/antigravity-switcher.ps1) and [`scripts/agys.ps1`](file:///%USERPROFILE%/Documents/myenv/scripts/agys.ps1):
- **Arbitrary Dynamic Instances**: Run `agy1`, `agy2`, through any arbitrary number (e.g. `agy100`).
- **On-Demand Auto-Creation**: Typing a new command slot for the first time automatically provisions an isolated virtual profile root at `%USERPROFILE%\.gemini\instances\agy<N>`.
- **100% Isolated `.gemini` Folders**: Each instance owns its independent `.gemini\antigravity-cli\` directory with dedicated SQLite session databases (`conversations/*.db`, `conversation_summaries.db`), history (`history.jsonl`), cache, and settings (`settings.json`). Zero database locking or session mix-ups.
- **Strict Account & Credential Separation**: Each instance maintains its distinct Google OAuth token stored in `token.json` and synchronized to Windows Credential Manager under Mutex concurrency locks.
- **Duplicate Account Detection**: Running `agys -l` highlights if any slots mistakenly share the same Google Account and provides one-click re-authentication commands (`agy<N> login`).
- **Central Guidelines & SSH Sharing**: Automatically links `%USERPROFILE%\.ssh` (for Git authentication) and `%USERPROFILE%\Documents\myenv\gemini` (for centralized global rules, skills, and MCP servers) into each instance.
- **Management CLI (`agys`)**:
  - `agys -l` / `agys list`: Displays formatted status table of all active slots, account emails, session counts, and duplicate flags.
  - `agys login <N>` (or `agy<N> login`): Re-authenticates slot `<N>` in browser with explicit prompt to choose a separate Google account.
  - `agys logout <N>` (or `agy<N> logout`): Clears credentials and disconnects slot `<N>`.
  - `agys rm <N>`: Deletes slot directory and tokens completely.

---

## Setup & Automation

To create junctions, initialize token directories, and configure PowerShell profile redirection:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-gemini-config.ps1"
```

Or run the master setup script:
```powershell
powershell -ExecutionPolicy Bypass -File "$env:USERPROFILE\Documents\myenv\scripts\setup-all.ps1"
```

