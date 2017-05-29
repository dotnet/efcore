// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for design events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class DesignEventId
    {
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Migrations events
            MigrationForceRemove = CoreEventId.CoreDesignBaseId,
            MigrationRemoving,
            MigrationFileNotFound,
            MigrationMetadataFileNotFound,
            MigrationManuallyDeleted,
            SnapshotRemoving,
            SnapshotFileNotFound,
            SnapshotWriting,
            NamespaceReusing,
            DirectoryReusing,
            SnapshotReverting,
            MigrationWriting,
            SnapshotNameReusing,
            DestructiveOperation,
            ForeignMigrations
        }

        private static readonly string _migrationsPrefix = DbLoggerCategory.Migrations.Name + ".";
        private static EventId MakeMigrationsId(Id id) => new EventId((int)id, _migrationsPrefix + id);

        /// <summary>
        ///     <para>
        ///         Removing a migration without checking the database.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationDesignEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationForceRemove = MakeMigrationsId(Id.MigrationForceRemove);

        /// <summary>
        ///     <para>
        ///         Removing migration.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationDesignEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationRemoving = MakeMigrationsId(Id.MigrationRemoving);

        /// <summary>
        ///     <para>
        ///         A migration file was not found.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationFileNameEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationFileNotFound = MakeMigrationsId(Id.MigrationFileNotFound);

        /// <summary>
        ///     <para>
        ///         A metadata file was not found.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationFileNameEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationMetadataFileNotFound = MakeMigrationsId(Id.MigrationMetadataFileNotFound);

        /// <summary>
        ///     <para>
        ///         A manual migration deletion was detected.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationDesignEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationManuallyDeleted = MakeMigrationsId(Id.MigrationManuallyDeleted);

        /// <summary>
        ///     <para>
        ///         Removing model snapshot.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ModelSnapshotFileNameEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SnapshotRemoving = MakeMigrationsId(Id.SnapshotRemoving);

        /// <summary>
        ///     <para>
        ///         No model snapshot file named was found.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ModelSnapshotFileNameEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SnapshotFileNotFound = MakeMigrationsId(Id.SnapshotFileNotFound);

        /// <summary>
        ///     <para>
        ///         Writing model snapshot to file.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ScaffoldedMigrationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SnapshotWriting = MakeMigrationsId(Id.SnapshotWriting);

        /// <summary>
        ///     <para>
        ///         Reusing namespace of a type.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ResourceReusedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NamespaceReusing = MakeMigrationsId(Id.NamespaceReusing);

        /// <summary>
        ///     <para>
        ///         Reusing directory for a file.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ResourceReusedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DirectoryReusing = MakeMigrationsId(Id.DirectoryReusing);

        /// <summary>
        ///     <para>
        ///         Reverting model snapshot.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ModelSnapshotFileNameEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SnapshotReverting = MakeMigrationsId(Id.SnapshotReverting);

        /// <summary>
        ///     <para>
        ///         Writing migration to file.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ScaffoldedMigrationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationWriting = MakeMigrationsId(Id.MigrationWriting);

        /// <summary>
        ///     <para>
        ///         Resuing model snapshot name.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="SnapshotNameEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SnapshotNameReusing = MakeMigrationsId(Id.SnapshotNameReusing);

        /// <summary>
        ///     <para>
        ///         An operation was scaffolded that may result in the loss of data. Please review the migration for accuracy.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationOperationsEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DestructiveOperation = MakeMigrationsId(Id.DestructiveOperation);

        /// <summary>
        ///     <para>
        ///         The namespace contains migrations for a different context.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NamespaceEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ForeignMigrations = MakeMigrationsId(Id.ForeignMigrations);
    }
}
