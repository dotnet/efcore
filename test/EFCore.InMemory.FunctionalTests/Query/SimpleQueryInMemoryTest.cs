// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            // ReSharper disable once UnusedParameter.Local
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [Fact(Skip = "See issue#13857")]
        public override void Auto_initialized_view_set()
        {
            base.Auto_initialized_view_set();
        }

        [Theory(Skip = "See issue#13857")]
        public override Task QueryType_simple(bool isAsync)
        {
            return base.QueryType_simple(isAsync);
        }

        [Theory(Skip = "See issue#13857")]
        public override Task QueryType_where_simple(bool isAsync)
        {
            return base.QueryType_where_simple(isAsync);
        }

        [Fact(Skip = "See issue#13857")]
        public override void Query_backed_by_database_view()
        {
            base.Query_backed_by_database_view();
        }
    }
}
