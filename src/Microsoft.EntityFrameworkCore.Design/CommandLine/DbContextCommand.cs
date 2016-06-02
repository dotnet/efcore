// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class DbContextCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
        {
            command.Description = "Commands to manage your DbContext types";

            command.HelpOption();
            command.VerboseOption();

            command.Command("list", c => DbContextListCommand.Configure(c, commonOptions));
            command.Command("scaffold", c => DbContextScaffoldCommand.Configure(c, commonOptions));

            command.OnExecute(() => command.ShowHelp());
        }
    }
}
