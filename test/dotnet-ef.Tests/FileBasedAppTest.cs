// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public sealed class FileBasedAppTest(ITestOutputHelper output)
{
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

            Exe.Run("dotnet", ["restore", csFile], handleOutput: Reporter.WriteVerbose);

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
