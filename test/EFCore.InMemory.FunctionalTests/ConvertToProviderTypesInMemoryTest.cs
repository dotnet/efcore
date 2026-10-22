// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ConvertToProviderTypesInMemoryTest(ConvertToProviderTypesInMemoryTest.ConvertToProviderTypesInMemoryFixture fixture)
    : ConvertToProviderTypesTestBase<
        ConvertToProviderTypesInMemoryTest.ConvertToProviderTypesInMemoryFixture>(fixture)
{
    public override Task Optional_datetime_reading_null_from_database()
        => Task.CompletedTask;

    public class ConvertToProviderTypesInMemoryFixture : ConvertToProviderTypesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => false;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

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
