// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class DatabaseDeleteCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Drop the database for specific enviroment";

            var startupProject = command.Option(
                "-s|--startup-project <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");

            var force = command.Option(
                "-f|--force",
                "Force confirm message. If omitted, confirm message not used");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(context.Value(), startupProject.Value(), environment.Value(), force.HasValue()));
        }

        private static int Execute(string context, string startupProject, string environment, bool isForced)
        {
            new OperationExecutor(startupProject, environment)
                .DropDatabase(
                    context,
                    message =>
                    {
                        Reporter.Output.WriteLine(message);
                        var readedKey = Console.ReadKey().KeyChar;

                        return isForced || (readedKey == 'y') || (readedKey == 'Y');
                    });

            return 0;
        }
    }
}
