// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore;

public class KeysWithConvertersInMemoryTest(KeysWithConvertersInMemoryTest.KeysWithConvertersInMemoryFixture fixture)
    : KeysWithConvertersTestBase<
        KeysWithConvertersInMemoryTest.KeysWithConvertersInMemoryFixture>(fixture)
{
    // Value converters of keys are not supported
    [Fact]
    public override Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents());

    // Value converters of keys are not supported
    [Fact]
    public override Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK());

    // Value converters of keys are not supported
    [Fact]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents());

    // Value converters of keys are not supported
    [Fact]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_insert_and_read_back_with_struct_binary_key_and_required_dependents());

    // Value converters of keys are not supported
    [Fact]
    public override Task Can_query_and_update_owned_entity_with_value_converter()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_query_and_update_owned_entity_with_value_converter());

    // Value converters of keys are not supported
    [Fact]
    public override Task Can_query_and_update_owned_entity_with_int_bare_class_key()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_query_and_update_owned_entity_with_int_bare_class_key());

    // Value converters of keys are not supported
    [Fact]
    public override Task Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents()
        => Assert.ThrowsAnyAsync<XunitException>(() =>
            base.Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents());

    public class KeysWithConvertersInMemoryFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.ConfigureWarnings(w => w.Ignore(CoreEventId.MappedEntityTypeIgnoredWarning)));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // Value converters of keys are not supported
            modelBuilder.Ignore<EnumerableClassKeyPrincipal>();
            modelBuilder.Ignore<EnumerableClassKeyOptionalDependent>();
            modelBuilder.Ignore<EnumerableClassKeyRequiredDependent>();
        }
    }
}
