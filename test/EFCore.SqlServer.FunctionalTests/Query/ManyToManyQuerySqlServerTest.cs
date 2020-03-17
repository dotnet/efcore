// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ManyToManyQuerySqlServerTest : ManyToManyQueryTestBase<ManyToManyQuerySqlServerFixture>
    {
        public ManyToManyQuerySqlServerTest(ManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Dummy_test_remove_later(bool async)
        {
            await base.Dummy_test_remove_later(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[Name]
FROM [EntityOnes] AS [e]
WHERE [e].[Id] > 1");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
