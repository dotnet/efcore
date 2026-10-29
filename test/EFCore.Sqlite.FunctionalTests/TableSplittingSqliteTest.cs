// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore;

public class TableSplittingSqliteTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : TableSplittingTestBase(fixture, testOutputHelper)
{
    public override async Task Can_remove_optional_dependent_with_json_complex_property()
    {
        await base.Can_remove_optional_dependent_with_json_complex_property();

        AssertSql(
            """
@p0=NULL
@p2='1'
@p1=NULL

UPDATE "Cars" SET "Specs" = @p0, "Color" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "c1"."c"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Specs" AS "c", "c0"."Specs"
    FROM "Cars" AS "c0"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "c1" ON "c"."Id" = CASE
    WHEN "c1"."Color" IS NOT NULL AND "c1"."Specs" IS NOT NULL THEN "c1"."Id"
END
ORDER BY "c"."Id"
""");
    }

    public override async Task Can_replace_dependent_with_sibling_type_sharing_json_column()
    {
        await base.Can_replace_dependent_with_sibling_type_sharing_json_column();

        AssertSql(
            """
@p0='{"Doors":5}' (Nullable = false) (Size = 11)
@p1='{"Doors":7}' (Nullable = false) (Size = 11)
@p4='1'
@p2='Blue' (Size = 4)
@p3='FamilyCarDetails' (Size = 16)

UPDATE "Cars" SET "Specs" = @p0, "Extras" = @p1, "Color" = @p2, "Discriminator" = @p3
WHERE "Id" = @p4
RETURNING 1;
""",
            //
            """
SELECT "c"."Id", "c"."Name", "c1"."Id", "c1"."Color", "c1"."Discriminator", "c1"."c", "c1"."c0", "c1"."c0"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Discriminator", "c0"."Specs" AS "c", "c0"."Extras" AS "c0"
    FROM "Cars" AS "c0"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Discriminator" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "c1" ON "c"."Id" = "c1"."Id"
LIMIT 2
""");
    }

    public override async Task Can_replace_dependent_with_base_type_without_any_json_column()
    {
        await base.Can_replace_dependent_with_base_type_without_any_json_column();

        AssertSql(
            """
@p0=NULL
@p3='1'
@p1='Blue' (Size = 4)
@p2='CarDetails' (Size = 10)

UPDATE "Cars" SET "SportsExtras" = @p0, "Color" = @p1, "Discriminator" = @p2
WHERE "Id" = @p3
RETURNING 1;
""",
            //
            """
SELECT "s"."Value"
FROM (
    SELECT SportsExtras AS Value FROM Cars
) AS "s"
LIMIT 2
""",
            //
            """
SELECT "c"."Id", "c"."Name", "c1"."Id", "c1"."Color", "c1"."Discriminator", "c1"."c"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Discriminator", "c0"."SportsExtras" AS "c"
    FROM "Cars" AS "c0"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Discriminator" IS NOT NULL
) AS "c1" ON "c"."Id" = "c1"."Id"
LIMIT 2
""");
    }

