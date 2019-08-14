// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQuerySqliteTest : CompiledQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public CompiledQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #16323")]
        public override Task DbQuery_query_async()
            => base.DbQuery_query_async();

        [ConditionalFact(Skip = "Issue #16323")]
        public override void DbQuery_query_first()
            => base.DbQuery_query_first();

        [ConditionalFact(Skip = "Issue #16323")]
        public override Task DbQuery_query_first_async()
            => base.DbQuery_query_first_async();

        [ConditionalFact(Skip = "Issue #16323")]
        public override void DbQuery_query()
            => base.DbQuery_query();
    }
}
