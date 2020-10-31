// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindDbFunctionsQuerySqliteTest : NorthwindDbFunctionsQueryRelationalTestBase<
        NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindDbFunctionsQuerySqliteTest(
            NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Glob(bool async)
        {
            await AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Glob(c.ContactName, "*M*"),
                c => c.ContactName.Contains("M"));

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" AS ""c""
WHERE glob('*M*', ""c"".""ContactName"")");
        }

        protected override string CaseInsensitiveCollation
            => "NOCASE";

        protected override string CaseSensitiveCollation
            => "BINARY";

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
