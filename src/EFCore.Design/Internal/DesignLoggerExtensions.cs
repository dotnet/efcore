// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DesignLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignMigrations(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] string migrationNamespace)
        {
            var eventId = DesignEventId.ForeignMigrations;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    DesignStrings.ForeignMigrations(migrationNamespace));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        MigrationNamespace = migrationNamespace
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotNameReusing(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] string lastModelSnapshotName)
        {
            var eventId = DesignEventId.SnapshotNameReusing;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.ReusingSnapshotName(lastModelSnapshotName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        LastModelSnapshotName = lastModelSnapshotName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DestructiveOperation(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IEnumerable<MigrationOperation> operations)
        {
            var eventId = DesignEventId.DestructiveOperation;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    DesignStrings.DestructiveOperation);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Operations = (ICollection<MigrationOperation>)operations.ToList()
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationForceRemove(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var eventId = DesignEventId.MigrationForceRemove;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    DesignStrings.ForceRemoveMigration(migration.GetId()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Migration = migration
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationRemoving(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var eventId = DesignEventId.MigrationRemoving;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                diagnostics.Logger.LogInformation(
                    eventId,
                    DesignStrings.RemovingMigration(migration.GetId()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Migration = migration
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationFileNotFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.MigrationFileNotFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    DesignStrings.NoMigrationFile(fileName, migration.GetType().ShortDisplayName()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Migration = migration,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationMetadataFileNotFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.MigrationMetadataFileNotFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.NoMigrationMetadataFile(fileName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Migration = migration,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationManuallyDeleted(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var eventId = DesignEventId.MigrationManuallyDeleted;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.ManuallyDeleted);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Migration = migration,
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotRemoving(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.SnapshotRemoving;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                diagnostics.Logger.LogInformation(
                    eventId,
                    DesignStrings.RemovingSnapshot);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ModelSnapshot = modelSnapshot,
                        FileName = fileName,
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotFileNotFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.SnapshotFileNotFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    DesignStrings.NoSnapshotFile(fileName, modelSnapshot.GetType().ShortDisplayName()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ModelSnapshot = modelSnapshot,
                        FileName = fileName,
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotReverting(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.SnapshotReverting;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                diagnostics.Logger.LogInformation(
                    eventId,
                    DesignStrings.RevertingSnapshot);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ModelSnapshot = modelSnapshot,
                        FileName = fileName,
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationWriting(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.MigrationWriting;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.WritingMigration(fileName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ScaffoldedMigration = scaffoldedMigration,
                        FileName = fileName,
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotWriting(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.SnapshotWriting;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.WritingSnapshot(fileName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ScaffoldedMigration = scaffoldedMigration,
                        FileName = fileName,
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NamespaceReusing(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] Type type)
        {
            var eventId = DesignEventId.NamespaceReusing;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.ReusingNamespace(type.ShortDisplayName()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Type = type
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DirectoryReusing(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] string fileName)
        {
            var eventId = DesignEventId.DirectoryReusing;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    DesignStrings.ReusingDirectory(fileName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        FileName = fileName
                    });
            }
        }
    }
}
