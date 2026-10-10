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
6. Present a realistic task without disclosing its solution. Prompts may name input files, proposed code, fixed artifact IDs, and required output paths, but must not state the expected diagnosis, owning symbol or stage, implementation mechanism, or regression-test design. Do not tell the agent to invoke the evaluated skill; treatment activation is runner-owned. Keep expected facts in the rubric and grade observable output that the prompt did not supply.
7. Use self-contained source and test snapshots for proposed changes, not `.diff` or `.patch` input fixtures. When comparison with prior behavior is necessary, freeze both `before/` and `proposed/` snapshots together; do not pair a frozen proposal with a live repository baseline. Generated trial workspace patches are output evidence, not eval fixtures.

The runner passes declared input files from the evaluated commit to Vally, which validates and stages them in an isolated workspace. Inputs are optional.

Skill evals load only their target skill. Do not add `skill-invocation` graders: shared graders score both variants, while the runner separately requires the exact target skill's activation in every treatment trial. The checked-in Vally experiment clears root `skills` and `files` for the unskilled control while the treatment inherits the eval's root environment.

 Keep only the target instruction in the eval root environment; stage source inputs and other repository guidance per stimulus so the control does not inherit the treatment.

## Acceptance

Pull request, post-merge harness, and manual evaluation use `vally experiment run` to execute the eval's configured number of unskilled-control and treatment trials, then use `vally compare` to judge paired trajectories. The treatment must meet the eval's committed `scoring.threshold`, comparison judging must complete, and `--fail-on-regression` rejects a statistically significant treatment regression. Reports include the treatment-relative quality verdict and mean token delta.

Control trials must complete execution and grading, but do not have to meet the scoring threshold. Fully scored experiments proceed to comparison even when a variant scores below threshold; missing trials and execution or grader errors remain failures.

### Comparison evidence

The wrapper prepares separate `comparison-input/control/results.jsonl` and `comparison-input/treatment/results.jsonl` files before calling `vally compare`. Vally 0.17.0 compares trajectory output, metrics, and timelines, not workspace files or patches. Its comparison CLI constructs its own judge config; stimulus `grader.config.prompt` and workspace-evidence selections do not configure that comparison. The wrapper forwards `defaults.judge_reasoning_effort` to comparison and supports `--judge-reasoning-effort`; reasoning effort does not increase evidence budgets.

Preparation includes explicit text-output requests such as `Write analysis.md`, `Create reports/review.md`, or `Save the report to report.md`, plus exact paths from `trajectory.stimulus.graders` of type `file-matches`. Paths shared by requests or graders are deduplicated and sorted. Preparation matches the result's `itemId` to saved trial `metadata.json` and reads that trial's final `workspace.patch`. Git's native, NUL-delimited `apply --numstat` output validates selected paths and each selected file section; no diff hunks are parsed by the wrapper. Only selected paths are applied in an empty temporary directory with isolated configuration. Newly created UTF-8 files are included as complete final content. An updated graded file such as a staged C# test is included as its actual unified diff when baseline contents are unavailable, explicitly labelled as a diff, NOT complete final content. Requested reports still require successful reconstruction and fail closed on baseline-dependent updates, even when also graded. Artifacts appear first in the copied trajectory's `output`, followed by the unchanged original final response. Grades, events, metrics, provenance, and original experiment files are not modified. Candidate checkout paths, trajectory `workDir`, and paths supplied in metadata are never used as evidence sources; staged inputs are not reread from mutable source checkouts.

Both runs use the same evidence policy. Reports are retained in full, not truncated to fit one judge read. Outputs over a 12,000-character single-read target are audited as requiring ranged reads; only the 1,000,000-character safety ceiling aborts preparation. If a requested or graded path has no change in the saved patch, its contents are explicitly marked unavailable: the artifact may be absent or unchanged. This preserves valid low-scoring controls without inventing evidence or rereading the candidate checkout. Missing native trial metadata or patches, unsafe paths, symlinks or junctions, submodules, binary patches, duplicate identities or sections, and malformed patches still fail preparation. Patches are limited to 8 MiB and 256 native file sections. Deletions, renames, copies, and baseline-dependent mode changes are unsupported. Exact grader paths, not globs, are required. Explicit report requests recognize `.md`, `.txt`, `.json`, `.patch`, or `.diff` filenames in write/create/save/produce/update directives. Ungraded source outputs are not inferred from links or tool calls.

