// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new NamespaceEventData(
                        definition,
                        ForeignMigrations,
                        migrationNamespace));
            }
        }

        private static string ForeignMigrations(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (NamespaceEventData)payload;
            return d.GenerateMessage(p.Namespace);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotNameReusing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new SnapshotNameEventData(
                        definition,
                        SnapshotNameReusing,
                        lastModelSnapshotName));
            }
        }

        private static string SnapshotNameReusing(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (SnapshotNameEventData)payload;
            return d.GenerateMessage(p.SnapshotName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DestructiveOperation(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IEnumerable<MigrationOperation> operations)
        {
            var definition = DesignStrings.LogDestructiveOperation;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationOperationsEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        operations));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationForceRemove(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new MigrationDesignEventData(
                        definition,
                        MigrationForceRemove,
                        migration));
            }
        }

        private static string MigrationForceRemove(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationDesignEventData)payload;
            return d.GenerateMessage(p.Migration.GetId());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationRemoving(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new MigrationDesignEventData(
                        definition,
                        MigrationRemoving,
                        migration));
            }
        }

        private static string MigrationRemoving(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationDesignEventData)payload;
            return d.GenerateMessage(p.Migration.GetId());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationFileNotFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new MigrationFileNameEventData(
                        definition,
                        MigrationFileNotFound,
                        migration,
                        fileName));
            }
        }

        private static string MigrationFileNotFound(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (MigrationFileNameEventData)payload;
            return d.GenerateMessage(p.FileName, p.Migration.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationMetadataFileNotFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogNoMigrationMetadataFile;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationFileNameEventData(
                        definition,
                        MigrationMetadataFileNotFound,
                        migration,
                        fileName));
            }
        }

        private static string MigrationMetadataFileNotFound(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationFileNameEventData)payload;
            return d.GenerateMessage(p.FileName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationManuallyDeleted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] Migration migration)
        {
            var definition = DesignStrings.LogManuallyDeleted;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationDesignEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        migration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotRemoving(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogRemovingSnapshot;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ModelSnapshotFileNameEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        modelSnapshot,
                        fileName));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotFileNotFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new ModelSnapshotFileNameEventData(
                        definition,
                        SnapshotFileNotFound,
                        modelSnapshot,
                        fileName));
            }
        }

        private static string SnapshotFileNotFound(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ModelSnapshotFileNameEventData)payload;
            return d.GenerateMessage(p.FileName, p.Snapshot.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotReverting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] ModelSnapshot modelSnapshot,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogRevertingSnapshot;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ModelSnapshotFileNameEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        modelSnapshot,
                        fileName));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationWriting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogWritingMigration;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ScaffoldedMigrationEventData(
                        definition,
                        MigrationWriting,
                        scaffoldedMigration,
                        fileName));
            }
        }

        private static string MigrationWriting(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (ScaffoldedMigrationEventData)payload;
            return d.GenerateMessage(p.FileName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SnapshotWriting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogWritingSnapshot;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ScaffoldedMigrationEventData(
                        definition,
                        SnapshotWriting,
                        scaffoldedMigration,
                        fileName));
            }
        }

        private static string SnapshotWriting(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (ScaffoldedMigrationEventData)payload;
            return d.GenerateMessage(p.FileName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NamespaceReusing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
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
                    new ResourceReusedEventData(
                        definition,
                        NamespaceReusing,
                        type.ShortDisplayName()));
            }
        }

        private static string NamespaceReusing(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (ResourceReusedEventData)payload;
            return d.GenerateMessage(p.ResourceName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DirectoryReusing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] string fileName)
        {
            var definition = DesignStrings.LogReusingDirectory;

            definition.Log(diagnostics, fileName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ResourceReusedEventData(
                        definition,
                        DirectoryReusing,
                        fileName));
            }
        }

        private static string DirectoryReusing(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (ResourceReusedEventData)payload;
            return d.GenerateMessage(p.ResourceName);
        }
    }
}
