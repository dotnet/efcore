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
        }
    }

    private class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ItemStatus Status { get; set; }
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
}
