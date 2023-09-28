// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DbContextListCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var types = executor.GetContextTypes().ToList();

        if (_json!.HasValue())
        {
            ReportJsonResults(types);
        }
        else
        {
            ReportResults(types);
        }

        return base.Execute(args);
    }

    private static void ReportJsonResults(IReadOnlyList<IDictionary> contextTypes)
    {
        var nameGroups = contextTypes.GroupBy(t => t["Name"]).ToList();
        var fullNameGroups = contextTypes.GroupBy(t => t["FullName"]).ToList();

        Reporter.WriteData("[");

        for (var i = 0; i < contextTypes.Count; i++)
        {
            var safeName = nameGroups.Count(g => g.Key == contextTypes[i]["Name"]) == 1
                ? contextTypes[i]["Name"]
                : fullNameGroups.Count(g => g.Key == contextTypes[i]["FullName"]) == 1
                    ? contextTypes[i]["FullName"]
                    : contextTypes[i]["AssemblyQualifiedName"];

            Reporter.WriteData("  {");
            Reporter.WriteData("     \"fullName\": \"" + contextTypes[i]["FullName"] + "\",");
            Reporter.WriteData("     \"safeName\": \"" + safeName + "\",");
            Reporter.WriteData("     \"name\": \"" + contextTypes[i]["Name"] + "\",");
            Reporter.WriteData("     \"assemblyQualifiedName\": \"" + contextTypes[i]["AssemblyQualifiedName"] + "\"");

            var line = "  }";
            if (i != contextTypes.Count - 1)
            {
                line += ",";
            }

            Reporter.WriteData(line);
        }

        Reporter.WriteData("]");
    }

    private static void ReportResults(IEnumerable<IDictionary> contextTypes)
    {
        var any = false;
        foreach (var contextType in contextTypes)
        {
            Reporter.WriteData((contextType["FullName"] as string)!);
            any = true;
        }

        if (!any)
        {
            Reporter.WriteInformation(Resources.NoDbContext);
        }
    }
}
