// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ConvertToStoreTypesInMemoryTest : ConvertToStoreTypesTestBase<ConvertToStoreTypesInMemoryTest.ConvertToStoreTypesInMemoryFixture>
    {
        public ConvertToStoreTypesInMemoryTest(ConvertToStoreTypesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class ConvertToStoreTypesInMemoryFixture : ConvertToStoreTypesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => false;

            public override DateTime DefaultDateTime => new DateTime();
        }
    }
}
