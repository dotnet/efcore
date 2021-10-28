// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlQuerySqliteTest : FromSqlQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public FromSqlQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task FromSqlRaw_queryable_composed(bool async)
        {
            await base.FromSqlRaw_queryable_composed(async);

            AssertSql(
                @"SELECT ""m"".""CustomerID"", ""m"".""Address"", ""m"".""City"", ""m"".""CompanyName"", ""m"".""ContactName"", ""m"".""ContactTitle"", ""m"".""Country"", ""m"".""Fax"", ""m"".""Phone"", ""m"".""PostalCode"", ""m"".""Region""
FROM (
    SELECT * FROM ""Customers""
) AS ""m""
WHERE ('z' = '') OR (instr(""m"".""ContactName"", 'z') > 0)");
        }

        public override async Task FromSqlRaw_queryable_with_parameters_and_closure(bool async)
        {
            await base.FromSqlRaw_queryable_with_parameters_and_closure(async);

            AssertSql(
                @"p0='London' (Size = 6)
@__contactTitle_1='Sales Representative' (Size = 20)

SELECT ""m"".""CustomerID"", ""m"".""Address"", ""m"".""City"", ""m"".""CompanyName"", ""m"".""ContactName"", ""m"".""ContactTitle"", ""m"".""Country"", ""m"".""Fax"", ""m"".""Phone"", ""m"".""PostalCode"", ""m"".""Region""
FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = @p0
) AS ""m""
WHERE ""m"".""ContactTitle"" = @__contactTitle_1");
        }

        public override Task Bad_data_error_handling_invalid_cast_key(bool async)
        {
            // Not supported on SQLite
            return Task.CompletedTask;
        }

        public override Task Bad_data_error_handling_invalid_cast(bool async)
        {
            // Not supported on SQLite
            return Task.CompletedTask;
        }

        public override Task Bad_data_error_handling_invalid_cast_projection(bool async)
        {
            // Not supported on SQLite
            return Task.CompletedTask;
        }

        public override Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
        {
            // Not supported on SQLite
            return Task.CompletedTask;
        }

        public override async Task FromSqlRaw_composed_with_common_table_expression(bool async)
        {
            await base.FromSqlRaw_composed_with_common_table_expression(async);

            AssertSql(
                @"SELECT ""m"".""CustomerID"", ""m"".""Address"", ""m"".""City"", ""m"".""CompanyName"", ""m"".""ContactName"", ""m"".""ContactTitle"", ""m"".""Country"", ""m"".""Fax"", ""m"".""Phone"", ""m"".""PostalCode"", ""m"".""Region""
FROM (

    WITH ""Customers2"" AS (
        SELECT * FROM ""Customers""
    )
    SELECT * FROM ""Customers2""
) AS ""m""
WHERE ('z' = '') OR (instr(""m"".""ContactName"", 'z') > 0)");
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new SqliteParameter { ParameterName = name, Value = value };

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
