// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsCollectionsSharedTypeQueryTestBase<TFixture> : ComplexNavigationsCollectionsQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsSharedTypeQueryFixtureBase, new()
    {
        protected ComplexNavigationsCollectionsSharedTypeQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override Task Multiple_complex_includes_self_ref(bool async)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "issue #13560")]
        public override Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_complex(bool async)
        {
            return base.Include_collection_with_multiple_orderbys_complex(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
        {
            return base.Include_collection_with_multiple_orderbys_complex_repeated(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
        {
            return base.Include_collection_with_multiple_orderbys_complex_repeated_checked(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_member(bool async)
        {
            return base.Include_collection_with_multiple_orderbys_member(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_methodcall(bool async)
        {
            return base.Include_collection_with_multiple_orderbys_methodcall(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_property(bool async)
        {
            return base.Include_collection_with_multiple_orderbys_property(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_inside_subquery(bool async)
        {
            return base.Include_inside_subquery(async);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Filtered_include_outer_parameter_used_inside_filter(bool async)
        {
            return base.Filtered_include_outer_parameter_used_inside_filter(async);
        }

        public override Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
        {
            // Navigations used are not mapped in shared type.
            return Task.CompletedTask;
        }
    }
}
