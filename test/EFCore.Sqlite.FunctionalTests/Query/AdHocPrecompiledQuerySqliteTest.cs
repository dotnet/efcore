// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(fixture, testOutputHelper)
{
    [Fact]
    public async Task Converted_JSON_scalars_use_native_precompiled_projections()
    {
        var factory = await InitializeNonSharedTest<JsonScalarReaderContext>(
            seed: async context =>
            {
                context.Add(
                    new JsonScalarReaderContext.Item
                    {
                        Id = 1,
                        Data = new JsonScalarReaderContext.JsonData
                        {
                            Bytes = [1, 2, 255],
                            Number = 43,
                            ConvertedNumber = 43,
                            Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        }
                    });
                await context.SaveChangesAsync();
            });

        await Test(
            """
await using var context = new AdHocPrecompiledQuerySqliteTest.JsonScalarReaderContext(dbContextOptions);
var result = await context.Items
    .Select(e => new { e.Data.Bytes, e.Data.Number, e.Data.ConvertedNumber, e.Data.Timestamp }).SingleAsync();
Assert.Equal(new byte[] { 1, 2, 255 }, result.Bytes);
Assert.Equal(43, result.Number);
Assert.Equal(43, result.ConvertedNumber);
Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), result.Timestamp);
""",
            typeof(JsonScalarReaderContext),
            factory.GetOptions());
    }

    public class JsonScalarReaderContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Item> Items
            => Set<Item>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Item>().OwnsOne(
                e => e.Data, owned =>
                {
                    owned.ToJson();
                    owned.Property(e => e.Number).HasConversion(v => v * 10, v => v / 10);
                    owned.Property(e => e.ConvertedNumber)
                        .HasConversion(new ValueConverter<int, int>(v => (v + 1) * 10, v => v / 10 - 1));
                    owned.Property(e => e.Timestamp).HasConversion(
                        v => new DateTimeOffset(v).ToUnixTimeMilliseconds(),
                        v => DateTimeOffset.FromUnixTimeMilliseconds(v).UtcDateTime);
                });
        }

        public class Item
        {
            public int Id { get; set; }
            public JsonData Data { get; set; } = null!;
        }

        public class JsonData
        {
            public byte[] Bytes { get; set; } = null!;
            public int Number { get; set; }
            public int ConvertedNumber { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }

    protected override bool AlwaysPrintGeneratedSources
        => false;

    protected override DbContextOptionsBuilder UseWindowFunctionAggregates(DbContextOptionsBuilder optionsBuilder)
    {
#pragma warning disable EF9001 // UseWindowFunctionAggregates is experimental
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseWindowFunctionAggregates();
#pragma warning restore EF9001
        return optionsBuilder;
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlitePrecompiledQueryTestHelpers.Instance;
}
