// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CustomConvertersSqliteTest : CustomConvertersTestBase<CustomConvertersSqliteTest.CustomConvertersSqliteFixture>
    {
        public CustomConvertersSqliteTest(CustomConvertersSqliteFixture fixture)
            : base(fixture)
        {
        }

        // Disabled: SQLite database is case-sensitive
        public override void Can_insert_and_read_back_with_case_insensitive_string_key()
        {
        }

        [ConditionalFact(Skip = "Issue#18147")]
        public override void Value_conversion_is_appropriately_used_for_join_condition()
        {
            base.Value_conversion_is_appropriately_used_for_join_condition();
        }

        [ConditionalFact(Skip = "Issue#18147")]
        public override void Value_conversion_is_appropriately_used_for_left_join_condition()
        {
            base.Value_conversion_is_appropriately_used_for_left_join_condition();
        }

        [ConditionalFact(Skip = "Issue#18147")]
        public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
        {
            base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used();
        }

        public class CustomConvertersSqliteFixture : CustomConvertersFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsDecimalComparisons => false;

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }
    }
}
