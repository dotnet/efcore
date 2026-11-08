// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class MigrationsOutputDirTest
{
    [Theory]
    [InlineData("migrations", "add", "InitialCreate")]
    [InlineData("database", "update", "--add", "InitialCreate")]
    public void Property_is_used_when_output_dir_is_not_specified(params string[] args)
        => Assert.Equal("Data/Migrations", RootCommand.ResolveMigrationsOutputDir(args, "Data/Migrations"));

    [Theory]
    [InlineData("migrations", "add", "InitialCreate", "-o", "Other")]
    [InlineData("migrations", "add", "InitialCreate", "--output-dir", "Other")]
    [InlineData("database", "update", "--add", "InitialCreate", "--output-dir", "Other")]
    public void Explicit_output_dir_wins(params string[] args)
        => Assert.Null(RootCommand.ResolveMigrationsOutputDir(args, "Data/Migrations"));

    [Theory]
    [InlineData("database", "update")]
    [InlineData("database", "update", "InitialCreate")]
    [InlineData("migrations", "remove")]
    [InlineData("migrations", "script")]
    [InlineData("dbcontext", "scaffold", "conn", "provider")]
    [InlineData("dbcontext", "optimize")]
    public void Property_is_not_used_for_other_commands(params string[] args)
        => Assert.Null(RootCommand.ResolveMigrationsOutputDir(args, "Data/Migrations"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Unset_property_is_ignored(string? value)
        => Assert.Null(RootCommand.ResolveMigrationsOutputDir(["migrations", "add", "InitialCreate"], value));
}
