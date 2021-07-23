// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for SQL Server events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class SqlServerEventId
    {
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Model validation events
            DecimalTypeDefaultWarning = CoreEventId.ProviderBaseId,
            ByteIdentityColumnWarning,
            ConflictingValueGenerationStrategiesWarning,
            DecimalTypeKeyWarning,

            // Transaction events
            SavepointsDisabledBecauseOfMARS,

            // Scaffolding events
            ColumnFound = CoreEventId.ProviderDesignBaseId,
            ColumnNotNamedWarning,
            ColumnSkipped,
            DefaultSchemaFound,
            ForeignKeyColumnFound,
            ForeignKeyColumnMissingWarning,
            ForeignKeyColumnNotNamedWarning,
            ForeignKeyColumnsNotMappedWarning,
            ForeignKeyNotNamedWarning,
            ForeignKeyReferencesMissingPrincipalTableWarning,
            IndexColumnFound,
            IndexColumnNotNamedWarning,
            IndexColumnSkipped,
            IndexColumnsNotMappedWarning,
            IndexNotNamedWarning,
            IndexTableMissingWarning,
            MissingSchemaWarning,
            MissingTableWarning,
            SequenceFound,
            SequenceNotNamedWarning,
            TableFound,
            TableSkipped,
            TypeAliasFound,
            ForeignKeyTableMissingWarning,
            PrimaryKeyFound,
            UniqueConstraintFound,
            IndexFound,
            ForeignKeyFound,
            ForeignKeyPrincipalColumnMissingWarning,
            ReflexiveConstraintIgnored
        }

        private static readonly string _validationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

        private static EventId MakeValidationId(Id id)
            => new((int)id, _validationPrefix + id);

        /// <summary>
        ///     <para>
        ///         Decimal column is part of the key.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DecimalTypeKeyWarning = MakeValidationId(Id.DecimalTypeKeyWarning);

        /// <summary>
        ///     <para>
        ///         No explicit type for a decimal column.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DecimalTypeDefaultWarning = MakeValidationId(Id.DecimalTypeDefaultWarning);

        /// <summary>
        ///     <para>
        ///         A byte property is set up to use a SQL Server identity column.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ByteIdentityColumnWarning = MakeValidationId(Id.ByteIdentityColumnWarning);

        /// <summary>
        ///     <para>
        ///         There are conflicting value generation methods for a property.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConflictingValueGenerationStrategiesEventData" />
        ///         payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConflictingValueGenerationStrategiesWarning =
            MakeValidationId(Id.ConflictingValueGenerationStrategiesWarning);

        private static readonly string _transactionPrefix = DbLoggerCategory.Database.Transaction.Name + ".";

        private static EventId MakeTransactionId(Id id)
            => new((int)id, _transactionPrefix + id);

        /// <summary>
        ///     <para>
        ///         Savepoints have been disabled when saving changes with an external transaction, because Multiple Active Result Sets is
        ///         enabled.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId SavepointsDisabledBecauseOfMARS = MakeTransactionId(Id.SavepointsDisabledBecauseOfMARS);

        private static readonly string _scaffoldingPrefix = DbLoggerCategory.Scaffolding.Name + ".";

        private static EventId MakeScaffoldingId(Id id)
            => new((int)id, _scaffoldingPrefix + id);

        /// <summary>
        ///     A column was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ColumnFound = MakeScaffoldingId(Id.ColumnFound);

        /// <summary>
        ///     A default schema was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId DefaultSchemaFound = MakeScaffoldingId(Id.DefaultSchemaFound);

        /// <summary>
        ///     A type alias was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId TypeAliasFound = MakeScaffoldingId(Id.TypeAliasFound);

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
        ///     A foreign key references a missing table at the principal end.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesMissingPrincipalTableWarning =
            MakeScaffoldingId(Id.ForeignKeyReferencesMissingPrincipalTableWarning);

        /// <summary>
        ///     A table was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId TableFound = MakeScaffoldingId(Id.TableFound);

        /// <summary>
        ///     A sequence was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId SequenceFound = MakeScaffoldingId(Id.SequenceFound);

        /// <summary>
        ///     Primary key was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId PrimaryKeyFound = MakeScaffoldingId(Id.PrimaryKeyFound);

        /// <summary>
        ///     An unique constraint was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId UniqueConstraintFound = MakeScaffoldingId(Id.UniqueConstraintFound);

        /// <summary>
        ///     An index was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexFound = MakeScaffoldingId(Id.IndexFound);

        /// <summary>
        ///     A foreign key was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyFound = MakeScaffoldingId(Id.ForeignKeyFound);

        /// <summary>
        ///     A principal column referenced by a foreign key was not found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyPrincipalColumnMissingWarning =
            MakeScaffoldingId(Id.ForeignKeyPrincipalColumnMissingWarning);

        /// <summary>
        ///     A reflexive foreign key constraint was skipped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ReflexiveConstraintIgnored = MakeScaffoldingId(Id.ReflexiveConstraintIgnored);
    }
}
