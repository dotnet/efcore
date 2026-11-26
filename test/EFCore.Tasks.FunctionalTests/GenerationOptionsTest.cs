// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

[Trait("Suite", "MSBuild")]
public class GenerationOptionsTest(ToolingTestAssemblyFixture fixture)
{
    [Theory]
    [InlineData("EFOptimizeContext", "true", "is no longer supported")]
    [InlineData("EFPrecompileQueriesStage", "build", "must also be set to 'publish'")]
    public async Task Rejects_invalid_generation_properties(string property, string value, string diagnostic)
    {
        await using var project = fixture.CreateProject(nameof(Rejects_invalid_generation_properties));
        project.EnableTasks();
        project.SetProperty(project.ContextProject, property, value);
        await project.RestoreAsync();
        var result = await project.BuildAsync();
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains(diagnostic, result.StandardOutput);
        Assert.Empty(project.GeneratedFiles("Build"));
        Assert.Empty(project.GeneratedFiles("Publish"));
        project.MarkSuccessful();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Existing_output_honors_namespace_output_context_and_nullable_options(bool nullable)
    {
        await using var project = fixture.CreateProject(nameof(Existing_output_honors_namespace_output_context_and_nullable_options));
        project.EnableTasks("none", "none");
        var output = Path.Combine(project.DirectoryPath, "generated code");
        project.SetProperty(project.ContextProject, "EFOutputDir", output);
        project.SetProperty(project.ContextProject, "EFTargetNamespace", "Generated.Models");
        project.SetProperty(project.ContextProject, "DbContextType", "BlogContext");
        project.SetProperty(project.ContextProject, "Nullable", nullable ? "enable" : "disable");
        project.WriteSource(
            project.ContextProject, "OtherModel.cs",
            fixture.Source("Model.cs.template")
                .Replace("{{MigrationsAssembly}}", "App")
                .Replace("Blog", "OtherBlog")
                .Replace("QueryProbe", "OtherQueryProbe"));
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        project.SetProperty(project.ContextProject, "EFScaffoldModelStage", "build");
        (await project.BuildAsync()).EnsureSuccess();
        var files = project.GeneratedFiles("Build").ToList();
        Assert.NotEmpty(files);
        Assert.All(files, file => Assert.StartsWith(output, file));
        Assert.DoesNotContain(files, file => file.Contains("OtherBlog", StringComparison.Ordinal));
        var model = File.ReadAllText(Assert.Single(files, file => file.EndsWith("BlogContextModel.g.cs", StringComparison.Ordinal)));
        Assert.Contains("namespace Generated.Models", model);
        Assert.Contains("#nullable disable", model);
        var application = await project.RunApplicationAsync();
        application.EnsureSuccess();
        Assert.Contains("MODEL:Generated.Models.BlogContextModel", application.StandardOutput);
        Assert.Contains("NAME_NULLABLE:" + !nullable, application.StandardOutput);
        project.MarkSuccessful();
    }
}
