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

    [Theory]
    [InlineData(false, 170)]
    [InlineData(true, 170)]
    [InlineData(false, 160)]
    [InlineData(true, 160)]
    public void Contains_over_primitive_collection_in_json_collection(bool azure, int compatibilityLevel)
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(compatibilityLevel));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(compatibilityLevel));
        }

        using var context = new NestedContainsContext(options.Options);
        var sql = context.Entities
            .Where(e => e.Details.Tags.Any(t => t.Names.Contains("a")))
            .Select(e => e.Id)
            .ToQueryString();

        Assert.Equal(
            compatibilityLevel < 170
                ? """
SELECT [e].[Id]
FROM [Entities] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([e].[Details], '$.Tags') WITH ([Names] nvarchar(max) '$.Names' AS JSON) AS [t]
    WHERE N'a' IN (
        SELECT [n].[value]
        FROM OPENJSON([t].[Names]) WITH ([value] nvarchar(max) '$') AS [n]
    ))
"""
                : azure
                    ? """
SELECT [e].[Id]
FROM [Entities] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([e].[Details], '$.Tags') WITH ([Names] nvarchar(max) '$.Names' AS JSON) AS [t]
    WHERE JSON_CONTAINS(CAST([t].[Names] AS json), N'a') = 1)
"""
                    : """
SELECT [e].[Id]
FROM [Entities] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([e].[Details], '$.Tags') WITH ([Names] json '$.Names' AS JSON) AS [t]
    WHERE JSON_CONTAINS([t].[Names], N'a') = 1)
""",
            sql,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Contains_over_primitive_collection_in_json_collection_concat()
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>()
            .UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));

        using var context = new NestedContainsContext(options.Options);
        var sql = context.Entities
            .Where(e => e.Details.Tags.Concat(e.Details.Tags).Any(t => t.Names.Contains("a")))
            .ToQueryString();

        Assert.Contains("UNION ALL", sql);
        Assert.Contains("JSON_CONTAINS(CAST([u].[Names] AS json), N'a')", sql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonContains_over_nested_document_with_path_and_search_mode(bool azure)
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(170));
        }

        using var context = new NestedContainsContext(options.Options);

#pragma warning disable EF9106 // JsonContains is experimental
        var withPath = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(t.Names, "a", "$") == 1))
            .ToQueryString();
        var withSearchMode = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(t.Names, "a", "$", 1) == 1))
            .ToQueryString();
#pragma warning restore EF9106

        var jsonDocument = azure ? "CAST([t].[Names] AS json)" : "[t].[Names]";
        Assert.Contains($"JSON_CONTAINS({jsonDocument}, N'a', N'$') = 1", withPath);
        Assert.Contains($"JSON_CONTAINS({jsonDocument}, N'a', N'$', 1) = 1", withSearchMode);
    }

    [Fact]
    public void JsonContains_casts_collated_nested_document()
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>()
            .UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));

        using var context = new NestedContainsContext(options.Options);
#pragma warning disable EF9106 // JsonContains is experimental
        var sql = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(
                EF.Functions.Collate(t.Names, "Latin1_General_100_BIN2"), "a") == 1))
            .ToQueryString();
#pragma warning restore EF9106

        Assert.Contains(
            "JSON_CONTAINS(CAST([t].[Names] COLLATE Latin1_General_100_BIN2 AS json), N'a') = 1",
            sql);
    }

    [Fact]
    public void JsonContains_inside_json_equality_comparison_visits_operands()
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>()
            .UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));

        using var context = new NestedContainsContext(options.Options);
        var sql = context.Entities
            .Where(e => e.Details.Tags.Any(t =>
                (t.Names.Contains("a") ? t.Names : t.AlternativeNames) == t.Names))
            .ToQueryString();

        Assert.Contains("JSON_CONTAINS(CAST([t].[Names] AS json), N'a') = 1", sql);
    }

    [Fact]
    public void JsonContains_inside_json_inequality_comparison_visits_operands()
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>()
            .UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));

        using var context = new NestedContainsContext(options.Options);
        var sql = context.Entities
            .Where(e => e.Details.Tags.Any(t =>
                (t.Names.Contains("a") ? t.Names : t.AlternativeNames) != t.Names))
            .ToQueryString();

        Assert.Contains("JSON_CONTAINS(CAST([t].[Names] AS json), N'a') = 1", sql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonContains_over_scalar_subquery_document(bool azure)
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(170));
        }

        using var context = new NestedContainsContext(options.Options);
#pragma warning disable EF9106 // JsonContains is experimental
        var sql = context.Entities
            .Where(e => EF.Functions.JsonContains(
                e.Details.Tags.OrderBy(t => t.Names.Count).Select(t => t.Names).First(), "a") == 1)
            .ToQueryString();
        var native = context.Entities
            .Where(e => EF.Functions.JsonContains(context.Entities.OrderBy(e2 => e2.Id).Select(e2 => e2.Names).First(), "a") == 1)
            .ToQueryString();
#pragma warning restore EF9106

        Assert.Contains(azure ? "JSON_CONTAINS(CAST((" : "JSON_CONTAINS((", sql);
        Assert.Contains(azure ? "AS json), N'a')" : "), N'a')", sql);
        Assert.Contains("SELECT TOP(1) [t].[Names]", sql);
        Assert.Contains(azure ? "[Names] nvarchar(max) '$.Names' AS JSON" : "[Names] json '$.Names' AS JSON", sql);
        Assert.Contains("JSON_CONTAINS((", native);
        Assert.Contains("SELECT TOP(1) [e0].[Names]", native);
        Assert.DoesNotContain("JSON_CONTAINS(CAST((", native);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonContains_over_conditional_document(bool azure)
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(170));
        }

        using var context = new NestedContainsContext(options.Options);
