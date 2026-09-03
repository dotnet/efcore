// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class UdfDbFunctionSqlServerTests : UdfDbFunctionTestBase<UdfDbFunctionSqlServerTests.SqlServer>
{
    public UdfDbFunctionSqlServerTests(SqlServer fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Scalar Tests

    #region Static

    public override void Scalar_Function_Extension_Method_Static()
    {
        base.Scalar_Function_Extension_Method_Static();

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE IsDate([c].[FirstName]) = CAST(0 AS bit)
""");
    }

    public override void Scalar_Function_With_Translator_Translates_Static()
    {
        base.Scalar_Function_With_Translator_Translates_Static();

        AssertSql(
            """
@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Constant_Parameter_Static()
    {
        base.Scalar_Function_Constant_Parameter_Static();

        AssertSql(
            """
@__customerId_0='1'

SELECT [dbo].[CustomerOrderCount](@__customerId_0)
FROM [Customers] AS [c]
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Correlated_Static();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Parameter_Static();

        AssertSql(
            """
@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Nested_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Nested_Static();

        AssertSql(
            """
@__starCount_1='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_1, [dbo].[CustomerOrderCount](@__customerId_0)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Where_Correlated_Static()
    {
        base.Scalar_Function_Where_Correlated_Static();

        AssertSql(
            """
SELECT LOWER(CONVERT(varchar(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].[IsTopCustomer]([c].[Id]) = CAST(1 AS bit)
""");
    }

    public override void Scalar_Function_Where_Not_Correlated_Static()
    {
        base.Scalar_Function_Where_Not_Correlated_Static();

        AssertSql(
            """
@__startDate_0='2000-04-01T00:00:00.0000000' (Nullable = true)

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].[GetCustomerWithMostOrdersAfterDate](@__startDate_0) = [c].[Id]
""");
    }

    public override void Scalar_Function_Where_Parameter_Static()
    {
        base.Scalar_Function_Where_Parameter_Static();

        AssertSql(
            """
@__period_0='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](@__period_0))
""");
    }

    public override void Scalar_Function_Where_Nested_Static()
    {
        base.Scalar_Function_Where_Nested_Static();

        AssertSql(
            """
SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](0))
""");
    }

    public override void Scalar_Function_Let_Correlated_Static()
    {
        base.Scalar_Function_Let_Correlated_Static();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2
""");
    }

    public override void Scalar_Function_Let_Not_Correlated_Static()
    {
        base.Scalar_Function_Let_Not_Correlated_Static();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2
""");
    }

    public override void Scalar_Function_Let_Not_Parameter_Static()
    {
        base.Scalar_Function_Let_Not_Parameter_Static();

        AssertSql(
            """
@__customerId_0='2'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Let_Nested_Static()
    {
        base.Scalar_Function_Let_Nested_Static();

        AssertSql(
            """
@__starCount_1='3'
@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_1, [dbo].[CustomerOrderCount](@__customerId_0)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
    {
        base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Static();

        AssertSql(
            """
SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]
""");
    }

    public override void Scalar_Nested_Function_UDF_BCL_Static()
    {
        base.Scalar_Nested_Function_UDF_BCL_Static();

        AssertSql(
            """
SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].[CustomerOrderCount](ABS([c].[Id]))
""");
    }

    public override void Nullable_navigation_property_access_preserves_schema_for_sql_function()
    {
        base.Nullable_navigation_property_access_preserves_schema_for_sql_function();

        AssertSql(
            """
SELECT TOP(1) [dbo].[IdentityString]([c].[FirstName])
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerId] = [c].[Id]
ORDER BY [o].[Id]
""");
    }

    public override void Compare_function_without_null_propagation_to_null()
    {
        base.Compare_function_without_null_propagation_to_null();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
WHERE [dbo].[IdentityString]([c].[FirstName]) IS NOT NULL
ORDER BY [c].[Id]
""");
    }

    public override void Compare_function_with_null_propagation_to_null()
    {
        base.Compare_function_with_null_propagation_to_null();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
WHERE [c].[FirstName] IS NOT NULL
ORDER BY [c].[Id]
""");
    }

    public override void Compare_non_nullable_function_to_null_gets_optimized()
    {
        base.Compare_non_nullable_function_to_null_gets_optimized();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
ORDER BY [c].[Id]
""");
    }

    public override void Compare_functions_returning_int_that_take_nullable_param_which_propagates_null()
    {
        base.Compare_functions_returning_int_that_take_nullable_param_which_propagates_null();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
WHERE ([dbo].[StringLength]([c].[FirstName]) <> [dbo].[StringLength]([c].[LastName]) OR [c].[FirstName] IS NULL OR [c].[LastName] IS NULL) AND ([c].[FirstName] IS NOT NULL OR [c].[LastName] IS NOT NULL)
ORDER BY [c].[Id]
""");
    }

    public override void Scalar_Function_SqlFragment_Static()
    {
        base.Scalar_Function_SqlFragment_Static();

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[LastName] = 'Two'
""");
    }

    public override void Scalar_Function_with_InExpression_translation()
    {
        base.Scalar_Function_with_InExpression_translation();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[FirstName], 0 + 1, 1) IN (N'A', N'B', N'C')
