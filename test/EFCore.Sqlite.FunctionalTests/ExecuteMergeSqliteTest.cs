// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

#pragma warning disable EF9107 // ExecuteMerge is experimental

public class ExecuteMergeSqliteTest
{
    [Fact]
    public void ExecuteMerge_do_nothing_inserts_new_and_ignores_conflict()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "original" });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var affected = context.Blogs.ExecuteMerge(
            [new Blog { Id = 1, Name = "should-be-ignored" }, new Blog { Id = 2, Name = "inserted" }],
            merge => { });

        Assert.Equal(1, affected);
        var blogs = context.Blogs.OrderBy(b => b.Id).ToList();
        Assert.Equal(2, blogs.Count);
        Assert.Equal("original", blogs[0].Name);
        Assert.Equal("inserted", blogs[1].Name);
    }

    [Fact]
    public void ExecuteMerge_do_nothing_match_on_unique_column()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "unique-name" });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var affected = context.Blogs.ExecuteMerge(
            [new Blog { Id = 2, Name = "unique-name" }, new Blog { Id = 3, Name = "fresh" }],
            merge => merge.Match(t => t.Name, s => s.Name));

        Assert.Equal(1, affected);
        var blogs = context.Blogs.OrderBy(b => b.Id).ToList();
        Assert.Equal(2, blogs.Count);
        Assert.Equal("unique-name", blogs[0].Name);
        Assert.Equal("fresh", blogs[1].Name);
    }

    [Fact]
    public void ExecuteMerge_when_matched_updates_and_inserts()
    {
        using var context = CreateContext();

        context.Visits.Add(new DailyVisit { UserId = 1, Day = "mon", Visits = 5 });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var affected = context.Visits.ExecuteMerge(
            [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
            merge => merge
                .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => t.Visits + s.Visits)));

        Assert.Equal(2, affected);
        var visits = context.Visits.OrderBy(v => v.UserId).ToList();
        Assert.Equal(2, visits.Count);
        Assert.Equal(8, visits[0].Visits); // 5 + 3
        Assert.Equal(7, visits[1].Visits); // inserted
    }

    [Fact]
    public async Task ExecuteMergeAsync_when_matched_updates_and_inserts()
    {
        using var context = CreateContext();

        context.Visits.Add(new DailyVisit { UserId = 1, Day = "mon", Visits = 5 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var affected = await context.Visits.ExecuteMergeAsync(
            [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
            merge => merge
                .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => t.Visits + s.Visits)));

        Assert.Equal(2, affected);
        var visits = await context.Visits.OrderBy(v => v.UserId).ToListAsync();
        Assert.Equal(8, visits[0].Visits);
        Assert.Equal(7, visits[1].Visits);
    }

    [Fact]
    public void ExecuteMerge_when_not_matched_uses_explicit_insert_setters()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "kept" });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var affected = context.Blogs.ExecuteMerge(
            [new Blog { Id = 1, Name = "ignored" }, new Blog { Id = 9, Name = "explicit" }],
            merge => merge.WhenNotMatched(insert => insert
                .SetProperty(t => t.Id, s => s.Id)
                .SetProperty(t => t.Name, s => "CONST-" + s.Name)));

        Assert.Equal(1, affected);
        var blogs = context.Blogs.OrderBy(b => b.Id).ToList();
        Assert.Equal("kept", blogs[0].Name);
        Assert.Equal("CONST-explicit", blogs[1].Name);
    }

    [Fact]
    public void ExecuteMergeReturning_projects_affected_rows()
    {
        using var context = CreateContext();

        context.Visits.Add(new DailyVisit { UserId = 1, Day = "mon", Visits = 5 });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var results = context.Visits.ExecuteMergeReturning(
                [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
                merge => merge
                    .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                    .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => t.Visits + s.Visits)),
                t => new { t.UserId, t.Visits })
            .OrderBy(r => r.UserId)
            .ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal(8, results[0].Visits); // updated 5 + 3
        Assert.Equal(7, results[1].Visits); // inserted
    }

    [Fact]
    public async Task ExecuteMergeReturningAsync_streams_projected_scalars()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "before" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var ids = new List<int>();
        await foreach (var id in context.Blogs.ExecuteMergeReturningAsync(
                           [new Blog { Id = 1, Name = "after" }, new Blog { Id = 2, Name = "new" }],
                           merge => merge.WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name)),
                           t => t.Id))
        {
            ids.Add(id);
        }

        Assert.Equal([1, 2], ids.OrderBy(i => i));
        Assert.Equal("after", context.Blogs.Single(b => b.Id == 1).Name);
    }

    [Fact]
    public void ExecuteMerge_match_uses_source_key_selector()
    {
        using var context = CreateContext();

        context.Accounts.Add(new Account { Id = 1, ExternalId = 0, Balance = 100 });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // Match target.Id against the differently-named source member ExternalId. The conflict/insert value for the Id column must be
        // taken from source.ExternalId, otherwise the first row wouldn't match the existing Id == 1 row.
        var affected = context.Accounts.ExecuteMerge(
            [new Account { Id = 999, ExternalId = 1, Balance = 50 }, new Account { Id = 888, ExternalId = 2, Balance = 70 }],
            merge => merge
                .Match(t => t.Id, s => s.ExternalId)
                .WhenMatched(u => u.SetProperty(t => t.Balance, (t, s) => t.Balance + s.Balance)));

        Assert.Equal(2, affected);
        var accounts = context.Accounts.OrderBy(a => a.Id).ToList();
        Assert.Equal(2, accounts.Count);
        Assert.Equal(1, accounts[0].Id);
        Assert.Equal(150, accounts[0].Balance); // matched via ExternalId == 1: 100 + 50
        Assert.Equal(2, accounts[1].Id); // inserted with Id taken from ExternalId
        Assert.Equal(70, accounts[1].Balance);
    }

    [Fact]
    public void ExecuteMerge_default_insert_skips_computed_columns()
    {
        using var context = CreateContext();

        context.Gadgets.Add(new Gadget { Id = 1, Price = 100 });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // No WhenNotMatched => default insert of all mapped columns. The computed Tax column must be excluded, otherwise SQLite rejects
        // the INSERT into a generated column.
        var affected = context.Gadgets.ExecuteMerge(
            [new Gadget { Id = 1, Price = 200 }, new Gadget { Id = 2, Price = 500 }],
            merge => merge.WhenMatched(u => u.SetProperty(t => t.Price, (t, s) => s.Price)));

        Assert.Equal(2, affected);
        var gadgets = context.Gadgets.OrderBy(g => g.Id).ToList();
        Assert.Equal(2, gadgets.Count);
        Assert.Equal(200, gadgets[0].Price);
        Assert.Equal(20, gadgets[0].Tax); // computed 200 / 10
        Assert.Equal(500, gadgets[1].Price);
        Assert.Equal(50, gadgets[1].Tax); // computed 500 / 10
    }

    [Fact]
    public void ExecuteMerge_when_matched_and_when_not_matched_together()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "old" });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var affected = context.Blogs.ExecuteMerge(
            [new Blog { Id = 1, Name = "updated" }, new Blog { Id = 2, Name = "new" }],
            merge => merge
                .WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name))
                .WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Name, s => "INS-" + s.Name)));

        Assert.Equal(2, affected);
        var blogs = context.Blogs.OrderBy(b => b.Id).ToList();
        // The WhenMatched source row is SQLite's "excluded" pseudo-row, i.e. the values that would have been inserted (after the
        // WhenNotMatched setters run). So s.Name for the matched row is "INS-updated", not the raw source "updated".
        Assert.Equal("INS-updated", blogs[0].Name);
        Assert.Equal("INS-new", blogs[1].Name);
    }

    [Fact]
    public void ExecuteMerge_with_multiple_match_calls()
    {
        using var context = CreateContext();

        context.Visits.Add(new DailyVisit { UserId = 1, Day = "mon", Visits = 5 });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // Two separate Match() calls should combine into a composite conflict target, equivalent to a single anonymous-type match.
        var affected = context.Visits.ExecuteMerge(
            [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
            merge => merge
                .Match(t => t.UserId, s => s.UserId)
                .Match(t => t.Day, s => s.Day)
                .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => t.Visits + s.Visits)));

        Assert.Equal(2, affected);
        var visits = context.Visits.OrderBy(v => v.UserId).ToList();
        Assert.Equal(8, visits[0].Visits);
        Assert.Equal(7, visits[1].Visits);
    }

    [Fact]
    public void ExecuteMerge_when_matched_with_constant_value()
    {
        using var context = CreateContext();

        context.Visits.Add(new DailyVisit { UserId = 1, Day = "mon", Visits = 5 });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // Exercises the value-type constant SetProperty overload (boxed to object then unwrapped during translation).
        var affected = context.Visits.ExecuteMerge(
            [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }],
            merge => merge
                .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                .WhenMatched(u => u.SetProperty(t => t.Visits, 0)));

        Assert.Equal(1, affected);
        Assert.Equal(0, context.Visits.Single().Visits);
    }

    [Fact]
    public void ExecuteMerge_when_not_matched_with_constant_value()
    {
        using var context = CreateContext();

        var affected = context.Blogs.ExecuteMerge(
            [new Blog { Id = 5, Name = "ignored" }],
            merge => merge.WhenNotMatched(insert => insert
                .SetProperty(t => t.Id, s => s.Id)
                .SetProperty(t => t.Name, "CONST")));

        Assert.Equal(1, affected);
        Assert.Equal("CONST", context.Blogs.Single().Name);
    }

    [Fact]
    public void ExecuteMergeReturning_with_do_nothing_returns_only_inserted_rows()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "existing" });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // ON CONFLICT DO NOTHING RETURNING yields only the rows actually inserted; the conflicting row is skipped.
        var ids = context.Blogs.ExecuteMergeReturning(
                [new Blog { Id = 1, Name = "existing" }, new Blog { Id = 2, Name = "fresh" }],
                merge => { },
                t => t.Id)
            .OrderBy(i => i)
            .ToList();

        Assert.Equal([2], ids);
    }

    [Fact]
    public void ExecuteMergeReturning_reads_null_column_as_default()
    {
        using var context = CreateContext();

        var notes = context.Blogs.ExecuteMergeReturning(
                [new Blog { Id = 7, Name = "no-note" }],
                merge => { },
                t => t.Note)
            .ToList();

        Assert.Single(notes);
        Assert.Null(notes[0]);
    }

    [Fact]
    public void ExecuteMerge_with_empty_source()
    {
        using var context = CreateContext();

        context.Blogs.Add(new Blog { Id = 1, Name = "kept" });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var affected = context.Blogs.ExecuteMerge(new Blog[] { }, merge => { });

        Assert.Equal(0, affected);
        Assert.Equal("kept", context.Blogs.Single().Name);
    }

    [Fact]
    public void ExecuteMerge_on_filtered_target_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Blogs.Where(b => b.Id > 0).ExecuteMerge(
                [new Blog { Id = 1, Name = "x" }],
                merge => { }));

        Assert.Equal(RelationalStrings.ExecuteMergeOnComplexQuery, exception.InnerException!.Message);
    }

    [Fact]
    public void ExecuteMerge_without_primary_key_and_no_match_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Set<Keyless>().ExecuteMerge(
                [new Keyless { Value = 1 }],
                merge => { }));

        Assert.Equal(
            RelationalStrings.ExecuteMergeNoPrimaryKey(nameof(Keyless)), exception.InnerException!.Message);
    }

    [Fact]
    public void ExecuteMerge_when_matched_unsupported_expression_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Blogs.ExecuteMerge(
                [new Blog { Id = 1, Name = "x" }],
                merge => merge.WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => t.Name.ToUpper()))));

        Assert.StartsWith("Unsupported expression", exception.InnerException!.Message);
    }

    [Fact]
    public void ExecuteMerge_property_not_found_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Blogs.ExecuteMerge(
                [new Blog { Id = 1, Name = "x" }],
                merge => merge.WhenMatched(u => u.SetProperty(t => EF.Property<int>(t, "Nonexistent"), (t, s) => 0))));

        Assert.Equal(
            RelationalStrings.ExecuteMergePropertyNotFound("Nonexistent", nameof(Blog)), exception.InnerException!.Message);
    }

    private static MergeContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var context = new MergeContext(connection);
        context.Database.EnsureCreated();
        return context;
    }

    private class MergeContext(SqliteConnection connection) : DbContext
    {
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<DailyVisit> Visits => Set<DailyVisit>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Gadget> Gadgets => Set<Gadget>();
        public DbSet<Keyless> Keyless => Set<Keyless>();

        public override void Dispose()
        {
            connection.Dispose();
            base.Dispose();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(connection);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.HasIndex(e => e.Name).IsUnique();
                });
            modelBuilder.Entity<DailyVisit>(b => b.HasKey(e => new { e.UserId, e.Day }));
            modelBuilder.Entity<Account>(b => b.Property(e => e.Id).ValueGeneratedNever());
            modelBuilder.Entity<Gadget>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Tax).HasComputedColumnSql(@"""Price"" / 10");
                });
            modelBuilder.Entity<Keyless>(b => b.HasNoKey().ToTable("Keyless"));
        }
    }

    private class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Note { get; set; }
    }

    private class Keyless
    {
        public int Value { get; set; }
    }

    private class DailyVisit
    {
        public int UserId { get; set; }
        public string Day { get; set; } = null!;
        public int Visits { get; set; }
    }

    private class Account
    {
        public int Id { get; set; }
        public int ExternalId { get; set; }
        public int Balance { get; set; }
    }

    private class Gadget
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public int Tax { get; set; }
    }
}
