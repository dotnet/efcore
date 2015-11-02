// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDatabaseFacadeExtensions
    {
        /// <summary>
        ///     Applies any pending migrations for the context to the database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade"/> for the context. </param>
        public static void Migrate([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IMigrator>().Migrate();

        /// <summary>
        ///     Asynchronously applies any pending migrations for the context to the database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade"/> for the context. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous migration operation. </returns>
        public static Task MigrateAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IMigrator>()
                .MigrateAsync(cancellationToken: cancellationToken);

        public static void ExecuteSqlCommand(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            [NotNull] params object[] parameters)
            => Check.NotNull(databaseFacade, nameof(databaseFacade))
                .GetService<RelationalSqlExecutor>()
                    .ExecuteSqlCommand(
                        sql,
                        parameters);

        public static Task ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken),
            [NotNull] params object[] parameters)
            => Check.NotNull(databaseFacade, nameof(databaseFacade))
                .GetService<RelationalSqlExecutor>()
                    .ExecuteSqlCommandAsync(
                        sql,
                        cancellationToken,
                        parameters);

        public static DbConnection GetDbConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).DbConnection;

        public static void OpenConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).Open();

        public static Task OpenConnectionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => GetRelationalConnection(databaseFacade).OpenAsync(cancellationToken);

        public static void CloseConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).Close();

        public static IRelationalTransaction BeginTransaction([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).BeginTransaction();

        public static Task<IRelationalTransaction> BeginTransactionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => GetRelationalConnection(databaseFacade).BeginTransactionAsync(cancellationToken);

        public static IRelationalTransaction BeginTransaction([NotNull] this DatabaseFacade databaseFacade, IsolationLevel isolationLevel)
            => GetRelationalConnection(databaseFacade).BeginTransaction(isolationLevel);

        public static Task<IRelationalTransaction> BeginTransactionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
            => GetRelationalConnection(databaseFacade).BeginTransactionAsync(isolationLevel, cancellationToken);

        public static IRelationalTransaction UseTransaction(
            [NotNull] this DatabaseFacade databaseFacade, [CanBeNull] DbTransaction transaction)
            => GetRelationalConnection(databaseFacade).UseTransaction(transaction);

        public static void CommitTransaction([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).Transaction.Commit();

        public static void RollbackTransaction([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).Transaction.Rollback();

        public static void SetCommandTimeout([NotNull] this DatabaseFacade databaseFacade, int? timeout)
            => GetRelationalConnection(databaseFacade).CommandTimeout = timeout;

        public static int? GetCommandTimeout([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).CommandTimeout;

        private static IRelationalConnection GetRelationalConnection([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IRelationalConnection>();
    }
}
