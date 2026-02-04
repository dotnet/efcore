// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

[SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
public class FullTextSearchTranslationsSqlServerTest : IClassFixture<FullTextSearchTranslationsSqlServerTest.FullTextSearchQueryFixture>
{
    private FullTextSearchQueryFixture Fixture { get; }

    public FullTextSearchTranslationsSqlServerTest(FullTextSearchQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region FREETEXTTABLE TVF tests

    [ConditionalFact]
    public async Task FreeTextTable_all_columns()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>("database")
            .ToListAsync();

        Assert.Single(results);

        AssertSql(
            """
@p='database' (Size = 4000)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], *, @p) AS [f]
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_single_column()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>(a => a.Title, "database")
            .ToListAsync();

        Assert.Single(results);
        Assert.All(results, r => Assert.True(r.Rank > 0));

        AssertSql(
            """
@p='database' (Size = 4000)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], [Title], @p) AS [f]
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_multiple_columns()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>(a => new { a.Title, a.Content }, "performance")
            .ToListAsync();

        Assert.Equal(3, results.Count);

        AssertSql(
            """
@p='performance' (Size = 4000)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], ([Title], [Content]), @p) AS [f]
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_with_language_term()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>(a => a.Title, "querying", languageTerm: "English")
            .ToListAsync();

        Assert.Single(results);

        AssertSql(
            """
@p='querying' (Size = 4000)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], [Title], @p, LANGUAGE N'English') AS [f]
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_with_top_n()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>(a => a.Content, "data", topN: 2)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        AssertSql(
            """
@p='data' (Size = 4000)
@p1='2' (Nullable = true)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], [Content], @p, @p1) AS [f]
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_with_language_and_top_n()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>(a => a.Content, "data", languageTerm: "English", topN: 2)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        AssertSql(
            """
@p='data' (Size = 4000)
@p1='2' (Nullable = true)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], [Content], @p, LANGUAGE N'English', @p1) AS [f]
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_join_to_get_entity()
    {
        using var context = CreateContext();

        var results = await (
            from ft in context.Articles.FreeTextTable<Article, int>(a => a.Title, "database")
            join a in context.Articles on ft.Key equals a.Id
            orderby ft.Rank descending
            select new { Article = a, ft.Rank })
            .ToListAsync();

        Assert.Single(results);
        Assert.NotNull(results[0].Article);

        AssertSql(
            """
@p='database' (Size = 4000)

SELECT [a0].[Id], [a0].[Content], [a0].[Title], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], [Title], @p) AS [f]
INNER JOIN [Articles] AS [a0] ON [f].[KEY] = [a0].[Id]
ORDER BY [f].[RANK] DESC
""");
    }

    [ConditionalFact]
    public async Task FreeTextTable_order_by_rank()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .FreeTextTable<Article, int>(a => a.Content, "data")
            .OrderByDescending(r => r.Rank)
            .ToListAsync();

        Assert.True(results.Count > 0);
        // Verify descending order
        for (var i = 1; i < results.Count; i++)
        {
            Assert.True(results[i - 1].Rank >= results[i].Rank);
        }

        AssertSql(
            """
@p='data' (Size = 4000)

SELECT [f].[KEY] AS [Key], [f].[RANK] AS [Rank]
FROM FREETEXTTABLE([Articles], [Content], @p) AS [f]
ORDER BY [f].[RANK] DESC
""");
    }

    #endregion

    #region CONTAINSTABLE TVF tests

    [ConditionalFact]
    public async Task ContainsTable_all_columns()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .ContainsTable<Article, int>("database")
            .ToListAsync();

        Assert.Single(results);

        AssertSql(
            """
@p='database' (Size = 4000)

SELECT [c].[KEY] AS [Key], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], *, @p) AS [c]
""");
    }

    [ConditionalFact]
    public async Task ContainsTable_single_column()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .ContainsTable<Article, int>(a => a.Title, "database")
            .ToListAsync();

        Assert.Single(results);
        Assert.All(results, r => Assert.True(r.Rank > 0));

        AssertSql(
            """
@p='database' (Size = 4000)

SELECT [c].[KEY] AS [Key], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], [Title], @p) AS [c]
""");
    }

    [ConditionalFact]
    public async Task ContainsTable_multiple_columns()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .ContainsTable<Article, int>(a => new { a.Title, a.Content }, "performance")
            .ToListAsync();

        Assert.Equal(3, results.Count);

        AssertSql(
            """
@p='performance' (Size = 4000)

SELECT [c].[KEY] AS [Key], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], ([Title], [Content]), @p) AS [c]
""");
    }

    [ConditionalFact]
    public async Task ContainsTable_with_language_term()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .ContainsTable<Article, int>(a => a.Title, "querying", languageTerm: "English")
            .ToListAsync();

        Assert.Single(results);

        AssertSql(
            """
@p='querying' (Size = 4000)

SELECT [c].[KEY] AS [Key], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], [Title], @p, LANGUAGE N'English') AS [c]
""");
    }

    [ConditionalFact]
    public async Task ContainsTable_with_top_n()
    {
        using var context = CreateContext();

        var results = await context.Articles
            .ContainsTable<Article, int>(a => a.Content, "data OR performance", topN: 2)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        AssertSql(
            """
@p='data OR performance' (Size = 4000)
@p1='2' (Nullable = true)

SELECT [c].[KEY] AS [Key], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], [Content], @p, @p1) AS [c]
""");
    }

    [ConditionalFact]
    public async Task ContainsTable_join_to_get_entity()
    {
        using var context = CreateContext();

        var results = await (
            from ft in context.Articles.ContainsTable<Article, int>(a => a.Title, "database")
            join a in context.Articles on ft.Key equals a.Id
            orderby ft.Rank descending
            select new { Article = a, ft.Rank })
            .ToListAsync();

        Assert.Single(results);
        Assert.NotNull(results[0].Article);

        AssertSql(
            """
@p='database' (Size = 4000)

SELECT [a0].[Id], [a0].[Content], [a0].[Title], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], [Title], @p) AS [c]
INNER JOIN [Articles] AS [a0] ON [c].[KEY] = [a0].[Id]
ORDER BY [c].[RANK] DESC
""");
    }

    #endregion

    #region FREETEXT predicate tests

    [ConditionalFact]
    public async Task FreeText_literal()
    {
        using var context = CreateContext();
        var result = await context.Articles
            .Where(a => EF.Functions.FreeText(a.Title, "database"))
            .ToListAsync();

        Assert.Single(result);

        AssertSql(
            """
SELECT [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE FREETEXT([a].[Title], N'database')
""");
    }

    [ConditionalFact]
    public void FreeText_client_eval_throws()
    {
        Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring"));
        Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring", 1033));
    }

    [ConditionalFact]
    public void FreeText_multiple_words()
    {
        using var context = CreateContext();
        var result = context.Articles
            .Where(a => EF.Functions.FreeText(a.Content, "data performance"))
            .Count();

        Assert.Equal(3, result);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Articles] AS [a]
