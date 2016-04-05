// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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
                "-s|--startup-project <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var json = command.JsonOption();

            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(
                    startupProject.Value(),
                    environment.Value(),
                    json.HasValue()
                        ? (Action<IEnumerable<IDictionary>>)ReportJsonResults
                        : ReportResults)
                );
        }

        private static int Execute(
            string startupProject,
            string environment,
            Action<IEnumerable<IDictionary>> reportResultsAction)
        {
            var contextTypes = new ReflectionOperationExecutor(startupProject, environment)
                .GetContextTypes();

            reportResultsAction(contextTypes);

            return 0;
        }

        private static void ReportJsonResults(IEnumerable<IDictionary> contextTypes)
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
                Reporter.Output.Write("  { \"fullName\": \"" + contextType["FullName"] + "\" }");
            }

            Reporter.Output.WriteLine();
            Reporter.Output.WriteLine("]");
        }

        private static void ReportResults(IEnumerable<IDictionary> contextTypes)
        {
            var any = false;
            foreach (var contextType in contextTypes)
            {
                Reporter.Output.WriteLine(contextType["FullName"] as string);
                any = true;
            }

            if (!any)
            {
                Reporter.Error.WriteLine("No DbContext was found");
            }
        }
    }
}