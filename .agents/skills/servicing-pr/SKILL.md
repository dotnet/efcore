---
name: servicing-pr
description: 'Create EF Core PRs targeting servicing release branches (release/*). Use when working on a PR that targets a release branch, backporting a fix, or when the user mentions servicing, patch, or release branch.'
---

# Servicing PRs

PRs targeting `release/*` branches require a specific description format and should include a quirk (AppContext switch) when applicable.

## When to Use

- Creating a PR that targets a `release/*` branch
- Backporting a fix from `main` to a servicing branch

## Backport Target Branch

- If a version is specified, target `release/XX.0` (e.g., `backport to 10` → `release/10.0`)
- Otherwise, find the latest `release/XX.0` branch (ignore preview branches like `release/11.0-preview2`)
- Verify the branch exists before creating the PR

## PR Title

`[release/XX.0] <original PR title>`

## PR Description Template

```
Fixes #{issue_number}
Backports #{source_pr_number_if_applicable}

**Description**
Provide information on the bug, why it occurs and how it was introduced. Put it in terms that developers can understand without knowledge of EF Core internals.

**Customer impact**
How the bug affects users without internal technical detail. Include a short (3-4 lines) code sample if possible. If data corruption occurs, state that explicitly. If a feasible workaround exists, briefly describe it and how discoverable it is, otherwise mention that there's no workaround.

**How found**
How the bug was discovered based on the information in the issue description. If user-reported, mention "User reported on <version>". If multiple users are affected, note that. Count the number of upvotes and comment authors to estimate impact.

**Regression**
Whether this is a regression from an earlier EF version. Add a link to the PR that introduced the regression if known. If it only affects a feature introduced in the same major version, it is not a regression.

**Testing**
State the number of new or modified tests, don't go into details. If test coverage is unfeasible, briefly explain the alternative validation approach that was used.

**Risk**
Brief risk assessment ranked from "extremely low" to "high". Note amount of code changed. If the fix is a one-liner, mention that. If a quirk was added, mention "Quirk added".
```

## Quirk (AppContext Switch)

A quirk lets users opt out of the fix at runtime, reducing patch risk. Add for all cases where it makes sense. Skip when the fix is 100% obvious/risk-free, or when the quirk couldn't be used, like in tools or analyzers.

### Adding a Quirk

Add in the class(es) containing code changes:

```csharp
private static readonly bool UseOldBehavior37585 =
    AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue37585", out var enabled) && enabled;
```

- Change `37585` to the relevant issue number
- Wrap changes with a condition on `!UseOldBehavior37585` so activating the switch bypasses the fix, prefer to minimize the number of times the switch is checked
- If the PR closes multiple issues, pick the most appropriate one for the switch name

## Validation

- PR description follows the template completely (all sections filled)
- Quirk added where appropriate with issue number matching the PR description
