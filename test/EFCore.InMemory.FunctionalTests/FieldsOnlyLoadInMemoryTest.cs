// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class FieldsOnlyLoadInMemoryTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadInMemoryTest.FieldsOnlyLoadInMemoryFixture>
{
    public FieldsOnlyLoadInMemoryTest(FieldsOnlyLoadInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class FieldsOnlyLoadInMemoryFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
