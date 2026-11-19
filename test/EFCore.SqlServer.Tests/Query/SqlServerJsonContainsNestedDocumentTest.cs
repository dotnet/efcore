// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class SqlServerJsonContainsNestedDocumentTest
{
    [Theory]
    [InlineData(false, 170)]
    [InlineData(true, 170)]
    [InlineData(false, 160)]
    [InlineData(true, 160)]
    public void Contains_over_primitive_collection_in_json_collection(bool azure, int compatibilityLevel)
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Contains_preserves_native_json_documents_and_casts_nested_text_documents(bool azure)
    {
        var options = new DbContextOptionsBuilder<ContainsContext>();
        if (azure)
        {
            options.UseAzureSql((string?)null, b => b.UseCompatibilityLevel(170));
        }
        else
        {
            options.UseSqlServer((string?)null, b => b.UseCompatibilityLevel(170));
        }

        using var context = new ContainsContext(options.Options);

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

    private class ContainsContext(DbContextOptions<ContainsContext> options) : DbContext(options)
    {
        public DbSet<ContainsEntity> Entities => Set<ContainsEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ContainsEntity>().OwnsOne(e => e.Details, b =>
            {
                b.ToJson();
                b.OwnsMany(e => e.Tags, t => t.OwnsOne(e => e.Child));
            });
    }

    private class ContainsEntity
    {
        public int Id { get; set; }
        public List<string> Names { get; set; } = [];
        public ContainsDetails Details { get; set; } = null!;
    }

    private class ContainsDetails
    {
        public List<string> Names { get; set; } = [];
        public List<ContainsTag> Tags { get; set; } = [];
    }

    private class ContainsTag
    {
        public List<string> Names { get; set; } = [];
        public ContainsChild Child { get; set; } = null!;
    }

    private class ContainsChild
    {
        public List<string> Names { get; set; } = [];
    }
}
