// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

#pragma warning disable 1998

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class AsyncQuerySqlServerTest : AsyncQueryTestBase<NorthwindQuerySqlServerFixture>
    {
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

        public override async Task String_Contains_Literal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M")
                                     || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        public override async Task String_Contains_MethodCall()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())
                                    || c.ContactName.Contains(LocalMethod2())), // case-sensitive
                entryCount: 34);
        }

        public async Task Skip_when_no_order_by()
        {
            await Assert.ThrowsAsync<Exception>(async () => await AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await Single_Predicate_Cancellation(TestSqlLoggerFactory.CancelQuery()));
        }

        [Fact]
        public async Task Invalid_query_should_not_generate_nested_aggregates()
        {
            AggregateException exception = await Assert.ThrowsAsync<AggregateException>(async () =>
                {
                    using (var context = CreateContext())
                    {
                        await context.Products
                            .Where(p => p.ProductID == 1)
                            .Take(50)
                            .Skip(5)
                            .ToArrayAsync();
                    }
                });

            Assert.IsNotType<AggregateException>(exception.InnerException);
        }

        public AsyncQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }
    }
}
