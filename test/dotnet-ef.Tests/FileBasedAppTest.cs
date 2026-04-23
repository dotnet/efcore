// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public sealed class FileBasedAppTest : IDisposable
{
    public FileBasedAppTest(ITestOutputHelper output)
    {
        Reporter.IsVerbose = true;
        Reporter.SetStdOut(new TestOutputWriter(output));
    }

    public void Dispose()
    {
        Reporter.IsVerbose = false;
        Reporter.SetStdOut(Console.Out);
    }

    [Fact]
    public void Build()
    {
        using var directory = new TempDirectory();
        var csFile = Path.Combine(directory.Path, "MyApp.cs");
        File.WriteAllText(csFile, """Console.WriteLine("Hello");""");

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
