---
name: make-instructions
description: 'Create VS Code file-based instructions (.instructions.md files). Use when asked to create, scaffold, or add file-based instructions for Copilot. Generates .instructions.md with YAML frontmatter and background knowledge content.'
---

# Create File-Based Instructions

This skill helps you scaffold VS Code file-based instructions (`.instructions.md` files) that provide background knowledge to Copilot about specific parts of the codebase. These files are applied automatically based on glob patterns or semantic matching, giving Copilot domain-specific context when working on matching files.

## When Not to Use

- Setting project-wide instructions — use `.github/copilot-instructions.md` or `AGENTS.md` instead
- Creating reusable agent workflows with structured steps — use Agent Skills instead
- Adding instructions that need to be invokable on demand — use Agent Skills instead

## Workflow

### Step 1: Investigate the topic

Build understanding of the area the instructions should cover. Identify:

- [ ] What files or file patterns the instructions apply to
- [ ] Key conventions, patterns, or architectural rules for that area
- [ ] Common pitfalls that Copilot should avoid
- [ ] Non-obvious domain knowledge that isn't discoverable from code alone

If the scope is unclear or overlaps with existing instructions, ask the user for clarification.

### Step 2: Choose the file location

Instructions files go in `.github/instructions/` by default. Pick a descriptive filename:

```
.github/instructions/<topic>.instructions.md
```

Examples: `csharp-style.instructions.md`, `query-pipeline.instructions.md`, `test-conventions.instructions.md`

### Step 3: Generate the file with YAML frontmatter

Create the file with the required YAML frontmatter header:

```yaml
---
name: '<Display Name>'              # Optional. Display name in the UI. Defaults to filename.
description: '<Short description>'  # Optional. Shown on hover. Also used for semantic matching.
applyTo: '<glob pattern>'           # Optional. Glob pattern relative to workspace root. Omit to require manual attachment or rely on semantic matching only.
---
```

Common `applyTo` patterns:

- `**/*.cs` — all C# files
- `test/**` — files under a specific folder

### Step 4: Write the body content

Write concise, actionable Markdown content. Follow these principles:

- **Be concise** — instructions share the context window; only include what the agent wouldn't already know
- **Be specific** — use concrete rules, not vague guidance
- **Include examples** — short code snippets showing preferred vs. avoided patterns are very effective
- **Explain why** — when a rule exists for a non-obvious reason, state it so the agent applies it correctly in edge cases
- **Skip linter-enforced rules** — don't repeat what formatters and linters already catch
- **Use Markdown links** to reference specific files or URLs for additional context

Recommended sections (adapt as needed):

1. **<Title>** — one-line heading describing the domain
2. **Context paragraph** — brief explanation of what this area is and why these rules matter
3. **Guidelines / Conventions** — bullet list of concrete rules
4. **Examples** — short code blocks showing do/don't patterns (optional)
5. **Key Files** — table of important files for orientation (optional)
6. **Common Pitfalls** — traps to avoid (optional)

### Step 5: Validate

After creating the file, verify:

- [ ] File is in `.github/instructions/` (or a configured instructions folder)
- [ ] Filename ends with `.instructions.md` and only contains lowercase letters, numbers, and hyphens
- [ ] YAML frontmatter is valid
- [ ] `applyTo` glob matches the intended files
- [ ] Content is concise (aim for under 500 lines or 5000 tokens) — long instructions dilute effectiveness
- [ ] No secrets, tokens, or internal URLs included
- [ ] Instructions don't duplicate what's already in `.github/copilot-instructions.md` or under `.agents/skills/`

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| `applyTo` too broad | Use specific globs; `**` applies to every file and wastes context |
| Missing `applyTo` | Without it, instructions won't auto-apply — they require manual attachment or semantic matching via `description` |
| Vague guidance | Replace "write good tests" with something like "add both positive and negative test cases using `[ConditionalFact]` methods" |
