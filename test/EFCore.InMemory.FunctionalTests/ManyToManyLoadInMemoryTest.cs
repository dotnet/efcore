// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
