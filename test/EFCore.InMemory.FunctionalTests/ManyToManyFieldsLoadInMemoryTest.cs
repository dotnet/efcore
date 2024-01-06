// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyFieldsLoadInMemoryTest(ManyToManyFieldsLoadInMemoryTest.ManyToManyFieldsLoadInMemoryFixture fixture) : ManyToManyFieldsLoadTestBase<
    ManyToManyFieldsLoadInMemoryTest.ManyToManyFieldsLoadInMemoryFixture>(fixture)
{
    public class ManyToManyFieldsLoadInMemoryFixture : ManyToManyFieldsLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