WHERE FREETEXT([a].[Content], N'data performance')
""");
    }

    [ConditionalFact]
    public void FreeText_with_language_term()
    {
        using var context = CreateContext();
        var result = context.Articles.SingleOrDefault(a => EF.Functions.FreeText(a.Title, "querying", 1033));

        Assert.NotNull(result);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE FREETEXT([a].[Title], N'querying', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public void FreeText_with_non_literal_language_term()
    {
        var language = 1033;
        using var context = CreateContext();
        var result = context.Articles.SingleOrDefault(a => EF.Functions.FreeText(a.Title, "querying", language));

        Assert.NotNull(result);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE FREETEXT([a].[Title], N'querying', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public void FreeText_multiple_predicates()
    {
        using var context = CreateContext();
        var result = context.Articles
            .Where(a => EF.Functions.FreeText(a.Title, "database")
                && EF.Functions.FreeText(a.Content, "performance", 1033))
            .FirstOrDefault();

        Assert.NotNull(result);

        AssertSql(
            """
SELECT TOP(1) [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE FREETEXT([a].[Title], N'database') AND FREETEXT([a].[Content], N'performance', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public async Task FreeText_throws_when_using_non_parameter_or_constant_for_freetext_string()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<SqlException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.FreeText(a.Title, a.Content)));

        await Assert.ThrowsAsync<SqlException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.FreeText(a.Title, "")));

        await Assert.ThrowsAsync<SqlException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.FreeText(a.Title, a.Content.ToUpper())));
    }

    [ConditionalFact]
    public async Task FreeText_throws_when_using_non_column_for_property_reference()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.FreeText(a.Title + "1", "database")));

        await Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.FreeText(a.Title.ToLower(), "database")));
    }

    #endregion

    #region CONTAINS predicate tests

    [ConditionalFact]
    public void Contains_should_throw_on_client_eval()
    {
        var exNoLang = Assert.Throws<InvalidOperationException>(() => EF.Functions.Contains("teststring", "teststring"));
        Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.Contains)),
            exNoLang.Message);

        var exLang = Assert.Throws<InvalidOperationException>(() => EF.Functions.Contains("teststring", "teststring", 1033));
        Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.Contains)),
            exLang.Message);
    }

    [ConditionalFact]
    public async Task Contains_should_throw_when_using_non_parameter_or_constant_for_contains_string()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<SqlException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.Contains(a.Title, a.Content)));

        await Assert.ThrowsAsync<SqlException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.Contains(a.Title, "")));

        await Assert.ThrowsAsync<SqlException>(async ()
            => await context.Articles.FirstOrDefaultAsync(a => EF.Functions.Contains(a.Title, a.Content.ToUpper())));
    }

    [ConditionalFact]
    public async Task Contains_literal()
    {
        using var context = CreateContext();
        var result = await context.Articles
            .Where(a => EF.Functions.Contains(a.Title, "database"))
            .ToListAsync();

        Assert.Single(result);

        AssertSql(
            """
SELECT [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE CONTAINS([a].[Title], N'database')
""");
    }

    [ConditionalFact]
    public void Contains_with_language_term()
    {
        using var context = CreateContext();
        var result = context.Articles.SingleOrDefault(a => EF.Functions.Contains(a.Title, "querying", 1033));

        Assert.NotNull(result);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE CONTAINS([a].[Title], N'querying', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public void Contains_with_non_literal_language_term()
    {
        var language = 1033;
        using var context = CreateContext();
        var result = context.Articles.SingleOrDefault(a => EF.Functions.Contains(a.Title, "querying", language));

        Assert.NotNull(result);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE CONTAINS([a].[Title], N'querying', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public async Task Contains_with_logical_operator()
    {
        using var context = CreateContext();
        var result = await context.Articles
            .Where(a => EF.Functions.Contains(a.Content, "data OR storage"))
            .ToListAsync();

        Assert.Equal(3, result.Count);

        AssertSql(
            """
SELECT [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE CONTAINS([a].[Content], N'data OR storage')
""");
    }

    [ConditionalFact]
    public async Task Contains_with_prefix_term_and_language_term()
    {
        using var context = CreateContext();
        var result = await context.Articles
            .SingleOrDefaultAsync(a => EF.Functions.Contains(a.Title, "\"query*\"", 1033));

        Assert.NotNull(result);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE CONTAINS([a].[Title], N'"query*"', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public async Task Contains_with_proximity_term_and_language_term()
    {
        using var context = CreateContext();
        var results = await context.Articles
            .Where(a => EF.Functions.Contains(a.Content, "NEAR((data, performance), 5)", 1033))
            .ToListAsync();

        Assert.Equal(3, results.Count);

        AssertSql(
            """
SELECT [a].[Id], [a].[Content], [a].[Title]
FROM [Articles] AS [a]
WHERE CONTAINS([a].[Content], N'NEAR((data, performance), 5)', LANGUAGE 1033)
""");
    }

    #endregion

    #region Binary column full-text search tests

    [ConditionalFact]
    public async Task FreeText_with_binary_column()
    {
        using var context = CreateContext();
        var result = await context.BinaryArticles.SingleAsync(b => EF.Functions.FreeText(b.Content, "bombing"));

        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Content], [b].[FileExtension]
FROM [BinaryArticles] AS [b]
WHERE FREETEXT([b].[Content], N'bombing')
""");
    }

    [ConditionalFact]
    public async Task FreeText_with_binary_column_and_language_term()
    {
        using var context = CreateContext();
        var result = await context.BinaryArticles.SingleAsync(b => EF.Functions.FreeText(b.Content, "bombing", 1033));

        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Content], [b].[FileExtension]
FROM [BinaryArticles] AS [b]
WHERE FREETEXT([b].[Content], N'bombing', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    public async Task Contains_with_binary_column()
    {
        using var context = CreateContext();
        var result = await context.BinaryArticles.SingleAsync(b => EF.Functions.Contains(b.Content, "bomb"));

        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Content], [b].[FileExtension]
FROM [BinaryArticles] AS [b]
WHERE CONTAINS([b].[Content], N'bomb')
""");
    }

    [ConditionalFact]
    public async Task Contains_with_binary_column_and_language_term()
    {
        using var context = CreateContext();
        var result = await context.BinaryArticles.SingleAsync(b => EF.Functions.Contains(b.Content, "bomb", 1033));

        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Content], [b].[FileExtension]
FROM [BinaryArticles] AS [b]
WHERE CONTAINS([b].[Content], N'bomb', LANGUAGE 1033)
""");
    }

    #endregion

    protected FullTextSearchContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FullTextSearchContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<BinaryArticle> BinaryArticles { get; set; } = null!;

        public static async Task SeedAsync(FullTextSearchContext context)
        {
            Article[] articles =
            [
                new() { Id = 1, Title = "Introduction to Database Systems", Content = "This article covers the basics of database systems including data storage, retrieval, and performance optimization." },
                new() { Id = 2, Title = "Advanced Querying Techniques", Content = "Learn about advanced querying techniques for better performance and data retrieval." },
                new() { Id = 3, Title = "Data Storage Best Practices", Content = "Best practices for data storage and performance tuning in modern applications." }
            ];

            context.Articles.AddRange(articles);

            BinaryArticle[] binaryArticles =
            [
                new() { Id = 1, FileExtension = ".html", Content = "<h1>Deploy the Lightmass Bomb to destroy the enemy</h1><p>Bombing mission details</p>"u8.ToArray() },
                new() { Id = 2, FileExtension = ".html", Content = "<h1>Reconnaissance mission</h1><p>Gather intelligence on enemy positions</p>"u8.ToArray() }
            ];

            context.BinaryArticles.AddRange(binaryArticles);

            await context.SaveChangesAsync();

            // Create full-text catalog and index for Articles
            await context.Database.ExecuteSqlAsync($"""
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ftCatalog')
    CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;
""");

            await context.Database.ExecuteSqlAsync($"""
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Articles'))
    CREATE FULLTEXT INDEX ON Articles([Title], [Content])
        KEY INDEX [PK_Articles]
        WITH STOPLIST = SYSTEM;
""");

            // Create full-text index for BinaryArticles (binary column with type column)
            await context.Database.ExecuteSqlAsync($"""
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('BinaryArticles'))
    CREATE FULLTEXT INDEX ON BinaryArticles([Content] TYPE COLUMN [FileExtension])
        KEY INDEX [PK_BinaryArticles];
""");

            // Wait for the full-text indexes to be populated
            await context.Database.ExecuteSqlAsync($"""
WHILE (SELECT FULLTEXTCATALOGPROPERTY('ftCatalog', 'PopulateStatus')) <> 0
BEGIN
    WAITFOR DELAY '00:00:00.100';
END
""");
        }
    }

    public class Article
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    public class BinaryArticle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(16)")]
        public string FileExtension { get; set; } = null!;

        [Column(TypeName = "varbinary(max)")]
        public byte[] Content { get; set; } = null!;
    }

    public class FullTextSearchQueryFixture : SharedStoreFixtureBase<FullTextSearchContext>
    {
        protected override string StoreName
            => "FullTextSearchTranslationsTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(FullTextSearchContext context)
            => FullTextSearchContext.SeedAsync(context);
    }
}
