// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class MigrationsCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Commands to manage your migrations";

            command.HelpOption();
            command.VerboseOption();

            command.Command("add", MigrationsAddCommand.Configure);
            command.Command("list", MigrationsListCommand.Configure);
            command.Command("remove", MigrationsRemoveCommand.Configure);
            command.Command("script", MigrationsScriptCommand.Configure);

            command.OnExecute(() => command.ShowHelp());
        }
    }
}
