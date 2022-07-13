// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class OverzealousInitializationInMemoryTest
    : OverzealousInitializationTestBase<OverzealousInitializationInMemoryTest.OverzealousInitializationInMemoryFixture>
{
    public OverzealousInitializationInMemoryTest(OverzealousInitializationInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class OverzealousInitializationInMemoryFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
