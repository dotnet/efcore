// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
