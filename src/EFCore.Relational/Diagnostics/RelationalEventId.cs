// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Event IDs for relational events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
/// </summary>
/// <remarks>
///     <para>
///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///         behavior of warnings.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
///     </para>
/// </remarks>
public static class RelationalEventId
{
    // Warning: These values must not change between releases.
    // Only add new values to the end of sections, never in the middle.
    // Try to use <Noun><Verb> naming and be consistent with existing names.
    private enum Id
    {
        // Connection events
        ConnectionOpening = CoreEventId.RelationalBaseId,
        ConnectionOpened,
        ConnectionClosing,
        ConnectionClosed,
        ConnectionError,
        ConnectionCreating,
        ConnectionCreated,
        ConnectionDisposing,
        ConnectionDisposed,

        // Command events
        CommandExecuting = CoreEventId.RelationalBaseId + 100,
        CommandExecuted,
        CommandError,
        CommandCreating,
        CommandCreated,
        CommandCanceled,
        CommandInitialized,

        // Transaction events
        TransactionStarted = CoreEventId.RelationalBaseId + 200,
        TransactionUsed,
        TransactionCommitted,
        TransactionRolledBack,
        TransactionDisposed,
        TransactionError,
        AmbientTransactionWarning,
        AmbientTransactionEnlisted,
        ExplicitTransactionEnlisted,
        TransactionStarting,
        TransactionCommitting,
        TransactionRollingBack,
        CreatingTransactionSavepoint,
        CreatedTransactionSavepoint,
        RollingBackToTransactionSavepoint,
        RolledBackToTransactionSavepoint,
        ReleasingTransactionSavepoint,
        ReleasedTransactionSavepoint,

        // DataReader events
        DataReaderDisposing = CoreEventId.RelationalBaseId + 300,
        DataReaderClosing,

        // Migrations events
        MigrateUsingConnection = CoreEventId.RelationalBaseId + 400,
        MigrationReverting,
        MigrationApplying,
        MigrationGeneratingDownScript,
        MigrationGeneratingUpScript,
        MigrationsNotApplied,
        MigrationsNotFound,
        MigrationAttributeMissingWarning,
        ColumnOrderIgnoredWarning,

        // Query events
        QueryClientEvaluationWarning = CoreEventId.RelationalBaseId + 500,
        QueryPossibleUnintendedUseOfEqualsWarning,

        // ReSharper disable twice InconsistentNaming
        Obsolete_QueryPossibleExceptionWithAggregateOperatorWarning,
        Obsolete_ValueConversionSqlLiteralWarning,
        MultipleCollectionIncludeWarning,
        NonQueryOperationFailed,
        ExecuteDeleteFailed,
        ExecuteUpdateFailed,

        // Model validation events
        ModelValidationKeyDefaultValueWarning = CoreEventId.RelationalBaseId + 600,
        BoolWithDefaultWarning,
        AllIndexPropertiesNotToMappedToAnyTable,
        IndexPropertiesBothMappedAndNotMappedToTable,
        IndexPropertiesMappedToNonOverlappingTables,
        ForeignKeyPropertiesMappedToUnrelatedTables,
        OptionalDependentWithoutIdentifyingPropertyWarning,
        DuplicateColumnOrders,
        ForeignKeyTpcPrincipalWarning,
        TpcStoreGeneratedIdentityWarning,
        KeyPropertiesNotMappedToTable,
        StoredProcedureConcurrencyTokenNotMapped,
        TriggerOnNonRootTphEntity,

        // Update events
        BatchReadyForExecution = CoreEventId.RelationalBaseId + 700,
        BatchSmallerThanMinBatchSize,
        BatchExecutorFailedToRollbackToSavepoint,
        BatchExecutorFailedToReleaseSavepoint,
        OptionalDependentWithAllNullPropertiesWarning,
        UnexpectedTrailingResultSetWhenSaving,
    }

    private static readonly string _connectionPrefix = DbLoggerCategory.Database.Connection.Name + ".";

    private static EventId MakeConnectionId(Id id)
        => new((int)id, _connectionPrefix + id);

    /// <summary>
    ///     A database connection is opening.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionOpening = MakeConnectionId(Id.ConnectionOpening);

    /// <summary>
    ///     A database connection has been opened.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionOpened = MakeConnectionId(Id.ConnectionOpened);

    /// <summary>
    ///     A database connection is closing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionClosing = MakeConnectionId(Id.ConnectionClosing);

    /// <summary>
    ///     A database connection has been closed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionClosed = MakeConnectionId(Id.ConnectionClosed);