    public override async Task Can_replace_TPT_dependent_with_derived_type_and_back()
    {
        await base.Can_replace_TPT_dependent_with_derived_type_and_back();

        AssertSql(
            """
@p0='{"Doors":3}' (Nullable = false) (Size = 11)
@p2='1'
@p1='Blue' (Size = 4)

UPDATE "Cars" SET "Specs" = @p0, "Color" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p0='{"Doors":4}' (Nullable = false) (Size = 11)
@p1='1'

INSERT INTO "SportsCarDetails" ("Specs", "Id")
VALUES (@p0, @p1);
""",
            //
            """
SELECT "c"."Id", "c"."Name", "s0"."Id", "s0"."Color", "s0"."c", "s0"."c0", "s0"."Discriminator"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Specs" AS "c", "s"."Specs" AS "c0", CASE
        WHEN "s"."Id" IS NOT NULL THEN 'SportsCarDetails'
    END AS "Discriminator", "c0"."Specs"
    FROM "Cars" AS "c0"
    LEFT JOIN "SportsCarDetails" AS "s" ON "c0"."Id" = "s"."Id"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "s0" ON "c"."Id" = CASE
    WHEN "s0"."Color" IS NOT NULL AND "s0"."Specs" IS NOT NULL THEN "s0"."Id"
END
LIMIT 2
""",
            //
            """
@p0='{"Doors":5}' (Nullable = false) (Size = 11)
@p2='1'
@p1='Green' (Size = 5)

UPDATE "Cars" SET "Specs" = @p0, "Color" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p0='1'

DELETE FROM "SportsCarDetails"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
SELECT "c"."Id", "c"."Name", "s0"."Id", "s0"."Color", "s0"."c", "s0"."c0", "s0"."Discriminator"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Specs" AS "c", "s"."Specs" AS "c0", CASE
        WHEN "s"."Id" IS NOT NULL THEN 'SportsCarDetails'
    END AS "Discriminator", "c0"."Specs"
    FROM "Cars" AS "c0"
    LEFT JOIN "SportsCarDetails" AS "s" ON "c0"."Id" = "s"."Id"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "s0" ON "c"."Id" = CASE
    WHEN "s0"."Color" IS NOT NULL AND "s0"."Specs" IS NOT NULL THEN "s0"."Id"
END
LIMIT 2
""");
    }

    public override async Task Can_replace_dependent_with_sibling_type_using_different_json_column()
    {
        await base.Can_replace_dependent_with_sibling_type_using_different_json_column();

        AssertSql(
            """
@p0=NULL
@p1='{"Doors":5}' (Nullable = false) (Size = 11)
@p2='{"Doors":60}' (Nullable = false) (Size = 12)
@p5='1'
@p3='Blue' (Size = 4)
@p4='FamilyCarDetails' (Size = 16)

UPDATE "Cars" SET "SportsExtras" = @p0, "Specs" = @p1, "FamilyExtras" = @p2, "Color" = @p3, "Discriminator" = @p4
WHERE "Id" = @p5
RETURNING 1;
""",
            //
            """
SELECT "s"."Value"
FROM (
    SELECT SportsExtras AS Value FROM Cars
) AS "s"
LIMIT 2
""",
            //
            """
SELECT "c"."Id", "c"."Name", "c1"."Id", "c1"."Color", "c1"."Discriminator", "c1"."c", "c1"."c0", "c1"."c1"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Discriminator", "c0"."Specs" AS "c", "c0"."FamilyExtras" AS "c0", "c0"."SportsExtras" AS "c1"
    FROM "Cars" AS "c0"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Discriminator" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "c1" ON "c"."Id" = "c1"."Id"
LIMIT 2
""");
    }

    public override async Task Can_replace_dependent_with_base_type_without_json_column()
    {
        await base.Can_replace_dependent_with_base_type_without_json_column();

        AssertSql(
            """
@p0=NULL
@p1='{"Doors":5}' (Nullable = false) (Size = 11)
@p4='1'
@p2='Blue' (Size = 4)
@p3='CarDetails' (Size = 10)

UPDATE "Cars" SET "SportsExtras" = @p0, "Specs" = @p1, "Color" = @p2, "Discriminator" = @p3
WHERE "Id" = @p4
RETURNING 1;
""",
            //
            """
SELECT "s"."Value"
FROM (
    SELECT SportsExtras AS Value FROM Cars
) AS "s"
LIMIT 2
""",
            //
            """
SELECT "c"."Id", "c"."Name", "c1"."Id", "c1"."Color", "c1"."Discriminator", "c1"."c", "c1"."c0", "c1"."c1"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Discriminator", "c0"."Specs" AS "c", "c0"."FamilyExtras" AS "c0", "c0"."SportsExtras" AS "c1"
    FROM "Cars" AS "c0"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Discriminator" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "c1" ON "c"."Id" = "c1"."Id"
LIMIT 2
""");
    }

