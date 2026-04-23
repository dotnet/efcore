// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;

namespace Microsoft.EntityFrameworkCore.Tools;

public sealed class FileBasedAppTest(ITestOutputHelper output)
{
    [Fact]
    public void File_option_is_used_as_project()
    {
        var project = new CommandOption("--project <PROJECT>", CommandOptionType.SingleValue);
        var file = new CommandOption("--file <FILE>", CommandOptionType.SingleValue);
        file.TryParse("MyApp.cs");

        var result = RootCommand.ResolveProjectOption(project, file, configValue: null);

        Assert.Equal("MyApp.cs", result);
    }

    [Fact]
    public void Project_option_is_used_when_file_is_not_specified()
    {
        var project = new CommandOption("--project <PROJECT>", CommandOptionType.SingleValue);
        var file = new CommandOption("--file <FILE>", CommandOptionType.SingleValue);
        project.TryParse("MyApp.csproj");

        var result = RootCommand.ResolveProjectOption(project, file, configValue: null);

        Assert.Equal("MyApp.csproj", result);
    }

    [Fact]
    public void Config_value_is_used_when_no_options_specified()
    {
        var project = new CommandOption("--project <PROJECT>", CommandOptionType.SingleValue);
        var file = new CommandOption("--file <FILE>", CommandOptionType.SingleValue);

        var result = RootCommand.ResolveProjectOption(project, file, configValue: "FromConfig");

        Assert.Equal("FromConfig", result);
    }

    [Fact]
    public void File_option_takes_precedence_over_config()
    {
        var project = new CommandOption("--project <PROJECT>", CommandOptionType.SingleValue);
        var file = new CommandOption("--file <FILE>", CommandOptionType.SingleValue);
        file.TryParse("MyApp.cs");

        var result = RootCommand.ResolveProjectOption(project, file, configValue: "FromConfig");

        Assert.Equal("MyApp.cs", result);
    }

    [Fact]
    public void Project_and_file_options_together_throws()
    {
        var project = new CommandOption("--project <PROJECT>", CommandOptionType.SingleValue);
        var file = new CommandOption("--file <FILE>", CommandOptionType.SingleValue);
        project.TryParse("MyApp.csproj");
        file.TryParse("MyApp.cs");

        Assert.Throws<CommandException>(
            () => RootCommand.ResolveProjectOption(project, file, configValue: null));
    }

    [Fact]
    public void Returns_null_when_nothing_specified()
    {
        var project = new CommandOption("--project <PROJECT>", CommandOptionType.SingleValue);
        var file = new CommandOption("--file <FILE>", CommandOptionType.SingleValue);

        var result = RootCommand.ResolveProjectOption(project, file, configValue: null);

        Assert.Null(result);
    }

    [Fact]
    public void Build()
    {
        var previousIsVerbose = Reporter.IsVerbose;
        Reporter.IsVerbose = true;
        Reporter.SetStdOut(new TestOutputWriter(output));
        try
        {
            using var directory = new TempDirectory();
            var csFile = Path.Combine(directory.Path, "MyApp.cs");
            File.WriteAllText(csFile, """
                #:property TargetFramework=net10.0
                Console.WriteLine("Hello");
                """);

            var project = Project.FromFile(csFile);

            Assert.Equal("C#", project.Language);
            Assert.Equal("MyApp", project.AssemblyName);
            Assert.NotNull(project.TargetFrameworkMoniker);
            Assert.NotNull(project.OutputPath);
            Assert.NotNull(project.ProjectDir);
            Assert.NotNull(project.TargetFileName);

            project.Build(additionalArgs: null);

            var targetDir = Path.GetFullPath(Path.Combine(project.ProjectDir!, project.OutputPath!));
            var targetPath = Path.Combine(targetDir, project.TargetFileName!);
            Assert.True(File.Exists(targetPath), $"Expected build output at {targetPath}");
        }
        finally
        {
            Reporter.IsVerbose = previousIsVerbose;
            Reporter.SetStdOut(Console.Out);
        }
    }

    private sealed class TestOutputWriter(ITestOutputHelper output) : StringWriter
    {
        public override void WriteLine(string? value)
        {
            if (value != null)
            {
                output.WriteLine(value);
            }
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
