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
    public class UdfDbFunctionOracleTest : UdfDbFunctionTestBase<UdfDbFunctionOracleTest.Oracle>
    {
        public UdfDbFunctionOracleTest(Oracle fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class Oracle : UdfFixtureBase
        {
            protected override string StoreName { get; } = "UDFDbFunctionOracleTests";
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            protected override void Seed(DbContext context)
            {
                base.Seed(context);

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

                context.SaveChanges();
            }
        }
    }
}
