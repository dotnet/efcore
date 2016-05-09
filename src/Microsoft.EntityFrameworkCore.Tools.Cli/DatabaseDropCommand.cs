// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class DatabaseDropCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
        {
            command.Description = "Drop the database for specific environment";

            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var force = command.Option(
                "-f|--force",
                "Drop without confirmation");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(commonOptions.Value(),
                    context.Value(),
                    environment.Value(),
                    force.HasValue()));
        }

        private static int Execute(CommonOptions commonOptions,
            string context,
            string environment,
            bool isForced)
        {
            new OperationExecutor(commonOptions, environment)
                .DropDatabase(
                    context,
                    (database, dataSource) =>
                    {
                        if (isForced)
                        {
                            return true;
                        }

                        Reporter.Output.WriteLine(
                            $"Are you sure you want to drop the database '{database}' on server '{dataSource}'? (y/N)");
                        var readedKey = Console.ReadKey().KeyChar;

                        return (readedKey == 'y') || (readedKey == 'Y');
                    });

            return 0;
        }
    }
}
