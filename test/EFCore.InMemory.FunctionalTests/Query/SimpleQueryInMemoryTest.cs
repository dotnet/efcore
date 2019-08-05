// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQueryInMemoryTest : SimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public SimpleQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "See issue#13857")]
        public override void Auto_initialized_view_set()
        {
            base.Auto_initialized_view_set();
        }

        [ConditionalTheory(Skip = "See issue#13857")]
        public override Task QueryType_simple(bool isAsync)
        {
            return base.QueryType_simple(isAsync);
        }

        [ConditionalTheory(Skip = "See issue#13857")]
        public override Task QueryType_where_simple(bool isAsync)
        {
            return base.QueryType_where_simple(isAsync);
        }

        [ConditionalFact(Skip = "See issue#13857")]
        public override void Query_backed_by_database_view()
        {
            base.Query_backed_by_database_view();
        }

         // InMemory can mimic throw behavior for subquery
        public override void Average_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Average_no_data_subquery());
        }

        public override void Min_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Min_no_data_subquery());
        }

        public override void Max_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Max_no_data_subquery());
        }

        #region SetOperations
        public override Task Concat(bool isAsync)
        {
            return Task.CompletedTask; //base.Concat(isAsync);
        }

        public override Task Concat_nested(bool isAsync)
        {
            return Task.CompletedTask; //base.Concat_nested(isAsync);
        }

        public override Task Concat_non_entity(bool isAsync)
        {
            return Task.CompletedTask; //base.Concat_non_entity(isAsync);
        }

        public override Task Except(bool isAsync)
        {
            return Task.CompletedTask; //base.Except(isAsync);
        }

        public override Task Except_nested(bool isAsync)
        {
            return Task.CompletedTask; //base.Except_nested(isAsync);
        }

        public override Task Except_non_entity(bool isAsync)
        {
            return Task.CompletedTask; //base.Except_non_entity(isAsync);
        }

        public override Task Except_simple_followed_by_projecting_constant(bool isAsync)
        {
            return Task.CompletedTask; //base.Except_simple_followed_by_projecting_constant(isAsync);
        }

        public override Task Intersect(bool isAsync)
        {
            return Task.CompletedTask; //base.Intersect(isAsync);
        }

        public override Task Intersect_nested(bool isAsync)
        {
            return Task.CompletedTask; //base.Intersect_nested(isAsync);
        }

        public override Task Intersect_non_entity(bool isAsync)
        {
            return Task.CompletedTask; //base.Intersect_non_entity(isAsync);
        }

        public override Task Union_Intersect(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Intersect(isAsync);
        }

        public override Task Union(bool isAsync)
        {
            return Task.CompletedTask; //base.Union(isAsync);
        }

        public override Task Union_Include(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Include(isAsync);
        }

        public override Task Union_nested(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_nested(isAsync);
        }

        public override void Union_non_entity(bool isAsync)
        {
            base.Union_non_entity(isAsync);
        }

        public override Task Union_OrderBy_Skip_Take(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_OrderBy_Skip_Take(isAsync);
        }

        public override Task Union_Select(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Select(isAsync);
        }

        public override Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Skip_Take_OrderBy_ThenBy_Where(isAsync);
        }

        public override Task Union_Take_Union_Take(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Take_Union_Take(isAsync);
        }

        public override Task Union_Union(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Union(isAsync);
        }

        public override Task Union_Where(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_Where(isAsync);
        }

        public override Task Union_with_anonymous_type_projection(bool isAsync)
        {
            return Task.CompletedTask; //base.Union_with_anonymous_type_projection(isAsync);
        }

        public override Task Client_eval_Union_FirstOrDefault(bool isAsync)
        {
            return Task.CompletedTask; //base.Client_eval_Union_FirstOrDefault(isAsync);
        }

        public override Task Include_Union(bool isAsync)
        {
            return Task.CompletedTask; //base.Include_Union(isAsync);
        }

        public override void Include_Union_different_includes_throws()
        {
            base.Include_Union_different_includes_throws();
        }

        public override void Include_Union_only_on_one_side_throws()
        {
            base.Include_Union_only_on_one_side_throws();
        }

        public override Task Select_Union(bool isAsync)
        {
            return Task.CompletedTask; //base.Select_Union(isAsync);
        }

        public override Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync)
        {
            return Task.CompletedTask; //base.Select_Union_different_fields_in_anonymous_with_subquery(isAsync);
        }

        public override Task Select_Union_unrelated(bool isAsync)
        {
            return Task.CompletedTask; //base.Select_Union_unrelated(isAsync);
        }

        public override Task SubSelect_Union(bool isAsync)
        {
            return Task.CompletedTask; //base.SubSelect_Union(isAsync);
        }

        public override Task Select_Except_reference_projection(bool isAsync)
        {
            return Task.CompletedTask; //base.Select_Except_reference_projection(isAsync);
        }

        public override Task GroupBy_Select_Union(bool isAsync)
        {
            return Task.CompletedTask; //base.GroupBy_Select_Union(isAsync);
        }

        public override Task Union_over_different_projection_types(bool isAsync, string leftType, string rightType)
        {
            return Task.CompletedTask; //base.Union_over_different_projection_types(isAsync);
        }

        #endregion

        [ConditionalTheory(Skip = "Issue#15711")]
        public override Task Include_with_orderby_skip_preserves_ordering(bool isAsync)
        {
            return base.Include_with_orderby_skip_preserves_ordering(isAsync);
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Select_nested_collection_multi_level()
        {
            base.Select_nested_collection_multi_level();
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Select_nested_collection_multi_level4()
        {
            base.Select_nested_collection_multi_level4();
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Select_nested_collection_multi_level5()
        {
            base.Select_nested_collection_multi_level5();
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Select_nested_collection_multi_level6()
        {
            base.Select_nested_collection_multi_level6();
        }

        [ConditionalTheory(Skip = "Issue#16575")]
        public override Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool isAsync)
        {
            return base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16963")]
        public override Task QueryType_with_included_nav(bool isAsync)
        {
            return base.QueryType_with_included_nav(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16963")]
        public override Task QueryType_with_included_navs_multi_level(bool isAsync)
        {
            return base.QueryType_with_included_navs_multi_level(isAsync);
        }
    }
}
