// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DatabaseCommand
    {
        public static void Configure(CommandLineApplication command, CommandLineOptions options)
        {
            command.Description = "Commands to manage your database";
            command.HelpOption();

            command.Command("update", c => DatabaseUpdateCommand.Configure(c, options));
            command.Command("drop", c => DatabaseDropCommand.Configure(c, options));

            command.OnExecute(() =>
                {
                    EfCommand.WriteLogo();
                    command.ShowHelp();
                });
        }
    }
}
