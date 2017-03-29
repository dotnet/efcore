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
    public class SqlServerDatabaseCreatorDeletionTest
    {
        [Fact]
        public async Task EnsureDeleted_will_delete_database()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeleted_will_delete_database_with_filename()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: false, file: true);
        }

        [Fact]
        public async Task EnsureDeletedAsync_will_delete_database()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeletedAsync_will_delete_database_with_filename()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: false, file: true);
        }

        [Fact]
        public async Task EnsureDeleted_will_delete_database_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeleted_will_delete_database_with_filename_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: true, file: true);
        }

        [Fact]
        public async Task EnsureDeletedAsync_will_delete_database_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeletedAsync_will_delete_database_with_filename_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: true, file: true);
        }

        private static async Task EnsureDeleted_will_delete_database_test(bool async, bool open, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true, useFileName: file))
            {
                if (!open)
                {
                    testDatabase.Connection.Close();
                }

                using (var context = new SqlServerDatabaseCreatorTestCommon.BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    if (async)
                    {
                        Assert.True(await context.Database.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.True(context.Database.EnsureDeleted());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task EnsuredDeleted_noop_when_database_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredDeleted_noop_when_database_with_filename_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: false, file: true);
        }

        [Fact]
        public async Task EnsuredDeletedAsync_noop_when_database_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredDeletedAsync_noop_when_database_with_filename_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: true, file: true);
        }

        private static async Task EnsuredDeleted_noop_when_database_doesnt_exist_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new SqlServerDatabaseCreatorTestCommon.BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    if (async)
                    {
                        Assert.False(await creator.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.False(creator.EnsureDeleted());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                testDatabase.Connection.Close();

                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Delete_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Delete_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
            {
                var creator = SqlServerDatabaseCreatorTestCommon.GetDatabaseCreator(testDatabase);

                if (async)
                {
                    await Assert.ThrowsAsync<SqlException>(() => creator.DeleteAsync());
                }
                else
                {
                    Assert.Throws<SqlException>(() => creator.Delete());
                }
            }
        }
    }
}
