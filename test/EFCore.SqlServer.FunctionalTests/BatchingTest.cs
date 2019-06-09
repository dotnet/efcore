// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class BatchingTest : IClassFixture<BatchingTest.BatchingTestFixture>
    {
        public BatchingTest(BatchingTestFixture fixture)
        {
            Fixture = fixture;
        }

        protected BatchingTestFixture Fixture { get; }

        [ConditionalTheory]
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
            var expectedBlogs = new List<Blog>();
            ExecuteWithStrategyInTransaction(
                context =>
                {
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

                        context.Set<Blog>().Add(blog);
                        expectedBlogs.Add(blog);
                    }

                    context.SaveChanges();
                },
                context => AssertDatabaseState(context, clientOrder, expectedBlogs));
        }

        [ConditionalFact]
        public void Inserts_and_updates_are_batched_correctly()
        {
            var expectedBlogs = new List<Blog>();

            ExecuteWithStrategyInTransaction(
                context =>
                {
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

                    context.Set<Blog>().Add(blog1);
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

                    context.Set<Blog>().Add(blog2);
                    expectedBlogs.Add(blog2);

                    var blog3 = new Blog
                    {
                        Id = Guid.NewGuid(),
                        Owner = owner2,
                        Order = 2
                    };

                    context.Set<Blog>().Add(blog3);
                    expectedBlogs.Add(blog3);

                    context.SaveChanges();
                },
                context => AssertDatabaseState(context, true, expectedBlogs));
        }

        [ConditionalFact]
        public void Inserts_when_database_type_is_different()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var owner1 = new Owner { Id = "0", Name = "Zero" };
                    var owner2 = new Owner { Id = "A", Name = string.Join("", Enumerable.Repeat('A', 900)) };
                    context.Owners.Add(owner1);
                    context.Owners.Add(owner2);

                    context.SaveChanges();
                },
                context => Assert.Equal(2, context.Owners.Count()));
        }

        [ConditionalTheory]
        [InlineData(3)]
        [InlineData(4)]
        public void Inserts_are_batched_only_when_necessary(int minBatchSize)
        {
            var expectedBlogs = new List<Blog>();
            TestHelpers.ExecuteWithStrategyInTransaction(
                () => (BloggingContext)Fixture.CreateContext(minBatchSize),
                UseTransaction,
                context =>
                {
                    var owner = new Owner();
                    context.Owners.Add(owner);

                    for (var i = 1; i < 4; i++)
                    {
                        var blog = new Blog
                        {
                            Id = Guid.NewGuid(),
                            Owner = owner
                        };

                        context.Set<Blog>().Add(blog);
                        expectedBlogs.Add(blog);
                    }

                    Fixture.TestSqlLoggerFactory.Clear();

                    context.SaveChanges();

                    Assert.Contains(
                        minBatchSize == 3
                            ? RelationalResources.LogBatchReadyForExecution(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage(3)
                            : RelationalResources.LogBatchSmallerThanMinBatchSize(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage(3, 4),
                        Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

                    Assert.Equal(minBatchSize <= 3 ? 2 : 4, Fixture.TestSqlLoggerFactory.SqlStatements.Count);
                }, context => AssertDatabaseState(context, false, expectedBlogs));
        }

        private void AssertDatabaseState(DbContext context, bool clientOrder, List<Blog> expectedBlogs)
        {
            expectedBlogs = clientOrder
                ? expectedBlogs.OrderBy(b => b.Order).ToList()
                : expectedBlogs.OrderBy(b => b.Id).ToList();
            var actualBlogs = clientOrder
                ? context.Set<Blog>().OrderBy(b => b.Order).ToList()
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

        private BloggingContext CreateContext() => (BloggingContext)Fixture.CreateContext();

        private void ExecuteWithStrategyInTransaction(
            Action<BloggingContext> testOperation,
            Action<BloggingContext> nestedTestOperation)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction, testOperation, nestedTestOperation);

        protected void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        private class BloggingContext : PoolableDbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Owner>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedOnAdd();
                        b.Property(e => e.Version).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                        b.Property(e => e.Name).HasColumnType("nvarchar(450)");
                    });
                modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                        b.Property(e => e.Version).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                    });
            }

            // ReSharper disable once UnusedMember.Local
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Owner> Owners { get; set; }
        }

        private class Blog
        {
            public Guid Id { get; set; }
            public int Order { get; set; }
            public string OwnerId { get; set; }
            public Owner Owner { get; set; }
            public byte[] Version { get; set; }
        }

        private class Owner
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public byte[] Version { get; set; }
        }

        public class BatchingTestFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "BatchingTest";
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            protected override Type ContextType { get; } = typeof(BloggingContext);

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Update.Name;

            protected override void Seed(PoolableDbContext context)
            {
                context.Database.EnsureCreatedResiliently();
                context.Database.ExecuteSqlRaw(
                    @"
ALTER TABLE dbo.Owners
    ALTER COLUMN Name nvarchar(MAX);");
            }

            public DbContext CreateContext(int minBatchSize)
            {
                var optionsBuilder = new DbContextOptionsBuilder(CreateOptions());
                new SqlServerDbContextOptionsBuilder(optionsBuilder).MinBatchSize(minBatchSize);
                return new BloggingContext(optionsBuilder.Options);
            }
        }
    }
}
