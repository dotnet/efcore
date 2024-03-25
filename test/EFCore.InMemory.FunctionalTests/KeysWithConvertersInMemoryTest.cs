// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class KeysWithConvertersInMemoryTest(KeysWithConvertersInMemoryTest.KeysWithConvertersInMemoryFixture fixture)
    : KeysWithConvertersTestBase<
        KeysWithConvertersInMemoryTest.KeysWithConvertersInMemoryFixture>(fixture)
{
    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_query_and_update_owned_entity_with_value_converter()
        => base.Can_query_and_update_owned_entity_with_value_converter();

    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_query_and_update_owned_entity_with_int_bare_class_key()
        => base.Can_query_and_update_owned_entity_with_int_bare_class_key();

    [ConditionalFact(Skip = "Issue #26238")]
    public override Task Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents();

    public class KeysWithConvertersInMemoryFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.ConfigureWarnings(w => w.Ignore(CoreEventId.MappedEntityTypeIgnoredWarning)));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // Issue #26238
            modelBuilder.Ignore<EnumerableClassKeyPrincipal>();
            modelBuilder.Ignore<EnumerableClassKeyOptionalDependent>();
            modelBuilder.Ignore<EnumerableClassKeyRequiredDependent>();
        }
    }
}
