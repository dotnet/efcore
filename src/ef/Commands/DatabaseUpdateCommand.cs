// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DatabaseUpdateCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);

        if (_add!.HasValue())
        {
            // Create and apply a new migration in one step
            var files = executor.AddAndApplyMigration(
                _migration!.Value!,
                _outputDir!.Value(),
                Context!.Value(),
                _namespace!.Value(),
                _connection!.Value());

            if (_json!.HasValue())
            {
                ReportJson(files);
            }
        }
        else
        {
            executor.UpdateDatabase(_migration!.Value, _connection!.Value(), Context!.Value());
        }

        return base.Execute(args);
    }

    private static void ReportJson(IDictionary files)
    {
        if (files.Count == 0)
        {
            return;
        }

        var migrationFile = files["MigrationFile"] as string;
        var metadataFile = files["MetadataFile"] as string;
        var snapshotFile = files["SnapshotFile"] as string;

        if (migrationFile == null && metadataFile == null && snapshotFile == null)
        {
            return;
        }

        Reporter.WriteData("{");
        Reporter.WriteData("  \"migrationFile\": " + Json.Literal(migrationFile) + ",");
        Reporter.WriteData("  \"metadataFile\": " + Json.Literal(metadataFile) + ",");
        Reporter.WriteData("  \"snapshotFile\": " + Json.Literal(snapshotFile));
        Reporter.WriteData("}");
    }
}
