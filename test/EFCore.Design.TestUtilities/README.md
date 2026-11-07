# Tooling end-to-end tests

The managed [CLI runner](../dotnet-ef.FunctionalTests) and [MSBuild runner](../EFCore.Tasks.FunctionalTests) use this non-shipping library. Existing command unit tests and the legacy trimming/NativeAOT smoke projects remain separate.

## Inputs and SDK selection

[ToolingTestAssets.targets](ToolingTestAssets.targets) stages the checkout-built CLI apphost, inner tool, real task imports and task assembly in their consumed relative layout. Tests run before EF NuGet packages exist and must not fall back to installed tools or published EF packages.

Use the configured `DOTNET_ROOT`, which may be outside the checkout. The assembly fixture verifies the selected SDK version against the build manifest and propagates the root/PATH to child processes. The shared targets also set Arcade's `TestDotNetRoot` from that environment when no explicit test runtime root is supplied. Do not unconditionally activate or bootstrap another SDK.

Each test owns a sandbox, CLI home, database, and build/publish outputs. Staging records the resolved source-project reference closure. Samples explicitly reference that prebuilt closure using `ProjectReference` metadata `Targets="GetTargetPath"` and disable implicit transitive project references, which would otherwise rebuild products without that metadata. Ordinary references between sample projects still build normally. Restore each owned project with `--no-dependencies`. Cleaning explicitly disables reference cleaning and visits owned projects only.

Native SQLite loading has stricter Windows path limits than managed file APIs, so sandbox labels are shortened; `test-name.txt` retains the full name.

## Layouts

| Layout | References |
| --- | --- |
| Single project | Startup, context, queries, and migrations share one project. |
| Separate data | Startup references the data/context project, which also owns migrations. |
| Separate migrations | Startup references data and migrations; migrations references data. |
| Separate queries | The separate-migrations layout also has startup -> queries -> data. |

Runtime and design-time configuration select the same migrations assembly. There is no data -> startup/migrations inverse reference. CLI tests exercise both application startup and a factory-based migrations startup. Task imports belong in projects requiring generated code and in the startup's dependency traversal.

## xUnit v3 and deployment targets

Managed hosts share immutable assets through an assembly fixture, use async lifetimes, typed theory rows with labels/traits/skips, cancellation from `TestContext`, attachments, and bounded parallelism via `Xunit.v3.Parallelization`. Registration uses the non-generic assembly-fixture attribute to remain compatible with the repository's pinned analyzer.

Deployment test projects are [templates](TestAssets), not extra solution-level runners. Native targets use `xunit.v3.aot.mtp-v2` at the repository's xUnit version. The source generator discovers real EF facts; the host publishes and directly executes the resulting executable, checks executed/failed counts, and retains its TRX report. An unpublished `dotnet test` run is not native validation.

The repository mirror does not yet carry the pinned AOT package. Materialized deployment targets add a source-mapped NuGet.org feed for `xunit.*` only; other dependencies retain the repository feeds. This does not change the repository's global package-source policy or download EF packages.

Do not reference this managed helper assembly or reflection-mode xUnit/Arcade extensions from AOT targets. Share source assets instead. Native tests use built-in attributes, concrete methods, and AOT-compatible assertions/data; host string-based conditional-row/serialization infrastructure is not an AOT contract.

## Running

From the repository root in PowerShell, after restoring changed project manifests:

```powershell
$dotnet = Join-Path $env:DOTNET_ROOT 'dotnet.exe'
& $dotnet test --project .\test\dotnet-ef.FunctionalTests\dotnet-ef.FunctionalTests.csproj --report-trx
& $dotnet test --project .\test\EFCore.Tasks.FunctionalTests\EFCore.Tasks.FunctionalTests.csproj --report-trx
```

Use `--filter-trait Suite=Infrastructure`, `Suite=CLI`, `Suite=MSBuild`, `Suite=Trimming`, `Suite=NativeAot`, or `Suite=LegacyFramework` to narrow a run. Layout rows carry a `Layout` trait. Use the SDK's native MTP `--project` syntax, without an extra `--` separator.

### NativeAOT prerequisites

NativeAOT tests **fail**, rather than skip, when the required compiler/linker prerequisites are missing, both locally and on CI. No environment variable is needed to enable prerequisite failures. A minimal native xUnit target distinguishes the publishing/test infrastructure from experimental EF behavior. To run other suites without a native toolchain, explicitly exclude this group with `--filter-not-trait Suite=NativeAot`.

