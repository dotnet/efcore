// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class SqlServerJsonContainsTest
{
    [Theory]
    [InlineData(false, 170)]
    [InlineData(true, 170)]
    [InlineData(false, 160)]
    [InlineData(true, 160)]
    public void Contains_uses_openjson_for_unsupported_search_types(bool azure, int compatibilityLevel)
    {
        var options = new DbContextOptionsBuilder<ContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(compatibilityLevel));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(compatibilityLevel));
        }

        using var context = new ContainsContext(options.Options);
        var id = new Guid("12345678-1234-1234-1234-123456789abc");
        var date = new DateTime(2026, 1, 2);

        AssertOpenJson(context.Entities.Where(e => e.Guids.Contains(id)).ToQueryString(), "uniqueidentifier");
        AssertOpenJson(context.Entities.Where(e => e.Guids.Contains(e.GuidValue)).ToQueryString(), "uniqueidentifier");
        AssertOpenJson(context.Entities.Where(e => e.Dates.Contains(date)).ToQueryString(), "datetime2");
        AssertOpenJson(context.Entities.Where(e => e.DateOffsets.Contains(e.DateOffset)).ToQueryString(), "datetimeoffset");
        AssertOpenJson(context.Entities.Where(e => e.Times.Contains(e.Time)).ToQueryString(), "time");
        AssertOpenJson(context.Entities.Where(e => e.Owned.Guids.Contains(id)).ToQueryString(), "uniqueidentifier");
        AssertOpenJson(context.Entities.Where(e => e.Owned.Dates.Any(d => d == date)).ToQueryString(), "datetime2");
        AssertOpenJson(context.Entities.Where(e => e.ConvertedInts.Contains(42)).ToQueryString(), "nvarchar(64)");

        var intSql = context.Entities.Where(e => e.Ints.Contains(42)).ToQueryString();
        Assert.Contains(compatibilityLevel == 170 ? "JSON_CONTAINS" : "OPENJSON", intSql);
    }

    private static void AssertOpenJson(string sql, string storeType)
    {
        Assert.Contains("OPENJSON", sql);
        Assert.Contains($"[value] {storeType} '$'", sql);
        Assert.DoesNotContain("JSON_CONTAINS", sql);
    }

    private class ContainsContext(DbContextOptions<ContainsContext> options) : DbContext(options)
    {
        public DbSet<ContainsEntity> Entities => Set<ContainsEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContainsEntity>().OwnsOne(e => e.Owned, b => b.ToJson());
            modelBuilder.Entity<ContainsEntity>().PrimitiveCollection(e => e.ConvertedInts).ElementType().HasConversion<string>();
        }
    }

    private class ContainsEntity
    {
        public int Id { get; set; }
        public Guid[] Guids { get; set; } = null!;
        public Guid GuidValue { get; set; }
        public DateTime[] Dates { get; set; } = null!;
        public DateTimeOffset[] DateOffsets { get; set; } = null!;
        public DateTimeOffset DateOffset { get; set; }
        public TimeOnly[] Times { get; set; } = null!;
        public TimeOnly Time { get; set; }
        public int[] Ints { get; set; } = null!;
        public int[] ConvertedInts { get; set; } = null!;
        public ContainsOwned Owned { get; set; } = null!;
    }

    private class ContainsOwned
    {
        public Guid[] Guids { get; set; } = null!;
        public DateTime[] Dates { get; set; } = null!;
    }
}
