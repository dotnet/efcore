# ApiChief

ApiChief helps EF Core manage public API baselines and review artifacts for compiled assemblies. It can emit a JSON baseline, produce a readable API summary, generate an API review folder, create a delta against an existing baseline, and fail on breaking changes.

## Summary

When run against an assembly, ApiChief:

- includes only the public/protected API surface
- filters out types in namespaces ending with `.Internal`
- filters out APIs marked with `[EntityFrameworkInternal]`

## Commands

### `emit baseline`

Creates a JSON API baseline for an assembly.

```console
dotnet ApiChief.dll MyAssembly.dll emit baseline -o MyAssembly.baseline.json
```

### `emit summary`

Creates a human-readable summary of the public API.

```console
dotnet ApiChief.dll MyAssembly.dll emit summary
```

Use `-o` to write to a file and `-x` to omit XML docs.

### `emit delta`

Creates a JSON delta between a current assembly or baseline JSON file and an existing baseline.

```console
dotnet ApiChief.dll MyAssembly.dll emit delta MyAssembly.baseline.json -o MyAssembly.delta.json
```

You can also compare two baseline files by passing a `.json` file as the first argument.

Exit codes:

- `0`: changes detected
- `2`: no changes detected
- `-1`: error

### `check breaking`

Fails if breaking changes are detected relative to a previous baseline.

```console
dotnet ApiChief.dll MyAssembly.dll check breaking MyAssembly.baseline.json
```

### `emit review`

Creates API review source files for the assembly.

```console
dotnet ApiChief.dll MyAssembly.dll emit review
```

Use `-o` to choose the output directory and `-n` to group output by namespace.
