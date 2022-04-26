// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class BatchingTest : IClassFixture<BatchingTest.BatchingTestFixture>
{
    public BatchingTest(BatchingTestFixture fixture)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
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

    [ConditionalTheory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(100)]
    public void Insertion_order_is_preserved(int maxBatchSize)
    {
        var blogId = new Guid();

        TestHelpers.ExecuteWithStrategyInTransaction(
            () => (BloggingContext)Fixture.CreateContext(maxBatchSize: maxBatchSize),
            UseTransaction,
            context =>
            {
                var owner = new Owner();
                var blog = new Blog { Owner = owner };

                for (var i = 0; i < 20; i++)
                {
                    context.Add(new Post { Order = i, Blog = blog });
                }

                context.SaveChanges();

                blogId = blog.Id;
            },
            context =>
            {
                var posts = context.Set<Post>().Where(p => p.BlogId == blogId).OrderBy(p => p.Order);
                var lastId = 0;
                foreach (var post in posts)
                {
                    Assert.True(post.PostId > lastId, $"Last ID: {lastId}, current ID: {post.PostId}");
                    lastId = post.PostId;
                }
            });
    }

    [ConditionalFact]
    public void Deadlock_on_inserts_and_deletes_with_dependents_is_handled_correctly()
    {
        var blogs = new List<Blog>();

        using (var context = CreateContext())
        {
            var owner1 = new Owner { Name = "0" };
            var owner2 = new Owner { Name = "1" };
            context.Owners.Add(owner1);
            context.Owners.Add(owner2);

            blogs.Add(
                new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner1,
                    Order = 1
                });
            blogs.Add(
                new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner2,
                    Order = 2
                });
            blogs.Add(
                new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner1,
                    Order = 3
                });
            blogs.Add(
                new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner2,
                    Order = 4
                });

            context.AddRange(blogs);

            context.SaveChanges();
        }

        for (var i = 0; i < 10; i++)
        {
            Parallel.ForEach(
                blogs, blog =>
                {
                    RemoveAndAddPosts(blog);
                });
        }

        void RemoveAndAddPosts(Blog blog)
        {
            using var context = (BloggingContext)Fixture.CreateContext(useConnectionString: true);

            context.Attach(blog);
            blog.Posts.Clear();

            blog.Posts.Add(new Post { Comments = { new Comment() } });
            blog.Posts.Add(new Post { Comments = { new Comment() } });
            blog.Posts.Add(new Post { Comments = { new Comment() } });

            context.SaveChanges();
        }

        Fixture.Reseed();
    }

    [ConditionalFact]
    public void Deadlock_on_deletes_with_dependents_is_handled_correctly()
    {
        var owners = new[] { new Owner { Name = "0" }, new Owner { Name = "1" } };
        using (var context = CreateContext())
        {
            context.Owners.AddRange(owners);

            for (var h = 0; h <= 40; h++)
            {
                var owner = owners[h % 2];
                var blog = new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner,
                    Order = h
                };

                for (var i = 0; i <= 40; i++)
                {
                    blog.Posts.Add(new Post { Comments = { new Comment() } });
                }

                context.Add(blog);
            }

            context.SaveChanges();
        }

        Parallel.ForEach(
            owners, owner =>
            {
                using var context = (BloggingContext)Fixture.CreateContext(useConnectionString: true);

                context.RemoveRange(context.Blogs.Where(b => b.OwnerId == owner.Id));

                context.SaveChanges();
            });

        using (var context = CreateContext())
        {
            Assert.Empty(context.Blogs);
        }

        Fixture.Reseed();
    }

    [ConditionalFact]
    public void Inserts_when_database_type_is_different()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var owner1 = new Owner { Id = "0", Name = "Zero" };
                var owner2 = new Owner { Id = "A", Name = string.Join("", Enumerable.Repeat('A', 900)) };
                context.Owners.Add(owner1);
                context.Owners.Add(owner2);

                context.SaveChanges();
            },
            context => Assert.Equal(2, context.Owners.Count()));

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

                for (var i = 1; i < 3; i++)
                {
                    var blog = new Blog { Id = Guid.NewGuid(), Owner = owner };

                    context.Set<Blog>().Add(blog);
                    expectedBlogs.Add(blog);
                }

                Fixture.TestSqlLoggerFactory.Clear();

                context.SaveChanges();

                Assert.Contains(
                    minBatchSize == 3
                        ? RelationalResources.LogBatchReadyForExecution(new TestLogger<SqlServerLoggingDefinitions>())
                            .GenerateMessage(3)
                        : RelationalResources.LogBatchSmallerThanMinBatchSize(new TestLogger<SqlServerLoggingDefinitions>())
                            .GenerateMessage(3, 4),
                    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

                Assert.Equal(minBatchSize <= 3 ? 1 : 3, Fixture.TestSqlLoggerFactory.SqlStatements.Count);
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

    private BloggingContext CreateContext()
        => (BloggingContext)Fixture.CreateContext();

    private void ExecuteWithStrategyInTransaction(
        Action<BloggingContext> testOperation,
        Action<BloggingContext> nestedTestOperation)
        => TestHelpers.ExecuteWithStrategyInTransaction(
            CreateContext, UseTransaction, testOperation, nestedTestOperation);

    protected void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

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
        public ICollection<Post> Posts { get; } = new HashSet<Post>();
    }

    private class Owner
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public byte[] Version { get; set; }
    }

    private class Post
    {
        public int PostId { get; set; }
        public int? Order { get; set; }
        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }
        public ICollection<Comment> Comments { get; } = new HashSet<Comment>();
    }

    private class Comment
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }

    public class BatchingTestFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName { get; } = "BatchingTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

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

        public DbContext CreateContext(
            int? minBatchSize = null,
            int? maxBatchSize = null,
            bool useConnectionString = false,
            bool disableConnectionResiliency = false)
        {
            var options = CreateOptions();
            var optionsBuilder = new DbContextOptionsBuilder(options);
            if (useConnectionString)
            {
                RelationalOptionsExtension extension = options.FindExtension<SqlServerOptionsExtension>()
                    ?? new SqlServerOptionsExtension();

                extension = extension.WithConnection(null).WithConnectionString(((SqlServerTestStore)TestStore).ConnectionString);
                ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            }

            if (minBatchSize.HasValue)
            {
                new SqlServerDbContextOptionsBuilder(optionsBuilder).MinBatchSize(minBatchSize.Value);
            }

            if (maxBatchSize.HasValue)
            {
                new SqlServerDbContextOptionsBuilder(optionsBuilder).MinBatchSize(maxBatchSize.Value);
            }

            if (disableConnectionResiliency)
            {
                new SqlServerDbContextOptionsBuilder(optionsBuilder).ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
            }

            return new BloggingContext(optionsBuilder.Options);
        }
    }
}
