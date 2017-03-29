// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.DatabaseCreatorTest
{
    public class SqlServerDatabaseCreatorExistenceTest
    {
        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task Exists_returns_false_when_database_with_filename_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false, file: true);
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task ExistsAsync_returns_false_when_database_with_filename_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true, file: true);
        }

        private static async Task Exists_returns_false_when_database_doesnt_exist_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new SqlServerDatabaseCreatorTestCommon.BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task Exists_returns_true_when_database_with_filename_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false, file: true);
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task ExistsAsync_returns_true_when_database_with_filename_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true, file: true);
        }

        private static async Task Exists_returns_true_when_database_exists_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true, useFileName: file))
            {
                using (var context = new SqlServerDatabaseCreatorTestCommon.BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task HasTables_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(async: true);
        }

        private static async Task HasTables_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
            {
                await ((SqlServerDatabaseCreatorTestCommon.TestDatabaseCreator)SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase)).ExecutionStrategyFactory.Create()
                    .ExecuteAsync(async creator =>
                        {
                            var errorNumber = async
                                ? (await Assert.ThrowsAsync<SqlException>(() => creator.HasTablesAsyncBase())).Number
                                : Assert.Throws<SqlException>(() => creator.HasTablesBase()).Number;

                            if (errorNumber != 233) // skip if no-process transient failure
                            {
                                Assert.Equal(
                                    4060, // Login failed error number
                                    errorNumber);
                            }
                        }, (SqlServerDatabaseCreatorTestCommon.TestDatabaseCreator)SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase));
            }
        }

        [Fact]
        public async Task HasTables_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(async: true);
        }

        private static async Task HasTables_returns_false_when_database_exists_but_has_no_tables_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                Assert.False(async
                    ? await ((SqlServerDatabaseCreatorTestCommon.TestDatabaseCreator)creator).HasTablesAsyncBase()
                    : ((SqlServerDatabaseCreatorTestCommon.TestDatabaseCreator)creator).HasTablesBase());
            }
        }

        [Fact]
        public async Task HasTables_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(async: true);
        }

        private static async Task HasTables_returns_true_when_database_exists_and_has_any_tables_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE SomeTable (Id uniqueidentifier)");

                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                Assert.True(async ? await ((SqlServerDatabaseCreatorTestCommon.TestDatabaseCreator)creator).HasTablesAsyncBase() : ((SqlServerDatabaseCreatorTestCommon.TestDatabaseCreator)creator).HasTablesBase());
            }
        }
    }
}
