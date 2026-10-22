// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class CompositeKeyEndToEndInMemoryTest(CompositeKeyEndToEndInMemoryTest.CompositeKeyEndToEndInMemoryFixture fixture)
    : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndInMemoryTest.CompositeKeyEndToEndInMemoryFixture>(fixture)
{
    public class CompositeKeyEndToEndInMemoryFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
