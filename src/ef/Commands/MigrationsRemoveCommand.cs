// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    partial class MigrationsRemoveCommand
    {
        protected override int Execute()
        {
            var result = CreateExecutor().RemoveMigration(Context.Value(), _force.HasValue());
            if (_json.HasValue())
            {
                ReportJsonResults(result);
            }

            return base.Execute();
        }

        private void ReportJsonResults(IDictionary result)
        {
            Reporter.WriteData("{");
            Reporter.WriteData("  \"migrationFile\": \"" + Json.Escape(result["MigrationFile"] as string) + "\",");
            Reporter.WriteData("  \"metadataFile\": \"" + Json.Escape(result["MetadataFile"] as string) + "\",");
            Reporter.WriteData("  \"snapshotFile\": \"" + Json.Escape(result["SnapshotFile"] as string) + "\"");
            Reporter.WriteData("}");
        }
    }
}
