// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "Infrastructure")]
public class NativeAotPrerequisitesTest(ToolingTestAssemblyFixture fixture)
{
    [Theory]
    [InlineData(Architecture.X64, false, true, "Microsoft.VisualStudio.Component.VC.Tools.x86.x64")]
    [InlineData(Architecture.Arm64, false, true, "Microsoft.VisualStudio.Component.VC.Tools.ARM64")]
    [InlineData(Architecture.X64, true, false, "Windows 10/11 SDK")]
    [InlineData(Architecture.Arm64, true, false, "Windows 10/11 SDK")]
    public async Task Missing_components_report_installation_instructions(
        Architecture architecture,
        bool toolsInstalled,
        bool sdkInstalled,
        string missingComponent)
    {
        await using var project = fixture.CreateProject(
            nameof(Missing_components_report_installation_instructions), kind: ToolingProjectKind.ProcessProbe);
        var (installation, sdk) = CreateToolchain(project.DirectoryPath, architecture);
        var reason = ToolingTestAssemblyFixture.WindowsNativeAotUnsupportedReason(
            toolsInstalled ? installation : null, sdkInstalled ? sdk : null, architecture);
        Assert.NotNull(reason);
        Assert.Contains(missingComponent, reason);
        Assert.Contains("Visual Studio Installer", reason);
        Assert.Contains("Desktop development with C++", reason);
        Assert.Contains("https://aka.ms/nativeaot-prerequisites", reason);
        project.MarkSuccessful();
    }

    [Theory]
    [InlineData(Architecture.X64)]
    [InlineData(Architecture.Arm64)]
    public async Task Complete_toolchain_is_accepted(Architecture architecture)
    {
        await using var project = fixture.CreateProject(
            nameof(Complete_toolchain_is_accepted), kind: ToolingProjectKind.ProcessProbe);
        var (installation, sdk) = CreateToolchain(project.DirectoryPath, architecture);
        Assert.Null(ToolingTestAssemblyFixture.WindowsNativeAotUnsupportedReason(installation, sdk, architecture));
        project.MarkSuccessful();
    }

    [Theory]
    [InlineData("VC\\Auxiliary\\Build\\vcvarsall.bat", true)]
    [InlineData("VC\\Tools\\MSVC\\14.44.0\\bin\\Hostx64\\x64\\link.exe", true)]
    [InlineData("VC\\Tools\\MSVC\\14.44.0\\lib\\x64\\libcmt.lib", true)]
    [InlineData("Lib\\10.0.26100.0\\um\\x64\\kernel32.lib", false)]
    [InlineData("Lib\\10.0.26100.0\\ucrt\\x64\\ucrt.lib", false)]
    public async Task Incomplete_installation_is_rejected(string missingFile, bool toolFile)
    {
        await using var project = fixture.CreateProject(
            nameof(Incomplete_installation_is_rejected), kind: ToolingProjectKind.ProcessProbe);
        var (installation, sdk) = CreateToolchain(project.DirectoryPath, Architecture.X64);
        File.Delete(Path.Combine(toolFile ? installation : sdk, missingFile.Replace('\\', Path.DirectorySeparatorChar)));
        var reason = ToolingTestAssemblyFixture.WindowsNativeAotUnsupportedReason(installation, sdk, Architecture.X64);
        Assert.NotNull(reason);
        Assert.Contains(toolFile ? "Microsoft.VisualStudio.Component.VC.Tools.x86.x64" : "Windows 10/11 SDK", reason);
        project.MarkSuccessful();
    }

    private static (string Installation, string Sdk) CreateToolchain(string root, Architecture architecture)
    {
        var installation = Path.Combine(root, "Visual Studio");
        var sdk = Path.Combine(root, "Windows Kits");
        var architectureName = architecture == Architecture.Arm64 ? "arm64" : "x64";
        var version = Path.Combine(installation, "VC", "Tools", "MSVC", "14.44.0");
        foreach (var file in new[]
                 {
                     Path.Combine(installation, "VC", "Auxiliary", "Build", "vcvarsall.bat"),
                     Path.Combine(version, "bin", "Host" + architectureName, architectureName, "cl.exe"),
                     Path.Combine(version, "bin", "Host" + architectureName, architectureName, "link.exe"),
                     Path.Combine(version, "bin", "Host" + architectureName, architectureName, "lib.exe"),
                     Path.Combine(version, "lib", architectureName, "libcmt.lib"),
                     Path.Combine(sdk, "Lib", "10.0.26100.0", "um", architectureName, "kernel32.lib"),
                     Path.Combine(sdk, "Lib", "10.0.26100.0", "ucrt", architectureName, "ucrt.lib")
                 })
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, "");
        }

        return (installation, sdk);
    }
}
