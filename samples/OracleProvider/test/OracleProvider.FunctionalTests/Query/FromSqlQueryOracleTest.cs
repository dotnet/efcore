// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlQueryOracleTest : FromSqlQueryTestBase<NorthwindQueryOracleFixture<NoopModelCustomizer>>
    {
        public FromSqlQueryOracleTest(NorthwindQueryOracleFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            // #9182
            Fixture.TestStore.CloseConnection();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public override void From_sql_with_db_parameters_called_multiple_times()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter(":id", "ALFKI");

                var query = context.Customers
                    .FromSql(@"SELECT * FROM ""Customers"" WHERE ""CustomerID"" = :id", parameter);

                var result1 = query.ToList();

                Assert.Equal(1, result1.Count);

                // This should not throw exception.
                var result2 = query.ToList();

                Assert.Equal(1, result2.Count);
            }
        }

        [Fact]
        public override void From_sql_with_dbParameter()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter(":city", "London");

                var actual = context.Customers
                    .FromSql(@"SELECT * FROM ""Customers"" WHERE ""City"" = :city", parameter)
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [Fact]
        public override void From_sql_with_dbParameter_mixed()
        {
            using (var context = CreateContext())
            {
                var city = "London";
                var title = "Sales Representative";

                var titleParameter = CreateDbParameter(":title", title);

                var actual = context.Customers
                    .FromSql(
                        @"SELECT * FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = :title",
                        city,
                        titleParameter)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                var cityParameter = CreateDbParameter(":city", city);

                actual = context.Customers
                    .FromSql(
                        @"SELECT * FROM ""Customers"" WHERE ""City"" = :city AND ""ContactTitle"" = {1}",
                        cityParameter,
                        title)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        public override void Bad_data_error_handling_invalid_cast()
        {
            // Not supported on Oracle
        }

        public override void Bad_data_error_handling_invalid_cast_projection()
        {
            // Not supported on Oracle
        }

        public override void Bad_data_error_handling_invalid_cast_no_tracking()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .AsNoTracking()
                                .FromSql(
                                    @"SELECT ""ProductID"" AS ""ProductName"", ""ProductName"" AS ""ProductID"", ""SupplierID"", ""UnitPrice"", ""UnitsInStock"", ""Discontinued""
                               FROM ""Products""")
                                .ToList()).Message);
            }
        }

        [Fact]
        public virtual void From_sql_in_subquery_with_dbParameter()
        {
            using (var context = CreateContext())
            {
                var actual = context.Orders.Where(
                        o =>
                            context.Customers
                                .FromSql(
                                    @"SELECT * FROM ""Customers"" WHERE ""City"" = :city",
                                    // ReSharper disable once FormatStringProblem
                                    new OracleParameter(":city", "London"))
                                .Select(c => c.CustomerID)
                                .Contains(o.CustomerID))
                    .ToArray();

                Assert.Equal(46, actual.Length);

                AssertSql(
                    @":city='London' (Nullable = false)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" IN (
    SELECT ""c"".""CustomerID""
    FROM (
        SELECT * FROM ""Customers"" WHERE ""City"" = :city
    ) ""c""
)");
            }
        }

        [Fact]
        public virtual void From_sql_in_subquery_with_positional_dbParameter_without_name()
        {
            using (var context = CreateContext())
            {
                var actual = context.Orders.Where(
                        o =>
                            context.Customers
                                .FromSql(
                                    @"SELECT * FROM ""Customers"" WHERE ""City"" = {0}",
                                    // ReSharper disable once FormatStringProblem
                                    new OracleParameter { Value = "London" })
                                .Select(c => c.CustomerID)
                                .Contains(o.CustomerID))
                    .ToArray();

                Assert.Equal(46, actual.Length);

                AssertSql(
                    @":p0='London' (Nullable = false)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" IN (
    SELECT ""c"".""CustomerID""
    FROM (
        SELECT * FROM ""Customers"" WHERE ""City"" = :p0
    ) ""c""
)");
            }
        }

        [Fact]
        public virtual void From_sql_in_subquery_with_positional_dbParameter_with_name()
        {
            using (var context = CreateContext())
            {
                var actual = context.Orders.Where(
                        o =>
                            context.Customers
                                .FromSql(
                                    @"SELECT * FROM ""Customers"" WHERE ""City"" = {0}",
                                    // ReSharper disable once FormatStringProblem
                                    new OracleParameter(":city", "London"))
                                .Select(c => c.CustomerID)
                                .Contains(o.CustomerID))
                    .ToArray();

                Assert.Equal(46, actual.Length);

                AssertSql(
                    @":city='London' (Nullable = false)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" IN (
    SELECT ""c"".""CustomerID""
    FROM (
        SELECT * FROM ""Customers"" WHERE ""City"" = :city
    ) ""c""
)");
            }
        }

        [Fact]
        public virtual void From_sql_with_dbParameter_mixed_in_subquery()
        {
            using (var context = CreateContext())
            {
                const string city = "London";
                const string title = "Sales Representative";

                var actual = context.Orders.Where(
                        o =>
                            context.Customers
                                .FromSql(
                                    @"SELECT * FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = :title",
                                    city,
                                    // ReSharper disable once FormatStringProblem
                                    new OracleParameter(":title", title))
                                .Select(c => c.CustomerID)
                                .Contains(o.CustomerID))
                    .ToArray();

                Assert.Equal(26, actual.Length);

                actual = context.Orders.Where(
                        o =>
                            context.Customers
                                .FromSql(
                                    @"SELECT * FROM ""Customers"" WHERE ""City"" = :city AND ""ContactTitle"" = {1}",
                                    // ReSharper disable once FormatStringProblem
                                    new OracleParameter(":city", city),
                                    title)
                                .Select(c => c.CustomerID)
                                .Contains(o.CustomerID))
                    .ToArray();

                Assert.Equal(26, actual.Length);

                AssertSql(
                    @":title='Sales Representative' (Nullable = false)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" IN (
    SELECT ""c"".""CustomerID""
    FROM (
        SELECT * FROM ""Customers"" WHERE ""City"" = N'London' AND ""ContactTitle"" = :title
    ) ""c""
)",
                    //
                    @":city='London' (Nullable = false)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" IN (
    SELECT ""c"".""CustomerID""
    FROM (
        SELECT * FROM ""Customers"" WHERE ""City"" = :city AND ""ContactTitle"" = N'Sales Representative'
    ) ""c""
)");
            }
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new OracleParameter
            {
                ParameterName = name.Replace("@", ":"),
                Value = value
            };

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected.Select(s => s.Replace("\r\n", "\n")).ToArray());
    }
}
