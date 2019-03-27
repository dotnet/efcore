// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
#if !Test21
    public class SpatialQueryInMemoryTest : SpatialQueryTestBase<SpatialQueryInMemoryFixture>
    {
        public SpatialQueryInMemoryTest(SpatialQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }
    }
#endif
}
