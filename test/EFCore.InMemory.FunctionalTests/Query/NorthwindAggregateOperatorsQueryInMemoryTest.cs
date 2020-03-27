// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAggregateOperatorsQueryInMemoryTest : NorthwindAggregateOperatorsQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        // InMemory can throw server side exception
        public override void Average_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Average_no_data_subquery());
        }

        public override void Max_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Max_no_data_subquery());
        }

        public override void Min_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Min_no_data_subquery());
        }

        public override Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Collection_Last_member_access_in_projection_translated(async));
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Contains_with_local_tuple_array_closure(bool async)
        {
            return base.Contains_with_local_tuple_array_closure(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Last_when_no_order_by(bool async)
        {
            return base.Last_when_no_order_by(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task LastOrDefault_when_no_order_by(bool async)
        {
            return base.LastOrDefault_when_no_order_by(async);
        }

        [ConditionalFact(Skip = "Issue#20023")]
        public override void Contains_over_keyless_entity_throws()
        {
            base.Contains_over_keyless_entity_throws();
        }
    }
}
