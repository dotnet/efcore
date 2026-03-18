# Azure DevOps and Helix Reference

## Build Definition IDs (Example: dotnet/efcore)

Each repository has its own build definition IDs. Here are common ones for dotnet/efcore:

| Definition ID | Name | Description |
|---------------|------|-------------|
| `17` | efcore-ci | Main PR validation build |

**Note:** The script auto-discovers builds for a PR, so you rarely need to know definition IDs.

## Azure DevOps Organizations

**Public builds (default):**
- Organization: `dnceng-public`
- Project: `cbb18261-c48f-4abb-8651-8cdcb5474649`

**Internal/private builds:**
- Organization: `dnceng`
- Project GUID: Varies by pipeline

Override with:
```powershell
./scripts/Get-CIStatus.ps1 -BuildId 1276327 -Organization "dnceng" -Project "internal-project-guid"
```

## Common Pipeline Names (Example: dotnet/efcore)

| Pipeline | Description |
|----------|-------------|
| `efcore-ci` | Main PR validation build |

Other repos have different pipelines - the script discovers them automatically from the PR.

## Useful Links

- [Helix Portal](https://helix.dot.net/): View Helix jobs and work items (all repos)
- [Helix API Documentation](https://helix.dot.net/swagger/): Swagger docs for Helix REST API
- [Build Analysis](https://github.com/dotnet/arcade/blob/main/Documentation/Projects/Build%20Analysis/LandingPage.md): Known issues tracking (arcade infrastructure)
- [dnceng-public AzDO](https://dev.azure.com/dnceng-public/public/_build): Public builds for all dotnet repos

## Test Execution Types

### Helix Tests
Tests run on Helix distributed test infrastructure. The script extracts console log URLs and can fetch detailed failure info with `-ShowLogs`.

### Local Tests (Non-Helix)
Some repositories (e.g., dotnet/efcore and dotnet/sdk) run tests directly on the build agent. The script detects these and extracts Azure DevOps Test Run URLs.

## Known Issue Labels

- `Known Build Error` - Used by Build Analysis across all dotnet repositories
- Search syntax: `repo:<owner>/<repo> is:issue is:open label:"Known Build Error" <test-name>`

Example searches (use `search_issues` when GitHub MCP is available, `gh` CLI otherwise):
```bash
# Search in runtime
gh issue list --repo dotnet/runtime --label "Known Build Error" --search "FileSystemWatcher"

# Search in efcore
gh issue list --repo dotnet/efcore --label "Known Build Error" --search "SaveChanges"
```
