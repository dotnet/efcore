// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsInMemoryTest : QueryNavigationsTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public QueryNavigationsInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_subquery_on_navigation_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_Where_Navigation_Client(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Join_with_nav_in_predicate_in_subquery_when_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Join_with_nav_projected_in_subquery_when_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Join_with_nav_in_orderby_in_subquery_when_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Collection_select_nav_prop_all_client(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override void Collection_where_nav_prop_all_client()
        {
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_simple(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_simple_followed_by_ordering_by_scalar(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_multi_part(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_multi_part2(bool isAsync) => null;
    }
}
