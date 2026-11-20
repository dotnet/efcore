// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "CLI")]
public class ProjectSelectionTest(ToolingTestAssemblyFixture fixture)
{
    public static IEnumerable<TheoryDataRow<ToolingProjectLayout>> Layouts => ToolingTestData.Layouts;

    [Theory]
    [MemberData(nameof(Layouts))]
    public async Task Uses_context_from_selected_project_graph(ToolingProjectLayout layout)
    {
        await using var project = fixture.CreateProject(nameof(Uses_context_from_selected_project_graph), layout);
        await project.RestoreAsync();
        using var data = CommandData.ReadJson(await project.EfAsync(["dbcontext", "info", "--json", "--framework", fixture.TargetFramework]));
        Assert.Equal("TestApp.BlogContext", data.RootElement.GetProperty("type").GetString());
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", data.RootElement.GetProperty("providerName").GetString());
        Assert.Equal(project.DatabasePath, data.RootElement.GetProperty("dataSource").GetString());
        project.MarkSuccessful();
    }

    [Fact]
    public async Task No_build_uses_existing_output_and_forwards_application_arguments()
    {
        await using var project = fixture.CreateProject(
            nameof(No_build_uses_existing_output_and_forwards_application_arguments), ToolingProjectLayout.SeparateData);
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        project.WriteSource(project.StartupProject, "Program.cs", "this is deliberately invalid C#");
        string[] arguments = ["argument with spaces", "--factory-option=value"];
        using var data = CommandData.ReadJson(
            await project.EfAsync(["dbcontext", "info", "--json", "--no-build", "--", .. arguments]));
        Assert.Equal("TestApp.BlogContext", data.RootElement.GetProperty("type").GetString());
        Assert.Equal(arguments, File.ReadAllLines(project.FactoryArgumentsPath));

        var rebuilt = await project.EfAsync(["dbcontext", "info", "--json"]);
        Assert.NotEqual(0, rebuilt.ExitCode);
        Assert.Contains("Build failed", rebuilt.StandardOutput);
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Honors_configuration_and_runtime_selection()
    {
        await using var project = fixture.CreateProject(
            nameof(Honors_configuration_and_runtime_selection), ToolingProjectLayout.SeparateData);
        var runtime = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
        foreach (var ownedProject in project.Projects)
        {
            project.SetProperty(ownedProject, "RuntimeIdentifier", runtime);
        }
        await project.RestoreAsync();
        using var data = CommandData.ReadJson(
            await project.EfAsync(["dbcontext", "info", "--json", "--runtime", runtime], configuration: "ToolingTest"));
        Assert.Equal("TestApp.BlogContext", data.RootElement.GetProperty("type").GetString());
        Assert.True(File.Exists(Path.Combine(
            Path.GetDirectoryName(project.StartupProject)!, "bin", "ToolingTest", fixture.TargetFramework, runtime, "Startup.dll")));
        project.MarkSuccessful();
    }
}
