// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class MigrationsScriptCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
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
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");

            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(commonOptions.Value(),
                    from.Value,
                    to.Value,
                    output.Value(),
                    idempotent.HasValue(),
                    context.Value(),
                    environment.Value()));
        }

        private static int Execute(CommonOptions commonOptions,
            string from,
            string to,
            string output,
            bool idempotent,
            string context,
            string environment)
        {
            var sql = new OperationExecutor(commonOptions, environment)
                .ScriptMigration(from, to, idempotent, context);

            if (string.IsNullOrEmpty(output))
            {
                ConsoleCommandLogger.Output(sql);
            }
            else
            {
                ConsoleCommandLogger.Verbose("Writing SQL script to '" + output + "'".Bold().Black());
                File.WriteAllText(output, sql, Encoding.UTF8);

                ConsoleCommandLogger.Output("Done");
            }

            return 0;
        }
    }
}
