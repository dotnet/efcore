// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Tools.Commands;

namespace Microsoft.EntityFrameworkCore.Tools;

public class MigrationsBundleDiscoveryTest
{
    private static string DataLines(params string[] jsonLines)
        => string.Join("\n", jsonLines.Select(l => Reporter.DataPrefix + l));

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
    public void ParseContextType_reads_type_from_prefixed_data_lines()
    {
        var output = DataLines("{", "  \"type\": \"My.App.BlogContext\",", "  \"providerName\": \"x\"", "}");
        Assert.Equal("My.App.BlogContext", MigrationsBundleCommand.ParseContextType(output));
    }

    [Fact]
    public void ParseContextType_ignores_unprefixed_app_stdout_even_with_braces()
    {
        // The user's app/host writes to the same stdout during discovery. Lines without the data
        // prefix — even ones containing braces — must not corrupt the parse. Regression test for #25555.
        var output =
            "Now listening on: http://localhost:5000 { not json }\n"
            + DataLines("{", "  \"type\": \"My.App.BlogContext\",", "  \"providerName\": \"x\"", "}")
            + "\n[12:00:00 INF] request finished }";
        Assert.Equal("My.App.BlogContext", MigrationsBundleCommand.ParseContextType(output));
    }

    [Fact]
    public void ParseContextType_throws_when_no_data_lines()
        => Assert.Throws<CommandException>(
            () => MigrationsBundleCommand.ParseContextType("startup banner { with braces }\nplain app output"));

    [Fact]
    public void ParseContextType_throws_when_type_missing()
        => Assert.Throws<CommandException>(
            () => MigrationsBundleCommand.ParseContextType(DataLines("{", "  \"providerName\": \"x\"", "}")));

    [Fact]
    public void ExtractErrorMessage_returns_error_prefixed_text()
    {
        var lines = new[]
        {
            Reporter.InfoPrefix + "Finding DbContext classes...",
            Reporter.ErrorPrefix + "More than one DbContext was found. Use the '--context' option.",
            null
        };
        Assert.Equal(
            "More than one DbContext was found. Use the '--context' option.",
            MigrationsBundleCommand.ExtractErrorMessage(lines));
    }

    [Fact]
    public void ExtractErrorMessage_joins_multiple_error_lines()
    {
        var lines = new[]
        {
            Reporter.ErrorPrefix + "Unable to create a 'DbContext' of type 'X'.",
            Reporter.ErrorPrefix + "See https://go.microsoft.com/fwlink/?linkid=851728 for details."
        };
        Assert.Equal(
            "Unable to create a 'DbContext' of type 'X'." + Environment.NewLine
            + "See https://go.microsoft.com/fwlink/?linkid=851728 for details.",
            MigrationsBundleCommand.ExtractErrorMessage(lines));
    }

    [Fact]
    public void ExtractErrorMessage_returns_null_when_no_error_lines()
        => Assert.Null(MigrationsBundleCommand.ExtractErrorMessage(
            new[] { Reporter.InfoPrefix + "info", Reporter.DataPrefix + "data" }));

    [Fact]
    public void CreateDiscoveryFailure_surfaces_error_from_stdout()
    {
        // Under --prefix-output the child's error lines arrive on STDOUT, interleaved with data/info.
        // The failure must scan stdout (the captured output), not the stderr collection.
        var stdout = string.Join(
            "\n",
            Reporter.InfoPrefix + "Finding DbContext classes...",
            Reporter.ErrorPrefix + "More than one DbContext was found. Use the '--context' option.");

        var ex = MigrationsBundleCommand.CreateDiscoveryFailure(stdout, Array.Empty<string?>());

        Assert.Contains("More than one DbContext was found. Use the '--context' option.", ex.Message);
    }

    [Fact]
    public void CreateDiscoveryFailure_falls_back_to_stderr_when_stdout_has_no_error()
    {
        var ex = MigrationsBundleCommand.CreateDiscoveryFailure(
            Reporter.InfoPrefix + "no error here",
            new[] { Reporter.ErrorPrefix + "Unhandled crash text." });

        Assert.Contains("Unhandled crash text.", ex.Message);
    }

    [Fact]
    public void CreateDiscoveryFailure_is_generic_when_no_error_lines_anywhere()
    {
        var ex = MigrationsBundleCommand.CreateDiscoveryFailure("plain noise\nmore noise", Array.Empty<string?>());

        Assert.Equal(Properties.Resources.BundleContextDiscoveryFailed, ex.Message);
    }

    [Fact]
    public void CreateDiscoveryFailure_falls_back_to_raw_stderr_when_unprefixed()
    {
        // Host-level failures (framework roll-forward, missing runtime, unhandled exceptions) write
        // unprefixed text to stderr. It must still reach the user rather than collapsing into the
        // generic message.
        var ex = MigrationsBundleCommand.CreateDiscoveryFailure(
            Reporter.InfoPrefix + "no error here",
            new[] { "A fatal error was encountered.", "The required framework was not found." });

        Assert.Contains("A fatal error was encountered.", ex.Message);
        Assert.Contains("The required framework was not found.", ex.Message);
    }

    [Fact]
    public void CreateDiscoveryFailure_prefers_prefixed_stderr_over_raw()
    {
        var ex = MigrationsBundleCommand.CreateDiscoveryFailure(
            Reporter.InfoPrefix + "no error here",
            new[] { "noise line", Reporter.ErrorPrefix + "Actionable error." });

        Assert.Contains("Actionable error.", ex.Message);
        Assert.DoesNotContain("noise line", ex.Message);
    }
}
