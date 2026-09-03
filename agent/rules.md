# Environment Guidelines & Agent Operating Rules (`myenv`)

These rules are strictly enforced for all tasks, modifications, and code generation within the `myenv` environment repository.

---

## 1. Documentation & Markdown Standards

- **Language Uniformity:** All Markdown (`*.md`) files across the entire repository must be authored exclusively in English.
- **Tone & Semantics:** 
  - Do not use emojis anywhere in the documentation, configuration comments, or generated files.
  - Maintain a formal, technical, and methodical style.
  - Structure all documentation systematically using consistent hierarchical headings (`#`, `##`, `###`), clear bulleted lists, and concise tables where appropriate.
  - Avoid redundant explanations, boilerplate text, and unnecessary verbosity. Keep documentation actionable and direct.

---

## 2. External Tool Ingestion & Repository Isolation

- **Zero In-Tree Clones:**
  - Never clone, download, or copy an external repository’s source tree directly into the `myenv` project directory or any of its subdirectories.
  - Nested Git repositories (`.git` inside `.git`) are strictly prohibited to prevent tree corruption and version control conflicts.
- **Workflow for Additions Accompanied by GitHub Links:**
  - When a tool is introduced via a GitHub repository link, inspect and analyze the remote repository’s structure, release assets, and build mechanics abstractly without pulling its codebase into `myenv`.
  - The repository in `myenv` must contain only the automation code responsible for fetching, building, or configuring that tool (e.g., PowerShell installer scripts, shell automation, or package orchestrators).
  - Target installations, binaries, or cloned assets must reside exclusively at designated system paths, external runtime paths, or user binaries locations outside the `myenv` working tree, unless integrated via a managed build output artifact specifically targeted by an environment script.
  - The installation logic must autonomously handle pulling from the official GitHub source (using releases, API endpoints, or isolated system-level directories) without polluting `myenv`'s Git index.

---

## 3. Autonomous Execution & Conflict Governance

- **Instruction Governance:** All operations inside `myenv` are subordinate to the Markdown documentation and these `.agent/` guidelines.
- **Conflict Prevention & Resolution:** 
  - Guidelines and configurations must not intentionally contradict each other.
  - If a conflict arises between general instructions and `myenv`-specific requirements, the agent must autonomously resolve the issue favoring the most robust, non-breaking, and maintainable outcome without halting execution or requesting manual intervention.
  