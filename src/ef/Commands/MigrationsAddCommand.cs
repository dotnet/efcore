// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class MigrationsAddCommand
{
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(_name!.Value))
        {
            throw new CommandException(Resources.MissingArgument(_name.Name));
        }
    }

    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var files = executor.AddMigration(_name!.Value!, _outputDir!.Value(), Context!.Value(), _namespace!.Value());

        if (_json!.HasValue())
        {
            ReportJson(files);
        }
        else
        {
            Reporter.WriteInformation(Resources.MigrationsAddCompleted);
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
