// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Globalization;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class UdfDbFunctionOracleTest : UdfDbFunctionTestBase<UdfDbFunctionOracleTest.OracleUdfFixture>
    {
        public UdfDbFunctionOracleTest(OracleUdfFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class OracleUdfFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "UDFDbFunctionOracleTests";
            protected override Type ContextType { get; } = typeof(UDFSqlContext);
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                base.AddOptions(builder);
                return builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.QueryClientEvaluationWarning));
            }

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""CustomerOrderCount"" (customerId INTEGER)
RETURN INTEGER IS
  result INTEGER;
BEGIN
  SELECT COUNT(""Id"") 
  INTO result
  FROM ""Orders"" 
  WHERE ""CustomerId"" = customerId;
  RETURN result;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""StarValue"" (starCount INTEGER, value NVARCHAR2)
RETURN NVARCHAR2 IS
BEGIN
  RETURN LPAD(value, starCount + LENGTH(value), '*');
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""DollarValue"" (starCount INTEGER, value NVARCHAR2)
RETURN NVARCHAR2 IS
BEGIN
  RETURN LPAD(value, starCount + LENGTH(value), '$');
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""GetReportingPeriodStartDate"" (period INTEGER)
RETURN TIMESTAMP IS
BEGIN
	RETURN TO_TIMESTAMP('01/01/1998', 'MM/DD/YYYY');
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""GetCustomerWithMostOrdersAfterDate"" (searchDate TIMESTAMP)
RETURN INTEGER IS
  result INTEGER;
BEGIN
  SELECT ""CustomerId""
  INTO result
  FROM ""Orders""
  WHERE ""OrderDate"" > searchDate
  GROUP BY ""CustomerId""
  ORDER BY COUNT(""Id"") DESC
  FETCH FIRST 1 ROWS ONLY;
  RETURN result;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""IsTopCustomer"" (customerId INTEGER)
RETURN INTEGER IS
BEGIN
  IF (customerId = 1) THEN
    RETURN 1;
  ELSE	
    RETURN 0;
  END IF;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""IsDate"" (value NVARCHAR2)
RETURN INTEGER IS
BEGIN
  RETURN 0;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""len"" (value NVARCHAR2)
RETURN INTEGER IS
BEGIN
  RETURN LENGTH(value);
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION ""IdentityString"" (customerName NVARCHAR2)
RETURN NVARCHAR2 IS
BEGIN
    RETURN customerName;
END;");                
                
                var order11 = new Order { Name = "Order11", ItemCount = 4, OrderDate = DateTime.Parse("1/20/2000", CultureInfo.InvariantCulture) };
                var order12 = new Order { Name = "Order12", ItemCount = 8, OrderDate = DateTime.Parse("2/21/2000", CultureInfo.InvariantCulture) };
                var order13 = new Order { Name = "Order13", ItemCount = 15, OrderDate = DateTime.Parse("3/20/2000", CultureInfo.InvariantCulture) };
                var order21 = new Order { Name = "Order21", ItemCount = 16, OrderDate = DateTime.Parse("4/21/2000", CultureInfo.InvariantCulture) };
                var order22 = new Order { Name = "Order22", ItemCount = 23, OrderDate = DateTime.Parse("5/20/2000", CultureInfo.InvariantCulture) };
                var order31 = new Order { Name = "Order31", ItemCount = 42, OrderDate = DateTime.Parse("6/21/2000", CultureInfo.InvariantCulture) };

                var customer1 = new Customer { FirstName = "Customer", LastName = "One", Orders = new List<Order> { order11, order12, order13 } };
                var customer2 = new Customer { FirstName = "Customer", LastName = "Two", Orders = new List<Order> { order21, order22 } };
                var customer3 = new Customer { FirstName = "Customer", LastName = "Three", Orders = new List<Order> { order31 } };

                ((UDFSqlContext)context).Customers.AddRange(customer1, customer2, customer3);
                ((UDFSqlContext)context).Orders.AddRange(order11, order12, order13, order21, order22, order31);
                context.SaveChanges();
            }
        }
    }
}
