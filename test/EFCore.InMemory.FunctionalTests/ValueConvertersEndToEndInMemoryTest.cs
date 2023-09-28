// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class ValueConvertersEndToEndInMemoryTest
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndInMemoryTest.ValueConvertersEndToEndInMemoryFixture>
{
    public ValueConvertersEndToEndInMemoryTest(ValueConvertersEndToEndInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class ValueConvertersEndToEndInMemoryFixture : ValueConvertersEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}

#nullable restore
