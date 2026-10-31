---
name: make-github-actions-workflow
description: 'Create GitHub Actions workflows for CI, automation, or PR management. Use when asked to create, scaffold, or add a GitHub Actions workflow (.yml file under .github/workflows/).'
---

# Create GitHub Actions Workflow

Create and configure GitHub Actions workflows that follow this repository's conventions and patterns. Workflows automate tasks like CI validation, PR management, issue labeling, and notifications.

## Repository Conventions

This repository uses specific patterns across all workflows. Follow these conventions when creating new workflows:

### File Location and Naming

- All workflows live in `.github/workflows/`
- Use kebab-case names: `label-and-milestone-issues.yml`, `validate-pr-target-branch.yml`
- Start with a descriptive comment block explaining the workflow's purpose

### Runner

- Use `ubuntu-latest` for general-purpose jobs
- Use `ubuntu-24.04` when a specific OS version is needed (e.g., copilot setup steps)

### Permissions

- **Always** declare explicit permissions — never rely on defaults
- Use **minimal permissions**: only request what the workflow needs
- Common patterns:
  - Read-only: `permissions: {}`
  - PR comments: `pull-requests: write`
  - Issue management: `issues: write`
  - Code read: `contents: read`
- For workflows that need write access to PRs from external contributors, use `pull_request_target` instead of `pull_request`
  - **Security warning:** `pull_request_target` workflows run in the base repository context with its permissions. Do **not** check out or execute code from the PR branch in these workflows. Instead, use the event payload and/or GitHub API (`actions/github-script`, `github.rest.*`, etc.) to inspect and act on the PR safely.

### Scripting

- Use `actions/github-script@v8` for complex logic instead of shell scripts
- Write JavaScript with `async/await` — the `github` object (Octokit) and `context` are available
- Use `github.rest.*` for REST API calls and `github.graphql()` for GraphQL queries
- Access PR/issue data via `context.payload.pull_request`, `context.repo.owner`, `context.repo.repo`
- Log with `console.log()` for debugging; use `core.setFailed()` for errors

### Error Handling

- Wrap API calls in try/catch blocks
- Collect errors and report at the end rather than failing on the first error
- Log skipped operations with a reason

## Workflow

### Step 1: Understand the Requirements

Before writing the workflow, clarify:
- [ ] What event(s) should trigger it? (push, pull_request, check_suite, schedule, etc.)
- [ ] What permissions are needed?
- [ ] What conditions should gate execution? (author, branch, labels, etc.)
- [ ] What API calls are needed?
- [ ] Are there rate limiting or idempotency concerns?

### Step 2: Choose the Trigger Event

| Use case | Event | Notes |
|----------|-------|-------|
| PR opened/updated | `pull_request` | Read-only access to PR |
| PR with write access | `pull_request_target` | Can comment, label, close |
| PR merged | `pull_request_target: [closed]` | Check `github.event.pull_request.merged == true` |
| CI checks complete | `check_suite: [completed]` | Filter `app.slug != 'github-actions'` to skip self |
| Code pushed | `push` | Filter by branches/paths |
| Manual trigger | `workflow_dispatch` | Add `inputs:` for parameters |
| Scheduled | `schedule` | Use cron syntax |

### Step 3: Write the Workflow

Create the YAML file following this template:

```yaml
# Description of what this workflow does and why.

name: Descriptive Workflow Name

on:
  <trigger_event>:
    types: [<event_types>]

permissions:
  <resource>: <read|write>

jobs:
  <job-name>:
    if: <guard_condition>  # Optional: skip when not needed
    runs-on: ubuntu-latest
    steps:
      - name: Descriptive step name
        uses: actions/github-script@v8
        with:
          script: |
            // Your logic here
```

### Step 4: Implement Guard Conditions

Add conditions to avoid unnecessary runs:
- Job-level `if:` for event payload checks (e.g., `github.event.check_suite.app.slug != 'github-actions'`)
- Early `return` in scripts for PR author, draft status, etc.
- Idempotency checks (e.g., hidden HTML comments as tags to prevent duplicate actions)

### Step 5: Validate

After creating the workflow:
- [ ] Permissions are minimal and explicitly declared
- [ ] Uses `actions/github-script@v8` for complex logic
- [ ] Guard conditions prevent unnecessary runs
- [ ] API calls handle pagination where needed (use `per_page: 100`)
- [ ] Error handling doesn't silently swallow failures
- [ ] No secrets or tokens are hardcoded
- [ ] Workflow doesn't create infinite loops (e.g., a workflow that triggers itself)

## Existing Workflows in This Repository

Study these workflows as reference:

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `validate-pr-target-branch.yml` | `pull_request_target: [opened, edited, reopened]` | Closes external PRs targeting release branches, adds community labels |
| `label-and-milestone-issues.yml` | `pull_request_target: [closed]` | Labels and milestones issues when their fixing PR is merged |
| `copilot-setup-steps.yml` | `workflow_dispatch`, `push`, `pull_request` | Sets up the development environment for Copilot |
| `inter-branch-merge-flow.yml` | `push` to `release/**` | Triggers inter-branch merge via arcade |

## Common Patterns

### Commenting on a PR

```javascript
await github.rest.issues.createComment({
  owner: context.repo.owner,
  repo: context.repo.repo,
  issue_number: prNumber,
  body: 'Comment text'
});
```

### Checking User Permissions

```javascript
const { data: permissions } = await github.rest.repos.getCollaboratorPermissionLevel({
  owner: context.repo.owner,
  repo: context.repo.repo,
  username: context.actor
});
const hasWriteAccess = ['admin', 'write'].includes(permissions.permission);
```

### Preventing Duplicate Comments (SHA Tag Pattern)

```javascript
const shaTag = `<!-- tag: ${headSha} -->`;
const { data: comments } = await github.rest.issues.listComments({
  owner: context.repo.owner,
  repo: context.repo.repo,
  issue_number: prNumber,
  per_page: 100
});
if (comments.some(c => c.body?.includes(shaTag))) {
  console.log('Already processed');
  return;
}
```

### Reading Repository Files at a Specific Commit

```javascript
const { data: fileData } = await github.rest.repos.getContent({
  owner, repo,
  path: 'eng/Versions.props',
  ref: commitSha
});
const content = Buffer.from(fileData.content, 'base64').toString('utf-8');
```

## References

- [GitHub Actions documentation](https://docs.github.com/en/actions)
- [Workflow syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [Events that trigger workflows](https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows)
- [actions/github-script](https://github.com/actions/github-script)
