// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDatabaseFacadeExtensions
    {
        public static void Migrate([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IMigrator>().Migrate();

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

        public static IDbContextTransaction BeginTransaction([NotNull] this DatabaseFacade databaseFacade, IsolationLevel isolationLevel)
        {
            var transactionManager = GetTransactionManager(databaseFacade);

            var relationalTransactionManager = transactionManager as IRelationalTransactionManager;

            return (relationalTransactionManager != null)
                ? relationalTransactionManager.BeginTransaction(isolationLevel)
                : transactionManager.BeginTransaction();
        }

        public static Task<IDbContextTransaction> BeginTransactionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var transactionManager = GetTransactionManager(databaseFacade);

            var relationalTransactionManager = transactionManager as IRelationalTransactionManager;

            return (relationalTransactionManager != null)
                ? relationalTransactionManager.BeginTransactionAsync(isolationLevel, cancellationToken)
                : transactionManager.BeginTransactionAsync(cancellationToken);
        }

        public static IDbContextTransaction UseTransaction(
            [NotNull] this DatabaseFacade databaseFacade, [CanBeNull] DbTransaction transaction)
        {
            var transactionManager = GetTransactionManager(databaseFacade);

            var relationalTransactionManager = transactionManager as IRelationalTransactionManager;

            if (relationalTransactionManager == null)
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return relationalTransactionManager.UseTransaction(transaction);
        }

        public static void SetCommandTimeout([NotNull] this DatabaseFacade databaseFacade, int? timeout)
            => GetRelationalConnection(databaseFacade).CommandTimeout = timeout;

        public static int? GetCommandTimeout([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).CommandTimeout;

        private static IRelationalConnection GetRelationalConnection([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IRelationalConnection>();

        private static IDbContextTransactionManager GetTransactionManager([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IDbContextTransactionManager>();
    }
}
