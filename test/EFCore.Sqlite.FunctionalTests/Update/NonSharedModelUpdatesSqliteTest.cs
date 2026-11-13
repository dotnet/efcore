// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesSqliteTest(NonSharedFixture fixture) : NonSharedModelUpdatesTestBase(fixture)
{
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

    public override async Task Nullable_json_collection_changed_from_empty_to_null_is_saved(bool alsoModifyName)
    {
        await base.Nullable_json_collection_changed_from_empty_to_null_is_saved(alsoModifyName);

        if (alsoModifyName)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p2='1'
@p1='B' (Size = 1)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_changed_from_null_to_empty_is_saved(bool alsoModifyName)
    {
        await base.Nullable_json_collection_changed_from_null_to_empty_is_saved(alsoModifyName);

        if (alsoModifyName)
        {
            AssertSql(
                """
@p0='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 1)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_with_elements_changed_to_null_is_saved()
    {
        await base.Nullable_json_collection_with_elements_changed_to_null_is_saved();

        AssertSql(
            """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_nested_in_json_changed_from_empty_to_null_is_saved(bool alsoModifyReference)
    {
        await base.Nullable_json_collection_nested_in_json_changed_from_empty_to_null_is_saved(alsoModifyReference);

        if (alsoModifyReference)
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonReferenceParent" ("Reference", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Reference"
FROM "NullableJsonReferenceParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R2","Items":null}' (Nullable = false) (Size = 26)
@p1='1'

UPDATE "NullableJsonReferenceParent" SET "Reference" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Reference"
FROM "NullableJsonReferenceParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonReferenceParent" ("Reference", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Reference"
FROM "NullableJsonReferenceParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='1'

UPDATE "NullableJsonReferenceParent" SET "Reference" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Reference"
FROM "NullableJsonReferenceParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Update_of_a_disconnected_entity_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        await base.Update_of_a_disconnected_entity_saves_the_null_ness_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
@p0=NULL (Nullable = false)
@p2='1'
@p1='A' (Size = 1)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='A' (Size = 1)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NotifyingNullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "NotifyingNullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NotifyingNullableJsonParent" AS "n"
LIMIT 2
""");
    }

    public override async Task Replacing_an_owner_with_the_same_key_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        await base.Replacing_an_owner_with_the_same_key_saves_the_null_ness_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0=NULL (Nullable = false)
@p2='1'
@p1='B' (Size = 1)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 1)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes()
    {
        await base.Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes();

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
UPDATE [CoercingNullableJsonParent] SET [Children] = NULL
""",
            //
            """
SELECT "c"."Id", "c"."Name", "c"."Children"
FROM "CoercingNullableJsonParent" AS "c"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 1)

INSERT INTO "InitializedNotifyingNullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
UPDATE [InitializedNotifyingNullableJsonParent] SET [Children] = NULL
""",
            //
            """
SELECT "i"."Id", "i"."Name", "i"."Children"
FROM "InitializedNotifyingNullableJsonParent" AS "i"
LIMIT 2
""");
    }

    public override async Task Querying_a_tracked_entity_keeps_the_in_memory_null_ness_of_a_nullable_json_collection(bool inMemoryNull)
    {
        await base.Querying_a_tracked_entity_keeps_the_in_memory_null_ness_of_a_nullable_json_collection(inMemoryNull);

        if (inMemoryNull)
        {
            AssertSql(
                """
@p0='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
UPDATE [NullableJsonParent] SET [Children] = '[]'
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
                //
                """
UPDATE [NullableJsonParent] SET [Children] = NULL
""",
                //
                """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
        }
    }

    public override async Task Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved()
    {
        await base.Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved();

        AssertSql(
            """
@p0='NullableJsonBase' (Nullable = false) (Size = 16)
@p1='Base' (Size = 4)

INSERT INTO "NullableJsonBase" ("Discriminator", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonDerived' (Nullable = false) (Size = 19)
@p2='Derived' (Size = 7)

INSERT INTO "NullableJsonBase" ("Children", "Discriminator", "Name")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Children"
FROM "NullableJsonBase" AS "n"
WHERE "n"."Discriminator" = 'NullableJsonDerived'
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='2'

UPDATE "NullableJsonBase" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Discriminator", "n"."Name", "n"."Children"
FROM "NullableJsonBase" AS "n"
ORDER BY "n"."Name"
""");
    }

    public override async Task Nested_nullable_json_collection_changed_to_null_is_saved()
    {
        await base.Nested_nullable_json_collection_changed_to_null_is_saved();

        AssertSql(
            """
@p0='[{"Name":"O1","Inner":[]},{"Name":"O2","Inner":[]}]' (Nullable = false) (Size = 51)

INSERT INTO "NestedNullableJsonOwner" ("Items")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Items"
FROM "NestedNullableJsonOwner" AS "n"
LIMIT 2
""",
            //
            """
@p0='{"Name":"O1","Inner":null}' (Nullable = false) (Size = 26)
@p1='1'

UPDATE "NestedNullableJsonOwner" SET "Items" = json_set("Items", '$[0]', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Items"
FROM "NestedNullableJsonOwner" AS "n"
LIMIT 2
""");
    }

    public override async Task Nullable_json_collection_changed_to_null_is_saved_for_an_entity_with_a_concurrency_token()
    {
        await base.Nullable_json_collection_changed_to_null_is_saved_for_an_entity_with_a_concurrency_token();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='T1' (Size = 2)

INSERT INTO "NullableJsonParentWithToken" ("Children", "Token")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Token", "n"."Children"
FROM "NullableJsonParentWithToken" AS "n"
LIMIT 2
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'
@p2='T1' (Size = 2)

UPDATE "NullableJsonParentWithToken" SET "Children" = @p0
WHERE "Id" = @p1 AND "Token" = @p2
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Token", "n"."Children"
FROM "NullableJsonParentWithToken" AS "n"
LIMIT 2
""");
    }

    public override async Task Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch()
    {
        await base.Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='ToNull' (Size = 6)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
@p0='ToEmpty' (Size = 7)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='OnlyName' (Size = 8)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='Elements' (Size = 8)

INSERT INTO "NullableJsonParent" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='2'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p2='3'
@p1='OnlyNameChanged' (Size = 15)

UPDATE "NullableJsonParent" SET "Children" = @p0, "Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='4'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
""");
    }

    public override async Task Nullable_json_collection_loaded_without_tracking_keeps_null_and_empty()
    {
        await base.Nullable_json_collection_loaded_without_tracking_keeps_null_and_empty();

        AssertSql(
            """
@p0='Null' (Size = 4)

INSERT INTO "NullableJsonParentWithInitializer" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='Empty' (Size = 5)

INSERT INTO "NullableJsonParentWithInitializer" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParentWithInitializer" AS "n"
ORDER BY "n"."Name"
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParentWithInitializer" AS "n"
ORDER BY "n"."Name"
""");
    }

    public override async Task Nullable_json_collection_projected_with_its_entity_keeps_null_and_empty()
    {
        await base.Nullable_json_collection_projected_with_its_entity_keeps_null_and_empty();

        AssertSql(
            """
@p0='Null' (Size = 4)

INSERT INTO "NullableJsonParentWithInitializer" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='Empty' (Size = 5)

INSERT INTO "NullableJsonParentWithInitializer" ("Children", "Name")
VALUES (@p0, @p1)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParentWithInitializer" AS "n"
ORDER BY "n"."Name"
""");
    }

    public override async Task Opting_out_of_a_nullable_json_collection_keeps_the_initializer_on_load()
    {
        await base.Opting_out_of_a_nullable_json_collection_keeps_the_initializer_on_load();

        AssertSql(
            """
@p0='Null' (Size = 4)

INSERT INTO "NullableJsonParentWithInitializer" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParentWithInitializer" AS "n"
LIMIT 2
""");
    }

    public override async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection_to_empty()
    {
        await base.Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection_to_empty();

        AssertSql(
            """
@p0='A' (Size = 1)

INSERT INTO "NullableJsonParent" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

UPDATE "NullableJsonParent" SET "Children" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "n"."Id", "n"."Name", "n"."Children"
FROM "NullableJsonParent" AS "n"
LIMIT 2
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
