// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class CosmosDatabaseCreatorTest
    {
        public static IEnumerable<object[]> IsAsyncData = new[]
        {
            new object[] { true },
            //new object[] { false }
        };

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureCreated_returns_true_when_database_does_not_exist(bool async)
        {
            await using (var testDatabase = CosmosTestStore.Create("NonExisting"))
            {
                try
                {
                    using (var context = new BloggingContext(testDatabase))
                    {
                        var creator = context.GetService<IDatabaseCreator>();

                        Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                    }
                }
                finally
                {
                    testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureCreated_returns_true_when_database_exists_but_collections_do_not(bool async)
        {
            await using (var testDatabase = CosmosTestStore.Create("EnsureCreatedTest"))
            {
                try
                {
                    using (var context = new BloggingContext(testDatabase))
                    {
                        var creator = context.GetService<IDatabaseCreator>();

                        Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                    }
                }
                finally
                {
                    testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureCreated_returns_false_when_database_and_collections_exist(bool async)
        {
            await using (var testDatabase = CosmosTestStore.Create("EnsureCreatedReady"))
            {
                testDatabase.Initialize(testDatabase.ServiceProvider, testStore => new BloggingContext((CosmosTestStore)testStore));

                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.False(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureDeleted_returns_true_when_database_exists(bool async)
        {
            await using (var testDatabase = CosmosTestStore.CreateInitialized("EnsureDeleteBlogging"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureDeleted_returns_false_when_database_does_not_exist(bool async)
        {
            await using (var testDatabase = CosmosTestStore.Create("EnsureDeleteBlogging"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.False(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
                }
            }
        }

        public class BloggingContext : DbContext
        {
            private readonly string _connectionUri;
            private readonly string _authToken;
            private readonly string _name;

            public BloggingContext(CosmosTestStore testStore)
            {
                _connectionUri = testStore.ConnectionUri;
                _authToken = testStore.AuthToken;
                _name = testStore.Name;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseCosmos(
                        _connectionUri,
                        _authToken,
                        _name,
                        b => b.ApplyConfiguration());
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public int Id { get; set; }
        }
    }
}
