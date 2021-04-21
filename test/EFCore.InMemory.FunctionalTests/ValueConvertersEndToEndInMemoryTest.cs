// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore
{
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
}

#nullable restore