""");
    }

    public override void Scalar_Function_with_nested_InExpression_translation()
    {
        base.Scalar_Function_with_nested_InExpression_translation();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
WHERE CASE
    WHEN SUBSTRING([c].[FirstName], 0 + 1, 1) IN (N'A', N'B', N'C') AND SUBSTRING([c].[FirstName], 0 + 1, 1) IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END IN (CAST(1 AS bit), CAST(0 AS bit))
""");
    }

    #endregion

    #region Instance

    public override void Scalar_Function_Non_Static()
    {
        base.Scalar_Function_Non_Static();

        AssertSql(
            """
SELECT TOP(2) [dbo].[StarValue](4, [c].[Id]) AS [Id], [dbo].[DollarValue](2, [c].[LastName]) AS [LastName]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1
""");
    }

    public override void Scalar_Function_Extension_Method_Instance()
    {
        base.Scalar_Function_Extension_Method_Instance();

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE IsDate([c].[FirstName]) = CAST(0 AS bit)
""");
    }

    public override void Scalar_Function_With_Translator_Translates_Instance()
    {
        base.Scalar_Function_With_Translator_Translates_Instance();

        AssertSql(
            """
@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Constant_Parameter_Instance()
    {
        base.Scalar_Function_Constant_Parameter_Instance();

        AssertSql(
            """
@__customerId_1='1'

SELECT [dbo].[CustomerOrderCount](@__customerId_1)
FROM [Customers] AS [c]
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Correlated_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Parameter_Instance();

        AssertSql(
            """
@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Nested_Instance();

        AssertSql(
            """
@__starCount_2='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_2, [dbo].[CustomerOrderCount](@__customerId_0)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0
""");
    }

    public override void Scalar_Function_Where_Correlated_Instance()
    {
        base.Scalar_Function_Where_Correlated_Instance();

        AssertSql(
            """
SELECT LOWER(CONVERT(varchar(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].[IsTopCustomer]([c].[Id]) = CAST(1 AS bit)
""");
    }

    public override void Scalar_Function_Where_Not_Correlated_Instance()
    {
        base.Scalar_Function_Where_Not_Correlated_Instance();

        AssertSql(
            """
@__startDate_1='2000-04-01T00:00:00.0000000' (Nullable = true)

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].[GetCustomerWithMostOrdersAfterDate](@__startDate_1) = [c].[Id]
""");
    }

    public override void Scalar_Function_Where_Parameter_Instance()
    {
        base.Scalar_Function_Where_Parameter_Instance();

        AssertSql(
            """
@__period_1='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](@__period_1))
""");
    }

    public override void Scalar_Function_Where_Nested_Instance()
    {
        base.Scalar_Function_Where_Nested_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](0))
""");
    }

    public override void Scalar_Function_Let_Correlated_Instance()
    {
        base.Scalar_Function_Let_Correlated_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2
""");
    }

    public override void Scalar_Function_Let_Not_Correlated_Instance()
    {
        base.Scalar_Function_Let_Not_Correlated_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2
""");
    }

    public override void Scalar_Function_Let_Not_Parameter_Instance()
    {
        base.Scalar_Function_Let_Not_Parameter_Instance();

        AssertSql(
            """
@__customerId_1='2'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_1
""");
    }

    public override void Scalar_Function_Let_Nested_Instance()
    {
        base.Scalar_Function_Let_Nested_Instance();

        AssertSql(
            """
@__starCount_2='3'
@__customerId_1='1'

SELECT TOP(2) [c].[LastName], [dbo].[StarValue](@__starCount_2, [dbo].[CustomerOrderCount](@__customerId_1)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_1
""");
    }

    public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
    {
        base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance();

        AssertSql(
            """
SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]
""");
    }

    public override void Scalar_Nested_Function_BCL_UDF_Instance()
    {
        base.Scalar_Nested_Function_BCL_UDF_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].[CustomerOrderCount]([c].[Id]))
""");
    }

    public override void Scalar_Nested_Function_UDF_BCL_Instance()
    {
        base.Scalar_Nested_Function_UDF_BCL_Instance();

        AssertSql(
            """
SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].[CustomerOrderCount](ABS([c].[Id]))
""");
    }

    #endregion

    #endregion

    #region Queryable Function Tests

    public override void QF_Stand_Alone()
    {
        base.QF_Stand_Alone();

        AssertSql(
            """
SELECT [g].[AmountSold], [g].[ProductId]
FROM [dbo].[GetTopTwoSellingProducts]() AS [g]
ORDER BY [g].[ProductId]
""");
    }

    public override void QF_Stand_Alone_Parameter()
    {
        base.QF_Stand_Alone_Parameter();

        AssertSql(
            """
@__customerId_1='1'

SELECT [g].[Count], [g].[CustomerId], [g].[Year]
FROM [dbo].[GetCustomerOrderCountByYear](@__customerId_1) AS [g]
ORDER BY [g].[Count] DESC
""");
    }

    public override void QF_CrossApply_Correlated_Select_Anonymous()
    {
        base.QF_CrossApply_Correlated_Select_Anonymous();

        AssertSql(
            """
SELECT [c].[Id], [c].[LastName], [g].[Year], [g].[Count]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [g]
ORDER BY [c].[Id], [g].[Year]
""");
    }

    public override void QF_CrossApply_Correlated_Select_QF_Type()
    {
        base.QF_CrossApply_Correlated_Select_QF_Type();

        AssertSql(
            """
SELECT [g].[Count], [g].[CustomerId], [g].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [g]
ORDER BY [g].[Year]
""");
    }

    public override void QF_Select_Direct_In_Anonymous_distinct()
    {
        base.QF_Select_Direct_In_Anonymous_distinct();

        AssertSql(
            @"");
    }

    public override void QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
    {
        base.QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous();

        AssertSql(
            """
SELECT [c].[Id], [g].[OrderId], [g].[CustomerId], [g].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetOrdersWithMultipleProducts]([dbo].[AddValues]([c].[Id], 1)) AS [g]
WHERE [c].[Id] = 1
ORDER BY [c].[Id]
""");
    }

    public override void QF_Select_Correlated_Subquery_In_Anonymous()
    {
        base.QF_Select_Correlated_Subquery_In_Anonymous();

        AssertSql(
            """
SELECT [c].[Id], [g0].[OrderId], [g0].[CustomerId], [g0].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [g].[OrderId], [g].[CustomerId], [g].[OrderDate]
    FROM [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [g]
    WHERE DATEPART(day, [g].[OrderDate]) = 21
) AS [g0]
ORDER BY [c].[Id]
""");
    }

    public override void QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF()
    {
        base.QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF();

        AssertSql(
            """
SELECT [o].[CustomerId], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT [g].[OrderId]
    FROM [Customers] AS [c]
    CROSS APPLY [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [g]
) AS [s] ON [o].[Id] = [s].[OrderId]
""");
    }

    public override void QF_Correlated_Select_In_Anonymous()
    {
        base.QF_Correlated_Select_In_Anonymous();

        AssertSql(
            """
SELECT [c].[Id], [c].[LastName], [g].[OrderId], [g].[CustomerId], [g].[OrderDate]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [g]
ORDER BY [c].[Id]
""");
    }

    public override void QF_CrossApply_Correlated_Select_Result()
    {
        base.QF_CrossApply_Correlated_Select_Result();

        AssertSql(
            """
SELECT [g].[Count], [g].[CustomerId], [g].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [g]
ORDER BY [g].[Count] DESC, [g].[Year] DESC
""");
    }

    public override void QF_CrossJoin_Not_Correlated()
    {
        base.QF_CrossJoin_Not_Correlated();

        AssertSql(
            """
SELECT [c].[Id], [c].[LastName], [g].[Year], [g].[Count]
FROM [Customers] AS [c]
CROSS JOIN [dbo].[GetCustomerOrderCountByYear](2) AS [g]
WHERE [c].[Id] = 2
ORDER BY [g].[Count]
""");
    }

    public override void QF_CrossJoin_Parameter()
    {
        base.QF_CrossJoin_Parameter();

        AssertSql(
            """
@__custId_1='2'

SELECT [c].[Id], [c].[LastName], [g].[Year], [g].[Count]
FROM [Customers] AS [c]
CROSS JOIN [dbo].[GetCustomerOrderCountByYear](@__custId_1) AS [g]
WHERE [c].[Id] = @__custId_1
ORDER BY [g].[Count]
""");
    }

    public override void QF_Join()
    {
        base.QF_Join();

        AssertSql(
            """
SELECT [p].[Id], [p].[Name], [g].[AmountSold]
FROM [Products] AS [p]
INNER JOIN [dbo].[GetTopTwoSellingProducts]() AS [g] ON [p].[Id] = [g].[ProductId]
ORDER BY [p].[Id]
""");
    }

    public override void QF_LeftJoin_Select_Anonymous()
    {
        base.QF_LeftJoin_Select_Anonymous();

        AssertSql(
            """
SELECT [p].[Id], [p].[Name], [g].[AmountSold]
FROM [Products] AS [p]
LEFT JOIN [dbo].[GetTopTwoSellingProducts]() AS [g] ON [p].[Id] = [g].[ProductId]
ORDER BY [p].[Id] DESC
""");
    }

    public override void QF_LeftJoin_Select_Result()
    {
        base.QF_LeftJoin_Select_Result();

        AssertSql(
            """
SELECT [g].[AmountSold], [g].[ProductId]
FROM [Products] AS [p]
LEFT JOIN [dbo].[GetTopTwoSellingProducts]() AS [g] ON [p].[Id] = [g].[ProductId]
ORDER BY [p].[Id] DESC
""");
    }

    public override void QF_OuterApply_Correlated_Select_QF()
    {
        base.QF_OuterApply_Correlated_Select_QF();

        AssertSql(
            """
SELECT [g].[Count], [g].[CustomerId], [g].[Year]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [g]
ORDER BY [c].[Id], [g].[Year]
""");
    }

    public override void QF_OuterApply_Correlated_Select_Entity()
    {
        base.QF_OuterApply_Correlated_Select_Entity();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [g]
WHERE [g].[Year] = 2000
ORDER BY [c].[Id], [g].[Year]
""");
    }

    public override void QF_OuterApply_Correlated_Select_Anonymous()
    {
        base.QF_OuterApply_Correlated_Select_Anonymous();

        AssertSql(
            """
SELECT [c].[Id], [c].[LastName], [g].[Year], [g].[Count]
FROM [Customers] AS [c]
OUTER APPLY [dbo].[GetCustomerOrderCountByYear]([c].[Id]) AS [g]
ORDER BY [c].[Id], [g].[Year]
""");
    }

    public override void QF_Nested()
    {
        base.QF_Nested();

        AssertSql(
            """
@__custId_1='2'

SELECT [c].[Id], [c].[LastName], [g].[Year], [g].[Count]
FROM [Customers] AS [c]
CROSS JOIN [dbo].[GetCustomerOrderCountByYear]([dbo].[AddValues](1, 1)) AS [g]
WHERE [c].[Id] = @__custId_1
ORDER BY [g].[Year]
""");
    }

    public override void QF_Correlated_Nested_Func_Call()
    {
        base.QF_Correlated_Nested_Func_Call();

        AssertSql(
            """
@__custId_1='2'

SELECT [c].[Id], [g].[Count], [g].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYear]([dbo].[AddValues]([c].[Id], 1)) AS [g]
WHERE [c].[Id] = @__custId_1
""");
    }

    public override void QF_Correlated_Func_Call_With_Navigation()
    {
        base.QF_Correlated_Func_Call_With_Navigation();

        AssertSql(
            """
