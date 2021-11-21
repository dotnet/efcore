// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public class IncludeOneToOneInMemoryTest : IncludeOneToOneTestBase<IncludeOneToOneInMemoryTest.OneToOneQueryInMemoryFixture>
{
    public IncludeOneToOneInMemoryTest(OneToOneQueryInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class OneToOneQueryInMemoryFixture : OneToOneQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
