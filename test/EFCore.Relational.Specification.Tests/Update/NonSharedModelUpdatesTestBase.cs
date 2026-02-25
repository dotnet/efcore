// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MethodHasAsyncOverload

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class NonSharedModelUpdatesTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "NonSharedModelUpdatesTestBase";

    [ConditionalTheory, MemberData(nameof(IsAsyncData))] // Issue #29356
    public virtual async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Author>(b =>
                {
                    b.HasOne(a => a.AuthorsClub)
                        .WithMany()
                        .HasForeignKey(a => a.AuthorsClubId);
                });

                mb.Entity<Book>(b =>
                {
                    b.HasOne(book => book.Author)
                        .WithMany()
                        .HasForeignKey(book => book.AuthorId);
                });
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
        var contextFactory = await InitializeAsync<DbContext>(onModelCreating: mb => mb.Entity<Blog>().HasIndex(b => b.Name).IsUnique());

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

    [ConditionalFact] // Issue #37335
    public virtual async Task SaveChanges_with_nullable_complex_property_on_shared_column_in_TPH()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Product37335>()
                    .HasDiscriminator<string>("Discriminator")
                    .HasValue<Product37335A>("A")
                    .HasValue<Product37335B>("B");
                mb.Entity<Product37335A>()
                    .ComplexProperty(x => x.Price, p => p.Property(a => a.Amount).HasColumnName("Price"));
                mb.Entity<Product37335B>()
                    .ComplexProperty(x => x.Price, p => p.Property(a => a.Amount).HasColumnName("Price"));
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new Product37335A { Name = "Product 1" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var product = await context.Set<Product37335A>().FirstAsync();
                Assert.Null(product.Price);
            });
    }

    private abstract class Product37335
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    private class Product37335A : Product37335
    {
        public Price37335? Price { get; set; }
    }

    private class Product37335B : Product37335
    {
        public Price37335? Price { get; set; }
    }

    private sealed class Price37335
    {
        public required string Amount { get; init; }
    }

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        ContextFactory<DbContext> contextFactory,
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task>? nestedTestOperation1 = null,
        Func<DbContext, Task>? nestedTestOperation2 = null,
        Func<DbContext, Task>? nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateContext, UseTransaction, testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
