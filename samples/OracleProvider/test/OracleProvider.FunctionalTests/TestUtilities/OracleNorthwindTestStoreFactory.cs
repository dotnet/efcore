// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class OracleNorthwindTestStoreFactory : OracleTestStoreFactory
    {
        public const string Name = "Northwind";
        public static readonly string NorthwindConnectionString = OracleTestStore.CreateConnectionString(Name);
        public new static OracleNorthwindTestStoreFactory Instance { get; } = new OracleNorthwindTestStoreFactory();

        protected OracleNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
        {
            var oracleTestStore = OracleTestStore.GetOrCreateInitialized(Name, "Northwind.sql");

            oracleTestStore.ExecuteNonQuery(
                @"CREATE OR REPLACE PROCEDURE ""Ten Most Expensive Products""(cur OUT sys_refcursor) AS
BEGIN
  OPEN cur FOR
  SELECT ""ProductName"" AS ""TenMostExpensiveProducts"", ""UnitPrice""
  FROM ""Products""
  ORDER BY ""UnitPrice"" DESC
  FETCH NEXT 10 ROWS ONLY;
END;");

            oracleTestStore.ExecuteNonQuery(
                @"CREATE OR REPLACE PROCEDURE ""CustOrderHist""(CustomerID IN NCHAR, cur OUT sys_refcursor) AS
BEGIN
  OPEN cur FOR
  SELECT ""ProductName"", SUM(""Quantity"") AS ""Total""
  FROM ""Products"" P, ""Order Details"" OD, ""Orders"" O, ""Customers"" C
  WHERE C.""CustomerID"" = CustomerID
  AND C.""CustomerID"" = O.""CustomerID"" AND O.""OrderID"" = OD.""OrderID"" AND OD.""ProductID"" = P.""ProductID""
  GROUP BY ""ProductName"";
END;");
            oracleTestStore.ExecuteNonQuery(
                @"CREATE OR REPLACE PROCEDURE ""SimpleProcedure"" AS
BEGIN
  NULL;
END;");

            oracleTestStore.ExecuteNonQuery(
                @"CREATE OR REPLACE PROCEDURE ""SimpleProcedure2""(""CustomerID"" NCHAR) AS
BEGIN
  NULL;
END;");

            return oracleTestStore;
        }
    }
}
