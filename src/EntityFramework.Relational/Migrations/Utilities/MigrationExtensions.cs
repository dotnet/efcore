// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Utilities
{
    public static class MigrationExtensions
    {
        public static IMigrationMetadata GetMetadata([NotNull] this Migration migration)
        {
            Check.NotNull(migration, "migration");

            var metadata = migration as IMigrationMetadata;
            if (metadata == null)
            {
                throw new InvalidCastException(Strings.MissingMigrationMetadata(migration.GetType()));
            }

            return metadata;
        }

        public static string GetMigrationId([NotNull] this Migration migration)
        {
            return migration.GetMetadata().MigrationId;
        }

        public static string GetMigrationName([NotNull] this Migration migration)
        {
            return migration.GetMetadata().GetMigrationName();
        }

        public static string GetProductVersion([NotNull] this Migration migration)
        {
            return migration.GetMetadata().ProductVersion;
        }

        public static IModel GetTargetModel([NotNull] this Migration migration)
        {
            return migration.GetMetadata().TargetModel;
        }

        public static IReadOnlyList<MigrationOperation> GetUpgradeOperations([NotNull] this Migration migration)
        {
            Check.NotNull(migration, "migration");

            var migrationBuilder = new MigrationBuilder();

            migration.Up(migrationBuilder);

            return migrationBuilder.Operations;
        }

        public static IReadOnlyList<MigrationOperation> GetDowngradeOperations([NotNull] this Migration migration)
        {
            Check.NotNull(migration, "migration");

            var migrationBuilder = new MigrationBuilder();

            migration.Down(migrationBuilder);

            return migrationBuilder.Operations;
        }
    }
}
