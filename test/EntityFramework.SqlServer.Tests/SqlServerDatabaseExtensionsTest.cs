using System;
using Microsoft.Data.Entity.Infrastructure;
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
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new SqlServerDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Same(database, database.AsSqlServer());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new ConcreteDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Equal(
                Strings.SqlServerNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsSqlServer()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(DbContextConfiguration configuration, ILoggerFactory loggerFactory)
                : base(configuration, loggerFactory)
            {
            }
        }
    }
}