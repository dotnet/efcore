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
}
