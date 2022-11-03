// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MethodHasAsyncOverload
namespace Microsoft.EntityFrameworkCore.Update;

#nullable enable

public abstract class NonSharedModelUpdatesTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "NonSharedModelUpdatesTestBase";

    [ConditionalTheory] // Issue #29356
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Author>(
                    b =>
                    {
                        b.HasOne(a => a.AuthorsClub)
                            .WithMany()
                            .HasForeignKey(a => a.AuthorsClubId);
                    });

                mb.Entity<Book>(
                    b =>
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
                    new Book
                    {
                        Author = new Author
                        {
                            Name = "Alice",
                            AuthorsClub = new AuthorsClub
                            {
                                Name = "AC South"
                            }
                        }
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                AuthorsClub authorsClubNorth = new()
                {
                    Name = "AC North"
                };
                Author authorOfTheYear2023 = new()
                {
                    Name = "Author of the year 2023",
                    AuthorsClub = authorsClubNorth
                };
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

    protected virtual void ExecuteWithStrategyInTransaction(
        ContextFactory<DbContext> contextFactory,
        Action<DbContext> testOperation,
        Action<DbContext>? nestedTestOperation1 = null,
        Action<DbContext>? nestedTestOperation2 = null,
        Action<DbContext>? nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransaction(
            contextFactory.CreateContext, UseTransaction, testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

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
