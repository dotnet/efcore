// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class ApiExtensionsTest
    {
        [Fact]
        public void Can_create_context_with_connection_string()
        {
            using (var context = new MySqlServerContext())
            {
                var dataStore = (SqlServerDataStore)context.Configuration.DataStore;

                Assert.Equal("Foo", dataStore.ConnectionString);
            }
        }

        private class MySqlServerContext : EntityContext
        {
            protected override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                builder.UseSqlServer(connectionString: "Foo");
            }
        }
    }
}
