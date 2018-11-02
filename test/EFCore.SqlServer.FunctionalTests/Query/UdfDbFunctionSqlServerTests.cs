// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
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

        [Fact]
        public override void Scalar_Function_Extension_Method_Static()
        {
            base.Scalar_Function_Extension_Method_Static();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE CASE
    WHEN IsDate([c].[FirstName]) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 0");
        }

        [Fact]
        public override void Scalar_Function_With_Translator_Translates_Static()
        {
            base.Scalar_Function_With_Translator_Translates_Static();

            AssertSql(
                @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        [Fact]
        public override void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
        {
            base.Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static();
        }

        [Fact]
        public override void Scalar_Function_Constant_Parameter_Static()
        {
            base.Scalar_Function_Constant_Parameter_Static();

            AssertSql(
                @"@__customerId_0='1'

SELECT [dbo].[CustomerOrderCount](@__customerId_0)
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Parameter_Static();

            AssertSql(
                @"@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        [Fact]
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

        [Fact]
        public override void Scalar_Function_Where_Correlated_Static()
        {
            base.Scalar_Function_Where_Correlated_Static();

            AssertSql(
                @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].[IsTopCustomer]([c].[Id]) = 1");
        }

        [Fact]
        public override void Scalar_Function_Where_Not_Correlated_Static()
        {
            base.Scalar_Function_Where_Not_Correlated_Static();

            AssertSql(
                @"@__startDate_0='2000-04-01T00:00:00' (Nullable = true)

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].[GetCustomerWithMostOrdersAfterDate](@__startDate_0) = [c].[Id]");
        }

        [Fact]
        public override void Scalar_Function_Where_Parameter_Static()
        {
            base.Scalar_Function_Where_Parameter_Static();

            AssertSql(
                @"@__period_0='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](@__period_0))");
        }

        [Fact]
        public override void Scalar_Function_Where_Nested_Static()
        {
            base.Scalar_Function_Where_Nested_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](0))");
        }

        [Fact]
        public override void Scalar_Function_Let_Correlated_Static()
        {
            base.Scalar_Function_Let_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Correlated_Static()
        {
            base.Scalar_Function_Let_Not_Correlated_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Parameter_Static()
        {
            base.Scalar_Function_Let_Not_Parameter_Static();

            AssertSql(
                @"@__customerId_0='2'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        [Fact]
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

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Where_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            base.Scalar_Nested_Function_Client_BCL_UDF_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            base.Scalar_Nested_Function_Client_UDF_BCL_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            base.Scalar_Nested_Function_BCL_Client_UDF_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            base.Scalar_Nested_Function_BCL_UDF_Client_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            base.Scalar_Nested_Function_UDF_BCL_Client_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            base.Scalar_Nested_Function_UDF_Client_BCL_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_Static()
        {
            base.Scalar_Nested_Function_Client_BCL_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_Static()
        {
            base.Scalar_Nested_Function_Client_UDF_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_Client_Static()
        {
            base.Scalar_Nested_Function_BCL_Client_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Static()
        {
            base.Scalar_Nested_Function_BCL_UDF_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].[CustomerOrderCount]([c].[Id]))");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_Client_Static()
        {
            base.Scalar_Nested_Function_UDF_Client_Static();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Static()
        {
            base.Scalar_Nested_Function_UDF_BCL_Static();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].[CustomerOrderCount](ABS([c].[Id]))");
        }

        [Fact]
        public override void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            base.Nullable_navigation_property_access_preserves_schema_for_sql_function();

            AssertSql(
                @"SELECT TOP(1) [dbo].[IdentityString]([o.Customer].[FirstName])
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerId] = [o.Customer].[Id]
ORDER BY [o].[Id]");
        }

        #endregion

        #region Instance

        [Fact]
        public override void Scalar_Function_Non_Static()
        {
            base.Scalar_Function_Non_Static();

            AssertSql(
                @"SELECT TOP(2) [dbo].[StarValue](4, [c].[Id]) AS [Id], [dbo].[DollarValue](2, [c].[LastName]) AS [LastName]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        [Fact]
        public override void Scalar_Function_Extension_Method_Instance()
        {
            base.Scalar_Function_Extension_Method_Instance();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE CASE
    WHEN IsDate([c].[FirstName]) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 0");
        }

        [Fact]
        public override void Scalar_Function_With_Translator_Translates_Instance()
        {
            base.Scalar_Function_With_Translator_Translates_Instance();

            AssertSql(
                @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        [Fact]
        public override void Scalar_Function_Constant_Parameter_Instance()
        {
            base.Scalar_Function_Constant_Parameter_Instance();

            AssertSql(
                @"@__customerId_1='1'

SELECT [dbo].[CustomerOrderCount](@__customerId_1)
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Parameter_Instance();

            AssertSql(
                @"@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
        }

        [Fact]
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

        [Fact]
        public override void Scalar_Function_Where_Correlated_Instance()
        {
            base.Scalar_Function_Where_Correlated_Instance();

            AssertSql(
                @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].[IsTopCustomer]([c].[Id]) = 1");
        }

        [Fact]
        public override void Scalar_Function_Where_Not_Correlated_Instance()
        {
            base.Scalar_Function_Where_Not_Correlated_Instance();

            AssertSql(
                @"@__startDate_1='2000-04-01T00:00:00' (Nullable = true)

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].[GetCustomerWithMostOrdersAfterDate](@__startDate_1) = [c].[Id]");
        }

        [Fact]
        public override void Scalar_Function_Where_Parameter_Instance()
        {
            base.Scalar_Function_Where_Parameter_Instance();

            AssertSql(
                @"@__period_1='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](@__period_1))");
        }

        [Fact]
        public override void Scalar_Function_Where_Nested_Instance()
        {
            base.Scalar_Function_Where_Nested_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].[GetCustomerWithMostOrdersAfterDate]([dbo].[GetReportingPeriodStartDate](0))");
        }

        [Fact]
        public override void Scalar_Function_Let_Correlated_Instance()
        {
            base.Scalar_Function_Let_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount]([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Correlated_Instance()
        {
            base.Scalar_Function_Let_Not_Correlated_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Parameter_Instance()
        {
            base.Scalar_Function_Let_Not_Parameter_Instance();

            AssertSql(
                @"@__8__locals1_customerId_1='2'

SELECT TOP(2) [c].[LastName], [dbo].[CustomerOrderCount](@__8__locals1_customerId_1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__8__locals1_customerId_1");
        }

        [Fact]
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

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            base.Scalar_Nested_Function_Client_BCL_UDF_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            base.Scalar_Nested_Function_Client_UDF_BCL_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            base.Scalar_Nested_Function_BCL_Client_UDF_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            base.Scalar_Nested_Function_BCL_UDF_Client_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            base.Scalar_Nested_Function_UDF_BCL_Client_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            base.Scalar_Nested_Function_UDF_Client_BCL_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_Instance()
        {
            base.Scalar_Nested_Function_Client_BCL_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_Instance()
        {
            base.Scalar_Nested_Function_Client_UDF_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_Client_Instance()
        {
            base.Scalar_Nested_Function_BCL_Client_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            base.Scalar_Nested_Function_BCL_UDF_Instance();

            AssertSql(
                @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].[CustomerOrderCount]([c].[Id]))");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_Client_Instance()
        {
            base.Scalar_Nested_Function_UDF_Client_Instance();

            AssertSql(
                @"SELECT [c].[Id]
FROM [Customers] AS [c]");
        }

        [Fact]
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

        public class SqlServer : UdfFixtureBase
        {
            protected override string StoreName { get; } = "UDFDbFunctionSqlServerTests";
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            protected override void Seed(DbContext context)
            {
                base.Seed(context);

                context.Database.ExecuteSqlCommand(
                    @"create function [dbo].[CustomerOrderCount] (@customerId int)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select count(id) from orders where customerId = @customerId);
                                                    end");

                context.Database.ExecuteSqlCommand(
                    @"create function[dbo].[StarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('*', @starCount) + @value
                                                    end");

                context.Database.ExecuteSqlCommand(
                    @"create function[dbo].[DollarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('$', @starCount) + @value
                                                    end");

                context.Database.ExecuteSqlCommand(
                    @"create function [dbo].[GetReportingPeriodStartDate] (@period int)
                                                    returns DateTime
                                                    as
                                                    begin
                                                        return '1998-01-01'
                                                    end");

                context.Database.ExecuteSqlCommand(
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

                context.Database.ExecuteSqlCommand(
                    @"create function [dbo].[IsTopCustomer] (@customerId int)
                                                    returns bit
                                                    as
                                                    begin
                                                        if(@customerId = 1)
                                                            return 1

                                                        return 0
                                                    end");

                context.Database.ExecuteSqlCommand(
                    @"create function [dbo].[IdentityString] (@customerName nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return @customerName;
                                                    end");

                context.SaveChanges();
            }
        }

        public void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
