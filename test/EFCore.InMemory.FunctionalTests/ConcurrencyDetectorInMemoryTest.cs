// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    // TODO: See Tasklist#23
    internal class ConcurrencyDetectorInMemoryTest : ConcurrencyDetectorTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
