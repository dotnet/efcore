// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDatabaseSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal("SQL Server Database", new SqlServerDatabaseProvider().Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            Assert.True(new SqlServerDatabaseProvider().IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Can_not_be_auto_configured()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var provider = new SqlServerDatabaseProvider();
            provider.AutoConfigure(optionsBuilder);

            Assert.False(provider.IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(new SqlServerDatabaseProvider().IsConfigured(optionsBuilder.Options));
        }
    }
}
