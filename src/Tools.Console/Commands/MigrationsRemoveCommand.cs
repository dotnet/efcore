// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class MigrationsRemoveCommand : ICommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "Remove the last migration";
            command.HelpOption();

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var force = command.Option(
                "-f|--force",
                "Removes the last migration without checking the database. If the last migration has been applied to the database, you will need to manually reverse the changes it made.");
            var json = command.JsonOption();

            command.OnExecute(() => { options.Command = new MigrationsRemoveCommand(context.Value(), force.HasValue(), json.HasValue()); });
        }

        private readonly string _context;
        private readonly bool _force;
        private readonly bool _json;

        public MigrationsRemoveCommand(string context, bool force, bool json)
        {
            _context = context;
            _force = force;
            _json = json;
        }

        public void Run(IOperationExecutor executor)
        {
            var deletedFiles = executor.RemoveMigration(_context, _force);
            if (_json)
            {
                ReportJsonResults(deletedFiles);
            }
        }

        private void ReportJsonResults(IEnumerable<string> files)
        {
            var output = new StringBuilder();
            output.AppendLine(Reporter.JsonPrefix);
            output.AppendLine("{");
            output.AppendLine("  \"files\": [");
            var first = true;
            foreach (var file in files)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.AppendLine(",");
                }

                output.Append("    \"" + SerializePath(file) + "\"");
            }
            output.AppendLine();
            output.AppendLine("  ]");
            output.AppendLine("}");
            output.AppendLine(Reporter.JsonSuffix);
            Reporter.Output(output.ToString());
        }

        private static string SerializePath(string path)
            => path?.Replace("\\", "\\\\");
    }
}
