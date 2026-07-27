// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocMiscellaneousQuerySqliteTest(NonSharedFixture fixture) : AdHocMiscellaneousQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder SetParameterizedCollectionMode(
        DbContextOptionsBuilder optionsBuilder,
        ParameterTranslationMode parameterizedCollectionMode)
    {
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);

        return optionsBuilder;
    }

    protected override Task Seed2951(Context2951 context)
        => context.Database.ExecuteSqlRawAsync(
            """
CREATE TABLE ZeroKey (Id int);
INSERT INTO ZeroKey VALUES (NULL)
""");

    protected override async Task Seed30915(Context30915 context)
    {
        context.Statuses.AddRange(
            new Context30915.PickupStatus30915 { PickupStatusId = 1, Name = "Active" },
            new Context30915.PickupStatus30915 { PickupStatusId = 2, Name = "NoRequests" },
            new Context30915.PickupStatus30915 { PickupStatusId = 3, Name = "Busy" });

        context.Requests.AddRange(
            new Context30915.PickupRequest30915 { PickupStatusId = 1, Priority = 5 },
            new Context30915.PickupRequest30915 { PickupStatusId = 1, Priority = null },
            new Context30915.PickupRequest30915 { PickupStatusId = 3, Priority = 7 });

        await context.SaveChangesAsync();
    }

    public override async Task Average_with_cast()
    {
        await base.Average_with_cast();

        AssertSql(
            """
SELECT "p"."Id", "p"."DecimalColumn", "p"."DoubleColumn", "p"."FloatColumn", "p"."IntColumn", "p"."LongColumn", "p"."NullableDecimalColumn", "p"."NullableDoubleColumn", "p"."NullableFloatColumn", "p"."NullableIntColumn", "p"."NullableLongColumn", "p"."Price"
FROM "Prices" AS "p"
""",
            //
            """
SELECT ef_avg("p"."Price")
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."IntColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."NullableIntColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."LongColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."NullableLongColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG("p"."FloatColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG("p"."NullableFloatColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG("p"."DoubleColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG("p"."NullableDoubleColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT ef_avg("p"."DecimalColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT ef_avg("p"."NullableDecimalColumn")
FROM "Prices" AS "p"
""");
    }

    public override async Task Check_inlined_constants_redacting(bool async, bool enableSensitiveDataLogging)
    {
        await base.Check_inlined_constants_redacting(async, enableSensitiveDataLogging);

        if (!enableSensitiveDataLogging)
        {
            AssertSql(
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE "t"."Id" IN (?, ?, ?)
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE EXISTS (
    SELECT 1
    FROM (SELECT CAST(? AS INTEGER) AS "Value" UNION ALL VALUES (?), (?)) AS "i"
    WHERE "i"."Value" = "t"."Id")
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE ? = "t"."Id"
""");
        }
        else
        {
            AssertSql(
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE "t"."Id" IN (1, 2, 3)
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE EXISTS (
    SELECT 1
    FROM (SELECT CAST(1 AS INTEGER) AS "Value" UNION ALL VALUES (2), (3)) AS "i"
    WHERE "i"."Value" = "t"."Id")
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE 1 = "t"."Id"
""");
        }
    }

    public override async Task Coalesce_in_conditional_with_value_conversion(bool async)
    {
        await base.Coalesce_in_conditional_with_value_conversion(async);

        AssertSql(
            """
SELECT "d"."Id", CASE
    WHEN COALESCE("d"."Foo", 99) = 10 THEN 'A'
    ELSE 'B'
END AS "Foo"
FROM "Data" AS "d"
ORDER BY "d"."Id"
""");
    }

    public override async Task Like_on_value_converted_string_column_does_not_produce_cast(bool async)
    {
        await base.Like_on_value_converted_string_column_does_not_produce_cast(async);

        AssertSql(
            """
SELECT "u"."Id", "u"."Name"
FROM "Users" AS "u"
WHERE "u"."Name" LIKE 'Name%'
""");
    }

    #region 13146

    [Fact]
    public async Task Unconstrained_required_reference_uses_left_join()
    {
        var contextFactory = await InitializeNonSharedTest<Context13146>();
        using var context = contextFactory.CreateDbContext();

        _ = await context.Set<Dependent13146>().Include(e => e.Principal).ToListAsync();

        AssertSql(
            """
SELECT "d"."Id", "d"."PrincipalId", "p"."Id"
FROM "Dependent13146" AS "d"
LEFT JOIN "Principal13146" AS "p" ON "d"."PrincipalId" = "p"."Id"
""");
    }

    protected class Context13146(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Dependent13146>()
                .HasOne(e => e.Principal).WithMany()
                .HasForeignKey(e => e.PrincipalId)
                .IsRequired()
                .IsConstrained(false);
    }

    protected class Principal13146
    {
        public int Id { get; set; }
    }

    protected class Dependent13146
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
        public Principal13146 Principal { get; set; }
    }

    #endregion

    #region 13146-prune

    [Fact]
    public async Task Unconstrained_join_is_not_pruned()
    {
        var contextFactory = await InitializeNonSharedTest<Context13146Prune>();
        using var context = contextFactory.CreateDbContext();

        var query = from d in context.Set<Dependent13146Prune>()
                    join p in context.Set<Principal13146Prune>() on d.PrincipalId equals p.Id
                    select d.Id;
        _ = await query.ToListAsync();

        AssertSql(
            """
SELECT "d"."Id"
FROM "Dependent13146Prune" AS "d"
INNER JOIN "Principal13146Prune" AS "p" ON "d"."PrincipalId" = "p"."Id"
""");
    }

    protected class Context13146Prune(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Dependent13146Prune>()
                .HasOne<Principal13146Prune>().WithMany()
                .HasForeignKey(e => e.PrincipalId)
                .IsRequired()
                .IsConstrained(false);
    }

    protected class Principal13146Prune
    {
        public int Id { get; set; }
    }

    protected class Dependent13146Prune
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
    }

    #endregion

    #region 13146-save

    [Fact]
    public async Task Dangling_required_unconstrained_FK_saves_without_error()
    {
        var contextFactory = await InitializeNonSharedTest<Context13146Save>();
        using var context = contextFactory.CreateDbContext();

        // Add a dependent whose FK references a non-existent principal.
        // Because the FK is unconstrained, no FK constraint exists in the schema,
        // so the INSERT must succeed even though the referenced row is absent.
        context.Add(new Dependent13146Save { Id = 1, PrincipalId = 12345 });

        var saved = await context.SaveChangesAsync();

        Assert.Equal(1, saved);
    }

    protected class Context13146Save(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Dependent13146Save>()
                .HasOne<Principal13146Save>().WithMany()
                .HasForeignKey(e => e.PrincipalId)
                .IsRequired()
                .IsConstrained(false);
    }

    protected class Principal13146Save
    {
        public int Id { get; set; }
    }

    protected class Dependent13146Save
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
    }

    #endregion

    #region 30915

    public override async Task Anon_whole_object_GroupJoin_DefaultIfEmpty()
    {
        await base.Anon_whole_object_GroupJoin_DefaultIfEmpty();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Anon_whole_object_LeftJoin_operator()
    {
        await base.Anon_whole_object_LeftJoin_operator();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Anon_client_null_check_GroupJoin()
    {
        await base.Anon_client_null_check_GroupJoin();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Anon_client_null_check_LeftJoin_operator()
    {
        await base.Anon_client_null_check_LeftJoin_operator();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Anon_member_only_nullable_cast()
    {
        await base.Anon_member_only_nullable_cast();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."Count"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Dto_memberinit_whole_object_LeftJoin()
    {
        await base.Dto_memberinit_whole_object_LeftJoin();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."PickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."PickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Nested_anon_whole_object()
    {
        await base.Nested_anon_whole_object();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Distinct_after_join_member()
    {
        await base.Distinct_after_join_member();

        AssertSql(
            """
SELECT DISTINCT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
""");
    }

    public override async Task Take_after_join_whole_object()
    {
        await base.Take_after_join_whole_object();

        AssertSql(
            """
@p='10'

SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
LIMIT @p
""");
    }

    public override async Task Projected_object_with_nullable_member()
    {
        await base.Projected_object_with_nullable_member();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."MaxPriority", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", MAX("r"."Priority") AS "MaxPriority", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Projected_object_with_string_member()
    {
        await base.Projected_object_with_string_member();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."Name", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 'cat' AS "Name", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Projected_object_all_nullable_members()
    {
        await base.Projected_object_all_nullable_members();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."MaxPriority", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", MAX("r"."Priority") AS "MaxPriority", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Matched_row_with_null_aggregate_keeps_object_non_null()
    {
        await base.Matched_row_with_null_aggregate_keeps_object_non_null();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."MaxPriority", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", MAX("r"."Priority") AS "MaxPriority", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Bare_whole_object_projection_is_null_on_no_match()
    {
        await base.Bare_whole_object_projection_is_null_on_no_match();

        AssertSql(
            """
SELECT "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task User_member_named_marker_does_not_collide_with_synthetic_marker()
    {
        await base.User_member_named_marker_does_not_collide_with_synthetic_marker();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."marker", "r0"."marker0" AS "marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "marker", 1 AS "marker0"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Anon_whole_object_GroupJoin_DefaultIfEmpty_sync()
    {
        await base.Anon_whole_object_GroupJoin_DefaultIfEmpty_sync();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Projected_object_with_decimal_member()
    {
        await base.Projected_object_with_decimal_member();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Total", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COALESCE(ef_sum(CAST("r"."PickupStatusId" AS TEXT)), '0.0') AS "Total", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Composed_user_marker_projection_into_subquery_self_heals()
    {
        await base.Composed_user_marker_projection_into_subquery_self_heals();

        AssertSql(
            """
SELECT "s0"."PickupStatusId", "s0"."pickupStatusId0" AS "pickupStatusId", "s0"."marker", "s0"."marker0" AS "marker"
FROM (
    SELECT DISTINCT "s"."PickupStatusId", "r0"."pickupStatusId" AS "pickupStatusId0", "r0"."marker", "r0"."marker0"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "marker", 1 AS "marker0"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
) AS "s0"
ORDER BY "s0"."PickupStatusId"
""");
    }

    public override async Task Nested_transparent_identifier_of_entities_as_leftjoin_inner()
    {
        await base.Nested_transparent_identifier_of_entities_as_leftjoin_inner();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "s1"."Id", "s1"."PickupStatusId", "s1"."Priority", "s1"."PickupStatusId0", "s1"."Name"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."Id", "r"."PickupStatusId", "r"."Priority", "s0"."PickupStatusId" AS "PickupStatusId0", "s0"."Name"
    FROM "Requests" AS "r"
    INNER JOIN "Statuses" AS "s0" ON "r"."PickupStatusId" = "s0"."PickupStatusId"
) AS "s1" ON "s"."PickupStatusId" = "s1"."PickupStatusId0"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Distinct_with_unconsumed_marker_is_benign()
    {
        await base.Distinct_with_unconsumed_marker_is_benign();

        AssertSql(
            """
SELECT "s0"."PickupStatusId", "s0"."pickupStatusId0", "s0"."Count", "s0"."marker"
FROM (
    SELECT DISTINCT "s"."PickupStatusId", "r0"."pickupStatusId" AS "pickupStatusId0", "r0"."Count", "r0"."marker"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
) AS "s0"
ORDER BY "s0"."PickupStatusId"
""");
    }

    public override async Task Member_only_access_nested_two_joins_deep()
    {
        await base.Member_only_access_nested_two_joins_deep();

        AssertSql(
            """
SELECT "s0"."PickupStatusId", "s0"."Name", "s1"."marker" IS NULL, "s1"."pickupStatusId0", "s1"."Count"
FROM (
    SELECT DISTINCT "s"."PickupStatusId", "r0"."pickupStatusId" AS "pickupStatusId0", "r0"."Count", "r0"."marker"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
) AS "s1"
INNER JOIN "Statuses" AS "s0" ON "s1"."PickupStatusId" = "s0"."PickupStatusId"
ORDER BY "s0"."PickupStatusId"
""");
    }

    public override async Task Second_join_after_then_whole_object()
    {
        await base.Second_join_after_then_whole_object();

        AssertSql(
            """
SELECT "s0"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
INNER JOIN "Statuses" AS "s0" ON "s"."PickupStatusId" = "s0"."PickupStatusId"
ORDER BY "s0"."PickupStatusId"
""");
    }

    public override async Task GroupBy_after_join_then_whole_object()
    {
        await base.GroupBy_after_join_then_whole_object();

        AssertSql(
            """
SELECT "s1"."PickupStatusId", "s3"."pickupStatusId", "s3"."Count", "s3"."marker", "s3"."c"
FROM (
    SELECT "s"."PickupStatusId"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId" AS "pickupStatusId"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
    GROUP BY "s"."PickupStatusId"
) AS "s1"
LEFT JOIN (
    SELECT "s2"."pickupStatusId", "s2"."Count", "s2"."marker", "s2"."c", "s2"."PickupStatusId0"
    FROM (
        SELECT "r1"."pickupStatusId", "r1"."Count", "r1"."marker", 1 AS "c", "s0"."PickupStatusId" AS "PickupStatusId0", ROW_NUMBER() OVER(PARTITION BY "s0"."PickupStatusId" ORDER BY "s0"."PickupStatusId", "r1"."pickupStatusId") AS "row"
        FROM "Statuses" AS "s0"
        LEFT JOIN (
            SELECT "r2"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
            FROM "Requests" AS "r2"
            GROUP BY "r2"."PickupStatusId"
        ) AS "r1" ON "s0"."PickupStatusId" = "r1"."pickupStatusId"
    ) AS "s2"
    WHERE "s2"."row" <= 1
) AS "s3" ON "s1"."PickupStatusId" = "s3"."PickupStatusId0"
ORDER BY "s1"."PickupStatusId"
""");
    }

    public override async Task GroupBy_after_join_then_whole_object_nested_in_wrapper()
    {
        await base.GroupBy_after_join_then_whole_object_nested_in_wrapper();

        // SQL is intentionally identical to the flat GroupBy_after_join_then_whole_object variant -- the wrapper is
        // client-side-only nesting, so it changes no SQL. This test exists to exercise the nested-node rekey path.
        AssertSql(
            """
SELECT "s1"."PickupStatusId", "s3"."pickupStatusId", "s3"."Count", "s3"."marker", "s3"."c"
FROM (
    SELECT "s"."PickupStatusId"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId" AS "pickupStatusId"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
    GROUP BY "s"."PickupStatusId"
) AS "s1"
LEFT JOIN (
    SELECT "s2"."pickupStatusId", "s2"."Count", "s2"."marker", "s2"."c", "s2"."PickupStatusId0"
    FROM (
        SELECT "r1"."pickupStatusId", "r1"."Count", "r1"."marker", 1 AS "c", "s0"."PickupStatusId" AS "PickupStatusId0", ROW_NUMBER() OVER(PARTITION BY "s0"."PickupStatusId" ORDER BY "s0"."PickupStatusId", "r1"."pickupStatusId") AS "row"
        FROM "Statuses" AS "s0"
        LEFT JOIN (
            SELECT "r2"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
            FROM "Requests" AS "r2"
            GROUP BY "r2"."PickupStatusId"
        ) AS "r1" ON "s0"."PickupStatusId" = "r1"."pickupStatusId"
    ) AS "s2"
    WHERE "s2"."row" <= 1
) AS "s3" ON "s1"."PickupStatusId" = "s3"."PickupStatusId0"
ORDER BY "s1"."PickupStatusId"
""");
    }

    public override async Task GroupBy_after_join_then_whole_object_dto_memberinit()
    {
        await base.GroupBy_after_join_then_whole_object_dto_memberinit();

        AssertSql(
            """
SELECT "s1"."PickupStatusId", "s3"."PickupStatusId", "s3"."Count", "s3"."marker", "s3"."c"
FROM (
    SELECT "s"."PickupStatusId"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."PickupStatusId"
    GROUP BY "s"."PickupStatusId"
) AS "s1"
LEFT JOIN (
    SELECT "s2"."PickupStatusId", "s2"."Count", "s2"."marker", "s2"."c", "s2"."PickupStatusId0"
    FROM (
        SELECT "r1"."PickupStatusId", "r1"."Count", "r1"."marker", 1 AS "c", "s0"."PickupStatusId" AS "PickupStatusId0", ROW_NUMBER() OVER(PARTITION BY "s0"."PickupStatusId" ORDER BY "s0"."PickupStatusId", "r1"."PickupStatusId") AS "row"
        FROM "Statuses" AS "s0"
        LEFT JOIN (
            SELECT "r2"."PickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
            FROM "Requests" AS "r2"
            GROUP BY "r2"."PickupStatusId"
        ) AS "r1" ON "s0"."PickupStatusId" = "r1"."PickupStatusId"
    ) AS "s2"
    WHERE "s2"."row" <= 1
) AS "s3" ON "s1"."PickupStatusId" = "s3"."PickupStatusId0"
ORDER BY "s1"."PickupStatusId"
""");
    }

    public override async Task GroupBy_after_join_then_whole_object_struct()
    {
        await base.GroupBy_after_join_then_whole_object_struct();

        AssertSql(
            """
SELECT "s1"."PickupStatusId", "s3"."PickupStatusId", "s3"."Count", "s3"."marker", "s3"."c"
FROM (
    SELECT "s"."PickupStatusId"
    FROM "Statuses" AS "s"
    LEFT JOIN (
        SELECT "r"."PickupStatusId"
        FROM "Requests" AS "r"
        GROUP BY "r"."PickupStatusId"
    ) AS "r0" ON "s"."PickupStatusId" = "r0"."PickupStatusId"
    GROUP BY "s"."PickupStatusId"
) AS "s1"
LEFT JOIN (
    SELECT "s2"."PickupStatusId", "s2"."Count", "s2"."marker", "s2"."c", "s2"."PickupStatusId0"
    FROM (
        SELECT "r1"."PickupStatusId", "r1"."Count", "r1"."marker", 1 AS "c", "s0"."PickupStatusId" AS "PickupStatusId0", ROW_NUMBER() OVER(PARTITION BY "s0"."PickupStatusId" ORDER BY "s0"."PickupStatusId", "r1"."PickupStatusId") AS "row"
        FROM "Statuses" AS "s0"
        LEFT JOIN (
            SELECT "r2"."PickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
            FROM "Requests" AS "r2"
            GROUP BY "r2"."PickupStatusId"
        ) AS "r1" ON "s0"."PickupStatusId" = "r1"."PickupStatusId"
    ) AS "s2"
    WHERE "s2"."row" <= 1
) AS "s3" ON "s1"."PickupStatusId" = "s3"."PickupStatusId0"
ORDER BY "s1"."PickupStatusId"
""");
    }

    public override async Task Two_left_joined_nonentity_objects_second_marker_orphaned()
    {
        await base.Two_left_joined_nonentity_objects_second_marker_orphaned();

        AssertSql(
            """
SELECT "s"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker", "r2"."pickupStatusId", "r2"."Count", "r2"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
LEFT JOIN (
    SELECT "r1"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r1"
    GROUP BY "r1"."PickupStatusId"
) AS "r2" ON "s"."PickupStatusId" = "r2"."pickupStatusId"
ORDER BY "s"."PickupStatusId"
""");
    }

    public override async Task Three_sequential_joins_marker_survives_two_remaps()
    {
        await base.Three_sequential_joins_marker_survives_two_remaps();

        AssertSql(
            """
SELECT "s1"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
INNER JOIN "Statuses" AS "s0" ON "s"."PickupStatusId" = "s0"."PickupStatusId"
INNER JOIN "Statuses" AS "s1" ON "s0"."PickupStatusId" = "s1"."PickupStatusId"
ORDER BY "s1"."PickupStatusId"
""");
    }

    public override async Task Marker_object_nested_in_outer_wrapper_across_second_join()
    {
        await base.Marker_object_nested_in_outer_wrapper_across_second_join();

        AssertSql(
            """
SELECT "s0"."PickupStatusId", "r0"."pickupStatusId", "r0"."Count", "r0"."marker"
FROM "Statuses" AS "s"
LEFT JOIN (
    SELECT "r"."PickupStatusId" AS "pickupStatusId", COUNT(*) AS "Count", 1 AS "marker"
    FROM "Requests" AS "r"
    GROUP BY "r"."PickupStatusId"
) AS "r0" ON "s"."PickupStatusId" = "r0"."pickupStatusId"
INNER JOIN "Statuses" AS "s0" ON "s"."PickupStatusId" = "s0"."PickupStatusId"
ORDER BY "s0"."PickupStatusId"
""");
    }

    #endregion
}
