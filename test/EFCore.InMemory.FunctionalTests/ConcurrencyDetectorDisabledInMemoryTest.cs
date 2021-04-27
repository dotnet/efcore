// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConcurrencyDetectorDisabledInMemoryTest : ConcurrencyDetectorDisabledTestBase<
        ConcurrencyDetectorDisabledInMemoryTest.ConcurrencyDetectorInMemoryFixture>
    {
        public ConcurrencyDetectorDisabledInMemoryTest(ConcurrencyDetectorInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class ConcurrencyDetectorInMemoryFixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.EnableThreadSafetyChecks(enableChecks: false);
        }
    }
}
