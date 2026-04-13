// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQueryCosmosTest(NonSharedFixture fixture) : AdHocComplexTypeQueryTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@entity_equality_container='{"Id":1,"Containee1":{"Id":2},"Containee2":{"Id":3}}'

SELECT VALUE c
FROM root c
WHERE (c["ComplexContainer"] = @entity_equality_container)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Projecting_complex_property_does_not_auto_include_owned_types()
    {
        await base.Projecting_complex_property_does_not_auto_include_owned_types();

        // #34067: Cosmos: Projecting out nested documents retrieves the entire document
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Optional_complex_type_with_discriminator()
    {
        await base.Optional_complex_type_with_discriminator();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["AllOptionalsComplexType"] = null)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Non_optional_complex_type_with_all_nullable_properties()
    {
        await base.Non_optional_complex_type_with_all_nullable_properties();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Non_optional_complex_type_with_all_nullable_properties_via_left_join()
    {
        Assert.Equal(
            CosmosStrings.UpdateConflict("1"),
            (await Assert.ThrowsAsync<DbUpdateException>(
                () => base.Non_optional_complex_type_with_all_nullable_properties_via_left_join())).Message);

        AssertSql();
    }

    public override async Task Nullable_complex_type_with_discriminator_and_shadow_property()
    {
        await base.Nullable_complex_type_with_discriminator_and_shadow_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
       => base.AddNonSharedOptions(builder)
               .ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
