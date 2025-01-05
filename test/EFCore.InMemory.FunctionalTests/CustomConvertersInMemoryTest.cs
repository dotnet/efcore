// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore;

public class CustomConvertersInMemoryTest(CustomConvertersInMemoryTest.CustomConvertersInMemoryFixture fixture)
    : CustomConvertersTestBase<CustomConvertersInMemoryTest.CustomConvertersInMemoryFixture>(fixture)
{
    public override Task Optional_datetime_reading_null_from_database()
        => Task.CompletedTask;

    // Disabled: In-memory database is case-sensitive
    public override Task Can_insert_and_read_back_with_case_insensitive_string_key()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Issue#17050")]
    public override void Value_conversion_with_property_named_value()
    {
    }

    [ConditionalFact(Skip = "Issue#17050")]
    public override void Collection_property_as_scalar_Any()
        => base.Collection_property_as_scalar_Any();

    [ConditionalFact(Skip = "Issue#17050")]
    public override void Collection_property_as_scalar_Count_member()
        => base.Collection_property_as_scalar_Count_member();

    [ConditionalFact(Skip = "Issue#17050")]
    public override void Collection_enum_as_string_Contains()
        => base.Collection_enum_as_string_Contains();

    public override void GroupBy_converted_enum()
        => Assert.Contains(
            CoreStrings.TranslationFailedWithDetails("", InMemoryStrings.NonComposedGroupByNotSupported)[21..],
            Assert.Throws<InvalidOperationException>(() => base.GroupBy_converted_enum()).Message);

    public class CustomConvertersInMemoryFixture : CustomConvertersFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => false;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => false;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => true;
    }
}
