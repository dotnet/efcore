// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQueryFiltersQuerySqliteTest : NorthwindQueryFiltersQueryTestBase<
        NorthwindQuerySqliteFixture<NorthwindQueryFiltersCustomizer>>
    {
        public NorthwindQueryFiltersQuerySqliteTest(
            NorthwindQuerySqliteFixture<NorthwindQueryFiltersCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Count_query(bool async)
        {
            await base.Count_query(async);

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 1)

SELECT COUNT(*)
FROM ""Customers"" AS ""c""
WHERE @__ef_filter__TenantPrefix_0 = '' OR (""c"".""CompanyName"" IS NOT NULL AND (((""c"".""CompanyName"" LIKE @__ef_filter__TenantPrefix_0 || '%') AND substr(""c"".""CompanyName"", 1, length(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0) OR @__ef_filter__TenantPrefix_0 = ''))");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
