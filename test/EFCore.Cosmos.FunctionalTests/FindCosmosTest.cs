// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FindCosmosTest : FindTestBase<FindCosmosTest.FindCosmosFixture>
{
    protected FindCosmosTest(FindCosmosFixture fixture)
        : base(fixture)
        => fixture.TestSqlLoggerFactory.Clear();

    [ConditionalFact(Skip = "#25886")]
    public override void Find_base_type_using_derived_set_tracked() { }

    [ConditionalTheory(Skip = "#25886")]
    public override Task Find_base_type_using_derived_set_tracked_async(CancellationType cancellationType)
        => Task.CompletedTask;

    [ConditionalFact(Skip = "#25886")]
    public override void Find_derived_using_base_set_type_from_store() { }

    [ConditionalTheory(Skip = "#25886")]
    public override Task Find_derived_using_base_set_type_from_store_async(CancellationType cancellationType)
        => Task.CompletedTask;

    public override void Find_int_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_int_key_from_store());

    public override void Returns_null_for_int_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_int_key_not_in_store());

    public override void Find_nullable_int_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_nullable_int_key_from_store());

    public override void Returns_null_for_nullable_int_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_nullable_int_key_not_in_store());

    public override void Find_string_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_string_key_from_store());

    public override void Returns_null_for_string_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_string_key_not_in_store());

    public override void Find_composite_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_composite_key_from_store());

    public override void Returns_null_for_composite_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_composite_key_not_in_store());

    public override void Find_base_type_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_base_type_from_store());

    public override void Returns_null_for_base_type_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_base_type_not_in_store());

    public override void Find_derived_type_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_derived_type_from_store());

    public override void Returns_null_for_derived_type_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_derived_type_not_in_store());

    public override void Find_base_type_using_derived_set_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_base_type_using_derived_set_from_store());

    public override void Find_shadow_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_shadow_key_from_store());

    public override void Returns_null_for_shadow_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_shadow_key_not_in_store());

    public override void Find_int_key_tracked()
    {
        base.Find_int_key_tracked();

        AssertSql();
    }

    public override void Find_nullable_int_key_tracked()
    {
        base.Find_nullable_int_key_tracked();

        AssertSql();
    }

    public override void Find_string_key_tracked()
    {
        base.Find_string_key_tracked();

        AssertSql();
    }

    public override void Find_composite_key_tracked()
    {
        base.Find_composite_key_tracked();

        AssertSql();
    }

    public override void Find_base_type_tracked()
    {
        base.Find_base_type_tracked();

        AssertSql();
    }

    public override void Find_derived_type_tracked()
    {
        base.Find_derived_type_tracked();

        AssertSql();
    }

    public override void Find_derived_type_using_base_set_tracked()
    {
        base.Find_derived_type_using_base_set_tracked();

        AssertSql();
    }

    public override void Find_shadow_key_tracked()
    {
        base.Find_shadow_key_tracked();

        AssertSql();
    }

    public override void Returns_null_for_null_key_values_array()
    {
        base.Returns_null_for_null_key_values_array();

        AssertSql();
    }

    public override void Returns_null_for_null_key()
    {
        base.Returns_null_for_null_key();

        AssertSql();
    }

    public override void Returns_null_for_null_nullable_key()
    {
        base.Returns_null_for_null_nullable_key();

        AssertSql();
    }

    public override void Returns_null_for_null_in_composite_key()
    {
        base.Returns_null_for_null_in_composite_key();

        AssertSql();
    }

    public override void Throws_for_multiple_values_passed_for_simple_key()
    {
        base.Throws_for_multiple_values_passed_for_simple_key();

        AssertSql();
    }

    public override void Throws_for_wrong_number_of_values_for_composite_key()
    {
        base.Throws_for_wrong_number_of_values_for_composite_key();

        AssertSql();
    }

    public override void Throws_for_bad_type_for_simple_key()
    {
        base.Throws_for_bad_type_for_simple_key();

        AssertSql();
    }

    public override void Throws_for_bad_type_for_composite_key()
    {
        base.Throws_for_bad_type_for_composite_key();

        AssertSql();
    }

    public override void Throws_for_bad_entity_type()
    {
        base.Throws_for_bad_entity_type();

        AssertSql();
    }

    public override void Throws_for_bad_entity_type_with_different_namespace()
    {
        base.Throws_for_bad_entity_type_with_different_namespace();

        AssertSql();
    }

    public override async Task Find_int_key_tracked_async(CancellationType cancellationType)
    {
        await base.Find_int_key_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_int_key_from_store_async(CancellationType cancellationType)
    {
        await base.Find_int_key_from_store_async(cancellationType);

        AssertSql("ReadItem(None, IntKey|77)");
    }

    public override async Task Returns_null_for_int_key_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_int_key_not_in_store_async(cancellationType);

        AssertSql("ReadItem(None, IntKey|99)");
    }

    public override async Task Find_nullable_int_key_tracked_async(CancellationType cancellationType)
    {
        await base.Find_nullable_int_key_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_nullable_int_key_from_store_async(CancellationType cancellationType)
    {
        await base.Find_nullable_int_key_from_store_async(cancellationType);

        AssertSql("ReadItem(None, NullableIntKey|77)");
    }

    public override async Task Returns_null_for_nullable_int_key_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_nullable_int_key_not_in_store_async(cancellationType);

        AssertSql("ReadItem(None, NullableIntKey|99)");
    }

    public override async Task Find_string_key_tracked_async(CancellationType cancellationType)
    {
        await base.Find_string_key_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_string_key_from_store_async(CancellationType cancellationType)
    {
        await base.Find_string_key_from_store_async(cancellationType);

        AssertSql("ReadItem(None, Cat)");
    }

    public override async Task Returns_null_for_string_key_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_string_key_not_in_store_async(cancellationType);

        AssertSql("ReadItem(None, Fox)");
    }

    public override async Task Find_composite_key_tracked_async(CancellationType cancellationType)
    {
        await base.Find_composite_key_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_composite_key_from_store_async(CancellationType cancellationType)
    {
        await base.Find_composite_key_from_store_async(cancellationType);

        AssertSql("ReadItem(None, 77|Dog)");
    }

    public override async Task Returns_null_for_composite_key_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_composite_key_not_in_store_async(cancellationType);

        AssertSql("ReadItem(None, 77|Fox)");
    }

    public override async Task Find_base_type_tracked_async(CancellationType cancellationType)
    {
        await base.Find_base_type_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_base_type_from_store_async(CancellationType cancellationType)
    {
        await base.Find_base_type_from_store_async(cancellationType);

        AssertSql("""ReadItem(None, BaseType|77)""");
    }

    public override async Task Returns_null_for_base_type_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_base_type_not_in_store_async(cancellationType);

        AssertSql("""ReadItem(None, BaseType|99)""");
    }

    public override async Task Find_derived_type_tracked_async(CancellationType cancellationType)
    {
        await base.Find_derived_type_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_derived_type_from_store_async(CancellationType cancellationType)
    {
        await base.Find_derived_type_from_store_async(cancellationType);

        AssertSql("ReadItem(None, BaseType|78)");
    }

    public override async Task Returns_null_for_derived_type_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_derived_type_not_in_store_async(cancellationType);

        AssertSql("ReadItem(None, BaseType|99)");
    }

    public override async Task Find_base_type_using_derived_set_from_store_async(CancellationType cancellationType)
    {
        Assert.Equal(
            CoreStrings.UnableToDiscriminate("DerivedType", "BaseType"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Find_base_type_using_derived_set_from_store_async(cancellationType))).Message);

        AssertSql("ReadItem(None, BaseType|77)");
    }

    public override async Task Find_derived_type_using_base_set_tracked_async(CancellationType cancellationType)
    {
        await base.Find_derived_type_using_base_set_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_shadow_key_tracked_async(CancellationType cancellationType)
    {
        await base.Find_shadow_key_tracked_async(cancellationType);

        AssertSql();
    }

    public override async Task Find_shadow_key_from_store_async(CancellationType cancellationType)
    {
        await base.Find_shadow_key_from_store_async(cancellationType);

        AssertSql("ReadItem(None, 77)");
    }

    public override async Task Returns_null_for_shadow_key_not_in_store_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_shadow_key_not_in_store_async(cancellationType);

        AssertSql("ReadItem(None, 99)");
    }

    public override async Task Returns_null_for_null_key_values_array_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_null_key_values_array_async(cancellationType);

        AssertSql();
    }

    public override async Task Returns_null_for_null_key_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_null_key_async(cancellationType);

        AssertSql();
    }

    public override async Task Returns_null_for_null_in_composite_key_async(CancellationType cancellationType)
    {
        await base.Returns_null_for_null_in_composite_key_async(cancellationType);

        AssertSql();
    }

    public override async Task Throws_for_multiple_values_passed_for_simple_key_async(CancellationType cancellationType)
    {
        await base.Throws_for_multiple_values_passed_for_simple_key_async(cancellationType);

        AssertSql();
    }

    public override async Task Throws_for_wrong_number_of_values_for_composite_key_async(CancellationType cancellationType)
    {
        await base.Throws_for_wrong_number_of_values_for_composite_key_async(cancellationType);

        AssertSql();
    }

    public override async Task Throws_for_bad_type_for_simple_key_async(CancellationType cancellationType)
    {
        await base.Throws_for_bad_type_for_simple_key_async(cancellationType);

        AssertSql();
    }

    public override async Task Throws_for_bad_type_for_composite_key_async(CancellationType cancellationType)
    {
        await base.Throws_for_bad_type_for_composite_key_async(cancellationType);

        AssertSql();
    }

    public override async Task Throws_for_bad_entity_type_async(CancellationType cancellationType)
    {
        await base.Throws_for_bad_entity_type_async(cancellationType);

        AssertSql();
    }

    public override async Task Throws_for_bad_entity_type_with_different_namespace_async(CancellationType cancellationType)
    {
        await base.Throws_for_bad_entity_type_with_different_namespace_async(cancellationType);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class FindCosmosTestSet(FindCosmosFixture fixture) : FindCosmosTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindCosmosTestContext(FindCosmosFixture fixture) : FindCosmosTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindCosmosTestNonGeneric(FindCosmosFixture fixture) : FindCosmosTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindCosmosFixture : FindFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<IntKey>()
                .ToContainer("Ints")
                .HasRootDiscriminatorInJsonId();

            modelBuilder.Entity<NullableIntKey>()
                .ToContainer("Ints")
                .HasRootDiscriminatorInJsonId();

            modelBuilder.Entity<StringKey>()
                .ToContainer("Strings");

            modelBuilder.Entity<CompositeKey>()
                .ToContainer("CompositeKeys");

            modelBuilder.Entity<BaseType>()
                .ToContainer("Base")
                .HasRootDiscriminatorInJsonId();

            modelBuilder.Entity<ShadowKey>().ToContainer("ShadowKeys");
        }
    }
}
