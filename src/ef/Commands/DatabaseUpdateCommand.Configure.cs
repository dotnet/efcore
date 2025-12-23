// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class DatabaseUpdateCommand : ContextCommandBase
{
    private CommandArgument? _migration;
    private CommandOption? _connection;
    private CommandOption? _add;
    private CommandOption? _outputDir;
    private CommandOption? _namespace;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DatabaseUpdateDescription;

        _migration = command.Argument("<MIGRATION>", Resources.MigrationDescription);

        _connection = command.Option("--connection <CONNECTION>", Resources.DbContextConnectionDescription);
        _add = command.Option("--add <NAME>", Resources.DatabaseUpdateAddDescription);
        _outputDir = command.Option("-o|--output-dir <PATH>", Resources.MigrationsOutputDirDescription);
        _namespace = command.Option("-n|--namespace <NAMESPACE>", Resources.MigrationsNamespaceDescription);

        base.Configure(command);
    }
}