    /// <summary>
    ///     A database connection is going to be disposed. This event is only triggered when Entity Framework is responsible for
    ///     disposing the connection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionDisposing = MakeConnectionId(Id.ConnectionDisposing);

    /// <summary>
    ///     A database connection has been disposed. This event is only triggered when Entity Framework is responsible for
    ///     disposing the connection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionDisposed = MakeConnectionId(Id.ConnectionDisposed);

    /// <summary>
    ///     A error occurred while opening or using a database connection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionError = MakeConnectionId(Id.ConnectionError);

    /// <summary>
    ///     A <see cref="DbConnection" /> is about to be created by EF.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionCreatingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionCreating = MakeConnectionId(Id.ConnectionCreating);

    /// <summary>
    ///     A <see cref="DbConnection" /> has been created by EF.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionCreatedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConnectionCreated = MakeConnectionId(Id.ConnectionCreated);

    private static readonly string _sqlPrefix = DbLoggerCategory.Database.Command.Name + ".";

    private static EventId MakeCommandId(Id id)
        => new((int)id, _sqlPrefix + id);

    /// <summary>
    ///     A <see cref="DbCommand" /> has been canceled.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandCanceled = MakeCommandId(Id.CommandCanceled);

    /// <summary>
    ///     A <see cref="DbCommand" /> is being created.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandCorrelatedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandCreating = MakeCommandId(Id.CommandCreating);

    /// <summary>
    ///     A <see cref="DbCommand" /> has been created.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandCreated = MakeCommandId(Id.CommandCreated);

    /// <summary>
    ///     A <see cref="DbCommand" /> has been initialized with command text and other parameters.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandInitialized = MakeCommandId(Id.CommandInitialized);

    /// <summary>
    ///     A database command is executing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandExecuting = MakeCommandId(Id.CommandExecuting);

    /// <summary>
    ///     A database command has been executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandExecuted = MakeCommandId(Id.CommandExecuted);

    /// <summary>
    ///     An error occurred while a database command was executing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CommandErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CommandError = MakeCommandId(Id.CommandError);

    private static readonly string _transactionPrefix = DbLoggerCategory.Database.Transaction.Name + ".";

    private static EventId MakeTransactionId(Id id)
        => new((int)id, _transactionPrefix + id);

    /// <summary>
    ///     A database transaction has been started.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionStarted = MakeTransactionId(Id.TransactionStarted);

    /// <summary>
    ///     A database transaction is starting.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionStartingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionStarting = MakeTransactionId(Id.TransactionStarting);

    /// <summary>
    ///     Entity Framework started using an already existing database transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionUsed = MakeTransactionId(Id.TransactionUsed);

    /// <summary>
    ///     A database transaction is being committed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionCommitting = MakeTransactionId(Id.TransactionCommitting);

    /// <summary>
    ///     A database transaction has been committed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionCommitted = MakeTransactionId(Id.TransactionCommitted);

    /// <summary>
    ///     A database transaction is being rolled back.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionRollingBack = MakeTransactionId(Id.TransactionRollingBack);

    /// <summary>
    ///     A database transaction has been rolled back.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionRolledBack = MakeTransactionId(Id.TransactionRolledBack);

    /// <summary>
    ///     A database transaction savepoint is being created.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CreatingTransactionSavepoint = MakeTransactionId(Id.CreatingTransactionSavepoint);

    /// <summary>
    ///     A database transaction savepoint has been created.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CreatedTransactionSavepoint = MakeTransactionId(Id.CreatedTransactionSavepoint);

    /// <summary>
    ///     A database transaction is being rolled back to a savepoint.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RollingBackToTransactionSavepoint = MakeTransactionId(Id.RollingBackToTransactionSavepoint);

    /// <summary>
    ///     A database transaction has been rolled back to a savepoint.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RolledBackToTransactionSavepoint = MakeTransactionId(Id.RolledBackToTransactionSavepoint);

    /// <summary>
    ///     A database transaction savepoint is being released.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ReleasingTransactionSavepoint = MakeTransactionId(Id.ReleasingTransactionSavepoint);

    /// <summary>
    ///     A database transaction savepoint has been released.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ReleasedTransactionSavepoint = MakeTransactionId(Id.ReleasedTransactionSavepoint);

    /// <summary>
    ///     A database transaction has been disposed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionDisposed = MakeTransactionId(Id.TransactionDisposed);

    /// <summary>
    ///     An error has occurred while using. committing, or rolling back a database transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionError = MakeTransactionId(Id.TransactionError);

