// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

public class SimpleQuerySqlServerTest : SimpleQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

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

    public override async Task Null_check_removal_in_ternary_maintain_appropriate_cast(bool async)
    {
        await base.Null_check_removal_in_ternary_maintain_appropriate_cast(async);

        AssertSql(
            @"SELECT CAST([f].[Taste] AS tinyint) AS [Bar]
FROM [Food] AS [f]");
    }

    public override async Task Bool_discriminator_column_works(bool async)
    {
        await base.Bool_discriminator_column_works(async);

        AssertSql(
            @"SELECT [a].[Id], [a].[BlogId], [b].[Id], [b].[IsPhotoBlog], [b].[Title], [b].[NumberOfPhotos]
FROM [Authors] AS [a]
LEFT JOIN [Blog] AS [b] ON [a].[BlogId] = [b].[Id]");
    }

    public override async Task Count_member_over_IReadOnlyCollection_works(bool async)
    {
        await base.Count_member_over_IReadOnlyCollection_works(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Books] AS [b]
    WHERE [a].[AuthorId] = [b].[AuthorId]) AS [BooksCount]
FROM [Authors] AS [a]");
    }

    public override async Task Multiple_different_entity_type_from_different_namespaces(bool async)
    {
        await base.Multiple_different_entity_type_from_different_namespaces(async);

        AssertSql(
            @"SELECT cast(null as int) AS MyValue");
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
            WHERE [m0].[UserId] = @__currentUserId_0 AND [m0].[GroupId] = [m].[GroupId]) AND [u0].[Id] = [u].[Id]) THEN CAST(1 AS bit)
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
            WHERE [m0].[UserId] = @__currentUserId_0 AND [g0].[Id] = [g].[Id]) AND [u0].[Id] = [u].[Id]) THEN CAST(1 AS bit)
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
            WHERE [m0].[UserId] = @__currentUserId_0 AND [m0].[GroupId] = [m].[GroupId]) AND [u0].[Id] = [u].[Id]) THEN CAST(1 AS bit)
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
    SELECT [o0].[OrderId] AS [Key], MAX(CASE
        WHEN [o0].[ShippingDate] IS NULL AND [o0].[CancellationDate] IS NULL THEN [o0].[OrderId]
        ELSE [o0].[OrderId] - 10000000
    END) AS [IsPending]
    FROM [OrderItems] AS [o0]
    WHERE [o0].[OrderId] = @__orderId_0
    GROUP BY [o0].[OrderId]
) AS [t] ON [o].[OrderId] = [t].[Key]
WHERE [o].[OrderId] = @__orderId_0
ORDER BY [o].[OrderId]");
    }

    public override async Task Enum_with_value_converter_matching_take_value(bool async)
    {
        await base.Enum_with_value_converter_matching_take_value(async);

        AssertSql(
            @"@__orderItemType_1='MyType1' (Nullable = false) (Size = 4000)
@__p_0='1'

SELECT [o1].[Id], COALESCE((
    SELECT TOP(1) [o2].[Price]
    FROM [OrderItems] AS [o2]
    WHERE [o1].[Id] = [o2].[Order26472Id] AND [o2].[Type] = @__orderItemType_1), 0.0E0) AS [SpecialSum]
FROM (
    SELECT TOP(@__p_0) [o].[Id]
    FROM [Orders] AS [o]
    WHERE EXISTS (
        SELECT 1
        FROM [OrderItems] AS [o0]
        WHERE [o].[Id] = [o0].[Order26472Id])
    ORDER BY [o].[Id]
) AS [t]
INNER JOIN [Orders] AS [o1] ON [t].[Id] = [o1].[Id]
ORDER BY [t].[Id]");
    }

    public override async Task GroupBy_Aggregate_over_navigations_repeated(bool async)
    {
        await base.GroupBy_Aggregate_over_navigations_repeated(async);

        AssertSql(
            @"SELECT (
    SELECT MIN([o].[HourlyRate])
    FROM [TimeSheets] AS [t0]
    LEFT JOIN [Order] AS [o] ON [t0].[OrderId] = [o].[Id]
    WHERE [t0].[OrderId] IS NOT NULL AND [t].[OrderId] = [t0].[OrderId]) AS [HourlyRate], (
    SELECT MIN([c].[Id])
    FROM [TimeSheets] AS [t1]
    INNER JOIN [Project] AS [p] ON [t1].[ProjectId] = [p].[Id]
    INNER JOIN [Customers] AS [c] ON [p].[CustomerId] = [c].[Id]
    WHERE [t1].[OrderId] IS NOT NULL AND [t].[OrderId] = [t1].[OrderId]) AS [CustomerId], (
    SELECT MIN([c0].[Name])
    FROM [TimeSheets] AS [t2]
    INNER JOIN [Project] AS [p0] ON [t2].[ProjectId] = [p0].[Id]
    INNER JOIN [Customers] AS [c0] ON [p0].[CustomerId] = [c0].[Id]
    WHERE [t2].[OrderId] IS NOT NULL AND [t].[OrderId] = [t2].[OrderId]) AS [CustomerName]
FROM [TimeSheets] AS [t]
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
WHERE [o].[Number] <> N'A1' OR [o].[Number] IS NULL
GROUP BY [o].[CustomerId], [o].[Number]");
    }

    public override async Task Aggregate_over_subquery_in_group_by_projection_2(bool async)
    {
        await base.Aggregate_over_subquery_in_group_by_projection_2(async);

        AssertSql(
            @"SELECT [t].[Value] AS [A], (
    SELECT MAX([t0].[Id])
    FROM [Table] AS [t0]
    WHERE [t0].[Value] = (MAX([t].[Id]) * 6) OR ([t0].[Value] IS NULL AND MAX([t].[Id]) IS NULL)) AS [B]
FROM [Table] AS [t]
GROUP BY [t].[Value]");
    }

    public override async Task Group_by_aggregate_in_subquery_projection_after_group_by(bool async)
    {
        await base.Group_by_aggregate_in_subquery_projection_after_group_by(async);

        AssertSql(
            @"SELECT [t].[Value] AS [A], COALESCE(SUM([t].[Id]), 0) AS [B], COALESCE((
    SELECT TOP(1) COALESCE(SUM([t].[Id]), 0) + COALESCE(SUM([t0].[Id]), 0)
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
            @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [c].[Value1]
        FROM (
            SELECT [p0].[Id], [p0].[Child1Id], [p0].[Child2Id], [p0].[ChildFilter1Id], [p0].[ChildFilter2Id], 1 AS [Key]
            FROM [Parents] AS [p0]
        ) AS [t1]
        LEFT JOIN [Child1] AS [c] ON [t1].[Child1Id] = [c].[Id]
        WHERE [t].[Key] = [t1].[Key]
    ) AS [t0]) AS [Test1], (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [c0].[Value2]
        FROM (
            SELECT [p1].[Id], [p1].[Child1Id], [p1].[Child2Id], [p1].[ChildFilter1Id], [p1].[ChildFilter2Id], 1 AS [Key]
            FROM [Parents] AS [p1]
        ) AS [t3]
        LEFT JOIN [Child2] AS [c0] ON [t3].[Child2Id] = [c0].[Id]
        WHERE [t].[Key] = [t3].[Key]
    ) AS [t2]) AS [Test2]
