// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class ForeignKeyTest
    {
        private readonly SqliteTestStore _testStore;

        public ForeignKeyTest()
        {
            _testStore = SqliteTestStore.CreateScratch();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void It_enforces_foreign_key(bool suppress)
        {
            var builder = new DbContextOptionsBuilder();
            var sqliteBuilder = builder.UseSqlite(_testStore.Connection.ConnectionString);
            if (suppress)
            {
                sqliteBuilder.SuppressForeignKeyEnforcement();
            }

            var options = builder.Options;

            using (var context = new MyContext(options))
            {
                context.Database.EnsureCreated();
                context.Add(new Child { ParentId = 4 });
                if (suppress)
                {
                    context.SaveChanges();
                }
                else
                {
                    var ex = Assert.Throws<DbUpdateException>(() => { context.SaveChanges(); });
                    Assert.Contains("FOREIGN KEY constraint failed", ex.InnerException.Message);
                }
            }
        }
    }

    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .Collection(b => b.Children)
                .InverseReference(b => b.MyParent)
                .ForeignKey(b => b.ParentId);
        }
    }

    public class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public Parent MyParent { get; set; }
    }

    public class Parent
    {
        public int Id { get; set; }
        public ICollection<Child> Children { get; set; }
    }
}
