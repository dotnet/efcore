// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class AsyncQuerySqlServerTest : AsyncQueryTestBase<NorthwindQuerySqlServerFixture>
    {
        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                Single_Predicate_Cancellation(TestSqlLoggerFactory.CancelQuery()));
        }

        public AsyncQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }
    }
}
