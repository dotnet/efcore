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
    public class DbContextListCommand : ICommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "List your DbContext types";
            command.HelpOption();

            var json = command.JsonOption();

            command.OnExecute(() => { options.Command = new DbContextListCommand(json.HasValue()); });
        }

        private readonly bool _json;

        public DbContextListCommand(bool json)
        {
            _json = json;
        }

        public void Run(IOperationExecutor executor)
        {
            var types = executor.GetContextTypes();

            if (_json)
            {
                ReportJsonResults(types);
            }
            else
            {
                ReportResults(types);
            }
        }

        private static void ReportJsonResults(IEnumerable<IDictionary> contextTypes)
        {
            var nameGroups = contextTypes.GroupBy(t => t["Name"]).ToList();
            var fullNameGroups = contextTypes.GroupBy(t => t["FullName"]).ToList();

            var output = new StringBuilder();

            output.AppendLine(Reporter.JsonPrefix);
            output.Append("[");

            var first = true;
            foreach (var contextType in contextTypes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append(",");
                }
                var safeName = nameGroups.Count(g => g.Key == contextType["Name"]) == 1
                    ? contextType["Name"]
                    : fullNameGroups.Count(g => g.Key == contextType["FullName"]) == 1
                        ? contextType["FullName"]
                        : contextType["AssemblyQualifiedName"];

                output.AppendLine();
                output.AppendLine("  {");
                output.AppendLine("     \"fullName\": \"" + contextType["FullName"] + "\",");
                output.AppendLine("     \"safeName\": \"" + safeName + "\",");
                output.AppendLine("     \"name\": \"" + contextType["Name"] + "\",");
                output.AppendLine("     \"assemblyQualifiedName\": \"" + contextType["AssemblyQualifiedName"] + "\"");
                output.Append("  }");
            }

            output.AppendLine();
            output.AppendLine("]");
            output.AppendLine(Reporter.JsonSuffix);

            Reporter.Output(output.ToString());
        }

        private static void ReportResults(IEnumerable<IDictionary> contextTypes)
        {
            var any = false;
            foreach (var contextType in contextTypes)
            {
                Reporter.Output(contextType["FullName"] as string);
                any = true;
            }

            if (!any)
            {
                Reporter.Error("No DbContext was found");
            }
        }
    }
}
