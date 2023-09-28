// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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
