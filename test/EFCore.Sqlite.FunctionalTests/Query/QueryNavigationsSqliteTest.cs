// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsSqliteTest : QueryNavigationsTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public QueryNavigationsSqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Collection_select_nav_prop_first_or_default(bool isAsync)
        {
            return base.Collection_select_nav_prop_first_or_default(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Collection_select_nav_prop_first_or_default_then_nav_prop(bool isAsync)
        {
            return base.Collection_select_nav_prop_first_or_default_then_nav_prop(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Project_single_entity_value_subquery_works(bool isAsync)
        {
            return base.Project_single_entity_value_subquery_works(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Select_collection_FirstOrDefault_project_anonymous_type(bool isAsync)
        {
            return base.Select_collection_FirstOrDefault_project_anonymous_type(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Select_collection_FirstOrDefault_project_entity(bool isAsync)
        {
            return base.Select_collection_FirstOrDefault_project_entity(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Skip_Select_Navigation(bool isAsync)
        {
            return base.Skip_Select_Navigation(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17292")]
        public override Task Take_Select_Navigation(bool isAsync)
        {
            return base.Take_Select_Navigation(isAsync);
        }
    }
}
