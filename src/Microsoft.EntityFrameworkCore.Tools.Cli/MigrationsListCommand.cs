// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class MigrationsListCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "List the migrations";

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var startupProject = command.Option(
                "-s|--startup-project <project>",
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
                    context.Value(),
                    startupProject.Value(),
                    environment.Value(),
                    json.HasValue()
                        ? (Action<IEnumerable<MigrationInfo>>)ReportJsonResults
                        : ReportResults));
        }

        private static int Execute(
            string context,
            string startupProject,
            string environment,
            Action<IEnumerable<MigrationInfo>> reportResultsAction)
        {
            var migrations = new OperationExecutor(startupProject, environment)
                .GetMigrations(context);

            reportResultsAction(migrations);

            return 0;
        }

        private static void ReportJsonResults(IEnumerable<MigrationInfo> migrations)
        {
            var output = migrations.Select(m => new { id = m.Id, name = m.Name }).ToArray();

            Reporter.Output.Write(JsonConvert.SerializeObject(output, Formatting.Indented));
        }

        private static void ReportResults(IEnumerable<MigrationInfo> migrations)
        {
            var any = false;
            foreach (var migration in migrations)
            {
                Reporter.Output.WriteLine(migration.Id as string);
                any = true;
            }

            if (!any)
            {
                Reporter.Error.WriteLine("No migrations were found");
            }
        }
    }
}