FROM (
    SELECT 1 AS [Key]
    FROM [Parents] AS [p]
) AS [t]
GROUP BY [t].[Key]");
    }

    public override async Task Group_by_multiple_aggregate_joining_different_tables_with_query_filter(bool async)
    {
        await base.Group_by_multiple_aggregate_joining_different_tables_with_query_filter(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [t2].[Value1]
        FROM (
            SELECT [p0].[Id], [p0].[Child1Id], [p0].[Child2Id], [p0].[ChildFilter1Id], [p0].[ChildFilter2Id], 1 AS [Key]
            FROM [Parents] AS [p0]
        ) AS [t0]
        LEFT JOIN (
            SELECT [c].[Id], [c].[Filter1], [c].[Value1]
            FROM [ChildFilter1] AS [c]
            WHERE [c].[Filter1] = N'Filter1'
        ) AS [t2] ON [t0].[ChildFilter1Id] = [t2].[Id]
        WHERE [t].[Key] = [t0].[Key]
    ) AS [t1]) AS [Test1], (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [t5].[Value2]
        FROM (
            SELECT [p1].[Id], [p1].[Child1Id], [p1].[Child2Id], [p1].[ChildFilter1Id], [p1].[ChildFilter2Id], 1 AS [Key]
            FROM [Parents] AS [p1]
        ) AS [t4]
        LEFT JOIN (
            SELECT [c0].[Id], [c0].[Filter2], [c0].[Value2]
            FROM [ChildFilter2] AS [c0]
            WHERE [c0].[Filter2] = N'Filter2'
        ) AS [t5] ON [t4].[ChildFilter2Id] = [t5].[Id]
        WHERE [t].[Key] = [t4].[Key]
    ) AS [t3]) AS [Test2]
