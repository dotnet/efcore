// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NonSharedModelBulkUpdatesSqliteTest : NonSharedModelBulkUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_predicate_based_on_optional_navigation(bool async)
    {
        await base.Delete_predicate_based_on_optional_navigation(async);

        AssertSql(
            @"DELETE FROM ""Posts"" AS ""p""
WHERE EXISTS (
    SELECT 1
    FROM ""Posts"" AS ""p0""
    LEFT JOIN ""Blogs"" AS ""b"" ON ""p0"".""BlogId"" = ""b"".""Id""
    WHERE ""b"".""Title"" IS NOT NULL AND (""b"".""Title"" LIKE 'Arthur%') AND ""p0"".""Id"" = ""p"".""Id"")");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
