// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DbContextScaffoldCommand : ICommand
    {
        public static void ParseOptions([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "Scaffolds a DbContext and entity type classes for a specified database";
            command.HelpOption();

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

            command.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(connection.Value))
                    {
                        Reporter.Error("Missing required argument '" + connection.Name + "'");
                        return 1;
                    }
                    if (string.IsNullOrEmpty(provider.Value))
                    {
                        Reporter.Error("Missing required argument '" + provider.Name + "'");
                        return 1;
                    }

                    options.Command = new DbContextScaffoldCommand(
                        provider.Value,
                        connection.Value,
                        outputDir.Value(),
                        context.Value(),
                        schemas.Values,
                        tables.Values,
                        dataAnnotations.HasValue(),
                        force.HasValue()
                        );

                    return 0;
                });
        }

        private readonly string _provider;
        private readonly string _connection;
        private readonly string _outputDir;
        private readonly string _context;
        private readonly IEnumerable<string> _schemas;
        private readonly IEnumerable<string> _tables;
        private readonly bool _dataAnnotations;
        private readonly bool _force;

        public DbContextScaffoldCommand(string provider,
            string connection,
            string outputDir,
            string context,
            IEnumerable<string> schemas,
            IEnumerable<string> tables,
            bool dataAnnotations,
            bool force)
        {
            _provider = provider;
            _connection = connection;
            _outputDir = outputDir;
            _context = context;
            _schemas = schemas;
            _tables = tables;
            _dataAnnotations = dataAnnotations;
            _force = force;
        }

        public void Run(IOperationExecutor executor)
            => executor.ReverseEngineer(_provider, _connection, _outputDir, _context, _schemas, _tables, _dataAnnotations, _force);
    }
}
