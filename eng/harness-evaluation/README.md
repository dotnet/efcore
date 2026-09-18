# Agent harness evaluations

This directory contains Vally evaluations for repository-owned Copilot instructions, skills, agents, agentic workflows, and prompts.

## Layout

- `instructions/<id>/eval.yaml` evaluates a repository instruction file.
- `skills/<skill-name>/eval.yaml` evaluates `.agents/skills/<skill-name>/SKILL.md`.
- `agents/<id>/eval.yaml` evaluates `.github/agents/<path>.agent.md` or `.md`.
- `workflows/<id>/eval.yaml` evaluates a `.github/workflows/<path>.md` agentic workflow source, not its compiled `.lock.yml`.
- `prompts/<id>/eval.yaml` evaluates `.github/prompts/<path>.prompt.md`.

Component IDs and eval paths are derived from these conventions. Nested instruction, agent, agentic-workflow, or prompt paths use `--` between path segments.

MCP servers and plugins are explicitly outside behavioral-eval coverage. Prefer shared plugins over repository-owned MCP servers or skills when one owns the capability. This repository enables `dotnet`, `dotnet-test`, `dotnet-diag`, `dotnet-msbuild`, and `dotnet-nuget` from `dotnet/skills`, plus `dotnet-helix`, `dotnet-codeflow`, and `dotnet-dnceng` from `dotnet/arcade-skills`. `dotnet-msbuild` supplies binlog tooling; enabling Arcade's `dotnet-binlog` too would register a second MCP named `binlog`. The Microsoft Docs plugin supplies the Microsoft Learn MCP endpoint and its usage skills. `eng/common/AGENTS.md` is generated Arcade guidance and is also outside the inventory.

## Authoring rules

Every stimulus must:

1. Set a positive `defaults.runs`. Use a parameterized default such as `${RUNS=5}` when local `--runs` overrides should be supported.
2. Declare the evaluated customization in the eval's root `agent_environment`: use `skills` for a skill and `files` for instructions, agents, workflows, or prompts.
3. Use stimulus-level `agent_environment.files` when repository inputs are needed. `src` is relative to the eval file; `dest` is relative to the isolated workspace.
4. Add a `token-budget` grader to every stimulus and define its weight in `scoring.weights`.
5. Use deterministic graders where possible and a narrow `prompt` rubric only for semantic quality.

The runner passes declared input files from the evaluated commit to Vally, which validates and stages them in an isolated workspace. Inputs are optional.

Skill evals load only their target skill and require a `skill-invocation` grader. The checked-in Vally experiment clears root `skills` and `files` for the unskilled control while the treatment inherits the eval's root environment.

## Acceptance

Pull request, post-merge harness, and manual evaluation use `vally experiment run` to execute the eval's configured number of unskilled-control and treatment trials, then use `vally compare` to judge paired trajectories. The treatment must meet the eval's committed `scoring.threshold`, comparison judging must complete, and `--fail-on-regression` rejects a statistically significant treatment regression. Reports include the treatment-relative quality verdict and mean token delta.

GitHub Actions pins the evaluation runner and dependencies to the repository default branch, then evaluates the authorized pull request from a separate candidate checkout. The candidate's customizations, eval specifications, and declared input files are data consumed by the trusted runner; candidate harness scripts are not executed.

The workflow discovers and labels affected PRs when they are opened, reopened, or marked ready for review. Component and eval changes are evaluated from authorized PRs, while harness infrastructure changes fan out to all components only after merging into the default branch. A contributor with write access can comment `/eval` to rerun all affected components or `/eval <component>` to rerun one. Manual dispatch from the repository default branch accepts a PR number, PR commit SHA and optional component filter.

## Local validation

From the repository root in PowerShell:

```powershell
# Restore the repository-managed .NET SDK and build dependencies.
.\restore.cmd

# Use the repository-managed .NET SDK.
. .\activate.ps1

Set-Location eng\harness-evaluation

# Install the Vally toolchain.
npm ci

# Exercise component discovery, selection, experiment output, and timeout logic.
npm test

# Check customization coverage and strict-lint all skills and source eval specifications.
npm run lint

# Run the configured treatment and control trials, then compare quality and token use.
node src/cli.mjs eval <component-id> --runs 5 --workers 1 --require-pass
```

Set `defaults.timeout` to five times the slowest observed trial, rounded up to the next five-minute boundary.

Behavioral runs use the Copilot SDK and require a valid local Copilot login or `COPILOT_GITHUB_TOKEN`. Results are written below `artifacts/TestResults/harness-evaluation/<component-id>/` as one timestamped experiment containing separate `control` and `treatment` variants, plus `comparison.jsonl`. They may contain prompts, model output, and tool payloads; do not commit them.

The runner defaults to one active trial per component. GitHub Actions parallelizes separate component jobs, so increasing Vally workers would multiply concurrent Copilot sessions and can cause session destruction or rate limiting.

Before accepting a new or materially changed eval, inspect both arms and `comparison.jsonl`. A control that consistently matches or beats the treatment means the eval is not discriminating enough.

## Adding components

- **Skill:** add `.agents/skills/<name>/SKILL.md` and `eng/harness-evaluation/skills/<name>/eval.yaml`.
- **Instruction, agent, agentic workflow, or prompt:** add the customization and a same-ID eval under the corresponding `eng/harness-evaluation/` directory.

The required coverage workflow rejects missing or orphaned repository-customization evals. MCP and plugin changes do not require eval coverage.
