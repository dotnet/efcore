// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Event IDs for SQL Server events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
/// </summary>
/// <remarks>
///     <para>
///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///         behavior of warnings.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///         for more information and examples.
///     </para>
/// </remarks>
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
        ReflexiveConstraintIgnored,
        DuplicateForeignKeyConstraintIgnored,
        ColumnWithoutTypeWarning,
        ForeignKeyReferencesUnknownPrincipalTableWarning,
        MissingViewDefinitionRightsWarning,
    }

    private static readonly string ValidationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

    private static EventId MakeValidationId(Id id)
        => new((int)id, ValidationPrefix + id);

    /// <summary>
    ///     Decimal column is part of the key.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DecimalTypeKeyWarning = MakeValidationId(Id.DecimalTypeKeyWarning);

    /// <summary>
    ///     No explicit type for a decimal column.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DecimalTypeDefaultWarning = MakeValidationId(Id.DecimalTypeDefaultWarning);

    /// <summary>
    ///     A byte property is set up to use a SQL Server identity column.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ByteIdentityColumnWarning = MakeValidationId(Id.ByteIdentityColumnWarning);

    /// <summary>
    ///     There are conflicting value generation methods for a property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConflictingValueGenerationStrategiesEventData" />
    ///         payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConflictingValueGenerationStrategiesWarning =
        MakeValidationId(Id.ConflictingValueGenerationStrategiesWarning);

    private static readonly string TransactionPrefix = DbLoggerCategory.Database.Transaction.Name + ".";

    private static EventId MakeTransactionId(Id id)
        => new((int)id, TransactionPrefix + id);

    /// <summary>
    ///     Savepoints have been disabled when saving changes with an external transaction, because Multiple Active Result Sets is
    ///     enabled.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    /// </remarks>
    public static readonly EventId SavepointsDisabledBecauseOfMARS = MakeTransactionId(Id.SavepointsDisabledBecauseOfMARS);

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
    ///     A default schema was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId DefaultSchemaFound = MakeScaffoldingId(Id.DefaultSchemaFound);

    /// <summary>
    ///     A type alias was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId TypeAliasFound = MakeScaffoldingId(Id.TypeAliasFound);

    /// <summary>
    ///     The database is missing a schema.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId MissingSchemaWarning = MakeScaffoldingId(Id.MissingSchemaWarning);

    /// <summary>
    ///     The database is missing a table.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId MissingTableWarning = MakeScaffoldingId(Id.MissingTableWarning);

    /// <summary>
    ///     A foreign key references a missing table at the principal end.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ForeignKeyReferencesMissingPrincipalTableWarning =
        MakeScaffoldingId(Id.ForeignKeyReferencesMissingPrincipalTableWarning);

    /// <summary>
    ///     A foreign key references a unknown table at the principal end.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ForeignKeyReferencesUnknownPrincipalTableWarning =
        MakeScaffoldingId(Id.ForeignKeyReferencesUnknownPrincipalTableWarning);

    /// <summary>
    ///     A table was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId TableFound = MakeScaffoldingId(Id.TableFound);

    /// <summary>
    ///     A sequence was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId SequenceFound = MakeScaffoldingId(Id.SequenceFound);

    /// <summary>
    ///     Primary key was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId PrimaryKeyFound = MakeScaffoldingId(Id.PrimaryKeyFound);

    /// <summary>
    ///     An unique constraint was found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId UniqueConstraintFound = MakeScaffoldingId(Id.UniqueConstraintFound);

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
    ///     A principal column referenced by a foreign key was not found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ForeignKeyPrincipalColumnMissingWarning =
        MakeScaffoldingId(Id.ForeignKeyPrincipalColumnMissingWarning);

    /// <summary>
    ///     A reflexive foreign key constraint was skipped.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ReflexiveConstraintIgnored = MakeScaffoldingId(Id.ReflexiveConstraintIgnored);

    /// <summary>
    ///     A duplicate foreign key constraint was skipped.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId DuplicateForeignKeyConstraintIgnored = MakeScaffoldingId(Id.DuplicateForeignKeyConstraintIgnored);

    /// <summary>
    ///     A column was skipped because its database type could not be found.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId ColumnWithoutTypeWarning = MakeScaffoldingId(Id.ColumnWithoutTypeWarning);

    /// <summary>
    ///     The database user has not been granted 'VIEW DEFINITION' rights. Scaffolding requires these rights to construct the Entity Framework
    ///     model correctly. Without these rights, parts of the scaffolded model may be missing, resulting in incorrect interactions between Entity
    ///     Framework and the database at runtime.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    /// </remarks>
    public static readonly EventId MissingViewDefinitionRightsWarning = MakeScaffoldingId(Id.MissingViewDefinitionRightsWarning);
}
