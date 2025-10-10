// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocQueryFiltersQuerySqlServerTest : AdHocQueryFiltersQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    #region 11803

    [ConditionalFact]
    public virtual async Task Query_filter_with_db_set_should_not_block_other_filters()
    {
        var contextFactory = await InitializeAsync<Context11803>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Factions.ToList();

        Assert.Empty(query);

        AssertSql(
            """
SELECT [f].[Id], [f].[Name]
FROM [Factions] AS [f]
WHERE EXISTS (
    SELECT 1
    FROM [Leaders] AS [l]
    WHERE [l].[Name] LIKE N'Bran%' AND [l].[Name] = N'Crach an Craite')
""");
    }

    [ConditionalFact]
    public virtual async Task Keyless_type_used_inside_defining_query()
    {
        var contextFactory = await InitializeAsync<Context11803>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.LeadersQuery.ToList();

        Assert.Single(query);

        AssertSql(
            """
SELECT [t].[Name]
FROM (
    SELECT [l].[Name]
    FROM [Leaders] AS [l]
    WHERE ([l].[Name] LIKE N'Bran' + N'%' AND (LEFT([l].[Name], LEN(N'Bran')) = N'Bran')) AND (([l].[Name] <> N'Foo') OR [l].[Name] IS NULL)
) AS [t]
WHERE ([t].[Name] <> N'Bar') OR [t].[Name] IS NULL
""");
    }

    protected class Context11803(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Faction11803> Factions { get; set; }
        public DbSet<Leader11803> Leaders { get; set; }
        public DbSet<LeaderQuery11803> LeadersQuery { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Leader11803>().HasQueryFilter(l => l.Name.StartsWith("Bran")); // this one is ignored
            modelBuilder.Entity<Faction11803>().HasQueryFilter(f => Leaders.Any(l => l.Name == "Crach an Craite"));

            modelBuilder
                .Entity<LeaderQuery11803>()
                .HasNoKey()
                .ToSqlQuery(
                    """
SELECT [t].[Name]
FROM (
    SELECT [l].[Name]
    FROM [Leaders] AS [l]
    WHERE ([l].[Name] LIKE N'Bran' + N'%' AND (LEFT([l].[Name], LEN(N'Bran')) = N'Bran')) AND (([l].[Name] <> N'Foo') OR [l].[Name] IS NULL)
) AS [t]
WHERE ([t].[Name] <> N'Bar') OR [t].[Name] IS NULL
""");
        }

        public Task SeedAsync()
        {
            var f1 = new Faction11803 { Name = "Skeliege" };
            var f2 = new Faction11803 { Name = "Monsters" };
            var f3 = new Faction11803 { Name = "Nilfgaard" };
            var f4 = new Faction11803 { Name = "Northern Realms" };
            var f5 = new Faction11803 { Name = "Scioia'tael" };

            var l11 = new Leader11803 { Faction = f1, Name = "Bran Tuirseach" };
            var l12 = new Leader11803 { Faction = f1, Name = "Crach an Craite" };
            var l13 = new Leader11803 { Faction = f1, Name = "Eist Tuirseach" };
            var l14 = new Leader11803 { Faction = f1, Name = "Harald the Cripple" };

            Factions.AddRange(f1, f2, f3, f4, f5);
            Leaders.AddRange(l11, l12, l13, l14);

            return SaveChangesAsync();
        }

        public class Faction11803
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Leader11803> Leaders { get; set; }
        }

        public class Leader11803
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Faction11803 Faction { get; set; }
        }

        public class LeaderQuery11803
        {
            public string Name { get; set; }
        }
    }

    #endregion

    public override async Task Query_filter_with_contains_evaluates_correctly()
    {
        await base.Query_filter_with_contains_evaluates_correctly();

        AssertSql(
            """
@__ef_filter___ids_0='[1,7]' (Size = 4000)

SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
WHERE [e].[Id] NOT IN (
    SELECT [e0].[value]
    FROM OPENJSON(@__ef_filter___ids_0) WITH ([value] int '$') AS [e0]
)
""");
    }

    public override async Task MultiContext_query_filter_test()
    {
        await base.MultiContext_query_filter_test();

        AssertSql(
            """
@__ef_filter__Tenant_0='0'

SELECT [b].[Id], [b].[SomeValue]
FROM [Blogs] AS [b]
WHERE [b].[SomeValue] = @__ef_filter__Tenant_0
""",
            //
            """
@__ef_filter__Tenant_0='1'

SELECT [b].[Id], [b].[SomeValue]
FROM [Blogs] AS [b]
WHERE [b].[SomeValue] = @__ef_filter__Tenant_0
""",
            //
            """
@__ef_filter__Tenant_0='2'

SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[SomeValue] = @__ef_filter__Tenant_0
""");
    }

    public override async Task Weak_entities_with_query_filter_subquery_flattening()
    {
        await base.Weak_entities_with_query_filter_subquery_flattening();

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Definitions] AS [d]
        WHERE [d].[ChangeInfo_RemovedPoint_Timestamp] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Query_filter_with_pk_fk_optimization()
    {
        await base.Query_filter_with_pk_fk_optimization();

        AssertSql(
            """
SELECT TOP(2) [e].[Id], CASE
    WHEN [r0].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [r0].[Id], [r0].[Public], [e].[RefEntityId]
FROM [Entities] AS [e]
LEFT JOIN (
    SELECT [r].[Id], [r].[Public]
    FROM [RefEntities] AS [r]
    WHERE [r].[Public] = CAST(1 AS bit)
) AS [r0] ON [e].[RefEntityId] = [r0].[Id]
WHERE [e].[Id] = 1
""");
    }

    public override async Task Self_reference_in_query_filter_works()
    {
        await base.Self_reference_in_query_filter_works();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [EntitiesWithQueryFilterSelfReference] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntitiesWithQueryFilterSelfReference] AS [e0]) AND ([e].[Name] <> N'Foo' OR [e].[Name] IS NULL)
