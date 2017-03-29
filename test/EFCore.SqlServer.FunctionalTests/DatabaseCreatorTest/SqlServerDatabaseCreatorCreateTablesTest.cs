// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.DatabaseCreatorTest
{
    public class SqlServerDatabaseCreatorCreateTablesTest
    {
        [Fact]
        public async Task CreateTables_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(async: false);
        }

        [Fact]
        public async Task CreateTablesAsync_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(async: true);
        }

        private static async Task CreateTables_creates_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                if (async)
                {
                    await creator.CreateTablesAsync();
                }
                else
                {
                    creator.CreateTables();
                }

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                var tables = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
                Assert.Equal(1, tables.Count());
                Assert.Equal("Blogs", tables.Single());

                var columns = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME + '.' + COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs'");
                Assert.Equal(15, columns.Count());
                Assert.True(columns.Any(c => c == "Blogs.Key1"));
                Assert.True(columns.Any(c => c == "Blogs.Key2"));
                Assert.True(columns.Any(c => c == "Blogs.Cheese"));
                Assert.True(columns.Any(c => c == "Blogs.ErMilan"));
                Assert.True(columns.Any(c => c == "Blogs.George"));
                Assert.True(columns.Any(c => c == "Blogs.TheGu"));
                Assert.True(columns.Any(c => c == "Blogs.NotFigTime"));
                Assert.True(columns.Any(c => c == "Blogs.ToEat"));
                Assert.True(columns.Any(c => c == "Blogs.CupOfChar"));
                Assert.True(columns.Any(c => c == "Blogs.OrNothing"));
                Assert.True(columns.Any(c => c == "Blogs.Fuse"));
                Assert.True(columns.Any(c => c == "Blogs.WayRound"));
                Assert.True(columns.Any(c => c == "Blogs.On"));
                Assert.True(columns.Any(c => c == "Blogs.AndChew"));
                Assert.True(columns.Any(c => c == "Blogs.AndRow"));
            }
        }

        [Fact]
        public async Task CreateTables_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(async: false);
        }

        [Fact]
        public async Task CreateTablesAsync_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(async: true);
        }

        private static async Task CreateTables_throws_if_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
            {
                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                var errorNumber
                    = async
                        ? (await Assert.ThrowsAsync<SqlException>(() => creator.CreateTablesAsync())).Number
                        : Assert.Throws<SqlException>(() => creator.CreateTables()).Number;

                if (errorNumber != 233) // skip if no-process transient failure
                {
                    Assert.Equal(
                        4060, // Login failed error number
                        errorNumber);
                }
            }
        }

        [Fact]
        public async Task Create_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(async: true);
        }

        private static async Task Create_creates_physical_database_but_not_tables_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
            {
                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                Assert.False(creator.Exists());

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }

                Assert.True(creator.Exists());

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                Assert.Equal(0, (await testDatabase.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")).Count());

                Assert.True(await testDatabase.ExecuteScalarAsync<bool>(
                    string.Concat(
                        "SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name='",
                        testDatabase.Connection.Database,
                        "'")));
            }
        }

        [Fact]
        public async Task Create_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(async: true);
        }

        private static async Task Create_throws_if_database_already_exists_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                var ex = async
                    ? await Assert.ThrowsAsync<SqlException>(() => creator.CreateAsync())
                    : Assert.Throws<SqlException>(() => creator.Create());
                Assert.Equal(
                    1801, // Database with given name already exists
                    ex.Number);
            }
        }
    }
}
