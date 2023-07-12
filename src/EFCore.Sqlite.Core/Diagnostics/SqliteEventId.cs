// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Event IDs for SQLite events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
/// </summary>
/// <remarks>
///     <para>
///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///         behavior of warnings.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
///         <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public static class SqliteEventId
{
    // Warning: These values must not change between releases.
    // Only add new values to the end of sections, never in the middle.
    // Try to use <Noun><Verb> naming and be consistent with existing names.
    private enum Id
    {
        // Model validation events
        SchemaConfiguredWarning = CoreEventId.ProviderBaseId,
        SequenceConfiguredWarning,
        CompositeKeyWithValueGeneration,

        // Infrastructure events
        UnexpectedConnectionTypeWarning = CoreEventId.ProviderBaseId + 100,

        // Migrations events
        TableRebuildPendingWarning = CoreEventId.ProviderBaseId + 200,

        // Scaffolding events
        ColumnFound = CoreEventId.ProviderDesignBaseId,
        ForeignKeyFound,
        ForeignKeyPrincipalColumnMissingWarning,
        ForeignKeyReferencesMissingTableWarning,
        IndexFound,
        MissingTableWarning,
        PrimaryKeyFound,
        SchemasNotSupportedWarning,
        TableFound,
        UniqueConstraintFound,
        InferringTypes,
        OutOfRangeWarning,
        FormatWarning
    }

    private static readonly string ValidationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

    private static EventId MakeValidationId(Id id)
        => new((int)id, ValidationPrefix + id);

    /// <summary>
    ///     A schema was configured for an entity type, but SQLite does not support schemas.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EntityTypeSchemaEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SchemaConfiguredWarning = MakeValidationId(Id.SchemaConfiguredWarning);

    /// <summary>
    ///     A sequence was configured for an entity type, but SQLite does not support sequences.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="SequenceEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SequenceConfiguredWarning = MakeValidationId(Id.SequenceConfiguredWarning);

    /// <summary>
    ///     An entity type has composite key which is configured to use generated values. SQLite does not support generated values
    ///     on composite keys.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="KeyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CompositeKeyWithValueGeneration = MakeValidationId(Id.CompositeKeyWithValueGeneration);

    private static readonly string InfraPrefix = DbLoggerCategory.Infrastructure.Name + ".";

    private static EventId MakeInfraId(Id id)
        => new((int)id, InfraPrefix + id);

    /// <summary>
    ///     A connection of an unexpected type is being used.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="UnexpectedConnectionTypeEventData" />
    ///         payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId UnexpectedConnectionTypeWarning = MakeInfraId(Id.UnexpectedConnectionTypeWarning);

    private static readonly string MigrationsPrefix = DbLoggerCategory.Migrations.Name + ".";

    private static EventId MakeMigrationsId(Id id)
        => new((int)id, MigrationsPrefix + id);

    /// <summary>
    ///     An operation may fail due to a pending rebuild of the table.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    /// </remarks>
    public static readonly EventId TableRebuildPendingWarning = MakeMigrationsId(Id.TableRebuildPendingWarning);

    private static readonly string ScaffoldingPrefix = DbLoggerCategory.Scaffolding.Name + ".";

    private static EventId MakeScaffoldingId(Id id)
        => new((int)id, ScaffoldingPrefix + id);

    /// <summary>
    ///     A column was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ColumnFound = MakeScaffoldingId(Id.ColumnFound);

    /// <summary>
    ///     SQLite does not support schemas.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId SchemasNotSupportedWarning = MakeScaffoldingId(Id.SchemasNotSupportedWarning);

    /// <summary>
    ///     A foreign key references a missing table.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ForeignKeyReferencesMissingTableWarning =
        MakeScaffoldingId(Id.ForeignKeyReferencesMissingTableWarning);

    /// <summary>
    ///     A table was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId TableFound = MakeScaffoldingId(Id.TableFound);

    /// <summary>
    ///     The database is missing a table.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId MissingTableWarning = MakeScaffoldingId(Id.MissingTableWarning);

    /// <summary>
    ///     A column referenced by a foreign key constraint was not found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ForeignKeyPrincipalColumnMissingWarning =
        MakeScaffoldingId(Id.ForeignKeyPrincipalColumnMissingWarning);

    /// <summary>
    ///     An index was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId IndexFound = MakeScaffoldingId(Id.IndexFound);

    /// <summary>
    ///     A foreign key was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ForeignKeyFound = MakeScaffoldingId(Id.ForeignKeyFound);

    /// <summary>
    ///     A primary key was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId PrimaryKeyFound = MakeScaffoldingId(Id.PrimaryKeyFound);

    /// <summary>
    ///     A unique constraint was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId UniqueConstraintFound = MakeScaffoldingId(Id.UniqueConstraintFound);

    /// <summary>
    ///     Inferring CLR types.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId InferringTypes = MakeScaffoldingId(Id.InferringTypes);

    /// <summary>
    ///     Values are out of range for the type.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId OutOfRangeWarning = MakeScaffoldingId(Id.OutOfRangeWarning);

    /// <summary>
    ///     Values are in an invalid format for the type.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId FormatWarning = MakeScaffoldingId(Id.FormatWarning);
}
