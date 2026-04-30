// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable EF9105 // Vector search is experimental

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

[SqlServerCondition(SqlServerCondition.SupportsVectorType)]
public class VectorTranslationsSqlServerTest : IClassFixture<VectorTranslationsSqlServerTest.VectorQueryFixture>
{
    private VectorQueryFixture Fixture { get; }

    public VectorTranslationsSqlServerTest(VectorQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public async Task VectorDistance_with_parameter()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });
        var results = await ctx.VectorEntities
            .OrderBy(v => EF.Functions.VectorDistance("cosine", v.Vector, vector))
            .Take(1)
            .ToListAsync();

        Assert.Equal(2, results.Single().Id);

        AssertSql(
            """
@p='1'
@vector='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p) [v].[Id]
FROM [VectorEntities] AS [v]
ORDER BY VECTOR_DISTANCE('cosine', [v].[Vector], @vector)
""");
    }

    [ConditionalFact]
    public async Task VectorDistance_with_constant()
    {
        using var ctx = CreateContext();

        var results = await ctx.VectorEntities
            .OrderBy(v => EF.Functions.VectorDistance("cosine", v.Vector, new SqlVector<float>(new float[] { 1, 2, 100 })))
            .Take(1)
            .ToListAsync();

        Assert.Equal(2, results.Single().Id);

        AssertSql(
            """
@p='1'

SELECT TOP(@p) [v].[Id]
FROM [VectorEntities] AS [v]
ORDER BY VECTOR_DISTANCE('cosine', [v].[Vector], CAST('[1,2,100]' AS VECTOR(3)))
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_project_entity_and_distance()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .OrderBy(r => r.Distance)
            .Take(1)
            .WithApproximate()
            .ToListAsync();

        Assert.Equal(2, results.Single().Value.Id);

        AssertSql(
            """
@p1='1'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p1) WITH APPROXIMATE [v].[Id], [v0].[Distance]
FROM VECTOR_SEARCH(
    TABLE = [VectorEntities] AS [v],
    COLUMN = [Vector],
    SIMILAR_TO = @p,
    METRIC = 'cosine'
) AS [v0]
ORDER BY [v0].[Distance]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_exact_knn()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .OrderBy(r => r.Distance)
            .Take(1)
            .ToListAsync();

        Assert.Equal(2, results.Single().Value.Id);

        AssertSql(
            """
@p1='1'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p1) [v].[Id], [v0].[Distance]
FROM VECTOR_SEARCH(
    TABLE = [VectorEntities] AS [v],
    COLUMN = [Vector],
    SIMILAR_TO = @p,
    METRIC = 'cosine'
) AS [v0]
ORDER BY [v0].[Distance]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_project_entity_only_with_distance_filter()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .Where(e => e.Distance < 0.01)
            .OrderBy(e => e.Distance)
            .Select(e => e.Value)
            .Take(3)
            .WithApproximate()
            .ToListAsync();

        Assert.Collection(
            results,
            r => Assert.Equal(2, r.Id),
            r => Assert.Equal(3, r.Id));

        AssertSql(
            """
@p1='3'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p1) WITH APPROXIMATE [v].[Id]
FROM VECTOR_SEARCH(
    TABLE = [VectorEntities] AS [v],
    COLUMN = [Vector],
    SIMILAR_TO = @p,
    METRIC = 'cosine'
) AS [v0]
WHERE [v0].[Distance] < 0.01E0
ORDER BY [v0].[Distance]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_in_subquery()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .OrderBy(e => e.Distance)
            .Take(3)
            .WithApproximate()
            .Select(e => new { e.Value.Id, e.Distance })
            .Where(e => e.Distance < 0.01)
            .ToListAsync();

        Assert.Collection(
            results,
            r => Assert.Equal(2, r.Id),
            r => Assert.Equal(3, r.Id));

        AssertSql(
            """
@p1='3'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT [v1].[Id], [v1].[Distance]
FROM (
    SELECT TOP(@p1) WITH APPROXIMATE [v].[Id], [v0].[Distance]
    FROM VECTOR_SEARCH(
        TABLE = [VectorEntities] AS [v],
        COLUMN = [Vector],
        SIMILAR_TO = @p,
        METRIC = 'cosine'
    ) AS [v0]
    ORDER BY [v0].[Distance]
) AS [v1]
WHERE [v1].[Distance] < 0.01E0
ORDER BY [v1].[Distance]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_with_Where_before_Take()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .Where(e => e.Value.Id > 5)
            .OrderBy(e => e.Distance)
            .Take(3)
            .WithApproximate()
            .ToListAsync();

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Value.Id > 5));

        AssertSql(
            """
@p1='3'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p1) WITH APPROXIMATE [v].[Id], [v0].[Distance]
FROM VECTOR_SEARCH(
    TABLE = [VectorEntities] AS [v],
    COLUMN = [Vector],
    SIMILAR_TO = @p,
    METRIC = 'cosine'
) AS [v0]
WHERE [v].[Id] > 5
ORDER BY [v0].[Distance]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_with_Join_before_Take()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .Join(
                ctx.VectorEntities,
                r => r.Value.Id,
                e => e.Id,
                (r, e) => new { e.Id, r.Distance })
            .OrderBy(r => r.Distance)
            .Take(3)
            .WithApproximate()
            .ToListAsync();

        Assert.Equal(3, results.Count);

        AssertSql(
            """
@p1='3'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p1) WITH APPROXIMATE [v1].[Id], [v0].[Distance]
FROM VECTOR_SEARCH(
    TABLE = [VectorEntities] AS [v],
    COLUMN = [Vector],
    SIMILAR_TO = @p,
    METRIC = 'cosine'
) AS [v0]
INNER JOIN [VectorEntities] AS [v1] ON [v].[Id] = [v1].[Id]
ORDER BY [v0].[Distance]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_with_Take_and_Skip()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var results = await ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .OrderBy(r => r.Distance)
            .Take(3)
            .WithApproximate()
            .Skip(1)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        AssertSql(
            """
@p1='3'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)
@p2='1'

SELECT [v1].[Id], [v1].[Distance]
FROM (
    SELECT TOP(@p1) WITH APPROXIMATE [v].[Id], [v0].[Distance]
    FROM VECTOR_SEARCH(
        TABLE = [VectorEntities] AS [v],
        COLUMN = [Vector],
        SIMILAR_TO = @p,
        METRIC = 'cosine'
    ) AS [v0]
    ORDER BY [v0].[Distance]
) AS [v1]
ORDER BY [v1].[Distance]
OFFSET @p2 ROWS
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_reranking()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        // Inner: approximate broad retrieval
        var candidates = ctx.VectorEntities
            .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
            .OrderBy(r => r.Distance)
            .Take(10)
            .WithApproximate();

        // Outer: exact re-ranking by a different criterion (no WithApproximate — exact top 3)
        var results = await candidates
            .OrderBy(r => r.Value.Id)
            .Take(3)
            .ToListAsync();

        Assert.Equal(3, results.Count);

        AssertSql(
            """
@p2='3'
@p1='10'
@p='Microsoft.Data.SqlTypes.SqlVector`1[System.Single]' (Size = 20) (DbType = Binary)

SELECT TOP(@p2) [v1].[Id], [v1].[Distance]
FROM (
    SELECT TOP(@p1) WITH APPROXIMATE [v].[Id], [v0].[Distance]
    FROM VECTOR_SEARCH(
        TABLE = [VectorEntities] AS [v],
        COLUMN = [Vector],
        SIMILAR_TO = @p,
        METRIC = 'cosine'
    ) AS [v0]
    ORDER BY [v0].[Distance]
) AS [v1]
ORDER BY [v1].[Id]
""");
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task WithApproximate_without_Take_throws()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.VectorEntities
                .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
                .WithApproximate()
                .ToListAsync());

        Assert.Equal(SqlServerStrings.WithApproximateRequiresTake, exception.Message);
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task WithApproximate_with_Skip_and_Take_throws()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.VectorEntities
                .VectorSearch(e => e.Vector, similarTo: vector, "cosine")
                .OrderBy(r => r.Distance)
                .Skip(1)
                .Take(3)
                .WithApproximate()
                .ToListAsync());

        Assert.Equal(SqlServerStrings.WithApproximateNotSupportedWithSkipAndTake, exception.Message);
    }

    // The latest vector index version (required for VECTOR_SEARCH) is only available on Azure SQL (#36384).
    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsAzureSql)]
    public async Task VectorSearch_without_WithApproximate_logs_warning()
    {
        using var ctx = CreateContext();

        var vector = new SqlVector<float>(new float[] { 1, 2, 100 });

        // Use a query structurally distinct from other tests to avoid compiled query cache hits
        _ = await ctx.VectorEntities
            .VectorSearch(e => e.IndexedVector, similarTo: vector, "cosine")
            .OrderBy(r => r.Distance)
            .Select(r => r.Value.Id)
            .Take(1)
            .ToListAsync();

        var warning = Assert.Single(Fixture.TestSqlLoggerFactory.Log, l => l.Id == SqlServerEventId.VectorSearchWithoutApproximateWarning);
        Assert.Equal(LogLevel.Warning, warning.Level);
        Assert.Contains("IndexedVector", warning.Message);
        Assert.Contains("VectorEntity", warning.Message);
    }

    [ConditionalFact]
    public async Task Length()
    {
        using var ctx = CreateContext();

        var count = await ctx.VectorEntities
            .Where(v => v.Vector.Length == 3)
            .CountAsync();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(await ctx.VectorEntities.CountAsync(), count);
        }

        AssertSql(
            """
SELECT COUNT(*)
FROM [VectorEntities] AS [v]
WHERE VECTORPROPERTY([v].[Vector], 'Dimensions') = 3
""");
    }

    protected VectorQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class VectorQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<VectorEntity> VectorEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<VectorEntity>().HasVectorIndex(e => e.IndexedVector).HasMetric("cosine");

        public static async Task SeedAsync(VectorQueryContext context)
        {
            // SQL Server vector indexes require at least 100 rows.
            var vectorEntities = Enumerable.Range(1, 100).Select(
                i => new VectorEntity
                {
                    Id = i,
                    Vector = new SqlVector<float>(new float[] { i * 0.01f, i * 0.02f, i * 0.03f }),
                    IndexedVector = new SqlVector<float>(new float[] { i * 0.01f, i * 0.02f, i * 0.03f })
                }).ToList();

            // Override specific rows we use in test assertions
            vectorEntities[0] = new VectorEntity
            {
                Id = 1,
                Vector = new SqlVector<float>(new float[] { 1, 2, 3 }),
                IndexedVector = new SqlVector<float>(new float[] { 1, 2, 3 })
            };
            vectorEntities[1] = new VectorEntity
            {
                Id = 2,
                Vector = new SqlVector<float>(new float[] { 1, 2, 100 }),
                IndexedVector = new SqlVector<float>(new float[] { 1, 2, 100 })
            };
            vectorEntities[2] = new VectorEntity
            {
                Id = 3,
                Vector = new SqlVector<float>(new float[] { 1, 2, 1000 }),
                IndexedVector = new SqlVector<float>(new float[] { 1, 2, 1000 })
            };

            context.VectorEntities.AddRange(vectorEntities);
            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlAsync($"ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON");

            await context.Database.ExecuteSqlAsync($"""
CREATE VECTOR INDEX vec_idx ON VectorEntities(IndexedVector)
WITH (METRIC = 'Cosine', TYPE = 'DiskANN');
""");
        }
    }

    public class VectorEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column(TypeName = "vector(3)")]
        public SqlVector<float> Vector { get; set; }

        [Column(TypeName = "vector(3)")]
        public SqlVector<float> IndexedVector { get; set; }
    }

    public class VectorQueryFixture : SharedStoreFixtureBase<VectorQueryContext>
    {
        protected override string StoreName
            => "VectorTranslationsTest";

        // Vector indexes require ≥100 rows with non-NULL vectors, so the standard EnsureClean
        // (which drops + recreates tables including the vector index before seeding) fails.
        // VectorSearchTestStoreFactory creates a store that drops/creates tables without the
        // vector index; SeedAsync then inserts data and creates the vector index via raw SQL.
        protected override ITestStoreFactory TestStoreFactory
            => VectorSearchTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .ConfigureWarnings(w => w.Log(SqlServerEventId.VectorSearchWithoutApproximateWarning));

        protected override Task SeedAsync(VectorQueryContext context)
            => VectorQueryContext.SeedAsync(context);

        private class VectorSearchTestStoreFactory : SqlServerTestStoreFactory
        {
            public static new VectorSearchTestStoreFactory Instance { get; } = new();

            public override TestStore GetOrCreate(string storeName)
                => new VectorSearchTestStore(storeName);
        }

        private class VectorSearchTestStore(string name) : SqlServerTestStore(name)
        {
            // Vector indexes require ≥100 rows, so we can't use the standard EnsureClean
            // (which drops + recreates all tables including vector indexes before data exists).
            // Instead we drop the table and recreate it without the vector index;
            // it gets created by SeedAsync after data is inserted.
            protected override async Task InitializeAsync(
                Func<DbContext> createContext,
                Func<DbContext, Task>? seed,
                Func<DbContext, Task>? clean)
            {
                await using var context = createContext();

                // Ensure the database itself exists (EnsureCreated would also create
                // the vector index, which fails on empty tables).
                await using (var master = new SqlConnection(CreateConnectionString("master", multipleActiveResultSets: false)))
                {
                    await master.OpenAsync();
                    await using var command = master.CreateCommand();
                    command.CommandText = $"""
                        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{Name}')
                            CREATE DATABASE [{Name}];
                        """;
                    await command.ExecuteNonQueryAsync();
                }

                await context.Database.ExecuteSqlRawAsync(
                    """
                    DROP TABLE IF EXISTS [VectorEntities];
                    CREATE TABLE [VectorEntities] (
                        [Id] int NOT NULL,
                        [IndexedVector] vector(3) NOT NULL,
                        [Vector] vector(3) NOT NULL,
                        CONSTRAINT [PK_VectorEntities] PRIMARY KEY ([Id])
                    );
                    """);

                if (seed != null)
                {
                    await seed(context);
                }
            }
        }
    }
}
