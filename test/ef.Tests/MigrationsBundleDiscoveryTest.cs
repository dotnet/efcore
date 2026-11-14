// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Commands;

namespace Microsoft.EntityFrameworkCore.Tools;

public class MigrationsBundleDiscoveryTest
{
    [Fact]
    public void BuildDiscoveryArguments_assembles_host_and_tool_args_in_order()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            efAssemblyPath: "/tools/ef.dll",
            depsFile: "S.deps.json",
            runtimeConfig: "S.runtimeconfig.json",
            fxVersion: null,
            additionalProbingPaths: new[] { "/n1", "/n2" },
            commonProjectArgs: new[] { "--assembly", "A.dll", "--startup-assembly", "S.dll" },
            context: "My.Context",
            verbose: true,
            applicationArgs: Array.Empty<string>());

        Assert.Equal(
            new[]
            {
                "exec",
                "--depsfile", "S.deps.json",
                "--additionalprobingpath", "/n1",
                "--additionalprobingpath", "/n2",
                "--runtimeconfig", "S.runtimeconfig.json",
                "/tools/ef.dll",
                "dbcontext", "info", "--json",
                "--prefix-output", "--no-color",
                "--assembly", "A.dll", "--startup-assembly", "S.dll",
                "--context", "My.Context",
                "--verbose"
            },
            args);
    }

    [Fact]
    public void BuildDiscoveryArguments_forwards_application_args_after_separator()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            "/tools/ef.dll", depsFile: null, runtimeConfig: null, fxVersion: null,
            additionalProbingPaths: Array.Empty<string>(),
            commonProjectArgs: new[] { "--assembly", "A.dll" },
            context: null, verbose: false,
            applicationArgs: new[] { "--environment", "Staging", "--connection", "Server=." });

        Assert.Equal(
            new[]
            {
                "exec", "/tools/ef.dll", "dbcontext", "info", "--json", "--prefix-output", "--no-color",
                "--assembly", "A.dll",
                "--", "--environment", "Staging", "--connection", "Server=."
            },
            args);
    }

    [Fact]
    public void BuildDiscoveryArguments_omits_optional_pieces_when_absent()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            "/tools/ef.dll", depsFile: null, runtimeConfig: null, fxVersion: null,
            additionalProbingPaths: Array.Empty<string>(),
            commonProjectArgs: new[] { "--assembly", "A.dll" },
            context: null, verbose: false, applicationArgs: Array.Empty<string>());

        Assert.Equal(
            new[]
            {
                "exec", "/tools/ef.dll", "dbcontext", "info", "--json",
                "--prefix-output", "--no-color", "--assembly", "A.dll"
            },
            args);
    }

    [Fact]
    public void BuildDiscoveryArguments_uses_fx_version_when_no_runtimeconfig()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            "/tools/ef.dll", depsFile: null, runtimeConfig: null, fxVersion: "8.0.5",
            additionalProbingPaths: Array.Empty<string>(),
            commonProjectArgs: new[] { "--assembly", "A.dll" },
            context: null, verbose: false, applicationArgs: Array.Empty<string>());

        Assert.Equal(
            new[]
            {
                "exec", "--fx-version", "8.0.5", "/tools/ef.dll", "dbcontext", "info", "--json",
                "--prefix-output", "--no-color", "--assembly", "A.dll"
            },
            args);
    }

    [Fact]
    public void BuildDiscoveryArguments_prefers_runtimeconfig_over_fx_version()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            "/tools/ef.dll", depsFile: null, runtimeConfig: "S.runtimeconfig.json", fxVersion: "8.0.5",
            additionalProbingPaths: Array.Empty<string>(),
            commonProjectArgs: new[] { "--assembly", "A.dll" },
            context: null, verbose: false, applicationArgs: Array.Empty<string>());

        Assert.Contains("--runtimeconfig", args);
        Assert.DoesNotContain("--fx-version", args);
    }

    [Fact]
    public void BuildDiscoveryArguments_keeps_prefix_output_and_no_color_together()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            "/tools/ef.dll", depsFile: null, runtimeConfig: null, fxVersion: null,
            additionalProbingPaths: Array.Empty<string>(),
            commonProjectArgs: Array.Empty<string>(),
            context: null, verbose: false, applicationArgs: Array.Empty<string>());

        var prefix = args.ToList().IndexOf("--prefix-output");
        Assert.True(prefix >= 0);
        Assert.Equal("--no-color", args[prefix + 1]);
    }

    [Fact]
    public void BuildDiscoveryArguments_preserves_paths_with_spaces_and_non_ascii_characters()
    {
        var args = MigrationsBundleCommand.BuildDiscoveryArguments(
            @"C:\Program Files\ef tool\ef.dll",
            depsFile: @"C:\proj ü\S.deps.json", runtimeConfig: @"C:\proj ü\S.runtimeconfig.json", fxVersion: null,
            additionalProbingPaths: new[] { @"C:\nuget cache\ü" },
            commonProjectArgs: Array.Empty<string>(),
            context: null, verbose: false, applicationArgs: Array.Empty<string>());

        Assert.Contains(@"C:\Program Files\ef tool\ef.dll", args);
        Assert.Contains(@"C:\proj ü\S.deps.json", args);
        Assert.Contains(@"C:\nuget cache\ü", args);

        // They must survive being turned into a command line (quoted as single arguments).
        var commandLine = Exe.ToArguments(args);
        Assert.Contains("\"C:\\Program Files\\ef tool\\ef.dll\"", commandLine);
        Assert.Contains("\"C:\\nuget cache\\ü\"", commandLine);
    }

    [Fact]
    public void ParseContextType_reads_type_from_json()
        => Assert.Equal(
            "My.App.BlogContext",
            MigrationsBundleCommand.ParseContextType("{  \"type\": \"My.App.BlogContext\",  \"providerName\": \"x\"}"));

    [Fact]
    public void ParseContextType_throws_when_no_json()
        => Assert.Throws<CommandException>(() => MigrationsBundleCommand.ParseContextType(""));

    [Fact]
    public void ParseContextType_throws_when_type_missing()
        => Assert.Throws<CommandException>(
            () => MigrationsBundleCommand.ParseContextType("{ \"providerName\": \"x\" }"));

    private sealed class Sink
    {
        public List<string> Messages { get; } = [];
        public List<string> Other { get; } = [];
        public StringBuilder Data { get; } = new();

        public void Handle(string? line)
            => MigrationsBundleCommand.HandleDiscoveryOutput(
                line,
                Data,
                Other,
                m => Messages.Add("error: " + m),
                m => Messages.Add("warn: " + m),
                m => Messages.Add("info: " + m),
                m => Messages.Add("verbose: " + m));
    }

    [Fact]
    public void HandleDiscoveryOutput_forwards_prefixed_lines_to_matching_handler()
    {
        var sink = new Sink();

        foreach (var line in new[]
                 {
                     Reporter.InfoPrefix + "Finding DbContext classes...",
                     Reporter.WarningPrefix + "careful",
                     Reporter.VerbosePrefix + "details",
                     Reporter.ErrorPrefix + "More than one DbContext was found. Use the '--context' option."
                 })
        {
            sink.Handle(line);
        }

        Assert.Equal(
            new[]
            {
                "info: Finding DbContext classes...",
                "warn: careful",
                "verbose: details",
                "error: More than one DbContext was found. Use the '--context' option."
            },
            sink.Messages);
        Assert.Equal(0, sink.Data.Length);
        Assert.Empty(sink.Other);
    }

    [Fact]
    public void HandleDiscoveryOutput_collects_data_payloads_without_reporting_them()
    {
        var sink = new Sink();

        sink.Handle(Reporter.DataPrefix + "{");
        sink.Handle("Now listening on: http://localhost:5000 { not json }");
        sink.Handle(Reporter.DataPrefix + "  \"type\": \"My.App.BlogContext\"");
        sink.Handle(null);
        sink.Handle(Reporter.DataPrefix + "}");

        Assert.Empty(sink.Messages);
        Assert.Equal(new[] { "Now listening on: http://localhost:5000 { not json }" }, sink.Other);
        Assert.Equal("My.App.BlogContext", MigrationsBundleCommand.ParseContextType(sink.Data.ToString()));
    }

    [Fact]
    public void HandleDiscoveryOutput_keeps_unprefixed_output_separate()
    {
        var sink = new Sink();

        sink.Handle("Unhandled exception. System.InvalidOperationException: boom");
        sink.Handle("");
        sink.Handle(null);

        Assert.Empty(sink.Messages);
        Assert.Equal(0, sink.Data.Length);
        Assert.Equal(new[] { "Unhandled exception. System.InvalidOperationException: boom" }, sink.Other);
    }

    [Fact]
    public void HandleDiscoveryOutput_handles_prefix_only_lines_trailing_carriage_returns_and_multi_line_errors()
    {
        var sink = new Sink();

        sink.Handle(Reporter.ErrorPrefix);
        sink.Handle(Reporter.ErrorPrefix + "first line\r");
        sink.Handle(Reporter.ErrorPrefix + "second line");

        Assert.Equal(new[] { "error: ", "error: first line", "error: second line" }, sink.Messages);
    }

    private sealed class TestBundleCommand : MigrationsBundleCommand
    {
        public string Discover(
            IReadOnlyList<string> applicationArgs,
            Func<IReadOnlyList<string>, Action<string?>, int> run,
            List<string> messages)
            => DiscoverContextType(
                applicationArgs,
                run,
                m => messages.Add("error: " + m),
                m => messages.Add("warn: " + m),
                m => messages.Add("info: " + m),
                m => messages.Add("verbose: " + m));

        protected override void Validate()
        {
        }

        protected override int Execute(string[] args)
            => 0;
    }

    private static TestBundleCommand CreateCommand(params string[] args)
    {
        var app = new CommandLineApplication { Name = "ef" };
        var command = new TestBundleCommand();
        command.Configure(app);
        app.Execute(args);
        return command;
    }

    [Fact]
    public void DiscoverContextType_returns_type_and_forwards_context_and_application_args()
    {
        var command = CreateCommand("--assembly", "A.dll", "--context", "My.Context", "--deps-file", "S.deps.json");
        IReadOnlyList<string>? launched = null;
        var messages = new List<string>();

        var type = command.Discover(
            new[] { "--environment", "Staging" },
            (args, handleLine) =>
            {
                launched = args;
                handleLine(Reporter.InfoPrefix + "Finding DbContext classes...");
                handleLine("startup banner { }");
                handleLine(Reporter.DataPrefix + "{ \"type\": \"My.App.BlogContext\" }");
                return 0;
            },
            messages);

        Assert.Equal("My.App.BlogContext", type);
        Assert.Contains("S.deps.json", launched!);
        Assert.Contains("--assembly", launched!);
        var contextIndex = launched!.ToList().IndexOf("--context");
        Assert.Equal("My.Context", launched![contextIndex + 1]);
        var separator = launched!.ToList().IndexOf("--");
        Assert.Equal(new[] { "--", "--environment", "Staging" }, launched!.Skip(separator));

        // Successful run: tagged output is reported, untagged output is demoted to verbose.
        Assert.Equal(new[] { "info: Finding DbContext classes...", "verbose: startup banner { }" }, messages);
    }

    [Fact]
    public void DiscoverContextType_reports_child_errors_then_throws_on_non_zero_exit()
    {
        var command = CreateCommand("--assembly", "A.dll");
        var messages = new List<string>();

        var ex = Assert.Throws<CommandException>(
            () => command.Discover(
                Array.Empty<string>(),
                (args, handleLine) =>
                {
                    handleLine(Reporter.ErrorPrefix + "More than one DbContext was found. Use the '--context' option.");
                    handleLine("Unhandled exception. System.InvalidOperationException: boom");
                    return 1;
                },
                messages));

        Assert.Equal(Properties.Resources.BundleContextDiscoveryFailed, ex.Message);
        Assert.Equal(
            new[]
            {
                "error: More than one DbContext was found. Use the '--context' option.",
                "error: Unhandled exception. System.InvalidOperationException: boom"
            },
            messages);
    }

    [Fact]
    public void DiscoverContextType_throws_when_child_succeeds_without_data()
    {
        var command = CreateCommand("--assembly", "A.dll");

        Assert.Throws<CommandException>(
            () => command.Discover(Array.Empty<string>(), (args, handleLine) => 0, []));
    }
}
