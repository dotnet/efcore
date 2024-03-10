// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

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
@p0='[{"Date":"2101-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"10.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"10.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (Size = 795)
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
@p0='[{"Date":"2221-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"221.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2222-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"222.1","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (Size = 779)
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
@p0='[{"Name":"e1_c1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"11.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"11.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"11.0","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"12.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"12.2","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"12.0","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Names":null,"Number":142,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}]' (Nullable = false) (Size = 2282)
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

    public override async Task Add_element_to_json_collection_root_null_navigations()
    {
        await base.Add_element_to_json_collection_root_null_navigations();

        AssertSql(
            """
@p0='[{"Name":"e1_c1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"11.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"11.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"11.0","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"12.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"12.2","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"12.0","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Names":null,"Number":142,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}}]' (Nullable = false) (Size = 2205)
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
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (Size = 355)
@p1='[]' (Nullable = false) (Size = 2)
@p2='2'
@p3=NULL (DbType = Int32)
@p4='NewEntity' (Size = 9)

INSERT INTO "JsonEntitiesBasic" ("OwnedReferenceRoot", "OwnedCollectionRoot", "Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
            //
            """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
""");
    }

    public override async Task Add_entity_with_json_null_navigations()
    {
        await base.Add_entity_with_json_null_navigations();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":null}}' (Nullable = false) (Size = 333)
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
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10 00:00:00","Enum":-3,"Enums":null,"Fraction":"42.42","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (Size = 355)
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
@p0=NULL (Nullable = false)
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
@p0=NULL (Nullable = false)
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
@p0='2111-11-11 00:00:00' (Nullable = false) (Size = 19)
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
@p0='[{"Date":"2111-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"11.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"...and another"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"11.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"yet another change"},{"SomethingSomething":"and another"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}]' (Nullable = false) (Size = 565)
@p1='{"Name":"edit","Names":["e1_r1","e1_r2"],"Number":10,"Numbers":[-2147483648,-1,0,1,2147483647],"OwnedCollectionBranch":[{"Date":"2101-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"10.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"10.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}}],"OwnedReferenceBranch":{"Date":"2111-11-11 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"10.0","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}}' (Nullable = false) (Size = 966)
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
@p0='[{"Date":"2101-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"4321.3","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"10.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2222-11-11 00:00:00","Enum":-3,"Enums":null,"Fraction":"45.32","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":{"SomethingSomething":"cc"}}]' (Nullable = false) (Size = 741)
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
@p0='[{"Name":"edit1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"11.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"11.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"11.0","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"edit2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01 00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":"12.1","NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"12.2","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"12.0","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}}]' (Nullable = false) (Size = 1925)
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
@p0='{"Date":"2102-01-01 00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":"10.2","NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"edit1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit2"}}' (Nullable = false) (Size = 264)
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
@p0='2'
@p1='2'
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
@p0='1024'
@p1='999'
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
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='14'
@p1='25'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestByte', @p0), "Reference" = json_set("Reference", '$.TestByte', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='3000-01-01 12:34:56' (Nullable = false) (Size = 19)
@p1='3000-01-01 12:34:56' (Nullable = false) (Size = 19)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateTime', @p0), "Reference" = json_set("Reference", '$.TestDateTime', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='3000-01-01 12:34:56-04:00' (Nullable = false) (Size = 25)
@p1='3000-01-01 12:34:56-04:00' (Nullable = false) (Size = 25)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateTimeOffset', @p0), "Reference" = json_set("Reference", '$.TestDateTimeOffset', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-13579.01'
@p1='-13579.01'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDecimal', @p0), "Reference" = json_set("Reference", '$.TestDecimal', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-1.23579'
@p1='-1.23579'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDouble', @p0), "Reference" = json_set("Reference", '$.TestDouble', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-3234'
@p1='-3234'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt16', @p0), "Reference" = json_set("Reference", '$.TestInt16', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-3234'
@p1='-3234'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt32', @p0), "Reference" = json_set("Reference", '$.TestInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-3234'
@p1='-3234'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt64', @p0), "Reference" = json_set("Reference", '$.TestInt64', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-108'
@p1='-108'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestSignedByte', @p0), "Reference" = json_set("Reference", '$.TestSignedByte', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-7.234'
@p1='-7.234'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestSingle', @p0), "Reference" = json_set("Reference", '$.TestSingle', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='10:01:01.007' (Nullable = false) (Size = 12)
@p1='10:01:01.007' (Nullable = false) (Size = 12)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestTimeSpan', @p0), "Reference" = json_set("Reference", '$.TestTimeSpan', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='1534'
@p1='1534'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt16', @p0), "Reference" = json_set("Reference", '$.TestUnsignedInt16', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='1237775789'
@p1='1237775789'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt32', @p0), "Reference" = json_set("Reference", '$.TestUnsignedInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='1234555555123456789'
@p1='1234555555123456789'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt64', @p0), "Reference" = json_set("Reference", '$.TestUnsignedInt64', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='122354'
@p1='64528'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableInt32', @p0), "Reference" = json_set("Reference", '$.TestNullableInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0=NULL (Nullable = false) (DbType = Int32)
@p1=NULL (Nullable = false) (DbType = Int32)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableInt32', @p0), "Reference" = json_set("Reference", '$.TestNullableInt32', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-3'
@p1='-3'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnum', @p0), "Reference" = json_set("Reference", '$.TestEnum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-3'
@p1='-3'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnumWithIntConverter', @p0), "Reference" = json_set("Reference", '$.TestEnumWithIntConverter', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-3'
@p1='-3'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnum', @p0), "Reference" = json_set("Reference", '$.TestEnum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0=NULL (Nullable = false) (DbType = Int32)
@p1=NULL (Nullable = false) (DbType = Int32)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnum', @p0), "Reference" = json_set("Reference", '$.TestNullableEnum', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='-1'
@p1='-3'
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithIntConverter', @p0), "Reference" = json_set("Reference", '$.TestNullableEnumWithIntConverter', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0=NULL (Nullable = false) (DbType = Int32)
@p1=NULL (Nullable = false) (DbType = Int32)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithIntConverter', @p0), "Reference" = json_set("Reference", '$.TestNullableEnumWithIntConverter', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='{"TestBoolean":false,"TestBooleanCollection":[true,false],"TestByte":25,"TestByteCollection":null,"TestCharacter":"h","TestCharacterCollection":["A","B","\u0022"],"TestDateOnly":"2323-04-03","TestDateOnlyCollection":["3234-01-23","4331-01-21"],"TestDateTime":"2100-11-11 12:34:56","TestDateTimeCollection":["2000-01-01 12:34:56","3000-01-01 12:34:56"],"TestDateTimeOffset":"2200-11-11 12:34:56-05:00","TestDateTimeOffsetCollection":["2000-01-01 12:34:56-08:00"],"TestDecimal":"-123450.01","TestDecimalCollection":["-1234567890.01"],"TestDefaultString":"MyDefaultStringInCollection1","TestDefaultStringCollection":["S1","\u0022S2\u0022","S3"],"TestDouble":-1.2345,"TestDoubleCollection":[-1.23456789,1.23456789,0],"TestEnum":-1,"TestEnumCollection":[-1,-3,-7],"TestEnumWithIntConverter":2,"TestEnumWithIntConverterCollection":[-1,-3,-7],"TestGuid":"00000000-0000-0000-0000-000000000000","TestGuidCollection":["12345678-1234-4321-7777-987654321000"],"TestInt16":-12,"TestInt16Collection":[-32768,0,32767],"TestInt32":32,"TestInt32Collection":[-2147483648,0,2147483647],"TestInt64":64,"TestInt64Collection":[-9223372036854775808,0,9223372036854775807],"TestMaxLengthString":"Baz","TestMaxLengthStringCollection":["S1","S2","S3"],"TestNullableEnum":-1,"TestNullableEnumCollection":[-1,null,-3,-7],"TestNullableEnumWithConverterThatHandlesNulls":"Two","TestNullableEnumWithConverterThatHandlesNullsCollection":[-1,null,-7],"TestNullableEnumWithIntConverter":-3,"TestNullableEnumWithIntConverterCollection":[-1,null,-3,-7],"TestNullableInt32":90,"TestNullableInt32Collection":[null,-2147483648,0,null,2147483647,null],"TestSignedByte":-18,"TestSignedByteCollection":[-128,0,127],"TestSingle":-1.4,"TestSingleCollection":[-1.234,0,-1.234],"TestTimeOnly":"05:07:08.0000000","TestTimeOnlyCollection":["13:42:23.0000000","07:17:25.0000000"],"TestTimeSpan":"6:05:04.003","TestTimeSpanCollection":["10:09:08.007","-9:50:51.993"],"TestUnsignedInt16":12,"TestUnsignedInt16Collection":[0,0,65535],"TestUnsignedInt32":12345,"TestUnsignedInt32Collection":[0,0,4294967295],"TestUnsignedInt64":1234567867,"TestUnsignedInt64Collection":[0,0,18446744073709551615]}' (Nullable = false) (Size = 2143)
@p1='{"TestBoolean":true,"TestBooleanCollection":[true,false],"TestByte":255,"TestByteCollection":null,"TestCharacter":"a","TestCharacterCollection":["A","B","\u0022"],"TestDateOnly":"2023-10-10","TestDateOnlyCollection":["1234-01-23","4321-01-21"],"TestDateTime":"2000-01-01 12:34:56","TestDateTimeCollection":["2000-01-01 12:34:56","3000-01-01 12:34:56"],"TestDateTimeOffset":"2000-01-01 12:34:56-08:00","TestDateTimeOffsetCollection":["2000-01-01 12:34:56-08:00"],"TestDecimal":"-1234567890.01","TestDecimalCollection":["-1234567890.01"],"TestDefaultString":"MyDefaultStringInReference1","TestDefaultStringCollection":["S1","\u0022S2\u0022","S3"],"TestDouble":-1.23456789,"TestDoubleCollection":[-1.23456789,1.23456789,0],"TestEnum":-1,"TestEnumCollection":[-1,-3,-7],"TestEnumWithIntConverter":2,"TestEnumWithIntConverterCollection":[-1,-3,-7],"TestGuid":"12345678-1234-4321-7777-987654321000","TestGuidCollection":["12345678-1234-4321-7777-987654321000"],"TestInt16":-1234,"TestInt16Collection":[-32768,0,32767],"TestInt32":32,"TestInt32Collection":[-2147483648,0,2147483647],"TestInt64":64,"TestInt64Collection":[-9223372036854775808,0,9223372036854775807],"TestMaxLengthString":"Foo","TestMaxLengthStringCollection":["S1","S2","S3"],"TestNullableEnum":-1,"TestNullableEnumCollection":[-1,null,-3,-7],"TestNullableEnumWithConverterThatHandlesNulls":"Three","TestNullableEnumWithConverterThatHandlesNullsCollection":[-1,null,-7],"TestNullableEnumWithIntConverter":2,"TestNullableEnumWithIntConverterCollection":[-1,null,-3,-7],"TestNullableInt32":78,"TestNullableInt32Collection":[null,-2147483648,0,null,2147483647,null],"TestSignedByte":-128,"TestSignedByteCollection":[-128,0,127],"TestSingle":-1.234,"TestSingleCollection":[-1.234,0,-1.234],"TestTimeOnly":"11:12:13.0000000","TestTimeOnlyCollection":["11:42:23.0000000","07:17:27.0000000"],"TestTimeSpan":"10:09:08.007","TestTimeSpanCollection":["10:09:08.007","-9:50:51.993"],"TestUnsignedInt16":1234,"TestUnsignedInt16Collection":[0,0,65535],"TestUnsignedInt32":1234565789,"TestUnsignedInt32Collection":[0,0,4294967295],"TestUnsignedInt64":1234567890123456789,"TestUnsignedInt64Collection":[0,0,18446744073709551615]}' (Nullable = false) (Size = 2173)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0]', json(@p0)), "Reference" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
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
@p0='{"Date":"2100-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"523.532","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (Size = 272)
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
@p0='{"Date":"2100-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"523.532","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"edit"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}' (Nullable = false) (Size = 236)
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
@p0='{"Date":"2100-01-01 00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":"523.532","NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (Size = 272)
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
@p0='0'
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

    public override async Task Edit_single_property_collection_of_numeric()
    {
        await base.Edit_single_property_collection_of_numeric();

        AssertSql(
            """
@p0='[1024,2048]' (Nullable = false) (Size = 11)
@p1='[999,997]' (Nullable = false) (Size = 9)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = json_set("OwnedCollectionRoot", '$[1].Numbers', json(@p0)), "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.Numbers', json(@p1))
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

    public override async Task Edit_single_property_collection_of_bool()
    {
        await base.Edit_single_property_collection_of_bool();

        AssertSql(
            """
@p0='[true,true,true,false]' (Nullable = false) (Size = 22)
@p1='[true,true,false]' (Nullable = false) (Size = 17)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestBooleanCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestBooleanCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_byte()
    {
        await base.Edit_single_property_collection_of_byte();

        AssertSql(
            """
@p0='0E' (Nullable = false) (Size = 2)
@p1='191A' (Nullable = false) (Size = 4)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestByteCollection', @p0), "Reference" = json_set("Reference", '$.TestByteCollection', @p1)
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_char()
    {
        await base.Edit_single_property_collection_of_char();

        AssertSql(
            """
@p0='["A","B","\u0022","\u0000"]' (Nullable = false) (Size = 27)
@p1='["E","F","C","\u00F6","r","E","\u0022","\\"]' (Nullable = false) (Size = 44)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestCharacterCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestCharacterCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_datetime()
    {
        await base.Edit_single_property_collection_of_datetime();

        AssertSql(
            """
@p0='["2000-01-01 12:34:56","3000-01-01 12:34:56","3000-01-01 12:34:56"]' (Nullable = false) (Size = 67)
@p1='["2000-01-01 12:34:56","3000-01-01 12:34:56","3000-01-01 12:34:56"]' (Nullable = false) (Size = 67)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateTimeCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestDateTimeCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_datetimeoffset()
    {
        await base.Edit_single_property_collection_of_datetimeoffset();

        AssertSql(
            """
@p0='["3000-01-01 12:34:56-04:00"]' (Nullable = false) (Size = 29)
@p1='["3000-01-01 12:34:56-04:00"]' (Nullable = false) (Size = 29)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateTimeOffsetCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestDateTimeOffsetCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_decimal()
    {
        await base.Edit_single_property_collection_of_decimal();
        AssertSql(
            """
@p0='["-13579.01"]' (Nullable = false) (Size = 13)
@p1='["-13579.01"]' (Nullable = false) (Size = 13)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDecimalCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestDecimalCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_double()
    {
        await base.Edit_single_property_collection_of_double();

        AssertSql(
            """
@p0='[-1.23456789,1.23456789,0,-1.23579]' (Nullable = false) (Size = 35)
@p1='[-1.23456789,1.23456789,0,-1.23579]' (Nullable = false) (Size = 35)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDoubleCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestDoubleCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_guid()
    {
        await base.Edit_single_property_collection_of_guid();
        AssertSql(
            """
@p0='["12345678-1234-4321-5555-987654321000"]' (Nullable = false) (Size = 40)
@p1='["12345678-1234-4321-5555-987654321000"]' (Nullable = false) (Size = 40)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestGuidCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestGuidCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_int16()
    {
        await base.Edit_single_property_collection_of_int16();
        AssertSql(
            """
@p0='[-3234]' (Nullable = false) (Size = 7)
@p1='[-3234]' (Nullable = false) (Size = 7)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt16Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestInt16Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_int32()
    {
        await base.Edit_single_property_collection_of_int32();
        AssertSql(
            """
@p0='[-3234]' (Nullable = false) (Size = 7)
@p1='[-3234]' (Nullable = false) (Size = 7)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt32Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestInt32Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_int64()
    {
        await base.Edit_single_property_collection_of_int64();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestInt64Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestInt64Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_signed_byte()
    {
        await base.Edit_single_property_collection_of_signed_byte();

        AssertSql(
            """
@p0='[-108]' (Nullable = false) (Size = 6)
@p1='[-108]' (Nullable = false) (Size = 6)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestSignedByteCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestSignedByteCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_single()
    {
        await base.Edit_single_property_collection_of_single();

        AssertSql(
            """
@p0='[-1.234,-1.234]' (Nullable = false) (Size = 15)
@p1='[0,-1.234]' (Nullable = false) (Size = 10)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestSingleCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestSingleCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_timespan()
    {
        await base.Edit_single_property_collection_of_timespan();
        AssertSql(
            """
@p0='["10:09:08.007","10:01:01.007"]' (Nullable = false) (Size = 31)
@p1='["10:01:01.007","-9:50:51.993"]' (Nullable = false) (Size = 31)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestTimeSpanCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestTimeSpanCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_dateonly()
    {
        await base.Edit_single_property_collection_of_dateonly();

        AssertSql(
            """
@p0='["3234-01-23","0001-01-07"]' (Nullable = false) (Size = 27)
@p1='["0001-01-07","4321-01-21"]' (Nullable = false) (Size = 27)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestDateOnlyCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestDateOnlyCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_timeonly()
    {
        await base.Edit_single_property_collection_of_timeonly();

        AssertSql(
            """
@p0='["13:42:23.0000000","01:01:07.0000000"]' (Nullable = false) (Size = 39)
@p1='["01:01:07.0000000","07:17:27.0000000"]' (Nullable = false) (Size = 39)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestTimeOnlyCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestTimeOnlyCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_uint16()
    {
        await base.Edit_single_property_collection_of_uint16();

        AssertSql(
            """
@p0='[1534]' (Nullable = false) (Size = 6)
@p1='[1534]' (Nullable = false) (Size = 6)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt16Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestUnsignedInt16Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_uint32()
    {
        await base.Edit_single_property_collection_of_uint32();

        AssertSql(
            """
@p0='[1237775789]' (Nullable = false) (Size = 12)
@p1='[1237775789]' (Nullable = false) (Size = 12)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt32Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestUnsignedInt32Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_uint64()
    {
        await base.Edit_single_property_collection_of_uint64();

        AssertSql(
            """
@p0='[1234555555123456789]' (Nullable = false) (Size = 21)
@p1='[1234555555123456789]' (Nullable = false) (Size = 21)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestUnsignedInt64Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestUnsignedInt64Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_int32()
    {
        await base.Edit_single_property_collection_of_nullable_int32();

        AssertSql(
            """
@p0='[null,77]' (Nullable = false) (Size = 9)
@p1='[null,-2147483648,0,null,2147483647,null,77,null]' (Nullable = false) (Size = 49)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableInt32Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableInt32Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_int32_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_int32_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableInt32Collection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableInt32Collection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_enum()
    {
        await base.Edit_single_property_collection_of_enum();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4)
@p1='[-3]' (Nullable = false) (Size = 4)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnumCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestEnumCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_enum_with_int_converter()
    {
        await base.Edit_single_property_collection_of_enum_with_int_converter();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4)
@p1='[-3]' (Nullable = false) (Size = 4)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnumWithIntConverterCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestEnumWithIntConverterCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum()
    {
        await base.Edit_single_property_collection_of_nullable_enum();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4)
@p1='[-3]' (Nullable = false) (Size = 4)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestEnumCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestEnumCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableEnumCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_int_converter()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_int_converter();

        AssertSql(
            """
@p0='[-1,null,-7,2]' (Nullable = false) (Size = 14)
@p1='[-1,-3,-7,2]' (Nullable = false) (Size = 12)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithIntConverterCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableEnumWithIntConverterCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithIntConverterCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableEnumWithIntConverterCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4)
@p1='[-1]' (Nullable = false) (Size = 4)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithConverterThatHandlesNullsCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableEnumWithConverterThatHandlesNullsCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null();
        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1=NULL (Nullable = false)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = json_set("Collection", '$[0].TestNullableEnumWithConverterThatHandlesNullsCollection', json(@p0)), "Reference" = json_set("Reference", '$.TestNullableEnumWithConverterThatHandlesNullsCollection', json(@p1))
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Add_and_update_top_level_optional_owned_collection_to_JSON(bool? value)
    {
        await base.Add_and_update_top_level_optional_owned_collection_to_JSON(value);

        switch (value)
        {
            case true:
                AssertSql(
        """
@p0='[{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":null}]' (Nullable = false) (Size = 111)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 9)

INSERT INTO "JsonEntitiesBasic" ("OwnedCollectionRoot", "Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2, @p3);
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""",
                        //
                        """
@p0=NULL (Nullable = false)
@p1='2'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                        //
                        """
select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""");
                break;
            case false:
                AssertSql(
        """
@p0='[]' (Nullable = false) (Size = 2)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 9)

INSERT INTO "JsonEntitiesBasic" ("OwnedCollectionRoot", "Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2, @p3);
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""",
                        //
                        """
@p0='[{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":null}]' (Nullable = false) (Size = 111)
@p1='2'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                        //
                        """
select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""");
                break;
            default:
                AssertSql(
        """
@p0='2'
@p1=NULL (DbType = Int32)
@p2='NewEntity' (Size = 9)

INSERT INTO "JsonEntitiesBasic" ("Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2);
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""",
                        //
                        """
@p0='[]' (Nullable = false) (Size = 2)
@p3='2'
@p1=NULL (DbType = Int32)
@p2='NewEntity' (Size = 9)

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0, "EntityBasicId" = @p1, "Name" = @p2
WHERE "Id" = @p3
RETURNING 1;
""",
                        //
                        """
select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""");
                break;
        }
    }

    public override async Task Add_and_update_nested_optional_owned_collection_to_JSON(bool? value)
    {
        await base.Add_and_update_nested_optional_owned_collection_to_JSON(value);

        switch (value)
        {
            case true:
                AssertSql(
        """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":[{"Date":"0001-01-01 00:00:00","Enum":0,"Enums":null,"Fraction":"0.0","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}],"OwnedReferenceBranch":null}' (Nullable = false) (Size = 270)
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
WHERE "j"."Id" = 2
LIMIT 2
""",
                        //
                        """
@p0=NULL (Nullable = false)
@p1='2'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""");
                break;
            case false:
                AssertSql(
        """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":null}' (Nullable = false) (Size = 107)
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
WHERE "j"."Id" = 2
LIMIT 2
""",
                        //
                        """
@p0='[{"Date":"0001-01-01 00:00:00","Enum":0,"Enums":null,"Fraction":"0.0","NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}]' (Nullable = false) (Size = 165)
@p1='2'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = json_set("OwnedReferenceRoot", '$.OwnedCollectionBranch', json(@p0))
WHERE "Id" = @p1
RETURNING 1;
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""");
                break;
            default:
                AssertSql(
        """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":null}' (Nullable = false) (Size = 109)
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
WHERE "j"."Id" = 2
LIMIT 2
""",
                        //
                        """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":null}' (Nullable = false) (Size = 107)
@p1='2'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
                        //
                        """
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."Id" = 2
LIMIT 2
""");
                break;
        }
    }

    public override async Task Add_and_update_nested_optional_primitive_collection(bool? value)
    {
        await base.Add_and_update_nested_optional_primitive_collection(value);

        string characterCollection = value switch
        {
            true => "[\"A\"]",
            false => "[]",
            _ => "null"
        };

        string parameterSize = value switch
        {
            true => "1541",
            false => "1538",
            _ => "1540"
        };

        string updateParameter = value switch
        {
            true => "NULL (Nullable = false)",
            false => "'[\"Z\"]' (Nullable = false) (Size = 5)",
            _ => "'[]' (Nullable = false) (Size = 2)"
        };

        AssertSql(
            @"@p0='[{""TestBoolean"":false,""TestBooleanCollection"":[],""TestByte"":0,""TestByteCollection"":null,""TestCharacter"":""\u0000"",""TestCharacterCollection"":" + characterCollection + @",""TestDateOnly"":""0001-01-01"",""TestDateOnlyCollection"":[],""TestDateTime"":""0001-01-01 00:00:00"",""TestDateTimeCollection"":[],""TestDateTimeOffset"":""0001-01-01 00:00:00+00:00"",""TestDateTimeOffsetCollection"":[],""TestDecimal"":""0.0"",""TestDecimalCollection"":[],""TestDefaultString"":null,""TestDefaultStringCollection"":[],""TestDouble"":0,""TestDoubleCollection"":[],""TestEnum"":0,""TestEnumCollection"":[],""TestEnumWithIntConverter"":0,""TestEnumWithIntConverterCollection"":[],""TestGuid"":""00000000-0000-0000-0000-000000000000"",""TestGuidCollection"":[],""TestInt16"":0,""TestInt16Collection"":[],""TestInt32"":0,""TestInt32Collection"":[],""TestInt64"":0,""TestInt64Collection"":[],""TestMaxLengthString"":null,""TestMaxLengthStringCollection"":[],""TestNullableEnum"":null,""TestNullableEnumCollection"":[],""TestNullableEnumWithConverterThatHandlesNulls"":null,""TestNullableEnumWithConverterThatHandlesNullsCollection"":[],""TestNullableEnumWithIntConverter"":null,""TestNullableEnumWithIntConverterCollection"":[],""TestNullableInt32"":null,""TestNullableInt32Collection"":[],""TestSignedByte"":0,""TestSignedByteCollection"":[],""TestSingle"":0,""TestSingleCollection"":[],""TestTimeOnly"":""00:00:00.0000000"",""TestTimeOnlyCollection"":[],""TestTimeSpan"":""0:00:00"",""TestTimeSpanCollection"":[],""TestUnsignedInt16"":0,""TestUnsignedInt16Collection"":[],""TestUnsignedInt32"":0,""TestUnsignedInt32Collection"":[],""TestUnsignedInt64"":0,""TestUnsignedInt64Collection"":[]}]' (Nullable = false) (Size = " + parameterSize + @")
@p1='7624'
@p2='[]' (Size = 2)
@p3=NULL (DbType = Binary)
@p4='[]' (Size = 2)
@p5='[]' (Size = 2)
@p6='[]' (Size = 2)
@p7='[]' (Size = 2)
@p8='[]' (Size = 2)
@p9='[]' (Size = 2)
@p10='[]' (Size = 2)
@p11='[]' (Size = 2)
@p12='[]' (Nullable = false) (Size = 2)
@p13='[]' (Size = 2)
@p14='[]' (Size = 2)
@p15='[]' (Size = 2)
@p16='[]' (Size = 2)
@p17='[]' (Size = 2)
@p18='[]' (Size = 2)
@p19='[]' (Size = 2)
@p20='[]' (Size = 2)
@p21='[]' (Size = 2)
@p22='[]' (Size = 2)
@p23='[]' (Size = 2)
@p24='[]' (Size = 2)
@p25='[]' (Size = 2)
@p26='[]' (Size = 2)

INSERT INTO ""JsonEntitiesAllTypes"" (""Collection"", ""Id"", ""TestBooleanCollection"", ""TestByteCollection"", ""TestCharacterCollection"", ""TestDateTimeCollection"", ""TestDateTimeOffsetCollection"", ""TestDecimalCollection"", ""TestDefaultStringCollection"", ""TestDoubleCollection"", ""TestEnumCollection"", ""TestEnumWithIntConverterCollection"", ""TestGuidCollection"", ""TestInt16Collection"", ""TestInt32Collection"", ""TestInt64Collection"", ""TestMaxLengthStringCollection"", ""TestNullableEnumCollection"", ""TestNullableEnumWithConverterThatHandlesNullsCollection"", ""TestNullableEnumWithIntConverterCollection"", ""TestNullableInt32Collection"", ""TestSignedByteCollection"", ""TestSingleCollection"", ""TestTimeSpanCollection"", ""TestUnsignedInt16Collection"", ""TestUnsignedInt32Collection"", ""TestUnsignedInt64Collection"")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21, @p22, @p23, @p24, @p25, @p26);",
                //
                """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 7624
LIMIT 2
""",
//

"@p0=" + updateParameter + @"
@p1='7624'

UPDATE ""JsonEntitiesAllTypes"" SET ""Collection"" = json_set(""Collection"", '$[0].TestCharacterCollection', json(@p0))
WHERE ""Id"" = @p1
RETURNING 1;",
                //
                """
SELECT "j"."Id", "j"."TestBooleanCollection", "j"."TestByteCollection", "j"."TestCharacterCollection", "j"."TestDateTimeCollection", "j"."TestDateTimeOffsetCollection", "j"."TestDecimalCollection", "j"."TestDefaultStringCollection", "j"."TestDoubleCollection", "j"."TestEnumCollection", "j"."TestEnumWithIntConverterCollection", "j"."TestGuidCollection", "j"."TestInt16Collection", "j"."TestInt32Collection", "j"."TestInt64Collection", "j"."TestMaxLengthStringCollection", "j"."TestNullableEnumCollection", "j"."TestNullableEnumWithConverterThatHandlesNullsCollection", "j"."TestNullableEnumWithIntConverterCollection", "j"."TestNullableInt32Collection", "j"."TestSignedByteCollection", "j"."TestSingleCollection", "j"."TestTimeSpanCollection", "j"."TestUnsignedInt16Collection", "j"."TestUnsignedInt32Collection", "j"."TestUnsignedInt64Collection", "j"."Collection", "j"."Reference"
FROM "JsonEntitiesAllTypes" AS "j"
WHERE "j"."Id" = 7624
LIMIT 2
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
