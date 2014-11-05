// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class SqLiteQueryTest : QueryTestBase<SqLiteNorthwindQueryFixture>
    {
        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"SELECT ""t0"".*
FROM (
    SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""CustomerID""
    LIMIT @p0
) AS ""t0""
LIMIT @p1",
                TestSqlLoggerFactory.Sql);
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            Assert.Equal(
                @"SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE @p0 || '%'",
                TestSqlLoggerFactory.Sql);
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE @p0 || '%'",
                TestSqlLoggerFactory.Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '%' || @p0",
                TestSqlLoggerFactory.Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""City"", ""e"".""Country"", ""e"".""EmployeeID"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ((""c"".""City"" = @p0 AND ""c"".""Country"" = @p1) AND (""e"".""City"" = @p0 AND ""e"".""Country"" = @p1))",
                TestSqlLoggerFactory.Sql);
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();

            Assert.Equal(
                @"SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""City"" IS NULL AND ""c"".""Country"" = @p0)",
                TestSqlLoggerFactory.Sql);
        }

        public void Skip_when_no_order_by()
        {
            AssertQuery<Customer>(cs => cs.Skip(5).Take(10));

            Assert.Equal(
                @"SELECT ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""CustomerID"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
LIMIT 10 OFFSET 5",
                TestSqlLoggerFactory.Sql);
        }

        public SqLiteQueryTest(SqLiteNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
