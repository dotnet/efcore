// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class LoadInMemoryTest(LoadInMemoryTest.LoadInMemoryFixture fixture) : LoadTestBase<LoadInMemoryTest.LoadInMemoryFixture>(fixture)
{
    public class LoadInMemoryFixture : LoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
