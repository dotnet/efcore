// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class DbContextListCommand
    {
        protected override int Execute()
        {
            var types = CreateExecutor().GetContextTypes().ToList();

            if (_json.HasValue())
            {
                ReportJsonResults(types);
            }
            else
            {
                ReportResults(types);
            }

            return base.Execute();
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
                Reporter.WriteData(contextType["FullName"] as string);
                any = true;
            }

            if (!any)
            {
                Reporter.WriteInformation(Resources.NoDbContext);
            }
        }
    }
}
