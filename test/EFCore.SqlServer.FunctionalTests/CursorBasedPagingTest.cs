// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CursorBasedPagingTest : QueryTestBase<GearsOfWarQuerySqlServerFixture>, IDisposable
    {
        public CursorBasedPagingTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public void Dispose()
        {
            
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task TakeAfterMatch(bool isAsync)
        {
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.TakeAfterMatch(g => g.IsMarcus, 5));

            AssertSql(
                @"xyz");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
