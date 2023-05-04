// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class JsonUpdateSqliteTest : JsonUpdateTestBase<JsonUpdateSqliteFixture>
{
    public JsonUpdateSqliteTest(JsonUpdateSqliteFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    public override async Task Add_element_to_json_collection_branch()
    {
        await base.Add_element_to_json_collection_branch();

        AssertSql(
"""
@p0='[{"Date":"2101-01-01T00:00:00","Enum":"Two","Fraction":10.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":"Three","Fraction":10.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2010-10-10T00:00:00","Enum":"Three","Fraction":42.42,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (Size = 684)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_leaf()
    {
        await base.Add_element_to_json_collection_leaf();

        AssertSql(
"""
@p0='[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"},{"SomethingSomething":"ss1"}]' (Nullable = false) (Size = 100)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedReferenceBranch.OwnedCollectionLeaf', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_on_derived()
    {
        await base.Add_element_to_json_collection_on_derived();

        AssertSql(
"""
@p0='[{"Date":"2221-01-01T00:00:00","Enum":"Two","Fraction":221.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2222-01-01T00:00:00","Enum":"Three","Fraction":222.1,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2010-10-10T00:00:00","Enum":"Three","Fraction":42.42,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (Size = 668)
@p1='2'

UPDATE "JsonEntitiesInheritance" SET "CollectionOnDerived" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Discriminator", "j"."Name", "j"."Fraction", "j"."CollectionOnBase", "j"."ReferenceOnBase", "j"."CollectionOnDerived", "j"."ReferenceOnDerived"
FROM "JsonEntitiesInheritance" AS "j"
WHERE "j"."Discriminator" = 'JsonEntityInheritanceDerived'
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_root()
    {
        await base.Add_element_to_json_collection_root();

        AssertSql(
"""
@p0='[{"Name":"e1_c1","Number":11,"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":"Two","Fraction":11.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":"Three","Fraction":11.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":"One","Fraction":11.0,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Number":12,"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":"Two","Fraction":12.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":"One","Fraction":12.2,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":"Three","Fraction":12.0,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Number":142,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":"Three","Fraction":42.42,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}]' (Nullable = false) (Size = 1867)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Add_entity_with_json()
    {
        await base.Add_entity_with_json();

        AssertSql(
"""
@p0='{"Name":"RootName","Number":42,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":"Three","Fraction":42.42,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (Size = 296)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 9)

INSERT INTO "JsonEntitiesBasic" ("OwnedReferenceRoot", "Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2, @p3);
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
""");
    }

    public override async Task Add_json_reference_leaf()
    {
        await base.Add_json_reference_leaf();

        AssertSql(
"""
@p0='{"SomethingSomething":"ss3"}' (Nullable = false) (Size = 28)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch[0].OwnedReferenceLeaf', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Add_json_reference_root()
    {
        await base.Add_json_reference_root();

        AssertSql(
"""
@p0='{"Name":"RootName","Number":42,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":"Three","Fraction":42.42,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (Size = 296)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Delete_entity_with_json()
    {
        await base.Delete_entity_with_json();

        AssertSql(
"""
@p0='1'

DELETE FROM "JsonEntitiesBasic"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
"""
SELECT COUNT(*)
FROM "JsonEntitiesBasic" AS "j"
""");
    }

    public override async Task Delete_json_collection_branch()
    {
        await base.Delete_json_collection_branch();

        AssertSql(
"""
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Delete_json_collection_root()
    {
        await base.Delete_json_collection_root();

        AssertSql(
"""
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Delete_json_reference_leaf()
    {
        await base.Delete_json_reference_leaf();

        AssertSql(
"""
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedReferenceBranch.OwnedReferenceLeaf', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Delete_json_reference_root()
    {
        await base.Delete_json_reference_root();

        AssertSql(
"""
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_collection_branch()
    {
        await base.Edit_element_in_json_collection_branch();

        AssertSql(
"""
@p0='2111-11-11T00:00:00' (Nullable = false) (Size = 19)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[0].OwnedCollectionBranch[0].Date', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_collection_root1()
    {
        await base.Edit_element_in_json_collection_root1();

        AssertSql(
"""
@p0='Modified' (Nullable = false) (Size = 8)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[0].Name', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_collection_root2()
    {
        await base.Edit_element_in_json_collection_root2();

        AssertSql(
"""
@p0='Modified' (Nullable = false) (Size = 8)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[1].Name', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_multiple_levels_partial_update()
    {
        await base.Edit_element_in_json_multiple_levels_partial_update();

        AssertSql(
"""
@p0='[{"Date":"2111-01-01T00:00:00","Enum":"Two","Fraction":11.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"...and another"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":"Three","Fraction":11.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"yet another change"},{"SomethingSomething":"and another"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}]' (Nullable = false) (Size = 485)
@p1='{"Name":"edit","Number":10,"OwnedCollectionBranch":[{"Date":"2101-01-01T00:00:00","Enum":"Two","Fraction":10.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":"Three","Fraction":10.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}}],"OwnedReferenceBranch":{"Date":"2111-11-11T00:00:00","Enum":"One","Fraction":10.0,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}}' (Nullable = false) (Size = 773)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[0].OwnedCollectionBranch', json(@p0)), "OwnedReferenceRoot" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection()
    {
        await base.Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection();

        AssertSql(
"""
@p0='[{"Date":"2101-01-01T00:00:00","Enum":"Two","Fraction":4321.3,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":"Three","Fraction":10.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2222-11-11T00:00:00","Enum":"Three","Fraction":45.32,"NullableEnum":null,"OwnedCollectionLeaf":[],"OwnedReferenceLeaf":{"SomethingSomething":"cc"}}]' (Nullable = false) (Size = 628)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection()
    {
        await base.Edit_two_elements_in_the_same_json_collection();

        AssertSql(
"""
@p0='[{"SomethingSomething":"edit1"},{"SomethingSomething":"edit2"}]' (Nullable = false) (Size = 63)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch[0].OwnedCollectionLeaf', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection_at_the_root()
    {
        await base.Edit_two_elements_in_the_same_json_collection_at_the_root();

        AssertSql(
"""
@p0='[{"Name":"edit1","Number":11,"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":"Two","Fraction":11.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":"Three","Fraction":11.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":"One","Fraction":11.0,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"edit2","Number":12,"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":"Two","Fraction":12.1,"NullableEnum":"One","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":"One","Fraction":12.2,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":"Three","Fraction":12.0,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}}]' (Nullable = false) (Size = 1569)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_collection_element_and_reference_at_once()
    {
        await base.Edit_collection_element_and_reference_at_once();

        AssertSql(
"""
@p0='{"Date":"2102-01-01T00:00:00","Enum":"Three","Fraction":10.2,"NullableEnum":"Two","OwnedCollectionLeaf":[{"SomethingSomething":"edit1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit2"}}' (Nullable = false) (Size = 225)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch[1]', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_single_enum_property()
    {
        await base.Edit_single_enum_property();

        AssertSql(
"""
@p0='Two' (Nullable = false) (Size = 3)
@p1='Two' (Nullable = false) (Size = 3)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[1].OwnedCollectionBranch[1].Enum', @p0), "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedReferenceBranch.Enum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_single_numeric_property()
    {
        await base.Edit_single_numeric_property();

        AssertSql(
"""
@p0='1024' (DbType = String)
@p1='999' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[1].Number', @p0), "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.Number', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_single_property_bool()
    {
        await base.Edit_single_property_bool();

        AssertSql(
"""
@p0='true' (Nullable = false) (Size = 4)
@p1='false' (Nullable = false) (Size = 5)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestBoolean', json(@p0)), "Reference" = json_set("Reference", '$.TestBoolean', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_byte()
    {
        await base.Edit_single_property_byte();

        AssertSql(
"""
@p0='14' (DbType = String)
@p1='25' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestByte', @p0), "Reference" = json_set("Reference", '$.TestByte', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_char()
    {
        await base.Edit_single_property_char();

        AssertSql(
"""
@p0='t' (DbType = String)
@p1='1'

UPDATE "JsonEntitiesAllTypes" SET "Reference" = json_set("Reference", '$.TestCharacter', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_datetime()
    {
        await base.Edit_single_property_datetime();

        AssertSql(
"""
@p0='3000-01-01T12:34:56' (Nullable = false) (Size = 19)
@p1='3000-01-01T12:34:56' (Nullable = false) (Size = 19)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateTime', @p0), "Reference" = json_set("Reference", '$.TestDateTime', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_datetimeoffset()
    {
        await base.Edit_single_property_datetimeoffset();

        AssertSql(
"""
@p0='3000-01-01T12:34:56-04:00' (Nullable = false) (Size = 25)
@p1='3000-01-01T12:34:56-04:00' (Nullable = false) (Size = 25)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateTimeOffset', @p0), "Reference" = json_set("Reference", '$.TestDateTimeOffset', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_decimal()
    {
        await base.Edit_single_property_decimal();

        AssertSql(
"""
@p0='-13579.01' (DbType = String)
@p1='-13579.01' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDecimal', json(@p0)), "Reference" = json_set("Reference", '$.TestDecimal', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_double()
    {
        await base.Edit_single_property_double();

        AssertSql(
"""
@p0='-1.23579' (DbType = String)
@p1='-1.23579' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDouble', @p0), "Reference" = json_set("Reference", '$.TestDouble', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_guid()
    {
        await base.Edit_single_property_guid();

        AssertSql(
"""
@p0='12345678-1234-4321-5555-987654321000' (Nullable = false) (Size = 36)
@p1='12345678-1234-4321-5555-987654321000' (Nullable = false) (Size = 36)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestGuid', @p0), "Reference" = json_set("Reference", '$.TestGuid', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_int16()
    {
        await base.Edit_single_property_int16();

        AssertSql(
"""
@p0='-3234' (DbType = String)
@p1='-3234' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt16', @p0), "Reference" = json_set("Reference", '$.TestInt16', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_int32()
    {
        await base.Edit_single_property_int32();

        AssertSql(
"""
@p0='-3234' (DbType = String)
@p1='-3234' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt32', @p0), "Reference" = json_set("Reference", '$.TestInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_int64()
    {
        await base.Edit_single_property_int64();

        AssertSql(
"""
@p0='-3234' (DbType = String)
@p1='-3234' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt64', @p0), "Reference" = json_set("Reference", '$.TestInt64', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_signed_byte()
    {
        await base.Edit_single_property_signed_byte();

        AssertSql(
"""
@p0='-108' (DbType = String)
@p1='-108' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestSignedByte', @p0), "Reference" = json_set("Reference", '$.TestSignedByte', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_single()
    {
        await base.Edit_single_property_single();

        AssertSql(
"""
@p0='-7.234' (DbType = String)
@p1='-7.234' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestSingle', @p0), "Reference" = json_set("Reference", '$.TestSingle', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_timespan()
    {
        await base.Edit_single_property_timespan();

        AssertSql(
"""
@p0='10:01:01.0070000' (Nullable = false) (Size = 16)
@p1='10:01:01.0070000' (Nullable = false) (Size = 16)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestTimeSpan', @p0), "Reference" = json_set("Reference", '$.TestTimeSpan', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_uint16()
    {
        await base.Edit_single_property_uint16();

        AssertSql(
"""
@p0='1534' (DbType = String)
@p1='1534' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt16', @p0), "Reference" = json_set("Reference", '$.TestUnsignedInt16', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_uint32()
    {
        await base.Edit_single_property_uint32();

        AssertSql(
"""
@p0='1237775789' (DbType = String)
@p1='1237775789' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt32', @p0), "Reference" = json_set("Reference", '$.TestUnsignedInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_uint64()
    {
        await base.Edit_single_property_uint64();

        AssertSql(
"""
@p0='1234555555123456789' (DbType = String)
@p1='1234555555123456789' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt64', @p0), "Reference" = json_set("Reference", '$.TestUnsignedInt64', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_int32()
    {
        await base.Edit_single_property_nullable_int32();

        AssertSql(
"""
@p0='122354' (DbType = String)
@p1='64528' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableInt32', @p0), "Reference" = json_set("Reference", '$.TestNullableInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_int32_set_to_null()
    {
        await base.Edit_single_property_nullable_int32_set_to_null();

        AssertSql(
"""
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableInt32', @p0), "Reference" = json_set("Reference", '$.TestNullableInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_enum()
    {
        await base.Edit_single_property_enum();

        AssertSql(
"""
@p0='Three' (Nullable = false) (Size = 5)
@p1='Three' (Nullable = false) (Size = 5)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnum', @p0), "Reference" = json_set("Reference", '$.TestEnum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_enum_with_int_converter()
    {
        await base.Edit_single_property_enum_with_int_converter();

        AssertSql(
"""
@p0='2' (DbType = String)
@p1='2' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnumWithIntConverter', @p0), "Reference" = json_set("Reference", '$.TestEnumWithIntConverter', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum()
    {
        await base.Edit_single_property_nullable_enum();

        AssertSql(
"""
@p0='Three' (Nullable = false) (Size = 5)
@p1='Three' (Nullable = false) (Size = 5)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnum', @p0), "Reference" = json_set("Reference", '$.TestEnum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_set_to_null();

        AssertSql(
"""
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnum', @p0), "Reference" = json_set("Reference", '$.TestNullableEnum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_int_converter()
    {
        await base.Edit_single_property_nullable_enum_with_int_converter();

        AssertSql(
"""
@p0='0' (DbType = String)
@p1='2' (DbType = String)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithIntConverter', @p0), "Reference" = json_set("Reference", '$.TestNullableEnumWithIntConverter', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_int_converter_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_with_int_converter_set_to_null();

        AssertSql(
"""
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithIntConverter', @p0), "Reference" = json_set("Reference", '$.TestNullableEnumWithIntConverter', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls()
    {
        await base.Edit_single_property_nullable_enum_with_converter_that_handles_nulls();

        AssertSql(
"""
@p0='Three' (Nullable = false) (Size = 5)
@p1='One' (Nullable = false) (Size = 3)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithConverterThatHandlesNulls', @p0), "Reference" = json_set("Reference", '$.TestNullableEnumWithConverterThatHandlesNulls', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null();

        AssertSql(
"""
@p0='Null' (Nullable = false) (Size = 4)
@p1='Null' (Nullable = false) (Size = 4)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithConverterThatHandlesNulls', @p0), "Reference" = json_set("Reference", '$.TestNullableEnumWithConverterThatHandlesNulls', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_two_properties_on_same_entity_updates_the_entire_entity()
    {
        await base.Edit_two_properties_on_same_entity_updates_the_entire_entity();

        AssertSql(
"""
@p0='{"TestBoolean":false,"TestByte":25,"TestCharacter":"h","TestDateTime":"2100-11-11T12:34:56","TestDateTimeOffset":"2200-11-11T12:34:56-05:00","TestDecimal":-123450.01,"TestDefaultString":"MyDefaultStringInCollection1","TestDouble":-1.2345,"TestEnum":"One","TestEnumWithIntConverter":1,"TestGuid":"00000000-0000-0000-0000-000000000000","TestInt16":-12,"TestInt32":32,"TestInt64":64,"TestMaxLengthString":"Baz","TestNullableEnum":"One","TestNullableEnumWithConverterThatHandlesNulls":"Two","TestNullableEnumWithIntConverter":2,"TestNullableInt32":90,"TestSignedByte":-18,"TestSingle":-1.4,"TestTimeSpan":"06:05:04.0030000","TestUnsignedInt16":12,"TestUnsignedInt32":12345,"TestUnsignedInt64":1234567867}' (Nullable = false) (Size = 700)
@p1='{"TestBoolean":true,"TestByte":255,"TestCharacter":"a","TestDateTime":"2000-01-01T12:34:56","TestDateTimeOffset":"2000-01-01T12:34:56-08:00","TestDecimal":-1234567890.01,"TestDefaultString":"MyDefaultStringInReference1","TestDouble":-1.23456789,"TestEnum":"One","TestEnumWithIntConverter":1,"TestGuid":"12345678-1234-4321-7777-987654321000","TestInt16":-1234,"TestInt32":32,"TestInt64":64,"TestMaxLengthString":"Foo","TestNullableEnum":"One","TestNullableEnumWithConverterThatHandlesNulls":"Three","TestNullableEnumWithIntConverter":1,"TestNullableInt32":78,"TestSignedByte":-128,"TestSingle":-1.234,"TestTimeSpan":"10:09:08.0070000","TestUnsignedInt16":1234,"TestUnsignedInt32":1234565789,"TestUnsignedInt64":1234567890123456789}' (Nullable = false) (Size = 730)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0]', json(@p0)), "Reference" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_a_scalar_property_and_reference_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_reference_navigation_on_the_same_entity();

        AssertSql(
"""
@p0='{"Date":"2100-01-01T00:00:00","Enum":"One","Fraction":523.532,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (Size = 227)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedReferenceBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_a_scalar_property_and_collection_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_collection_navigation_on_the_same_entity();

        AssertSql(
"""
@p0='{"Date":"2100-01-01T00:00:00","Enum":"One","Fraction":523.532,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"edit"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}' (Nullable = false) (Size = 191)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedReferenceBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity();

        AssertSql(
"""
@p0='{"Date":"2100-01-01T00:00:00","Enum":"One","Fraction":523.532,"NullableEnum":null,"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (Size = 227)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedReferenceBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_int_zero_one()
    {
        await base.Edit_single_property_with_converter_bool_to_int_zero_one();

        AssertSql(
"""
@p0='0' (DbType = String)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = json_set("Reference", '$.BoolConvertedToIntZeroOne', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_string_True_False()
    {
        await base.Edit_single_property_with_converter_bool_to_string_True_False();

        AssertSql(
"""
@p0='True' (Nullable = false) (Size = 4)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = json_set("Reference", '$.BoolConvertedToStringTrueFalse', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_string_Y_N()
    {
        await base.Edit_single_property_with_converter_bool_to_string_Y_N();

        AssertSql(
"""
@p0='N' (Nullable = false) (Size = 1)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = json_set("Reference", '$.BoolConvertedToStringYN', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_int_zero_one_to_bool()
    {
        await base.Edit_single_property_with_converter_int_zero_one_to_bool();

        AssertSql(
"""
@p0='true' (Nullable = false) (Size = 4)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = json_set("Reference", '$.IntZeroOneConvertedToBool', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_string_True_False_to_bool()
    {
        await base.Edit_single_property_with_converter_string_True_False_to_bool();

        AssertSql(
"""
@p0='false' (Nullable = false) (Size = 5)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = json_set("Reference", '$.StringTrueFalseConvertedToBool', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_string_Y_N_to_bool()
    {
        await base.Edit_single_property_with_converter_string_Y_N_to_bool();

        AssertSql(
"""
@p0='true' (Nullable = false) (Size = 4)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = json_set("Reference", '$.StringYNConvertedToBool', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
            //
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
