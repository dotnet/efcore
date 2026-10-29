// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedComplexPropertiesQuerySqliteTest(NonSharedFixture fixture) : OwnedComplexPropertiesQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Owned_reference()
    {
        await base.Owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_separate_table()
    {
        await base.Owned_reference_separate_table();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_separate_table_with_key_on_complex_property()
    {
        await base.Owned_reference_separate_table_with_key_on_complex_property();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o0"."Name", "o0"."OwnerId", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_with_json_complex_property()
    {
        await base.Owned_reference_with_json_complex_property();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_json()
    {
        await base.Owned_reference_json();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_collection()
    {
        await base.Owned_collection();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
ORDER BY "o"."Id", "o0"."OwnerId", "o0"."Name"
""");
    }

    public override async Task Filter_on_complex_property_of_owned_reference()
    {
        await base.Filter_on_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Owned_Address_Country_Code" = 'DE'
LIMIT 2
""");
    }

    public override async Task Project_complex_property_of_owned_reference()
    {
        await base.Project_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Project_nested_complex_property_of_owned_reference()
    {
        await base.Project_nested_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_in_subquery()
    {
        await base.Owned_reference_in_subquery();

        AssertSql(
            """
@p='5'

SELECT "o0"."Id", "o0"."Owned_Name", "o0"."Owned_Address_City", "o0"."Owned_Address_Street", "o0"."Owned_Address_Country_Code", "o0"."Owned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
    FROM "Owners" AS "o"
    ORDER BY "o"."Id"
    LIMIT @p
) AS "o0"
WHERE "o0"."Id" > 1
ORDER BY "o0"."Id"
""");
    }

    public override async Task Owned_reference_with_json_complex_property_in_subquery()
    {
        await base.Owned_reference_with_json_complex_property_in_subquery();

        AssertSql(
            """
@p='5'

SELECT "o0"."Id", "o0"."Owned_Name", "o0"."c"
FROM (
    SELECT "o"."Id", "o"."Owned_Name", "o"."Owned_Address" AS "c"
    FROM "Owners" AS "o"
    ORDER BY "o"."Id"
    LIMIT @p
) AS "o0"
WHERE "o0"."Id" > 1
ORDER BY "o0"."Id"
""");
    }

    public override async Task Owned_reference_with_distinct()
    {
        await base.Owned_reference_with_distinct();

        AssertSql(
            """
SELECT "o0"."Id", "o0"."Name", "o0"."Owned_Name", "o0"."Owned_Address_City", "o0"."Owned_Address_Street", "o0"."Owned_Address_Country_Code", "o0"."Owned_Address_Country_Name"
FROM (
    SELECT DISTINCT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
    FROM "Owners" AS "o"
) AS "o0"
ORDER BY "o0"."Id"
""");
    }

    public override async Task Owned_reference_in_union()
    {
        await base.Owned_reference_in_union();

        AssertSql(
            """
SELECT "u"."Id", "u"."Name", "o1"."Id", "o1"."Owned_Name", "o1"."Owned_Address_City", "o1"."Owned_Address_Street", "o1"."Owned_Address_Country_Code", "o1"."Owned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name"
    FROM "Owners" AS "o"
    WHERE "o"."Id" = 1
    UNION
    SELECT "o0"."Id", "o0"."Name"
    FROM "Owners" AS "o0"
    WHERE "o0"."Id" = 2
) AS "u"
LEFT JOIN "Owners" AS "o1" ON "u"."Id" = "o1"."Id"
ORDER BY "u"."Id"
""");
    }

    public override async Task Update_complex_property_of_owned_reference()
    {
        await base.Update_complex_property_of_owned_reference();

        AssertSql(
            """
@p2='1'
@p0='Melbourne' (Nullable = false) (Size = 9)
@p1='Oz' (Nullable = false) (Size = 2)

UPDATE "Owners" SET "Owned_Address_City" = @p0, "Owned_Address_Country_Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Owned_reference_on_TPH_owner()
    {
        await base.Owned_reference_on_TPH_owner();

        AssertSql(
            """
SELECT "o"."Id", "o"."Discriminator", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name", "o"."DerivedOwned_Name", "o"."DerivedOwned_Address_City", "o"."DerivedOwned_Address_Street", "o"."DerivedOwned_Address_Country_Code", "o"."DerivedOwned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""",
            //
            """
SELECT "o"."DerivedOwned_Address_City"
FROM "Owners" AS "o"
WHERE "o"."Discriminator" = 'SpecialOwner'
""");
    }

    public override async Task Owned_reference_on_TPT_owner()
    {
        await base.Owned_reference_on_TPT_owner();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", CASE
    WHEN "s"."Id" IS NOT NULL THEN 'SpecialOwner'
END AS "Discriminator", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name", "s"."Id", "s"."DerivedOwned_Name", "s"."DerivedOwned_Address_City", "s"."DerivedOwned_Address_Street", "s"."DerivedOwned_Address_Country_Code", "s"."DerivedOwned_Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "SpecialOwner" AS "s" ON "o"."Id" = "s"."Id"
ORDER BY "o"."Id"
""",
            //
            """
SELECT "s"."DerivedOwned_Address_City"
FROM "Owners" AS "o"
LEFT JOIN "SpecialOwner" AS "s" ON "o"."Id" = "s"."Id"
WHERE "s"."Id" IS NOT NULL
""");
    }

    public override async Task Owned_reference_on_TPC_owner()
    {
        await base.Owned_reference_on_TPC_owner();

        AssertSql(
            """
SELECT "u"."Id", "u"."Name", "u"."Discriminator", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name", "s0"."Id", "s0"."DerivedOwned_Name", "s0"."DerivedOwned_Address_City", "s0"."DerivedOwned_Address_Street", "s0"."DerivedOwned_Address_Country_Code", "s0"."DerivedOwned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name", 'Owner' AS "Discriminator"
    FROM "Owners" AS "o"
    UNION ALL
    SELECT "s"."Id", "s"."Name", 'SpecialOwner' AS "Discriminator"
    FROM "SpecialOwner" AS "s"
) AS "u"
LEFT JOIN "Owned" AS "o0" ON "u"."Id" = "o0"."OwnerId"
LEFT JOIN "SpecialOwner" AS "s0" ON "u"."Id" = "s0"."Id"
ORDER BY "u"."Id"
""",
            //
            """
SELECT "s0"."DerivedOwned_Address_City"
FROM (
    SELECT "s"."Id"
    FROM "SpecialOwner" AS "s"
) AS "u"
LEFT JOIN "SpecialOwner" AS "s0" ON "u"."Id" = "s0"."Id"
""");
    }

    public override async Task Owned_reference_entity_splitting()
    {
        await base.Owned_reference_entity_splitting();

        AssertSql(
            """
SELECT "o"."Id", "s"."Description", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "OwnerOfSplit" AS "o"
LEFT JOIN "SplitOwnedDescription" AS "s" ON "o"."Id" = "s"."OwnerOfSplitId"
ORDER BY "o"."Id"
""");
    }

    public override async Task Optional_owned_reference_with_only_complex_property()
    {
        await base.Optional_owned_reference_with_only_complex_property();

        AssertSql();
    }

    public override async Task Update_complex_property_of_owned_reference_separate_table()
    {
        await base.Update_complex_property_of_owned_reference_separate_table();

        AssertSql(
            """
@p2='1'
@p0='Melbourne' (Nullable = false) (Size = 9)
@p1='Oz' (Nullable = false) (Size = 2)

UPDATE "Owned" SET "Address_City" = @p0, "Address_Country_Name" = @p1
WHERE "OwnerId" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_json_complex_property_of_owned_reference()
    {
        await base.Update_json_complex_property_of_owned_reference();

        AssertSql(
            """
@p0='{"City":"Melbourne","Street":"1 Main St","Country":{"Code":"AU","Name":"Australia"}}' (Nullable = false) (Size = 84)
@p1='1'

UPDATE "Owners" SET "Owned_Address" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_complex_property_of_json_owned_reference()
    {
        await base.Update_complex_property_of_json_owned_reference();

        AssertSql(
            """
@p0='Melbourne' (Nullable = false) (Size = 9)
@p1='1'

UPDATE "Owners" SET "Owned" = json_set("Owned", '$.Address.City', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_complex_properties_of_json_owned_reference()
    {
        await base.Update_complex_properties_of_json_owned_reference();

        AssertSql(
            """
@p0='{"Name":"Owned1","Address":{"City":"Melbourne","Street":"1 Main St","Country":{"Code":"AU","Name":"Oz"}}}' (Nullable = false) (Size = 105)
@p1='1'

UPDATE "Owners" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_complex_property_of_owned_collection_element()
    {
        await base.Update_complex_property_of_owned_collection_element();

        AssertSql(
            """
@p1='Owned1b' (Nullable = false) (Size = 7)
@p2='1'
@p0='Fremantle' (Nullable = false) (Size = 9)

UPDATE "Owned" SET "Address_City" = @p0
WHERE "Name" = @p1 AND "OwnerId" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o1"."Id", "o1"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name"
    FROM "Owners" AS "o"
    WHERE "o"."Id" = 1
    LIMIT 2
) AS "o1"
LEFT JOIN "Owned" AS "o0" ON "o1"."Id" = "o0"."OwnerId"
ORDER BY "o1"."Id", "o0"."OwnerId", "o0"."Name"
""");
    }

    public override async Task Replace_complex_property_of_owned_reference()
    {
        await base.Replace_complex_property_of_owned_reference();

        AssertSql(
            """
@p2='1'
@p0='Brisbane' (Nullable = false) (Size = 8)
@p1='5 Queen St' (Nullable = false) (Size = 10)

UPDATE "Owners" SET "Owned_Address_City" = @p0, "Owned_Address_Street" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Replace_owned_reference_with_complex_property()
    {
        await base.Replace_owned_reference_with_complex_property();

        AssertSql(
            """
@p3='1'
@p0='Owned1New' (Nullable = false) (Size = 9)
@p1='Brisbane' (Nullable = false) (Size = 8)
@p2='5 Queen St' (Nullable = false) (Size = 10)

UPDATE "Owners" SET "Owned_Name" = @p0, "Owned_Address_City" = @p1, "Owned_Address_Street" = @p2
WHERE "Id" = @p3
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Delete_owner_with_owned_complex_property_separate_table()
    {
        await base.Delete_owner_with_owned_complex_property_separate_table();

        AssertSql(
            """
@p0='1'

DELETE FROM "Owned"
WHERE "OwnerId" = @p0
RETURNING 1;
""",
            //
            """
@p0='1'

DELETE FROM "Owners"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
LIMIT 2
""");
    }

    public override async Task Delete_owner_with_owned_complex_property()
    {
        await base.Delete_owner_with_owned_complex_property();

        AssertSql(
            """
@p0='1'

DELETE FROM "Owners"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
LIMIT 2
""");
    }

    public override async Task Value_converter_in_complex_type_on_owned_reference()
    {
        await base.Value_converter_in_complex_type_on_owned_reference();

        AssertSql(
            """
@p0='[{"Code":"A","Id":30,"Label":"Ten","RelatedIds":[]}]' (Nullable = false) (Size = 52)
@p3='1'
@p1='3'
@p2='[5,6,7]' (Nullable = false) (Size = 7)

UPDATE "OwnerWithTag" SET "Owned_Tags" = @p0, "Owned_Tag_Id" = @p1, "Owned_Tag_RelatedIds" = @p2
WHERE "Id" = @p3
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag_Code", "o"."Owned_Tag_Id", "o"."Owned_Tag_Label", "o"."Owned_Tag_RelatedIds", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Owned_Tag_Id" = 3
LIMIT 2
""");
    }

    public override async Task Value_converter_in_complex_type_on_json_owned_reference()
    {
        await base.Value_converter_in_complex_type_on_json_owned_reference();

        AssertSql(
            """
@p0='{"Tag":{"Code":"A","Id":3,"Label":"One","RelatedIds":[5,6,7]},"Tags":[{"Code":"A","Id":30,"Label":"Ten","RelatedIds":[]}]}' (Nullable = false) (Size = 122)
@p1='1'

UPDATE "OwnerWithTag" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Owned" ->> '$.Tag.Id' = 3
LIMIT 2
""");
    }

    public override async Task Value_converter_in_json_complex_type_on_owned_reference()
    {
        await base.Value_converter_in_json_complex_type_on_owned_reference();

        AssertSql(
            """
@p0='{"Code":"A","Id":3,"Label":"One","RelatedIds":[5,6,7]}' (Nullable = false) (Size = 54)
@p1='[{"Code":"A","Id":30,"Label":"Ten","RelatedIds":[]}]' (Nullable = false) (Size = 52)
@p2='1'

UPDATE "OwnerWithTag" SET "Owned_Tag" = @p0, "Owned_Tags" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Owned_Tag" ->> 'Id' = 3
LIMIT 2
""");
    }

    public override async Task Update_complex_property_and_nested_owned_reference_with_same_json_name()
    {
        await base.Update_complex_property_and_nested_owned_reference_with_same_json_name();

        AssertSql(
            """
@p0='{"Address":{"City":"Melbourne","Street":"1 Main St","Country":{"Code":"AU","Name":"Australia"}},"City":{"Name":"Brisbane"}}' (Nullable = false) (Size = 123)
@p1='1'

UPDATE "OwnerOfNested" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerOfNested" AS "o"
LIMIT 2
""");
    }

    public override async Task Update_complex_property_and_nested_owned_reference_with_same_configured_json_name()
    {
        await base.Update_complex_property_and_nested_owned_reference_with_same_configured_json_name();

        AssertSql(
            """
@p0='{"Address":{"Place":"Melbourne","Street":"1 Main St","Country":{"Code":"AU","Name":"Australia"}},"Place":{"Name":"Brisbane"}}' (Nullable = false) (Size = 125)
@p1='1'

UPDATE "OwnerOfNested" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerOfNested" AS "o"
LIMIT 2
""");
    }

    public override async Task Set_nullable_complex_property_of_json_owned_reference()
    {
        await base.Set_nullable_complex_property_of_json_owned_reference();

        AssertSql(
            """
@p0='{"Name":"Owned","Outer":{"Inner":{"Value":"New"}}}' (Nullable = false) (Size = 50)
@p1='1'

UPDATE "OwnerOfNullableComplex" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerOfNullableComplex" AS "o"
LIMIT 2
""");
    }

    public override async Task Clear_nullable_complex_property_of_json_owned_reference()
    {
        await base.Clear_nullable_complex_property_of_json_owned_reference();

        AssertSql(
            """
@p0='{"Name":"Owned","Outer":null}' (Nullable = false) (Size = 29)
@p1='1'

UPDATE "OwnerOfNullableComplex" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerOfNullableComplex" AS "o"
LIMIT 2
""");
    }

    public override async Task Update_property_in_nullable_complex_property_of_json_owned_reference()
    {
        await base.Update_property_in_nullable_complex_property_of_json_owned_reference();

        AssertSql(
            """
@p0='{"Name":"Owned","Outer":{"Inner":{"Value":"New"}}}' (Nullable = false) (Size = 50)
@p1='1'

UPDATE "OwnerOfNullableComplex" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerOfNullableComplex" AS "o"
LIMIT 2
""");
    }

    public override async Task Complex_property_of_owned_reference_after_pushdown()
    {
        await base.Complex_property_of_owned_reference_after_pushdown();

        AssertSql(
            """
@p='5'

SELECT "o0"."Id", "o0"."Name", "o0"."Owned_Name", "o0"."Owned_Address_City", "o0"."Owned_Address_Street", "o0"."Owned_Address_Country_Code", "o0"."Owned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
    FROM "Owners" AS "o"
    WHERE "o"."Owned_Name" <> 'X'
    ORDER BY "o"."Id"
    LIMIT @p
) AS "o0"
WHERE "o0"."Owned_Address_City" = 'Berlin'
ORDER BY "o0"."Id"
""");
    }

    public override async Task Filter_on_json_complex_property_of_owned_reference_in_left_joined_subquery()
    {
        await base.Filter_on_json_complex_property_of_owned_reference_in_left_joined_subquery();

        AssertSql(
            """
@p='5'

SELECT "o"."Id"
FROM "Owners" AS "o"
LEFT JOIN (
    SELECT "o0"."Id", "o0"."Owned_Address" AS "c"
    FROM "Owners" AS "o0"
    ORDER BY "o0"."Id"
    LIMIT @p
) AS "o1" ON "o"."Id" + 1 = "o1"."Id"
WHERE "o1"."c" ->> 'City' <> 'Berlin' OR "o1"."c" ->> 'City' IS NULL
""");
    }

    public override async Task Nested_complex_property_of_optional_owned_reference_after_pushdown()
    {
        await base.Nested_complex_property_of_optional_owned_reference_after_pushdown();

        AssertSql(
            """
@p='5'

SELECT "o0"."Holder_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Holder_Address_Country_Name"
    FROM "OwnerOfOptional" AS "o"
    ORDER BY "o"."Id"
    LIMIT @p
) AS "o0"
WHERE "o0"."Id" > 0
ORDER BY "o0"."Id"
""",
            //
            """
@p='5'

SELECT "o1"."Holder_Address_Country_Code"
FROM (
    SELECT DISTINCT "o0"."Id", "o0"."Holder_Note", "o0"."Holder_Address_City", "o0"."Holder_Address_Street", "o0"."Holder_Address_Country_Code", "o0"."Holder_Address_Country_Name"
    FROM (
        SELECT "o"."Id", "o"."Holder_Note", "o"."Holder_Address_City", "o"."Holder_Address_Street", "o"."Holder_Address_Country_Code", "o"."Holder_Address_Country_Name"
        FROM "OwnerOfOptional" AS "o"
        ORDER BY "o"."Id"
        LIMIT @p
    ) AS "o0"
    WHERE "o0"."Holder_Address_Country_Code" <> 'DE' OR "o0"."Holder_Address_Country_Code" IS NULL
) AS "o1"
ORDER BY "o1"."Id"
""");
    }

    public override async Task Remove_optional_owned_reference_with_json_complex_property()
    {
        await base.Remove_optional_owned_reference_with_json_complex_property();

        AssertSql(
            """
@p0=NULL
@p2='1'
@p1=NULL

UPDATE "OwnerOfOptional" SET "Holder_Address" = @p0, "Holder_Note" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p0='3'

INSERT INTO "OwnerOfOptional" ("Id")
VALUES (@p0);
""",
            //
            """
SELECT "o"."Holder_Address"
FROM "OwnerOfOptional" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task ExecuteUpdate_complex_property_of_owned_reference()
    {
        await base.ExecuteUpdate_complex_property_of_owned_reference();

        AssertSql(
            """
@p='Melbourne' (Size = 9)

UPDATE "Owners" AS "o"
SET "Owned_Address_City" = @p
WHERE "o"."Id" = 1
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task ExecuteUpdate_complex_property_of_json_owned_reference()
    {
        await base.ExecuteUpdate_complex_property_of_json_owned_reference();

        AssertSql();
    }

    public override async Task Json_complex_properties_on_same_owned_type_under_two_navigations_throws()
    {
        await base.Json_complex_properties_on_same_owned_type_under_two_navigations_throws();

        AssertSql();
    }

    public override async Task Update_scalar_and_complex_collection_of_json_owned_reference()
    {
        await base.Update_scalar_and_complex_collection_of_json_owned_reference();

        AssertSql(
            """
@p0='{"Tag":{"Code":"A","Id":1,"Label":"Uno","RelatedIds":"5,6"},"Tags":[{"Code":"A","Id":10,"Label":"Ten","RelatedIds":""},{"Code":"E","Id":11,"Label":"Eleven","RelatedIds":""}]}' (Nullable = false) (Size = 174)
@p1='1'

UPDATE "OwnerWithTag" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Optional_owned_reference_with_null_scalars_and_complex_property()
    {
        await base.Optional_owned_reference_with_null_scalars_and_complex_property();

        AssertSql();
    }

    public override async Task Value_comparer_in_complex_type_on_owned_reference()
    {
        await base.Value_comparer_in_complex_type_on_owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Owned_Tag_Code", "o"."Owned_Tag_Id", "o"."Owned_Tag_Label", "o"."Owned_Tag_RelatedIds", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""",
            //
            """
@p1='1'
@p0='5,6,7' (Nullable = false) (Size = 5)

UPDATE "OwnerWithTag" SET "Owned_Tag_RelatedIds" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag_Code", "o"."Owned_Tag_Id", "o"."Owned_Tag_Label", "o"."Owned_Tag_RelatedIds", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Value_comparer_in_complex_type_on_json_owned_reference()
    {
        await base.Value_comparer_in_complex_type_on_json_owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""",
            //
            """
@p0='5,6,7' (Nullable = false) (Size = 5)
@p1='1'

UPDATE "OwnerWithTag" SET "Owned" = json_set("Owned", '$.Tag.RelatedIds', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Value_comparer_in_json_complex_type_on_owned_reference()
    {
        await base.Value_comparer_in_json_complex_type_on_owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Owned_Tag", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""",
            //
            """
@p0='{"Code":"A","Id":1,"Label":"One","RelatedIds":"5,6,7"}' (Nullable = false) (Size = 54)
@p1='1'

UPDATE "OwnerWithTag" SET "Owned_Tag" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Json_value_reader_writer_in_complex_type_on_json_owned_reference()
    {
        await base.Json_value_reader_writer_in_complex_type_on_json_owned_reference();

        AssertSql(
            """
@p0='C' (Nullable = false) (Size = 1)
@p1='1'

UPDATE "OwnerWithTag" SET "Owned" = json_set("Owned", '$.Tag.Code', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Json_value_reader_writer_in_json_complex_type_on_owned_reference()
    {
        await base.Json_value_reader_writer_in_json_complex_type_on_owned_reference();

        AssertSql(
            """
@p0='{"Code":"#C","Id":1,"Label":"One","RelatedIds":"5,6"}' (Nullable = false) (Size = 53)
@p1='1'

UPDATE "OwnerWithTag" SET "Owned_Tag" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag", "o"."Owned_Tags"
FROM "OwnerWithTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }
}
