// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
