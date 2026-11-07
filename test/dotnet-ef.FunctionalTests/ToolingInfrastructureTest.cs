// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "Infrastructure")]
public class ToolingInfrastructureTest(ToolingTestAssemblyFixture fixture)
{
    public static IEnumerable<TheoryDataRow<ToolingProjectLayout>> Layouts
        => ToolingTestData.Layouts;

    [Fact]
    public void Payload_comes_from_checkout()
    {
        Assert.Equal(
            File.ReadAllBytes(fixture.CliAssembly),
            File.ReadAllBytes(Path.Combine(fixture.AssetsDirectory, "dotnet-ef.dll")));
        Assert.Equal(
            Path.GetFullPath(Environment.GetEnvironmentVariable("DOTNET_ROOT")!),
            fixture.DotNetRoot);
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public async Task Samples_build_without_rebuilding_product(ToolingProjectLayout layout)
    {
        await using var project = fixture.CreateProject(nameof(Samples_build_without_rebuilding_product), layout);
        var coreAssembly = fixture.Property("CoreAssembly");
        Assert.True(File.Exists(coreAssembly), coreAssembly);
        var originalWrite = File.GetLastWriteTimeUtc(coreAssembly);
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        using var runtimeConfig = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(
                Path.GetDirectoryName(project.StartupProject)!, "bin", fixture.Configuration, fixture.TargetFramework,
                Path.GetFileNameWithoutExtension(project.StartupProject) + ".runtimeconfig.json")));
        Assert.Equal(fixture.TargetFramework, runtimeConfig.RootElement.GetProperty("runtimeOptions").GetProperty("tfm").GetString());
        var result = await project.RunApplicationAsync();
        result.EnsureSuccess();
        Assert.Contains("RESULT:Alice", result.StandardOutput);
        Assert.Equal(originalWrite, File.GetLastWriteTimeUtc(coreAssembly));
        Assert.Equal(layout == ToolingProjectLayout.SingleProject, project.StartupProject == project.ContextProject);
        Assert.Equal(
            layout is ToolingProjectLayout.SeparateMigrations or ToolingProjectLayout.SeparateQueries,
            project.MigrationsProject != project.ContextProject);
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Captures_output_and_preserves_arguments()
    {
        await using var project = fixture.CreateProject(
            "probe with spaces", kind: ToolingProjectKind.ProcessProbe);
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        string[] arguments = ["", "path with spaces", "trailing\\", "\"quoted\""];
        var result = await project.RunApplicationAsync(["echo", .. arguments]);
        result.EnsureSuccess();
        Assert.Equal(
            arguments,
            result.StandardOutput.Replace("\r\n", "\n").Split('\n').Take(arguments.Length)
                .Select(line => Encoding.UTF8.GetString(Convert.FromBase64String(line))));
        Assert.Equal("stderr", result.StandardError.Trim());
        Assert.True(File.Exists(result.LogPath + ".stdout.log"));
        using var metadata = JsonDocument.Parse(File.ReadAllText(result.LogPath + ".command.json"));
        Assert.Equal(project.DirectoryPath, metadata.RootElement.GetProperty("workingDirectory").GetString());
        Assert.Equal(arguments, metadata.RootElement.GetProperty("arguments").EnumerateArray().Skip(2).Select(value => value.GetString()));
        Assert.False(metadata.RootElement.GetProperty("environment").TryGetProperty("PATH", out _));
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Reports_nonzero_exit_and_diagnostics()
    {
        await using var project = fixture.CreateProject(nameof(Reports_nonzero_exit_and_diagnostics), kind: ToolingProjectKind.ProcessProbe);
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        var result = await project.RunApplicationAsync("failure");
        Assert.Equal(23, result.ExitCode);
        var exception = Assert.Throws<InvalidOperationException>(result.EnsureSuccess);
        Assert.Contains("expected failure", exception.Message);
        Assert.Contains(result.LogPath, exception.Message);
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Timeout_terminates_owned_process_tree()
    {
        await using var project = fixture.CreateProject(
            nameof(Timeout_terminates_owned_process_tree), kind: ToolingProjectKind.ProcessProbe);
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        await Assert.ThrowsAsync<TimeoutException>(
            () => project.RunDotNetAsync(
                [ProbeAssembly(project), "tree"], TimeSpan.FromSeconds(5)));
        var output = File.ReadAllText(Directory.GetFiles(project.LogDirectory, "*.stdout.log").Order().Last());
        foreach (var prefix in new[] { "parent:", "child:" })
        {
            var line = Assert.Single(output.Split('\n'), line => line.StartsWith(prefix, StringComparison.Ordinal));
            Assert.True(HasExited(int.Parse(line[prefix.Length..])));
        }

        project.MarkSuccessful();
    }

    [Fact]
    public async Task Cancellation_is_not_reported_as_timeout()
    {
        await using var project = fixture.CreateProject(
            nameof(Cancellation_is_not_reported_as_timeout), kind: ToolingProjectKind.ProcessProbe);
        await project.RestoreAsync();
        (await project.BuildAsync()).EnsureSuccess();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => project.RunDotNetAsync([ProbeAssembly(project), "wait"], cancellationToken: cancellation.Token));
        project.MarkSuccessful();
    }

    [Fact]
    public async Task Successful_sandbox_cleanup_preserves_logs()
    {
        var project = fixture.CreateProject(nameof(Successful_sandbox_cleanup_preserves_logs), kind: ToolingProjectKind.ProcessProbe);
        var result = await project.RunDotNetAsync(["--version"]);
        result.EnsureSuccess();
        project.MarkSuccessful();
        await project.DisposeAsync();
        Assert.False(Directory.Exists(project.DirectoryPath));
        Assert.True(File.Exists(result.LogPath + ".stdout.log"));
    }

    private string ProbeAssembly(ToolingTestProject project)
        => Path.Combine(
            Path.GetDirectoryName(project.StartupProject)!, "bin", fixture.Configuration, fixture.TargetFramework, "Probe.dll");

    private static bool HasExited(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
