// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#if !EXCLUDE_ON_MAC

public class GrpcInMemoryTest(GrpcInMemoryTest.GrpcInMemoryFixture fixture) : GrpcTestBase<GrpcInMemoryTest.GrpcInMemoryFixture>(fixture)
{
    public class GrpcInMemoryFixture : GrpcFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}

#endif
