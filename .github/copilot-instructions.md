# Entity Framework Core - GitHub Copilot Instructions

Read implementations and usages before changing behavior; similarly named components often differ between the core, relational, and provider layers. If intent remains unclear, do not infer it from names alone.

## Build and test

The repository bootstraps its pinned preview .NET SDK into `.dotnet`. On Windows, use `.\restore.cmd` followed by `. .\activate.ps1`. On Linux/macOS, use `./restore.sh` followed by `. ./activate.sh`.

.\build.{cmd|sh} restores and builds the solution; .\test.{cmd|sh} builds and runs the full test suite.

Tests use Microsoft.Testing.Platform with xUnit.

```powershell
dotnet test .\test\EFCore.Tests\EFCore.Tests.csproj -- --filter-method "*ModelBuilderTest.Some_test"
```

`test\Directory.Build.props` adds `--filter-not-trait category=failing --ignore-exit-code 8`. Do not add `--no-build` unless the test assembly was built immediately beforehand.

SQL Server functional tests use LocalDB on Windows or `Test__SqlServer__DefaultConnection`. Cosmos tests use `Test__Cosmos__DefaultConnection` or start the configured emulator/container. Tests requiring unavailable services may skip.

Set `EF_TEST_REWRITE_BASELINES=1` to rewrite SQL and compiled-model baselines. Public API changes require running `.\test\EFCore.ApiBaseline.Tests\EFCore.ApiBaseline.Tests.csproj`.

## Architecture overview

- `src\EFCore.Abstractions` contains minimal contracts; `src\EFCore` contains provider-independent functionality.
- `src\EFCore.Relational` adds the shared functionality for relational providers.
- `src\Microsoft.Data.Sqlite.Core` is a standalone ADO.NET provider.
- `src\EFCore.Design` implements design-time functionality. `src\dotnet-ef` is the CLI front end wrapping `src\ef`; `EFCore.Tools` is the Package Manager Console integration; `EFCore.Tasks` supplies MSBuild integration.

## Repository conventions

- Put provider-independent behavior in core or relational specification tests, then override new virtual tests in inheriting provider class adding specific assertions such as `AssertSql` for providers that produce SQL. `Check_all_tests_overridden` detects missing overrides.
- Prefer existing test infrastructure: `TestHelpers` for services/models, `NonSharedModelTestBase` for both the tests that share a model as well as those that do not.
- Preserve public API and binary compatibility. Prefer overloads over changing shipped signatures. If you need to break a public API, add a new API instead and mark the old one as obsolete. Use `ObsoleteAttribute` with the message pointing to the new API
- Types are public by default. Types under `.Internal` or marked `[EntityFrameworkInternal]` must use the repository's internal-API XML documentation pattern on all members and they don't need to preserve compatibility.
- User-facing messages come from the owning project's `.resx` resource and generated `*Strings.Designer.cs`.
- Configure asynchronous calls using `ConfigureAwait(false)`. Avoid reflection or runtime code generation where NativeAOT-compatible alternatives exist; otherwise use the established annotations/guards.
- Package versions belong in `eng\Versions.props` or `Directory.Packages.props`, never inline in project files.
- Do not edit `eng\common`; it is mirrored from dotnet/arcade and overwritten by automation.
- Follow `.editorconfig` for formatting.
- `bool` parameters should not begin with "is" or "are".
- Prefer minimal comments; use `Check.DebugAssert` when the intent is an invariant rather than explanatory prose.

## Domain guidance

Skill files in `.agents\skills\<area>\SKILL.md` provide domain-specific knowledge. Keep skills updated: when you discover non-obvious patterns or recurring review feedback during a session, distill the insight into the relevant `SKILL.md`. Additions must be concise, broadly useful, and stable. Avoid task-specific details, speculation, and statements that contradict existing content. Remove or correct stale information rather than appending conflicting rules.
