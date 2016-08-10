// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class MigrationsCommand
    {
        public static void Configure(CommandLineApplication command, CommandLineOptions options)
        {
            command.Description = "Commands to manage your migrations";
            command.HelpOption();

            command.Command("add", c => MigrationsAddCommand.Configure(c, options));
            command.Command("list", c => MigrationsListCommand.Configure(c, options));
            command.Command("remove", c => MigrationsRemoveCommand.Configure(c, options));
            command.Command("script", c => MigrationsScriptCommand.Configure(c, options));
            command.OnExecute(() =>
                {
                    EfCommand.WriteLogo();
                    command.ShowHelp();
                });
        }
    }
}
