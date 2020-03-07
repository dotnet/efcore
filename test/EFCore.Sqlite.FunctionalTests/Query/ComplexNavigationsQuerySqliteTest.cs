// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqliteTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQuerySqliteFixture>
    {
        public ComplexNavigationsQuerySqliteTest(ComplexNavigationsQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool async)
        {
            return base.SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(async);
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task Project_collection_navigation_nested_with_take(bool async)
        {
            return base.Project_collection_navigation_nested_with_take(async);
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task Include_inside_subquery(bool async)
        {
            return base.Include_inside_subquery(async);
        }

        // Sqlite does not support cross/outer apply
        public override Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool async) => null;
        public override Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async) => null;
    }
}
