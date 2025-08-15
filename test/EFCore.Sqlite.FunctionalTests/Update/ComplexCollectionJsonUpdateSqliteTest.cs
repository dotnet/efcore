// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class ComplexCollectionJsonUpdateSqliteTest : ComplexCollectionJsonUpdateTestBase<
    ComplexCollectionJsonUpdateSqliteTest.ComplexCollectionJsonUpdateSqliteFixture>
{
    public ComplexCollectionJsonUpdateSqliteTest(ComplexCollectionJsonUpdateSqliteFixture fixture)
        : base(fixture)
        => ClearLog();

    public override async Task Add_element_to_complex_collection_mapped_to_json()
    {
        await base.Add_element_to_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"First Contact","PhoneNumbers":["555-1234","555-5678"]},{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]},{"Name":"New Contact","PhoneNumbers":["555-0000"]}]' (Nullable = false) (Size = 181)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Remove_element_from_complex_collection_mapped_to_json()
    {
        await base.Remove_element_from_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]}]' (Nullable = false) (Size = 66)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Modify_element_in_complex_collection_mapped_to_json()
    {
        await base.Modify_element_in_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"First Contact - Modified","PhoneNumbers":["555-1234","555-5678"]},{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]}]' (Nullable = false) (Size = 141)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Move_elements_in_complex_collection_mapped_to_json()
    {
        await base.Move_elements_in_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]},{"Name":"First Contact","PhoneNumbers":["555-1234","555-5678"]}]' (Nullable = false) (Size = 130)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Change_complex_collection_mapped_to_json_to_null_and_to_empty()
    {
        await base.Change_complex_collection_mapped_to_json_to_null_and_to_empty();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Complex_collection_with_nested_complex_type_mapped_to_json()
    {
        await base.Complex_collection_with_nested_complex_type_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"John Doe","PhoneNumbers":["555-1234","555-5678"],"Address":{"City":"Seattle","Country":"USA","PostalCode":"98101","Street":"123 Main St"}},{"Name":"Jane Smith","PhoneNumbers":["555-9876"],"Address":{"City":"Portland","Country":"USA","PostalCode":"97201","Street":"456 Oak Ave"}}]' (Nullable = false) (Size = 289)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Modify_multiple_complex_properties_mapped_to_json()
    {
        await base.Modify_multiple_complex_properties_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Contact 1","PhoneNumbers":["555-1111"]}]' (Nullable = false) (Size = 50)
@p1='{"Budget":"50000.0","Name":"Department A"}' (Nullable = false) (Size = 42)
@p2='1'

UPDATE "Companies" SET "Contacts" = @p0, "Department" = @p1
WHERE "Id" = @p2
RETURNING 1;
""");
    }

    public override async Task Clear_complex_collection_mapped_to_json()
    {
        await base.Clear_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Replace_entire_complex_collection_mapped_to_json()
    {
        await base.Replace_entire_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Replacement Contact 1","PhoneNumbers":["999-1111"]},{"Name":"Replacement Contact 2","PhoneNumbers":["999-2222","999-3333"]}]' (Nullable = false) (Size = 134)
@p1='1'

UPDATE "Companies" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Add_element_to_nested_complex_collection_mapped_to_json()
    {
        await base.Add_element_to_nested_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Initial Employee","PhoneNumbers":["555-0001","555-9999"],"Address":{"City":"Initial City","Country":"USA","PostalCode":"00001","Street":"100 First St"}}]' (Nullable = false) (Size = 163)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Modify_nested_complex_property_in_complex_collection_mapped_to_json()
    {
        await base.Modify_nested_complex_property_in_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Initial Employee","PhoneNumbers":["555-0001"],"Address":{"City":"Modified City","Country":"USA","PostalCode":"99999","Street":"100 First St"}}]' (Nullable = false) (Size = 153)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Set_complex_collection_to_null_mapped_to_json()
    {
        await base.Set_complex_collection_to_null_mapped_to_json();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Set_null_complex_collection_to_non_empty_mapped_to_json()
    {
        await base.Set_null_complex_collection_to_non_empty_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"New Employee","PhoneNumbers":["555-1111"],"Address":{"City":"New City","Country":"USA","PostalCode":"12345","Street":"123 New St"}}]' (Nullable = false) (Size = 142)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Replace_complex_collection_element_mapped_to_json()
    {
        await base.Replace_complex_collection_element_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Replacement Employee","PhoneNumbers":["555-7777","555-8888"],"Address":{"City":"Replace City","Country":"Canada","PostalCode":"54321","Street":"789 Replace St"}}]' (Nullable = false) (Size = 172)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Complex_collection_with_empty_nested_collections_mapped_to_json()
    {
        await base.Complex_collection_with_empty_nested_collections_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Initial Employee","PhoneNumbers":["555-0001"],"Address":{"City":"Initial City","Country":"USA","PostalCode":"00001","Street":"100 First St"}},{"Name":"Employee No Phone","PhoneNumbers":[],"Address":{"City":"Quiet City","Country":"USA","PostalCode":"00000","Street":"456 No Phone St"}}]' (Nullable = false) (Size = 295)
@p1='1'

UPDATE "Companies" SET "Employees" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Set_complex_property_mapped_to_json_to_null()
    {
        await base.Set_complex_property_mapped_to_json_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1='1'

UPDATE "Companies" SET "Department" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Set_null_complex_property_to_non_null_mapped_to_json()
    {
        await base.Set_null_complex_property_to_non_null_mapped_to_json();

        AssertSql(
            """
@p0='{"Budget":"25000.0","Name":"New Department"}' (Nullable = false) (Size = 44)
@p1='1'

UPDATE "Companies" SET "Department" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public override async Task Replace_complex_property_mapped_to_json()
    {
        await base.Replace_complex_property_mapped_to_json();

        AssertSql(
            """
@p0='{"Budget":"99999.99","Name":"Replacement Department"}' (Nullable = false) (Size = 53)
@p1='1'

UPDATE "Companies" SET "Department" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    public class ComplexCollectionJsonUpdateSqliteFixture : ComplexCollectionJsonUpdateFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
