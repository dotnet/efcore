namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQuerySqliteTest(NonSharedFixture fixture)
    : OwnedEntityQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory => SqliteTestStoreFactory.Instance;

    public override async Task Owned_reference_projects_scalar(bool async)
    {
        await base.Owned_reference_projects_scalar(async);
        AssertSql(
            """
SELECT "o"."Details_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }
}