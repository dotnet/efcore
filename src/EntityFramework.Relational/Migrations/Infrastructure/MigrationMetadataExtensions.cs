// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public static class MigrationMetadataExtensions
    {
        public const string TimestampFormat = "yyyyMMddHHmmssf";

        private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");

        public static string CreateMigrationId([NotNull] string migrationName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            return DateTime.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture) + "_" + migrationName;
        }

        public static bool IsValidMigrationId([NotNull] string migrationId)
        {
            Check.NotEmpty(migrationId, "migrationId");

            return _migrationIdPattern.IsMatch(migrationId)
                   || migrationId == Migrator.InitialDatabase;
        }

        public static string GetMigrationName([NotNull] this IMigrationMetadata metadata)
        {
            Check.NotNull(metadata, "metadata");

            return metadata.MigrationId.Substring(TimestampFormat.Length + 1);
        }
    }
}
