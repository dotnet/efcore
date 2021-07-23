// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class BuiltInDataTypesInMemoryTest : BuiltInDataTypesTestBase<BuiltInDataTypesInMemoryTest.BuiltInDataTypesInMemoryFixture>
    {
        public BuiltInDataTypesInMemoryTest(BuiltInDataTypesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public override void Optional_datetime_reading_null_from_database()
        {
        }

        public class BuiltInDataTypesInMemoryFixture : BuiltInDataTypesFixtureBase
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
        }
    }
}
