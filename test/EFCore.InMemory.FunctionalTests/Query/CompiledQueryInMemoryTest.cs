// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryInMemoryTest : CompiledQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public CompiledQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "See issue#13857")]
        public override void DbQuery_query()
        {
            base.DbQuery_query();
        }

        [Fact(Skip = "See issue#13857")]
        public override Task DbQuery_query_async()
        {
            return base.DbQuery_query_async();
        }

        [Fact(Skip = "See issue#13857")]
        public override void DbQuery_query_first()
        {
            base.DbQuery_query_first();
        }

        [Fact(Skip = "See issue#13857")]
        public override Task DbQuery_query_first_async()
        {
            return base.DbQuery_query_first_async();
        }
    }
}
