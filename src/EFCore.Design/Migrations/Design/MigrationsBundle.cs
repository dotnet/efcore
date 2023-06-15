// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tools;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Defines the entry point for Migrations bundles.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migration-bundles">EF Core migration bundles</see>.
/// </remarks>
public static class MigrationsBundle
{
    private static string? _context;
    private static Assembly? _assembly;
    private static Assembly? _startupAssembly;
    private static CommandArgument? _migration;
    private static CommandOption? _connection;

    /// <summary>
    ///     The entry point for Migrations bundles.
    /// </summary>
    /// <param name="context">The DbContext to use.</param>
    /// <param name="assembly">The Migrations assembly.</param>
    /// <param name="startupAssembly">The startup assembly.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>Zero if the command succeeds; otherwise, one.</returns>
    public static int Execute(string? context, Assembly assembly, Assembly startupAssembly, string[] args)
    {
        _context = context;
        _assembly = assembly;
        _startupAssembly = startupAssembly;

        var app = new CommandLineApplication { Name = "efbundle" };

        Configure(app);

        try
        {
            return app.Execute(args);
        }
        catch (Exception ex)
        {
            if (ex is CommandParsingException or OperationException)
            {
                Reporter.WriteVerbose(ex.ToString());
            }
            else
            {
                Reporter.WriteInformation(ex.ToString());
            }

            Reporter.WriteError(ex.Message);

            return 1;
        }
    }

    // Internal for testing
    internal static void Configure(CommandLineApplication app)
    {
        app.FullName = DesignStrings.BundleFullName;
        app.AllowArgumentSeparator = true;

        _migration = app.Argument("<MIGRATION>", DesignStrings.MigrationDescription);
        _connection = app.Option("--connection <CONNECTION>", DesignStrings.ConnectionDescription);

        app.VersionOption("--version", ProductInfo.GetVersion);
        app.HelpOption("-h|--help");
        var verbose = app.Option("-v|--verbose", DesignStrings.VerboseDescription);
        var noColor = app.Option("--no-color", DesignStrings.NoColorDescription);
        var prefixOutput = app.Option("--prefix-output", DesignStrings.PrefixDescription);

        app.HandleResponseFiles = true;

        app.OnExecute(
            args =>
            {
                Reporter.IsVerbose = verbose.HasValue();
                Reporter.NoColor = noColor.HasValue();
                Reporter.PrefixOutput = prefixOutput.HasValue();

                ExecuteInternal(args);

                return 0;
            });
    }

    private static void ExecuteInternal(string[] args)
        => new MigrationsOperations(
                new OperationReporter(
                    new OperationReportHandler(
                        Reporter.WriteError,
                        Reporter.WriteWarning,
                        Reporter.WriteInformation,
                        Reporter.WriteVerbose)),
                _assembly!,
                _startupAssembly!,
                projectDir: string.Empty,
                rootNamespace: null,
                language: null,
                nullable: false,
                args)
            .UpdateDatabase(_migration!.Value, _connection!.Value(), _context);
}
