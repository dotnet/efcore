// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class MigrationsCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
        {
            command.Description = "Commands to manage your migrations";

            command.HelpOption();
            command.VerboseOption();

            command.Command("add", c => MigrationsAddCommand.Configure(c, commonOptions));
            command.Command("list", c => MigrationsListCommand.Configure(c, commonOptions));
            command.Command("remove", c => MigrationsRemoveCommand.Configure(c, commonOptions));
            command.Command("script", c => MigrationsScriptCommand.Configure(c, commonOptions));

            command.OnExecute(() => command.ShowHelp());
        }
    }
}
