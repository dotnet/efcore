// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConvertToProviderTypesInMemoryTest : ConvertToProviderTypesTestBase<
        ConvertToProviderTypesInMemoryTest.ConvertToProviderTypesInMemoryFixture>
    {
        public ConvertToProviderTypesInMemoryTest(ConvertToProviderTypesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class ConvertToProviderTypesInMemoryFixture : ConvertToProviderTypesFixtureBase
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
