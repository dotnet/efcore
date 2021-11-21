// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class ConcurrencyDetectorDisabledInMemoryTest : ConcurrencyDetectorDisabledTestBase<
    ConcurrencyDetectorDisabledInMemoryTest.ConcurrencyDetectorInMemoryFixture>
{
    public ConcurrencyDetectorDisabledInMemoryTest(ConcurrencyDetectorInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class ConcurrencyDetectorInMemoryFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.EnableThreadSafetyChecks(enableChecks: false);
    }
}
