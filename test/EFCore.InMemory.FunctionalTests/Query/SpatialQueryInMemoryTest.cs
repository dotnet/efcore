// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SpatialQueryInMemoryTest : SpatialQueryTestBase<SpatialQueryInMemoryFixture>
    {
        public SpatialQueryInMemoryTest(SpatialQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "issue #19661")]
        public override Task Distance_constant_lhs(bool async)
        {
            return base.Distance_constant_lhs(async);
        }

        public override Task Intersects_equal_to_null(bool async)
        {
            return Assert.ThrowsAsync<NullReferenceException>(() => base.Intersects_equal_to_null(async));
        }

        public override Task Intersects_not_equal_to_null(bool async)
        {
            return Assert.ThrowsAsync<NullReferenceException>(() => base.Intersects_not_equal_to_null(async));
        }

        public override Task GetGeometryN_with_null_argument(bool async)
        {
            // Sequence contains no elements
            return Task.CompletedTask;
        }
    }
}
