// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsCollectionsSplitQueryRelationalTestBase<TFixture> : ComplexNavigationsCollectionsQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsQueryFixtureBase, new()
    {
        protected ComplexNavigationsCollectionsSplitQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
            => new SplitQueryRewritingExpressionVisitor().Visit(serverQueryExpression);

        private class SplitQueryRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly MethodInfo _asSplitQueryMethod
                = typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.AsSplitQuery));

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is QueryRootExpression rootExpression)
                {
                    var splitMethod = _asSplitQueryMethod.MakeGenericMethod(rootExpression.EntityType.ClrType);

                    return Expression.Call(splitMethod, rootExpression);
                }

                return base.VisitExtension(extensionExpression);
            }
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
        {
            return base.Complex_query_with_let_collection_projection_FirstOrDefault(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Filtered_include_outer_parameter_used_inside_filter(bool async)
        {
            return base.Filtered_include_outer_parameter_used_inside_filter(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Include_inside_subquery(bool async)
        {
            return base.Include_inside_subquery(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
        {
            return base.Lift_projection_mapping_when_pushing_down_subquery(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
        {
            return base.Null_check_in_anonymous_type_projection_should_not_be_removed(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
        {
            return base.Null_check_in_Dto_projection_should_not_be_removed(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Project_collection_navigation_composed(bool async)
        {
            return base.Project_collection_navigation_composed(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Project_collection_navigation_nested_with_take(bool async)
        {
            return base.Project_collection_navigation_nested_with_take(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Select_nav_prop_collection_one_to_many_required(bool async)
        {
            return base.Select_nav_prop_collection_one_to_many_required(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Select_subquery_single_nested_subquery(bool async)
        {
            return base.Select_subquery_single_nested_subquery(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Select_subquery_single_nested_subquery2(bool async)
        {
            return base.Select_subquery_single_nested_subquery2(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
        {
            return base.SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Skip_Take_Select_collection_Skip_Take(bool async)
        {
            return base.Skip_Take_Select_collection_Skip_Take(async);
        }

        [ConditionalTheory(Skip = "Split query not fully supported yet.")]
        public override Task Take_Select_collection_Take(bool async)
        {
            return base.Take_Select_collection_Take(async);
        }
    }
}
