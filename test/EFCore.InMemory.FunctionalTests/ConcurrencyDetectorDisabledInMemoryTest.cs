// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ConcurrencyDetectorDisabledInMemoryTest(ConcurrencyDetectorDisabledInMemoryTest.ConcurrencyDetectorInMemoryFixture fixture) : ConcurrencyDetectorDisabledTestBase<
    ConcurrencyDetectorDisabledInMemoryTest.ConcurrencyDetectorInMemoryFixture>(fixture)
{
    public class ConcurrencyDetectorInMemoryFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.EnableThreadSafetyChecks(enableChecks: false);
    }
}
