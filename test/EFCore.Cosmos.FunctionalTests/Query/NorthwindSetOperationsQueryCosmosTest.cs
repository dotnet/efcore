// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSetOperationsQueryCosmosTest : NorthwindSetOperationsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindSetOperationsQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // Set operations aren't supported on Cosmos
        public override Task Concat(bool async) => Task.CompletedTask;
        public override Task Concat_nested(bool async) => Task.CompletedTask;
        public override Task Concat_non_entity(bool async) => Task.CompletedTask;
        public override Task Except(bool async) => Task.CompletedTask;
        public override Task Except_simple_followed_by_projecting_constant(bool async) => Task.CompletedTask;
        public override Task Except_nested(bool async) => Task.CompletedTask;
        public override Task Except_non_entity(bool async) => Task.CompletedTask;
        public override Task Intersect(bool async) => Task.CompletedTask;
        public override Task Intersect_nested(bool async) => Task.CompletedTask;
        public override Task Intersect_non_entity(bool async) => Task.CompletedTask;
        public override Task Union(bool async) => Task.CompletedTask;
        public override Task Union_nested(bool async) => Task.CompletedTask;
        public override Task Union_non_entity(bool async) => Task.CompletedTask;
        public override Task Union_OrderBy_Skip_Take(bool async) => Task.CompletedTask;
        public override Task Union_Where(bool async) => Task.CompletedTask;
        public override Task Union_Skip_Take_OrderBy_ThenBy_Where(bool async) => Task.CompletedTask;
        public override Task Union_Union(bool async) => Task.CompletedTask;
        public override Task Union_Intersect(bool async) => Task.CompletedTask;
        public override Task Union_Take_Union_Take(bool async) => Task.CompletedTask;
        public override Task Select_Union(bool async) => Task.CompletedTask;
        public override Task Union_Select(bool async) => Task.CompletedTask;
        public override Task Union_Select_scalar(bool async) => Task.CompletedTask;
        public override Task Union_with_anonymous_type_projection(bool async) => Task.CompletedTask;
        public override Task Select_Union_unrelated(bool async) => Task.CompletedTask;
        public override Task Select_Union_different_fields_in_anonymous_with_subquery(bool async) => Task.CompletedTask;
        public override Task Union_Include(bool async) => Task.CompletedTask;
        public override Task Include_Union(bool async) => Task.CompletedTask;
        public override Task Select_Except_reference_projection(bool async) => Task.CompletedTask;
        public override void Include_Union_only_on_one_side_throws() { }
        public override void Include_Union_different_includes_throws() { }
        public override Task SubSelect_Union(bool async) => Task.CompletedTask;
        public override Task Client_eval_Union_FirstOrDefault(bool async) => Task.CompletedTask;
        public override Task GroupBy_Select_Union(bool async) => Task.CompletedTask;
        public override Task Union_over_columns_with_different_nullability(bool async) => Task.CompletedTask;
        public override Task Union_over_different_projection_types(bool async, string leftType, string rightType) => Task.CompletedTask;
        public override Task OrderBy_Take_Union(bool async) => Task.CompletedTask;

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
