// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class MigrationsScriptCommand : ICommand
    {
        public static void ParseOptions([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "Generate a SQL script from migrations";
            command.HelpOption();

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

            command.OnExecute(() =>
                {
                    options.Command = new MigrationsScriptCommand(from.Value,
                        to.Value,
                        context.Value(),
                        idempotent.HasValue(),
                        output.Value());
                });
        }

        private readonly string _from;
        private readonly string _to;
        private readonly bool _idempotent;
        private readonly string _context;
        private readonly string _outputFile;

        public MigrationsScriptCommand(
            [CanBeNull] string from,
            [CanBeNull] string to,
            [CanBeNull] string context,
            bool idempotent,
            [CanBeNull] string outputFile)
        {
            _from = from;
            _to = to;
            _idempotent = idempotent;
            _outputFile = outputFile;
            _context = context;
        }

        public void Run(IOperationExecutor executor)
        {
            var generated = executor.ScriptMigration(_from, _to, _idempotent, _context);

            ReportResult(generated);
        }

        private void ReportResult(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new OperationErrorException("No sql was generated");
            }

            if (string.IsNullOrEmpty(_outputFile))
            {
                Reporter.Output(sql);
            }
            else
            {
                var directory = Path.GetDirectoryName(_outputFile);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Reporter.Verbose("Writing SQL script to '" + _outputFile + "'");
                File.WriteAllText(_outputFile, sql, Encoding.UTF8);

                Reporter.Output("Done");
            }
        }
    }
}
