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
        protected override void Validate() { } // skip file-existence checks for unit test
        protected override int Execute(string[] args) => 0;
    }

    [Fact]
    public void GetCommonProjectArgs_round_trips_supplied_options()
    {
        var app = new CommandLineApplication { Name = "ef" };
        var command = new TestCommand();
        command.Configure(app);

        app.Execute(
            "--assembly", "A.dll", "--startup-assembly", "S.dll",
            "--project", "A.csproj", "--language", "C#", "--nullable");

        var common = command.CommonArgs();
        Assert.Equal(
            new[] { "--assembly", "A.dll", "--startup-assembly", "S.dll", "--project", "A.csproj", "--language", "C#", "--nullable" },
            common);
    }

    private sealed class TestBundleCommand : MigrationsBundleCommand
    {
        public IReadOnlyList<string> CommonArgs() => GetCommonProjectArgs();
        protected override void Validate() { } // skip file-existence checks for unit test
        protected override int Execute(string[] args) => 0;
    }

    [Fact]
    public void Bundle_command_accepts_host_args_forwarded_by_dotnet_ef_and_keeps_them_out_of_common_args()
    {
        var app = new CommandLineApplication { Name = "ef" };
        var command = new TestBundleCommand();
        command.Configure(app);

        app.Execute(
            "--assembly", "A.dll",
            "--deps-file", "S.deps.json", "--runtime-config", "S.runtimeconfig.json",
            "--runtime-framework-version", "10.0.0",
            "--additional-probing-path", "/n1", "--additional-probing-path", "/n2");

        Assert.Equal(new[] { "--assembly", "A.dll" }, command.CommonArgs());
    }
}
