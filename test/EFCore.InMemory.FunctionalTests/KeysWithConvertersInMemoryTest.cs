// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class KeysWithConvertersInMemoryTest : KeysWithConvertersTestBase<
    KeysWithConvertersInMemoryTest.KeysWithConvertersInMemoryFixture>
{
    public KeysWithConvertersInMemoryTest(KeysWithConvertersInMemoryFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact(Skip = "Issue #26238")]
    public override void Can_insert_and_read_back_with_bare_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue #26238")]
    public override void Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue #26238")]
    public override void Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue #26238")]
    public override void Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue #26238")]
    public override void Can_query_and_update_owned_entity_with_value_converter()
        => base.Can_query_and_update_owned_entity_with_value_converter();

    [ConditionalFact(Skip = "Issue #26238")]
    public override void Can_query_and_update_owned_entity_with_int_bare_class_key()
        => base.Can_query_and_update_owned_entity_with_int_bare_class_key();

    public class KeysWithConvertersInMemoryFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
