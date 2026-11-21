// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using EFCommand = Microsoft.EntityFrameworkCore.Tools.Commands.RootCommand;

namespace Microsoft.EntityFrameworkCore.Tools;

public class BundleHostArgsTest
{
    [Theory]
    [InlineData("migrations", "bundle", true)]
    [InlineData("Migrations", "Bundle", true)]
    [InlineData("migrations", "add", false)]
    [InlineData("database", "update", false)]
    [InlineData("dbcontext", "list", false)]
    public void IsMigrationsBundle_only_matches_migrations_bundle(string first, string second, bool expected)
        => Assert.Equal(expected, RootCommand.IsMigrationsBundle([first, second]));

    [Fact]
    public void IsMigrationsBundle_is_false_for_incomplete_commands()
    {
        Assert.False(RootCommand.IsMigrationsBundle([]));
        Assert.False(RootCommand.IsMigrationsBundle(["migrations"]));
    }

    [Fact]
    public void GetBundleHostArgs_forwards_existing_files_and_probing_paths()
    {
        using var directory = new TempDirectory();
        var deps = Path.Combine(directory.Path, "S.deps.json");
        var runtimeConfig = Path.Combine(directory.Path, "S.runtimeconfig.json");
        File.WriteAllText(deps, "{}");
        File.WriteAllText(runtimeConfig, "{}");

        var args = RootCommand.GetBundleHostArgs(deps, runtimeConfig, "10.0.0", ["/n1", "/n2"]);

        Assert.Equal(
            new[]
            {
                "--deps-file", deps,
                "--runtime-config", runtimeConfig,
                "--additional-probing-path", "/n1",
                "--additional-probing-path", "/n2"
            },
            args);
    }

    [Fact]
    public void GetBundleHostArgs_falls_back_to_framework_version_when_there_is_no_runtimeconfig()
    {
        using var directory = new TempDirectory();

        var args = RootCommand.GetBundleHostArgs(
            Path.Combine(directory.Path, "missing.deps.json"),
            Path.Combine(directory.Path, "missing.runtimeconfig.json"),
            "10.0.0",
            []);

        Assert.Equal(new[] { "--runtime-framework-version", "10.0.0" }, args);
    }

    [Fact]
    public void GetBundleHostArgs_is_empty_when_nothing_is_available()
    {
        using var directory = new TempDirectory();

        Assert.Empty(
            RootCommand.GetBundleHostArgs(
                Path.Combine(directory.Path, "missing.deps.json"),
                Path.Combine(directory.Path, "missing.runtimeconfig.json"),
                null,
                []));
    }
}
