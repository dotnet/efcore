// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqlServerTest(ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Index_no_evaluatability()
    {
        await base.Index_no_evaluatability();

        AssertSql("""
SELECT [j].[Id], [j].[IntList], [j].[JsonThing]
FROM [JsonEntities] AS [j]
WHERE CAST(JSON_VALUE([j].[IntList], '$[' + CAST([j].[Id] AS nvarchar(max)) + ']') AS int) = 2
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Index_with_captured_variable()
    {
        await base.Index_with_captured_variable();

        AssertSql("""
@__id_0='1'

SELECT [j].[Id], [j].[IntList], [j].[JsonThing]
FROM [JsonEntities] AS [j]
WHERE CAST(JSON_VALUE([j].[IntList], '$[' + CAST(@__id_0 AS nvarchar(max)) + ']') AS int) = 2
""");
    }

    public override async Task JsonScalar()
    {
        await base.JsonScalar();

        AssertSql("""
SELECT [j].[Id], [j].[IntList], [j].[JsonThing]
FROM [JsonEntities] AS [j]
WHERE JSON_VALUE([j].[JsonThing], '$.StringProperty') = N'foo'
""");
    }

    public override async Task Materialize_non_public()
    {
        await base.Materialize_non_public();

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
