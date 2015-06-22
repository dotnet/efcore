// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDatabaseExtensions
    {
        public static void ApplyMigrations([NotNull] this Database database)
            => Check.NotNull(database, nameof(database)).GetService<IMigrator>().ApplyMigrations();

        public static DbConnection GetDbConnection([NotNull] this Database database)
            => GetRelationalConnection(database).DbConnection;

        public static void OpenConnection([NotNull] this Database database)
            => GetRelationalConnection(database).Open();

        public static Task OpenConnectionAsync(
            [NotNull] this Database database, 
            CancellationToken cancellationToken = default(CancellationToken))
            => GetRelationalConnection(database).OpenAsync(cancellationToken);

        public static void CloseConnection([NotNull] this Database database)
            => GetRelationalConnection(database).Close();

        public static DbTransaction GetDbTransaction([NotNull] this Database database)
            => GetRelationalConnection(database).DbTransaction;

        public static IRelationalTransaction BeginTransaction([NotNull] this Database database)
            => GetRelationalConnection(database).BeginTransaction();

        public static Task<IRelationalTransaction> BeginTransactionAsync(
            [NotNull] this Database database, 
            CancellationToken cancellationToken = default(CancellationToken))
            => GetRelationalConnection(database).BeginTransactionAsync(cancellationToken);

        public static IRelationalTransaction BeginTransaction([NotNull] this Database database, IsolationLevel isolationLevel)
            => GetRelationalConnection(database).BeginTransaction(isolationLevel);

        public static Task<IRelationalTransaction> BeginTransactionAsync(
            [NotNull] this Database database,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
            => GetRelationalConnection(database).BeginTransactionAsync(isolationLevel, cancellationToken);

        public static IRelationalTransaction UseTransaction(
            [NotNull] this Database database, [CanBeNull] DbTransaction transaction)
            => GetRelationalConnection(database).UseTransaction(transaction);

        public static void CommitTransaction([NotNull] this Database database)
            => GetRelationalConnection(database).Transaction.Commit();

        public static void RollbackTransaction([NotNull] this Database database)
            => GetRelationalConnection(database).Transaction.Rollback();

        public static void SetCommandTimeout([NotNull] this Database database, int? timeout)
            => GetRelationalConnection(database).CommandTimeout = timeout;

        public static int? GetCommandTimeout([NotNull] this Database database)
            => GetRelationalConnection(database).CommandTimeout;

        private static IRelationalConnection GetRelationalConnection([NotNull] this Database database)
            => Check.NotNull(database, nameof(database)).GetService<IRelationalConnection>();
    }
}
