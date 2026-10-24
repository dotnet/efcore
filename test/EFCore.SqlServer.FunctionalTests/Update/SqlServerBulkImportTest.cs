// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update;

public class SqlServerBulkImportTest : IAsyncLifetime
{
    private class BulkImportContext(IServiceProvider serviceProvider, string databaseName) : DbContext
    {
        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(SqlServerTestStore.CreateConnectionString(databaseName), b => b.ApplyConfiguration())
                .UseInternalServiceProvider(serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.Property(p => p.Id).UseIdentityColumn();
                b.Property(p => p.Name).HasMaxLength(100).IsRequired();
                b.Property(p => p.Price).HasPrecision(18, 2);
                b.Property(p => p.Category).HasConversion<string>();
                b.Property(p => p.Description).HasMaxLength(500);
            });
        }
    }

    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public ProductCategory Category { get; set; }
        public string? Description { get; set; }
    }

    private enum ProductCategory
    {
        Electronics,
        Books,
        Clothing
    }

    private IServiceProvider CreateServiceProvider()
        => new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

    [Fact]
    public async Task Can_bulk_import_entities_basic()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, TestStore.Name);
        await context.Database.EnsureCreatedResilientlyAsync();

        var products = Enumerable.Range(1, 500).Select(i => new Product
        {
            Name = $"Product {i}",
            Price = i * 1.5m,
            Category = i % 2 == 0 ? ProductCategory.Electronics : ProductCategory.Books,
            Description = i % 3 == 0 ? null : $"Description {i}"
        }).ToList();

        await context.Products.ImportFromAsync(products);

        var count = await context.Products.CountAsync();
        Assert.Equal(500, count);

        var first = await context.Products.OrderBy(p => p.Id).FirstAsync();
        Assert.Equal("Product 1", first.Name);
        Assert.Equal(1.5m, first.Price);
        Assert.Equal(ProductCategory.Books, first.Category);
    }

    [Fact]
    public async Task Can_bulk_import_with_explicit_transaction_rollback()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, TestStore.Name);
        await context.Database.EnsureCreatedResilientlyAsync();

        var initialCount = await context.Products.CountAsync();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync();

            var products = Enumerable.Range(1, 50).Select(i => new Product
            {
                Name = $"TxProduct {i}",
                Price = 10m,
                Category = ProductCategory.Clothing
            }).ToList();

            await context.Products.ImportFromAsync(products);

            // Should see 50 items inside the transaction
            var inTxCount = await context.Products.CountAsync();
            Assert.Equal(initialCount + 50, inTxCount);

            await tx.RollbackAsync();
        });

        // After rollback, the items should not exist
        var afterRollbackCount = await context.Products.CountAsync();
        Assert.Equal(initialCount, afterRollbackCount);
    }

    [Fact]
    public async Task Can_bulk_import_with_keep_identity()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, TestStore.Name);
        await context.Database.EnsureCreatedResilientlyAsync();

        var explicitProducts = new List<Product>
        {
            new() { Id = 9001, Name = "Explicit 9001", Price = 99m, Category = ProductCategory.Books },
            new() { Id = 9002, Name = "Explicit 9002", Price = 199m, Category = ProductCategory.Electronics }
        };

        var options = new BulkImportOptions { KeepIdentity = true };
        await context.Products.ImportFromAsync(explicitProducts, options);

        var imported = await context.Products.Where(p => p.Id >= 9001).OrderBy(p => p.Id).ToListAsync();
        Assert.Equal(2, imported.Count);
        Assert.Equal(9001, imported[0].Id);
        Assert.Equal(9002, imported[1].Id);
    }

    [Fact]
    public async Task Can_bulk_import_async_stream()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, TestStore.Name);
        await context.Database.EnsureCreatedResilientlyAsync();

        async IAsyncEnumerable<Product> StreamProducts()
        {
            for (var i = 1; i <= 20; i++)
            {
                await Task.Yield();
                yield return new Product
                {
                    Name = $"StreamProduct {i}",
                    Price = 5m * i,
                    Category = ProductCategory.Clothing
                };
            }
        }

        await context.Products.ImportFromAsync(StreamProducts());

        var anyStreamed = await context.Products.AnyAsync(p => p.Name == "StreamProduct 20");
        Assert.True(anyStreamed);
    }

    protected SqlServerTestStore TestStore { get; private set; } = null!;

    public async ValueTask InitializeAsync()
        => TestStore = await SqlServerTestStore.CreateInitializedAsync("SqlServerBulkImportTest");

    public async ValueTask DisposeAsync()
        => await TestStore.DisposeAsync();
}
