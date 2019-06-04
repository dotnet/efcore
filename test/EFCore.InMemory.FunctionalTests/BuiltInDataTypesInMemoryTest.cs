// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class BuiltInDataTypesInMemoryTest : BuiltInDataTypesTestBase<BuiltInDataTypesInMemoryTest.BuiltInDataTypesInMemoryFixture>
    {
        public BuiltInDataTypesInMemoryTest(BuiltInDataTypesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Can_insert_and_read_back_with_string_key()
        {
            base.Can_insert_and_read_back_with_string_key();
        }

        public class BuiltInDataTypesInMemoryFixture : BuiltInDataTypesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override bool StrictEquality => true;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsBinaryKeys => false;

            public override bool SupportsDecimalComparisons => true;

            public override DateTime DefaultDateTime => new DateTime();
        }
    }
}
