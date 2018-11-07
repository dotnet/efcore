// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
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

        public override Task Query_backed_by_database_view()
        {
            // Not present on SQLite
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () =>
                    await Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));
        }
    }
}
