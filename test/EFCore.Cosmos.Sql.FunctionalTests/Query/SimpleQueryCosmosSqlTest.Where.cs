// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query
{
    public partial class SimpleQueryCosmosSqlTest : QueryTestBase<NorthwindQueryCosmosSqlFixture<NoopModelCustomizer>>
    {
        [ConditionalFact]
        public virtual void Where_add()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID + 10 == 10258),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] + 10) = 10258))");
        }

        [ConditionalFact]
        public virtual void Where_subtract()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID - 10 == 10238),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] - 10) = 10238))");
        }

        [ConditionalFact]
        public virtual void Where_multiply()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID * 1 == 10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] * 1) = 10248))");
        }

        [ConditionalFact]
        public virtual void Where_divide()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID / 1 == 10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] / 1) = 10248))");
        }

        [ConditionalFact]
        public virtual void Where_modulo()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID % 10248 == 0),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] % 10248) = 0))");
        }

        [ConditionalFact]
        public virtual void Where_bitwise_or()
        {
            AssertQuery<Order>(
                os => os.Where(o => (o.OrderID | 10248) == 10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] | 10248) = 10248))");
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and()
        {
            AssertQuery<Order>(
                os => os.Where(o => (o.OrderID & 11067) == 11067),
                entryCount: 2);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] & 11067) = 11067))");
        }

        [ConditionalFact]
        public virtual void Where_bitwise_xor()
        {
            AssertQuery<Order>(
                os => os.Where(o => (o.OrderID ^ 10248) == 0),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] ^ 10248) = 0))");
        }

        [ConditionalFact]
        public virtual void Where_bitwise_leftshift()
        {
            AssertQuery<Order>(
                os => os.Where(o => (o.OrderID << 1) == 20496),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] << 1) = 20496))");
        }

        [ConditionalFact]
        public virtual void Where_bitwise_rightshift()
        {
            AssertQuery<Order>(
                os => os.Where(o => (o.OrderID >> 1) == 5124),
                entryCount: 2);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] >> 1) = 5124))");
        }

        [ConditionalFact]
        public virtual void Where_logical_and()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "Seattle" && c.ContactTitle == "Owner"),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""City""] = ""Seattle"") AND (c[""ContactTitle""] = ""Owner"")))");
        }

        [ConditionalFact]
        public virtual void Where_logical_or()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" || c.CustomerID == "ANATR"),
                entryCount: 2);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] = ""ALFKI"") OR (c[""CustomerID""] = ""ANATR"")))");
        }

        [ConditionalFact]
        public virtual void Where_logical_not()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => !(c.City != "Seattle")),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT((c[""City""] != ""Seattle"")))");
        }

        [ConditionalFact]
        public virtual void Where_equality()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == 2),
                entryCount: 5);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = 2))");
        }

        [ConditionalFact]
        public virtual void Where_inequality()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo != 2),
                entryCount: 4);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] != 2))");
        }

        [ConditionalFact]
        public virtual void Where_greaterthan()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo > 2),
                entryCount: 3);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] > 2))");
        }

        [ConditionalFact]
        public virtual void Where_greaterthanorequal()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo >= 2),
                entryCount: 8);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] >= 2))");
        }

        [ConditionalFact]
        public virtual void Where_lessthan()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo < 2),
                entryCount: 0);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] < 2))");
        }

        [ConditionalFact]
        public virtual void Where_lessthanorequal()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo <= 2),
                entryCount: 5);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] <= 2))");
        }

        [ConditionalFact]
        public virtual void Where_string_concat()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID + "END" == "ALFKIEND"),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] || ""END"") = ""ALFKIEND""))");
        }

        [ConditionalFact]
        public virtual void Where_unary_minus()
        {
            AssertQuery<Order>(
                os => os.Where(o => -o.OrderID == -10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (-(c[""OrderID""]) = -10248))");
        }

        [ConditionalFact]
        public virtual void Where_bitwise_not()
        {
            AssertQuery<Order>(
                os => os.Where(o => ~o.OrderID == -10249),
                entryCount: 1);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (~(c[""OrderID""]) = -10249))");
        }

        [ConditionalFact]
        public virtual void Where_ternary()
        {
            AssertQuery<Customer>(
#pragma warning disable IDE0029 // Use coalesce expression
                cs => cs.Where(c => (c.Region != null ? c.Region : "SP") == "BC"),
#pragma warning restore IDE0029 // Use coalesce expression
                entryCount: 2);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""Region""] != null) ? c[""Region""] : ""SP"") = ""BC""))");
        }

        [ConditionalFact]
        public virtual void Where_coalesce()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => (c.Region ?? "SP") == "BC"),
                entryCount: 2);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""Region""] ?? ""SP"") = ""BC""))");
        }

        [ConditionalFact]
        public virtual void Where_simple_closure()
        {
            var city = "London";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);

            AssertSql(
                @"@__city_0='London'

SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_0))");
        }

        [ConditionalFact]
        public virtual void Where_simple_closure_nullable_type()
        {
            int? reportsTo = 2;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);

            reportsTo = 5;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = null;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);

            AssertSql(
                @"@__reportsTo_0='2'

SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__reportsTo_0))",
                //
                @"@__reportsTo_0='5'

SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__reportsTo_0))",
                //
                @"@__reportsTo_0=null

SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__reportsTo_0))");
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"),
                entryCount: 6);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""Title""] = ""Sales Representative""))");
        }
    }
}
