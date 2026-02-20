---
description: This workflow backports PRs to servicing branches.

on:
  slash_command:
    name: "backport"
    events: [pull_request_comment]  # Only trigger in PR comments
  workflow_dispatch:

permissions:
  contents: read
  issues: read
  pull-requests: read

safe-outputs:
  create-pull-request:
    reviewers: [efteam art83]
    draft: true
    fallback-as-issue: false
    # TEMPORARY, until we get an org token
    github-token: ${{ secrets.ROJI_TOKEN }}

  # Allow adding a comment in case of failure
  add-comment:
    hide-older-comments: true    # hide previous comments from same workflow
    allowed-reasons: [outdated]
---

# Backport PR from main branch to servicing/release branches

This workflow analyzes an existing PR - typically merged to main - and submits a new PR which backports the same changes to a 

## Backport target branch

* If the user specifies a version to backport to, target the release branch for that version. For example, if the user executes `/backport 10.0.0`, submit a PR against branch `release/10.0`. Verify that the branch exists.
* If the user hasn't specified which branch to port to, find the latest release branch that corresponds to the pattern `release/XX.0` (where X is the greatest). Ignore other branches such as `release/11.0-preview2`.

## Quirk

A "quirk" is a .NET appcontext switch that we add in backport fixes which allows the user to opt out of the fix at runtime; this reduces the patch fix, as if a bug is introduced, users can simply disable the patch. Add a quirk for all cases where it makes sense; avoid adding a quirk when the fix is 100% obvious and risk-free, or when adding the quirk would simply cause another failure/exception to occur immediately.

To add a quirk:

* Add the following in the class (or classes) involving code changes:

```c#
private static readonly bool UseOldBehavior37585 =
    AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue37585", out var enabled) && enabled;
```

* Change the number (both in `UseOldBehavior37585` and in `Issue37585`) to refer to the relevant issue which the PR closes. If the PR closes multiple issues, find the most appropriate one.
* Wrap backported code changes with conditions such as `if (!UseOldBehavior37585)`, so that if the user activates the switch, the changes are bypassed and the code works exactly as it did before the patch.

## Testing

Running all the EF tests to validate the backport PR requires multiple databases to be present, which currently isn't configured in this workflow. For now, run unit tests only and allow the regular CI flow to detect any issues.

## PR content

The title of the PR should start with `[release/10.0]` - adjust based on the target branch name - followed by the exact title of the source PR.

The contents of the PR should strictly follow the following template:

<START_OF_TEMPLATE>
Fixes #<ISSUE_NUMBER>
Backports #<TRIGGERING_PR_NUMBER>

**Description**

Provide information on the bug, why it occurs, how and when it was introduced.

**Customer impact**

Information about how the bug affects actual users, without internal technical detail. If possible, post a short (3-4 lines) code sample that shows when the error occurs.

If the bug causes any sort of data corruption (incorrect data being saved or returned by EF), clearly and explicitly specify that as these bugs are severe. If the bug has no feasible workaround (or the workaround is hard or undiscoverable), mention that too.

**How found**

Read the originating issue to understand how the bug was discovered; if a user filed the issue, mention "User reported on <VERSION>", and if multiple users signaled that they're affected by the issue, mention that.

**Regression**

Whether this bug represents a regression from an earlier EF version or not.

**Testing**

State simply that "a test was added" if a test was indeed was added. If test coverage is unfeasible for some reason, briefly mention that and provide reasons why.

**Risk**

Post a brief risk assessment, and rank the risk from "extremely low" to "high", taking into account the amount of code changed. If the fix is basically a one-liner, mention that. If you've added a quirk as detailed above, mentioned "Quirk added".

</END_OF_TEMPLATE>

* The `ISSUE_NUMBER` value should be the issue closed by the originating PR (the one that is being backported). If the originating PR closes multiple issues, list those out separated by cmomas.
* The <TRIGGERING_PR_NUMBER> value should be the triggering PR, the one being backported.

## Additional instructions

If you encounter any error or issue, post a comment on the triggering PR detailing the problem(s) you encountered.
