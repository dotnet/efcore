// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;

namespace Microsoft.EntityFrameworkCore;

#pragma warning disable EF9104

public class FullTextSearchCosmosTest : IClassFixture<FullTextSearchCosmosTest.FullTextSearchFixture>
{
    public FullTextSearchCosmosTest(FullTextSearchFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        _testOutputHelper = testOutputHelper;
        fixture.TestSqlLoggerFactory.Clear();
    }

    protected FullTextSearchFixture Fixture { get; }

    private readonly ITestOutputHelper _testOutputHelper;

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_in_predicate_using_constant_argument()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContains(x.Description, "beaver"))
            .ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver")));

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContains(c["Description"], "beaver")
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_in_predicate_using_parameter_argument()
    {
        await using var context = CreateContext();

        var beaver = "beaver";
        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContains(x.Description, beaver))
            .ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver")));

        AssertSql(
"""
@beaver='beaver'

SELECT VALUE c
FROM root c
WHERE FullTextContains(c["Description"], @beaver)
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContainsAny_in_predicate()
    {
        await using var context = CreateContext();

        var beaver = "beaver";

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContainsAny(x.Description, beaver, "bat"))
            .ToListAsync();

        Assert.Equal(4, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver") || x.Description.Contains("bat")));

        AssertSql(
"""
@beaver='beaver'

SELECT VALUE c
FROM root c
WHERE FullTextContainsAny(c["Description"], @beaver, "bat")
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContainsAll_in_predicate()
    {
        await using var context = CreateContext();

        var beaver = "beaver";
        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContainsAll(x.Description, beaver, "salmon", "frog"))
            .ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver") && x.Description.Contains("salmon") && x.Description.Contains("frog")));

        AssertSql(
"""
@beaver='beaver'

SELECT VALUE c
FROM root c
WHERE FullTextContainsAll(c["Description"], @beaver, "salmon", "frog")
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_in_projection_using_constant_argument()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => x.Id)
            .Select(x => new { x.Description, ContainsBeaver = EF.Functions.FullTextContains(x.Description, "beaver") })
            .ToListAsync();

        Assert.True(result.All(x => x.Description.Contains("beaver") == x.ContainsBeaver));

        AssertSql(
"""
SELECT c["Description"], FullTextContains(c["Description"], "beaver") AS ContainsBeaver
FROM root c
ORDER BY c["Id"]
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_in_projection_using_parameter_argument()
    {
        await using var context = CreateContext();

        var beaver = "beaver";
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => x.Id)
            .Select(x => new { x.Description, ContainsBeaver = EF.Functions.FullTextContains(x.Description, beaver) })
            .ToListAsync();

        Assert.True(result.All(x => x.Description.Contains("beaver") == x.ContainsBeaver));

        AssertSql(
"""
@beaver='beaver'

SELECT c["Description"], FullTextContains(c["Description"], @beaver) AS ContainsBeaver
FROM root c
ORDER BY c["Id"]
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_in_projection_using_complex_expression()
    {
        await using var context = CreateContext();

        var beaver = "beaver";
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Description, ContainsBeaverOrSometimesDuck = EF.Functions.FullTextContains(x.Description, x.Id < 3 ? beaver : "duck") })
            .ToListAsync();

        Assert.True(result.All(x => (x.Id < 3 ? x.Description.Contains("beaver") : x.Description.Contains("duck")) == x.ContainsBeaverOrSometimesDuck));

        AssertSql(
"""
@beaver='beaver'

SELECT c["Id"], c["Description"], FullTextContains(c["Description"], ((c["Id"] < 3) ? @beaver : "duck")) AS ContainsBeaverOrSometimesDuck
FROM root c
ORDER BY c["Id"]
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_non_property()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContains("habitat is the natural environment in which a particular species thrives", x.PartitionKey))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContains("habitat is the natural environment in which a particular species thrives", c["PartitionKey"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "otter", "beaver" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["otter","beaver"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_using_parameters()
    {
        await using var context = CreateContext();

        var otter = "otter";
        var beaver = "beaver";

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { otter, beaver }))
            .ToListAsync();

        AssertSql(
"""
@otter='otter'
@beaver='beaver'

SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], [@otter, @beaver])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_using_complex_expression()
    {
        await using var context = CreateContext();

        var otter = "otter";

        var message = (await Assert.ThrowsAsync<CosmosException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { x.Id > 2 ? otter : "beaver" }))
                .ToListAsync())).Message;

        Assert.Contains(
            "The second argument of the FullTextScore function must be a non-empty array of string literals.",
            message);
    }

    [ConditionalFact]
    public virtual async Task Select_FullTextScore()
    {
        await using var context = CreateContext();

        var message = (await Assert.ThrowsAsync<CosmosException>(
            () => context.Set<FullTextSearchAnimals>()
                .Select(x => EF.Functions.FullTextScore(x.Description, new string[] { "otter", "beaver" }))
                .ToListAsync())).Message;

        Assert.Contains(
            "The FullTextScore function is only allowed in the ORDER BY RANK clause.",
            message);
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_with_RRF_using_two_FullTextScore_functions()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.Rrf(
                EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }),
                EF.Functions.FullTextScore(x.Description, new string[] { "otter", "bat" })))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK RRF(FullTextScore(c["Description"], ["beaver"]), FullTextScore(c["Description"], ["otter","bat"]))
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_with_nested_RRF()
    {
        await using var context = CreateContext();

        var message = (await Assert.ThrowsAsync<CosmosException>(
            () => context.Set<FullTextSearchAnimals>().OrderBy(x => EF.Functions.Rrf(
                EF.Functions.Rrf(
                    EF.Functions.FullTextScore(x.Description, new string[] { "bison" }),
                    EF.Functions.FullTextScore(x.Description, new string[] { "fox", "bat" })),
                EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }),
                EF.Functions.FullTextScore(x.Description, new string[] { "otter", "bat" })))
            .ToListAsync())).Message;

        // TODO: this doesn't seem right
        Assert.Contains(
            "'RRF' is not a recognized built-in function name.",
            message);
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_with_RRF_with_one_argument()
    {
        await using var context = CreateContext();

        var message = (await Assert.ThrowsAsync<CosmosException>(
            () => context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.Rrf(EF.Functions.FullTextScore(x.Description, new string[] { "beaver" })))
            .ToListAsync())).Message;

        // TODO: this doesn't seem right
        Assert.Contains(
            "The ORDER BY RANK clause must be followed by a VectorDistance and/or a FullTextScore function call.",
            message);
    }


    [ConditionalFact]
    public virtual async Task OrderByRank_RRF_with_non_function_argument()
    {
        await using var context = CreateContext();
        var message = (await Assert.ThrowsAsync<CosmosException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.Rrf(
                    EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }),
                    20.5d))
                .ToListAsync())).Message;

        Assert.Contains(
            "The ORDER BY RANK clause must be followed by a VectorDistance and/or a FullTextScore function call.",
            message);
    }

    [ConditionalFact(Skip = "issue #35867")]
    public virtual async Task OrderByRank_Take()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }))
            .Take(10)
            .ToListAsync();

        AssertSql();
    }

    [ConditionalFact(Skip = "issue #35867")]
    public virtual async Task OrderByRank_Skip_Take()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
            .Skip(1)
            .Take(20)
            .ToListAsync();

        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task OrderByDescending_FullTextScore()
    {
        await using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderByDescending(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
                .ToListAsync());

        // TODO: add message validation once it's baked and stored as resource string.
    }

    [ConditionalFact]
    public virtual async Task OrderBy_scoring_function_overridden_by_another()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin", "first" }))
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin", "second" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["beaver","dolphin","second"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderBy_scoring_function_overridden_by_regular_OrderBy()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
            .OrderBy(x => x.Name)
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY c["Name"]
""");
    }

    [ConditionalFact]
    public virtual async Task Regular_OrderBy_overridden_by_OrderBy_using_scoring_function()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => x.Name)
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["beaver","dolphin"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderBy_scoring_function_ThenBy_scoring_function()
    {
        await using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin", "first" }))
                .ThenBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin", "second" }))
                .ToListAsync());

        // TODO: add message validation once it's baked and stored as resource string.
    }

    [ConditionalFact]
    public virtual async Task OrderBy_scoring_function_ThenBy_regular()
    {
        await using var context = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
                .ThenBy(x => x.Name)
                .ToListAsync());

        // TODO: add message validation once it's baked and stored as resource string.
    }

    [ConditionalFact]
    public virtual async Task OrderBy_regular_ThenBy_scoring_function()
    {
        await using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => x.Name)
                .ThenBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
                .ToListAsync());

        // TODO: add message validation once it's baked and stored as resource string.
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_Where()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
            .Where(x => x.PartitionKey + "Foo" == "habitatFoo")
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE ((c["PartitionKey"] || "Foo") = "habitatFoo")
ORDER BY RANK FullTextScore(c["Description"], ["beaver","dolphin"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_Distinct()
    {
        await using var context = CreateContext();

        var message = (await Assert.ThrowsAsync<CosmosException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }))
                .Distinct()
                .ToListAsync())).Message;

        Assert.Contains(
            "The DISTINCT keyword is not allowed with the ORDER BY RANK clause.",
            message);
    }

    private class FullTextSearchAnimals
    {
        public int Id { get; set; }

        public string PartitionKey { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FullTextSearchFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "FullTextSearchTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<FullTextSearchAnimals>(b =>
            {
                b.ToContainer("FullTextSearchAnimals");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).IsFullText();
                b.HasIndex(x => x.Name).ForFullText();

                b.Property(x => x.Description).IsFullText();
                b.HasIndex(x => x.Description).ForFullText();
            });
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var landAnimals = new FullTextSearchAnimals
            {
                Id = 1,
                PartitionKey = "habitat",
                Name = "List of several land animals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
            };

            var waterAnimals = new FullTextSearchAnimals
            {
                Id = 2,
                PartitionKey = "habitat",
                Name = "List of several water animals",
                Description = "beaver, otter, duck, dolphin, salmon, turtle, frog",
            };

            var airAnimals = new FullTextSearchAnimals
            {
                Id = 3,
                PartitionKey = "habitat",
                Name = "List of several air animals",
                Description = "duck, bat, eagle, butterfly, sparrow",
            };

            var mammals = new FullTextSearchAnimals
            {
                Id = 4,
                PartitionKey = "taxonomy",
                Name = "List of several mammals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
            };

            var avians = new FullTextSearchAnimals
            {
                Id = 5,
                PartitionKey = "taxonomy",
                Name = "List of several avians",
                Description = "duck, eagle, sparrow",
            };

            context.Set<FullTextSearchAnimals>().AddRange(landAnimals, waterAnimals, airAnimals, mammals, avians);
            return context.SaveChangesAsync();
        }

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
#pragma warning restore EF9104
