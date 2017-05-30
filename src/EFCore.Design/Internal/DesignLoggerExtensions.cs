// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] string migrationNamespace)
        {
            var definition = DesignStrings.LogForeignMigrations;

            definition.Log(
                diagnostics,
                migrationNamespace);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] string lastModelSnapshotName)
        {
            var definition = DesignStrings.LogReusingSnapshotName;

            definition.Log(
                diagnostics,
                lastModelSnapshotName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] IEnumerable<MigrationOperation> operations)
        {
            var definition = DesignStrings.LogDestructiveOperation;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var definition = DesignStrings.LogForceRemoveMigration;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migration.GetId());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var definition = DesignStrings.LogRemovingMigration;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migration.GetId());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] Migration migration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogNoMigrationFile;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    fileName,
                    migration.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] Migration migration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogNoMigrationMetadataFile;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var definition = DesignStrings.LogManuallyDeleted;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
        public static void SnapshotRemoving(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogRemovingSnapshot;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ModelSnapshot = modelSnapshot,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotFileNotFound(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogNoSnapshotFile;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    fileName,
                    modelSnapshot.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ModelSnapshot = modelSnapshot,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotReverting(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogRevertingSnapshot;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ModelSnapshot = modelSnapshot,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationWriting(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogWritingMigration;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ScaffoldedMigration = scaffoldedMigration,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotWriting(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogWritingSnapshot;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ScaffoldedMigration = scaffoldedMigration,
                        FileName = fileName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NamespaceReusing(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] Type type)
        {
            var definition = DesignStrings.LogReusingNamespace;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, type.ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
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
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Migrations> diagnostics,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogReusingDirectory;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        FileName = fileName
                    });
            }
        }
    }
}
