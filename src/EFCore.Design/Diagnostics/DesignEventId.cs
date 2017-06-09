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
            // Scaffolding warning events
            SequenceTypeNotSupportedWarning = CoreEventId.CoreDesignBaseId,
            UnableToGenerateEntityTypeWarning,
            ColumnTypeNotMappedWarning,
            MissingPrimaryKeyWarning,
            PrimaryKeyColumnsNotMappedWarning,
            ForeignKeyReferencesNotMappedTableWarning,
            ForeignKeyReferencesMissingPrincipalKeyWarning,
            ForeignKeyPrincipalEndContainsNullableColumnsWarning,
            NonNullableBoooleanColumnHasDefaultConstraintWarning
        }

        private static readonly string _scaffoldingPrefix = DbLoggerCategory.Scaffolding.Name + ".";
        private static EventId MakeScaffoldingId(Id id) => new EventId((int)id, _scaffoldingPrefix + id);

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
        ///     A foreign key references a table that was not mapped.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesNotMappedTableWarning = MakeScaffoldingId(Id.ForeignKeyReferencesNotMappedTableWarning);

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
        ///     A column would be mapped to a bool type, is non-nullable and has a default constraint.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId NonNullableBoooleanColumnHasDefaultConstraintWarning = MakeScaffoldingId(Id.NonNullableBoooleanColumnHasDefaultConstraintWarning);
    }
}
