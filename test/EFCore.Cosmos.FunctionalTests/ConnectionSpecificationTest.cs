// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ConnectionSpecificationTest
    {
        [ConditionalFact]
        public async Task Can_specify_connection_string_in_OnConfiguring()
        {
            await using var testDatabase = CosmosTestStore.Create("NonExisting");
            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.False(await creator.EnsureDeletedAsync());
        }

        public class BloggingContext : DbContext
        {
            private readonly string _connectionString;
            private readonly string _name;

            public BloggingContext(CosmosTestStore testStore)
            {
                _connectionString = testStore.ConnectionString;
                _name = testStore.Name;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseCosmos(_connectionString, _name, b => b.ApplyConfiguration());
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [ConditionalFact]
        public async Task Specifying_connection_string_and_account_endpoint_throws()
        {
            await using var testDatabase = CosmosTestStore.Create("NonExisting");

            using var context = new BloggingContextWithConnectionConflict(testDatabase);

            Assert.Equal(
                CosmosStrings.ConnectionStringConflictingConfiguration,
                Assert.Throws<InvalidOperationException>(() => context.GetService<IDatabaseCreator>()).Message);
        }

        public class BloggingContextWithConnectionConflict : DbContext
        {
            private readonly string _connectionString;
            private readonly string _connectionUri;
            private readonly string _authToken;
            private readonly string _name;

            public BloggingContextWithConnectionConflict(CosmosTestStore testStore)
            {
                _connectionString = testStore.ConnectionString;
                _connectionUri = testStore.ConnectionUri;
                _authToken = testStore.AuthToken;
                _name = testStore.Name;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseCosmos(_connectionString, _name, b => b.ApplyConfiguration())
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
