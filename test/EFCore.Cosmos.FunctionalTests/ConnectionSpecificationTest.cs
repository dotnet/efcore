// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
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
            try
            {
                using var context = new BloggingContext(testDatabase);
                var creator = context.GetService<IDatabaseCreator>();

                Assert.True(creator.EnsureCreated());
            }
            finally
            {
                testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
            }
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

        public class Blog
        {
            public int Id { get; set; }
        }
    }
}
