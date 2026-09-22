// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlTypes;

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

SELECT TOP(@p) [v].[Id], [v].[Vector]
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

SELECT TOP(@p) [v].[Id], [v].[Vector]
FROM [VectorEntities] AS [v]
ORDER BY VECTOR_DISTANCE('cosine', [v].[Vector], CAST('[1,2,100]' AS VECTOR(3)))
""");
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

        public static async Task SeedAsync(VectorQueryContext context)
        {
            var vectorEntities = new VectorEntity[]
            {
                new() { Id = 1, Vector = new SqlVector<float>(new float[] { 1, 2, 3 }) },
                new() { Id = 2, Vector = new SqlVector<float>(new float[] { 1, 2, 100 }) },
                new() { Id = 3, Vector = new SqlVector<float>(new float[] { 1, 2, 1000 }) }
            };

            context.VectorEntities.AddRange(vectorEntities);
            await context.SaveChangesAsync();
        }
    }

    public class VectorEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column(TypeName = "vector(3)")]
        public SqlVector<float> Vector { get; set; }
    }

    public class VectorQueryFixture : SharedStoreFixtureBase<VectorQueryContext>
    {
        protected override string StoreName
            => "VectorQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(VectorQueryContext context)
            => VectorQueryContext.SeedAsync(context);
    }
}
