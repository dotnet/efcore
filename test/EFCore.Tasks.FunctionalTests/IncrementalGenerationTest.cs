// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

[Trait("Suite", "MSBuild")]
public class IncrementalGenerationTest(ToolingTestAssemblyFixture fixture)
{
    [Fact(Timeout = 600_000, Skip = KnownToolingIssues.ChangedModel)]
    public async Task Regenerates_changed_model_and_cleans_only_owned_files()
    {
        await using var project = await CreateExistingOutputProjectAsync(nameof(Regenerates_changed_model_and_cleans_only_owned_files));
        project.WriteSource(
            project.ContextProject, "Model.cs",
            fixture.Source("Model.cs.template").Replace("{{MigrationsAssembly}}", "App").Replace("HasMaxLength(64)", "HasMaxLength(128)"));
        (await project.BuildAsync()).EnsureSuccess();
        var application = await project.RunApplicationAsync();
        application.EnsureSuccess();
        Assert.Contains("MAX_LENGTH:128", application.StandardOutput);
        var generated = project.GeneratedFiles("Build").ToList();
        await project.CleanAsync();
        Assert.Empty(project.GeneratedFiles("Build"));
        Assert.All(generated, file => Assert.False(File.Exists(file), file));
        Assert.True(File.Exists(fixture.CliAssembly));
        project.MarkSuccessful();
    }

    [Fact(Timeout = 600_000)]
    public async Task Unchanged_build_and_clean_preserve_product_inputs()
    {
        await using var project = await CreateExistingOutputProjectAsync(nameof(Unchanged_build_and_clean_preserve_product_inputs));
        var files = project.GeneratedFiles("Build").ToDictionary(file => file, File.GetLastWriteTimeUtc);
        Assert.NotEmpty(files);
        var unchanged = await project.BuildAsync();
        unchanged.EnsureSuccess();
        Assert.DoesNotContain("Optimizing DbContext", unchanged.StandardOutput);
        Assert.All(files, file => Assert.Equal(file.Value, File.GetLastWriteTimeUtc(file.Key)));
        var coreAssembly = fixture.Property("CoreAssembly");
        var originalWrite = File.GetLastWriteTimeUtc(coreAssembly);
        await project.CleanAsync();
        Assert.Empty(project.GeneratedFiles("Build"));
        Assert.All(files, file => Assert.False(File.Exists(file.Key), file.Key));
        Assert.True(File.Exists(coreAssembly));
        Assert.Equal(originalWrite, File.GetLastWriteTimeUtc(coreAssembly));
        project.MarkSuccessful();
    }

    private async Task<ToolingTestProject> CreateExistingOutputProjectAsync(string name)
    {
        var project = fixture.CreateProject(name);
        project.EnableTasks("none", "none");
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        project.SetProperty(project.ContextProject, "EFScaffoldModelStage", "build");
        (await project.BuildAsync()).EnsureSuccess();
        return project;
    }
}
