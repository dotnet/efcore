// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

            command.Confirm(
                "Are you sure? (y/N)",
                () => Execute(context.Value(), startupProject.Value(), environment.Value()),
                () =>
                {
                    Reporter.Output.WriteLine(" Cancel droping database. Use -f|--force to skip.");
                    return 1;
                },
                () => force.HasValue());
        }

        private static int Execute(string context, string startupProject, string environment)
        {
            Reporter.Output.WriteLine(" Start droping database");

            new OperationExecutor(startupProject, environment)
                .DropDatabase(context);

            return 0;
        }
    }
}