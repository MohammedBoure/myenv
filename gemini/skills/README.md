# Antigravity Global Skills Directory (`gemini/skills/`)

This directory stores custom global skills available on-demand to Antigravity agents across all workspaces.

## 📂 Structure & Usage

Each skill resides in its own sub-directory containing a `SKILL.md` file:
```
skills/
└── my-custom-skill/
    ├── SKILL.md
    └── scripts/ (optional)
```

### `SKILL.md` Specification:
```markdown
---
name: my-custom-skill
description: Comprehensive explanation of what this skill does and when to activate it.
---

# Skill Instructions
Step-by-step procedures, tool usage, or specific runbooks for the agent.
```
