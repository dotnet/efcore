// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerConnectionTest
    {
        [Fact]
        public void Creates_SQL_Server_connection_string()
        {
            using (var connection = new SqlServerConnection(CreateConfiguration()))
            {
                Assert.IsType<SqlConnection>(connection.DbConnection);
            }
        }

        [Fact]
        public void Can_create_master_connection_string()
        {
            using (var connection = new SqlServerConnection(CreateConfiguration()))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(@"Data Source=""(localdb)11.0"";Initial Catalog=master;Integrated Security=True", master.ConnectionString);
                }
            }
        }

        public static DbContextConfiguration CreateConfiguration()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework().AddSqlServer();
            return new DbContext(serviceCollection.BuildServiceProvider(),
                new DbContextOptions()
                    .UseSqlServer("Server=(localdb)\v11.0;Database=SqlServerConnectionTest;Trusted_Connection=True;")
                    .BuildConfiguration()).Configuration;
        }
    }
}