    /// <summary>
    ///     An application may have expected an ambient transaction to be used when it was actually ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId AmbientTransactionWarning = MakeTransactionId(Id.AmbientTransactionWarning);

    /// <summary>
    ///     Entity Framework enlisted the connection in an ambient transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEnlistedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId AmbientTransactionEnlisted = MakeTransactionId(Id.AmbientTransactionEnlisted);

    /// <summary>
    ///     The connection was explicitly enlisted in a transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TransactionEnlistedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExplicitTransactionEnlisted = MakeTransactionId(Id.ExplicitTransactionEnlisted);

    /// <summary>
    ///     A database data reader has been disposed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DataReaderDisposingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DataReaderDisposing = MakeCommandId(Id.DataReaderDisposing);

    /// <summary>
    ///     A database data reader is about to be closed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DataReaderClosingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DataReaderClosing = MakeCommandId(Id.DataReaderClosing);

    private static readonly string _migrationsPrefix = DbLoggerCategory.Migrations.Name + ".";

    private static EventId MakeMigrationsId(Id id)
        => new((int)id, _migrationsPrefix + id);

    /// <summary>
    ///     Migrations is using a database connection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigratorConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrateUsingConnection = MakeMigrationsId(Id.MigrateUsingConnection);

    /// <summary>
    ///     A migration is being reverted.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationReverting = MakeMigrationsId(Id.MigrationReverting);

    /// <summary>
    ///     A migration is being applied.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationApplying = MakeMigrationsId(Id.MigrationApplying);

    /// <summary>
    ///     Migrations is generating a "down" script.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationScriptingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationGeneratingDownScript = MakeMigrationsId(Id.MigrationGeneratingDownScript);

    /// <summary>
    ///     Migrations is generating an "up" script.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationScriptingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationGeneratingUpScript = MakeMigrationsId(Id.MigrationGeneratingUpScript);

    /// <summary>
    ///     Migrations weren't applied.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigratorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationsNotApplied = MakeMigrationsId(Id.MigrationsNotApplied);

    /// <summary>
    ///     Migrations weren't found.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationAssemblyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationsNotFound = MakeMigrationsId(Id.MigrationsNotFound);

    /// <summary>
    ///     A MigrationAttribute isn't specified on the class.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationTypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MigrationAttributeMissingWarning = MakeMigrationsId(Id.MigrationAttributeMissingWarning);

    /// <summary>
    ///     Column order was ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MigrationColumnOperationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ColumnOrderIgnoredWarning = MakeMigrationsId(Id.ColumnOrderIgnoredWarning);

    private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";

    private static EventId MakeQueryId(Id id)
        => new((int)id, _queryPrefix + id);

    /// <summary>
    ///     A query is using equals comparisons in a possibly unintended way.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoSqlExpressionsEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId QueryPossibleUnintendedUseOfEqualsWarning =
        MakeQueryId(Id.QueryPossibleUnintendedUseOfEqualsWarning);

    /// <summary>
    ///     A query is loading multiple related collections without configuring a <see cref="QuerySplittingBehavior" />.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
    /// </remarks>
    public static readonly EventId MultipleCollectionIncludeWarning = MakeQueryId(Id.MultipleCollectionIncludeWarning);

    /// <summary>
    ///     An error occurred while executing a non-query operation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextTypeErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId NonQueryOperationFailed = MakeQueryId(Id.NonQueryOperationFailed);

    /// <summary>
    ///     An error occurred while executing an 'ExecuteDelete' operation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextTypeErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecuteDeleteFailed = MakeQueryId(Id.ExecuteDeleteFailed);

    /// <summary>
    ///     An error occurred while executing an 'ExecuteUpdate' operation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextTypeErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecuteUpdateFailed = MakeQueryId(Id.ExecuteUpdateFailed);

    private static readonly string _validationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

    private static EventId MakeValidationId(Id id)
        => new((int)id, _validationPrefix + id);

    /// <summary>
    ///     A single database default column value has been set on a key column.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ModelValidationKeyDefaultValueWarning = MakeValidationId(Id.ModelValidationKeyDefaultValueWarning);

    /// <summary>
    ///     A bool property is configured with a store-generated default.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId BoolWithDefaultWarning = MakeValidationId(Id.BoolWithDefaultWarning);

    /// <summary>
    ///     An index specifies properties all of which are not mapped to a column in any table.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="IndexEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId AllIndexPropertiesNotToMappedToAnyTable =
        MakeValidationId(Id.AllIndexPropertiesNotToMappedToAnyTable);

