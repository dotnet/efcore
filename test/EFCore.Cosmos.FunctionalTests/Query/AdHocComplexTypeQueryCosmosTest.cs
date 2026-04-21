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
""",
            //
            """
SELECT VALUE c
FROM root c
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

    public override async Task Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw()
    {
        await base.Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        await base.Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip()
    {
        await base.Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip()
    {
        await base.Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_set_to_different_value()
    {
        await base.Nullable_complex_type_with_discriminator_set_to_different_value();
    }

    public override async Task Nullable_complex_type_with_discriminator_set_to_null()
    {
        // On Cosmos, setting the discriminator shadow property to null doesn't affect materialization
        // because the complex property's data is still present in the JSON document.
        var contextFactory = await InitializeNonSharedTest<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }

    }

    public override async Task Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        await base.Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
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
