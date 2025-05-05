// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

[CosmosCondition(CosmosCondition.DoesNotUseTokenCredential)]
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
    public virtual async Task Use_FullTextContainsAny_constant_in_predicate()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContainsAny(x.Description, "bat"))
            .ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.True(result.All(x => x.Description.Contains("bat")));

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContainsAny(c["Description"], "bat")
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContainsAny_constant_array_in_predicate()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContainsAny(x.Description, new[] { "bat", "beaver" }))
            .ToListAsync();

        Assert.Equal(4, result.Count);
        Assert.True(result.All(x => x.Description.Contains("bat") || x.Description.Contains("beaver")));

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContainsAny(c["Description"], "bat", "beaver")
""");
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContainsAny_mixed_in_predicate()
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
    public virtual async Task Use_FullTextContainsAll_in_predicate_parameter()
    {
        await using var context = CreateContext();

        var beaver = "beaver";
        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContainsAll(x.Description, beaver))
            .ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver")));

        AssertSql(
"""
@beaver='beaver'

SELECT VALUE c
FROM root c
WHERE FullTextContainsAll(c["Description"], @beaver)
""");
    }


    [ConditionalFact]
    public virtual async Task Use_FullTextContainsAll_in_predicate_with_parameterized_keyword_list()
    {
        await using var context = CreateContext();

        var keywords = new string[] { "beaver", "salmon", "frog" };
        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContainsAll(x.Description, keywords))
            .ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver") && x.Description.Contains("salmon") && x.Description.Contains("frog")));

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContainsAll(c["Description"], "beaver", "salmon", "frog")
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
    public virtual async Task OrderByRank_FullTextScore_constant()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, "otter"))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["otter"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_constants()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, "otter", "beaver"))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["otter","beaver"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_constant_array()
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
    public virtual async Task OrderByRank_FullTextScore_constant_array_with_one_element()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "otter" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["otter"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_parameter_array()
    {
        await using var context = CreateContext();

        var prm = new string[] { "otter", "beaver" };
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, prm))
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
    public virtual async Task OrderByRank_FullTextScore_using_one_parameter()
    {
        await using var context = CreateContext();

        var otter = "otter";

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, otter))
            .ToListAsync();

        AssertSql(
"""
@otter='otter'

SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], [@otter])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_using_parameters_constant_mix()
    {
        await using var context = CreateContext();

        var beaver = "beaver";

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "otter", beaver }))
            .ToListAsync();

        AssertSql(
"""
@beaver='beaver'

SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["otter", @beaver])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_FullTextScore_using_parameter()
    {
        await using var context = CreateContext();

        var otter = "otter";

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, otter))
            .ToListAsync();

        AssertSql(
"""
@otter='otter'

SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], [@otter])
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
    public virtual async Task OrderByRank_FullTextScore_on_non_FTS_property()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.PartitionKey, new string[] { "taxonomy" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["PartitionKey"], ["taxonomy"])
