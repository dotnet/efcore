// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleInsertBatchTest : IClassFixture<OracleFixture>
    {
        private const string DatabaseName = "OracleBatchInsertTest";

        protected OracleFixture Fixture { get; }

        public OracleInsertBatchTest(OracleFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact]
        public void Insert_batch_record()
        {
            using (var testDatabase = OracleTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                using (var context = new OracleBatchInsertContext(options))
                {
                    context.GetService<IRelationalDatabaseCreator>().CreateTables();

                    for (int i = 0; i < 5000; i++)
                    {
                        context.Add(new Movie
                        {
                            Description = $"The EntityFramework {i}",
                            Publication = DateTime.Now
                        });
                    }

                    var rows = context.SaveChanges();

                    Assert.Equal(1, context.Movies.Single(e => e.Description == "The EntityFramework 0").Id);
                    Assert.Equal(5000, context.Movies.Last().Id);
                    Assert.Equal(5000, rows);
                    Assert.NotEqual(4999, rows);
                }
            }
        }

        [Fact]
        public async void Insert_batch_record_async()
        {
            using (var testDatabase = OracleTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                using (var context = new OracleBatchInsertContext(options))
                {
                    await context.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();

                    for (int i = 0; i < 5000; i++)
                    {
                        context.Add(new Movie
                        {
                            Description = $"The EntityFramework {i}",
                            Publication = DateTime.Now
                        });
                    }

                    var rows = await context.SaveChangesAsync();

                    Assert.Equal(1, (await context.Movies.SingleAsync(e => e.Description == "The EntityFramework 0")).Id);
                    Assert.Equal(5000, (await context.Movies.LastAsync()).Id);
                    Assert.Equal(5000, rows);
                    Assert.NotEqual(4999, rows);
                }
            }
        }

        private class OracleBatchInsertContext : DbContext
        {
            public OracleBatchInsertContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Movie> Movies { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                   .Entity<Movie>().ToTable("Movies");
            }
        }

        private class Movie
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public DateTime Publication { get; set; }
        }
    }
}
