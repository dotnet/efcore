// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

[Trait("Suite", "MSBuild")]
public class PublishGenerationTest(ToolingTestAssemblyFixture fixture)
{
    public static IEnumerable<TheoryDataRow<bool>> Stages
        => [new(false), new TheoryDataRow<bool>(true).WithSkip(KnownToolingIssues.ColdBuild)];

    [Theory]
    [MemberData(nameof(Stages))]
    public async Task Generates_compiled_model_on_publish(bool modelOnBuild)
    {
        await using var project = fixture.CreateProject(
            nameof(Generates_compiled_model_on_publish), ToolingProjectLayout.SeparateMigrations);
        project.EnableTasks(modelOnBuild ? "build" : "publish", "none");
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.Contains(
            project.GeneratedFiles(modelOnBuild ? "Build" : "Publish"),
            file => file.EndsWith("BlogContextModel.g.cs", StringComparison.Ordinal));
        var application = await project.RunDotNetAsync([Path.Combine(project.PublishDirectory, "Startup.dll")]);
        application.EnsureSuccess();
        Assert.Contains("MODEL:TestApp.BlogContextModel", application.StandardOutput);
        project.MarkSuccessful();
    }

    [Fact(Timeout = 600_000)]
    public async Task Default_publish_generates_and_executes_precompiled_queries()
    {
        await using var project = fixture.CreateProject(nameof(Default_publish_generates_and_executes_precompiled_queries));
        project.EnableTasks();
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.NotEmpty(project.GeneratedFiles("Publish"));
        var application = await project.RunDotNetAsync([Path.Combine(project.PublishDirectory, "App.dll")]);
        application.EnsureSuccess();
        Assert.Contains("MODEL:TestApp.BlogContextModel", application.StandardOutput);
        Assert.Contains("QUERY_COMPILATIONS:0", application.StandardOutput);
        Assert.Contains("RESULT:Alice", application.StandardOutput);
        project.MarkSuccessful();
    }
}
