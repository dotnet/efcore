// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqliteTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQuerySqliteFixture>
    {
        public ComplexNavigationsQuerySqliteTest(ComplexNavigationsQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // Skip for SQLite. Issue #14935. Cannot eval 'from <>f__AnonymousType100`1 <generated>_1 in {from Level2 l2 in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) where  ?= (Convert(Property([l1], \"Id\"), Nullable`1) == Property([l2], \"OneToMany_Optional_Inverse2Id\")) =? select new <>f__AnonymousType100`1(Name = [l2].Name)}'
        public override Task SelectMany_subquery_with_custom_projection(bool isAsync) => null;

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool isAsync)
        {
            return base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_navigation_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15081")]
        public override Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool isAsync)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(isAsync);
        }
    }
}