    public override async Task Can_query_optional_dependent_with_nullable_json_and_scalar_properties(bool buffered)
    {
        await base.Can_query_optional_dependent_with_nullable_json_and_scalar_properties(buffered);

        AssertSql(
            """
SELECT "g"."Id", "g1"."Id", "g1"."Label", "g1"."c"
FROM "Gadgets" AS "g"
LEFT JOIN (
    SELECT "g0"."Id", "g0"."Label", "g0"."Specs" AS "c", "g0"."Specs"
    FROM "Gadgets" AS "g0"
    WHERE "g0"."Label" IS NOT NULL OR "g0"."Specs" IS NOT NULL
) AS "g1" ON "g"."Id" = CASE
    WHEN "g1"."Label" IS NOT NULL OR "g1"."Specs" IS NOT NULL THEN "g1"."Id"
END
ORDER BY "g"."Id"
""",
            //
            """
SELECT CASE
    WHEN "g"."Label" IS NOT NULL OR "g"."Specs" IS NOT NULL THEN "g"."Id"
END
FROM "Gadgets" AS "g"
WHERE "g"."Label" IS NOT NULL OR "g"."Specs" IS NOT NULL
ORDER BY CASE
    WHEN "g"."Label" IS NOT NULL OR "g"."Specs" IS NOT NULL THEN "g"."Id"
END
""",
            //
            """
SELECT "g"."Id"
FROM "Gadgets" AS "g"
LEFT JOIN (
    SELECT "g0"."Id", "g0"."Label", "g0"."Specs" AS "c", "g0"."Specs"
    FROM "Gadgets" AS "g0"
    WHERE "g0"."Label" IS NOT NULL OR "g0"."Specs" IS NOT NULL
) AS "g1" ON "g"."Id" = CASE
    WHEN "g1"."Label" IS NOT NULL OR "g1"."Specs" IS NOT NULL THEN "g1"."Id"
END
WHERE "g1"."Label" IS NULL AND "g1"."c" IS NULL
""",
            //
            """
SELECT "g"."Id"
FROM "Gadgets" AS "g"
LEFT JOIN (
    SELECT "g0"."Id", "g0"."Label", "g0"."Specs" AS "c", "g0"."Specs"
    FROM "Gadgets" AS "g0"
    WHERE "g0"."Label" IS NOT NULL OR "g0"."Specs" IS NOT NULL
) AS "g1" ON "g"."Id" = CASE
    WHEN "g1"."Label" IS NOT NULL OR "g1"."Specs" IS NOT NULL THEN "g1"."Id"
END
WHERE "g1"."Label" IS NOT NULL OR "g1"."c" IS NOT NULL
ORDER BY "g"."Id"
""",
            //
            """
SELECT "g"."Id", "g"."Label", "g"."Specs"
FROM "Gadgets" AS "g"
WHERE "g"."Label" IS NOT NULL OR "g"."Specs" IS NOT NULL
ORDER BY CASE
    WHEN "g"."Label" IS NOT NULL OR "g"."Specs" IS NOT NULL THEN "g"."Id"
END
""",
            //
            """
SELECT "m"."Id", "m"."Label", "m"."Specs"
FROM (
    SELECT Specs, Id, Label FROM Gadgets
) AS "m"
""");
    }