On Windows, install **Visual Studio 2022 or later**, or the standalone **Build Tools for Visual Studio**, with **Desktop development with C++** and its recommended components. In **Visual Studio Installer**, choose **Modify**, select the workload, and check these components in **Installation details**:

| Component | Installer ID / details |
| --- | --- |
| Desktop development with C++ | `Microsoft.VisualStudio.Workload.NativeDesktop` for the IDE; `Microsoft.VisualStudio.Workload.VCTools` for Build Tools. |
| MSVC C++ x64/x86 build tools (Latest) | `Microsoft.VisualStudio.Component.VC.Tools.x86.x64`; labeled **MSVC v143 - VS 2022 C++ x64/x86 build tools (Latest)** in Visual Studio 2022. Includes the native compiler, linker, librarian, and C/C++ runtime libraries. |
| Windows 10 or Windows 11 SDK | Include the Windows native libraries and Universal C Runtime libraries. For example, **Windows 11 SDK (10.0.26100)** is `Microsoft.VisualStudio.Component.Windows11SDK.26100`; the installed **10.0.22621** SDK (`Microsoft.VisualStudio.Component.Windows11SDK.22621`) is also suitable. |
| C++ ARM64/ARM64EC build tools (Latest), for an Arm64 test host | `Microsoft.VisualStudio.Component.VC.Tools.ARM64`; install the ARM64 tools in addition to the workload defaults. |

An installed Visual Studio IDE, the .NET SDK, or the **Visual C++ Redistributable** alone does not supply these native build tools. Prerequisite discovery includes Visual Studio preview installations, checks the host-architecture tool component and actual tool files, and verifies that an installed Windows SDK provides both Windows and UCRT native libraries.

Alternatively, download the [Visual Studio 2022 Build Tools bootstrapper](https://aka.ms/vs/17/release/vs_BuildTools.exe) and run this command from an elevated PowerShell prompt, accepting the installer's prompts and license terms:

```powershell
.\vs_BuildTools.exe --wait --passive --norestart --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended
```

For Arm64, append `--add Microsoft.VisualStudio.Component.VC.Tools.ARM64`. When modifying an existing IDE installation, use **Visual Studio Installer > Modify** rather than installing a second IDE. Restart the terminal after installation and rerun the NativeAOT suite.

On Ubuntu, install `clang` and `zlib1g-dev` (`sudo apt-get install clang zlib1g-dev`). On macOS, install the Xcode Command Line Tools (`xcode-select --install`). See the [.NET NativeAOT prerequisites](https://aka.ms/nativeaot-prerequisites) and [Visual Studio Build Tools workload/component catalog](https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools?view=vs-2022) for other platforms and installer component details.

GitHub runs portable suites on Windows/Linux/macOS and native suites on provisioned Windows/Linux agents. Arcade discovers the new functional projects on non-Helix agents. The helper library, templates, and checkout-dependent functional runners are explicitly excluded from Helix enumeration.

## Diagnostics and known limitations

Structured command metadata (arguments, working directory, and selected safe environment overrides), stdout/stderr logs, and MSBuild binlogs live below `artifacts/TestResults/<configuration>/<runner>/logs/`. Child TRX files are stored there too. Successful sandboxes are deleted without deleting diagnostics; failed inputs remain for reproduction. The runner drains both streams concurrently and kills only its owned process tree on cancellation/timeout.

Known product failures use individual test/row skips with an issue URL; ordinary command, restore, timeout, or assertion failures must never become skips. Current regressions recorded under [#33136](https://github.com/dotnet/efcore/issues/33136):

- Cold build-stage generation executes before the target assembly is copied to `bin`.
- Build-stage regeneration can consume the previous output assembly and retain old model metadata.

Warm-build scenarios are explicitly named and prepared as such; they do not replace the skipped cold-build contracts. Trimming/AOT diagnostics remain visible in command logs. There is no dedicated feature-switch suite or speculative feature-switch contract: workflows exercise the relevant product components end-to-end.

Compiled-model code intentionally disables nullable annotations for translated expressions. Nullable-setting tests check the resulting property nullability and compilation, not an assumed `#nullable enable` directive in generated code.

The minimal published xUnit target is copied to a system temporary directory and executed after its original publish directory is removed. Both trimmed and native infrastructure tests validate this relocation and preserve separate reports.

The complete SDK-driving harness intentionally remains checkout-dependent: samples evaluate source-project references to consume prebuilt outputs, and staging uses repository configuration. Removing those dependencies would require packaging the project/reference closure and restore metadata, beyond a small Copy-target extension. Published-target relocation is not claimed as full harness or Helix portability.
