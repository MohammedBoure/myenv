# Antigravity Global Rules Directory (`gemini/rules/`)

This directory stores granular, project-agnostic rules loaded across all sessions and workspaces in Antigravity.

## 📂 Structure & Usage

- Place Markdown rule files (e.g., `*.md`) here.
- Rules can include optional YAML frontmatter specifying loading triggers:
  ```markdown
  ---
  trigger: always_on
  description: Brief description of rule
  ---
  # Rule Content Here
  ```
- Any rule placed here is globally discovered and applied to all development workspaces.
