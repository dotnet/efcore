// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class IncludeOneToOneInMemoryTest(IncludeOneToOneInMemoryTest.OneToOneQueryInMemoryFixture fixture) : IncludeOneToOneTestBase<IncludeOneToOneInMemoryTest.OneToOneQueryInMemoryFixture>(fixture)
{
    public class OneToOneQueryInMemoryFixture : OneToOneQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
