// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ToSqlQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "ToSqlQueryTests";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))] // Issue #27629
    public virtual async Task Entity_type_with_navigation_mapped_to_SqlQuery(bool async)
    {
        var contextFactory = await InitializeAsync<Context27629>(
            seed: async c =>
            {
                var author = new Author { Name = "Toast", Posts = { new Post { Title = "Sausages of the world!" } } };
                c.Add(author);
                await c.SaveChangesAsync();

                var postStat = new PostStat { Count = 10, Author = author };
                author.PostStat = postStat;
                c.Add(postStat);
                await c.SaveChangesAsync();
            });

        using var context = contextFactory.CreateContext();

        var authors = await
            (from o in context.Authors
             select new { Author = o, PostCount = o.PostStat!.Count }).ToListAsync();

        Assert.Single(authors);
        Assert.Equal("Toast", authors[0].Author.Name);
        Assert.Equal(10, authors[0].PostCount);
    }

    protected class Context27629(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Author> Authors
            => Set<Author>();

        public DbSet<Post> Posts
            => Set<Post>();

        public DbSet<PostStat> PostStats
            => Set<PostStat>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(
                builder =>
                {
                    builder.ToTable("Authors");
                    builder.Property(o => o.Name).HasMaxLength(50);
                });

            modelBuilder.Entity<Post>(
                builder =>
                {
                    builder.ToTable("Posts");
                    builder.Property(o => o.Title).HasMaxLength(50);
                    builder.Property(o => o.Content).HasMaxLength(500);

                    builder
                        .HasOne(o => o.Author)
                        .WithMany(o => o.Posts)
                        .HasForeignKey(o => o.AuthorId)
                        .OnDelete(DeleteBehavior.ClientCascade);
                });

            modelBuilder.Entity<PostStat>(
                builder =>
                {
                    builder
                        .ToSqlQuery("SELECT * FROM PostStats")
                        .HasKey(o => o.AuthorId);

                    builder
                        .HasOne(o => o.Author)
                        .WithOne().HasForeignKey<PostStat>(o => o.AuthorId)
                        .OnDelete(DeleteBehavior.ClientCascade);
                });
        }
    }

    protected class Author
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public List<Post> Posts { get; } = [];
        public PostStat? PostStat { get; set; }
    }

    protected class Post
    {
        public long Id { get; set; }
        public long AuthorId { get; set; }
        public Author Author { get; set; } = null!;
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    protected class PostStat
    {
        public long AuthorId { get; set; }
        public Author Author { get; set; } = null!;
        public long? Count { get; set; }
    }

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();
}
