// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class AutoincrementTest : IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        [Fact]
        public void Autoincrement_prevents_reusing_rowid()
        {
            using (var context = CreateContext())
            {
                context.Database.EnsureClean();
                context.People.Add(new Person { Name = "Bruce" });
                context.SaveChanges();

                var hero = context.People.First(p => p.Id == 1);

                context.People.Remove(hero);
                context.SaveChanges();
                context.People.Add(new Person { Name = "Batman" });
                context.SaveChanges();
                var gone = context.People.FirstOrDefault(p => p.Id == 1);
                var begins = context.People.FirstOrDefault(p => p.Id == 2);

                Assert.Null(gone);
                Assert.NotNull(begins);
            }
        }

        [Fact]
        public void Identity_metadata_not_on_text_is_ignored()
        {
            using (var context = new JokerContext(_options))
            {
                context.Database.EnsureClean();
            }
        }

        public AutoincrementTest()
        {
            _testStore = SqliteTestStore.CreateScratch();

            var provider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(provider)
                .UseSqlite(_testStore.Connection)
                .Options;
        }

        private BatContext CreateContext() => new BatContext(_options);

        public void Dispose() => _testStore.Dispose();
    }

    public class JokerContext : DbContext
    {
        public JokerContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(b =>
                {
                    b.ForSqliteToTable("People2");
                    b.HasKey(t => t.Name);
                    b.Property(t => t.Name).ValueGeneratedOnAdd();
                });
        }
    }

    public class BatContext : DbContext
    {
        public BatContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
