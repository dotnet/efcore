// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public class UdfDbFunctionSqlServerTests : UdfDbFunctionTestBase<UdfDbFunctionSqlServerTests.SqlServer>
    {
        public UdfDbFunctionSqlServerTests(SqlServer fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Scalar Tests

        #region Static

        public override void Scalar_Function_Extension_Method_Static()
        {
            base.Scalar_Function_Extension_Method_Static();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE IsDate([c].[FirstName]) = CAST(0 AS bit)");
        }

        public override void Scalar_Function_With_Translator_Translates_Static()
        {
            base.Scalar_Function_With_Translator_Translates_Static();

            AssertSql(
                @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Constant_Parameter_Static()
        {
            base.Scalar_Function_Constant_Parameter_Static();

            AssertSql(
                @"@__customerId_0='1'

SELECT [dbo].[CustomerOrderCount](@__customerId_0)
FROM [Customers] AS [c]");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Parameter_Static();

            AssertSql(
                @"@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Nested_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Nested_Static();

            AssertSql(
                @"@__starCount_1='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_1, [dbo].[CustomerOrderCount](@__customerId_0)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Where_Correlated_Static()
        {
            base.Scalar_Function_Where_Correlated_Static();

            AssertSql(
                @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].[IsTopCustomer]([c].[Id]) = CAST(1 AS bit)");
        }

        public override void Scalar_Function_Where_Not_Correlated_Static()
        {
            base.Scalar_Function_Where_Not_Correlated_Static();

            AssertSql(
                @"@__startDate_0='2000-04-01T00:00:00.0000000' (Nullable = true)

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].[GetCustomerWithMostOrdersAfterDate](@__startDate_0) = [c].[Id]");
        }

        public override void Scalar_Function_Where_Parameter_Static()
        {
            base.Scalar_Function_Where_Parameter_Static();

            AssertSql(
                @"@__period_0='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](@__period_0))");
        }

        public override void Scalar_Function_Where_Nested_Static()
        {
            base.Scalar_Function_Where_Nested_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](0))");
        }

        public override void Scalar_Function_Let_Correlated_Static()
        {
            base.Scalar_Function_Let_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        public override void Scalar_Function_Let_Not_Correlated_Static()
        {
            base.Scalar_Function_Let_Not_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        public override void Scalar_Function_Let_Not_Parameter_Static()
        {
            base.Scalar_Function_Let_Not_Parameter_Static();

            AssertSql(
                @"@__customerId_0='2'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Let_Nested_Static()
        {
            base.Scalar_Function_Let_Nested_Static();

            AssertSql(
                @"@__starCount_0='3'
@__customerId_1='1'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_0, [dbo].[CustomerOrderCount](@__customerId_1)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_1");
        }

        public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]");
        }

        public override void Scalar_Nested_Function_UDF_BCL_Static()
        {
            base.Scalar_Nested_Function_UDF_BCL_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].[CustomerOrderCount](ABS([c].[Id]))");
        }

        public override void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            base.Nullable_navigation_property_access_preserves_schema_for_sql_function();

            AssertSql(
                @"SELECT TOP(1) [dbo].[IdentityString]([c].[FirstName])
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerId] = [c].[Id]
ORDER BY [o].[Id]");
        }

        public override void Scalar_Function_SqlFragment_Static()
        {
            base.Scalar_Function_SqlFragment_Static();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[LastName] = 'Two'");
        }

        #endregion

        #region Instance

        public override void Scalar_Function_Non_Static()
        {
            base.Scalar_Function_Non_Static();

            AssertSql(
                @"SELECT TOP(2) [dbo].[StarValue](4, [c].[Id]) AS [Id], [dbo].[DollarValue](2, [c].[LastName]) AS [LastName]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        public override void Scalar_Function_Extension_Method_Instance()
        {
            base.Scalar_Function_Extension_Method_Instance();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE IsDate([c].[FirstName]) = CAST(0 AS bit)");
        }

        public override void Scalar_Function_With_Translator_Translates_Instance()
        {
            base.Scalar_Function_With_Translator_Translates_Instance();

            AssertSql(
                @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Constant_Parameter_Instance()
        {
            base.Scalar_Function_Constant_Parameter_Instance();

            AssertSql(
                @"@__customerId_1='1'

SELECT [dbo].[CustomerOrderCount](@__customerId_1)
FROM [Customers] AS [c]");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Parameter_Instance();

            AssertSql(
                @"@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Nested_Instance();

            AssertSql(
                @"@__starCount_2='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_2, [dbo].[CustomerOrderCount](@__customerId_0)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        public override void Scalar_Function_Where_Correlated_Instance()
        {
            base.Scalar_Function_Where_Correlated_Instance();

            AssertSql(
                @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].[IsTopCustomer]([c].[Id]) = CAST(1 AS bit)");
        }

        public override void Scalar_Function_Where_Not_Correlated_Instance()
        {
            base.Scalar_Function_Where_Not_Correlated_Instance();

            AssertSql(
                @"@__startDate_1='2000-04-01T00:00:00.0000000' (Nullable = true)

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].[GetCustomerWithMostOrdersAfterDate](@__startDate_1) = [c].[Id]");
        }

        public override void Scalar_Function_Where_Parameter_Instance()
        {
            base.Scalar_Function_Where_Parameter_Instance();

            AssertSql(
                @"@__period_1='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](@__period_1))");
        }

        public override void Scalar_Function_Where_Nested_Instance()
        {
            base.Scalar_Function_Where_Nested_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](0))");
        }

        public override void Scalar_Function_Let_Correlated_Instance()
        {
            base.Scalar_Function_Let_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        public override void Scalar_Function_Let_Not_Correlated_Instance()
        {
            base.Scalar_Function_Let_Not_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        public override void Scalar_Function_Let_Not_Parameter_Instance()
        {
            base.Scalar_Function_Let_Not_Parameter_Instance();

            AssertSql(
                @"@__customerId_1='2'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_1");
        }

        public override void Scalar_Function_Let_Nested_Instance()
        {
            base.Scalar_Function_Let_Nested_Instance();

            AssertSql(
                @"@__starCount_1='3'
@__customerId_2='1'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_1, [dbo].[CustomerOrderCount](@__customerId_2)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_2");
        }

        public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]");
        }

        public override void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            base.Scalar_Nested_Function_BCL_UDF_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].[CustomerOrderCount]([c].[Id]))");
        }

        public override void Scalar_Nested_Function_UDF_BCL_Instance()
        {
            base.Scalar_Nested_Function_UDF_BCL_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].[CustomerOrderCount](ABS([c].[Id]))");
        }

        #endregion

        #endregion

        #region Queryable Function Tests

        public override void QF_Stand_Alone()
        {
            base.QF_Stand_Alone();

            AssertSql(@"SELECT [t].[AmountSold], [t].[ProductId]
FROM [dbo].[GetTopTwoSellingProducts]() AS [t]
ORDER BY [t].[ProductId]");
        }

        public override void QF_Stand_Alone_Parameter()
        {
            base.QF_Stand_Alone_Parameter();

            AssertSql(
                @"@__customerId_1='1'

SELECT [o].[Count], [o].[CustomerId], [o].[Year]
FROM [dbo].[GetCustomerOrderCountByYear](@__customerId_1) AS [o]
ORDER BY [o].[Count] DESC");
        }

        public override void QF_CrossApply_Correlated_Select_Anonymous()
        {
            base.QF_CrossApply_Correlated_Select_Anonymous();

            AssertSql(@"SELECT [c].[Id], [c].[LastName], [o].[Year], [o].[Count]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [o]
ORDER BY [c].[Id], [o].[Year]");
        }

        public override void QF_CrossApply_Correlated_Select_QF_Type()
        {
            base.QF_CrossApply_Correlated_Select_QF_Type();

            AssertSql(@"SELECT [o].[Count], [o].[CustomerId], [o].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [o]
ORDER BY [o].[Year]");
        }


        public override void QF_Select_Direct_In_Anonymous()
        {
            base.QF_Select_Direct_In_Anonymous();

            AssertSql(@"SELECT [t].[AmountSold], [t].[ProductId]
FROM [dbo].[GetTopTwoSellingProducts]() AS [t]",

@"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        public override void QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
        {
            base.QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous();

            AssertSql(@"SELECT [c].[Id], [m].[OrderId], [m].[CustomerId], [m].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetOrdersWithMultipleProducts]([dbo].[AddValues]([c].[Id], 1)) AS [m]
WHERE [c].[Id] = 1
ORDER BY [c].[Id], [m].[OrderId]");
        }

        public override void QF_Select_Correlated_Subquery_In_Anonymous()
        {
            base.QF_Select_Correlated_Subquery_In_Anonymous();

            AssertSql(@"SELECT [c].[Id], [t].[OrderId], [t].[CustomerId], [t].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [m].[OrderId], [m].[CustomerId], [m].[OrderDate]
    FROM [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [m]
    WHERE DATEPART(day, [m].[OrderDate]) = 21
) AS [t]
ORDER BY [c].[Id], [t].[OrderId]");
        }

        public override void QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF()
        {
            base.QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF();

            AssertSql(@"SELECT [o].[CustomerId], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT [c].[Id], [c].[FirstName], [c].[LastName], [m].[OrderId], [m].[CustomerId], [m].[OrderDate]
    FROM [Customers] AS [c]
    CROSS APPLY [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [m]
) AS [t] ON [o].[Id] = [t].[OrderId]");
        }

        public override void QF_Select_Correlated_Subquery_In_Anonymous_Nested()
        {
            base.QF_Select_Correlated_Subquery_In_Anonymous_Nested();

            AssertSql(@"SELECT [t].[AmountSold], [t].[ProductId]
FROM [dbo].[GetTopTwoSellingProducts]() AS [t]",

                    @"SELECT [c].[Id], [t].[OrderId], [t].[OrderId0], [t].[CustomerId], [t].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [m].[OrderId], [m0].[OrderId] AS [OrderId0], [m0].[CustomerId], [m0].[OrderDate]
    FROM [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [m]
    OUTER APPLY [dbo].[GetOrdersWithMultipleProducts]([m].[CustomerId]) AS [m0]
    WHERE DATEPART(day, [m].[OrderDate]) = 21
) AS [t]
ORDER BY [c].[Id], [t].[OrderId], [t].[OrderId0]");
        }

        public override void QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections()
        {
            base.QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections();

            AssertSql(@"SELECT [c].[Id], [t0].[ProductId], [t1].[Id], [t1].[City], [t1].[CustomerId], [t1].[State], [t1].[Street]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [t].[ProductId]
    FROM [dbo].[GetTopTwoSellingProducts]() AS [t]
    WHERE [t].[AmountSold] = 249
) AS [t0]
LEFT JOIN (
    SELECT [a].[Id], [a].[City], [a].[CustomerId], [a].[State], [a].[Street]
    FROM [Addresses] AS [a]
    WHERE [a].[State] = N'NY'
) AS [t1] ON [c].[Id] = [t1].[CustomerId]
ORDER BY [c].[Id], [t1].[Id]");
        }

        public override void QF_Select_NonCorrelated_Subquery_In_Anonymous()
        {
            base.QF_Select_NonCorrelated_Subquery_In_Anonymous();

            AssertSql(@"SELECT [c].[Id], [t0].[ProductId]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [t].[ProductId]
    FROM [dbo].[GetTopTwoSellingProducts]() AS [t]
    WHERE [t].[AmountSold] = 249
) AS [t0]
ORDER BY [c].[Id]");
        }

        public override void QF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter()
        {
            base.QF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter();

            AssertSql(
                @"@__amount_1='27' (Nullable = true)

SELECT [c].[Id], [t0].[ProductId]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [t].[ProductId]
    FROM [dbo].[GetTopTwoSellingProducts]() AS [t]
    WHERE [t].[AmountSold] = @__amount_1
) AS [t0]
ORDER BY [c].[Id]");
        }

        public override void QF_Correlated_Select_In_Anonymous()
        {
            base.QF_Correlated_Select_In_Anonymous();

            AssertSql(@"SELECT [c].[Id], [c].[LastName], [m].[OrderId], [m].[CustomerId], [m].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [m]
ORDER BY [c].[Id], [m].[OrderId]");
        }

        public override void QF_CrossApply_Correlated_Select_Result()
        {
            base.QF_CrossApply_Correlated_Select_Result();

            AssertSql(@"SELECT [o].[Count], [o].[CustomerId], [o].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [o]
ORDER BY [o].[Count] DESC, [o].[Year] DESC");
        }

        public override void QF_CrossJoin_Not_Correlated()
        {
            base.QF_CrossJoin_Not_Correlated();

            AssertSql(
                @"SELECT [c].[Id], [c].[LastName], [o].[Year], [o].[Count]
FROM [Customers] AS [c]
CROSS JOIN [dbo].[GetCustomerOrderCountByYear](2) AS [o]
WHERE [c].[Id] = 2
ORDER BY [o].[Count]");
        }

        public override void QF_CrossJoin_Parameter()
        {
            base.QF_CrossJoin_Parameter();

            AssertSql(
                @"@__custId_1='2'

SELECT [c].[Id], [c].[LastName], [o].[Year], [o].[Count]
FROM [Customers] AS [c]
CROSS JOIN [dbo].[GetCustomerOrderCountByYear](@__custId_1) AS [o]
WHERE [c].[Id] = @__custId_1
ORDER BY [o].[Count]");
        }

        public override void QF_Join()
        {
            base.QF_Join();

            AssertSql(@"SELECT [p].[Id], [p].[Name], [t].[AmountSold]
FROM [Products] AS [p]
INNER JOIN [dbo].[GetTopTwoSellingProducts]() AS [t] ON [p].[Id] = [t].[ProductId]
ORDER BY [p].[Id]");
        }

        public override void QF_LeftJoin_Select_Anonymous()
        {
            base.QF_LeftJoin_Select_Anonymous();

            AssertSql(@"SELECT [p].[Id], [p].[Name], [t].[AmountSold]
FROM [Products] AS [p]
LEFT JOIN [dbo].[GetTopTwoSellingProducts]() AS [t] ON [p].[Id] = [t].[ProductId]
ORDER BY [p].[Id] DESC");
        }

        public override void QF_LeftJoin_Select_Result()
        {
            base.QF_LeftJoin_Select_Result();

            AssertSql(@"SELECT [t].[AmountSold], [t].[ProductId]
FROM [Products] AS [p]
LEFT JOIN [dbo].[GetTopTwoSellingProducts]() AS [t] ON [p].[Id] = [t].[ProductId]
ORDER BY [p].[Id] DESC");
        }

        public override void QF_OuterApply_Correlated_Select_QF()
        {
            base.QF_OuterApply_Correlated_Select_QF();

            AssertSql(@"SELECT [o].[Count], [o].[CustomerId], [o].[Year]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [o]
ORDER BY [c].[Id], [o].[Year]");
        }

        public override void QF_OuterApply_Correlated_Select_Entity()
        {
            base.QF_OuterApply_Correlated_Select_Entity();

            AssertSql(
                @"SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [o]
WHERE [o].[Year] = 2000
ORDER BY [c].[Id], [o].[Year]");
        }

        public override void QF_OuterApply_Correlated_Select_Anonymous()
        {
            base.QF_OuterApply_Correlated_Select_Anonymous();

            AssertSql(@"SELECT [c].[Id], [c].[LastName], [o].[Year], [o].[Count]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [o]
ORDER BY [c].[Id], [o].[Year]");
        }

        public override void QF_Nested()
        {
            base.QF_Nested();

            AssertSql(@"@__custId_1='2'

SELECT [c].[Id], [c].[LastName], [o].[Year], [o].[Count]
FROM [Customers] AS [c]
CROSS JOIN [dbo].[GetCustomerOrderCountByYear]([dbo].[AddValues](1, 1)) AS [o]
WHERE [c].[Id] = @__custId_1
ORDER BY [o].[Year]");
        }

        public override void QF_Correlated_Nested_Func_Call()
        {
            base.QF_Correlated_Nested_Func_Call();

            AssertSql(@"@__custId_1='2'

SELECT [c].[Id], [o].[Count], [o].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([dbo].[AddValues]([c].[Id], 1)) AS [o]
WHERE [c].[Id] = @__custId_1");
        }

        public override void QF_Correlated_Func_Call_With_Navigation()
        {
            base.QF_Correlated_Func_Call_With_Navigation();

            AssertSql(@"SELECT [c].[Id], [t].[LastName], [t].[OrderId], [t].[Id]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c0].[LastName], [m].[OrderId], [c0].[Id]
    FROM [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [m]
    INNER JOIN [Customers] AS [c0] ON [m].[CustomerId] = [c0].[Id]
) AS [t]
ORDER BY [c].[Id], [t].[OrderId], [t].[Id]");
        }

        #endregion

        public class SqlServer : UdfFixtureBase
        {
            protected override string StoreName { get; } = "UDFDbFunctionSqlServerTests";
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            protected override void Seed(DbContext context)
            {
                base.Seed(context);

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].[CustomerOrderCount] (@customerId int)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select count(id) from orders where customerId = @customerId);
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function[dbo].[StarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('*', @starCount) + @value
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function[dbo].[DollarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('$', @starCount) + @value
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].[GetReportingPeriodStartDate] (@period int)
                                                    returns DateTime
                                                    as
                                                    begin
                                                        return '1998-01-01'
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].[GetCustomerWithMostOrdersAfterDate] (@searchDate Date)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select top 1 customerId
                                                                from orders
                                                                where orderDate > @searchDate
                                                                group by CustomerId
                                                                order by count(id) desc)
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].[IsTopCustomer] (@customerId int)
                                                    returns bit
                                                    as
                                                    begin
                                                        if(@customerId = 1)
                                                            return 1

                                                        return 0
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].[IdentityString] (@customerName nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return @customerName;
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].GetCustomerOrderCountByYear(@customerId int)
                                                    returns @reports table
                                                    (
                                                        CustomerId int not null,
                                                        Count int not null,
                                                        Year int not null
                                                    )
                                                    as
                                                    begin
                                                        insert into @reports
                                                        select @customerId, count(id), year(orderDate)
                                                        from orders
                                                        where customerId = @customerId
                                                        group by customerId, year(orderDate)
                                                        order by year(orderDate)

                                                        return
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].GetTopTwoSellingProducts()
                                                    returns @products table
                                                    (
                                                        ProductId int not null,
                                                        AmountSold int
                                                    )
                                                    as
                                                    begin
                                                        insert into @products
                                                        select top 2 ProductID, sum(Quantity) as totalSold
                                                        from lineItem
                                                        group by ProductID
                                                        order by totalSold desc

                                                        return
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].GetTopSellingProductsForCustomer(@customerId int)
                                                    returns @products table
                                                    (
                                                        ProductId int not null,
                                                        AmountSold int
                                                    )
                                                    as
                                                    begin
                                                        insert into @products
                                                        select ProductID, sum(Quantity) as totalSold
                                                        from lineItem li
                                                        join orders o on o.id = li.orderId
                                                        where o.customerId = @customerId
                                                        group by ProductID

                                                        return
                                                    end");

                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].GetOrdersWithMultipleProducts(@customerId int)
                                                    returns @orders table
                                                    (
                                                        OrderId int not null,
                                                        CustomerId int not null,
                                                        OrderDate dateTime2
                                                    )
                                                    as
                                                    begin
                                                        insert into @orders
                                                        select o.id, @customerId, OrderDate
                                                        from orders o
                                                        join lineItem li on o.id = li.orderId
                                                        where o.customerId = @customerId
                                                        group by o.id, OrderDate
                                                        having count(productId) > 1

                                                        return
                                                    end");


                context.Database.ExecuteSqlRaw(
                    @"create function [dbo].[AddValues] (@a int, @b int)
                                                    returns int
                                                    as
                                                    begin
                                                        return @a + @b;
                                                    end");

                context.SaveChanges();
            }
        }

        public void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
