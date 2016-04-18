// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class DatabaseCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Commands to manage your database";

            command.HelpOption();
            command.VerboseOption();

            command.Command("update", DatabaseUpdateCommand.Configure);
            command.Command("drop", DatabaseDropCommand.Configure);

            command.OnExecute(() => command.ShowHelp());
        }
    }
}
