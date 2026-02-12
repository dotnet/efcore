// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedQueryInMemoryTest(OwnedQueryInMemoryTest.OwnedQueryInMemoryFixture fixture)
    : OwnedQueryTestBase<OwnedQueryInMemoryTest.OwnedQueryInMemoryFixture>(fixture)
{
    public override Task Contains_over_owned_collection(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Contains_over_owned_collection(async));

    public override Task ElementAt_over_owned_collection(bool async)
        => AssertTranslationFailed(() => base.ElementAt_over_owned_collection(async));

    public override Task ElementAtOrDefault_over_owned_collection(bool async)
        => AssertTranslationFailed(() => base.ElementAt_over_owned_collection(async));

    public override Task FirstOrDefault_over_owned_collection(bool async)
        => Assert.ThrowsAsync<NullReferenceException>(() => base.FirstOrDefault_over_owned_collection(async));

    public override Task OrderBy_ElementAt_over_owned_collection(bool async)
        => AssertTranslationFailed(() => base.OrderBy_ElementAt_over_owned_collection(async));

    public class OwnedQueryInMemoryFixture : OwnedQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