""",
            //
            """
SELECT [e].[Id], [e].[Name]
FROM [EntitiesReferencingEntityWithQueryFilterSelfReference] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM [EntitiesWithQueryFilterSelfReference] AS [e0]
    WHERE EXISTS (
        SELECT 1
        FROM [EntitiesWithQueryFilterSelfReference] AS [e1])) AND ([e].[Name] <> N'Foo' OR [e].[Name] IS NULL)
""");
    }

    public override async Task Invoke_inside_query_filter_gets_correctly_evaluated_during_translation()
    {
        await base.Invoke_inside_query_filter_gets_correctly_evaluated_during_translation();

        AssertSql(
            """
@__ef_filter__p_0='1'

SELECT [e].[Id], [e].[Name], [e].[TenantId]
FROM [Entities] AS [e]
WHERE ([e].[Name] <> N'Foo' OR [e].[Name] IS NULL) AND [e].[TenantId] = @__ef_filter__p_0
""",
            //
            """
@__ef_filter__p_0='2'

SELECT [e].[Id], [e].[Name], [e].[TenantId]
FROM [Entities] AS [e]
WHERE ([e].[Name] <> N'Foo' OR [e].[Name] IS NULL) AND [e].[TenantId] = @__ef_filter__p_0
""");
    }

    public override async Task Query_filter_with_null_constant()
    {
        await base.Query_filter_with_null_constant();

        AssertSql(
            """
SELECT [p].[Id], [p].[UserDeleteId]
FROM [People] AS [p]
LEFT JOIN [User18759] AS [u] ON [p].[UserDeleteId] = [u].[Id]
WHERE [u].[Id] IS NOT NULL
""");
    }

    public override async Task GroupJoin_SelectMany_gets_flattened()
    {
        await base.GroupJoin_SelectMany_gets_flattened();

        AssertSql(
            """
