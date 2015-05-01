// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.InMemory.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    internal class ChangeTrackingInMemoryTest : ChangeTrackingTestBase<NorthwindQueryInMemoryFixture>
    {
        public ChangeTrackingInMemoryTest(NorthwindQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }
    }
}
