// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
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
                @"SELECT TOP 9 City, EmployeeID
FROM Employees",
                _fixture.Sql);
        }

        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override async Task Where_simple_async()
        {
            await base.Where_simple_async();

            Assert.Equal(
                @"SELECT Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM Customers",
                _fixture.Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT City, CompanyName
FROM Customers",
                _fixture.Sql);
        }

        public override async Task Select_project_filter_async()
        {
            await base.Select_project_filter_async();

            Assert.Equal(
                @"SELECT City, CompanyName
FROM Customers",
                _fixture.Sql);
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT City
FROM Customers",
                _fixture.Sql);
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
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

            Assert.Equal(3774, _fixture.Sql.Length);
            Assert.StartsWith(
                @"SELECT City, EmployeeID
FROM Employees

SELECT 1
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
FROM Employees

SELECT City, EmployeeID
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

        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"SELECT TOP 2 Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM (SELECT TOP 1 *
FROM Customers) AS t0
ORDER BY CustomerID",
                _fixture.Sql);
        }

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

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private class SqlLoggerFactory : ILoggerFactory
        {
            private const string ContextName = "__SQL";

            public ILogger Create(string name)
            {
                return Logger;
            }

            public void Init()
            {
                CallContext.LogicalSetData(ContextName, new SqlLogger());
            }

            public static SqlLogger Logger
            {
                get { return (SqlLogger)CallContext.LogicalGetData(ContextName); }
            }

            public class SqlLogger : ILogger
            {
                public readonly List<string> _sqlStatements = new List<string>();

                public bool WriteCore(
                    TraceType eventType,
                    int eventId,
                    object state,
                    Exception exception,
                    Func<object, Exception, string> formatter)
                {
                    if (eventId == RelationalLoggingEventIds.Sql)
                    {
                        var sql = formatter(state, exception);

                        _sqlStatements.Add(sql);

                        //Trace.WriteLine(sql);
                    }

                    return true;
                }
            }
        }

        private readonly SqlLoggerFactory _loggingFactory = new SqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly ImmutableDbContextOptions _configuration;
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

            _configuration
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(_testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();
        }

        public string Sql
        {
            get { return string.Join("\r\n\r\n", SqlLoggerFactory.Logger._sqlStatements); }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _configuration);
        }

        public void InitLogger()
        {
            _loggingFactory.Init();
        }
    }
}
