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

        [ConditionalFact(Skip = "issue #4311")]
        public override Task Nested_group_join_with_take()
        {
            return base.Nested_group_join_with_take();
        }

        [ConditionalFact(Skip = "issue #9591")]
        public override Task Multi_include_with_groupby_in_subquery()
        {
            return base.Multi_include_with_groupby_in_subquery();
        }
    }
}
