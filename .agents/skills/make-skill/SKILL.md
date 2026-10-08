---
name: make-skill
description: 'Create, review, or fix Agent Skills for GitHub Copilot. Use when asked to create, scaffold, improve, or repair a SKILL.md and its paired harness evaluation.'
---

# Create Or Fix A Skill

Produce concise, stable task guidance that adds knowledge an agent cannot reliably infer from source alone, together with a paired evaluation that proves the guidance changes behavior.

## Choose The Right Primitive

- Use a skill for a task-specific workflow or body of knowledge loaded on demand.
- Use file instructions for background guidance scoped to paths.
- Use a custom agent when context isolation or distinct tool restrictions are required.
- Prefer an existing skill over creating an overlapping one.

## Workflow

1. Read the relevant implementation, tests, repository instructions, and neighboring customizations.
2. Identify repeated mistakes or non-obvious invariants. Do not turn source inventories, line counts, or facts easily rediscovered from code into skill content.
3. Create `.agents/skills/<name>/SKILL.md`. The frontmatter `name` must exactly match the directory; the description must state what the skill does and when natural user requests should trigger it.
4. Mark background domain guidance `user-invocable: false`. Keep user-invocable workflows focused on an outcome a user would deliberately request.
5. Write actionable guidance with observable validation. Explain why edge cases matter; avoid generic engineering advice already present in repository instructions.
6. Create `eng/harness-evaluation/skills/<name>/eval.yaml` and follow `eng/harness-evaluation/README.md`.
7. Lint the customization and inspect both control and treatment artifacts before accepting it.

## Paired Evaluation

- The eval root `agent_environment.skills` contains only the target skill. Stimulus inputs belong under each stimulus's `agent_environment.files`.
- The runner removes the root skill for control and separately verifies treatment activation. Do not add a `skill-invocation` grader and do not tell the agent to invoke the skill in the shared prompt.
- Prompts may identify fixtures, proposed code, fixed IDs, and output paths, but must not reveal the expected diagnosis, owning mechanism, or regression-test design.
- Use deterministic graders for observable artifacts, a narrow semantic rubric for behavior, and a `token-budget` grader. Define bounded turns, tokens, and duration plus a committed scoring threshold.
- The task must exercise guidance distinctive to the skill. A control that consistently matches treatment means the skill or eval is not useful enough.

## Validation

- Frontmatter parses and the name matches the directory.
- The skill is concise, stable, and does not duplicate root or scoped instructions.
- File references are relative and exist.
- Validation steps are observable and task-specific.
- The paired eval targets only the skill, has complete bounded grading, and uses a solution-neutral prompt.
- `npm run lint` and the harness inventory tests pass.

## Common Pitfalls

| Pitfall | Correction |
|---|---|
| Copying implementation inventories into the skill | Keep only non-obvious invariants and routing decisions |
| Exact line counts or volatile metrics | Remove them; point to stable symbols or behavior |
| Generic test-folder guidance | Describe the distinctive test shape or omit it |
| Eval prompt tells the agent what to find | Move expected facts into the hidden rubric |
| Eval root contains repository fixtures | Keep only the target skill at root; stage fixtures per stimulus |
| Prompt instructs skill invocation | Let the treatment-only runner gate enforce activation |
| Initial scaffold launches multi-model review | First produce and lint the requested artifacts; run expensive behavioral validation separately |