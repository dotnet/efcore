// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NorthwindAsyncQueryTest : NorthwindAsyncQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            var aggregateException
                = await Assert.ThrowsAsync<AggregateException>(() =>
                    Single_Predicate_Cancellation(_fixture.CancelQuery()));

            Assert.IsType<TaskCanceledException>(aggregateException.InnerException.InnerException);
        }

        private readonly NorthwindQueryFixture _fixture;

        public NorthwindAsyncQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
