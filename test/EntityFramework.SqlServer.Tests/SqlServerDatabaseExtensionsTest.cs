using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
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
                new DbContextService<IModel>(() => null),
                Mock.Of<SqlServerDataStoreCreator>(),
                Mock.Of<SqlServerConnection>(),
                Mock.Of<SqlServerMigrator>(),
                new LoggerFactory());

            Assert.Same(database, database.AsSqlServer());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var database = new ConcreteDatabase(
                new DbContextService<IModel>(() => null),
                Mock.Of<DataStoreCreator>(),
                Mock.Of<DataStoreConnection>(),
                new LoggerFactory());

            Assert.Equal(
                Strings.SqlServerNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsSqlServer()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                DbContextService<IModel> model,
                DataStoreCreator dataStoreCreator,
                DataStoreConnection connection,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, loggerFactory)
            {
            }
        }
    }
}