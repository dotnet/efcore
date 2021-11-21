// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class GrpcInMemoryTest : GrpcTestBase<GrpcInMemoryTest.GrpcInMemoryFixture>
{
    public GrpcInMemoryTest(GrpcInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class GrpcInMemoryFixture : GrpcFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
