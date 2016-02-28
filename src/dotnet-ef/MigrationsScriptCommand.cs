// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class MigrationsScriptCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Generate a SQL script from migrations";

            var from = command.Argument(
                "[from]",
                "The starting migration. If omitted, '0' (the initial database) is used");
            var to = command.Argument(
                "[to]",
                "The ending migration. If omitted, the last migration is used");

            var output = command.Option(
                "-o|--output <file>",
                "The file to write the script to instead of stdout");
            var idempotent = command.Option(
                "-i|--idempotent",
                "Generates an idempotent script that can used on a database at any migration");
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
                () => Execute(
                    from.Value,
                    to.Value,
                    output.Value(),
                    idempotent.HasValue(),
                    context.Value(),
                    startupProject.Value(),
                    environment.Value()));
        }

        private static int Execute(
            string from,
            string to,
            string output,
            bool idempotent,
            string context,
            string startupProject,
            string environment)
        {
            var sql = new OperationExecutor(startupProject, environment)
                .ScriptMigration(from, to, idempotent, context);

            if (string.IsNullOrWhiteSpace(sql))
            {
                Reporter.Error.WriteLine("There is no migration");

                return 0;
            }

            if (string.IsNullOrEmpty(output))
            {
                Reporter.Output.WriteLine(sql);
            }
            else
            {
                Reporter.Verbose.WriteLine("Writing SQL script to '" + output + "'".Bold().Black());
                File.WriteAllText(output, sql);

                Reporter.Error.WriteLine("Done");
            }

            return 0;
        }
    }
}