// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQueryInMemoryTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryInMemoryFixture>
    {
        public ComplexNavigationsQueryInMemoryTest(ComplexNavigationsQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "issue #4311")]
        public override Task Nested_group_join_with_take(bool IsAsync)
        {
            return base.Nested_group_join_with_take(IsAsync);
        }

        [ConditionalTheory(Skip = "issue #9591")]
        public override Task Multi_include_with_groupby_in_subquery(bool IsAsync)
        {
            return base.Multi_include_with_groupby_in_subquery(IsAsync);
        }

        [ConditionalTheory(Skip = "issue #13561")]
        public override Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool isAsync)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(isAsync);
        }
    }
}
