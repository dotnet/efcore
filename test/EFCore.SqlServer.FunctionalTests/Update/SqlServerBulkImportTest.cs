// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data.Common;
using Microsoft.Data.SqlClient;
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

    [Fact]
    public async Task Benchmark_BulkImport_vs_SaveChanges()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, TestStore.Name);
        await context.Database.EnsureCreatedResilientlyAsync();

        // Warmup
        var warmupProducts = Enumerable.Range(1, 100).Select(i => new Product
        {
            Name = $"Warmup {i}",
            Price = i,
            Category = ProductCategory.Books
        }).ToList();
        await context.Products.ImportFromAsync(warmupProducts);

        foreach (var count in new[] { 1000, 10000 })
        {
            var productsSave = Enumerable.Range(1, count).Select(i => new Product
            {
                Name = $"SaveProduct {i}",
                Price = i,
                Category = ProductCategory.Electronics
            }).ToList();

            var productsBulk = Enumerable.Range(1, count).Select(i => new Product
            {
                Name = $"BulkProduct {i}",
                Price = i,
                Category = ProductCategory.Electronics
            }).ToList();

            // SaveChanges
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var alloc1 = GC.GetAllocatedBytesForCurrentThread();
            await context.Products.AddRangeAsync(productsSave);
            await context.SaveChangesAsync();
            sw1.Stop();
            var memorySave = GC.GetAllocatedBytesForCurrentThread() - alloc1;

            // ImportFromAsync (SqlBulkCopy)
            var sw2 = System.Diagnostics.Stopwatch.StartNew();
            var alloc2 = GC.GetAllocatedBytesForCurrentThread();
            await context.Products.ImportFromAsync(productsBulk);
            sw2.Stop();
            var memoryBulk = GC.GetAllocatedBytesForCurrentThread() - alloc2;

            var speedup = sw1.Elapsed.TotalMilliseconds / Math.Max(1.0, sw2.Elapsed.TotalMilliseconds);
            Console.WriteLine($"[BENCHMARK] Rows: {count} | SaveChanges: {sw1.ElapsedMilliseconds} ms ({memorySave / 1024 / 1024} MB) | SqlBulkCopy: {sw2.ElapsedMilliseconds} ms ({memoryBulk / 1024 / 1024} MB) | Speedup: {speedup:F1}x");
        }
    }

    [Fact]
    public async Task Benchmark_Overhead_vs_Direct_SqlBulkCopy()
    {
        var sp = CreateServiceProvider();
        await using var context = new BulkImportContext(sp, TestStore.Name);
        await context.Database.EnsureCreatedResilientlyAsync();

        var connString = SqlServerTestStore.CreateConnectionString(TestStore.Name);

        // Warmup
        var warmup = Enumerable.Range(1, 100).Select(i => new Product { Name = $"Warm {i}", Price = 10m, Category = ProductCategory.Books }).ToList();
        await ExecuteRawSqlBulkCopyAsync(connString, warmup);
        await context.Products.ImportFromAsync(warmup);

        foreach (var count in new[] { 10000, 50000 })
        {
            var rawItems = Enumerable.Range(1, count).Select(i => new Product
            {
                Name = $"RawProduct {i}",
                Price = i * 2.5m,
                Category = ProductCategory.Electronics,
                Description = i % 2 == 0 ? $"Desc {i}" : null
            }).ToList();

            var efItems = Enumerable.Range(1, count).Select(i => new Product
            {
                Name = $"EFProduct {i}",
                Price = i * 2.5m,
                Category = ProductCategory.Electronics,
                Description = i % 2 == 0 ? $"Desc {i}" : null
            }).ToList();

            // Direct ADO.NET SqlBulkCopy
            var swRaw = System.Diagnostics.Stopwatch.StartNew();
            var allocRaw = GC.GetAllocatedBytesForCurrentThread();
            await ExecuteRawSqlBulkCopyAsync(connString, rawItems);
            swRaw.Stop();
            var memRaw = GC.GetAllocatedBytesForCurrentThread() - allocRaw;

            // EF Core DbSet.ImportFromAsync
            var swEf = System.Diagnostics.Stopwatch.StartNew();
            var allocEf = GC.GetAllocatedBytesForCurrentThread();
            await context.Products.ImportFromAsync(efItems);
            swEf.Stop();
            var memEf = GC.GetAllocatedBytesForCurrentThread() - allocEf;

            var overheadMs = swEf.ElapsedMilliseconds - swRaw.ElapsedMilliseconds;
            var overheadPercent = (double)(swEf.ElapsedMilliseconds - swRaw.ElapsedMilliseconds) / Math.Max(1, swRaw.ElapsedMilliseconds) * 100.0;

            Console.WriteLine($"[OVERHEAD BENCHMARK] Rows: {count} | Raw ADO.NET: {swRaw.ElapsedMilliseconds} ms ({memRaw / 1024} KB) | EF Core ImportFromAsync: {swEf.ElapsedMilliseconds} ms ({memEf / 1024} KB) | Overhead: {overheadMs:+0;-0} ms ({overheadPercent:+0.0;-0.0}%)");
        }
    }

    private static async Task ExecuteRawSqlBulkCopyAsync(
        string connectionString,
        IReadOnlyList<Product> products,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = "[Products]";
        bulkCopy.ColumnMappings.Add("Name", "Name");
        bulkCopy.ColumnMappings.Add("Price", "Price");
        bulkCopy.ColumnMappings.Add("Category", "Category");
        bulkCopy.ColumnMappings.Add("Description", "Description");

        using var reader = new RawProductDataReader(products);
        await bulkCopy.WriteToServerAsync(reader, cancellationToken);
    }

    private sealed class RawProductDataReader(IReadOnlyList<Product> products) : DbDataReader
    {
        private int _index = -1;
        public override int FieldCount => 4;
        public override bool HasRows => products.Count > 0;
        public override bool IsClosed => false;
        public override int RecordsAffected => -1;
        public override int Depth => 0;

        public override bool Read() => ++_index < products.Count;
        public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read());

        public override string GetName(int ordinal) => ordinal switch
        {
            0 => "Name",
            1 => "Price",
            2 => "Category",
            3 => "Description",
            _ => throw new IndexOutOfRangeException()
        };

        public override int GetOrdinal(string name) => name switch
        {
            "Name" => 0,
            "Price" => 1,
            "Category" => 2,
            "Description" => 3,
            _ => throw new IndexOutOfRangeException()
        };

        public override Type GetFieldType(int ordinal) => ordinal switch
        {
            0 => typeof(string),
            1 => typeof(decimal),
            2 => typeof(string),
            3 => typeof(string),
            _ => throw new IndexOutOfRangeException()
        };

        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

        public override object GetValue(int ordinal)
        {
            var p = products[_index];
            return ordinal switch
            {
                0 => p.Name,
                1 => p.Price,
                2 => p.Category.ToString(),
                3 => (object?)p.Description ?? DBNull.Value,
                _ => throw new IndexOutOfRangeException()
            };
        }

        public override int GetValues(object[] values)
        {
            for (var i = 0; i < 4; i++) values[i] = GetValue(i);
            return 4;
        }

        public override bool IsDBNull(int ordinal) => ordinal == 3 && products[_index].Description is null;
        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => Task.FromResult(IsDBNull(ordinal));
        public override bool NextResult() => false;
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);
        public override void Close() { }
        public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);
        public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);
        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
        public override char GetChar(int ordinal) => (char)GetValue(ordinal);
        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
        public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
        public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);
        public override double GetDouble(int ordinal) => (double)GetValue(ordinal);
        public override float GetFloat(int ordinal) => (float)GetValue(ordinal);
        public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
        public override short GetInt16(int ordinal) => (short)GetValue(ordinal);
        public override int GetInt32(int ordinal) => (int)GetValue(ordinal);
        public override long GetInt64(int ordinal) => (long)GetValue(ordinal);
        public override string GetString(int ordinal) => (string)GetValue(ordinal);
        public override IEnumerator GetEnumerator() => new DbEnumerator(this, closeReader: false);
        public override object this[int ordinal] => GetValue(ordinal);
        public override object this[string name] => GetValue(GetOrdinal(name));
    }

    protected SqlServerTestStore TestStore { get; private set; } = null!;

    public async ValueTask InitializeAsync()
        => TestStore = await SqlServerTestStore.CreateInitializedAsync("SqlServerBulkImportTest");

    public async ValueTask DisposeAsync()
        => await TestStore.DisposeAsync();
}
