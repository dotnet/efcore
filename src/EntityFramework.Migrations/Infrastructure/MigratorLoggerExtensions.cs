// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    internal static class MigratorLoggerExtensions
    {
        public static void CreatingHistoryTable([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, "logger");

            logger.WriteCore(TraceType.Information, MigratorLoggerEventIds.CreatingHistoryTable, null, null,
                (_, __) => Strings.MigratorLoggerCreatingHistoryTable);
        }

        public static void DroppingHistoryTable([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, "logger");

            logger.WriteCore(TraceType.Information, MigratorLoggerEventIds.DroppingHistoryTable, null, null,
                (_, __) => Strings.MigratorLoggerDroppingHistoryTable);
        }

        public static void ApplyingMigration([NotNull] this ILogger logger, [NotNull] string migrationId)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(migrationId, "migrationId");

            logger.WriteCore(TraceType.Information, MigratorLoggerEventIds.ApplyingMigration, migrationId, null,
                (o, _) => Strings.FormatMigratorLoggerApplyingMigration(o));
        }

        public static void RevertingMigration([NotNull] this ILogger logger, [NotNull] string migrationId)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(migrationId, "migrationId");

            logger.WriteCore(TraceType.Information, MigratorLoggerEventIds.RevertingMigration, migrationId, null,
                (o, _) => Strings.FormatMigratorLoggerRevertingMigration(o));
        }
    }
}