`comparison-input/preparation.json` records source hashes, requested and graded paths, artifact kinds (`final-content`, `diff`, or `unavailable`), output lengths, and whether ranged reads are needed. Original results and patches remain the audit source; grades, timelines, completion, activation, regression, and conservative position-swap gates are unchanged. Preparation neither regrades nor selects favorable verdicts. It does not control the judge's read order or its 64,000-character total retrieval budget. Fewer inconsistent behavioral verdicts require a behavioral rerun to establish; unit tests verify evidence delivery and policy preservation.

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
node src/cli.mjs eval <component-id> --workers 1 [--require-pass] [--runs 5] [--model <model-name>] [--judge-model <model-name>] [--judge-reasoning-effort medium]
```

Set `defaults.timeout` to five times the slowest observed trial, rounded up to the next five-minute boundary.

### Timeout and recovery controls

`constraints.max_duration` overrides `defaults.timeout` for agent execution. Increase the stimulus limit as well as the default when an agent hits its execution timeout; increasing only the default does not change that trial's effective budget.

Judge timeouts are separate. Vally 0.17.0's Copilot LLM client defaults to 120 seconds per `sendAndWait`; eval execution limits do not extend it. The native CLI does not expose a judge-timeout flag. Timeout-prone evals use `judge_reasoning_effort: ${JUDGE_REASONING_EFFORT=medium}`. The wrapper's `--judge-reasoning-effort` supplies that parameter and the environment fallback for experiment grading, and the native flag for paired comparison. Keep one worker per component; do not multiply concurrent sessions to recover latency. The public LLM client API also accepts `timeoutMs` on `judge()` calls, so an isolated offline recovery can use a longer limit without changing the installed package.

Retry only execution or grader infrastructure failures, never valid low grades. Offline regrading must use the original saved rubric, trajectory, inputs, and final workspace patch. A later input-file timestamp alone is not evidence of changed content: complete captured file reads can reconstruct and verify the original bytes. Do not substitute today's fixtures when their content differs or the original bytes cannot be established. Preserve original results, replace only failed grader results in a separate recovery copy, retain static grades and activation/completion gates, and recompute comparisons from that copy.

The Cosmos transactional-batch eval is offline code generation. It needs no running emulator or database; only the generated functional test is intended to run against Cosmos later. Do not launch infrastructure or execute functional tests during this eval.

Behavioral runs use the Copilot SDK and require a valid local Copilot login or `COPILOT_GITHUB_TOKEN`. Results are written below `artifacts/TestResults/harness-evaluation/<component-id>/` as one timestamped experiment containing separate `control` and `treatment` variants, plus `comparison.jsonl`. They may contain prompts, model output, and tool payloads; do not commit them.

### Agent execution

`cli.mjs eval` produces large progress and grading reports. Agents must use their execution environment's background mode and redirect stdout and stderr to files; do not stream command output into the conversation context. A PowerShell fallback from `eng/harness-evaluation` is:

```powershell
$component = '<component-id>'
$logDirectory = '..\..\artifacts\TestResults\harness-evaluation-logs'
New-Item -ItemType Directory -Force $logDirectory | Out-Null

$process = Start-Process node -ArgumentList @(
    'src/cli.mjs', 'eval', $component,
    '--workers', '1', '--require-pass', '--runs', '5'
  ) -RedirectStandardOutput (Join-Path $logDirectory "$component.log") `
    -RedirectStandardError (Join-Path $logDirectory "$component.err.log") `
    -PassThru
$process.Id
```

Wait for process completion or a terminal completion notification; do not repeatedly poll or print the growing log. The component artifact root is `..\..\artifacts\TestResults\harness-evaluation\<component-id>`. Then inspect, in order:

1. `validation.json` for the treatment quality and activation gates.
2. `comparison.jsonl` for aggregate quality, grader flips, and token deltas.
3. The newest timestamped experiment's `control/results.jsonl` and `treatment/results.jsonl` for per-stimulus failures.
4. The redirected `.err.log`, then targeted lines from `.log`, only when the summary artifacts are missing or incomplete.

Project only the fields needed for diagnosis. Do not dump complete `results.jsonl`, `events.jsonl`, workspace patches, or redirected logs into the conversation context.

The runner defaults to one active trial per component. GitHub Actions parallelizes separate component jobs, so increasing Vally workers would multiply concurrent Copilot sessions and can cause session destruction or rate limiting.

Before accepting a new or materially changed eval, inspect both arms and `comparison.jsonl`. A control that consistently matches or beats the treatment means the eval is not discriminating enough.

Repository skills must demonstrate comparative value, not merely pass their treatment rubric. If a skill consistently fails five-run comparison evals and can't be reasonably improved, remove both the skill directory and its paired eval rather than lowering thresholds, weights, or semantic requirements. Move its paired eval stimuli to `eng\harness-evaluation\instructions\copilot-instructions\`.

## Adding components

- **Skill:** add `.agents/skills/<name>/SKILL.md` and `eng/harness-evaluation/skills/<name>/eval.yaml`.
- **Instruction, agent, agentic workflow, or prompt:** add the customization and a same-ID eval under the corresponding `eng/harness-evaluation/` directory.

The required coverage workflow rejects missing or orphaned repository-customization evals. MCP and plugin changes do not require eval coverage.
