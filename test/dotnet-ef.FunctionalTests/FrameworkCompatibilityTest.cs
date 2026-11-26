// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "LegacyFramework")]
public class FrameworkCompatibilityTest(ToolingTestAssemblyFixture fixture)
{
    [Fact]
    public async Task Net_framework_is_rejected_without_loading_inner_tool()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), ".NET Framework reference assemblies require Windows.");
        await using var project = fixture.CreateProject(
            nameof(Net_framework_is_rejected_without_loading_inner_tool), kind: ToolingProjectKind.ProcessProbe);
        project.SetProperty(project.StartupProject, "TargetFramework", "net48");
        project.WriteSource(project.StartupProject, "Program.cs", "internal static class Program { private static void Main() {} }");
        await project.RestoreAsync();
        var result = await project.EfAsync(["dbcontext", "info", "--framework", "net48", "--no-build"]);
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("don't support .NET Framework projects", result.StandardOutput);
        Assert.DoesNotContain("BadImageFormatException", result.StandardOutput + result.StandardError);
        Assert.DoesNotContain("Unhandled exception", result.StandardOutput + result.StandardError);
        Assert.Empty(Directory.GetFiles(project.DirectoryPath, "*ModelSnapshot.cs", SearchOption.AllDirectories));
        project.MarkSuccessful();
    }
}
