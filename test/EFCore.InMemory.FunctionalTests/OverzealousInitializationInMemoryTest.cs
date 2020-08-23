// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class OverzealousInitializationInMemoryTest
        : OverzealousInitializationTestBase<OverzealousInitializationInMemoryTest.OverzealousInitializationInMemoryFixture>
    {
        public OverzealousInitializationInMemoryTest(OverzealousInitializationInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class OverzealousInitializationInMemoryFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;
        }
    }
}
