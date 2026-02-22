---
name: make-skill
description: 'Create new Agent Skills for GitHub Copilot. Use when asked to create, scaffold, or add a skill. Generates SKILL.md with frontmatter, directory structure, and optional resources.'
---

# Create Skill

This skill helps you scaffold new agent skills that conform to the Agent Skills specification. Agent Skills are a lightweight, open format for extending AI agent capabilities with specialized knowledge and workflows.

## When to Use

- Creating a new skill from scratch
- Generating a SKILL.md file with proper frontmatter in compliance with agentskills.io specification

## When Not to Use

- Creating custom agents (use the agents/ directory pattern)
- Adding language-specific, framework-specific, or module-specific coding guidelines (use file-based instructions instead)

### Key Principles

- **Frontmatter is critical**: `name` and `description` determine when the skill triggers—be clear and comprehensive
- **Concise is key**: Only include what agents don't already know; context window is shared
- **Useful instructions**: Only include information that's stable, not easily searchable and can be used for any task within the skill's scope
- **No duplication**: Information lives in SKILL.md OR reference files, not both

## Workflow

### Step 1: Investigate the Topic

Build deep understanding of the relevant topics using the repository content, existing documentation, and any linked external resources.

After investigating, verify:
- [ ] Can explain what the skill does in one paragraph
- [ ] Can list 3-5 specific scenarios where the skill is applicable
- [ ] Can identify common pitfalls or misconceptions about the topic
- [ ] Can outline a step-by-step skill workflow with clear validation steps
- [ ] Have search queries for deeper topics
- [ ] Can determine if the skill should be user-invokable or background knowledge only

If there are any ambiguities, gaps in understanding, or multiple valid approaches, ask the user for clarification before proceeding to skill creation.
Also, evaluate whether the task might be better handled by a custom agent, agentic workflow, an existing skill or multiple narrower skills, and discuss this with the user if relevant.

### Step 2: Create the skill directory

```
.agents/skills/<skill-name>/
├── SKILL.md          # Required: instructions + metadata
```

### Step 3: Generate SKILL.md with frontmatter

Create the file with required YAML frontmatter:

```yaml
---
name: <skill-name>
description: <description of what the skill does and when to use it>
user-invokable: <Optional, defaults to true. Set to false for background knowledge skills.>
argument-hint: <Optional, guidance for how agents should format arguments when invoking the skill.>
disable-model-invocation: <Optional, set to true to prevent agents from invoking the skill and only allow to be used through manual invocation.>
compatibility: <Optional, specify any environment, tool, or context requirements for the skill.>
metadata: <Optional, key-value mapping for additional metadata that may be relevant for discovery or execution.>
allowed-tools: <Optional, list of pre-approved tools that agents could use when invoking the skill.>
---
```

### Step 4: Add body content sections

Include these recommended sections, following this file's structure:

1. **<Human-readable skill name>**: One paragraph describing the outcome beyond what's already in the description
2. **When to Use**: Bullet list of appropriate scenarios
3. **When Not to Use**: Bullet list of exclusions, optional
4. **Inputs and Outputs**: Example inputs and expected outputs, if applicable
5. **Workflow**: Numbered steps with checkpoints
6. **Testing**: Instructions for how to create automated tests for the skill output, if applicable
7. **Validation**: How to confirm the skill worked correctly
8. **Common Pitfalls**: Known traps and how to avoid them

### Step 5: Add and populate optional directories if needed

```
.agents/skills/<skill-name>/
├── SKILL.md
├── scripts/          # Optional: executable code that agents can run
├── references/       # Optional: REFERENCE.md (Detailed technical reference), FORMS.md (Form templates or structured data formats), domain-specific instruction files
└── assets/           # Optional: templates, resources and other data files that aren't executable or Markdown
```

### Step 6: Validate the skill

Ensure the name:
- Does not start or end with a hyphen
- Does not contain consecutive hyphens
- Is between 1-64 characters
- YAML frontmatter name matches directory name exactly

After creating a skill, verify:
- [ ] frontmatter fields are valid
- [ ] SKILL.md is under 500 lines and 5000 tokens, split into references if needed
- [ ] File references use relative paths
- [ ] Instructions are actionable and specific
- [ ] Instructions don't duplicate what's already in `.github/copilot-instructions.md` or under `.github/instructions/`
- [ ] Workflow has numbered steps with clear checkpoints
- [ ] Validation section exists with observable success criteria
- [ ] No secrets, tokens, or internal URLs included
- [ ] Common pitfalls are relevant and have solutions
- [ ] Optional directories are used appropriately
- [ ] Scripts handle edge cases gracefully and return structured outputs and helpful error messages when applicable

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| Description is vague | Include what it does AND when to use it |
| Instructions are ambiguous | Use numbered steps with concrete actions |
| Missing validation steps | Add checkpoints that verify success |
| Hardcoded environment assumptions | Document requirements in `compatibility` field |
| Key files section lists files previously mentioned | Avoid duplication, only include in one place and rename section to "Other Key Files" |
| Testing section lists test folders that are obvious from the repo structure | Remove the section if it doesn't add value |

## References

- [Agent Skills Specification](https://agentskills.io/specification)
- [Copilot Instructions](../../../.github/copilot-instructions.md)
- [Contributing Guidelines](../../../.github/CONTRIBUTING.md)
