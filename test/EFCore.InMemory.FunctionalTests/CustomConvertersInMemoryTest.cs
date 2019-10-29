// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CustomConvertersInMemoryTest : CustomConvertersTestBase<CustomConvertersInMemoryTest.CustomConvertersInMemoryFixture>
    {
        public CustomConvertersInMemoryTest(CustomConvertersInMemoryFixture fixture)
            : base(fixture)
        {
        }

        // Disabled: In-memory database is case-sensitive
        public override void Can_insert_and_read_back_with_case_insensitive_string_key()
        {
        }

        [ConditionalTheory(Skip = "Issue#14042")]
        public override Task Can_query_custom_type_not_mapped_by_default_equality(bool isAsync)
        {
            return base.Can_query_custom_type_not_mapped_by_default_equality(isAsync);
        }

        [ConditionalFact(Skip = "Issue#17050")]
        public override void Value_conversion_with_property_named_value()
        {
        }

        [ConditionalFact(Skip = "Issue#17050")]
        public override void Collection_property_as_scalar()
        {
            base.Collection_property_as_scalar();
        }

        public class CustomConvertersInMemoryFixture : CustomConvertersFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => false;

            public override bool SupportsDecimalComparisons => true;

            public override DateTime DefaultDateTime => new DateTime();
        }
    }
}
