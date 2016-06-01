// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class MigrationsListCommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommonCommandOptions commonOptions)
        {
            command.Description = "List the migrations";

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var json = command.JsonOption();

            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(commonOptions.Value(),
                    context.Value(),
                    environment.Value(),
                    json.HasValue()
                        ? (Action<IEnumerable<MigrationInfo>>)ReportJsonResults
                        : ReportResults));
        }

        private static int Execute(CommonOptions commonOptions,
            string context,
            string environment,
            Action<IEnumerable<MigrationInfo>> reportResultsAction)
        {
            var migrations = new OperationExecutor(commonOptions, environment)
                .GetMigrations(context);

            reportResultsAction(migrations);

            return 0;
        }

        private static void ReportJsonResults(IEnumerable<MigrationInfo> migrations)
        {
            var nameGroups = migrations.GroupBy(m => m.Name).ToList();
            var output = migrations.Select(
                m => new
                {
                    id = m.Id,
                    name = m.Name,
                    safeName = nameGroups.Count(g => g.Key == m.Name) == 1
                        ? m.Name
                        : m.Id
                }).ToArray();

            ConsoleCommandLogger.Json(output);
        }

        private static void ReportResults(IEnumerable<MigrationInfo> migrations)
        {
            var any = false;
            foreach (var migration in migrations)
            {
                ConsoleCommandLogger.Output(migration.Id as string);
                any = true;
            }

            if (!any)
            {
                ConsoleCommandLogger.Error("No migrations were found");
            }
        }
    }
}
