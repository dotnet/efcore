// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class OverzealousInitializationCosmosTest(OverzealousInitializationCosmosTest.OverzealousInitializationCosmosFixture fixture)
    : OverzealousInitializationTestBase<OverzealousInitializationCosmosTest.OverzealousInitializationCosmosFixture>(fixture)
{
    [ConditionalFact(Skip = "Issue #17246")]
    public override void Fixup_ignores_eagerly_initialized_reference_navs()
    {
    }

    public class OverzealousInitializationCosmosFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
