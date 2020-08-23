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
    public class NorthwindAsyncSimpleQuerySqliteTest : NorthwindAsyncSimpleQueryRelationalTestBase<
        NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindAsyncSimpleQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () =>
                    await Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));
        }
    }
}
