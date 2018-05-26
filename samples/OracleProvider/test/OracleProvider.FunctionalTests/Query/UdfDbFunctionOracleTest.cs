// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
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


        #region Table Valued Tests

        [Fact(Skip = "TODO")]
        public override void TVF_Stand_Alone()
        {
            base.TVF_Stand_Alone();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Stand_Alone_With_Translation()
        {
            base.TVF_Stand_Alone_With_Translation();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Stand_Alone_Parameter()
        {
            base.TVF_Stand_Alone_Parameter();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Stand_Alone_Nested()
        {
            base.TVF_Stand_Alone_Nested();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_CrossApply_Correlated_Select_Anonymous()
        {
            base.TVF_CrossApply_Correlated_Select_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_Direct_In_Anonymous()
        {
            base.TVF_Select_Direct_In_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_Correlated_Direct_In_Anonymous()
        {
            base.TVF_Select_Correlated_Direct_In_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
        {
            base.TVF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_Correlated_Subquery_In_Anonymous()
        {
            base.TVF_Select_Correlated_Subquery_In_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_Correlated_Subquery_In_Anonymous_Nested()
        {
            base.TVF_Select_Correlated_Subquery_In_Anonymous_Nested();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_NonCorrelated_Subquery_In_Anonymous()
        {
            base.TVF_Select_NonCorrelated_Subquery_In_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter()
        {
            base.TVF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Correlated_Select_In_Anonymous()
        {
            base.TVF_Correlated_Select_In_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_CrossApply_Correlated_Select_Result()
        {
            base.TVF_CrossApply_Correlated_Select_Result();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_CrossJoin_Not_Correlated()
        {
            base.TVF_CrossJoin_Not_Correlated();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_CrossJoin_Parameter()
        {
            base.TVF_CrossJoin_Parameter();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Join()
        {
            base.TVF_Join();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_LeftJoin_Select_Anonymous()
        {
            base.TVF_LeftJoin_Select_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_LeftJoin_Select_Result()
        {
            base.TVF_LeftJoin_Select_Result();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_OuterApply_Correlated_Select_TVF()
        {
            base.TVF_OuterApply_Correlated_Select_TVF();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_OuterApply_Correlated_Select_DbSet()
        {
            base.TVF_OuterApply_Correlated_Select_DbSet();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_OuterApply_Correlated_Select_Anonymous()
        {
            base.TVF_OuterApply_Correlated_Select_Anonymous();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Nested()
        {
            base.TVF_Nested();
        }

        [Fact(Skip = "TODO")]
        public override void TVF_Correlated_Nested_Func_Call()
        {
            base.TVF_Correlated_Nested_Func_Call();
        }

        #endregion


        public class Oracle : BaseUdfFixture
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