SELECT [c].[Id], [s].[CustomerName], [s].[OrderId], [s].[Id]
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT [c0].[LastName] AS [CustomerName], [g].[OrderId], [c0].[Id]
    FROM [dbo].[GetOrdersWithMultipleProducts]([c].[Id]) AS [g]
    INNER JOIN [Customers] AS [c0] ON [g].[CustomerId] = [c0].[Id]
) AS [s]
ORDER BY [c].[Id], [s].[OrderId]
""");
    }

    public override void DbSet_mapped_to_function()
    {
        base.DbSet_mapped_to_function();

        AssertSql(
            """
SELECT [g].[AmountSold], [g].[ProductId]
FROM [dbo].[GetTopTwoSellingProducts]() AS [g]
ORDER BY [g].[ProductId]
""");
    }

    public override void TVF_with_navigation_in_projection_groupby_aggregate()
    {
        base.TVF_with_navigation_in_projection_groupby_aggregate();

        AssertSql(
            """
SELECT [c].[LastName], (
    SELECT COALESCE(SUM(CAST(LEN([c1].[FirstName]) AS int)), 0)
    FROM [Orders] AS [o0]
    INNER JOIN [Customers] AS [c0] ON [o0].[CustomerId] = [c0].[Id]
    INNER JOIN [Customers] AS [c1] ON [o0].[CustomerId] = [c1].[Id]
    WHERE NOT EXISTS (
        SELECT 1
        FROM [dbo].[GetTopTwoSellingProducts]() AS [g0]
        WHERE [g0].[ProductId] = 25) AND ([c].[LastName] = [c0].[LastName] OR ([c].[LastName] IS NULL AND [c0].[LastName] IS NULL))) AS [SumOfLengths]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerId] = [c].[Id]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[GetTopTwoSellingProducts]() AS [g]
    WHERE [g].[ProductId] = 25)
