// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class MigrationsListCommand
    {
        protected override int Execute()
        {
            var migrations = CreateExecutor().GetMigrations(Context.Value()).ToList();

            if (_json.HasValue())
            {
                ReportJsonResults(migrations);
            }
            else
            {
                ReportResults(migrations);
            }

            return base.Execute();
        }

        private static void ReportJsonResults(IReadOnlyList<IDictionary> migrations)
        {
            var nameGroups = migrations.GroupBy(m => m["Name"]).ToList();

            Reporter.WriteData("[");

            for (var i = 0; i < migrations.Count; i++)
            {
                var safeName = nameGroups.Count(g => g.Key == migrations[i]["Name"]) == 1
                    ? migrations[i]["Name"]
                    : migrations[i]["Id"];

                Reporter.WriteData("  {");
                Reporter.WriteData("    \"id\": \"" + migrations[i]["Id"] + "\",");
                Reporter.WriteData("    \"name\": \"" + migrations[i]["Name"] + "\",");
                Reporter.WriteData("    \"safeName\": \"" + safeName + "\"");

                var line = "  }";
                if (i != migrations.Count - 1)
                {
                    line += ",";
                }

                Reporter.WriteData(line);
            }

            Reporter.WriteData("]");
        }

        private static void ReportResults(IEnumerable<IDictionary> migrations)
        {
            var any = false;
            foreach (var migration in migrations)
            {
                Reporter.WriteData(migration["Id"] as string);
                any = true;
            }

            if (!any)
            {
                Reporter.WriteInformation(Resources.NoMigrations);
            }
        }
    }
}
