// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MethodHasAsyncOverload

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class NonSharedModelUpdatesTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "NonSharedModelUpdatesTestBase";

    [ConditionalTheory, MemberData(nameof(IsAsyncData))] // Issue #29356
    public virtual async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Author>(b => b.HasOne(a => a.AuthorsClub)
                        .WithMany()
                        .HasForeignKey(a => a.AuthorsClubId));

                mb.Entity<Book>(b => b.HasOne(book => book.Author)
                        .WithMany()
                        .HasForeignKey(book => book.AuthorId));
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new Book { Author = new Author { Name = "Alice", AuthorsClub = new AuthorsClub { Name = "AC South" } } });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                AuthorsClub authorsClubNorth = new() { Name = "AC North" };
                Author authorOfTheYear2023 = new() { Name = "Author of the year 2023", AuthorsClub = authorsClubNorth };
                context.Add(authorsClubNorth);
                context.Add(authorOfTheYear2023);

                var book = await context
                    .Set<Book>()
                    .Include(b => b.Author)
                    .SingleAsync();
                var authorOfTheYear2022 = book.Author!;
                book.Author = authorOfTheYear2023;

                context.Remove(authorOfTheYear2022);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            });
    }

    private class AuthorsClub
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class Author
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int AuthorsClubId { get; set; }
        public AuthorsClub? AuthorsClub { get; set; }
    }

    private class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))] // Issue #29379
    public virtual async Task DbUpdateException_Entries_is_correct_with_multiple_inserts(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(onModelCreating: mb => mb.Entity<Blog>().HasIndex(b => b.Name).IsUnique());

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new Blog { Name = "Blog2" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                context.Add(new Blog { Name = "Blog1" });
                context.Add(new Blog { Name = "Blog2" });
                context.Add(new Blog { Name = "Blog3" });

                var exception = async
                    ? await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())
                    : Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                var entry = Assert.Single(exception.Entries);

                Assert.Equal("Blog2", ((Blog)entry.Entity).Name);
            });
    }

    public class Blog
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_entity_with_not_loaded_property_excludes_column_from_SQL(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithDescription>(
                    b => b.Property(e => e.Description).Metadata.IsAutoLoaded = false),
            seed: async context =>
            {
                context.Add(new BlogWithDescription { Name = "EF Blog", Description = "Original description" });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = new BlogWithDescription { Id = 1, Name = "Updated Blog" };
                context.Update(blog);

                var entry = context.Entry(blog);
                // Description starts as not-loaded (IsAutoLoaded = false)
                Assert.False(entry.Property(e => e.Description).IsLoaded);
                Assert.False(entry.Property(e => e.Description).IsModified);
                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var blog = await context.Set<BlogWithDescription>().SingleAsync();
                Assert.Equal("Updated Blog", blog.Name);
                Assert.Equal("Original description", blog.Description);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Save_and_query_with_partially_loaded_primitive_collection(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithTags>(
                    b =>
                    {
                        b.Property(e => e.Tags).Metadata.IsAutoLoaded = false;
                        b.Property(e => e.Tags).Metadata.Sentinel = new List<string>();
                    }),
            seed: async context =>
            {
                context.Add(new BlogWithTags { Name = "EF Blog", Tags = ["efcore", "dotnet"] });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = new BlogWithTags { Id = 1, Name = "Updated Blog" };
                context.Update(blog);

                var entry = context.Entry(blog);
                Assert.False(entry.Property(e => e.Tags).IsLoaded);
                Assert.False(entry.Property(e => e.Tags).IsModified);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var blog = await context.Set<BlogWithTags>().SingleAsync();
                Assert.Equal("Updated Blog", blog.Name);
                Assert.Equal(new[] { "efcore", "dotnet" }, blog.Tags);
            });
    }

    private class BlogWithDescription
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private class BlogWithTags
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))] // Issue #36059
    public virtual async Task Replacing_owned_entity_with_FK_to_another_entity(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Document36059>(b => b.OwnsOne(d => d.File, fb =>
                    {
                        fb.Property(f => f.Id).ValueGeneratedNever();
                        fb.HasOne(f => f.Content)
                            .WithMany()
                            .HasForeignKey(f => f.ContentId)
                            .IsRequired()
                            .OnDelete(DeleteBehavior.Restrict);
                    }));

                mb.Entity<Content36059>(b => b.Property(c => c.Id).ValueGeneratedNever());
            });

        var oldContentId = Guid.NewGuid();
        var newContentId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var oldFileId = Guid.NewGuid();
        var newFileId = Guid.NewGuid();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new Document36059
                {
                    Id = documentId,
                    File = new File36059
                    {
                        Id = oldFileId,
                        Name = "old.jpg",
                        ContentId = oldContentId,
                        Content = new Content36059 { Id = oldContentId, Data = "initial" }
                    }
                });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var document = await context.Set<Document36059>()
                    .Include(d => d.File)
                    .SingleAsync(d => d.Id == documentId);

                document.File = new File36059
                {
                    Id = newFileId,
                    Name = "new.png",
                    ContentId = newContentId,
                    Content = new Content36059 { Id = newContentId, Data = "updated" }
                };

                context.Set<Content36059>().Remove(new Content36059 { Id = oldContentId });

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var document = await context.Set<Document36059>()
                    .Include(d => d.File)
                    .SingleAsync(d => d.Id == documentId);

                Assert.NotNull(document.File);
                Assert.Equal(newFileId, document.File.Id);
                Assert.Equal("new.png", document.File.Name);
                Assert.Equal(newContentId, document.File.ContentId);

                Assert.Null(await context.Set<Content36059>().SingleOrDefaultAsync(c => c.Id == oldContentId));
                Assert.NotNull(await context.Set<Content36059>().SingleOrDefaultAsync(c => c.Id == newContentId));
            });
    }

    private class Document36059
    {
        public Guid Id { get; set; }
        public File36059? File { get; set; }
    }

    private class File36059
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid ContentId { get; set; }
        public required Content36059 Content { get; set; }
    }

    private class Content36059
    {
        public Guid Id { get; set; }
        public string? Data { get; set; }
    }

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        ContextFactory<DbContext> contextFactory,
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task>? nestedTestOperation1 = null,
        Func<DbContext, Task>? nestedTestOperation2 = null,
        Func<DbContext, Task>? nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateDbContext, UseTransaction, testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
