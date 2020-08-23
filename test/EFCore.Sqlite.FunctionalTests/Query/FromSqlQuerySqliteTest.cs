// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
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
            fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override string FromSqlRaw_queryable_composed()
        {
            var queryString = base.FromSqlRaw_queryable_composed();

            var expected =
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT * FROM ""Customers""
) AS ""c""
WHERE ('z' = '') OR (instr(""c"".""ContactName"", 'z') > 0)";

            Assert.Equal(expected, queryString, ignoreLineEndingDifferences: true);

            return null;
        }

        public override string FromSqlRaw_queryable_with_parameters_and_closure()
        {
            var queryString = base.FromSqlRaw_queryable_with_parameters_and_closure();

            Assert.Equal(
                @".param set p0 'London'
.param set @__contactTitle_1 'Sales Representative'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = @p0
) AS ""c""
WHERE ""c"".""ContactTitle"" = @__contactTitle_1", queryString, ignoreLineEndingDifferences: true);

            return null;
        }

        public override void Bad_data_error_handling_invalid_cast_key()
        {
            // Not supported on SQLite
        }

        public override void Bad_data_error_handling_invalid_cast()
        {
            // Not supported on SQLite
        }

        public override void Bad_data_error_handling_invalid_cast_projection()
        {
            // Not supported on SQLite
        }

        public override void Bad_data_error_handling_invalid_cast_no_tracking()
        {
            // Not supported on SQLite
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new SqliteParameter { ParameterName = name, Value = value };
    }
}