SELECT [c].[CustomerId], [c].[CustomerMembershipId]
FROM [CustomerFilters] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Customers] AS [c0]
    LEFT JOIN [CustomerMemberships] AS [c1] ON [c0].[Id] = [c1].[CustomerId]
    WHERE [c1].[Id] IS NOT NULL AND [c0].[Id] = [c].[CustomerId]) > 0
""",
            //
            """
SELECT [c].[Id], [c].[Name], [c0].[Id] AS [CustomerMembershipId], CASE
    WHEN [c0].[Id] IS NOT NULL THEN [c0].[Name]
    ELSE N''
END AS [CustomerMembershipName]
FROM [Customers] AS [c]
LEFT JOIN [CustomerMemberships] AS [c0] ON [c].[Id] = [c0].[CustomerId]
""");
    }

    public override async Task Group_by_multiple_aggregate_joining_different_tables(bool async)
    {
        await base.Group_by_multiple_aggregate_joining_different_tables(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [c].[Value1]
        FROM (
            SELECT [p2].[Child1Id], 1 AS [Key]
            FROM [Parents] AS [p2]
        ) AS [p1]
        LEFT JOIN [Child1] AS [c] ON [p1].[Child1Id] = [c].[Id]
        WHERE [p0].[Key] = [p1].[Key]
    ) AS [s]) AS [Test1], (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [c0].[Value2]
        FROM (
            SELECT [p4].[Child2Id], 1 AS [Key]
            FROM [Parents] AS [p4]
        ) AS [p3]
        LEFT JOIN [Child2] AS [c0] ON [p3].[Child2Id] = [c0].[Id]
        WHERE [p0].[Key] = [p3].[Key]
    ) AS [s0]) AS [Test2]
FROM (
    SELECT 1 AS [Key]
    FROM [Parents] AS [p]
) AS [p0]
GROUP BY [p0].[Key]
""");
    }

    public override async Task Group_by_multiple_aggregate_joining_different_tables_with_query_filter(bool async)
    {
        await base.Group_by_multiple_aggregate_joining_different_tables_with_query_filter(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [c0].[Value1]
        FROM (
            SELECT [p2].[ChildFilter1Id], 1 AS [Key]
            FROM [Parents] AS [p2]
        ) AS [p1]
        LEFT JOIN (
            SELECT [c].[Id], [c].[Value1]
            FROM [ChildFilter1] AS [c]
            WHERE [c].[Filter1] = N'Filter1'
        ) AS [c0] ON [p1].[ChildFilter1Id] = [c0].[Id]
        WHERE [p0].[Key] = [p1].[Key]
    ) AS [s]) AS [Test1], (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [c2].[Value2]
        FROM (
            SELECT [p4].[ChildFilter2Id], 1 AS [Key]
            FROM [Parents] AS [p4]
        ) AS [p3]
        LEFT JOIN (
            SELECT [c1].[Id], [c1].[Value2]
            FROM [ChildFilter2] AS [c1]
            WHERE [c1].[Filter2] = N'Filter2'
        ) AS [c2] ON [p3].[ChildFilter2Id] = [c2].[Id]
        WHERE [p0].[Key] = [p3].[Key]
    ) AS [s0]) AS [Test2]
FROM (
    SELECT 1 AS [Key]
    FROM [Parents] AS [p]
) AS [p0]
GROUP BY [p0].[Key]
""");
    }

    public override async Task IsDeleted_query_filter_with_conversion_to_int_works(bool async)
    {
        await base.IsDeleted_query_filter_with_conversion_to_int_works(async);

        AssertSql(
            """
SELECT [s].[SupplierId], [s].[IsDeleted], [s].[LocationId], [s].[Name], [l0].[LocationId], [l0].[Address], [l0].[IsDeleted]
FROM [Suppliers] AS [s]
LEFT JOIN (
    SELECT [l].[LocationId], [l].[Address], [l].[IsDeleted]
    FROM [Locations] AS [l]
    WHERE [l].[IsDeleted] = 0
) AS [l0] ON [s].[LocationId] = [l0].[LocationId]
WHERE [s].[IsDeleted] = 0
ORDER BY [s].[Name]
""");
    }
}
