// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class MigrationsAddCommand
    {
        protected override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(_name.Value))
            {
                throw new CommandException(Resources.MissingArgument(_name.Name));
            }
        }

        protected override int Execute(string[] args)
        {
            var files = CreateExecutor(args)
                .AddMigration(_name.Value, _outputDir.Value(), Context.Value(), _namespace.Value());

            if (_json.HasValue())
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
}
