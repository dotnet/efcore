// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
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

        public static ContextConfiguration CreateConfiguration()
        {
            return new DbContext(new ServiceCollection()
                .AddEntityFramework(s => s.AddSqlServer())
                .BuildServiceProvider(),
                new EntityConfigurationBuilder()
                    .SqlServerConnectionString("Server=(localdb)\v11.0;Database=SqlServerConnectionTest;Trusted_Connection=True;")
                    .BuildConfiguration()).Configuration;
        }
    }
}
