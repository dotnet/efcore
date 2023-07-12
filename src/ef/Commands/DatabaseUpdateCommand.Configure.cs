// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class DatabaseUpdateCommand : ContextCommandBase
{
    private CommandArgument? _migration;
    private CommandOption? _connection;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DatabaseUpdateDescription;

        _migration = command.Argument("<MIGRATION>", Resources.MigrationDescription);

        _connection = command.Option("--connection <CONNECTION>", Resources.DbContextConnectionDescription);

        base.Configure(command);
    }
}
