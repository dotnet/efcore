// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerDatabaseSourceTest
    {
        [ConditionalFact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(SqlServerConnection).Assembly.GetName().Name,
                new DatabaseProvider<SqlServerOptionsExtension>(new DatabaseProviderDependencies()).Name);
        }

        [ConditionalFact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            Assert.True(
                new DatabaseProvider<SqlServerOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
        }

        [ConditionalFact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(
                new DatabaseProvider<SqlServerOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
        }
    }
}
