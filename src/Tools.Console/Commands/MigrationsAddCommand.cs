// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class MigrationsAddCommand : ICommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "Add a new migration";
            command.HelpOption();

            var name = command.Argument("[name]", "The name of the migration");

            var outputDir = command.Option(
                "-o|--output-dir <path>",
                "The directory (and sub-namespace) to use. If omitted, \"Migrations\" is used. Relative paths are relative the directory in which the command is executed.");
            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var json = command.JsonOption();

            command.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(name.Value))
                    {
                        Reporter.Error("Missing required argument '" + name.Name + "'");
                        return 1;
                    }

                    options.Command = new MigrationsAddCommand(name.Value,
                        outputDir.Value(),
                        context.Value(),
                        json.HasValue());

                    return 0;
                });
        }

        private readonly string _name;
        private readonly string _outputDir;
        private readonly string _context;
        private readonly bool _json;

        public MigrationsAddCommand(string name, string outputDir, string context, bool json)
        {
            _name = name;
            _outputDir = outputDir;
            _context = context;
            _json = json;
        }

        public void Run(IOperationExecutor executor)
        {
            var files = executor.AddMigration(_name, _outputDir, _context);

            if (_json)
            {
                ReportJson(files);
            }
            else
            {
                Reporter.Output("Done. To undo this action, use 'dotnet ef migrations remove'");
            }
        }

        private static void ReportJson(IDictionary files)
        {
            var output = new StringBuilder();
            output.AppendLine(Reporter.JsonPrefix);
            output.AppendLine("{");
            output.AppendLine("  \"MigrationFile\": \"" + SerializePath(files["MigrationFile"] as string) + "\",");
            output.AppendLine("  \"MetadataFile\": \"" + SerializePath(files["MetadataFile"] as string) + "\",");
            output.AppendLine("  \"SnapshotFile\": \"" + SerializePath(files["SnapshotFile"] as string) + "\"");
            output.AppendLine("}");
            output.AppendLine(Reporter.JsonSuffix);
            Reporter.Output(output.ToString());
        }

        private static string SerializePath(string path)
            => path?.Replace("\\", "\\\\");
    }
}
