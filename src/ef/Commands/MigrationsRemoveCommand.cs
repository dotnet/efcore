// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class MigrationsRemoveCommand
    {
        protected override int Execute(string[] args)
        {
            var result = CreateExecutor(args)
                .RemoveMigration(Context.Value(), _force.HasValue());
            if (_json.HasValue())
            {
                ReportJsonResults(result);
            }

            return base.Execute(args);
        }

        private static void ReportJsonResults(IDictionary result)
        {
            Reporter.WriteData("{");
            Reporter.WriteData("  \"migrationFile\": " + Json.Literal(result["MigrationFile"] as string) + ",");
            Reporter.WriteData("  \"metadataFile\": " + Json.Literal(result["MetadataFile"] as string) + ",");
            Reporter.WriteData("  \"snapshotFile\": " + Json.Literal(result["SnapshotFile"] as string));
            Reporter.WriteData("}");
        }
    }
}
