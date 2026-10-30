// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Commands;

namespace Microsoft.EntityFrameworkCore.Tools;

public class ProjectCommandBaseTest
{
    private sealed class TestCommand : ProjectCommandBase
    {
        public IReadOnlyList<string> CommonArgs() => GetCommonProjectArgs();
        public string? Deps => DepsFile!.Value();
        public string? RuntimeCfg => RuntimeConfig!.Value();
        public IReadOnlyList<string?> Probing => AdditionalProbingPaths!.Values;
        protected override void Validate() { } // skip file-existence checks for unit test
        protected override int Execute(string[] args) => 0;
    }

    [Fact]
    public void GetCommonProjectArgs_round_trips_supplied_options_and_receives_host_args()
    {
        var app = new CommandLineApplication { Name = "ef" };
        var command = new TestCommand();
        command.Configure(app);

        app.Execute(
            "--assembly", "A.dll", "--startup-assembly", "S.dll",
            "--project", "A.csproj", "--language", "C#", "--nullable",
            "--deps-file", "S.deps.json", "--runtime-config", "S.runtimeconfig.json",
            "--additional-probing-path", "/n1", "--additional-probing-path", "/n2");

        Assert.Equal("S.deps.json", command.Deps);
        Assert.Equal("S.runtimeconfig.json", command.RuntimeCfg);
        Assert.Equal(new[] { "/n1", "/n2" }, command.Probing);

        var common = command.CommonArgs();
        Assert.Equal(
            new[] { "--assembly", "A.dll", "--startup-assembly", "S.dll", "--project", "A.csproj", "--language", "C#", "--nullable" },
            common);
        Assert.DoesNotContain("--deps-file", common);
        Assert.DoesNotContain("--additional-probing-path", common);
    }
}
