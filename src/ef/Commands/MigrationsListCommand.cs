// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class MigrationsListCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var migrations = executor.GetMigrations(Context!.Value(), _connection!.Value(), _noConnect!.HasValue()).ToList();

        if (_json!.HasValue())
        {
            ReportJsonResults(migrations);
        }
        else
        {
            ReportResults(migrations);
        }

        return base.Execute(args);
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
            Reporter.WriteData("    \"safeName\": \"" + safeName + "\",");
            Reporter.WriteData("    \"applied\": " + Json.Literal(migrations[i]["Applied"] as bool?));

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
        var anyUnknown = false;
        var any = false;
        foreach (var migration in migrations)
        {
            var id = migration["Id"] as string;
            var applied = migration["Applied"] as bool?;
            Reporter.WriteData($"{id}{(applied != false ? null : Resources.Pending)}");
            anyUnknown |= !applied.HasValue;
            any = true;
        }

        if (!any)
        {
            Reporter.WriteInformation(Resources.NoMigrations);
        }
        else if (anyUnknown)
        {
            Reporter.WriteWarning(Resources.PendingUnknown);
        }
    }
}
