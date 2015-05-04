// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDatabaseExtensionsTest
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var database = new SqlServerDatabase(
                TestHelpers.Instance.CreateContext(),
                Mock.Of<IRelationalDataStoreCreator>(),
                Mock.Of<ISqlServerConnection>(),
                Mock.Of<IMigrator>(),
                new LoggerFactory());

            Assert.Same(database, database.AsSqlServer());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var database = new ConcreteDatabase(
                TestHelpers.Instance.CreateContext(),
                Mock.Of<IDataStoreCreator>(),
                new LoggerFactory());

            Assert.Equal(
                Strings.SqlServerNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsSqlServer()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                DbContext context,
                IDataStoreCreator dataStoreCreator,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, loggerFactory)
            {
            }
        }
    }
}
