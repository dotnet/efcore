// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class DbContextCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Commands to manage your DbContext types";

            command.HelpOption("-h|--help");

            command.Command("list", DbContextListCommand.Configure);
            command.Command("scaffold", DbContextScaffoldCommand.Configure);

            command.OnExecute(() => command.ShowHelp());
        }
    }
}
