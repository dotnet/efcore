// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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
