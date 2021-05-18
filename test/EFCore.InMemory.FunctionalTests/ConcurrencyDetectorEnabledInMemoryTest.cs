// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConcurrencyDetectorEnabledInMemoryTest : ConcurrencyDetectorEnabledTestBase<
        ConcurrencyDetectorEnabledInMemoryTest.ConcurrencyDetectorInMemoryFixture>
    {
        public ConcurrencyDetectorEnabledInMemoryTest(ConcurrencyDetectorInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class ConcurrencyDetectorInMemoryFixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;
        }
    }
}
