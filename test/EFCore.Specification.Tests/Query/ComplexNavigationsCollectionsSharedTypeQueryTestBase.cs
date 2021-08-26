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
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "issue #13560")]
        public override Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
            => base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(async);

        // include after select is not supported
        public override Task Include_collection_with_multiple_orderbys_complex(bool async)
            => Task.CompletedTask;

        public override Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
            => Task.CompletedTask;

        public override Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
            => Task.CompletedTask;

        public override Task Include_collection_with_multiple_orderbys_member(bool async)
            => Task.CompletedTask;

        public override Task Include_collection_with_multiple_orderbys_methodcall(bool async)
            => Task.CompletedTask;

        public override Task Include_collection_with_multiple_orderbys_property(bool async)
            => Task.CompletedTask;

        public override Task Include_inside_subquery(bool async)
            => Task.CompletedTask;

        public override Task Filtered_include_outer_parameter_used_inside_filter(bool async)
            => Task.CompletedTask;

        public override Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
            => Task.CompletedTask;

        public override Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
            => Task.CompletedTask;

        public override Task Required_navigation_with_Include(bool async)
            => Task.CompletedTask;

        public override Task Required_navigation_with_Include_ThenInclude(bool async)
            => Task.CompletedTask;

        // Navigations used are not mapped in shared type.
        public override Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
            => Task.CompletedTask;
    }
}
