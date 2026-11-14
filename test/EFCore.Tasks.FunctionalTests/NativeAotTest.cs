// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

[Trait("Suite", "NativeAot")]
public class NativeAotTest(ToolingTestAssemblyFixture fixture)
{
    [Fact(Timeout = 1_200_000)]
    public async Task Published_xunit_runner_executes_native_test()
    {
        await RequireNativeToolchainAsync();
        await using var project = fixture.CreateProject(
            nameof(Published_xunit_runner_executes_native_test), kind: ToolingProjectKind.AotProbe);
        project.ConfigureDeployment(native: true);
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.Equal(1, await project.RunPublishedTestsAsync());
        Assert.Equal(1, await project.RunRelocatedPublishedTestsAsync());
        project.MarkSuccessful();
    }

    [Theory(Timeout = 1_200_000)]
    [InlineData(ToolingProjectLayout.SeparateMigrations)]
    [InlineData(ToolingProjectLayout.SeparateQueries)]
    public async Task Published_ef_tests_execute_natively(ToolingProjectLayout layout)
    {
        await RequireNativeToolchainAsync();
        await using var project = fixture.CreateProject(
            nameof(Published_ef_tests_execute_natively), layout, ToolingProjectKind.AotTests);
        project.EnableTasks();
        project.ConfigureDeployment(native: true);
        await project.RestoreAsync();
        (await project.PublishAsync()).EnsureSuccess();
        Assert.Equal(3, await project.RunPublishedTestsAsync());
        project.MarkSuccessful();
    }

    private async Task RequireNativeToolchainAsync()
    {
        var reason = await fixture.NativeAotUnsupportedReasonAsync();
        if (reason != null)
        {
            Assert.Fail(reason);
        }
    }
}
