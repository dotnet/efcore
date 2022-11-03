// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class DatabaseDropCommand : ContextCommandBase
{
    private CommandOption? _force;
    private CommandOption? _dryRun;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DatabaseDropDescription;

        _force = command.Option("-f|--force", Resources.DatabaseDropForceDescription);
        _dryRun = command.Option("--dry-run", Resources.DatabaseDropDryRunDescription);

        base.Configure(command);
    }
}
