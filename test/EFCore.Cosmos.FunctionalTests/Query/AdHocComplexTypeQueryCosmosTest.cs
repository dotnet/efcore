// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQueryCosmosTest(NonSharedFixture fixture) : AdHocComplexTypeQueryTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@entity_equality_container='{}'
@entity_equality_entity_equality_container_Containee1='{}'
@entity_equality_entity_equality_container_Containee1_Id='2'
@entity_equality_entity_equality_container_Containee2='{}'
@entity_equality_entity_equality_container_Containee2_Id='3'
@entity_equality_container_Id='1'

SELECT VALUE c
FROM root c
WHERE (((c["ComplexContainer"] = null) AND (@entity_equality_container = null)) OR ((@entity_equality_container != null) AND (((((c["ComplexContainer"]["Containee1"] = null) AND (@entity_equality_entity_equality_container_Containee1 = null)) OR ((@entity_equality_entity_equality_container_Containee1 != null) AND (c["ComplexContainer"]["Containee1"]["Id"] = @entity_equality_entity_equality_container_Containee1_Id))) AND (((c["ComplexContainer"]["Containee2"] = null) AND (@entity_equality_entity_equality_container_Containee2 = null)) OR ((@entity_equality_entity_equality_container_Containee2 != null) AND (c["ComplexContainer"]["Containee2"]["Id"] = @entity_equality_entity_equality_container_Containee2_Id)))) AND (c["ComplexContainer"]["Id"] = @entity_equality_container_Id))))
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

    public override async Task Nullable_complex_type_with_discriminator_and_shadow_property()
    {
        await base.Nullable_complex_type_with_discriminator_and_shadow_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
       => base.AddOptions(builder)
               .ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override Task<ContextFactory<TContext>> InitializeAsync<TContext>(
        Action<ModelBuilder>? onModelCreating = null,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null,
        Action<ModelConfigurationBuilder>? configureConventions = null,
        Func<TContext, Task>? seed = null,
        Func<string, bool>? shouldLogCategory = null,
        Func<TestStore>? createTestStore = null,
        bool usePooling = true,
        bool useServiceProvider = true)
        => base.InitializeAsync(model =>
        {
            onModelCreating?.Invoke(model);
            AdHocCosmosTestHelpers.UseTestAutoIncrementIntIds(model);
        },
            onConfiguring,
            addServices,
            configureConventions,
            seed,
            shouldLogCategory,
            createTestStore,
            usePooling,
            useServiceProvider);
}
