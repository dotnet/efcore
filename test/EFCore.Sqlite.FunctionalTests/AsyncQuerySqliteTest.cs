// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

#pragma warning disable 1998
namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class AsyncQuerySqliteTest : AsyncQueryTestBase<NorthwindQuerySqliteFixture>
    {
        public AsyncQuerySqliteTest(NorthwindQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // TODO: Complex projection translation.
        public override async Task Projection_when_arithmetic_expressions()
        {
            //base.Projection_when_arithmetic_expressions();
        }

        public override async Task Projection_when_arithmetic_mixed()
        {
            //base.Projection_when_arithmetic_mixed();
        }

        public override async Task Projection_when_arithmetic_mixed_subqueries()
        {
            //base.Projection_when_arithmetic_mixed_subqueries();
        }

        public async Task Skip_when_no_order_by()
        {
            await Assert.ThrowsAsync<Exception>(async () => await AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await Single_Predicate_Cancellation(Fixture.CancelQuery()));
        }
    }
}
