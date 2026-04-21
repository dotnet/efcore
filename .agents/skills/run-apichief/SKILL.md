---
name: run-apichief
description: 'Run ApiChief in the EF Core repo to emit baselines, summaries, deltas, review files, or breaking-change checks. Use when refreshing `*.baseline.json`, preparing API review artifacts, or validating API changes.'
user-invocable: false
---

# Run ApiChief

Use the [ApiChief tool](../../../eng/Tools/ApiChief/README.md) to inspect or refresh EF Core public API baselines for projects under `src/`.

ApiChief can run against either a compiled assembly or a previously emitted baseline JSON file. Prefer the repo-local `.dotnet` SDK and the checked-in build scripts in this repo.

## Commands

| Command | Description | Extra arguments |
| --- | --- | --- |
| `emit baseline` | Emit a JSON API baseline | `-o <file>` |
| `emit summary` | Emit a human-readable API summary | `-o <file>`, `-x` |
| `emit review` | Emit API review files | `-o <dir>`, `-n` |
| `emit delta` | Emit a delta against an existing baseline, or markdown diff review files | `<baseline-path>`, `-o <file-or-dir>`, `--diff` |
| `check breaking` | Fail if breaking changes exist vs. a baseline | `<baseline-path>` |

Default to `emit baseline` if the user only asks to "run ApiChief".

## Workflow

### 1. Identify the target project(s)

- Match user intent against project folders under `src/` such as `EFCore`, `EFCore.Cosmos`, `EFCore.Relational`, or `Microsoft.Data.Sqlite.Core`.
- Ask for clarification only if the target is ambiguous.
- The checked-in baseline path convention in this repo is `src/<ProjectName>/<ProjectName>.baseline.json`.

### 2. Prepare the environment

Always initialize the repo environment before building or running the tool:

```powershell
.\restore.cmd
. .\activate.ps1
dotnet build .\eng\Tools\ApiChief\ApiChief.csproj --nologo --verbosity q
```

On Bash:

```bash
./restore.sh
. ./activate.sh
dotnet build ./eng/Tools/ApiChief/ApiChief.csproj --nologo --verbosity q
```

### 3. Ensure the target binaries exist

ApiChief operates on built assemblies. If the target project has not been built yet, build it first.

Preferred options:

```powershell
# Refreshes checked-in baselines after the normal build
.\build.cmd -c Debug

# Or build a single project when working narrowly
dotnet build .\src\<ProjectName>\<ProjectName>.csproj --nologo --verbosity q
```

Use the highest available `net*` TFM under `artifacts/bin/<ProjectName>/<Configuration>/` by default. Avoid `netstandard*` and `net4*` targets unless the user explicitly asks for them.

### 4. Locate the built ApiChief binary and target assembly

```powershell
$dotnet = ".\\.dotnet\\dotnet"
$apiChiefProject = ".\\eng\\Tools\\ApiChief\\ApiChief.csproj"
$apiChief = (& $dotnet msbuild $apiChiefProject --getProperty:TargetPath -p:Configuration=Debug --nologo).Trim()

$name = "<ProjectName>"
$tfm = Get-ChildItem ".\\artifacts\\bin\\$name\\Debug" -Directory |
    Where-Object { $_.Name -match '^net\d+\.\d+$' } |
    Sort-Object { [version]($_.Name -replace '^net', '') } -Descending |
    Select-Object -First 1 -ExpandProperty Name

$assemblyName = (& $dotnet msbuild ".\\src\\$name\\$name.csproj" --getProperty:AssemblyName -p:Configuration=Debug --nologo).Trim()
if ([string]::IsNullOrWhiteSpace($assemblyName))
{
    $assemblyName = $name
}

$assemblyPath = ".\\artifacts\\bin\\$name\\Debug\\$tfm\\$assemblyName.dll"
```

Before running, report the selected TFM if it matters for the task.

### 5. Run the requested ApiChief command

```powershell
# Emit a baseline
& $dotnet $apiChief $assemblyPath emit baseline -o ".\\src\\$name\\$name.baseline.json"

# Emit a summary
& $dotnet $apiChief $assemblyPath emit summary

# Emit a delta
& $dotnet $apiChief $assemblyPath emit delta ".\\src\\$name\\$name.baseline.json" -o ".\\artifacts\\tmp\\$name.delta.json"

# Check for breaking changes
& $dotnet $apiChief $assemblyPath check breaking ".\\src\\$name\\$name.baseline.json"

# Emit API review artifacts
& $dotnet $apiChief $assemblyPath emit review -o ".\\artifacts\\tmp\\API.$name"

# Emit GitHub-friendly markdown diff files against a baseline
& $dotnet $apiChief $assemblyPath emit delta ".\\src\\$name\\$name.baseline.json" --diff -o ".\\artifacts\\tmp\\API.$name.Diff"
```

`emit delta` also supports passing a `.json` file as the current input instead of a DLL.

### 6. Review or clean up baseline changes

After `emit baseline`:

- preserve the original BOM and trailing newline behavior of the committed file
- revert version-only diffs in the `"Name"` field
- inspect removed `//` instruction comments in previous baselines and carry forward any still-needed manual edits
- if those comments reference GitHub issues, check whether the underlying issue is still open before reapplying the workaround

### 7. Report results clearly

- list the processed project(s)
- show the chosen TFM(s)
- report the output path(s)
- for `check breaking`, state pass/fail
- for `emit delta` and `emit delta --diff`, mention that exit code `0` means changes, `2` means no changes, and `-1` means an error
- prefer `emit delta --diff` output because it generates ready-to-post ```diff fenced markdown split across per-type `.md` files