    /// <summary>
    ///     An index specifies properties some of which are mapped and some of which are not mapped to a column in a table.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="IndexWithPropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId IndexPropertiesBothMappedAndNotMappedToTable =
        MakeValidationId(Id.IndexPropertiesBothMappedAndNotMappedToTable);

    /// <summary>
    ///     An index specifies properties which map to columns on non-overlapping tables.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="IndexWithPropertiesEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId IndexPropertiesMappedToNonOverlappingTables =
        MakeValidationId(Id.IndexPropertiesMappedToNonOverlappingTables);

    /// <summary>
    ///     A key specifies properties which don't map to a single table.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="KeyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId KeyPropertiesNotMappedToTable =
        MakeValidationId(Id.KeyPropertiesNotMappedToTable);

    /// <summary>
    ///     An entity type is mapped to the stored procedure with a concurrency token not mapped to any original value parameter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="StoredProcedurePropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId StoredProcedureConcurrencyTokenNotMapped =
        MakeValidationId(Id.StoredProcedureConcurrencyTokenNotMapped);

    /// <summary>
    ///     Can't configure a trigger on the non-root entity type in a TPH hierarchy.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EntityTypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TriggerOnNonRootTphEntity =
        MakeValidationId(Id.TriggerOnNonRootTphEntity);

    /// <summary>
    ///     A foreign key specifies properties which don't map to the related tables.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ForeignKeyPropertiesMappedToUnrelatedTables =
        MakeValidationId(Id.ForeignKeyPropertiesMappedToUnrelatedTables);

    /// <summary>
    ///     A foreign key specifies properties which don't map to the related tables.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ForeignKeyTpcPrincipalWarning =
        MakeValidationId(Id.ForeignKeyTpcPrincipalWarning);

    /// <summary>
    ///     The PK is using store-generated values in TPC.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TpcStoreGeneratedIdentityWarning =
        MakeValidationId(Id.TpcStoreGeneratedIdentityWarning);

    /// <summary>
    ///     The entity does not have any property with a non-default value to identify whether the entity exists.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EntityTypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId OptionalDependentWithoutIdentifyingPropertyWarning
        = MakeValidationId(Id.OptionalDependentWithoutIdentifyingPropertyWarning);

    /// <summary>
    ///     The configured column orders for a table contains duplicates.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ColumnsEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DuplicateColumnOrders = MakeValidationId(Id.DuplicateColumnOrders);

    private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";

    private static EventId MakeUpdateId(Id id)
        => new((int)id, _updatePrefix + id);

    /// <summary>
    ///     Update commands were batched and are now ready for execution
    ///     <see cref="RelationalDbContextOptionsBuilder{TBuilder,TExtension}.MinBatchSize" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="BatchEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId BatchReadyForExecution = MakeUpdateId(Id.BatchReadyForExecution);

    /// <summary>
    ///     Update commands were not batched because there were fewer than
    ///     <see cref="RelationalDbContextOptionsBuilder{TBuilder,TExtension}.MinBatchSize" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="MinBatchSizeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId BatchSmallerThanMinBatchSize = MakeUpdateId(Id.BatchSmallerThanMinBatchSize);

    /// <summary>
    ///     An error occurred while the batch executor was rolling back the transaction to a savepoint, after an exception occurred.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Update" /> category.
    /// </remarks>
    public static readonly EventId BatchExecutorFailedToRollbackToSavepoint = MakeUpdateId(Id.BatchExecutorFailedToRollbackToSavepoint);

    /// <summary>
    ///     An error occurred while the batch executor was releasing a transaction savepoint.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Update" /> category.
    /// </remarks>
    public static readonly EventId BatchExecutorFailedToReleaseSavepoint = MakeUpdateId(Id.BatchExecutorFailedToReleaseSavepoint);

    /// <summary>
    ///     The entity does not have any property with a non-default value to identify whether the entity exists.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="UpdateEntryEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId OptionalDependentWithAllNullPropertiesWarning
        = MakeUpdateId(Id.OptionalDependentWithAllNullPropertiesWarning);

    /// <summary>
    ///     An unexpected trailing result set was found when reading the results of a SaveChanges operation; this may indicate that a stored
    ///     procedure returned a result set without being configured for it in the EF model. Check your stored procedure definitions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    /// </remarks>
    public static readonly EventId UnexpectedTrailingResultSetWhenSaving =
        MakeUpdateId(Id.UnexpectedTrailingResultSetWhenSaving);
}
