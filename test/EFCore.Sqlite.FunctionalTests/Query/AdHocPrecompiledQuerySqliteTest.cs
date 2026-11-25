// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(fixture, testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    protected override DbContextOptionsBuilder UseWindowFunctionAggregates(DbContextOptionsBuilder optionsBuilder)
    {
#pragma warning disable EF9001 // UseWindowFunctionAggregates is experimental
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseWindowFunctionAggregates();
#pragma warning restore EF9001
        return optionsBuilder;
    }

    public override async Task Coalesce_nullable_long_in_anonymous_projection()
    {
        await base.Coalesce_nullable_long_in_anonymous_projection();

        AssertSql(
            """
SELECT COALESCE("r"."SizeBytes", 0) AS "SizeBytes"
FROM "Records" AS "r"
ORDER BY "r"."Id"
""");
    }

    [Fact]
    public async Task JsonExtract_single_path()
    {
        var contextFactory = await InitializeNonSharedTest<JsonContext>();
        await using (var context = contextFactory.CreateDbContext())
        {
            context.JsonEntities.Add(new JsonEntity { JsonThing = new JsonThing { StringProperty = "value" }, IntList = [42] });
            await context.SaveChangesAsync();
        }

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
var result = context.JsonEntities.Select(e => new
{
    Text = EF.Functions.JsonExtract(e.JsonThing, "$.StringProperty"),
    Number = EF.Functions.JsonExtract(e.IntList, "$[0]"),
    NumberAsText = EF.Functions.JsonExtract(e.IntList, new[] { "$[0]" }),
    NumbersAsJson = EF.Functions.JsonExtract(e.IntList, "$[0]", "$[1]"),
    Matches = object.Equals(EF.Functions.JsonExtract(e.IntList, "$[0]"), 42L),
    Fallback = EF.Functions.JsonExtract(e.JsonThing, "$.missing") ?? "don't"
}).Single();
Assert.Equal("value", result.Text);
Assert.Equal(42L, result.Number);
Assert.Equal("42", result.NumberAsText);
Assert.Equal("[42,null]", result.NumbersAsJson);
Assert.True(result.Matches);
Assert.Equal("don't", result.Fallback);
""",
            typeof(JsonContext),
            contextFactory.GetOptions());
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlitePrecompiledQueryTestHelpers.Instance;
}
