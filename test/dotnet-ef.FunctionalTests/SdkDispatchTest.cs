// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "CLI")]
public class SdkDispatchTest(ToolingTestAssemblyFixture fixture)
{
    [Fact]
    public async Task Sdk_dispatches_to_checkout_tool()
    {
        await using var project = fixture.CreateProject(
            nameof(Sdk_dispatches_to_checkout_tool), kind: ToolingProjectKind.ProcessProbe);
        var result = await project.RunDotNetAsync(["ef", "--version"]);
        result.EnsureSuccess();
        var version = FileVersionInfo.GetVersionInfo(fixture.CliAssembly).ProductVersion!.Split('+')[0];
        Assert.Contains(version, result.StandardOutput + result.StandardError);
        project.MarkSuccessful();
    }

    [Theory]
    [InlineData("-d")]
    [InlineData("--data-annotations")]
    public async Task Sdk_preserves_scaffolding_annotation_option(string annotationOption)
    {
        await using var project = fixture.CreateProject(nameof(Sdk_preserves_scaffolding_annotation_option));
        await project.RestoreAsync();
        var result = await project.EfAsync(
            ["dbcontext", "scaffold", "Data Source=" + project.DatabasePath, "Microsoft.EntityFrameworkCore.Sqlite",
                annotationOption, "--context", "ScaffoldContext", "--namespace", "TestApp.Scaffolded",
                "--output-dir", "Scaffolded", "--table", "Blog Entries", "--no-onconfiguring"]);
        result.EnsureSuccess();
        var generated = Directory.GetFiles(
            Path.Combine(Path.GetDirectoryName(project.ContextProject)!, "Scaffolded"), "*.cs");
        Assert.Contains(generated, file => File.ReadAllText(file).Contains("[Table(\"Blog Entries\")]", StringComparison.Ordinal));
        (await project.BuildAsync()).EnsureSuccess();
        project.MarkSuccessful();
    }
}
