// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DbContextCommand
    {
        public static void Configure(CommandLineApplication command, CommandLineOptions options)
        {
            command.Description = "Commands to manage your DbContext types";
            command.HelpOption();

            command.Command("list", c => DbContextListCommand.Configure(c, options));
            command.Command("scaffold", c => DbContextScaffoldCommand.Configure(c, options));
            command.OnExecute(() =>
            {
                EfCommand.WriteLogo();
                command.ShowHelp();
            });
        }
    }
}