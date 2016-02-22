// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class DbContextScaffoldCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Scaffolds a DbContext and entity type classes for a specified database";

            var connection = command.Argument(
                "[connection]",
                "The connection string of the database");
            var provider = command.Argument(
                "[provider]",
                "The provider to use. For example, EntityFramework.MicrosoftSqlServer");

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
                    if (string.IsNullOrEmpty(connection.Value))
                    {
                        Reporter.Error.WriteLine(("Missing required argument '" + connection.Name + "'").Bold().Red());
                        command.ShowHelp();

                        return 1;
                    }
                    if (string.IsNullOrEmpty(provider.Value))
                    {
                        Reporter.Error.WriteLine(("Missing required argument '" + provider.Name + "'").Bold().Red());
                        command.ShowHelp();

                        return 1;
                    }

                    return Execute(
                        connection.Value,
                        provider.Value,
                        dataAnnotations.HasValue(),
                        context.Value(),
                        force.HasValue(),
                        outputDir.Value(),
                        schemas.Values,
                        tables.Values,
                        startupProject.Value(),
                        environment.Value());
                });
        }

        private static int Execute(
            string connection,
            string provider,
            bool dataAnnotations,
            string context,
            bool force,
            string outputDir,
            IEnumerable<string> schemas,
            IEnumerable<string> tables,
            string startupProject,
            string environment)
        {
            new OperationExecutor(startupProject, environment)
                .ReverseEngineer(
                    provider,
                    connection,
                    outputDir,
                    context,
                    schemas,
                    tables,
                    dataAnnotations,
                    force);

            Reporter.Error.WriteLine("Done");

            return 0;
        }
    }
}