// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesSqliteTest(NonSharedFixture fixture) : NonSharedModelUpdatesTestBase(fixture)
{
    [Theory, InlineData(false), InlineData(true)]
    public async Task Tracking_query_preserves_replaced_owned_reference(bool json)
    {
        var contextFactory = await InitializeNonSharedTest<Context39595>(
            onModelCreating: b => b.Entity<Context39595.Owner>().OwnsOne(
                o => o.Patch, p =>
                {
                    if (json)
                    {
                        p.ToJson();
                    }
                }));

        await using (var context = contextFactory.CreateDbContext())
        {
            var owner = new Context39595.Owner { Patch = new Context39595.Patch { Start = 5 } };
            context.Add(owner);
            await context.SaveChangesAsync();

            var original = owner.Patch;
            owner.Patch = new Context39595.Patch { Title = "new title", Start = 5 };
            Assert.Equal(EntityState.Added, context.Entry(owner).Reference(o => o.Patch).TargetEntry!.State);
            Assert.Equal(EntityState.Deleted, context.Entry(original).State);

            Assert.Same(owner, await context.Set<Context39595.Owner>().SingleAsync());

            Assert.Equal(EntityState.Added, context.Entry(owner.Patch).State);
            Assert.Equal(EntityState.Deleted, context.Entry(original).State);
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<Context39595.Owner>().SingleAsync();
            Assert.Equal("new title", owner.Patch.Title);
            Assert.Equal(5, owner.Patch.Start);
        }
    }

    protected class Context39595(DbContextOptions options) : DbContext(options)
    {
        public class Owner
        {
            public int Id { get; set; }
            public Patch Patch { get; set; } = null!;
        }

        public class Patch
        {
            public string? Title { get; set; }
            public int? Start { get; set; }
        }
    }

    public override async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        await base.Principal_and_dependent_roundtrips_with_cycle_breaking(async);

        AssertSql(
            """
@p0='AC South' (Size = 8)

INSERT INTO "AuthorsClub" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p1='1'
@p2='Alice' (Size = 5)

INSERT INTO "Author" ("AuthorsClubId", "Name")
VALUES (@p1, @p2)
RETURNING "Id";
""",
            //
            """
@p3='1'
@p4=NULL

INSERT INTO "Book" ("AuthorId", "Title")
VALUES (@p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "b"."Id", "b"."AuthorId", "b"."Title", "a"."Id", "a"."AuthorsClubId", "a"."Name"
FROM "Book" AS "b"
INNER JOIN "Author" AS "a" ON "b"."AuthorId" = "a"."Id"
LIMIT 2
""",
            //
            """
@p0='AC North' (Size = 8)

INSERT INTO "AuthorsClub" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p1='2'
@p2='Author of the year 2023' (Size = 23)

INSERT INTO "Author" ("AuthorsClubId", "Name")
VALUES (@p1, @p2)
RETURNING "Id";
""",
            //
            """
@p4='1'
@p3='2'

UPDATE "Book" SET "AuthorId" = @p3
WHERE "Id" = @p4
RETURNING 1;
""",
            //
            """
@p0='1'

DELETE FROM "Author"
WHERE "Id" = @p0
RETURNING 1;
""");
    }

    public override async Task DbUpdateException_Entries_is_correct_with_multiple_inserts(bool async)
    {
        await base.DbUpdateException_Entries_is_correct_with_multiple_inserts(async);

        AssertSql(
            """
@p0='Blog2' (Size = 5)

INSERT INTO "Blog" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='Blog1' (Size = 5)

INSERT INTO "Blog" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='Blog2' (Size = 5)

INSERT INTO "Blog" ("Name")
VALUES (@p0)
RETURNING "Id";
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    public override async Task Nullable_json_collection_change_is_saved(
        bool async,
        NullableJsonCollectionValue from,
        NullableJsonCollectionValue to,
        bool alsoModifyName)
    {
        await base.Nullable_json_collection_change_is_saved(async, from, to, alsoModifyName);

        if (to == NullableJsonCollectionValue.WithElements)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3='T1' (Size = 2)

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 2)

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" = @p3
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else if (from == NullableJsonCollectionValue.Empty && !alsoModifyName)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4='T1' (Size = 2)

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 2)

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" = @p3
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else if (from == NullableJsonCollectionValue.Empty)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4='T1' (Size = 2)

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p3='1'
@p2='B' (Size = 1)
@p4='T1' (Size = 2)

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Name" = @p2
WHERE "Id" = @p3 AND "Token" = @p4
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else if (from == NullableJsonCollectionValue.Null && !alsoModifyName)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3='T1' (Size = 2)

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 2)

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" = @p3
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else if (from == NullableJsonCollectionValue.Null)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3='T1' (Size = 2)

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p3='1'
@p2='B' (Size = 1)
@p4='T1' (Size = 2)

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Name" = @p2
WHERE "Id" = @p3 AND "Token" = @p4
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4='T1' (Size = 2)

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 2)

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" = @p3
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_nested_in_a_json_reference_changed_from_empty_to_null_is_saved(
        bool async,
        bool alsoModifyReference)
    {
        await base.Nullable_json_collection_nested_in_a_json_reference_changed_from_empty_to_null_is_saved(async, alsoModifyReference);

        if (alsoModifyReference)
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R2","Items":null}' (Nullable = false) (Size = 26)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Reference" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Reference" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_nested_in_a_json_reference_changed_from_null_is_saved(bool async, bool withElements)
    {
        await base.Nullable_json_collection_nested_in_a_json_reference_changed_from_null_is_saved(async, withElements);

        if (withElements)
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R","Items":[{"Name":"C1"}]}' (Nullable = false) (Size = 36)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Reference" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Reference" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_nested_in_json_collection_elements_changed_from_empty_to_null_is_saved(bool async)
    {
        await base.Nullable_json_collection_nested_in_json_collection_elements_changed_from_empty_to_null_is_saved(async);

        AssertSql(
            """
@p0='[{"Name":"O1","Inner":[]},{"Name":"O2","Inner":[]}]' (Nullable = false) (Size = 51)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Outer", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0='{"Name":"O1","Inner":null}' (Nullable = false) (Size = 26)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Outer" = json_set("Outer", '$[0]', json(@p0)), "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_nested_in_json_collection_elements_changed_from_null_is_saved(
        bool async,
        bool withElements)
    {
        await base.Nullable_json_collection_nested_in_json_collection_elements_changed_from_null_is_saved(async, withElements);

        if (withElements)
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null},{"Name":"O2","Inner":null}]' (Nullable = false) (Size = 55)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Outer", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"O1","Inner":[{"Name":"C1"}]}' (Nullable = false) (Size = 37)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Outer" = json_set("Outer", '$[0]', json(@p0)), "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null},{"Name":"O2","Inner":null}]' (Nullable = false) (Size = 55)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Outer", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"O1","Inner":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Outer" = json_set("Outer", '$[0]', json(@p0)), "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved(bool async)
    {
        await base.Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonDerivedParent' (Nullable = false) (Size = 25)
@p3='Derived' (Size = 7)
@p4=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "DerivedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='Base' (Size = 4)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Discriminator" = 'NullableJsonDerivedParent'
LIMIT 2
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1=NULL (Nullable = false)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "InitializedChildren" = @p0, "DerivedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
ORDER BY "n"."Name"
""");
    }

    public override async Task Nullable_json_collection_stored_as_null_or_empty_is_loaded_as_null_or_empty(
        bool async,
        NullableJsonCollectionQuery queryKind)
    {
        await base.Nullable_json_collection_stored_as_null_or_empty_is_loaded_as_null_or_empty(async, queryKind);

        if (queryKind == NullableJsonCollectionQuery.Tracking)
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null},{"Name":"O2","Inner":[]}]' (Nullable = false) (Size = 53)
@p1='{"Name":"R1","Items":null}' (Nullable = false) (Size = 26)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='Null' (Size = 4)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Outer", "Reference", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
@p0='{"Name":"R2","Items":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='[]' (Nullable = false) (Size = 2)
@p3='[]' (Nullable = false) (Size = 2)
@p4='NullableJsonParent' (Nullable = false) (Size = 18)
@p5='Empty' (Size = 5)
@p6=NULL

INSERT INTO "NullableJsonParent" ("Reference", "Children", "InitializedChildren", "Outer", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
ORDER BY "n"."Name"
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
ORDER BY "n"."Name"
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null},{"Name":"O2","Inner":[]}]' (Nullable = false) (Size = 53)
@p1='{"Name":"R1","Items":null}' (Nullable = false) (Size = 26)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='Null' (Size = 4)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Outer", "Reference", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
@p0='{"Name":"R2","Items":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='[]' (Nullable = false) (Size = 2)
@p3='[]' (Nullable = false) (Size = 2)
@p4='NullableJsonParent' (Nullable = false) (Size = 18)
@p5='Empty' (Size = 5)
@p6=NULL

INSERT INTO "NullableJsonParent" ("Reference", "Children", "InitializedChildren", "Outer", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
ORDER BY "n"."Name"
""");
        }
    }

    public override async Task Nullable_json_collection_missing_from_the_stored_json_is_loaded_as_null(bool async, bool tracking)
    {
        await base.Nullable_json_collection_missing_from_the_stored_json_is_loaded_as_null(async, tracking);

        if (tracking)
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":[]}]' (Nullable = false) (Size = 26)
@p1='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonParent' (Nullable = false) (Size = 18)
@p4='A' (Size = 1)
@p5=NULL

INSERT INTO "NullableJsonParent" ("Outer", "Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
""",
                //
                """
UPDATE "NullableJsonParent" SET "Reference" = '{"Name":"R"}'
""",
                //
                """
UPDATE "NullableJsonParent" SET "Outer" = '[{"Name":"O1"}]'
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 1)
@p3=NULL

UPDATE "NullableJsonParent" SET "InitializedChildren" = @p0, "Name" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":[]}]' (Nullable = false) (Size = 26)
@p1='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonParent' (Nullable = false) (Size = 18)
@p4='A' (Size = 1)
@p5=NULL

INSERT INTO "NullableJsonParent" ("Outer", "Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
""",
                //
                """
UPDATE "NullableJsonParent" SET "Reference" = '{"Name":"R"}'
""",
                //
                """
UPDATE "NullableJsonParent" SET "Outer" = '[{"Name":"O1"}]'
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_projected_directly_is_loaded_as_null_or_empty(bool async, bool nested)
    {
        await base.Nullable_json_collection_projected_directly_is_loaded_as_null_or_empty(async, nested);

        if (nested)
        {
            AssertSql(
                """
@p0='{"Name":"R1","Items":null}' (Nullable = false) (Size = 26)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='Null' (Size = 4)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
@p0='{"Name":"R2","Items":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonParent' (Nullable = false) (Size = 18)
@p4='Empty' (Size = 5)
@p5=NULL

INSERT INTO "NullableJsonParent" ("Reference", "Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Reference" ->> 'Items', "n"."Id"
FROM "NullableJsonParent" AS "n"
ORDER BY "n"."Name"
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R1","Items":null}' (Nullable = false) (Size = 26)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='Null' (Size = 4)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
@p0='{"Name":"R2","Items":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonParent' (Nullable = false) (Size = 18)
@p4='Empty' (Size = 5)
@p5=NULL

INSERT INTO "NullableJsonParent" ("Reference", "Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Children", "n"."Id"
FROM "NullableJsonParent" AS "n"
ORDER BY "n"."Name"
""");
        }
    }

    public override async Task Update_of_a_disconnected_entity_saves_the_nullness_of_a_nullable_json_collection(bool async, bool toNull)
    {
        await base.Update_of_a_disconnected_entity_saves_the_nullness_of_a_nullable_json_collection(async, toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p5='1'
@p3='A' (Size = 1)
@p4=NULL
@p6=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Outer" = @p2, "Name" = @p3, "Token" = @p4
WHERE "Id" = @p5 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p5='1'
@p3='A' (Size = 1)
@p4=NULL
@p6=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Outer" = @p2, "Name" = @p3, "Token" = @p4
WHERE "Id" = @p5 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Replacing_an_owner_with_the_same_key_saves_the_nullness_of_a_nullable_json_collection(
        bool async,
        bool toNull)
    {
        await base.Replacing_an_owner_with_the_same_key_saves_the_nullness_of_a_nullable_json_collection(async, toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p4='1'
@p3='B' (Size = 1)
@p5=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Outer" = @p2, "Name" = @p3
WHERE "Id" = @p4 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p4='1'
@p3='B' (Size = 1)
@p5=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Outer" = @p2, "Name" = @p3
WHERE "Id" = @p4 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch(bool async)
    {
        await base.Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='ToNull' (Size = 6)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='ToEmpty' (Size = 7)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='OnlyName' (Size = 8)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='Elements' (Size = 8)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='2'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p3='3'
@p2='OnlyNameChanged' (Size = 15)
@p4=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Name" = @p2
WHERE "Id" = @p3 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='4'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
""");
    }

    public override async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(
        bool async,
        bool toNull,
        bool detectChanges)
    {
        await base.Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(async, toNull, detectChanges);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nested_nullable_json_collection(
        bool async,
        bool inReference)
    {
        await base.Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nested_nullable_json_collection(async, inReference);

        if (inReference)
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null}]' (Nullable = false) (Size = 28)
@p1='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonParent' (Nullable = false) (Size = 18)
@p4='A' (Size = 1)
@p5=NULL

INSERT INTO "NullableJsonParent" ("Outer", "Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Reference" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null}]' (Nullable = false) (Size = 28)
@p1='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonParent' (Nullable = false) (Size = 18)
@p4='A' (Size = 1)
@p5=NULL

INSERT INTO "NullableJsonParent" ("Outer", "Reference", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"O1","Inner":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Outer" = json_set("Outer", '$[0]', json(@p0)), "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Streamed_duplicate_of_a_tracked_entity_does_not_overwrite_a_change_to_a_nullable_json_collection(bool async)
    {
        await base.Streamed_duplicate_of_a_tracked_entity_does_not_overwrite_a_change_to_a_nullable_json_collection(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='B' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
CROSS JOIN "NullableJsonParent" AS "n0"
WHERE "n"."Name" = 'A'
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Name" = 'A'
LIMIT 2
""");
    }

    public override async Task Compiled_query_enumerated_twice_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(bool async)
    {
        await base.Compiled_query_enumerated_twice_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='B' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Name" = 'A'
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Name" = 'A'
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Name" = 'A'
LIMIT 2
""");
    }

    public override async Task Querying_a_tracked_entity_keeps_a_pending_null_of_a_nullable_json_collection_that_is_stored_with_elements(
        bool async)
    {
        await base.Querying_a_tracked_entity_keeps_a_pending_null_of_a_nullable_json_collection_that_is_stored_with_elements(async);

        AssertSql(
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p5='1'
@p3='A' (Size = 1)
@p4=NULL
@p6=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1, "Outer" = @p2, "Name" = @p3, "Token" = @p4
WHERE "Id" = @p5 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Querying_a_tracked_entity_keeps_the_in_memory_nullness_of_a_nullable_json_collection(
        bool async,
        bool inMemoryNull)
    {
        await base.Querying_a_tracked_entity_keeps_the_in_memory_nullness_of_a_nullable_json_collection(async, inMemoryNull);

        if (inMemoryNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
UPDATE "NullableJsonParent" SET "Children" = '[]'
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
UPDATE "NullableJsonParent" SET "Children" = NULL
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Reloading_an_entity_discards_a_pending_change_of_a_nullable_json_collection(bool async, bool toNull)
    {
        await base.Reloading_an_entity_discards_a_pending_change_of_a_nullable_json_collection(async, toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p='1'

SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Id" = @p
LIMIT 1
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 18)
@p2='A' (Size = 1)
@p3=NULL

INSERT INTO "NullableJsonParent" ("InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p='1'

SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token"
FROM "NullableJsonParent" AS "n"
WHERE "n"."Id" = @p
LIMIT 1
""");
        }
    }

    public override async Task Json_collection_that_is_not_nullable_or_opted_out_stored_as_null_is_loaded_as_empty_with_initializer(
        bool async)
    {
        await base.Json_collection_that_is_not_nullable_or_opted_out_stored_as_null_is_loaded_as_empty_with_initializer(async);

        AssertSql(
            """
INSERT INTO "NotNullableJsonParent"
DEFAULT VALUES
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Children", "n"."OptedOutChildren", "n"."RequiredChildren"
FROM "NotNullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Json_collection_that_is_opted_out_of_being_nullable_set_to_null_is_not_saved_as_null(bool async)
    {
        await base.Json_collection_that_is_opted_out_of_being_nullable_set_to_null_is_not_saved_as_null(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='[]' (Nullable = false) (Size = 2)

INSERT INTO "NotNullableJsonParent" ("Children", "OptedOutChildren", "RequiredChildren")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Children", "n"."OptedOutChildren", "n"."RequiredChildren"
FROM "NotNullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
SELECT "n"."Id", "n"."Children", "n"."OptedOutChildren", "n"."RequiredChildren"
FROM "NotNullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes(bool async)
    {
        await base.Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "CoercingNullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
UPDATE "CoercingNullableJsonParent" SET "Children" = NULL
""",
            //
            """
SELECT "c"."Id", "c"."Name", "c"."Children"
FROM "CoercingNullableJsonParent" AS "c"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_of_an_owner_loaded_from_several_rows_is_not_changed_by_the_later_rows(
        bool async,
        bool splitQuery)
    {
        await base.Nullable_json_collection_of_an_owner_loaded_from_several_rows_is_not_changed_by_the_later_rows(async, splitQuery);

        if (splitQuery)
        {
            AssertSql(
                """
@p0='Null' (Size = 4)

INSERT INTO "NullableJsonBlog" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='Elements' (Size = 8)

INSERT INTO "NullableJsonBlog" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p2='1'
@p3='P1' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p2, @p3)
RETURNING "Id";
""",
                //
                """
@p0='1'
@p1='P2' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p0='2'
@p1='P3' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p0='2'
@p1='P4' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonBlog" AS "n"
ORDER BY "n"."Name", "n"."Id"
""",
                //
                """
SELECT "n0"."Id", "n0"."BlogId", "n0"."Title", "n"."Id"
FROM "NullableJsonBlog" AS "n"
INNER JOIN "NullableJsonPost" AS "n0" ON "n"."Id" = "n0"."BlogId"
ORDER BY "n"."Name", "n"."Id"
""");
        }
        else
        {
            AssertSql(
                """
@p0='Null' (Size = 4)

INSERT INTO "NullableJsonBlog" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='Elements' (Size = 8)

INSERT INTO "NullableJsonBlog" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p2='1'
@p3='P1' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p2, @p3)
RETURNING "Id";
""",
                //
                """
@p0='1'
@p1='P2' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p0='2'
@p1='P3' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p0='2'
@p1='P4' (Size = 2)

INSERT INTO "NullableJsonPost" ("BlogId", "Title")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children", "n0"."Id", "n0"."BlogId", "n0"."Title"
FROM "NullableJsonBlog" AS "n"
LEFT JOIN "NullableJsonPost" AS "n0" ON "n"."Id" = "n0"."BlogId"
ORDER BY "n"."Name", "n"."Id"
""");
        }
    }

    public override async Task Nullable_json_collection_with_elements_set_to_null_is_saved_when_orphans_are_deleted_on_SaveChanges(
        bool async)
    {
        await base.Nullable_json_collection_with_elements_set_to_null_is_saved_when_orphans_are_deleted_on_SaveChanges(async);

        AssertSql(
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 18)
@p3='A' (Size = 1)
@p4=NULL

INSERT INTO "NullableJsonParent" ("Children", "InitializedChildren", "Discriminator", "Name", "Token")
VALUES (@p0, @p1, @p2, @p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL

UPDATE "NullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2 AND "Token" IS NULL
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Token", "n"."Children", "n"."InitializedChildren", "n"."Outer", "n"."Reference", "n"."DerivedChildren"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(
        bool async,
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(async, strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("Children", "InitializedChildren", "Name")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'

UPDATE "NotifyingNullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_replaced_between_null_and_populated_is_saved(
        bool async,
        ChangeTrackingStrategy strategy,
        bool toNull)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_replaced_between_null_and_populated_is_saved(async, strategy, toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("Children", "InitializedChildren", "Name")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'

UPDATE "NotifyingNullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("InitializedChildren", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[{"Name":"C3"},{"Name":"C4"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'

UPDATE "NotifyingNullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_loaded_by_a_query_is_notified_of_added_and_removed_elements(
        bool async,
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_loaded_by_a_query_is_notified_of_added_and_removed_elements(async, strategy);

        AssertSql(
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("Children", "InitializedChildren", "Name")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='B' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("Children", "InitializedChildren", "Name")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
ORDER BY "n"."Name"
""",
            //
            """
@p0='[{"Name":"C2"},{"Name":"C3"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'

UPDATE "NotifyingNullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p0='[{"Name":"D1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='2'

UPDATE "NotifyingNullableJsonParent" SET "Children" = @p0, "InitializedChildren" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
ORDER BY "n"."Name"
""");
    }

    public override async Task Nullable_json_collections_of_a_notifying_entity_stored_as_null_stay_null_when_tracked(
        bool async,
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collections_of_a_notifying_entity_stored_as_null_stay_null_when_tracked(async, strategy);

        AssertSql(
            """
@p0='[{"Name":"I1","Inner":null},{"Name":"I2","Inner":[]}]' (Nullable = false) (Size = 53)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("Outer", "InitializedChildren", "Name")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 1)

UPDATE "NotifyingNullableJsonParent" SET "InitializedChildren" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(
        bool async,
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(async, strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("InitializedChildren", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
UPDATE "NotifyingNullableJsonParent" SET "InitializedChildren" = NULL
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children", "n"."InitializedChildren", "n"."Outer"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
