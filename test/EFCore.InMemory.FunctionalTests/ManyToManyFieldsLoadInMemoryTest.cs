// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyFieldsLoadInMemoryTest : ManyToManyFieldsLoadTestBase<
        ManyToManyFieldsLoadInMemoryTest.ManyToManyFieldsLoadInMemoryFixture>
    {
        public ManyToManyFieldsLoadInMemoryTest(ManyToManyFieldsLoadInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyFieldsLoadInMemoryFixture : ManyToManyFieldsLoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;
        }
    }
}
