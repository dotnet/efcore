// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
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

        private static readonly string _migrationsPrefix = LoggerCategory.Migrations.Name + ".";
        private static EventId MakeMigrationsId(Id id) => new EventId((int)id, _migrationsPrefix + id);

        /// <summary>
        ///     Removing a migration without checking the database.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationForceRemove = MakeMigrationsId(Id.MigrationForceRemove);

        /// <summary>
        ///     Removing migration.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationRemoving = MakeMigrationsId(Id.MigrationRemoving);

        /// <summary>
        ///     A migration file was not found.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationFileNotFound = MakeMigrationsId(Id.MigrationFileNotFound);

        /// <summary>
        ///     A metadata file was not found.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationMetadataFileNotFound = MakeMigrationsId(Id.MigrationMetadataFileNotFound);

        /// <summary>
        ///     A manual migration deletion was detected.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationManuallyDeleted = MakeMigrationsId(Id.MigrationManuallyDeleted);

        /// <summary>
        ///     Removing model snapshot.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId SnapshotRemoving = MakeMigrationsId(Id.SnapshotRemoving);

        /// <summary>
        ///     No model snapshot file named was found.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId SnapshotFileNotFound = MakeMigrationsId(Id.SnapshotFileNotFound);

        /// <summary>
        ///     Writing model snapshot to file.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId SnapshotWriting = MakeMigrationsId(Id.SnapshotWriting);

        /// <summary>
        ///     Reusing namespace of a type.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId NamespaceReusing = MakeMigrationsId(Id.NamespaceReusing);

        /// <summary>
        ///     Reusing directory for a file.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId DirectoryReusing = MakeMigrationsId(Id.DirectoryReusing);

        /// <summary>
        ///     Reverting model snapshot.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId SnapshotReverting = MakeMigrationsId(Id.SnapshotReverting);

        /// <summary>
        ///     Writing migration to file.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationWriting = MakeMigrationsId(Id.MigrationWriting);

        /// <summary>
        ///     Resuing model snapshot name.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId SnapshotNameReusing = MakeMigrationsId(Id.SnapshotNameReusing);

        /// <summary>
        ///     An operation was scaffolded that may result in the loss of data. Please review the migration for accuracy.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId DestructiveOperation = MakeMigrationsId(Id.DestructiveOperation);

        /// <summary>
        ///     The namespace contains migrations for a different context.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId ForeignMigrations = MakeMigrationsId(Id.ForeignMigrations);
    }
}
