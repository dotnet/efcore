// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class BatchingTest
    {
        private static readonly string DatabaseName = "BatchingTest";

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Inserts_are_batched_correctly(bool clientPk, bool clientFk, bool clientOrder)
        {
            using (var testStore = SqlServerTestStore.Create(DatabaseName))
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlServer()
                            .BuildServiceProvider())
                    .Options;

                var expectedBlogs = new List<Blog>();
                using (var context = new BloggingContext(options))
                {
                    context.Database.EnsureClean();

                    var owner1 = new Owner();
                    var owner2 = new Owner();
                    context.Owners.Add(owner1);
                    context.Owners.Add(owner2);

                    for (var i = 1; i < 500; i++)
                    {
                        var blog = new Blog();
                        if (clientPk)
                        {
                            blog.Id = Guid.NewGuid();
                        }

                        if (clientFk)
                        {
                            blog.Owner = i % 2 == 0 ? owner1 : owner2;
                        }

                        if (clientOrder)
                        {
                            blog.Order = i;
                        }

                        context.Blogs.Add(blog);
                        expectedBlogs.Add(blog);
                    }

                    context.SaveChanges();
                }

                AssertDatabaseState(clientOrder, expectedBlogs, options);
            }
        }

        [Fact]
        public void Inserts_and_updates_are_batched_correctly()
        {
            using (var testStore = SqlServerTestStore.Create(DatabaseName))
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlServer()
                            .BuildServiceProvider())
                    .Options;
                var expectedBlogs = new List<Blog>();
                using (var context = new BloggingContext(options))
                {
                    context.Database.EnsureClean();

                    var owner1 = new Owner { Name = "0" };
                    var owner2 = new Owner { Name = "1" };
                    context.Owners.Add(owner1);
                    context.Owners.Add(owner2);

                    var blog1 = new Blog
                    {
                        Id = Guid.NewGuid(),
                        Owner = owner1,
                        Order = 1
                    };

                    context.Blogs.Add(blog1);
                    expectedBlogs.Add(blog1);

                    context.SaveChanges();

                    owner2.Name = "2";

                    blog1.Order = 0;
                    var blog2 = new Blog
                    {
                        Id = Guid.NewGuid(),
                        Owner = owner1,
                        Order = 1
                    };

                    context.Blogs.Add(blog2);
                    expectedBlogs.Add(blog2);

                    var blog3 = new Blog
                    {
                        Id = Guid.NewGuid(),
                        Owner = owner2,
                        Order = 2
                    };

                    context.Blogs.Add(blog3);
                    expectedBlogs.Add(blog3);

                    context.SaveChanges();
                }

                AssertDatabaseState(true, expectedBlogs, options);
            }
        }

        private void AssertDatabaseState(bool clientOrder, List<Blog> expectedBlogs, DbContextOptions options)
        {
            expectedBlogs = clientOrder
                ? expectedBlogs.OrderBy(b => b.Order).ToList()
                : expectedBlogs.OrderBy(b => b.Id).ToList();
            using (var context = new BloggingContext(options))
            {
                var actualBlogs = clientOrder
                    ? context.Blogs.OrderBy(b => b.Order).ToList()
                    : expectedBlogs.OrderBy(b => b.Id).ToList();
                Assert.Equal(expectedBlogs.Count, actualBlogs.Count);

                for (var i = 0; i < actualBlogs.Count; i++)
                {
                    var expected = expectedBlogs[i];
                    var actual = actualBlogs[i];
                    Assert.Equal(expected.Id, actual.Id);
                    Assert.Equal(expected.Order, actual.Order);
                    Assert.Equal(expected.OwnerId, actual.OwnerId);
                    Assert.Equal(expected.Version, actual.Version);
                }
            }
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Owner>().Property(e => e.Version).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                modelBuilder.Entity<Blog>(b =>
                    {
                        b.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                        b.Property(e => e.Version).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                    });
            }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Owner> Owners { get; set; }
        }

        private class Blog
        {
            public Guid Id { get; set; }
            public int Order { get; set; }
            public int? OwnerId { get; set; }
            public Owner Owner { get; set; }
            public byte[] Version { get; set; }
        }

        public class Owner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte[] Version { get; set; }
        }
    }
}
