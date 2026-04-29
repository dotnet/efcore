// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;

namespace Microsoft.EntityFrameworkCore.Tools;

public sealed class ProjectTest(ITestOutputHelper output)
{
    private const string TargetFramework = "net10.0";

    [Fact]
    public void Alias_option_is_used()
    {
        var primary = CreateOption("--project");
        var alias = CreateOption("--file");
        alias.TryParse("MyApp.cs");

        Assert.Equal("MyApp.cs", RootCommand.ResolveOption(primary, alias, configValue: null));
    }

    [Fact]
    public void Primary_option_is_used_when_alias_is_not_specified()
    {
        var primary = CreateOption("--project");
        var alias = CreateOption("--file");
        primary.TryParse("MyApp.csproj");

        Assert.Equal("MyApp.csproj", RootCommand.ResolveOption(primary, alias, configValue: null));
    }

    [Fact]
    public void Config_value_is_used_when_no_options_specified()
    {
        var primary = CreateOption("--project");
        var alias = CreateOption("--file");

        Assert.Equal("FromConfig", RootCommand.ResolveOption(primary, alias, configValue: "FromConfig"));
    }

    [Fact]
    public void Alias_option_takes_precedence_over_config()
    {
        var primary = CreateOption("--project");
        var alias = CreateOption("--file");
        alias.TryParse("MyApp.cs");

        Assert.Equal("MyApp.cs", RootCommand.ResolveOption(primary, alias, configValue: "FromConfig"));
    }

    [Fact]
    public void Primary_and_alias_options_together_throws()
    {
        var primary = CreateOption("--project");
        var alias = CreateOption("--file");
        primary.TryParse("MyApp.csproj");
        alias.TryParse("MyApp.cs");

        Assert.Throws<CommandException>(
            () => RootCommand.ResolveOption(primary, alias, configValue: null));
    }

    [Fact]
    public void Returns_null_when_nothing_specified()
    {
        var primary = CreateOption("--project");
        var alias = CreateOption("--file");

        Assert.Null(RootCommand.ResolveOption(primary, alias, configValue: null));
    }

    [Fact]
    public void Csproj_metadata_can_be_extracted()
    {
        var capturedOutput = WithCapturedOutput(() =>
        {
            using var directory = new TempDirectory();
            var csprojFile = Path.Combine(directory.Path, "MyApp.csproj");
            File.WriteAllText(csprojFile, $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>{TargetFramework}</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            Exe.Run("dotnet", ["restore", csprojFile], handleOutput: _ => { });

            var project = Project.FromFile(csprojFile);

            Assert.Equal("C#", project.Language);
            Assert.Equal("MyApp", project.AssemblyName);
            Assert.Equal(TargetFramework, project.TargetFramework);
            Assert.NotNull(project.OutputPath);
            Assert.NotNull(project.ProjectDir);
            Assert.Equal("MyApp.dll", project.TargetFileName);
        });

        Assert.DoesNotContain(Reporter.ErrorPrefix, capturedOutput);
    }

    [Fact]
    public void Throws_for_missing_project_file()
    {
        var missing = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "Missing.csproj");

        var capturedOutput = WithCapturedOutput(() =>
        {
            var ex = Assert.Throws<CommandException>(() => Project.FromFile(missing));
            Assert.Contains(missing, ex.Message);
        });

        Assert.DoesNotContain(Reporter.ErrorPrefix, capturedOutput);
    }

    [Fact]
    public void File_based_app_can_be_built()
    {
        var capturedOutput = WithCapturedOutput(() =>
        {
            using var directory = new TempDirectory();
            var csFile = Path.Combine(directory.Path, "MyApp.cs");
            File.WriteAllText(csFile, $"""
                #:property TargetFramework={TargetFramework}
                Console.WriteLine("Hello");
                """);

            Exe.Run("dotnet", ["restore", csFile], handleOutput: Reporter.WriteVerbose);

            var project = Project.FromFile(csFile);

            Assert.Equal("C#", project.Language);
            Assert.Equal("MyApp", project.AssemblyName);
            Assert.Equal(TargetFramework, project.TargetFramework);
            Assert.NotNull(project.OutputPath);
            Assert.NotNull(project.ProjectDir);
            Assert.NotNull(project.TargetFileName);

            project.Build(additionalArgs: null);

            var targetDir = Path.GetFullPath(Path.Combine(project.ProjectDir!, project.OutputPath!));
            var targetPath = Path.Combine(targetDir, project.TargetFileName!);
            Assert.True(File.Exists(targetPath), $"Expected build output at {targetPath}");
        });

        Assert.DoesNotContain(Reporter.ErrorPrefix, capturedOutput);
    }

    private string WithCapturedOutput(Action action)
    {
        var captured = new StringBuilder();
        var previousIsVerbose = Reporter.IsVerbose;
        var previousPrefixOutput = Reporter.PrefixOutput;
        var previousNoColor = Reporter.NoColor;
        Reporter.IsVerbose = true;
        Reporter.PrefixOutput = true;
        Reporter.NoColor = true;
        Reporter.SetStdOut(new TestOutputWriter(output, captured));
        try
        {
            action();
        }
        finally
        {
            Reporter.IsVerbose = previousIsVerbose;
            Reporter.PrefixOutput = previousPrefixOutput;
            Reporter.NoColor = previousNoColor;
            Reporter.SetStdOut(Console.Out);
        }

        return captured.ToString();
    }

    private sealed class TestOutputWriter(ITestOutputHelper output, StringBuilder captured) : StringWriter
    {
        public override void WriteLine(string? value)
        {
            if (value != null)
            {
                output.WriteLine(value);
                captured.AppendLine(value);
            }
        }
    }

    private static CommandOption CreateOption(string name)
        => new($"{name} <VALUE>", CommandOptionType.SingleValue);
}
