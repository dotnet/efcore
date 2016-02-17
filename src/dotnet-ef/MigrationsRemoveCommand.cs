// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class MigrationsRemoveCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Remove the last migration";

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var startupProject = command.Option(
                "-s|--startupProject <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(
                    context.Value(),
                    startupProject.Value(),
                    environment.Value()));
        }

        private static int Execute(string context, string startupProject, string environment)
        {
            new OperationExecutor(startupProject, environment)
                .RemoveMigration(context);

            return 0;
        }
    }
}