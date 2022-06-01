﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQuerySqlServerTest : SimpleQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override async Task Multiple_nested_reference_navigations(bool async)
        {
            await base.Multiple_nested_reference_navigations(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(1) [s].[Id], [s].[Email], [s].[Logon], [s].[ManagerId], [s].[Name], [s].[SecondaryManagerId]
FROM [Staff] AS [s]
WHERE [s].[Id] = @__p_0",
                //
                @"@__id_0='1'

SELECT TOP(2) [a].[Id], [a].[Complete], [a].[Deleted], [a].[PeriodEnd], [a].[PeriodStart], [a].[StaffId], [s].[Id], [s].[Email], [s].[Logon], [s].[ManagerId], [s].[Name], [s].[SecondaryManagerId], [s0].[Id], [s0].[Email], [s0].[Logon], [s0].[ManagerId], [s0].[Name], [s0].[SecondaryManagerId], [s1].[Id], [s1].[Email], [s1].[Logon], [s1].[ManagerId], [s1].[Name], [s1].[SecondaryManagerId]
FROM [Appraisals] AS [a]
INNER JOIN [Staff] AS [s] ON [a].[StaffId] = [s].[Id]
LEFT JOIN [Staff] AS [s0] ON [s].[ManagerId] = [s0].[Id]
LEFT JOIN [Staff] AS [s1] ON [s].[SecondaryManagerId] = [s1].[Id]
WHERE [a].[Id] = @__id_0");
        }

        public override async Task Comparing_enum_casted_to_byte_with_int_parameter(bool async)
        {
            await base.Comparing_enum_casted_to_byte_with_int_parameter(async);

            AssertSql(
                @"@__bitterTaste_0='1'

SELECT [i].[IceCreamId], [i].[Name], [i].[Taste]
FROM [IceCreams] AS [i]
WHERE [i].[Taste] = @__bitterTaste_0");
        }

        public override async Task Comparing_enum_casted_to_byte_with_int_constant(bool async)
        {
            await base.Comparing_enum_casted_to_byte_with_int_constant(async);

            AssertSql(
                @"SELECT [i].[IceCreamId], [i].[Name], [i].[Taste]
FROM [IceCreams] AS [i]
WHERE [i].[Taste] = 1");
        }

        public override async Task Comparing_byte_column_to_enum_in_vb_creating_double_cast(bool async)
        {
            await base.Comparing_byte_column_to_enum_in_vb_creating_double_cast(async);

            AssertSql(
                @"SELECT [f].[Id], [f].[Taste]
FROM [Food] AS [f]
WHERE [f].[Taste] = CAST(1 AS tinyint)");
        }

        public override async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery(bool async)
        {
            await base.Unwrap_convert_node_over_projection_when_translating_contains_over_subquery(async);

            AssertSql(
                @"@__currentUserId_0='1'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Memberships] AS [m]
        INNER JOIN [Users] AS [u0] ON [m].[UserId] = [u0].[Id]
        WHERE EXISTS (
            SELECT 1
            FROM [Memberships] AS [m0]
            WHERE ([m0].[UserId] = @__currentUserId_0) AND ([m0].[GroupId] = [m].[GroupId])) AND ([u0].[Id] = [u].[Id])) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [HasAccess]
FROM [Users] AS [u]");
        }

        public override async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_2(bool async)
        {
            await base.Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_2(async);

            AssertSql(
                @"@__currentUserId_0='1'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Memberships] AS [m]
        INNER JOIN [Groups] AS [g] ON [m].[GroupId] = [g].[Id]
        INNER JOIN [Users] AS [u0] ON [m].[UserId] = [u0].[Id]
        WHERE EXISTS (
            SELECT 1
            FROM [Memberships] AS [m0]
            INNER JOIN [Groups] AS [g0] ON [m0].[GroupId] = [g0].[Id]
            WHERE ([m0].[UserId] = @__currentUserId_0) AND ([g0].[Id] = [g].[Id])) AND ([u0].[Id] = [u].[Id])) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [HasAccess]
FROM [Users] AS [u]");
        }

        public override async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_3(bool async)
        {
            await base.Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_3(async);

            AssertSql(
                @"@__currentUserId_0='1'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Memberships] AS [m]
        INNER JOIN [Users] AS [u0] ON [m].[UserId] = [u0].[Id]
        WHERE EXISTS (
            SELECT 1
            FROM [Memberships] AS [m0]
            WHERE ([m0].[UserId] = @__currentUserId_0) AND ([m0].[GroupId] = [m].[GroupId])) AND ([u0].[Id] = [u].[Id])) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [HasAccess]
FROM [Users] AS [u]");
        }

        public override async Task GroupBy_aggregate_on_right_side_of_join(bool async)
        {
            await base.GroupBy_aggregate_on_right_side_of_join(async);

            AssertSql(
                @"@__orderId_0='123456'

SELECT [o].[Id], [o].[CancellationDate], [o].[OrderId], [o].[ShippingDate]
FROM [OrderItems] AS [o]
INNER JOIN (
    SELECT [o0].[OrderId] AS [Key]
    FROM [OrderItems] AS [o0]
    WHERE [o0].[OrderId] = @__orderId_0
    GROUP BY [o0].[OrderId]
) AS [t] ON [o].[OrderId] = [t].[Key]
WHERE [o].[OrderId] = @__orderId_0
ORDER BY [o].[OrderId]");
        }

        public override async Task GroupBy_Aggregate_over_navigations_repeated(bool async)
        {
            await base.GroupBy_Aggregate_over_navigations_repeated(async);

            AssertSql(
                @"SELECT MIN([o].[HourlyRate]) AS [HourlyRate], MIN([c].[Id]) AS [CustomerId], MIN([c0].[Name]) AS [CustomerName]
FROM [TimeSheets] AS [t]
LEFT JOIN [Order] AS [o] ON [t].[OrderId] = [o].[Id]
INNER JOIN [Project] AS [p] ON [t].[ProjectId] = [p].[Id]
INNER JOIN [Customers] AS [c] ON [p].[CustomerId] = [c].[Id]
INNER JOIN [Project] AS [p0] ON [t].[ProjectId] = [p0].[Id]
INNER JOIN [Customers] AS [c0] ON [p0].[CustomerId] = [c0].[Id]
WHERE [t].[OrderId] IS NOT NULL
GROUP BY [t].[OrderId]");
        }

        public override async Task Aggregate_over_subquery_in_group_by_projection(bool async)
        {
            await base.Aggregate_over_subquery_in_group_by_projection(async);

            AssertSql(
                @"SELECT [o].[CustomerId], (
    SELECT MIN([o0].[HourlyRate])
    FROM [Order] AS [o0]
    WHERE [o0].[CustomerId] = [o].[CustomerId]) AS [CustomerMinHourlyRate], MIN([o].[HourlyRate]) AS [HourlyRate], COUNT(*) AS [Count]
FROM [Order] AS [o]
WHERE ([o].[Number] <> N'A1') OR ([o].[Number] IS NULL)
GROUP BY [o].[CustomerId], [o].[Number]");
        }

        public override async Task Aggregate_over_subquery_in_group_by_projection_2(bool async)
        {
            await base.Aggregate_over_subquery_in_group_by_projection_2(async);

            AssertSql(
                @"SELECT [t].[Value] AS [A], (
    SELECT MAX([t0].[Id])
    FROM [Table] AS [t0]
    WHERE ([t0].[Value] = ((
        SELECT MAX([t1].[Id])
        FROM [Table] AS [t1]
        WHERE ([t].[Value] = [t1].[Value]) OR (([t].[Value] IS NULL) AND ([t1].[Value] IS NULL))) * 6)) OR (([t0].[Value] IS NULL) AND ((
        SELECT MAX([t1].[Id])
        FROM [Table] AS [t1]
        WHERE ([t].[Value] = [t1].[Value]) OR (([t].[Value] IS NULL) AND ([t1].[Value] IS NULL))) IS NULL))) AS [B]
FROM [Table] AS [t]
GROUP BY [t].[Value]");
        }

        public override async Task Group_by_aggregate_in_subquery_projection_after_group_by(bool async)
        {
            await base.Group_by_aggregate_in_subquery_projection_after_group_by(async);

            AssertSql(
                @"SELECT [t].[Value] AS [A], COALESCE(SUM([t].[Id]), 0) AS [B], COALESCE((
    SELECT TOP(1) (
        SELECT COALESCE(SUM([t1].[Id]), 0)
        FROM [Table] AS [t1]
        WHERE ([t].[Value] = [t1].[Value]) OR (([t].[Value] IS NULL) AND ([t1].[Value] IS NULL))) + COALESCE(SUM([t0].[Id]), 0)
    FROM [Table] AS [t0]
    GROUP BY [t0].[Value]
    ORDER BY (SELECT 1)), 0) AS [C]
FROM [Table] AS [t]
GROUP BY [t].[Value]");
        }

        public override async Task Group_by_multiple_aggregate_joining_different_tables(bool async)
        {
            await base.Group_by_multiple_aggregate_joining_different_tables(async);

            AssertSql(
                @"SELECT COUNT(DISTINCT ([c].[Value1])) AS [Test1], COUNT(DISTINCT ([c0].[Value2])) AS [Test2]
FROM (
    SELECT [p].[Child1Id], [p].[Child2Id], 1 AS [Key]
    FROM [Parents] AS [p]
) AS [t]
LEFT JOIN [Child1] AS [c] ON [t].[Child1Id] = [c].[Id]
LEFT JOIN [Child2] AS [c0] ON [t].[Child2Id] = [c0].[Id]
GROUP BY [t].[Key]");
        }

        public override async Task Group_by_multiple_aggregate_joining_different_tables_with_query_filter(bool async)
        {
            await base.Group_by_multiple_aggregate_joining_different_tables_with_query_filter(async);

            AssertSql(
                @"SELECT COUNT(DISTINCT ([t0].[Value1])) AS [Test1], (
    SELECT DISTINCT COUNT(DISTINCT ([t2].[Value2]))
    FROM (
        SELECT [p0].[Id], [p0].[Child1Id], [p0].[Child2Id], [p0].[ChildFilter1Id], [p0].[ChildFilter2Id], 1 AS [Key]
        FROM [Parents] AS [p0]
    ) AS [t1]
    LEFT JOIN (
        SELECT [c0].[Id], [c0].[Filter2], [c0].[Value2]
        FROM [ChildFilter2] AS [c0]
        WHERE [c0].[Filter2] = N'Filter2'
    ) AS [t2] ON [t1].[ChildFilter2Id] = [t2].[Id]
    WHERE [t].[Key] = [t1].[Key]) AS [Test2]
FROM (
    SELECT [p].[ChildFilter1Id], 1 AS [Key]
    FROM [Parents] AS [p]
) AS [t]
LEFT JOIN (
    SELECT [c].[Id], [c].[Value1]
    FROM [ChildFilter1] AS [c]
    WHERE [c].[Filter1] = N'Filter1'
) AS [t0] ON [t].[ChildFilter1Id] = [t0].[Id]
GROUP BY [t].[Key]");
        }

        public override async Task Subquery_first_member_compared_to_null(bool async)
        {
            await base.Subquery_first_member_compared_to_null(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [c1].[SomeOtherNullableDateTime]
    FROM [Child26744] AS [c1]
    WHERE ([p].[Id] = [c1].[ParentId]) AND ([c1].[SomeNullableDateTime] IS NULL)
    ORDER BY [c1].[SomeInteger])
FROM [Parents] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM [Child26744] AS [c]
    WHERE ([p].[Id] = [c].[ParentId]) AND ([c].[SomeNullableDateTime] IS NULL)) AND ((
    SELECT TOP(1) [c0].[SomeOtherNullableDateTime]
    FROM [Child26744] AS [c0]
    WHERE ([p].[Id] = [c0].[ParentId]) AND ([c0].[SomeNullableDateTime] IS NULL)
    ORDER BY [c0].[SomeInteger]) IS NOT NULL)");
        }

        public override async Task SelectMany_where_Select(bool async)
        {
            await base.SelectMany_where_Select(async);

            AssertSql(
                @"SELECT [t0].[SomeNullableDateTime]
FROM [Parents] AS [p]
INNER JOIN (
    SELECT [t].[ParentId], [t].[SomeNullableDateTime], [t].[SomeOtherNullableDateTime]
    FROM (
        SELECT [c].[ParentId], [c].[SomeNullableDateTime], [c].[SomeOtherNullableDateTime], ROW_NUMBER() OVER(PARTITION BY [c].[ParentId] ORDER BY [c].[SomeInteger]) AS [row]
        FROM [Child26744] AS [c]
        WHERE [c].[SomeNullableDateTime] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [p].[Id] = [t0].[ParentId]
WHERE [t0].[SomeOtherNullableDateTime] IS NOT NULL");
        }

        public override async Task Pushdown_does_not_add_grouping_key_to_projection_when_distinct_is_applied(bool async)
        {
            await base.Pushdown_does_not_add_grouping_key_to_projection_when_distinct_is_applied(async);

            AssertSql(
                @"@__p_0='123456'

SELECT TOP(@__p_0) [t].[JSON]
FROM [TableData] AS [t]
INNER JOIN (
    SELECT DISTINCT [i].[Parcel]
    FROM [IndexData] AS [i]
    WHERE [i].[Parcel] = N'some condition'
    GROUP BY [i].[Parcel], [i].[RowId]
    HAVING COUNT(*) = 1
) AS [t0] ON [t].[ParcelNumber] = [t0].[Parcel]
WHERE [t].[TableId] = 123
ORDER BY [t].[ParcelNumber]");
        }
    }
}
