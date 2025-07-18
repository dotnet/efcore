// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

// TODO: Requires query support for complex collections mapped to JSON, issue #31252
public abstract class ComplexCollectionJsonUpdateSqliteTest : ComplexCollectionJsonUpdateTestBase<ComplexCollectionJsonUpdateSqliteTest.ComplexCollectionJsonUpdateSqliteFixture>
{
    public ComplexCollectionJsonUpdateSqliteTest(ComplexCollectionJsonUpdateSqliteFixture fixture)
        : base(fixture)
        => ClearLog();

    public override async Task Add_element_to_complex_collection_mapped_to_json()
    {
        await base.Add_element_to_complex_collection_mapped_to_json();

        AssertSql(
            """
@p0='[{"Name":"First Contact","PhoneNumbers":["555-1234","555-5678"]},{"Name":"Second Contact","PhoneNumbers":["555-9876","555-5432"]},{"Name":"New Contact","PhoneNumbers":["555-0000"]}]' (Size = 200)
@p1='1' (DbType = String)

UPDATE "CompanyWithComplexCollections" SET "Contacts" = @p0
WHERE "Id" = @p1
RETURNING 1;
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class ComplexCollectionJsonUpdateSqliteFixture : ComplexCollectionJsonUpdateFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