""");
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

    [ConditionalFact]
    public virtual async Task OrderByRank_Take()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }))
            .Take(10)
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["beaver"])
OFFSET 0 LIMIT 10
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_Skip_Take()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
            .Skip(1)
            .Take(20)
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Description"], ["beaver","dolphin"])
OFFSET 1 LIMIT 20
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByDescending_FullTextScore()
    {
        await using var context = CreateContext();
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderByDescending(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
                .ToListAsync())).Message;

        Assert.Equal(CosmosStrings.OrderByDescendingScoringFunction("OrderByDescending", "OrderBy"), message);
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
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin", "first" }))
                .ThenBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin", "second" }))
                .ToListAsync())).Message;

        Assert.Equal(CosmosStrings.OrderByMultipleScoringFunctionWithoutRrf("Rrf"), message);
    }

    [ConditionalFact]
    public virtual async Task OrderBy_scoring_function_ThenBy_regular()
    {
        await using var context = CreateContext();

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
                .ThenBy(x => x.Name)
                .ToListAsync())).Message;

        Assert.Equal(CosmosStrings.OrderByScoringFunctionMixedWithRegularOrderby, message);
    }

    [ConditionalFact]
    public virtual async Task OrderBy_regular_ThenBy_scoring_function()
    {
        await using var context = CreateContext();
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<FullTextSearchAnimals>()
                .OrderBy(x => x.Name)
                .ThenBy(x => EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "dolphin" }))
                .ToListAsync())).Message;

        Assert.Equal(CosmosStrings.OrderByScoringFunctionMixedWithRegularOrderby, message);
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
                .Select(x => x.Name)
                .Distinct()
                .ToListAsync())).Message;

        Assert.Contains(
            "The DISTINCT keyword is not allowed with the ORDER BY RANK clause.",
            message);
    }

    [ConditionalFact]
    public virtual async Task Use_FullTextContains_in_predicate_on_nested_owned_type()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContains(x.Owned.NestedReference.AnotherDescription, "beaver"))
            .ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver")));

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContains(c["Owned"]["NestedReference"]["AnotherDescription"], "beaver")
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_with_FullTextScore_on_nested_owned_type()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Owned.NestedReference.AnotherDescription, new string[] { "beaver", "dolphin" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Owned"]["NestedReference"]["AnotherDescription"], ["beaver","dolphin"])
""");
    }

    [ConditionalFact(Skip = "issue #35898")]
    public virtual async Task Use_FullTextContains_in_predicate_on_nested_owned_collection_element()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .Where(x => EF.Functions.FullTextContains(x.Owned.NestedCollection[0].AnotherDescription, "beaver"))
            .ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result.All(x => x.Description.Contains("beaver")));

        AssertSql(
"""
SELECT VALUE c
FROM root c
WHERE FullTextContains(c["Owned"]["NestedCollection"][0]["AnotherDescription"], "beaver")
""");
    }

    [ConditionalFact(Skip = "issue #35898")]
    public virtual async Task OrderByRank_with_FullTextScore_on_nested_owned_collection_element()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Owned.NestedCollection[0].AnotherDescription, new string[] { "beaver", "dolphin" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Owned"]["NestedCollection"][0]["AnotherDescription"], ["beaver","dolphin"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderBy_scoring_function_on_property_with_modified_json_name()
    {
        await using var context = CreateContext();
        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.ModifiedDescription, new string[] { "beaver", "dolphin" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["CustomDecription"], ["beaver","dolphin"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_with_FullTextScore_on_nested_owned_type_with_modified_json_name()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.Owned.ModifiedNestedReference.AnotherDescription, new string[] { "beaver", "dolphin" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["Owned"]["CustomNestedReference"]["AnotherDescription"], ["beaver","dolphin"])
""");
    }

    [ConditionalFact]
    public virtual async Task OrderByRank_with_FullTextScore_on_property_without_index()
    {
        await using var context = CreateContext();

        var result = await context.Set<FullTextSearchAnimals>()
            .OrderBy(x => EF.Functions.FullTextScore(x.DescriptionNoIndex, new string[] { "beaver", "dolphin" }))
            .ToListAsync();

        AssertSql(
"""
SELECT VALUE c
FROM root c
ORDER BY RANK FullTextScore(c["DescriptionNoIndex"], ["beaver","dolphin"])
""");
    }

    private class FullTextSearchAnimals
    {
        public int Id { get; set; }

        public string PartitionKey { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string ModifiedDescription { get; set; } = null!;
        public string DescriptionNoIndex { get; set; } = null!;

        public FullTextSearchOwned Owned { get; set; } = null!;
    }

    private class FullTextSearchOwned
    {
        public FullTextSearchNested NestedReference { get; set; } = null!;
        public FullTextSearchNested ModifiedNestedReference { get; set; } = null!;

        public List<FullTextSearchNested> NestedCollection { get; set; } = null!;
    }

    private class FullTextSearchNested
    {
        public string AnotherDescription { get; set; } = null!;
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
            => modelBuilder.Entity<FullTextSearchAnimals>(b =>
            {
                b.ToContainer("FullTextSearchAnimals");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();

                b.Property(x => x.Description).EnableFullTextSearch();
                b.HasIndex(x => x.Description).IsFullTextIndex();

                b.Property(x => x.ModifiedDescription).ToJsonProperty("CustomDecription");
                b.Property(x => x.ModifiedDescription).EnableFullTextSearch();
                b.HasIndex(x => x.ModifiedDescription).IsFullTextIndex();

                b.Property(x => x.DescriptionNoIndex).EnableFullTextSearch();

                b.OwnsOne(x => x.Owned, bb =>
                {
                    bb.OwnsOne(x => x.NestedReference, bbb =>
                    {
                        bbb.Property(x => x.AnotherDescription).EnableFullTextSearch();
                        bbb.HasIndex(x => x.AnotherDescription).IsFullTextIndex();
                    });

                    bb.OwnsOne(x => x.ModifiedNestedReference, bbb =>
                    {
                        bbb.ToJsonProperty("CustomNestedReference");
                        bbb.Property(x => x.AnotherDescription).EnableFullTextSearch();
                        bbb.HasIndex(x => x.AnotherDescription).IsFullTextIndex();
                    });

                    // issue #35898
                    //bb.OwnsMany(x => x.NestedCollection, bbb =>
                    //{
                    //    bbb.Property(x => x.AnotherDescription).EnableFullTextSearch();
                    //    bbb.HasIndex(x => x.AnotherDescription).IsFullTextIndex();
                    //});
                });
            });

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var landAnimals = new FullTextSearchAnimals
            {
                Id = 1,
                PartitionKey = "habitat",
                Name = "List of several land animals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                ModifiedDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                DescriptionNoIndex = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                Owned = new FullTextSearchOwned
                {
                    NestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                    },
                    ModifiedNestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                    },
                    // issue #35898
                    //NestedCollection =
                    //[
                    //    new FullTextSearchNested
                    //    {
                    //        AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                    //    }
                    //]
                }
            };

            var waterAnimals = new FullTextSearchAnimals
            {
                Id = 2,
                PartitionKey = "habitat",
                Name = "List of several water animals",
                Description = "beaver, otter, duck, dolphin, salmon, turtle, frog",
                ModifiedDescription = "beaver, otter, duck, dolphin, salmon, turtle, frog",
                DescriptionNoIndex = "beaver, otter, duck, dolphin, salmon, turtle, frog",
                Owned = new FullTextSearchOwned
                {
                    NestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "beaver, otter, duck, dolphin, salmon, turtle, frog",
                    },
                    ModifiedNestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "beaver, otter, duck, dolphin, salmon, turtle, frog",
                    },
                    // issue #35898
                    //NestedCollection =
                    //[
                    //    new FullTextSearchNested
                    //    {
                    //        AnotherDescription = "beaver, otter, duck, dolphin, salmon, turtle, frog",
                    //    }
                    //]
                }
            };

            var airAnimals = new FullTextSearchAnimals
            {
                Id = 3,
                PartitionKey = "habitat",
                Name = "List of several air animals",
                Description = "duck, bat, eagle, butterfly, sparrow",
                ModifiedDescription = "duck, bat, eagle, butterfly, sparrow",
                DescriptionNoIndex = "duck, bat, eagle, butterfly, sparrow",
                Owned = new FullTextSearchOwned
                {
                    NestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "duck, bat, eagle, butterfly, sparrow",
                    },
                    ModifiedNestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "duck, bat, eagle, butterfly, sparrow",
                    },
                    // issue #35898
                    //NestedCollection =
                    //[
                    //    new FullTextSearchNested
                    //    {
                    //        AnotherDescription = "duck, bat, eagle, butterfly, sparrow",
                    //    }
                    //]
                }
            };

            var mammals = new FullTextSearchAnimals
            {
                Id = 4,
                PartitionKey = "taxonomy",
                Name = "List of several mammals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                ModifiedDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                DescriptionNoIndex = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                Owned = new FullTextSearchOwned
                {
                    NestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                    },
                    ModifiedNestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                    },
                    // issue #35898
                    //NestedCollection =
                    //[
                    //    new FullTextSearchNested
                    //    {
                    //        AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                    //    }
                    //]
                }
            };

            var avians = new FullTextSearchAnimals
            {
                Id = 5,
                PartitionKey = "taxonomy",
                Name = "List of several avians",
                Description = "duck, eagle, sparrow",
                ModifiedDescription = "duck, eagle, sparrow",
                DescriptionNoIndex = "duck, eagle, sparrow",
                Owned = new FullTextSearchOwned
                {
                    NestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "duck, eagle, sparrow",
                    },
                    ModifiedNestedReference = new FullTextSearchNested
                    {
                        AnotherDescription = "duck, eagle, sparrow",
                    },
                    // issue #35898
                    //NestedCollection =
                    //[
                    //    new FullTextSearchNested
                    //    {
                    //        AnotherDescription = "duck, eagle, sparrow",
                    //    }
                    //]
                }
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
