// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Tests
{
    public class RelationalConnectionTest
    {
        [Fact]
        public void Can_create_new_connection_with_connection_string()
        {
            using (var connection = new FakeConnection(CreateConfiguration(e => e.ConnectionString = "Database=FrodoLives")))
            {
                var dbConnection = connection.DbConnection;

                Assert.Equal("Database=FrodoLives", dbConnection.ConnectionString);
            }
        }

        private static ContextConfiguration CreateConfiguration(Action<FakeConfigurationExtension> configUpdater)
        {
            IEntityConfigurationConstruction entityConfiguration = new EntityConfiguration();
            entityConfiguration.AddOrUpdateExtension(configUpdater);

            var contextConfigurationMock = new Mock<ContextConfiguration>();
            contextConfigurationMock.Setup(m => m.EntityConfiguration).Returns((EntityConfiguration)entityConfiguration);

            return contextConfigurationMock.Object;
        }

        private class FakeConnection : RelationalConnection
        {
            public FakeConnection([NotNull] ContextConfiguration configuration)
                : base(configuration)
            {
            }

            protected override DbConnection CreateDbConnection()
            {
                var connectionMock = new Mock<DbConnection>();
                connectionMock.Setup(m => m.ConnectionString).Returns(ConnectionString);
                return connectionMock.Object;
            }
        }

        private class FakeConfigurationExtension : RelationalConfigurationExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }
    }
}
