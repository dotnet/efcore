// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Xunit;
#if K10
using System.Threading;
#else
using System.Runtime.Remoting.Messaging;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    // TODO: Test non-SqlServer SQL (i.e. generated from Relational base) elsewhere
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Take_simple()
        {
            base.Take_simple();

            Assert.Equal(
                @"SELECT TOP 10 Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
ORDER BY CustomerID",
                _fixture.Sql);
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            Assert.Equal(
                @"SELECT TOP 10 CustomerID, City
FROM Customers
ORDER BY CustomerID",
                _fixture.Sql);
        }

        public override void Any_simple()
        {
            base.Any_simple();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Select_scalar()
        {
            base.Select_scalar();

            Assert.Equal(
                @"SELECT City
FROM Customers",
                _fixture.Sql);
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            Assert.Equal(
                @"SELECT TOP 9 City, Country, EmployeeID
FROM Employees",
                _fixture.Sql);
        }

        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE City = @p0",
                _fixture.Sql);
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE City IS NULL",
                _fixture.Sql);
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE City IS NOT NULL",
                _fixture.Sql);
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE 1 = 1",
                _fixture.Sql);
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE 1 = 0",
                _fixture.Sql);
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE 1 = 0",
                _fixture.Sql);
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE 1 = 1",
                _fixture.Sql);
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE @p0 = City",
                _fixture.Sql);
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE City = City",
                _fixture.Sql);
        }

        public override async Task Where_simple_async()
        {
            await base.Where_simple_async();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE City = @p0",
                _fixture.Sql);
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            Assert.StartsWith(
                 @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees",
                 _fixture.Sql);
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.StartsWith(
                 @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE (City = @p0 OR City = @p1)

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees",
                 _fixture.Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.StartsWith(
                 @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE (City = @p0 AND Country = @p1)

SELECT City, Country, EmployeeID
FROM Employees
WHERE (City = @p0 AND Country = @p1)

SELECT City, Country, EmployeeID
FROM Employees
WHERE (City = @p0 AND Country = @p1)",
                 _fixture.Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT City, CompanyName
FROM Customers
WHERE City = @p0",
                _fixture.Sql);
        }

        public override async Task Select_project_filter_async()
        {
            await base.Select_project_filter_async();

            Assert.Equal(
                @"SELECT City, CompanyName
FROM Customers
WHERE City = @p0",
                _fixture.Sql);
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT City
FROM Customers
WHERE City = @p0",
                _fixture.Sql);
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees",
                _fixture.Sql);
        }

        public override void Join_customers_orders()
        {
            base.Join_customers_orders();

            Assert.Equal(89, _fixture.Sql.Length);
            Assert.Equal(
                @"SELECT CustomerID, OrderID
FROM Orders

SELECT CustomerID, ContactName
FROM Customers",
                _fixture.Sql);
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(4512, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT City, Country, EmployeeID
FROM Employees

SELECT 1
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees

SELECT 1
FROM Employees",
                _fixture.Sql);
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT CustomerID, OrderDate, OrderID
FROM Orders",
                _fixture.Sql);
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            Assert.Equal(4341, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
ORDER BY CustomerID DESC

SELECT City
FROM Employees
ORDER BY City

SELECT City
FROM Employees
ORDER BY City",
                _fixture.Sql);
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            Assert.Equal(
                @"SELECT CustomerID, OrderDate, OrderID
FROM Orders

SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        // TODO: Single
//        public override void Take_with_single()
//        {
//            base.Take_with_single();
//
//            Assert.Equal(
//                @"SELECT TOP 2 Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
//FROM (SELECT TOP 1 *
//FROM Customers) AS t0
//ORDER BY CustomerID",
//                _fixture.Sql);
//        }

        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            Assert.Equal(
                @"SELECT DISTINCT City
FROM Customers",
                _fixture.Sql);
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            Assert.Equal(
                @"SELECT CustomerID, City
FROM Customers
ORDER BY CustomerID",
                _fixture.Sql);
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            // TODO: 
            //            Assert.Equal(
            //                @"SELECT DISTINCT City
            //FROM Customers
            //ORDER BY City",
            //                _fixture.Sql);
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            Assert.Equal(1194, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT City, Country, EmployeeID
FROM Employees
ORDER BY EmployeeID

SELECT City, Country, EmployeeID
FROM Employees

SELECT City, Country, EmployeeID
FROM Employees
ORDER BY EmployeeID

SELECT City, Country, EmployeeID
FROM Employees",
                _fixture.Sql);
        }

        public override void Where_false()
        {
            base.Where_false();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE 1 = 0",
                _fixture.Sql);
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            Assert.Equal(
                @"SELECT TOP 9 EmployeeID
FROM Employees",
                _fixture.Sql);
        }

        public override void Where_true()
        {
            base.Where_true();

            Assert.Equal(
                 @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE 1 = 1",
                 _fixture.Sql);
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            Assert.Equal(
                 @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                 _fixture.Sql);
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            Assert.Equal(
                    @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                    _fixture.Sql);
        }

        // TODO: Single
//        public override void Single_Predicate()
//        {
//            base.Single_Predicate();
//
//            Assert.Equal(
//                    @"SELECT TOP 2 Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
//FROM Customers
//WHERE CustomerID = @p0",
//                    _fixture.Sql);
//        }

        public override void Projection_when_null_value()
        {
            base.Projection_when_null_value();

            Assert.Equal(
                     @"SELECT Region
FROM Customers",
                     _fixture.Sql);
        }
        public override void All_top_level()
        {
            base.All_top_level();

            // TODO:
            //            Assert.Equal(
            //                @"SELECT CASE WHEN (NOT EXISTS(
            //  SELECT NULL 
            //  FROM [Customers] AS t0
            //  WHERE NOT (t0.[ContactName] LIKE @p0 + '%')
            //  )) THEN 1 ELSE 0 END AS [value]",
            //                _fixture.Sql);
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE @p0 + '%'",
                _fixture.Sql);
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE ContactName + '%'",
                _fixture.Sql);
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE ContactName + '%'",
                _fixture.Sql);
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE @p0 + '%'",
                _fixture.Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE '%' + @p0",
                _fixture.Sql);
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE '%' + ContactName",
                _fixture.Sql);
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE '%' + ContactName",
                _fixture.Sql);
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers
WHERE ContactName LIKE '%' + @p0",
                _fixture.Sql);
        }

        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase, IDisposable
    {
        private readonly TestSqlLoggerFactory _loggingFactory = new TestSqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly TestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(_loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            _testDatabase = TestDatabase.Northwind().Result;

            _options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(_testDatabase.Connection.ConnectionString);
        }

        public string Sql
        {
            get { return string.Join("\r\n\r\n", TestSqlLoggerFactory.Logger._sqlStatements); }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public void InitLogger()
        {
            _loggingFactory.Init();
        }
    }
}
