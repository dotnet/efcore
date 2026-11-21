// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Sqlite.Update;

public class SqliteBulkImportTest : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteBulkImportTest()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose()
        => _connection.Dispose();

    private class BulkImportContext(IServiceProvider serviceProvider, SqliteConnection connection) : DbContext
    {
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemWithComputed> ComputedItems { get; set; } = null!;
        public DbSet<GuidItem> GuidItems { get; set; } = null!;
        public DbSet<OnlyIdentityItem> OnlyIdentityItems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlite(connection)
                .UseInternalServiceProvider(serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>(b =>
            {
                b.ToTable("Items");
                b.Property(i => i.Id).ValueGeneratedOnAdd();
                b.Property(i => i.Name).IsRequired();
                b.Property(i => i.Status).HasConversion<string>();
            });

            modelBuilder.Entity<ItemWithComputed>(b =>
            {
                b.ToTable("ComputedItems");
                b.Property(i => i.Id).ValueGeneratedOnAdd();
                b.Property(i => i.Name).IsRequired();
                b.Property(i => i.Computed).HasComputedColumnSql("Name || '!'");
            });

            modelBuilder.Entity<GuidItem>(b =>
            {
                b.ToTable("GuidItems");
                b.Property(i => i.Id).ValueGeneratedNever();
                b.Property(i => i.Label).IsRequired();
            });

            modelBuilder.Entity<OnlyIdentityItem>(b =>
            {
                b.ToTable("OnlyIdentityItems");
                b.Property(i => i.Id).ValueGeneratedOnAdd();
            });
        }
    }

    private class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ItemStatus Status { get; set; }
    }

    private class ItemWithComputed
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Computed { get; set; }
    }

    private class GuidItem
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = null!;
    }

    private class OnlyIdentityItem
    {
        public int Id { get; set; }
    }

    private enum ItemStatus
    {
        Draft,
        Published
    }

    private IServiceProvider CreateServiceProvider()
        => new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .BuildServiceProvider(validateScopes: true);

    [Fact]
    public async Task Can_bulk_import_entities_via_relational_fallback()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, _connection);
        await context.Database.EnsureCreatedAsync();

        var items = Enumerable.Range(1, 250).Select(i => new Item
        {
            Name = $"Item {i}",
            Status = i % 2 == 0 ? ItemStatus.Published : ItemStatus.Draft
        }).ToList();

        await context.Items.ImportFromAsync(items);

        var count = await context.Items.CountAsync();
        Assert.Equal(250, count);

        var first = await context.Items.OrderBy(i => i.Id).FirstAsync();
        Assert.Equal("Item 1", first.Name);
        Assert.Equal(ItemStatus.Draft, first.Status);
    }

    [Fact]
    public async Task Can_bulk_import_with_computed_columns()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, _connection);
        await context.Database.EnsureCreatedAsync();

        var items = Enumerable.Range(1, 10).Select(i => new ItemWithComputed
        {
            Name = $"ComputedItem {i}"
        }).ToList();

        await context.ComputedItems.ImportFromAsync(items);

        var count = await context.ComputedItems.CountAsync();
        Assert.Equal(10, count);

        var first = await context.ComputedItems.OrderBy(i => i.Id).FirstAsync();
        Assert.Equal("ComputedItem 1", first.Name);
        Assert.Equal("ComputedItem 1!", first.Computed);
    }

    [Fact]
    public async Task Can_bulk_import_with_client_generated_guid_keys()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, _connection);
        await context.Database.EnsureCreatedAsync();

        var guids = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();
        var items = guids.Select(g => new GuidItem { Id = g, Label = $"Label {g}" }).ToList();

        await context.GuidItems.ImportFromAsync(items);

        var count = await context.GuidItems.CountAsync();
        Assert.Equal(5, count);

        var loadedGuids = await context.GuidItems.Select(x => x.Id).ToListAsync();
        Assert.Equal(guids.OrderBy(g => g), loadedGuids.OrderBy(g => g));
    }

    [Fact]
    public async Task Can_bulk_import_with_no_writable_columns()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, _connection);
        await context.Database.EnsureCreatedAsync();

        var items = Enumerable.Range(1, 7).Select(_ => new OnlyIdentityItem()).ToList();

        await context.OnlyIdentityItems.ImportFromAsync(items);

        var count = await context.OnlyIdentityItems.CountAsync();
        Assert.Equal(7, count);
    }
}
