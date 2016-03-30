// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Storage
{
    public class RelationalTransactionExtensionsTest
    {
        [Fact]
        public void GetDbTransaction_returns_the_DbTransaction()
        {
            var dbConnection = new FakeDbConnection(ConnectionString);
            var dbTransaction = new FakeDbTransaction(dbConnection);

            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection }));

            var transaction = new RelationalTransaction(
                connection,
                dbTransaction,
                new ListLogger(new List<Tuple<LogLevel, string>>()),
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
                .AddOrUpdateExtension(optionsExtension ?? new FakeRelationalOptionsExtension { ConnectionString = ConnectionString });

            return optionsBuilder.Options;
        }
    }
}
