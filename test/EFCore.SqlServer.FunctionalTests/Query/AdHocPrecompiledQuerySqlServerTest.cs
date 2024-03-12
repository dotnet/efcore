// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqlServerTest(ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => true; // TODO: Revert this back to false before committing

    public override async Task Materialize_non_public_members()
    {
        await base.Materialize_non_public_members();

        AssertSql(
            """
@p0='10' (Nullable = true)
@p1='9' (Nullable = true)
@p2='8' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NonPublicEntities] ([PrivateAutoProperty], [PrivateProperty], [_privateField])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[PrivateAutoProperty], [n].[PrivateProperty], [n].[_privateField]
FROM [NonPublicEntities] AS [n]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlServerPrecompiledQueryTestHelpers.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder);

        // TODO: Figure out if there's a nice way to continue using the retrying strategy
        var sqlServerOptionsBuilder = new SqlServerDbContextOptionsBuilder(builder);
        sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
        return builder;
    }
}
