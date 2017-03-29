// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.DatabaseCreatorTest
{
    public class SqlServerDatabaseCreatorEnsureCreatedTest
    {
        [Fact]
        public async Task EnsureCreated_can_create_schema_in_existing_database()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreated_can_create_schema_in_existing_database_with_filename()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: false, file: true);
        }

        [Fact]
        public async Task EnsureCreatedAsync_can_create_schema_in_existing_database()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreatedAsync_can_create_schema_in_existing_database_with_filename()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: true, file: true);
        }

        private static async Task EnsureCreated_can_create_schema_in_existing_database_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(useFileName: file))
            {
                await RunDatabaseCreationTest(testDatabase, async);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
        public async Task EnsureCreated_can_create_physical_database_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreated_can_create_physical_database_with_filename_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: false, file: true);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
        public async Task EnsureCreatedAsync_can_create_physical_database_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreatedAsync_can_create_physical_database_with_filename_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: true, file: true);
        }

        private static Task EnsureCreated_can_create_physical_database_and_schema_test(bool async, bool file)
        {
            return SqlServerTestStore.GetExecutionStrategy().ExecuteAsync(async state =>
                {
                    using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: state.file))
                    {
                        await RunDatabaseCreationTest(testDatabase, state.async);
                    }
                }, new { async, file });
        }

        private static async Task RunDatabaseCreationTest(SqlServerTestStore testStore, bool async)
        {
            using (var context = new SqlServerDatabaseCreatorTestCommon.BloggingContext(testStore))
            {
                var creator = context.GetService<IRelationalDatabaseCreator>();

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (async)
                {
                    Assert.True(await creator.EnsureCreatedAsync());
                }
                else
                {
                    Assert.True(creator.EnsureCreated());
                }

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (testStore.Connection.State != ConnectionState.Open)
                {
                    await testStore.Connection.OpenAsync();
                }

                var tables = await testStore.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
                Assert.Equal(1, tables.Count());
                Assert.Equal("Blogs", tables.Single());
            }
        }

        [Fact]
        public async Task EnsuredCreated_is_noop_when_database_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredCreated_is_noop_when_database_with_filename_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: false, file: true);
        }

        [Fact]
        public async Task EnsuredCreatedAsync_is_noop_when_database_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredCreatedAsync_is_noop_when_database_with_filename_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: true, file: true);
        }

        private static async Task EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new SqlServerDatabaseCreatorTestCommon.BloggingContext(testDatabase))
                {
                    context.Database.EnsureCreated();

                    if (async)
                    {
                        Assert.False(await context.Database.EnsureCreatedAsync());
                    }
                    else
                    {
                        Assert.False(context.Database.EnsureCreated());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }
    }
}
