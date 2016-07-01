// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class DbContextListCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
        {
            command.Description = "List your DbContext types";

            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var json = command.JsonOption();

            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(commonOptions.Value(),
                    environment.Value(),
                    json.HasValue()
                        ? (Action<IEnumerable<Type>>)ReportJsonResults
                        : ReportResults)
                );
        }

        private static int Execute(CommonOptions commonOptions,
            string environment,
            Action<IEnumerable<Type>> reportResultsAction)
        {
            var contextTypes = new OperationExecutor(commonOptions, environment)
                .GetContextTypes();

            reportResultsAction(contextTypes);

            return 0;
        }

        private static void ReportJsonResults(IEnumerable<Type> contextTypes)
        {
            var nameGroups = contextTypes.GroupBy(t => t.Name).ToList();
            var fullNameGroups = contextTypes.GroupBy(t => t.FullName).ToList();

            var output = new StringBuilder();

            output.AppendLine("//BEGIN");
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
                var safeName = nameGroups.Count(g => g.Key == contextType.Name) == 1
                    ? contextType.Name
                    : fullNameGroups.Count(g => g.Key == contextType.FullName) == 1
                        ? contextType.FullName
                        : contextType.AssemblyQualifiedName;

                output.AppendLine();
                output.AppendLine("  {");
                output.AppendLine("     \"fullName\": \"" + contextType.FullName + "\",");
                output.AppendLine("     \"safeName\": \"" + safeName + "\",");
                output.AppendLine("     \"name\": \"" + contextType.Name + "\",");
                output.AppendLine("     \"assemblyQualifiedName\": \"" + contextType.AssemblyQualifiedName + "\"");
                output.AppendLine("  }");
            }

            output.AppendLine();
            output.AppendLine("]");
            output.AppendLine("//END");

            ConsoleCommandLogger.Output(output.ToString());
        }

        private static void ReportResults(IEnumerable<Type> contextTypes)
        {
            var any = false;
            foreach (var contextType in contextTypes)
            {
                ConsoleCommandLogger.Output(contextType.FullName as string);
                any = true;
            }

            if (!any)
            {
                ConsoleCommandLogger.Error("No DbContext was found");
            }
        }
    }
}
