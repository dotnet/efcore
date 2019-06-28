// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncSimpleQueryInMemoryTest : AsyncSimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public AsyncSimpleQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "See issue#15711")]
        public override Task ToList_context_subquery_deadlock_issue()
        {
            return base.ToList_context_subquery_deadlock_issue();
        }

        [ConditionalFact(Skip = "See issue#15711")]
        public override Task ToList_on_nav_subquery_in_projection()
        {
            return base.ToList_on_nav_subquery_in_projection();
        }

        [ConditionalFact(Skip = "See issue#15711")]
        public override Task ToList_on_nav_subquery_with_predicate_in_projection()
        {
            return base.ToList_on_nav_subquery_with_predicate_in_projection();
        }

        [ConditionalFact(Skip = "See issue#13857")]
        public override Task Query_backed_by_database_view()
        {
            return base.Query_backed_by_database_view();
        }
    }
}
