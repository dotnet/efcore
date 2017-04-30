// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
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

            // DataReader events
            DataReaderDisposing = CoreEventId.RelationalBaseId + 300,

            // Migrations events
            MigrateUsingConnection = CoreEventId.RelationalBaseId + 400,
            MigrationReverting,
            MigrationApplying,
            MigrationGeneratingDownScript,
            MigrationGeneratingUpScript,

            // Query events
            QueryClientEvaluationWarning = CoreEventId.RelationalBaseId + 500,
            QueryPossibleUnintendedUseOfEqualsWarning,

            // Model validation events
            ModelValidationKeyDefaultValueWarning = CoreEventId.RelationalBaseId + 600
        }

        private static readonly string _connectionPrefix = LoggerCategory.Database.Connection.Name + ".";
        private static EventId MakeConnectionId(Id id) => new EventId((int)id, _connectionPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database connection is opening.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionOpening = MakeConnectionId(Id.ConnectionOpening);

        /// <summary>
        ///     <para>
        ///         A database connection has been opened.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEndData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionOpened = MakeConnectionId(Id.ConnectionOpened);

        /// <summary>
        ///     <para>
        ///         A database connection is closing.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionClosing = MakeConnectionId(Id.ConnectionClosing);

        /// <summary>
        ///     <para>
        ///         A database connection has been closed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionEndData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionClosed = MakeConnectionId(Id.ConnectionClosed);

        /// <summary>
        ///     <para>
        ///         A error occurred while opening or using a database connection.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionErrorData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConnectionError = MakeConnectionId(Id.ConnectionError);

        private static readonly string _sqlPrefix = LoggerCategory.Database.Sql.Name + ".";
        private static EventId MakeCommandId(Id id) => new EventId((int)id, _sqlPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database command is executing.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Sql" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CommandData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CommandExecuting = MakeCommandId(Id.CommandExecuting);

        /// <summary>
        ///     <para>
        ///         A database command has been executed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Sql" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CommandExecutedData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CommandExecuted = MakeCommandId(Id.CommandExecuted);

        /// <summary>
        ///     <para>
        ///         An error occurred while a database command was executing.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Sql" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CommandErrorData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CommandError = MakeCommandId(Id.CommandError);

        private static readonly string _transactionPrefix = LoggerCategory.Database.Transaction.Name + ".";
        private static EventId MakeTransactionId(Id id) => new EventId((int)id, _transactionPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database transaction has been started.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionStarted = MakeTransactionId(Id.TransactionStarted);

        /// <summary>
        ///     <para>
        ///         Entity Framework started using an already existing database transaction.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionUsed = MakeTransactionId(Id.TransactionUsed);

        /// <summary>
        ///     <para>
        ///         A database transaction has been committed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEndData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionCommitted = MakeTransactionId(Id.TransactionCommitted);

        /// <summary>
        ///     <para>
        ///         A database transaction has been rolled back.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionEndData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionRolledBack = MakeTransactionId(Id.TransactionRolledBack);

        /// <summary>
        ///     <para>
        ///         A database transaction has been disposed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionDisposed = MakeTransactionId(Id.TransactionDisposed);

        /// <summary>
        ///     <para>
        ///         An error has occurred while using. committing, or rolling back a database transaction.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TransactionErrorData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionError = MakeTransactionId(Id.TransactionError);

        /// <summary>
        ///     <para>
        ///         An application may have expected an ambient transaction to be used when it was actually ignorred.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ConnectionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId AmbientTransactionWarning = MakeTransactionId(Id.AmbientTransactionWarning);

        private static readonly string _dataReaderPrefix = LoggerCategory.Database.DataReader.Name + ".";
        private static EventId MakeReaderId(Id id) => new EventId((int)id, _dataReaderPrefix + id);

        /// <summary>
        ///     <para>
        ///         A database data reader has been disposed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Database.DataReader" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DataReaderDisposingData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DataReaderDisposing = MakeReaderId(Id.DataReaderDisposing);

        private static readonly string _migrationsPrefix = LoggerCategory.Migrations.Name + ".";
        private static EventId MakeMigrationsId(Id id) => new EventId((int)id, _migrationsPrefix + id);

        /// <summary>
        ///     <para>
        ///         Migrations is using a database connection.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigratorConnectionData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrateUsingConnection = MakeMigrationsId(Id.MigrateUsingConnection);

        /// <summary>
        ///     <para>
        ///         A migration is being reverted.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationReverting = MakeMigrationsId(Id.MigrationReverting);

        /// <summary>
        ///     <para>
        ///         A migration is being applied.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationApplying = MakeMigrationsId(Id.MigrationApplying);

        /// <summary>
        ///     <para>
        ///         Migrations is generating a "down" script.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationScriptingData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationGeneratingDownScript = MakeMigrationsId(Id.MigrationGeneratingDownScript);

        /// <summary>
        ///     <para>
        ///         Migrations is generating an "up" script.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Migrations" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="MigrationScriptingData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MigrationGeneratingUpScript = MakeMigrationsId(Id.MigrationGeneratingUpScript);

        private static readonly string _queryPrefix = LoggerCategory.Query.Name + ".";
        private static EventId MakeQueryId(Id id) => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     <para>
        ///         Part of a query is being evaluated on the client instead of on the database server.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryClientEvaluationWarning = MakeQueryId(Id.QueryClientEvaluationWarning);

        /// <summary>
        ///     <para>
        ///         A query is using equals comparisons in a possibly unintended way.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryPossibleUnintendedUseOfEqualsWarning = MakeQueryId(Id.QueryPossibleUnintendedUseOfEqualsWarning);

        private static readonly string _validationPrefix = LoggerCategory.Model.Validation.Name + ".";
        private static EventId MakeValidationId(Id id) => new EventId((int)id, _validationPrefix + id);

        /// <summary>
        ///     <para>
        ///         A single database default column value has been set on a key column.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="LoggerCategory.Model.Validation" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId ModelValidationKeyDefaultValueWarning = MakeValidationId(Id.ModelValidationKeyDefaultValueWarning);
    }
}
