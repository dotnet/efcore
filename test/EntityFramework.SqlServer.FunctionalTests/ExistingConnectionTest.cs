// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class ExistingConnectionTest
    {
        // See aspnet/Data#135
#if !ASPNETCORE50
        [Fact]
        public async Task Can_use_an_existing_closed_connection()
        {
            await Can_use_an_existing_closed_connection_test(openConnection: false);
        }
#endif

        [Fact]
        public async Task Can_use_an_existing_open_connection()
        {
            await Can_use_an_existing_closed_connection_test(openConnection: true);
        }

        private static async Task Can_use_an_existing_closed_connection_test(bool openConnection)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .UseLoggerFactory<LoggerFactory>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerTestDatabase.Northwind())
            {
                var openCount = 0;
                var closeCount = 0;
                var disposeCount = 0;

                using (var connection = new SqlConnection(SqlServerTestDatabase.NorthwindConnectionString))
                {
                    if (openConnection)
                    {
                        await connection.OpenAsync();
                    }

                    connection.StateChange += (_, a) =>
                        {
                            if (a.CurrentState == ConnectionState.Open)
                            {
                                openCount++;
                            }
                            else if (a.CurrentState == ConnectionState.Closed)
                            {
                                closeCount++;
                            }
                        };
#if !ASPNETCORE50
                    connection.Disposed += (_, __) => disposeCount++;
#endif

                    using (var context = new NorthwindContext(serviceProvider, connection))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }

                    if (openConnection)
                    {
                        Assert.Equal(ConnectionState.Open, connection.State);
                        Assert.Equal(0, openCount);
                        Assert.Equal(0, closeCount);
                    }
                    else
                    {
                        Assert.Equal(ConnectionState.Closed, connection.State);
                        Assert.Equal(1, openCount);
                        Assert.Equal(1, closeCount);
                    }

                    Assert.Equal(0, disposeCount);
                }
            }
        }

        private class NorthwindContext : DbContext
        {
            private readonly SqlConnection _connection;

            public NorthwindContext(IServiceProvider serviceProvider, SqlConnection connection)
                : base(serviceProvider)
            {
                _connection = connection;
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(_connection);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Key(c => c.CustomerID);
                        b.ForSqlServer().Table("Customers");
                    });
            }
        }

        private class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }
    }
}
