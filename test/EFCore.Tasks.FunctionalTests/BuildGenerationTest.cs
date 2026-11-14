// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

[Trait("Suite", "MSBuild")]
public class BuildGenerationTest(ToolingTestAssemblyFixture fixture)
{
    public static IEnumerable<TheoryDataRow<ToolingProjectLayout>> Layouts
        => ToolingTestData.Layouts.Select(row => row.WithSkip(KnownToolingIssues.ColdBuild));

    [Theory]
    [MemberData(nameof(Layouts))]
    public async Task Generates_and_uses_compiled_model(ToolingProjectLayout layout)
    {
        await using var project = fixture.CreateProject(nameof(Generates_and_uses_compiled_model), layout);
        project.EnableTasks("build", "none");
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        var files = project.GeneratedFiles("Build").ToList();
        Assert.Contains(files, file => file.EndsWith("BlogContextModel.g.cs", StringComparison.Ordinal));
        Assert.All(files, file => Assert.True(File.Exists(file), file));
        Assert.Empty(project.GeneratedFiles("Publish"));
        var application = await project.RunApplicationAsync();
        application.EnsureSuccess();
        Assert.Contains("RESULT:Alice", application.StandardOutput);
        Assert.Contains("MODEL:TestApp.BlogContextModel", application.StandardOutput);
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Disabled_generation_does_not_create_sources()
    {
        await using var project = fixture.CreateProject(nameof(Disabled_generation_does_not_create_sources));
        project.EnableTasks("none", "none");
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.Empty(project.GeneratedFiles("Build"));
        Assert.Empty(project.GeneratedFiles("Publish"));
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Default_publish_stages_do_not_generate_on_build()
    {
        await using var project = fixture.CreateProject(nameof(Default_publish_stages_do_not_generate_on_build));
        project.EnableTasks();
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        Assert.Empty(project.GeneratedFiles("Build"));
        Assert.Empty(project.GeneratedFiles("Publish"));
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Generates_compiled_model_from_existing_output()
    {
        await using var project = fixture.CreateProject(nameof(Generates_compiled_model_from_existing_output));
        project.EnableTasks("none", "none");
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        project.SetProperty(project.ContextProject, "EFScaffoldModelStage", "build");
        (await project.BuildAsync()).EnsureSuccess();
        Assert.NotEmpty(project.GeneratedFiles("Build"));
        var application = await project.RunApplicationAsync();
        application.EnsureSuccess();
        Assert.Contains("MODEL:TestApp.BlogContextModel", application.StandardOutput);
        project.MarkSuccessful();
    }
}
