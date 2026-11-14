// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "CLI")]
public class OptimizeCommandTest(ToolingTestAssemblyFixture fixture)
{
    [Theory(Timeout = 600_000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generated_model_and_queries_are_used(bool precompileQueries)
    {
        await using var project = fixture.CreateProject(
            nameof(Generated_model_and_queries_are_used), ToolingProjectLayout.SeparateData);
        await project.RestoreAsync();
        var arguments = new List<string>
        {
            "dbcontext", "optimize", "--output-dir", "CompiledModels", "--namespace", "TestApp", "--context", "BlogContext"
        };
        if (precompileQueries)
        {
            arguments.AddRange(["--precompile-queries", "--nativeaot"]);
        }

        (await project.EfAsync(arguments)).EnsureSuccess();
        var generated = Path.Combine(Path.GetDirectoryName(project.ContextProject)!, "CompiledModels");
        Assert.NotEmpty(Directory.GetFiles(generated, "*.cs"));
        (await project.BuildAsync()).EnsureSuccess();
        var application = await project.RunApplicationAsync();
        application.EnsureSuccess();
        Assert.Contains("MODEL:TestApp.BlogContextModel", application.StandardOutput);
        Assert.Contains("RESULT:Alice", application.StandardOutput);
        if (precompileQueries)
        {
            Assert.Contains("QUERY_COMPILATIONS:0", application.StandardOutput);
        }

        project.MarkSuccessful();
    }
}
