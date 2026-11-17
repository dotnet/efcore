// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class MigrationsOutputDirTest
{
    [Theory]
    [InlineData("migrations", "add", "InitialCreate")]
    [InlineData("migrations", "remove")]
    [InlineData("database", "update", "--add", "InitialCreate")]
    public void Property_is_used_when_output_dir_is_not_specified(params string[] args)
        => Assert.Equal("Data/Migrations", RootCommand.ResolveMigrationsOutputDir(args, "Data/Migrations"));

    [Theory]
    [InlineData("migrations", "add", "InitialCreate", "-o", "Other")]
    [InlineData("migrations", "add", "InitialCreate", "--output-dir", "Other")]
    [InlineData("migrations", "add", "InitialCreate", "--output-dir=Other")]
    [InlineData("migrations", "add", "InitialCreate", "-o:Other")]
    [InlineData("migrations", "remove", "-o", "Other")]
    [InlineData("database", "update", "--add", "InitialCreate", "--output-dir", "Other")]
    public void Explicit_output_dir_wins(params string[] args)
        => Assert.Null(RootCommand.ResolveMigrationsOutputDir(args, "Data/Migrations"));

    [Theory]
    [InlineData("database", "update")]
    [InlineData("database", "update", "InitialCreate")]
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

    [Theory]
    [InlineData("Data/Migrations", "Data/Migrations")]
    [InlineData("  Data/Migrations ", "Data/Migrations")]
    [InlineData("\n      Data/Migrations\n    ", "Data/Migrations")]
    [InlineData("../Shared/Migrations", "../Shared/Migrations")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" \r\n ", null)]
    public void Property_value_is_trimmed(string? value, string? expected)
        => Assert.Equal(expected, Project.NormalizeMigrationsOutputDir(value));

    [Fact]
    public void Property_value_uses_the_platform_directory_separator()
        => Assert.Equal(
            "Data" + Path.DirectorySeparatorChar + "Migrations",
            Project.NormalizeMigrationsOutputDir("Data\\Migrations"));
}
