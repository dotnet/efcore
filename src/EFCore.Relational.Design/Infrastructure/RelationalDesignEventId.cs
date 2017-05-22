// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Event IDs for relational design events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class RelationalDesignEventId
    {
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Scaffolding warning events
            MissingSchemaWarning = CoreEventId.RelationalDesignBaseId,
            MissingTableWarning,
            SequenceNotNamedWarning,
            SequenceTypeNotSupportedWarning,
            UnableToGenerateEntityTypeWarning,
            ColumnTypeNotMappedWarning,
            MissingPrimaryKeyWarning,
            PrimaryKeyColumnsNotMappedWarning,
            IndexColumnsNotMappedWarning,
            ForeignKeyReferencesMissingTableWarning,
            ForeignKeyReferencesMissingPrincipalTableWarning,
            ForeignKeyReferencesNotMappedTableWarning,
            ForeignKeyColumnsNotMappedWarning,
            ForeignKeyReferencesMissingPrincipalKeyWarning,
            ForeignKeyPrincipalEndContainsNullableColumnsWarning,
            ForeignKeyNotNamedWarning,
            ForeignKeyColumnMissingWarning,
            ForeignKeyColumnNotNamedWarning,
            ForeignKeyPrincipalColumnMissingWarning,
            ColumnNotNamedWarning,
            IndexNotNamedWarning,
            IndexTableMissingWarning,
            IndexColumnNotNamedWarning,

            // Scaffolding events
            TableFound = CoreEventId.RelationalDesignBaseId + 1000,
            TableSkipped,
            ColumnSkipped,
            IndexFound,
            IndexColumnFound,
            IndexColumnSkipped,
            SequenceFound
        }

        private static readonly string _scaffoldingPrefix = DbLoggerCategory.Scaffolding.Name + ".";
        private static EventId MakeScaffoldingId(Id id) => new EventId((int)id, _scaffoldingPrefix + id);

        /// <summary>
        ///     The database is missing a schema.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId MissingSchemaWarning = MakeScaffoldingId(Id.MissingSchemaWarning);

        /// <summary>
        ///     The database is missing a table.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId MissingTableWarning = MakeScaffoldingId(Id.MissingTableWarning);

        /// <summary>
        ///     The database has an unnamed sequence.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId SequenceNotNamedWarning = MakeScaffoldingId(Id.SequenceNotNamedWarning);

        /// <summary>
        ///     The database has a sequence of a type that is not supported.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId SequenceTypeNotSupportedWarning = MakeScaffoldingId(Id.SequenceTypeNotSupportedWarning);

        /// <summary>
        ///     An entity type could not be generated.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId UnableToGenerateEntityTypeWarning = MakeScaffoldingId(Id.UnableToGenerateEntityTypeWarning);

        /// <summary>
        ///     A column type could not be mapped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ColumnTypeNotMappedWarning = MakeScaffoldingId(Id.ColumnTypeNotMappedWarning);

        /// <summary>
        ///     A table is missing a primary key.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId MissingPrimaryKeyWarning = MakeScaffoldingId(Id.MissingPrimaryKeyWarning);

        /// <summary>
        ///     Columns in a primary key were not mapped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId PrimaryKeyColumnsNotMappedWarning = MakeScaffoldingId(Id.PrimaryKeyColumnsNotMappedWarning);

        /// <summary>
        ///     Columns in an index were not mapped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexColumnsNotMappedWarning = MakeScaffoldingId(Id.IndexColumnsNotMappedWarning);

        /// <summary>
        ///     A foreign key references a missing table.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesMissingTableWarning = MakeScaffoldingId(Id.ForeignKeyReferencesMissingTableWarning);

        /// <summary>
        ///     A foreign key references a missing table at the principal end.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesMissingPrincipalTableWarning = MakeScaffoldingId(Id.ForeignKeyReferencesMissingPrincipalTableWarning);

        /// <summary>
        ///     A foreign key references a table that was not mapped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesNotMappedTableWarning = MakeScaffoldingId(Id.ForeignKeyReferencesNotMappedTableWarning);

        /// <summary>
        ///     Columns in a foreign key were not mapped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyColumnsNotMappedWarning = MakeScaffoldingId(Id.ForeignKeyColumnsNotMappedWarning);

        /// <summary>
        ///     A foreign key references missing prinicpal key columns.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesMissingPrincipalKeyWarning = MakeScaffoldingId(Id.ForeignKeyReferencesMissingPrincipalKeyWarning);

        /// <summary>
        ///     A principal key contains nullable columns.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyPrincipalEndContainsNullableColumnsWarning = MakeScaffoldingId(Id.ForeignKeyPrincipalEndContainsNullableColumnsWarning);

        /// <summary>
        ///     A foreign key is not named.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyNotNamedWarning = MakeScaffoldingId(Id.ForeignKeyNotNamedWarning);

        /// <summary>
        ///     A foreign key column was not found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyColumnMissingWarning = MakeScaffoldingId(Id.ForeignKeyColumnMissingWarning);

        /// <summary>
        ///     A column referenced by a foreign key constraint was not found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyPrincipalColumnMissingWarning = MakeScaffoldingId(Id.ForeignKeyPrincipalColumnMissingWarning);

        /// <summary>
        ///     A foreign key column was not named.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyColumnNotNamedWarning = MakeScaffoldingId(Id.ForeignKeyColumnNotNamedWarning);

        /// <summary>
        ///     A column is not named. 
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ColumnNotNamedWarning = MakeScaffoldingId(Id.ColumnNotNamedWarning);

        /// <summary>
        ///     An index is not named. 
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexNotNamedWarning = MakeScaffoldingId(Id.IndexNotNamedWarning);

        /// <summary>
        ///     The table referened by an index was not found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexTableMissingWarning = MakeScaffoldingId(Id.IndexTableMissingWarning);

        /// <summary>
        ///     An index column was not named.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexColumnNotNamedWarning = MakeScaffoldingId(Id.IndexColumnNotNamedWarning);

        /// <summary>
        ///     A table was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId TableFound = MakeScaffoldingId(Id.TableFound);

        /// <summary>
        ///     A table was skipped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId TableSkipped = MakeScaffoldingId(Id.TableSkipped);

        /// <summary>
        ///     A column was skipped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ColumnSkipped = MakeScaffoldingId(Id.ColumnSkipped);

        /// <summary>
        ///     An index was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexFound = MakeScaffoldingId(Id.IndexFound);

        /// <summary>
        ///     An index was skipped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexColumnFound = MakeScaffoldingId(Id.IndexColumnFound);

        /// <summary>
        ///     A column of an index was skipped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexColumnSkipped = MakeScaffoldingId(Id.IndexColumnSkipped);

        /// <summary>
        ///     A sequence was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId SequenceFound = MakeScaffoldingId(Id.SequenceFound);
    }
}
