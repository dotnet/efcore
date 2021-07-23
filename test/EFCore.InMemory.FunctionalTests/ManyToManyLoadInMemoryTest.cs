// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyLoadInMemoryTest : ManyToManyLoadTestBase<ManyToManyLoadInMemoryTest.ManyToManyLoadInMemoryFixture>
    {
        public ManyToManyLoadInMemoryTest(ManyToManyLoadInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyLoadInMemoryFixture : ManyToManyLoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;
        }
    }
}
