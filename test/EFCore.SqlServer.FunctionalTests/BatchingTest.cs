// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

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
    public Task Inserts_are_batched_correctly(bool clientPk, bool clientFk, bool clientOrder)
    {
        var expectedBlogs = new List<Blog>();
        return ExecuteWithStrategyInTransactionAsync(
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

                return context.SaveChangesAsync();
            },
            context => AssertDatabaseState(context, clientOrder, expectedBlogs));
    }

    [ConditionalFact]
    public Task Inserts_and_updates_are_batched_correctly()
    {
        var expectedBlogs = new List<Blog>();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
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

                await context.SaveChangesAsync();

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

                await context.SaveChangesAsync();
            },
            context => AssertDatabaseState(context, true, expectedBlogs));
    }

    [ConditionalTheory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(100)]
    public Task Insertion_order_is_preserved(int maxBatchSize)
    {
        var blogId = new Guid();

        return TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => (BloggingContext)Fixture.CreateContext(maxBatchSize: maxBatchSize),
            UseTransaction, async context =>
            {
                var owner = new Owner();
                var blog = new Blog { Owner = owner };

                for (var i = 0; i < 20; i++)
                {
                    context.Add(new Post { Order = i, Blog = blog });
                }

                await context.SaveChangesAsync();

                blogId = blog.Id;
            }, async context =>
            {
                var posts = context.Set<Post>().Where(p => p.BlogId == blogId).OrderBy(p => p.Order);
                var lastId = 0;
                foreach (var post in await posts.ToListAsync())
                {
                    Assert.True(post.PostId > lastId, $"Last ID: {lastId}, current ID: {post.PostId}");
                    lastId = post.PostId;
                }
            });
    }

    [ConditionalFact]
    public async Task Deadlock_on_inserts_and_deletes_with_dependents_is_handled_correctly()
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

            await context.SaveChangesAsync();
        }

        var tasks = new List<Task>();
        for (var i = 0; i < 10; i++)
        {
            foreach (var blog in blogs)
            {
                tasks.Add(RemoveAndAddPosts(blog));
            }
        }

        Task.WaitAll(tasks.ToArray());

        async Task RemoveAndAddPosts(Blog blog)
        {
            using var context = (BloggingContext)Fixture.CreateContext(useConnectionString: true);

            context.Attach(blog);
            blog.Posts.Clear();

            blog.Posts.Add(new Post { Comments = { new Comment() } });
            blog.Posts.Add(new Post { Comments = { new Comment() } });
            blog.Posts.Add(new Post { Comments = { new Comment() } });

            await context.SaveChangesAsync();
        }

        await Fixture.ReseedAsync();
    }

    [ConditionalFact]
    public async Task Deadlock_on_deletes_with_dependents_is_handled_correctly()
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

            await context.SaveChangesAsync();
        }

        async Task Action(Owner owner)
        {
            using var context = (BloggingContext)Fixture.CreateContext(useConnectionString: true);

            context.RemoveRange(await context.Blogs.Where(b => b.OwnerId == owner.Id).ToListAsync());

            await context.SaveChangesAsync();
        }

        var tasks = new List<Task>();
        foreach (var owner in owners)
        {
            tasks.Add(Action(owner));
        }

        Task.WaitAll(tasks.ToArray());

        using (var context = CreateContext())
        {
            Assert.Empty(await context.Blogs.ToListAsync());
        }

        await Fixture.ReseedAsync();
    }

    [ConditionalFact]
    public Task Inserts_when_database_type_is_different()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var owner1 = new Owner { Id = "0", Name = "Zero" };
                var owner2 = new Owner { Id = "A", Name = string.Join("", Enumerable.Repeat('A', 900)) };
                context.Owners.Add(owner1);
                context.Owners.Add(owner2);

                return context.SaveChangesAsync();
            }, async context => Assert.Equal(2, await context.Owners.CountAsync()));

    [ConditionalTheory]
    [InlineData(3)]
    [InlineData(4)]
    public Task Inserts_are_batched_only_when_necessary(int minBatchSize)
    {
        var expectedBlogs = new List<Blog>();
        return TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => (BloggingContext)Fixture.CreateContext(minBatchSize),
            UseTransaction, async context =>
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

                await context.SaveChangesAsync();

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

    private async Task AssertDatabaseState(DbContext context, bool clientOrder, List<Blog> expectedBlogs)
    {
        expectedBlogs = clientOrder
            ? expectedBlogs.OrderBy(b => b.Order).ToList()
            : expectedBlogs.OrderBy(b => b.Id).ToList();
        var actualBlogs = clientOrder
            ? await context.Set<Blog>().OrderBy(b => b.Order).ToListAsync()
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

    private Task ExecuteWithStrategyInTransactionAsync(
        Func<BloggingContext, Task> testOperation,
        Func<BloggingContext, Task> nestedTestOperation)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction, testOperation, nestedTestOperation);

    protected void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private class BloggingContext(DbContextOptions options) : PoolableDbContext(options)
    {
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
        protected override string StoreName
            => "BatchingTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override Type ContextType { get; } = typeof(BloggingContext);

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Update.Name;

        protected override async Task SeedAsync(PoolableDbContext context)
        {
            await context.Database.EnsureCreatedResilientlyAsync();
            await context.Database.ExecuteSqlRawAsync(
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
