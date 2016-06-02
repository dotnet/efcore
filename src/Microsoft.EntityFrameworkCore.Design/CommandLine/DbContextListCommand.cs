// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
            var typeNames = contextTypes.Select(t => new { fullName = t.FullName }).ToArray();

            ConsoleCommandLogger.Json(typeNames);
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
