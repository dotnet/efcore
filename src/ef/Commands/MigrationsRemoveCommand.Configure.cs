// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsRemoveCommand : ContextCommandBase
{
    private CommandOption? _force;
    private CommandOption? _json;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.MigrationsRemoveDescription;

        _force = command.Option("-f|--force", Resources.MigrationsRemoveForceDescription);
        _json = Json.ConfigureOption(command);

        base.Configure(command);
    }
}
