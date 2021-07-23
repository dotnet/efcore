// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class KeysWithConvertersCosmosTest : KeysWithConvertersTestBase<KeysWithConvertersCosmosTest.KeysWithConvertersCosmosFixture>
    {
        public KeysWithConvertersCosmosTest(KeysWithConvertersCosmosFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_struct_key_and_optional_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_struct_key_and_required_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_class_key_and_optional_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents()
        {
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents()
        {
            base.Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents()
        {
            base.Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents()
        {
            base.Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents()
        {
            base.Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents()
        {
            base.Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents()
        {
            base.Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
        {
            base.Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents();
        }

        [ConditionalFact(Skip = "Issue=#16920 (Include)")]
        public override void Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
        {
            base.Can_insert_and_read_back_with_struct_binary_key_and_required_dependents();
        }

        public class KeysWithConvertersCosmosFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => CosmosTestStoreFactory.Instance;
        }
    }
}