    public override async Task Can_query_optional_dependent_with_only_json_complex_collection()
    {
        await base.Can_query_optional_dependent_with_only_json_complex_collection();

        AssertSql(
            """
SELECT "b"."Id", "b"."Name", "b1"."Id", "b1"."c"
FROM "Boats" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Boats" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
ORDER BY "b"."Id"
""",
            //
            """
SELECT CASE
    WHEN "b"."Specs" IS NOT NULL THEN "b"."Id"
END
FROM "Boats" AS "b"
WHERE "b"."Specs" IS NOT NULL
ORDER BY CASE
    WHEN "b"."Specs" IS NOT NULL THEN "b"."Id"
END
""",
            //
            """
SELECT "b"."Id", "b"."Name"
FROM "Boats" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Boats" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
WHERE "b1"."c" IS NULL
LIMIT 2
""",
            //
            """
SELECT "b"."Id"
FROM "Boats" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Boats" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
WHERE "b1"."c" IS NOT NULL
ORDER BY "b"."Id"
""",
            //
            """
@p0=NULL
@p1='1'

UPDATE "Boats" SET "Specs" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b1"."Id", "b1"."c"
FROM "Boats" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Boats" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
ORDER BY "b"."Id"
""");
    }

    public override async Task Can_query_optional_dependent_with_only_json_complex_property()
    {
        await base.Can_query_optional_dependent_with_only_json_complex_property();

        AssertSql(
            """
SELECT "b"."Id", "b"."Name", "b1"."Id", "b1"."c"
FROM "Bikes" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Bikes" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
ORDER BY "b"."Id"
""",
            //
            """
SELECT "b"."Id", "b"."Specs"
FROM "Bikes" AS "b"
WHERE "b"."Specs" IS NOT NULL
LIMIT 2
""",
            //
            """
SELECT "b"."Id", "b"."Name"
FROM "Bikes" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Bikes" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
WHERE "b1"."c" IS NULL
LIMIT 2
""",
            //
            """
SELECT "b"."Id", "b"."Name"
FROM "Bikes" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Bikes" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
WHERE "b1"."c" IS NOT NULL
LIMIT 2
""",
            //
            """
@p0=NULL
@p1='1'

UPDATE "Bikes" SET "Specs" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b1"."Id", "b1"."c"
FROM "Bikes" AS "b"
LEFT JOIN (
    SELECT "b0"."Id", "b0"."Specs" AS "c", "b0"."Specs"
    FROM "Bikes" AS "b0"
    WHERE "b0"."Specs" IS NOT NULL
) AS "b1" ON "b"."Id" = CASE
    WHEN "b1"."Specs" IS NOT NULL THEN "b1"."Id"
END
""");
    }

    public override async Task Can_remove_optional_TPT_dependent_with_json_complex_properties()
    {
        await base.Can_remove_optional_TPT_dependent_with_json_complex_properties();

        AssertSql(
            """
@p0=NULL
@p2='1'
@p1=NULL

UPDATE "Cars" SET "Specs" = @p0, "Color" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p0='1'

DELETE FROM "SportsCarDetails"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
SELECT "c"."Id", "c"."Name", "s0"."Id", "s0"."Color", "s0"."c", "s0"."c0", "s0"."Discriminator"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Specs" AS "c", "s"."Specs" AS "c0", CASE
        WHEN "s"."Id" IS NOT NULL THEN 'SportsCarDetails'
    END AS "Discriminator", "c0"."Specs"
    FROM "Cars" AS "c0"
    LEFT JOIN "SportsCarDetails" AS "s" ON "c0"."Id" = "s"."Id"
    WHERE "c0"."Color" IS NOT NULL AND "c0"."Specs" IS NOT NULL
) AS "s0" ON "c"."Id" = CASE
    WHEN "s0"."Color" IS NOT NULL AND "s0"."Specs" IS NOT NULL THEN "s0"."Id"
END
ORDER BY "c"."Id"
""");
    }

    public override async Task ExecuteUpdate_works_for_table_sharing(bool async)
    {
        await base.ExecuteUpdate_works_for_table_sharing(async);

        AssertSql(
            """
@p='1'

UPDATE "Vehicles" AS "v"
SET "SeatingCapacity" = @p
""",
            //
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Vehicles" AS "v"
    WHERE "v"."SeatingCapacity" <> 1)
""");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).HasComputedColumnSql("1");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
