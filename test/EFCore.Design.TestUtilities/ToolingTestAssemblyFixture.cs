// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public sealed class ToolingTestAssemblyFixture : IAsyncLifetime
{
    private readonly IReadOnlyDictionary<string, string> _properties;
    private readonly Lazy<Task<string?>> _nativeAotSupport;

    public ToolingTestAssemblyFixture()
    {
        AssetsDirectory = Path.Combine(AppContext.BaseDirectory, "ToolingAssets");
        var manifest = Path.Combine(AssetsDirectory, "ToolingAssets.properties");
        _properties = File.ReadAllLines(manifest)
            .Where(line => line.Length != 0)
            .Select(line => line.Split('=', 2))
            .ToDictionary(line => line[0], line => line[1], StringComparer.Ordinal);
        ProductProjects = File.ReadAllLines(Path.Combine(AssetsDirectory, "ProductReferences.txt"));

        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        DotNetRoot = Path.GetFullPath(string.IsNullOrEmpty(dotnetRoot) ? Property("DotNetRoot") : dotnetRoot);
        DotNetPath = Path.Combine(DotNetRoot, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
        ResultsDirectory = Path.Combine(
            Property("ArtifactsDir"), "TestResults", Configuration, Assembly.GetEntryAssembly()!.GetName().Name!);
        Directory.CreateDirectory(ResultsDirectory);
        _nativeAotSupport = new Lazy<Task<string?>>(FindNativeAotSupportAsync);
    }

    public string AssetsDirectory { get; }
    public string DotNetRoot { get; }
    public string DotNetPath { get; }
    public string ResultsDirectory { get; }
    public string Configuration => Property("Configuration");
    public string TargetFramework => Property("TargetFramework");
    public string RepositoryRoot => Property("RepositoryRoot");
    public string CliAssembly => Property("CliAssembly");
    public IReadOnlyList<string> ProductProjects { get; }

    public async ValueTask InitializeAsync()
    {
        if (!Directory.Exists(Path.Combine(DotNetRoot, "sdk", Property("SdkVersion"))))
        {
            throw new DirectoryNotFoundException(
                $"DOTNET_ROOT '{DotNetRoot}' does not contain SDK {Property("SdkVersion")}. Select the bootstrapped SDK root.");
        }

        foreach (var file in new[]
                 {
                     DotNetPath,
                     Path.Combine(AssetsDirectory, OperatingSystem.IsWindows() ? "dotnet-ef.exe" : "dotnet-ef"),
                     Path.Combine(AssetsDirectory, "dotnet-ef.dll"),
                     Path.Combine(AssetsDirectory, "dotnet-ef.deps.json"),
                     Path.Combine(AssetsDirectory, "dotnet-ef.runtimeconfig.json"),
                     Property("CoreAssembly"),
                     Path.Combine(AssetsDirectory, "tools", "net", "ef.dll"),
                     Path.Combine(AssetsDirectory, "tasks", "net", "Microsoft.EntityFrameworkCore.Tasks.dll"),
                     Path.Combine(AssetsDirectory, "buildTransitive", "Microsoft.EntityFrameworkCore.Tasks.props"),
                     Path.Combine(AssetsDirectory, "buildTransitive", "Microsoft.EntityFrameworkCore.Tasks.targets")
                 })
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("Required tooling test input is missing.", file);
            }
        }

        var startInfo = new ProcessStartInfo(DotNetPath);
        startInfo.ArgumentList.Add("--version");
        var result = await ToolingProcess.RunAsync(
            startInfo, Path.Combine(ResultsDirectory, "sdk"), TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken)
            .ConfigureAwait(false);
        result.EnsureSuccess();
        if (result.StandardOutput.Trim() != Property("SdkVersion"))
        {
            throw new InvalidOperationException(
                $"Expected SDK {Property("SdkVersion")} at {DotNetPath}, got {result.StandardOutput.Trim()}.");
        }

        SQLitePCL.Batteries_V2.Init();
    }

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    public ToolingTestProject CreateProject(
        string name,
        ToolingProjectLayout layout = ToolingProjectLayout.SingleProject,
        ToolingProjectKind kind = ToolingProjectKind.Application)
        => new(this, name, layout, kind);

    public string Property(string name)
        => _properties.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value)
            ? value
            : throw new InvalidOperationException($"Missing tooling asset property '{name}'.");

    public string Source(string name)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestAssets", name));

    public Task<string?> NativeAotUnsupportedReasonAsync()
        => _nativeAotSupport.Value;

    public static string? WindowsNativeAotUnsupportedReason(
        string? installationPath,
        string? windowsSdkRoot,
        Architecture architecture)
    {
        var architectureName = architecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new ArgumentOutOfRangeException(nameof(architecture), architecture, "NativeAOT tests require x64 or Arm64.")
        };
        var toolComponent = architecture == Architecture.Arm64 ? "ARM64" : "x86.x64";
        var missing = new List<string>();
        var toolsDirectory = installationPath == null ? null : Path.Combine(installationPath, "VC", "Tools", "MSVC");
        if (installationPath == null
            || !File.Exists(Path.Combine(installationPath, "VC", "Auxiliary", "Build", "vcvarsall.bat"))
            || !Directory.Exists(toolsDirectory)
            || !Directory.EnumerateDirectories(toolsDirectory).Any(
                directory => new[] { "cl.exe", "link.exe", "lib.exe" }.All(
                    tool => File.Exists(Path.Combine(directory, "bin", "Host" + architectureName, architectureName, tool)))
                    && File.Exists(Path.Combine(directory, "lib", architectureName, "libcmt.lib"))))
        {
            missing.Add($"MSVC C++ {architectureName} build tools (Microsoft.VisualStudio.Component.VC.Tools.{toolComponent})");
        }

        var sdkLibraryDirectory = windowsSdkRoot == null ? null : Path.Combine(windowsSdkRoot, "Lib");
        if (!Directory.Exists(sdkLibraryDirectory)
            || !Directory.EnumerateDirectories(sdkLibraryDirectory, "10.*").Any(
                directory => File.Exists(Path.Combine(directory, "um", architectureName, "kernel32.lib"))
                    && File.Exists(Path.Combine(directory, "ucrt", architectureName, "ucrt.lib"))))
        {
            missing.Add($"Windows 10/11 SDK with {architectureName} Windows and Universal C Runtime libraries");
        }

        return missing.Count == 0
            ? null
            : "Missing NativeAOT prerequisites: " + string.Join("; ", missing) + "." + Environment.NewLine
                + "Open Visual Studio Installer, choose Modify for Visual Studio 2022 or later (or Build Tools), "
                + "and install 'Desktop development with C++' with its recommended components, including the MSVC build tools "
                + "and a Windows 10/11 SDK. For Arm64, also select C++ ARM64/ARM64EC build tools. "
                + "A C++ Redistributable alone is not sufficient. See test/EFCore.Design.TestUtilities/README.md "
                + "and https://aka.ms/nativeaot-prerequisites.";
    }

    private async Task<string?> FindNativeAotSupportAsync()
    {
        if (RuntimeInformation.ProcessArchitecture is not (Architecture.X64 or Architecture.Arm64))
        {
            return "NativeAOT tests require an x64 or Arm64 host.";
        }

        if (OperatingSystem.IsWindows())
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            var toolComponent = architecture == Architecture.Arm64 ? "ARM64" : "x86.x64";
            var windowsSdkRoot = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Kits\Installed Roots", "KitsRoot10", null) as string;
            var vswhere = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");
            if (!File.Exists(vswhere))
            {
                return WindowsNativeAotUnsupportedReason(null, windowsSdkRoot, architecture);
            }

            var startInfo = new ProcessStartInfo(vswhere);
            foreach (var argument in new[]
                     {
                         "-latest", "-prerelease", "-products", "*", "-version", "[17.0,)", "-requires",
                         "Microsoft.VisualStudio.Component.VC.Tools." + toolComponent,
                         "-property", "installationPath"
                     })
            {
                startInfo.ArgumentList.Add(argument);
            }

            var result = await ToolingProcess.RunAsync(
                startInfo, Path.Combine(ResultsDirectory, "native-toolchain"), TimeSpan.FromMinutes(1),
                TestContext.Current.CancellationToken).ConfigureAwait(false);
            result.EnsureSuccess();
            return WindowsNativeAotUnsupportedReason(
                string.IsNullOrWhiteSpace(result.StandardOutput) ? null : result.StandardOutput.Trim(), windowsSdkRoot, architecture);
        }

        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            return "This host OS is not supported by the NativeAOT test fixtures.";
        }

        return (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Any(directory => File.Exists(Path.Combine(directory, "clang")))
                ? null
                : "Missing NativeAOT prerequisites: install clang and the platform native development libraries. "
                    + "On Ubuntu, run 'sudo apt-get install clang zlib1g-dev'; on macOS, run 'xcode-select --install'. "
                    + "See https://aka.ms/nativeaot-prerequisites.";
    }
}
