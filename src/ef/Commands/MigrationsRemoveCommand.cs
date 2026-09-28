// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class MigrationsRemoveCommand
{
    protected override void Validate()
    {
        base.Validate();

        if (_offline!.HasValue() && _force!.HasValue())
        {
            throw new CommandException(Resources.OfflineForceConflict);
        }
    }

    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var result = executor.RemoveMigration(Context!.Value(), _force!.HasValue(), _offline!.HasValue(), _connection!.Value());

        if (_json!.HasValue())
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
