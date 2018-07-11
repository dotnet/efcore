// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query
{
    public partial class SimpleQueryCosmosSqlTest : QueryTestBase<NorthwindQueryCosmosSqlFixture<NoopModelCustomizer>>
    {
        public SimpleQueryCosmosSqlTest(NorthwindQueryCosmosSqlFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual void Simple_IQuaryable()
        {
            AssertQuery<Customer>(cs => cs, entryCount: 91);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        protected virtual void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        private void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