#pragma warning disable EF9106 // JsonContains is experimental
        var bothText = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(
                e.Id > 0 ? t.Names : t.AlternativeNames, "a") == 1))
            .ToQueryString();
        var whenText = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(
                e.Id > 0 ? t.Names : e.Names, "a") == 1))
            .ToQueryString();
        var elseText = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(
                e.Id > 0 ? e.Names : t.Names, "a") == 1))
            .ToQueryString();
        var native = context.Entities
            .Where(e => EF.Functions.JsonContains(e.Id > 0 ? e.Names : e.Details.Names, "a") == 1)
            .ToQueryString();
#pragma warning restore EF9106

        foreach (var sql in new[] { bothText, whenText, elseText })
        {
            Assert.Contains(azure ? "JSON_CONTAINS(CAST(CASE" : "JSON_CONTAINS(CASE", sql);
            Assert.Contains(azure ? "END AS json), N'a')" : "END, N'a')", sql);
        }

        Assert.Contains("ELSE [t].[AlternativeNames]", bothText);
        Assert.Contains("THEN [t].[Names]", whenText);
        Assert.Contains("ELSE [t].[Names]", elseText);
        Assert.Contains("JSON_CONTAINS(CASE", native);
        Assert.DoesNotContain("JSON_CONTAINS(CAST(CASE", native);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonContains_over_coalesced_document(bool azure)
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(170));
        }

        using var context = new NestedContainsContext(options.Options);
#pragma warning disable EF9106 // JsonContains is experimental
        var bothText = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(
                t.MaybeNames ?? t.AlternativeNames, "a") == 1))
            .ToQueryString();
        var fallbackText = context.Entities
            .Where(e => e.Details.Tags.Any(t => EF.Functions.JsonContains(
                e.MaybeNames ?? t.Names, "a") == 1))
            .ToQueryString();
        var native = context.Entities
            .Where(e => EF.Functions.JsonContains(e.MaybeNames ?? e.Names, "a") == 1)
            .ToQueryString();
#pragma warning restore EF9106

        Assert.Contains(
            azure
                ? "JSON_CONTAINS(CAST(COALESCE([t].[MaybeNames], [t].[AlternativeNames]) AS json), N'a')"
                : "JSON_CONTAINS(COALESCE([t].[MaybeNames], [t].[AlternativeNames]), N'a')",
            bothText);
        Assert.Contains(
            azure
                ? "JSON_CONTAINS(CAST(COALESCE([e].[MaybeNames], [t].[Names]) AS json), N'a')"
                : "JSON_CONTAINS(COALESCE([e].[MaybeNames], [t].[Names]), N'a')",
            fallbackText);
        Assert.Contains("JSON_CONTAINS(COALESCE([e].[MaybeNames], [e].[Names]), N'a')", native);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Contains_preserves_native_json_documents_and_casts_nested_text_documents(bool azure)
    {
        var options = new DbContextOptionsBuilder<NestedContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(170));
        }

        using var context = new NestedContainsContext(options.Options);

        var topLevel = context.Entities.Where(e => e.Names.Contains("a")).ToQueryString();
        Assert.Contains("JSON_CONTAINS([e].[Names], N'a')", topLevel);

        var reference = context.Entities.Where(e => e.Details.Names.Contains("a")).ToQueryString();
        Assert.Contains("JSON_CONTAINS(JSON_QUERY([e].[Details], '$.Names'), N'a')", reference);

        var nestedReference = context.Entities
            .Where(e => e.Details.Tags.Any(t => t.Child.Names.Contains("a")))
            .ToQueryString();
        Assert.Contains(
            azure
                ? "JSON_CONTAINS(CAST(JSON_QUERY([t].[Child], '$.Names') AS json), N'a')"
                : "JSON_CONTAINS(JSON_QUERY([t].[Child], '$.Names'), N'a')",
            nestedReference);

        var orderedCollection = context.Entities
            .Where(e => e.Details.Tags.Skip(1).Any(t => t.Names.Contains("a")))
            .ToQueryString();
        Assert.Contains("JSON_CONTAINS(CAST([t0].[Names] AS json), N'a')", orderedCollection);
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

    private class NestedContainsContext(DbContextOptions<NestedContainsContext> options) : DbContext(options)
    {
        public DbSet<NestedContainsEntity> Entities => Set<NestedContainsEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<NestedContainsEntity>().OwnsOne(e => e.Details, b =>
            {
                b.ToJson();
                b.OwnsMany(e => e.Tags, t => t.OwnsOne(e => e.Child));
            });
    }

    private class NestedContainsEntity
    {
        public int Id { get; set; }
        public List<string> Names { get; set; } = [];
        public List<string>? MaybeNames { get; set; }
        public NestedContainsDetails Details { get; set; } = null!;
    }

    private class NestedContainsDetails
    {
        public List<string> Names { get; set; } = [];
        public List<NestedContainsTag> Tags { get; set; } = [];
    }

    private class NestedContainsTag
    {
        public List<string> Names { get; set; } = [];
        public List<string> AlternativeNames { get; set; } = [];
        public List<string>? MaybeNames { get; set; }
        public NestedContainsChild Child { get; set; } = null!;
    }

    private class NestedContainsChild
    {
        public List<string> Names { get; set; } = [];
    }
}
