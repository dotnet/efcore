// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalTransactionExtensionsTest
    {
        [Fact]
        public void GetDbTransaction_returns_the_DbTransaction()
        {
            var dbConnection = new FakeDbConnection(ConnectionString);
            var dbTransaction = new FakeDbTransaction(dbConnection);

            var connection = new FakeRelationalConnection(
                CreateOptions((FakeRelationalOptionsExtension)new FakeRelationalOptionsExtension().WithConnection(dbConnection)));

            var loggerFactory = new ListLoggerFactory();

            var transaction = new RelationalTransaction(
                connection,
                dbTransaction,
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    loggerFactory,
                    new LoggingOptions(),
                    new DiagnosticListener("Fake")),
                false);

            Assert.Equal(dbTransaction, transaction.GetDbTransaction());
        }

        [Fact]
        public void GetDbTransaction_throws_on_non_relational_provider()
        {
            var transaction = new NonRelationalTransaction();

            Assert.Equal(
                RelationalStrings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(
                    () => transaction.GetDbTransaction()).Message);
        }

        private class NonRelationalTransaction : IDbContextTransaction
        {
            public Guid TransactionId { get; } = Guid.NewGuid();

            public void Commit()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Rollback()
            {
                throw new NotImplementedException();
            }
        }

        private const string ConnectionString = "Fake Connection String";

        public static IDbContextOptions CreateOptions(
            FakeRelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(optionsExtension ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

            return optionsBuilder.Options;
        }
    }
}
