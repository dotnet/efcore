# Anti-Patterns

Empirical pitfalls discovered during skill development. Every item here was hit in practice — not theoretical.

## Skill Design Anti-Patterns

### Re-documenting MCP tools
Skills that restate MCP tool parameter schemas or chain tool calls into rigid recipes create two sources of truth that drift when tools change. The agent already has tool descriptions in its context.

**The distinction**: Don't re-document tool schemas. Do provide examples that add context the tool description lacks.

```markdown
# ❌ Bad — restates tool schema, breaks when API changes
Use `hlx_batch_status` with `jobIds` (string array, max 50).
Use `hlx_status` with `filter: "all"` to include passed items.

# ❌ Bad — rigid recipe the agent can't adapt
1. Call `azure-devops-pipelines_get_builds` with `branchName`
2. Call `azure-devops-pipelines_get_build_log_by_id` with `logId: 5`
3. Call `mcp-binlog-tool-load_binlog` then `search_binlog`

# ✅ Good — example adds context missing from terse tool description
Query AzDO for builds on `refs/pull/{PR}/merge` branch.
⚠️ `sourceVersion` is the merge commit, not the PR HEAD.
Use `triggerInfo.'pr.sourceSha'` instead.

# ✅ Good — domain knowledge, not tool docs
For timed-out jobs, look for recoverable test results:
structured results (TRX) first, then search result files remotely,
then download as last resort.
```

**When examples ARE needed**: CLI tools and terse MCP tools often lack context that only domain experience provides — the branch ref pattern `refs/pull/{PR}/merge`, the checkout log location (typically log ID 5), the field name `triggerInfo.'pr.sourceSha'`. These are examples that teach *domain usage*, not tool mechanics.

**When examples are NOT needed**: Rich MCP tools (like Helix MCP) that document their parameters, return shapes, and usage patterns in their own descriptions. Don't restate what the agent already sees.

> 💡 **Use domain language, not tool names.** Say "search the console log for error patterns" instead of naming a specific tool. This creates a semantic connection to tool *descriptions* rather than a literal coupling to tool *names* — so skills work across MCP servers, CLIs, and API fallbacks without updating.

> 💡 **CLI examples can reinforce domain language.** Self-describing CLI commands like `gh issue view --comments` or `az pipelines list --name "X"` are *not* the same problem as opaque MCP tool names like `hlx_status`. CLI flags describe intent — models read them as "get issue with comments" and map to the best available tool (MCP, CLI, or API). Multi-model eval (3 families, 15/15 correct) confirmed models never copy-paste CLI examples when MCP tools are available. Strip opaque identifiers; keep self-describing examples.

**Rule of thumb**: If removing the example would leave the agent unable to accomplish the task even with the tool description in front of it, keep the example. If the tool description alone is sufficient, the example is redundant.

**What belongs in frontmatter only**: `INVOKES: tool-a, tool-b` — routing signals that help the LLM understand skill→tool relationships without duplicating tool docs.

> 📎 **For MCP server authors**: If you're designing the tools that skills consume, see `mcp-server-design` skill — covers tool description patterns, naming conventions, and knowledge tool architecture from the server author's perspective.

### Over-scripting
**Don't write scripts that replicate what agents already do.** Agents have `create`, `edit`, `task` (subagents), `powershell`, `gh` CLI, and dozens of other tools. A scaffold script that calls `New-Item` and writes template files is strictly worse than the agent doing it directly — the agent adapts to context, a script doesn't.

**When scripts ARE needed**: Complex logic (API pagination, data correlation, regex parsing), deterministic processing that benefits from being testable outside an agent, or operations that must work identically every time regardless of agent model.

