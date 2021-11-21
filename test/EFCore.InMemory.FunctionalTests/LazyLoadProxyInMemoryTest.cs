// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class LazyLoadProxyInMemoryTest : LazyLoadProxyTestBase<LazyLoadProxyInMemoryTest.LoadInMemoryFixture>
{
    public LazyLoadProxyInMemoryTest(LoadInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class LoadInMemoryFixture : LoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
