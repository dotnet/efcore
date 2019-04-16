// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class CosmosDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true)]
        //[InlineData(false)]
        public async Task EnsureCreated_returns_true_when_database_does_not_exist(bool async)
        {
            using (var testDatabase = CosmosTestStore.Create("NonExisting"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        //[InlineData(false)]
        public async Task EnsureCreated_returns_true_when_database_exists_but_collections_does_not(bool async)
        {
            using (var testDatabase = CosmosTestStore.CreateInitialized("EnsureCreatedTest"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        //[InlineData(false)]
        public async Task EnsureCreated_returns_false_when_database_and_collections_exists(bool async)
        {
            using (var testDatabase = CosmosTestStore.Create("EnsureCreatedReady"))
            {
                testDatabase.Initialize(null, () => new BloggingContext(testDatabase), null);

                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.False(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        //[InlineData(false)]
        public async Task EnsureDeleted_returns_true_when_database_exists(bool async)
        {
            using (var testDatabase = CosmosTestStore.CreateInitialized("EnsureDeleteBlogging"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        //[InlineData(false)]
        public async Task EnsureDeleted_returns_false_when_database_does_not_exist(bool async)
        {
            using (var testDatabase = CosmosTestStore.Create("EnsureDeleteBlogging"))
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
                        _name);
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