GROUP BY [c].[LastName]
""");
    }

    public override void TVF_with_argument_being_a_subquery_with_navigation_in_projection_groupby_aggregate()
    {
        base.TVF_with_argument_being_a_subquery_with_navigation_in_projection_groupby_aggregate();

        AssertSql(
            """
SELECT [c0].[LastName], (
    SELECT COALESCE(SUM(CAST(LEN([c3].[FirstName]) AS int)), 0)
    FROM [Orders] AS [o0]
    INNER JOIN [Customers] AS [c1] ON [o0].[CustomerId] = [c1].[Id]
    INNER JOIN [Customers] AS [c3] ON [o0].[CustomerId] = [c3].[Id]
    WHERE 25 NOT IN (
        SELECT [g0].[CustomerId]
        FROM [dbo].[GetOrdersWithMultipleProducts]((
            SELECT TOP(1) [c2].[Id]
            FROM [Customers] AS [c2]
            ORDER BY [c2].[Id])) AS [g0]
    ) AND ([c0].[LastName] = [c1].[LastName] OR ([c0].[LastName] IS NULL AND [c1].[LastName] IS NULL))) AS [SumOfLengths]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c0] ON [o].[CustomerId] = [c0].[Id]
WHERE 25 NOT IN (
    SELECT [g].[CustomerId]
    FROM [dbo].[GetOrdersWithMultipleProducts]((
        SELECT TOP(1) [c].[Id]
        FROM [Customers] AS [c]
        ORDER BY [c].[Id])) AS [g]
)
GROUP BY [c0].[LastName]
""");
    }

    public override void TVF_backing_entity_type_mapped_to_view()
    {
        base.TVF_backing_entity_type_mapped_to_view();

        AssertSql(
            """
