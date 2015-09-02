// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDatabaseSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(SqlServerConnection).GetTypeInfo().Assembly.GetName().Name,
                new SqlServerDatabaseProviderServices(SqlServerTestHelpers.Instance.CreateServiceProvider()).InvariantName);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            Assert.True(new DatabaseProvider<SqlServerDatabaseProviderServices, SqlServerOptionsExtension>().IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(new DatabaseProvider<SqlServerDatabaseProviderServices, SqlServerOptionsExtension>().IsConfigured(optionsBuilder.Options));
        }
    }
}
