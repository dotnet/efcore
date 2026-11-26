// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

[Trait("Suite", "Trimming")]
public class TrimmingTest(ToolingTestAssemblyFixture fixture)
{
    [Fact(Timeout = 1_200_000)]
    public async Task Published_xunit_aot_target_discovers_and_runs_tests()
    {
        await using var project = fixture.CreateProject(
            nameof(Published_xunit_aot_target_discovers_and_runs_tests), kind: ToolingProjectKind.AotProbe);
        project.ConfigureDeployment(native: false);
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.Equal(1, await project.RunPublishedTestsAsync());
        Assert.Equal(1, await project.RunRelocatedPublishedTestsAsync());
        project.MarkSuccessful();
    }

    [Theory(Timeout = 1_200_000)]
    [InlineData(ToolingProjectLayout.SeparateMigrations)]
    [InlineData(ToolingProjectLayout.SeparateQueries)]
    public async Task Published_ef_tests_execute_generated_code(ToolingProjectLayout layout)
    {
        await using var project = fixture.CreateProject(
            nameof(Published_ef_tests_execute_generated_code), layout, ToolingProjectKind.AotTests);
        project.EnableTasks();
        project.ConfigureDeployment(native: false);
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.Equal(3, await project.RunPublishedTestsAsync());
        project.MarkSuccessful();
    }
}