FROM (
    SELECT 1 AS [Key]
    FROM [Parents] AS [p]
) AS [t]
GROUP BY [t].[Key]");
    }

    public override async Task Subquery_first_member_compared_to_null(bool async)
    {
        await base.Subquery_first_member_compared_to_null(async);

        AssertSql(
            @"SELECT (
    SELECT TOP(1) [c1].[SomeOtherNullableDateTime]
    FROM [Child26744] AS [c1]
    WHERE [p].[Id] = [c1].[ParentId] AND [c1].[SomeNullableDateTime] IS NULL
    ORDER BY [c1].[SomeInteger])
FROM [Parents] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM [Child26744] AS [c]
    WHERE [p].[Id] = [c].[ParentId] AND [c].[SomeNullableDateTime] IS NULL) AND (
    SELECT TOP(1) [c0].[SomeOtherNullableDateTime]
    FROM [Child26744] AS [c0]
    WHERE [p].[Id] = [c0].[ParentId] AND [c0].[SomeNullableDateTime] IS NULL
    ORDER BY [c0].[SomeInteger]) IS NOT NULL");
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

    public override async Task StoreType_for_UDF_used(bool async)
    {
        await base.StoreType_for_UDF_used(async);

        AssertSql(
            @"@__date_0='2012-12-12T00:00:00.0000000' (DbType = DateTime)

SELECT [m].[Id], [m].[SomeDate]
FROM [MyEntities] AS [m]
WHERE [m].[SomeDate] = @__date_0",
                //
                @"@__date_0='2012-12-12T00:00:00.0000000' (DbType = DateTime)

SELECT [m].[Id], [m].[SomeDate]
FROM [MyEntities] AS [m]
WHERE [dbo].[ModifyDate]([m].[SomeDate]) = @__date_0");
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

    public override async Task Hierarchy_query_with_abstract_type_sibling(bool async)
    {
        await base.Hierarchy_query_with_abstract_type_sibling(async);

        AssertSql(
            @"SELECT [a].[Id], [a].[Discriminator], [a].[Species], [a].[Name], [a].[EdcuationLevel], [a].[FavoriteToy]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Cat', N'Dog') AND [a].[Species] IS NOT NULL AND ([a].[Species] LIKE N'F%')");
    }

    public override async Task Hierarchy_query_with_abstract_type_sibling_TPT(bool async)
    {
        await base.Hierarchy_query_with_abstract_type_sibling_TPT(async);

        AssertSql(
            @"SELECT [a].[Id], [a].[Species], [p].[Name], [c].[EdcuationLevel], [d].[FavoriteToy], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'Dog'
    WHEN [c].[Id] IS NOT NULL THEN N'Cat'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Pets] AS [p] ON [a].[Id] = [p].[Id]
LEFT JOIN [Cats] AS [c] ON [a].[Id] = [c].[Id]
LEFT JOIN [Dogs] AS [d] ON [a].[Id] = [d].[Id]
WHERE ([d].[Id] IS NOT NULL OR [c].[Id] IS NOT NULL) AND [a].[Species] IS NOT NULL AND ([a].[Species] LIKE N'F%')");
    }

    public override async Task Hierarchy_query_with_abstract_type_sibling_TPC(bool async)
    {
        await base.Hierarchy_query_with_abstract_type_sibling_TPC(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[Species], [t].[Name], [t].[EdcuationLevel], [t].[FavoriteToy], [t].[Discriminator]
FROM (
    SELECT [c].[Id], [c].[Species], [c].[Name], [c].[EdcuationLevel], NULL AS [FavoriteToy], N'Cat' AS [Discriminator]
    FROM [Cats] AS [c]
    UNION ALL
    SELECT [d].[Id], [d].[Species], [d].[Name], NULL AS [EdcuationLevel], [d].[FavoriteToy], N'Dog' AS [Discriminator]
    FROM [Dogs] AS [d]
) AS [t]
WHERE [t].[Species] IS NOT NULL AND ([t].[Species] LIKE N'F%')");
        }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Muliple_occurrences_of_FromSql_in_group_by_aggregate(bool async)
    {
        var contextFactory = await InitializeAsync<Context27427>();
        using var context = contextFactory.CreateContext();
        var query = context.DemoEntities
                 .FromSqlRaw("SELECT * FROM DemoEntities WHERE Id = {0}", new SqlParameter { Value = 1 })
                 .Select(e => e.Id);

        var query2 = context.DemoEntities
                 .Where(e => query.Contains(e.Id))
                 .GroupBy(e => e.Id)
                 .Select(g => new { g.Key, Aggregate = g.Count() });

        if (async)
        {
            await query2.ToListAsync();
        }
        else
        {
            query2.ToList();
        }

        AssertSql(
            @"p0='1'

SELECT [d].[Id] AS [Key], COUNT(*) AS [Aggregate]
FROM [DemoEntities] AS [d]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT * FROM DemoEntities WHERE Id = @p0
    ) AS [m]
    WHERE [m].[Id] = [d].[Id])
GROUP BY [d].[Id]");
    }

    protected class Context27427 : DbContext
    {
        public Context27427(DbContextOptions options)
               : base(options)
        {
        }

        public DbSet<DemoEntity> DemoEntities { get; set; }
    }

    protected class DemoEntity
    {
        public int Id { get; set; }
    }
}
