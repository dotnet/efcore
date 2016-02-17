// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class DbContextListCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "List your DbContext types";

            var startupProject = command.Option(
                "-s|--startupProject <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var json = command.Option(
                "--json",
                "Use json output");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(
                    startupProject.Value(),
                    environment.Value(),
                    json.HasValue()
                        ? (Action<IEnumerable<Type>>)ReportJsonResults
                        : ReportResults)
                );
        }

        private static int Execute(
            string startupProject,
            string environment,
            Action<IEnumerable<Type>> reportResultsAction)
        {
            var contextTypes = new OperationExecutor(startupProject, environment)
                .GetContextTypes();

            reportResultsAction(contextTypes);

            return 0;
        }

        private static void ReportJsonResults(IEnumerable<Type> contextTypes)
        {
            Reporter.Output.Write("[");

            var first = true;
            foreach (var contextType in contextTypes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Reporter.Output.Write(",");
                }

                Reporter.Output.WriteLine();
                Reporter.Output.Write("  { \"fullName\": \"" + contextType.FullName + "\" }");
            }

            Reporter.Output.WriteLine();
            Reporter.Output.WriteLine("]");
        }

        private static void ReportResults(IEnumerable<Type> contextTypes)
        {
            var any = false;
            foreach (var contextType in contextTypes)
            {
                Reporter.Output.WriteLine(contextType.FullName);
                any = true;
            }

            if (!any)
            {
                Reporter.Error.WriteLine("No DbContext was found");
            }
        }
    }
}