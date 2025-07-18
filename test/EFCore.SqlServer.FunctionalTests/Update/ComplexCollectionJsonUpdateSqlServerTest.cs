// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

// TODO: Requires query support for complex collections mapped to JSON, issue #31252
public abstract class ComplexCollectionJsonUpdateSqlServerTest : ComplexCollectionJsonUpdateTestBase<ComplexCollectionJsonUpdateSqlServerTest.ComplexCollectionJsonUpdateSqlServerFixture>
{
    public ComplexCollectionJsonUpdateSqlServerTest(ComplexCollectionJsonUpdateSqlServerFixture fixture)
        : base(fixture)
        => ClearLog();

    public override async Task Add_element_to_complex_collection_mapped_to_json()
    {
        await base.Add_element_to_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"First Contact","PhoneNumbers":["555-1234","555-5678"]},{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]},{"Name":"New Contact","PhoneNumbers":["555-0000"]}]' (Nullable = false) (Size = 200)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Remove_element_from_complex_collection_mapped_to_json()
    {
        await base.Remove_element_from_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]}]' (Nullable = false) (Size = 65)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Modify_element_in_complex_collection_mapped_to_json()
    {
        await base.Modify_element_in_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"First Contact - Modified","PhoneNumbers":["555-1234","555-5678"]},{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]}]' (Nullable = false) (Size = 141)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Move_elements_in_complex_collection_mapped_to_json()
    {
        await base.Move_elements_in_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]},{"Name":"First Contact","PhoneNumbers":["555-1234","555-5678"]}]' (Nullable = false) (Size = 141)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Change_empty_complex_collection_to_null_mapped_to_json()
    {
        await base.Change_empty_complex_collection_to_null_mapped_to_json();

        AssertSql(
            """
@p0=NULL (Size = 4000)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Change_null_complex_collection_to_empty_mapped_to_json()
    {
        await base.Change_null_complex_collection_to_empty_mapped_to_json();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Complex_collection_with_nested_complex_type_mapped_to_json()
    {
        await base.Complex_collection_with_nested_complex_type_mapped_to_json();

        AssertSql(
            """
@p0='[{"Address":{"City":"Seattle","Country":"USA","PostalCode":"98101","Street":"123 Main St"},"Name":"John Doe","PhoneNumbers":["555-1234","555-5678"]},{"Address":{"City":"Portland","Country":"USA","PostalCode":"97201","Street":"456 Oak Ave"},"Name":"Jane Smith","PhoneNumbers":["555-9876"]}]' (Nullable = false) (Size = 320)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Employees] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Modify_multiple_complex_properties_mapped_to_json()
    {
        await base.Modify_multiple_complex_properties_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Contact 1","PhoneNumbers":["555-1111"]}]' (Nullable = false) (Size = 51)
@p1='[{"Name":"Department A","Budget":50000.00}]' (Nullable = false) (Size = 44)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0, [Departments] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""");
    }

    public override async Task Clear_complex_collection_mapped_to_json()
    {
        await base.Clear_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task Replace_entire_complex_collection_mapped_to_json()
    {
        await base.Replace_entire_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"Replacement Contact 1","PhoneNumbers":["999-1111"]},{"Name":"Replacement Contact 2","PhoneNumbers":["999-2222","999-3333"]}]' (Nullable = false) (Size = 144)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [CompanyWithComplexCollections] SET [Contacts] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class ComplexCollectionJsonUpdateSqlServerFixture : ComplexCollectionJsonUpdateFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Log(SqlServerEventId.JsonTypeExperimental));
    }
}
