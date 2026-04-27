// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public sealed class ProjectTest(ITestOutputHelper output)
{
    private const string TargetFramework = "net10.0";

    [Fact]
    public void Csproj_metadata_can_be_extracted()
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
    }

    [Fact]
    public void File_based_app_can_be_built()
    {
        WithVerboseOutput(() =>
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
    }

    private void WithVerboseOutput(Action action)
    {
        var previousIsVerbose = Reporter.IsVerbose;
        var previousPrefixOutput = Reporter.PrefixOutput;
        Reporter.IsVerbose = true;
        Reporter.PrefixOutput = true;
        Reporter.SetStdOut(new TestOutputWriter(output));
        try
        {
            action();
        }
        finally
        {
            Reporter.IsVerbose = previousIsVerbose;
            Reporter.PrefixOutput = previousPrefixOutput;
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
}
