// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class MigrationsListCommand : ICommand
    {
        public static void ParseOptions([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "List the migrations";
            command.HelpOption();

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var json = command.JsonOption();

            command.OnExecute(() => { options.Command = new MigrationsListCommand(context.Value(), json.HasValue()); });
        }

        private readonly string _context;
        private readonly bool _json;

        public MigrationsListCommand(string context, bool json)
        {
            _context = context;
            _json = json;
        }

        public void Run(IOperationExecutor executor)
        {
            var migrations = executor.GetMigrations(_context);

            if (_json)
            {
                ReportJsonResults(migrations);
            }
            else
            {
                ReportResults(migrations);
            }
        }

        private static void ReportJsonResults(IEnumerable<IDictionary> migrations)
        {
            var nameGroups = migrations.GroupBy(m => m["Name"]).ToList();
            var output = new StringBuilder();
            output.AppendLine(Reporter.JsonPrefix);

            output.Append("[");

            var first = true;
            foreach (var m in migrations)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append(",");
                }

                var safeName = nameGroups.Count(g => g.Key == m["Name"]) == 1
                    ? m["Name"]
                    : m["Id"];

                output.AppendLine();
                output.AppendLine("  {");
                output.AppendLine("    \"id\": \"" + m["Id"] + "\",");
                output.AppendLine("    \"name\": \"" + m["Name"] + "\",");
                output.AppendLine("    \"safeName\": \"" + safeName + "\"");
                output.Append("  }");
            }

            output.AppendLine();
            output.AppendLine("]");
            output.AppendLine(Reporter.JsonSuffix);

            Reporter.Output(output.ToString());
        }

        private static void ReportResults(IEnumerable<IDictionary> migrations)
        {
            var any = false;
            foreach (var migration in migrations)
            {
                Reporter.Output(migration["Id"] as string);
                any = true;
            }

            if (!any)
            {
                Reporter.Error("No migrations were found");
            }
        }
    }
}
