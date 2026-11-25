// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

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

    [Fact]
    public async Task JsonExtract_dynamic_mapping_with_runtime_command_cache()
    {
        var contextFactory = await InitializeNonSharedTest<JsonContext>();
        await using (var context = contextFactory.CreateDbContext())
        {
            var typeMappingSource = context.GetService<IRelationalTypeMappingSource>();
            Assert.Null(typeMappingSource.FindMapping(typeof(object)));
            Assert.Null(typeMappingSource.FindMapping(typeof(object), context.Model));
            Assert.IsType<SqliteDynamicTypeMapping>(typeMappingSource.FindMapping(typeof(object), "TEXT"));

            context.JsonEntities.Add(new JsonEntity { JsonThing = new JsonThing { StringProperty = "value" }, IntList = [42] });
            await context.SaveChangesAsync();
        }

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
string? a = "a", b = "b", c = "c", d = null;
object fallback = "fallback";
var result = context.JsonEntities
    .Where(e => e.JsonThing.StringProperty != a && e.JsonThing.StringProperty != b
        && e.JsonThing.StringProperty != c && e.JsonThing.StringProperty != d)
    .Select(e => new
    {
        Literal = EF.Functions.JsonExtract(e.JsonThing, "$.missing") ?? "fallback",
        Parameter = EF.Functions.JsonExtract(e.JsonThing, "$.missing") ?? fallback
    })
    .Single();
Assert.Equal("fallback", result.Literal);
Assert.Equal("fallback", result.Parameter);
""",
            typeof(JsonContext),
            contextFactory.GetOptions(),
            interceptorCodeAsserter: code =>
            {
                Assert.Contains("RelationalCommandCache", code);
                Assert.Contains("relationalTypeMappingSource.FindMapping(typeof(object), \"TEXT\"", code);
            });
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlitePrecompiledQueryTestHelpers.Instance;
}
