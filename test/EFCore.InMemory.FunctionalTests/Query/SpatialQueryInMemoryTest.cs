// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        [ConditionalTheory(Skip = "issue #19664")]
        public override Task Intersects_equal_to_null(bool async)
        {
            return base.Intersects_equal_to_null(async);
        }

        [ConditionalTheory(Skip = "issue #19664")]
        public override Task Intersects_not_equal_to_null(bool async)
        {
            return base.Intersects_not_equal_to_null(async);
        }

        public override Task GetGeometryN_with_null_argument(bool async)
        {
            // Sequence contains no elements
            return Task.CompletedTask;
        }
    }
}
