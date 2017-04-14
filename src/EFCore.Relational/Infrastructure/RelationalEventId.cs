// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
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
            ModelValidationKeyDefaultValueWarning = CoreEventId.RelationalBaseId + 600,
        }

        private static readonly string _connectionPrefix = LoggerCategory.Database.Connection.Name + ".";
        private static EventId MakeConnectionId(Id id) => new EventId((int)id, _connectionPrefix + id);

        /// <summary>
        ///     A database connection is opening.
        ///     This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        /// </summary>
        public static readonly EventId ConnectionOpening = MakeConnectionId(Id.ConnectionOpening);

        /// <summary>
        ///     A database connection has been opened.
        ///     This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        /// </summary>
        public static readonly EventId ConnectionOpened = MakeConnectionId(Id.ConnectionOpened);

        /// <summary>
        ///     A database connection is closing.
        ///     This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        /// </summary>
        public static readonly EventId ConnectionClosing = MakeConnectionId(Id.ConnectionClosing);

        /// <summary>
        ///     A database connection has been closed.
        ///     This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        /// </summary>
        public static readonly EventId ConnectionClosed = MakeConnectionId(Id.ConnectionClosed);

        /// <summary>
        ///     A error occurred while opening or using a database connection.
        ///     This event is in the <see cref="LoggerCategory.Database.Connection" /> category.
        /// </summary>
        public static readonly EventId ConnectionError = MakeConnectionId(Id.ConnectionError);

        private static readonly string _sqlPrefix = LoggerCategory.Database.Sql.Name + ".";
        private static EventId MakeCommandId(Id id) => new EventId((int)id, _sqlPrefix + id);

        /// <summary>
        ///     A database command is executing.
        ///     This event is in the <see cref="LoggerCategory.Database.Sql" /> category.
        /// </summary>
        public static readonly EventId CommandExecuting = MakeCommandId(Id.CommandExecuting);

        /// <summary>
        ///     A database command has been executed.
        ///     This event is in the <see cref="LoggerCategory.Database.Sql" /> category.
        /// </summary>
        public static readonly EventId CommandExecuted = MakeCommandId(Id.CommandExecuted);

        /// <summary>
        ///     An error occurred while a database command was executing.
        ///     This event is in the <see cref="LoggerCategory.Database.Sql" /> category.
        /// </summary>
        public static readonly EventId CommandError = MakeCommandId(Id.CommandError);

        private static readonly string _transactionPrefix = LoggerCategory.Database.Transaction.Name + ".";
        private static EventId MakeTransactionId(Id id) => new EventId((int)id, _transactionPrefix + id);

        /// <summary>
        ///     A database transaction has been started.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionStarted = MakeTransactionId(Id.TransactionStarted);

        /// <summary>
        ///     Entity Framework started using an already existing database transaction.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionUsed = MakeTransactionId(Id.TransactionUsed);

        /// <summary>
        ///     A database transaction has been committed.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionCommitted = MakeTransactionId(Id.TransactionCommitted);

        /// <summary>
        ///     A database transaction has been rolled back.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionRolledBack = MakeTransactionId(Id.TransactionRolledBack);

        /// <summary>
        ///     A database transaction has been disposed.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionDisposed = MakeTransactionId(Id.TransactionDisposed);

        /// <summary>
        ///     An error has occurred while using. committing, or rolling back a database transaction.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionError = MakeTransactionId(Id.TransactionError);

        /// <summary>
        ///     An application may have expected an ambient transaction to be used when it was actually ignorred.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId AmbientTransactionWarning = MakeTransactionId(Id.AmbientTransactionWarning);

        private static readonly string _dataReaderPrefix = LoggerCategory.Database.DataReader.Name + ".";
        private static EventId MakeReaderId(Id id) => new EventId((int)id, _dataReaderPrefix + id);

        /// <summary>
        ///     A database data reader has been disposed.
        ///     This event is in the <see cref="LoggerCategory.Database.DataReader" /> category.
        /// </summary>
        public static readonly EventId DataReaderDisposing = MakeReaderId(Id.DataReaderDisposing);

        private static readonly string _migrationsPrefix = LoggerCategory.Migrations.Name + ".";
        private static EventId MakeMigrationsId(Id id) => new EventId((int)id, _migrationsPrefix + id);

        /// <summary>
        ///     Migrations is using a database connection.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrateUsingConnection = MakeMigrationsId(Id.MigrateUsingConnection);

        /// <summary>
        ///     A migration is being reverted.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationReverting = MakeMigrationsId(Id.MigrationReverting);

        /// <summary>
        ///     A migration is being applied.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationApplying = MakeMigrationsId(Id.MigrationApplying);

        /// <summary>
        ///     Migrations is generating a "down" script.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationGeneratingDownScript = MakeMigrationsId(Id.MigrationGeneratingDownScript);

        /// <summary>
        ///     Migrations is generating an "up" script.
        ///     This event is in the <see cref="LoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId MigrationGeneratingUpScript = MakeMigrationsId(Id.MigrationGeneratingUpScript);

        private static readonly string _queryPrefix = LoggerCategory.Query.Name + ".";
        private static EventId MakeQueryId(Id id) => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     Part of a query is being evaluated on the client instead of on the database server.
        ///     This event is in the <see cref="LoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId QueryClientEvaluationWarning = MakeQueryId(Id.QueryClientEvaluationWarning);

        /// <summary>
        ///     A query is using equals comparisons in a possibly unintended way.
        ///     This event is in the <see cref="LoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId QueryPossibleUnintendedUseOfEqualsWarning = MakeQueryId(Id.QueryPossibleUnintendedUseOfEqualsWarning);

        private static readonly string _validationPrefix = LoggerCategory.Model.Validation.Name + ".";
        private static EventId MakeValidationId(Id id) => new EventId((int)id, _validationPrefix + id);

        /// <summary>
        ///     A single database default column value has been set on a key column.
        ///     This event is in the <see cref="LoggerCategory.Model.Validation" /> category.
        /// </summary>
        public static readonly EventId ModelValidationKeyDefaultValueWarning = MakeValidationId(Id.ModelValidationKeyDefaultValueWarning);
    }
}
