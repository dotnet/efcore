// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DatabaseUpdateCommand
{
    protected override void Validate()
    {
        base.Validate();

        if (_add!.HasValue())
        {
            if (string.IsNullOrEmpty(_migration!.Value))
            {
                throw new CommandException(Resources.MissingArgument(_migration.Name));
            }
        }
        else
        {
            if (_outputDir!.HasValue())
            {
                throw new CommandException(Resources.OutputDirRequiresAdd);
            }

            if (_namespace!.HasValue())
            {
                throw new CommandException(Resources.NamespaceRequiresAdd);
            }
        }
    }

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
        Reporter.WriteData("{");
        Reporter.WriteData("  \"migrationFile\": " + Json.Literal(files["MigrationFile"] as string) + ",");
        Reporter.WriteData("  \"metadataFile\": " + Json.Literal(files["MetadataFile"] as string) + ",");
        Reporter.WriteData("  \"snapshotFile\": " + Json.Literal(files["SnapshotFile"] as string));
        Reporter.WriteData("}");
    }
}
