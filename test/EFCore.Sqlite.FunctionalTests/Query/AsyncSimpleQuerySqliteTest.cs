// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
#pragma warning disable 1998
namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncSimpleQuerySqliteTest : AsyncSimpleQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public AsyncSimpleQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        public async Task Skip_when_no_order_by()
        {
            await Assert.ThrowsAsync<Exception>(async () => await AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));
        }
    }
}
