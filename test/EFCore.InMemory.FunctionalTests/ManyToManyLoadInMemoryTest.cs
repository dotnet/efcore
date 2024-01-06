// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyLoadInMemoryTest(ManyToManyLoadInMemoryTest.ManyToManyLoadInMemoryFixture fixture) : ManyToManyLoadTestBase<ManyToManyLoadInMemoryTest.ManyToManyLoadInMemoryFixture>(fixture)
{
    public class ManyToManyLoadInMemoryFixture : ManyToManyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
