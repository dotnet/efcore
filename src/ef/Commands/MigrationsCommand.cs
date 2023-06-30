// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal class MigrationsCommand : HelpCommandBase
{
    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.MigrationsDescription;

        command.Command("add", new MigrationsAddCommand().Configure);
        command.Command("bundle", new MigrationsBundleCommand().Configure);
        command.Command("has-pending-model-changes", new MigrationsHasPendingModelChangesCommand().Configure);
        command.Command("list", new MigrationsListCommand().Configure);
        command.Command("remove", new MigrationsRemoveCommand().Configure);
        command.Command("script", new MigrationsScriptCommand().Configure);

        base.Configure(command);
    }
}