### Bloated SKILL.md
A knowledge-driven SKILL.md can be large (stephentoub's is 54KB) but only if the content is **applied once per task** (like review rules applied to a diff). An orchestrating SKILL.md that guides multi-step work should be compact (2K–4K tokens) so the agent has context budget for the actual work.

**Fix**: Move depth to `references/*.md` — the agent loads them on demand.

### Vague trigger descriptions
```yaml
# ❌ Bad — won't trigger on real user queries
description: "A tool for analysis"

# ✅ Good — matches natural language intent
description: "Analyze CI build and test status from Azure DevOps and Helix for dotnet repository PRs. Use when checking CI status, investigating failures, or given URLs containing dev.azure.com."
```

### Missing "When to Use" section
Without explicit trigger scenarios, agents either invoke the skill too broadly or miss cases where it should activate. List 5-8 concrete scenarios with the keywords users would actually say.

### Temp files for intermediate data
```powershell
# ❌ BUG: Writing intermediate data to temp files triggers approval prompts
# in Copilot CLI, requires cleanup, and breaks if the agent crashes mid-task.
$data | ConvertTo-Json | Out-File "$env:TEMP/skill-cache.json"
$cached = Get-Content "$env:TEMP/skill-cache.json" | ConvertFrom-Json

# ✅ FIX: Use the SQL tool for structured intermediate data.
# Agent inserts, queries, and updates without approval prompts or cleanup.
```

**When this matters:** Orchestrating skills that discover items (files, PRs, test results) and track them across multiple phases or tool calls. The SQL tool is queryable (`WHERE status='pending'`), survives across tool calls, and needs no cleanup.

**When temp files are OK:** Script-driven skills where the script manages its own cache internally (e.g., ci-analysis's URL-hashed JSON cache) — the script handles creation, TTL, and cleanup atomically.

### Hardcoded lookup tables

Scripts that hardcode mappings between names and paths, IDs, or URLs go stale when the underlying data changes. Prefer data-driven discovery — parse the data source and match dynamically.

```powershell
# ❌ BUG: Hardcoded map goes stale when repos are added/renamed/removed.
$ComponentMap = @{
    "runtime"    = "src/runtime"
    "aspnetcore" = "src/aspnetcore"
    "roslyn"     = "src/roslyn"
}
$entry = $manifest.repos | Where-Object { $_.path -eq $ComponentMap[$name] }

# ✅ FIX: Search the data directly. Match by name, URI, or partial match.
$entry = $manifest.repos | Where-Object {
    $_.path -eq $name -or $_.remoteUri -match "/$name(\.\w+)?$" -or $_.path -like "*$name*"
}
```

**When hardcoding IS acceptable:**
- Values that are truly stable (pipeline IDs, org names, well-known URLs)
- Curated subsets where you intentionally limit options
- Performance-critical paths where fetching the data source adds substantial overhead

**When in doubt:** Prompt the user for the value rather than guessing from a stale table. Many ecosystems (dotnet repos, npm packages, GitHub orgs) follow consistent patterns — look for those patterns rather than enumerating instances.

### Syntax-only script testing

PowerShell syntax checks (`Parser::ParseFile`) catch typos but miss every bug that depends on runtime data — API response formats, field names, encoding quirks, null values from missing data.

```powershell
# ❌ FALSE CONFIDENCE: Script parses cleanly but crashes on real data.
# - GitHub Contents API returns base64 with embedded newlines
# - `gh api --jq '.content'` wraps output in quotes
# - source-manifest.json uses "runtime" not "src/runtime"
# None of these are syntax errors.

# ✅ FIX: After syntax check, run the script against a real (or recorded) API response.
# Even one real invocation catches entire classes of data-format bugs.
```

**Rule**: Any script that calls external APIs must be tested with at least one real invocation before shipping. Record the API response for future regression testing if the API is expensive or requires auth.

## PowerShell Anti-Patterns

### Array boolean coercion
```powershell
# ❌ BUG: Where-Object can return an array. $array.state -eq 'SUCCESS' 
# yields a boolean ARRAY, which is always truthy (even if all $false)
$checks = $data | Where-Object { $_.name -match 'Codeflow' }
if ($checks.state -eq 'SUCCESS') { ... }  # ALWAYS true if multiple matches!

# ✅ FIX: Force scalar with Select-Object -First 1
$check = @($data | Where-Object { $_.name -match 'Codeflow' }) | Select-Object -First 1
if ($check -and $check.state -eq 'SUCCESS') { ... }
```

### Nullable<T> unwrapping
```powershell
# ❌ BUG: PowerShell auto-unwraps Nullable<T> — .HasValue and .Value 
# silently return nothing
$lastMod = $resp.Content.Headers.LastModified  # Nullable<DateTimeOffset>
$published = $lastMod.Value.UtcDateTime  # Returns $null!

# ✅ FIX: Cast directly — PS unwraps the nullable for you
$published = ([DateTimeOffset]$lastMod).UtcDateTime
```

### Fail-open error handling
```powershell
# ❌ BUG: API failure counts as "healthy" because only conflict/staleness 
# are checked — Unknown result falls through to the else
$health = Get-Health -PR $pr
if ($health.HasConflict) { $blocked++ } else { $healthy++ }  # Unknown → healthy!

# ✅ FIX: Fail closed — Unknown is neither healthy nor blocked
if ($health.HasConflict) { $blocked++ }
elseif ($health.Status -notlike '*Unknown*') { $healthy++ }
# Unknown PRs are simply not counted
```

## GitHub API Anti-Patterns

### PowerShell string escaping in gh CLI
```powershell
# ❌ BUG: Backticks, $variables, and special chars get mangled by PowerShell
gh pr create --body "Added `flow status` and `$CheckMissing` keywords"
# Result: "Added low status and  keywords" (backticks eaten, $var expanded)

# ✅ FIX: Always use --body-file for multi-line or markdown content
$body | Out-File -FilePath "$env:TEMP/pr-desc.md" -Encoding utf8NoBOM
gh pr create --body-file "$env:TEMP/pr-desc.md"
Remove-Item "$env:TEMP/pr-desc.md"
```

This applies to `gh pr create`, `gh pr edit`, `gh issue create`, and any command taking markdown body text. The same problem occurs with `gh api graphql -f query=` — use heredoc strings or file-based input.

### Assuming field names
```powershell
# ❌ BUG: "conclusion" is not a valid field for gh pr checks
gh pr checks $PR --json name,state,conclusion  # Error: Unknown JSON field

# ✅ FIX: Always verify available fields first
gh pr checks $PR --json help  # Or trigger the error and read available fields
# Available: bucket, completedAt, description, event, link, name, startedAt, state, workflow
```

**Rule**: Never assume an API field exists based on other APIs or training data. Verify with `--help`, error output, or documentation.

### Not disposing HTTP responses
```powershell
# ❌ LEAK: Response and client never disposed in error paths
$client = [System.Net.Http.HttpClient]::new($handler)
$resp = $client.GetAsync($url).Result
if ($resp.StatusCode -ne 200) { return $null }  # Leaked!

# ✅ FIX: Dispose in finally blocks with null checks
try {
    $resp = $client.GetAsync($url).Result
    # ... use resp ...
} finally {
    if ($resp) { $resp.Dispose() }
    $client.Dispose()
    $handler.Dispose()
}
```

### Encoding reasoning into scripts
```powershell
# ❌ BUG: 130 lines of if/elseif producing canned recommendation text.
# Can't adapt to edge cases, closed PRs, partially-resolved states, or
# combinations the author didn't anticipate.
if ($conflict -and -not $resolved) { Write-Host "Resolve conflicts..." }
elseif ($stale -and $manual) { Write-Host "Merge as-is or force trigger..." }
elseif ($stale) { Write-Host "Close & reopen..." }
# ... 6 more branches ...

# ✅ FIX: Script emits structured facts. Agent reasons.
$summary = @{ conflict = $true; resolved = $false; stale = $true; manual = 3 }
Write-Host ($summary | ConvertTo-Json -Compress)
# SKILL.md teaches the agent: "Given conflict + not resolved → suggest resolve command"
```

**Rule**: If a script section is a chain of `if/elseif` branches producing prose text, that reasoning belongs in SKILL.md guidance, not in a script. Scripts collect data; agents reason over it.

## Agent Workflow Anti-Patterns

### Result fabrication
Agents will confidently report results they didn't actually produce. This was discovered in dotnet/maui PR #33733 where an agent ran **one** test command but reported "tests failed both with and without the fix" — which requires two separate runs.

```markdown
# ❌ BUG: Agent runs Gate verification inline, substitutes a simpler command,
# then fabricates the second test result it never ran.
# Example: Used BuildAndRunHostApp.ps1 (single run) instead of 
# verify-tests-fail-without-fix (dual run), then invented the second result.

# ✅ FIX: Force verification through a task agent (isolated context).
# The task agent can't improvise with other commands or access prior conversation.
# It runs exactly what's specified and reports only what actually happened.
```

**Rule**: Any skill that requires multiple independent observations (test-without-fix vs test-with-fix, before/after comparisons) must enforce isolation — either via task agents or scripts that perform both steps atomically. Never let the orchestrating agent run these inline where it can shortcut.

### Agents approving or blocking PRs
Without explicit prohibition, agents will eventually use `gh pr review --approve` or `--request-changes`. Both are human decisions.

```markdown
# ❌ DANGER: Agent approves its own fix or blocks a PR based on automated analysis
gh pr review --approve -b "LGTM, all tests pass"
gh pr review --request-changes -b "Found issues"

# ✅ FIX: Explicit NEVER rule in skill instructions + copilot-instructions.md
# Only `gh pr review --comment` is allowed for posting findings.
# Approval and blocking are human-only actions.
```

**Rule**: Any skill that interacts with PRs should include an explicit prohibition against `--approve` and `--request-changes`. Add this as a `🚨 CRITICAL` section near the top of the SKILL.md — not buried at the bottom. Discovered in dotnet/maui's pr-finalize skill.

### Agents switching branches during review
Agents will run `git checkout`, `git stash`, and other branch-switching commands during PR review, causing loss of local changes and confusion about which code is being reviewed.

```markdown
# ❌ BUG: Agent runs git checkout to "get the latest" or "switch to PR branch"
git checkout main && git pull  # Loses current work!
gh pr checkout 12345           # Changes working directory state!

# ✅ FIX: Agent is ALWAYS on the correct branch. Use git diff or gh pr diff 
# to see changes. User handles all branch operations.
```

**Rule**: Skills that *review or modify code* should state that the agent never runs git commands that change working directory state. However, *investigation skills* (CI analysis, codeflow status) may legitimately need to inspect other branches or repos — don't apply this rule to them.

### Agents endlessly troubleshooting environment blockers
Without retry limits, agents will spend 10+ tool calls trying to fix WinAppDriver, Appium, emulator boot failures, or port conflicts — none of which are the agent's job to fix.

```markdown
# ❌ BUG: Agent spends 15 tool calls trying to install and configure WinAppDriver
# instead of stopping after first failure.

# ✅ FIX: Explicit retry limits table in skill instructions:
# | Blocker Type         | Max Retries | Action              |
# |----------------------|-------------|---------------------|
# | Server errors (500)  | 0           | STOP immediately    |
# | Missing tools        | 1 install   | STOP and ask user   |
# | Port conflicts       | 1 kill      | STOP and ask user   |
# | Driver errors        | 0           | STOP immediately    |
```

**Rule**: Any skill that runs external tools (emulators, test servers, build infrastructure) should include explicit blocker handling with retry limits. The agent's job is to STOP and report, not to become a sysadmin.

## Review & Iteration Anti-Patterns

### Trusting training data over evidence
> "Never assert that something 'does not exist,' 'is deprecated,' or 'is unavailable' based on training data alone. Your knowledge has a cutoff date. When uncertain, ask rather than assert."
> — stephentoub's code-review skill

This applies to both writing skills AND reviewing them. If an automated reviewer claims an API doesn't exist, verify before accepting.

### Misdiagnosing flow PR failures as infrastructure
```
# ❌ MISDIAGNOSIS: "Package not found" on a codeflow PR → "feed propagation delay"
# Reality: The flowed code changed which package is requested.
# Example: SDK flow changed runtime pack resolution, causing builds to look for
# Microsoft.NETCore.App.Runtime.browser-wasm (CoreCLR — doesn't exist)
# instead of Microsoft.NETCore.App.Runtime.Mono.browser-wasm (correct).

# ✅ FIX: Always check WHICH package is missing and WHY it's being requested.
# Compare the package name against what the build used before the flow.
# If the package name itself changed, it's a code issue — not infrastructure.
```

### Accepting all reviewer suggestions uncritically
Automated reviewers have ~30-50% false positive rates on non-trivial code. For each suggestion:
1. Does the claim match reality? (Verify the specific API, field, behavior)
2. Does the suggested fix compile/work? (Test it)
3. Is it actually an improvement? (Sometimes the original code was correct)

Push back with evidence when the reviewer is wrong. This preserves correct behavior and builds a record of known false positives.

### Not counting unknown states as a category
When aggregating health/status across multiple items, always handle the "couldn't determine" case explicitly. Lumping unknowns into "healthy" hides real problems; lumping them into "unhealthy" creates false alarms. Track them separately.

### Stale PR descriptions
```
# ❌ BUG: PR description written at creation time, never updated.
# After 3 rounds of review: "When NOT to Use section" → actually renamed to "Script Limitations",
# PS 5.1 fix added, new tips added — none reflected in description.

# ✅ FIX: After every push, review the PR description against actual changes.
# Update via --body-file if anything drifted.
```

## Security Anti-Patterns

### Hardcoded credentials in scripts
Never embed API keys, tokens, or passwords in skill scripts. Use environment variables or the platform's credential store.

### Unvalidated inputs passed to shell commands
If a script constructs commands from user input or API responses, sanitize inputs. Avoid `Invoke-Expression` with untrusted strings.

### Scripts that ignore errors silently
Fail-closed: unknown ≠ healthy. If an API call fails, return "Unknown" status — don't count it as success or skip it silently. Use `$ErrorActionPreference = 'Stop'` or explicit try/catch blocks.

### Secrets committed to git history
Even if removed in a later commit, secrets in git history are extractable. Never commit tokens, keys, or credentials in skill scripts — even temporarily. Use `.gitignore` for files that may contain secrets, and rotate any key that was accidentally committed.

### Missing permission documentation
If a skill requires specific access (repo permissions, API tokens, org membership), document it in the Prerequisites section. Users shouldn't discover missing permissions at runtime. Follow least-privilege: if a skill only needs read access, don't request write.
