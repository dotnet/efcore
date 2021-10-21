// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAggregateOperatorsQueryInMemoryTest : NorthwindAggregateOperatorsQueryTestBase<
        NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
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
        public override async Task Average_no_data_subquery(bool async)
        {
            Assert.Equal(
                "Sequence contains no elements",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_no_data_subquery(async))).Message);
        }

        public override async Task Max_no_data_subquery(bool async)
        {
            Assert.Equal(
                "Sequence contains no elements",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Max_no_data_subquery(async))).Message);
        }

        public override async Task Min_no_data_subquery(bool async)
        {
            Assert.Equal(
                "Sequence contains no elements",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Min_no_data_subquery(async))).Message);
        }

        public override async Task Average_on_nav_subquery_in_projection(bool async)
        {
            Assert.Equal(
                "Sequence contains no elements",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_on_nav_subquery_in_projection(async))).Message);
        }

        public override Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Collection_Last_member_access_in_projection_translated(async));
        }
    }
}
