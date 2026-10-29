// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedComplexPropertiesQueryCosmosTest(NonSharedFixture fixture) : OwnedComplexPropertiesQueryTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder).ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override async Task Owned_reference()
    {
        await base.Owned_reference();

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Owned_collection()
    {
        await base.Owned_collection();

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Filter_on_complex_property_of_owned_reference()
    {
        await base.Filter_on_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Owned"]["Address"]["Country"]["Code"] = "DE")
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Project_complex_property_of_owned_reference()
    {
        await base.Project_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT VALUE c["Owned"]["Address"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_nested_complex_property_of_owned_reference()
    {
        await base.Project_nested_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT VALUE c["Owned"]["Address"]["Country"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Owned_reference_on_TPH_owner()
    {
        await base.Owned_reference_on_TPH_owner();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("Owner", "SpecialOwner")
ORDER BY c["Id"]
""",
            //
            """
SELECT VALUE c["DerivedOwned"]["Address"]["City"]
FROM root c
WHERE (c["$type"] IN ("Owner", "SpecialOwner") AND (c["$type"] = "SpecialOwner"))
""");
    }

    public override async Task Optional_owned_reference_with_only_complex_property()
    {
        await base.Optional_owned_reference_with_only_complex_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Update_complex_property_of_owned_reference()
    {
        await base.Update_complex_property_of_owned_reference();

        AssertSql(
            """
ReadItem(None, 1)
""");
    }

    public override async Task Update_complex_property_of_owned_collection_element()
    {
        await base.Update_complex_property_of_owned_collection_element();

        AssertSql(
            """
ReadItem(None, 1)
""");
    }

    public override async Task Replace_complex_property_of_owned_reference()
    {
        await base.Replace_complex_property_of_owned_reference();

        AssertSql(
            """
ReadItem(None, 1)
""");
    }

    public override async Task Replace_owned_reference_with_complex_property()
    {
        await base.Replace_owned_reference_with_complex_property();

        AssertSql(
            """
ReadItem(None, 1)
""");
    }

    public override async Task Delete_owner_with_owned_complex_property()
    {
        await base.Delete_owner_with_owned_complex_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Value_converter_in_complex_type_on_owned_reference()
    {
        // Primitive collections with element converters aren't supported on Cosmos
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Value_converter_in_complex_type_on_owned_reference);

        Assert.Equal(CosmosStrings.ElementWithValueConverter("List<TagId>", "Tag", "RelatedIds", "TagId"), exception.Message);

        AssertSql();
    }

    public override async Task Value_comparer_in_complex_type_on_owned_reference()
    {
        await base.Value_comparer_in_complex_type_on_owned_reference();

        AssertSql(
            """
ReadItem(None, 1)
""",
            //
            """
ReadItem(None, 1)
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