SELECT [c].[Id], [c].[FirstName], [c].[LastName]
FROM [Customers] AS [c]
ORDER BY [c].[FirstName]
""");
    }

    public override void Udf_with_argument_being_comparison_to_null_parameter()
    {
        base.Udf_with_argument_being_comparison_to_null_parameter();

        AssertSql(
            """
SELECT [g].[Count], [g].[CustomerId], [g].[Year]
FROM [Customers] AS [c]
CROSS APPLY [dbo].[GetCustomerOrderCountByYearOnlyFrom2000]([c].[Id], CASE
    WHEN [c].[LastName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) AS [g]
ORDER BY [g].[Year]
""");
    }

    public override void Udf_with_argument_being_comparison_of_nullable_columns()
    {
        base.Udf_with_argument_being_comparison_of_nullable_columns();

        AssertSql(
            """
SELECT [g].[Count], [g].[CustomerId], [g].[Year]
FROM [Addresses] AS [a]
CROSS APPLY [dbo].[GetCustomerOrderCountByYearOnlyFrom2000](1, CASE
    WHEN ([a].[City] = [a].[State] AND [a].[City] IS NOT NULL AND [a].[State] IS NOT NULL) OR ([a].[City] IS NULL AND [a].[State] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) AS [g]
ORDER BY [a].[Id], [g].[Year]
""");
    }

    #endregion

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class SqlServer : UdfFixtureBase
    {
        protected override string StoreName
            => "UDFDbFunctionSqlServerTests";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override async Task SeedAsync(DbContext context)
        {
            await base.SeedAsync(context);

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[CustomerOrderCount] (@customerId int)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select count(id) from orders where customerId = @customerId);
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function[dbo].[StarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('*', @starCount) + @value
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function[dbo].[DollarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('$', @starCount) + @value
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[GetReportingPeriodStartDate] (@period int)
                                                    returns DateTime
                                                    as
                                                    begin
                                                        return '1998-01-01'
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
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

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[IsTopCustomer] (@customerId int)
                                                    returns bit
                                                    as
                                                    begin
                                                        if(@customerId = 1)
                                                            return 1

                                                        return 0
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[IdentityString] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return @s;
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[IdentityStringPropagatesNull] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return @s;
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[IdentityStringNonNullable] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return COALESCE(@s, 'NULL');
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[IdentityStringNonNullableFluent] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return COALESCE(@s, 'NULL');
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].[StringLength] (@s nvarchar(max))
                                                    returns int
                                                    as
                                                    begin
                                                        return LEN(@s);
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
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

            await context.Database.ExecuteSqlRawAsync(
                @"create function [dbo].GetCustomerOrderCountByYearOnlyFrom2000(@customerId int, @onlyFrom2000 bit)
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
                                                        where customerId = 1 AND (@onlyFrom2000 = 0 OR @onlyFrom2000 IS NULL OR (@onlyFrom2000 = 1 AND year(orderDate) = 2000))
                                                        group by customerId, year(orderDate)
                                                        order by year(orderDate)

                                                        return
                                                    end");

            await context.Database.ExecuteSqlRawAsync(
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

            await context.Database.ExecuteSqlRawAsync(
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

            await context.Database.ExecuteSqlRawAsync(
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

            await context.Database.ExecuteSqlRawAsync(
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
