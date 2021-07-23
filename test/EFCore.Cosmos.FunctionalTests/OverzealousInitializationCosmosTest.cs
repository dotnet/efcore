// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class OverzealousInitializationCosmosTest
        : OverzealousInitializationTestBase<OverzealousInitializationCosmosTest.OverzealousInitializationCosmosFixture>
    {
        public OverzealousInitializationCosmosTest(OverzealousInitializationCosmosFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Fixup_does_not_ignore_eagerly_initialized_reference_navs()
        {
        }

        public class OverzealousInitializationCosmosFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => CosmosTestStoreFactory.Instance;
        }
    }
}
