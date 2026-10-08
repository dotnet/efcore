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
    private CommandOption? _json;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DatabaseUpdateDescription;

        _migration = command.Argument("<MIGRATION>", Resources.MigrationDescription);

        _connection = command.Option("--connection <CONNECTION>", Resources.DbContextConnectionDescription);
        _add = command.Option("--add", Resources.DatabaseUpdateAddDescription);
        _outputDir = command.Option("-o|--output-dir <PATH>", Resources.MigrationsOutputDirDescription);
        _namespace = command.Option("-n|--namespace <NAMESPACE>", Resources.MigrationsNamespaceDescription);
        _json = Json.ConfigureOption(command);

        base.Configure(command);
    }

    protected override void Validate()
    {
        base.Validate();

        if (_add!.HasValue())
        {
            if (string.IsNullOrEmpty(_migration!.Value))
            {
                throw new CommandException(Resources.MissingArgument(_migration.Name));
            }
        }
        else
        {
            if (_outputDir!.HasValue())
            {
                throw new CommandException(
                    Resources.MissingConditionalOption(_add!.LongName, _outputDir.LongName));
            }

            if (_namespace!.HasValue())
            {
                throw new CommandException(
                    Resources.MissingConditionalOption(_add!.LongName, _namespace.LongName));
            }

            if (_json!.HasValue())
            {
                throw new CommandException(
                    Resources.MissingConditionalOption(_add!.LongName, _json.LongName));
            }
        }
    }
}
