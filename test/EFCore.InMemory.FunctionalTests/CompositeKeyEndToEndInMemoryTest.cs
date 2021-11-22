// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class CompositeKeyEndToEndInMemoryTest
    : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndInMemoryTest.CompositeKeyEndToEndInMemoryFixture>
{
    public CompositeKeyEndToEndInMemoryTest(CompositeKeyEndToEndInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class CompositeKeyEndToEndInMemoryFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
