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
        public override Task Select_collection_navigation_simple(bool isAsync)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_simple_followed_by_ordering_by_scalar(bool isAsync)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_multi_part(bool isAsync)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_collection_navigation_multi_part2(bool isAsync)
            => Task.CompletedTask;
    }
}
