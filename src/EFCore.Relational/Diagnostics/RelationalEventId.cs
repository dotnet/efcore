// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for relational events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
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

            // Command events
            CommandExecuting = CoreEventId.RelationalBaseId + 100,
            CommandExecuted,
            CommandError,

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

            // DataReader events
            DataReaderDisposing = CoreEventId.RelationalBaseId + 300,

            // Migrations events
            MigrateUsingConnection = CoreEventId.RelationalBaseId + 400,
            MigrationReverting,
            MigrationApplying,
            MigrationGeneratingDownScript,
            MigrationGeneratingUpScript,
            MigrationsNotApplied,
            MigrationsNotFound,
            MigrationAttributeMissingWarning,

            // Query events
            QueryClientEvaluationWarning = CoreEventId.RelationalBaseId + 500,
            QueryPossibleUnintendedUseOfEqualsWarning,
            QueryPossibleExceptionWithAggregateOperator,
            ValueConversionSqlLiteralWarning,

            // Model validation events
            ModelValidationKeyDefaultValueWarning = CoreEventId.RelationalBaseId + 600,
            BoolWithDefaultWarning,

            // Update events
            BatchReadyForExecution = CoreEventId.RelationalBaseId + 700,
            BatchSmallerThanMinBatchSize
        }

        private static readonly string _connectionPrefix = DbLoggerCategory.Database.Connection.Name + ".";
        private static EventId MakeConnectionId(Id id) => new EventId((int)id, _connectionPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database connection is opening.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionOpening = MakeConnectionId(Id.ConnectionOpening);

        /// <summary>
        ///     <para>
        ///         A database connection has been opened.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionOpened = MakeConnectionId(Id.ConnectionOpened);

        /// <summary>
        ///     <para>
        ///         A database connection is closing.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionClosing = MakeConnectionId(Id.ConnectionClosing);

        /// <summary>
        ///     <para>
        ///         A database connection has been closed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionClosed = MakeConnectionId(Id.ConnectionClosed);

        /// <summary>
        ///     <para>
        ///         A error occurred while opening or using a database connection.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionError = MakeConnectionId(Id.ConnectionError);

        private static readonly string _sqlPrefix = DbLoggerCategory.Database.Command.Name + ".";
        private static EventId MakeCommandId(Id id) => new EventId((int)id, _sqlPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database command is executing.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CommandEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CommandExecuting = MakeCommandId(Id.CommandExecuting);

        /// <summary>
        ///     <para>
        ///         A database command has been executed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CommandExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CommandExecuted = MakeCommandId(Id.CommandExecuted);

        /// <summary>
        ///     <para>
        ///         An error occurred while a database command was executing.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CommandErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CommandError = MakeCommandId(Id.CommandError);

        private static readonly string _transactionPrefix = DbLoggerCategory.Database.Transaction.Name + ".";
        private static EventId MakeTransactionId(Id id) => new EventId((int)id, _transactionPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database transaction has been started.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionStarted = MakeTransactionId(Id.TransactionStarted);

        /// <summary>
        ///     <para>
        ///         Entity Framework started using an already existing database transaction.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionUsed = MakeTransactionId(Id.TransactionUsed);

        /// <summary>
        ///     <para>
        ///         A database transaction has been committed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionCommitted = MakeTransactionId(Id.TransactionCommitted);

        /// <summary>
        ///     <para>
        ///         A database transaction has been rolled back.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEndEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionRolledBack = MakeTransactionId(Id.TransactionRolledBack);

        /// <summary>
        ///     <para>
        ///         A database transaction has been disposed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionDisposed = MakeTransactionId(Id.TransactionDisposed);

        /// <summary>
        ///     <para>
        ///         An error has occurred while using. committing, or rolling back a database transaction.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionError = MakeTransactionId(Id.TransactionError);

        /// <summary>
        ///     <para>
        ///         An application may have expected an ambient transaction to be used when it was actually ignored.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId AmbientTransactionWarning = MakeTransactionId(Id.AmbientTransactionWarning);

        /// <summary>
        ///     <para>
        ///         Entity Framework enlisted the connection in an ambient transaction.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEnlistedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId AmbientTransactionEnlisted = MakeTransactionId(Id.AmbientTransactionEnlisted);

        /// <summary>
        ///     <para>
        ///         The connection was explicitly enlisted in a transaction.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEnlistedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ExplicitTransactionEnlisted = MakeTransactionId(Id.ExplicitTransactionEnlisted);

        /// <summary>
        ///     <para>
        ///         A database data reader has been disposed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DataReaderDisposingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DataReaderDisposing = MakeCommandId(Id.DataReaderDisposing);

        private static readonly string _migrationsPrefix = DbLoggerCategory.Migrations.Name + ".";
        private static EventId MakeMigrationsId(Id id) => new EventId((int)id, _migrationsPrefix + id);

        /// <summary>
        ///     <para>
        ///         Migrations is using a database connection.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigratorConnectionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrateUsingConnection = MakeMigrationsId(Id.MigrateUsingConnection);

        /// <summary>
        ///     <para>
        ///         A migration is being reverted.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationReverting = MakeMigrationsId(Id.MigrationReverting);

        /// <summary>
        ///     <para>
        ///         A migration is being applied.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationApplying = MakeMigrationsId(Id.MigrationApplying);

        /// <summary>
        ///     <para>
        ///         Migrations is generating a "down" script.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationScriptingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationGeneratingDownScript = MakeMigrationsId(Id.MigrationGeneratingDownScript);

        /// <summary>
        ///     <para>
        ///         Migrations is generating an "up" script.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationScriptingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationGeneratingUpScript = MakeMigrationsId(Id.MigrationGeneratingUpScript);

        /// <summary>
        ///     <para>
        ///         Migrations weren't applied.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigratorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationsNotApplied = MakeMigrationsId(Id.MigrationsNotApplied);

        /// <summary>
        ///     <para>
        ///         Migrations weren't found.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationAssemblyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationsNotFound = MakeMigrationsId(Id.MigrationsNotFound);

        /// <summary>
        ///     <para>
        ///         A MigrationAttribute isn't specified on the class.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationTypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationAttributeMissingWarning = MakeMigrationsId(Id.MigrationAttributeMissingWarning);

        private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";
        private static EventId MakeQueryId(Id id) => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     <para>
        ///         Part of a query is being evaluated on the client instead of on the database server.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryModelClientEvalEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryClientEvaluationWarning = MakeQueryId(Id.QueryClientEvaluationWarning);

        /// <summary>
        ///     <para>
        ///         A query is using equals comparisons in a possibly unintended way.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryPossibleUnintendedUseOfEqualsWarning = MakeQueryId(Id.QueryPossibleUnintendedUseOfEqualsWarning);

        /// <summary>
        ///     <para>
        ///         A query is using a possibly throwing aggregate operation in a sub-query.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryPossibleExceptionWithAggregateOperator = MakeQueryId(Id.QueryPossibleExceptionWithAggregateOperator);

        /// <summary>
        ///     <para>
        ///         A SQL literal is being generated for a value that is using a value conversion.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId ValueConversionSqlLiteralWarning = MakeQueryId(Id.ValueConversionSqlLiteralWarning);

        private static readonly string _validationPrefix = DbLoggerCategory.Model.Validation.Name + ".";
        private static EventId MakeValidationId(Id id) => new EventId((int)id, _validationPrefix + id);

        /// <summary>
        ///     <para>
        ///         A single database default column value has been set on a key column.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ModelValidationKeyDefaultValueWarning = MakeValidationId(Id.ModelValidationKeyDefaultValueWarning);

        /// <summary>
        ///     <para>
        ///         A bool property is configured with a store-generated default.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId BoolWithDefaultWarning = MakeValidationId(Id.BoolWithDefaultWarning);

        private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";
        private static EventId MakeUpdateId(Id id) => new EventId((int)id, _updatePrefix + id);

        /// <summary>
        ///     <para>
        ///         Update commands were batched and are now ready for execution
        ///         <see cref="RelationalDbContextOptionsBuilder{TBuilder,TExtension}.MinBatchSize" />.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="BatchEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId BatchReadyForExecution = MakeUpdateId(Id.BatchReadyForExecution);

        /// <summary>
        ///     <para>
        ///         Update commands were not batched because there were fewer than
        ///         <see cref="RelationalDbContextOptionsBuilder{TBuilder,TExtension}.MinBatchSize" />.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MinBatchSizeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId BatchSmallerThanMinBatchSize = MakeUpdateId(Id.BatchSmallerThanMinBatchSize);
    }
}
