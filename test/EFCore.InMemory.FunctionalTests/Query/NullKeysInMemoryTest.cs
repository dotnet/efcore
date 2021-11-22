// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NullKeysInMemoryTest : NullKeysTestBase<NullKeysInMemoryTest.NullKeysInMemoryFixture>
{
    public NullKeysInMemoryTest(NullKeysInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class NullKeysInMemoryFixture : NullKeysFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
