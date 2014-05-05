// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerDatabaseCreationTest
    {
        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Exists_returns_false_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    Assert.False(async ? await context.Database.ExistsAsync() : context.Database.Exists());

                    Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);
                }
            }
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false);
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true);
        }

        private static async Task Exists_returns_true_when_database_exists_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: true))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    Assert.True(async ? await context.Database.ExistsAsync() : context.Database.Exists());

                    Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);
                }
            }
        }

        [Fact]
        public async Task Delete_will_delete_database()
        {
            await Delete_will_delete_database_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_will_delete_database()
        {
            await Delete_will_delete_database_test(async: true);
        }

        private static async Task Delete_will_delete_database_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: true))
            {
                testDatabase.Connection.Close();

                using (var context = new BloggingContext(testDatabase))
                {
                    Assert.True(async ? await context.Database.ExistsAsync() : context.Database.Exists());

                    if (async)
                    {
                        await context.Database.DeleteAsync();
                    }
                    else
                    {
                        context.Database.Delete();
                    }

                    Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);

                    Assert.False(async ? await context.Database.ExistsAsync() : context.Database.Exists());

                    Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);
                }
            }
        }

        [Fact]
        public async Task Delete_noop_when_database_doesnt_exist()
        {
            await Delete_noop_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_noop_when_database_doesnt_exist()
        {
            await Delete_noop_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Delete_noop_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    Assert.False(async ? await context.Database.ExistsAsync() : context.Database.Exists());

                    if (async)
                    {
                        await context.Database.DeleteAsync();
                    }
                    else
                    {
                        context.Database.Delete();
                    }

                    Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);

                    Assert.False(async ? await context.Database.ExistsAsync() : context.Database.Exists());

                    Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);
                }
            }
        }

        [Fact]
        public async Task Can_create_schema_in_existing_database()
        {
            await Can_create_schema_in_existing_database_test(async: false);
        }

        [Fact]
        public async Task Can_create_schema_in_existing_database_async()
        {
            await Can_create_schema_in_existing_database_test(async: true);
        }

        private static async Task Can_create_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                await RunDatabaseCreationTest(testDatabase, async);
            }
        }

        [Fact]
        public async Task Can_create_physical_database_and_schema()
        {
            await Can_create_physical_database_and_schema_test(async: false);
        }

        [Fact]
        public async Task Can_create_physical_database_and_schema_async()
        {
            await Can_create_physical_database_and_schema_test(async: true);
        }

        private static async Task Can_create_physical_database_and_schema_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                await RunDatabaseCreationTest(testDatabase, async);
            }
        }

        private static DbContextConfiguration CreateConfiguration(TestDatabase testDatabase)
        {
            return new DbContext(
                new ServiceCollection()
                    .AddEntityFramework(s => s.AddSqlServer())
                    .BuildServiceProvider(),
                new DbContextOptions()
                    .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration())
                .Configuration;
        }

        private static SqlServerDataStoreCreator GetDataStoreCreator(TestDatabase testDatabase)
        {
            return CreateConfiguration(testDatabase).Services.ServiceProvider.GetService<SqlServerDataStoreCreator>();
        }

        private static async Task RunDatabaseCreationTest(TestDatabase testDatabase, bool async)
        {
            using (var context = new BloggingContext(testDatabase))
            {
                Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);

                if (async)
                {
                    await context.Database.CreateAsync();
                }
                else
                {
                    context.Database.Create();
                }

                Assert.Equal(ConnectionState.Closed, ((RelationalConnection)context.Database.Connection).DbConnection.State);

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                var tables = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
                Assert.Equal(1, tables.Count());
                Assert.Equal("Blog", tables.Single());

                var columns = (await testDatabase.QueryAsync<string>(
                    "SELECT TABLE_NAME + '.' + COLUMN_NAME + ' (' + DATA_TYPE + ')' FROM INFORMATION_SCHEMA.COLUMNS")).ToArray();
                Assert.Equal(19, columns.Length);

                Assert.Equal(
                    new[]
                        {
                            "Blog.AndChew (varbinary)", 
                            "Blog.AndRow (timestamp)", 
                            "Blog.Cheese (nvarchar)", 
                            "Blog.CupOfChar (int)", 
                            "Blog.ErMilan (int)", 
                            "Blog.Fuse (smallint)", 
                            "Blog.George (bit)", 
                            "Blog.Key1 (nvarchar)", 
                            "Blog.Key2 (varbinary)", 
                            "Blog.NotFigTime (datetime2)", 
                            "Blog.NotToEat (smallint)", 
                            "Blog.On (real)", 
                            "Blog.OrNothing (float)", 
                            "Blog.OrULong (int)", 
                            "Blog.OrUShort (numeric)", 
                            "Blog.OrUSkint (bigint)", 
                            "Blog.TheGu (uniqueidentifier)", 
                            "Blog.ToEat (tinyint)", 
                            "Blog.WayRound (bigint)"
                        },
                    columns);
            }
        }

        private static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddEntityFramework(s => s.AddSqlServer())
                .BuildServiceProvider();
        }

        private class BloggingContext : DbContext
        {
            private readonly TestDatabase _testDatabase;

            public BloggingContext(TestDatabase testDatabase)
                : base(CreateServiceProvider())
            {
                _testDatabase = testDatabase;
            }

            protected override void OnConfiguring(DbContextOptions builder)
            {
                builder.SqlServerConnectionString(_testDatabase.Connection.ConnectionString);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                var blogType = builder.Model.GetEntityType(typeof(Blog));

                blogType.SetKey(blogType.GetProperty("Key1"), blogType.GetProperty("Key2"));
                blogType.RemoveProperty(blogType.GetProperty("AndRow"));
                blogType.AddProperty("AndRow", typeof(byte[]), shadowProperty: false, concurrencyToken: true);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public string Key1 { get; set; }
            public byte[] Key2 { get; set; }
            public string Cheese { get; set; }
            public int ErMilan { get; set; }
            public bool George { get; set; }
            public Guid TheGu { get; set; }
            public DateTime NotFigTime { get; set; }
            public byte ToEat { get; set; }
            public char CupOfChar { get; set; }
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            public sbyte NotToEat { get; set; }
            public float On { get; set; }
            public ushort OrULong { get; set; }
            public uint OrUSkint { get; set; }
            public ulong OrUShort { get; set; }
            public byte[] AndChew { get; set; }
            public byte[] AndRow { get; set; }
        }
    }
}
