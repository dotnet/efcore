// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class DbContextScaffoldCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
        {
            command.Description = "Scaffolds a DbContext and entity type classes for a specified database";

            var connection = command.Argument(
                "[connection]",
                "The connection string of the database");
            var provider = command.Argument(
                "[provider]",
                "The provider to use. For example, Microsoft.EntityFrameworkCore.SqlServer");

            var dataAnnotations = command.Option(
                "-a|--data-annotations",
                "Use DataAnnotation attributes to configure the model where possible. If omitted, the output code will use only the fluent API.",
                CommandOptionType.NoValue);
            var context = command.Option(
                "-c|--context <name>",
                "Name of the generated DbContext class.");
            var force = command.Option(
                "-f|--force",
                "Force scaffolding to overwrite existing files. Otherwise, the code will only proceed if no output files would be overwritten.",
                CommandOptionType.NoValue);
            var outputDir = command.Option(
                "-o|--output-dir <path>",
                "Directory of the project where the classes should be output. If omitted, the top-level project directory is used.");
            var schemas = command.Option(
                "--schema <schema>",
                "Selects a schema for which to generate classes.",
                CommandOptionType.MultipleValue);
            var tables = command.Option(
                "-t|--table <schema.table>",
                "Selects a table for which to generate classes.",
                CommandOptionType.MultipleValue);
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () =>
                {
                    if (string.IsNullOrEmpty(connection.Value))
                    {
                        ConsoleCommandLogger.Error(("Missing required argument '" + connection.Name + "'").Bold().Red());
                        command.ShowHelp();

                        return Task.FromResult(1);
                    }
                    if (string.IsNullOrEmpty(provider.Value))
                    {
                        ConsoleCommandLogger.Error(("Missing required argument '" + provider.Name + "'").Bold().Red());
                        command.ShowHelp();

                        return Task.FromResult(1);
                    }

                    return ExecuteAsync(commonOptions.Value(),
                        connection.Value,
                        provider.Value,
                        dataAnnotations.HasValue(),
                        context.Value(),
                        force.HasValue(),
                        outputDir.Value(),
                        schemas.Values,
                        tables.Values,
                        environment.Value());
                });
        }

        private static async Task<int> ExecuteAsync(CommonOptions commonOptions,
            string connection,
            string provider,
            bool dataAnnotations,
            string context,
            bool force,
            string outputDir,
            IEnumerable<string> schemas,
            IEnumerable<string> tables,
            string environment)
        {
            await new OperationExecutor(commonOptions, environment)
                .ReverseEngineerAsync(
                    provider,
                    connection,
                    outputDir,
                    context,
                    schemas,
                    tables,
                    dataAnnotations,
                    force);

            ConsoleCommandLogger.Output("Done");

            return 0;
        }
    }
}
