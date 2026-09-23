---
name: code-review
description: 'Review EF Core pull requests or local changes for concrete correctness, compatibility, performance, security, and test-coverage problems. Use when asked to review, critique, or check code changes before merge or submission.'
---

# EF Core Code Review

Review pull requests and local working-tree changes without modifying source. Findings must be concrete, evidence-backed problems introduced or exposed by the change, not style preferences or general improvement ideas.

## Scope and Safety

- Review the provided pull request, or staged, unstaged, and untracked local changes. Ask for clarification when both are plausible.
- Never checkout, switch branches, stash, reset, clean, or otherwise mutate the working tree, except for checking out the requested head for review purposes. Never edit source as part of a review.
- For a pull request, record its base, head, and immutable head SHA. If local `HEAD` differs, use read-only repository or GitHub context when sufficient; otherwise ask the user to checkout the requested commit.
- Never submit an `APPROVE` or `REQUEST_CHANGES` event. COMMENT emission follows Step 7.
- Do not use this skill to address existing review comments or implement fixes.

## Workflow

### 1. Establish the Review Surface

1. Load `.github/copilot-instructions.md` and any area skill whose description matches the changed implementation.
2. Obtain the complete diff and changed-file list once. Include staged, unstaged, and untracked files for local reviews.
3. For pull requests, gather the base/head refs and head SHA, but defer the PR body, linked issues, and prior review comments until Step 3.
4. Stop with a clear limitation if the requested change surface cannot be established.

### 2. Form an Independent Assessment

Before reading the author's narrative:

1. Read each changed implementation beyond its diff hunk. Diff-only review is insufficient evidence for a finding. For very large files, read the containing members, their contracts, and relevant state. When many files are changed, use subagents to break down the review into manageable parts and gather context incrementally.
2. Search callers, implementations, overrides, sibling provider paths, and nearby tests. Read shared helpers whose contracts control the changed behavior.
3. Use focused history when it can explain an invariant, revert, or prior fix attempt.
4. Describe the old and new behavior, likely motivation, affected callers/providers, and initial concerns in your own words.

### 3. Reconcile Pull Request Context

For pull requests, now read the description, linked issues, and existing review threads. Treat them as claims to verify against the code. Do not duplicate an existing finding unless the changed revision leaves it unresolved.

### 4. Route Conditional Reviews

- Read [references/api-review.md](references/api-review.md) when the change adds, removes, or changes supported public or protected API outside an `.Internal` namespace and not annotated with `[EntityFrameworkInternal]`.
- Read [references/security-review.md](references/security-review.md) when changed data crosses a documented trust boundary or the change affects serialization, generated code from database metadata, command construction, sensitive-data handling, caches or complexity bounds, assembly loading, elevated operations, or another high-risk path.

### 5. Analyze Behavior and Coverage

For each non-trivial production change, identify:

1. **Changed behavior**: the concrete old and new code paths.
2. **Affected surfaces**: callers, providers, public contracts, generated output, persistence, diagnostics, and concurrency where applicable.
3. **Regression risks**: plausible inputs, interleavings, failures, or compatibility breaks.
4. **Expected coverage**: the focused test that would fail without the change or expose the regression.
5. **Coverage gaps**: affected behavior not exercised by changed or existing tests.

Check correctness, error handling, async behavior, concurrency, state and resource lifetime, compatibility, provider hierarchy behavior, NativeAOT constraints, and performance only where the changed path plausibly makes them relevant.

For new functionality consider other feature areas that might be affected but are not directly touched by the change.

Do not request tests for documentation-only changes, comments, formatting, or demonstrably behavior-preserving mechanical changes. Do not flag issues that the compiler, formatter, or existing analyzers deterministically report.

### 6. Verify and Report Findings

Before reporting a concern:

- Verify it against surrounding and unchanged code, including caller/callee defenses.
- Identify a realistic failure or user impact. Do not promote a theoretical possibility with negligible likelihood into a finding.
- Verify factual claims about APIs from repository evidence or current documentation; never rely on model memory to claim an API is absent, deprecated, or unavailable.
- Report only test results actually observed. Separate unexecuted validation and residual risk from findings.

Order findings by severity:

- **Critical**: practical exploitation, arbitrary code execution, widespread data loss, or similarly catastrophic impact.
- **High**: merge-blocking correctness, security, compatibility, data-integrity, or concurrency defect.
- **Medium**: concrete defect with limited impact or a meaningful regression gap for changed behavior.
- **Low**: minor but real defect worth fixing in this change.
- **Info**: observations or suggestions that do not constitute a defect but may improve code quality or maintainability.

Each finding must contain:

1. Severity and concise title.
2. Certainty level or confidence in the finding based on the available evidence.
3. Changed file and line or symbol.
4. Concrete failure mode and impact.
5. Evidence that makes the finding actionable.
6. Fix direction and, when relevant, the missing regression-test shape.

Present findings first. If none meet the evidence bar, say **No findings** and then state any validation gaps or residual risk. Do not add praise or filler.

### 7. Optionally Post Pull Request Comments

Posting is allowed through either authorization path:

- **Direct invocation**: number the findings, let the user select or edit them, and obtain explicit confirmation before posting.
- **Code-reviewer agent delegation**: an invoking code-reviewer agent may explicitly authorize COMMENT emission and provide the selected findings or complete review payload. That caller contract is sufficient authorization; do not add a redundant confirmation prompt. An analysis-only delegation does not authorize posting.

Immediately before posting, confirm that the pull request is still open and its base, head, and head SHA match the reviewed revision. If they changed, stop and refresh the review context.

Create a pending review, add only authorized findings, and submit it as `COMMENT`. Never approve or request changes. Include a concise AI-generated disclosure when posting under a user's identity rather than a bot identity. Do not post findings that are similar to existing or resolved comments; for multiple defects in the same area, consolidate them into a single finding when appropriate.

## Validation

- Every finding resolves to changed code or a changed contract and cites supporting repository evidence.
- Test requests follow the behavior-impact analysis.
- No source or worktree mutation occurred.
- Any posted review used the reviewed head SHA and COMMENT-only authorization.

## Common Pitfalls

- Reviewing hunks without their callers or provider hierarchy.
- Treating a public CLR member in `.Internal` as supported public API.
- Reporting a security keyword without tracing attacker-controlled data to an impact.
- Inferring that passing broad tests covers the changed failure mode.
- Turning uncertainty into a firm finding instead of investigating or recording a limitation.