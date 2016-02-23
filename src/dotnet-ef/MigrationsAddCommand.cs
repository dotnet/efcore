// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class MigrationsAddCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Add a new migration";

            var name = command.Argument("[name]", "The name of the migration");

            var outputDir = command.Option(
                "-o|--output-dir <path>",
                "The directory (and sub-namespace) to use. If omitted, \"Migrations\" is used.");
            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var startupProject = command.Option(
                "-s|--startup-project <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () =>
                {
                    if (string.IsNullOrEmpty(name.Value))
                    {
                        Reporter.Error.WriteLine(("Missing required argument '" + name.Name + "'").Bold().Red());
                        command.ShowHelp();

                        return 1;
                    }

                    return Execute(
                        name.Value,
                        outputDir.Value(),
                        context.Value(),
                        startupProject.Value(),
                        environment.Value());
                });
        }

        private static int Execute(
            string name,
            string outputDir,
            string context,
            string startupProject,
            string environment)
        {
            new OperationExecutor(startupProject, environment)
                .AddMigration(name, outputDir, context);

            Reporter.Error.WriteLine("Done. To undo this action, use 'ef migrations remove'");

            return 0;
        }
    }
}
