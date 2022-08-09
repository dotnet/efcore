// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class EntitySplittingQuerySqlServerTest : EntitySplittingQueryTestBase<EntitySplittingQuerySqlServerFixture>
{
    public EntitySplittingQuerySqlServerTest(EntitySplittingQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Can_query_entity_which_is_split(bool async)
    {
        await base.Can_query_entity_which_is_split(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[SharedValue], [s0].[SplitValue], [s].[Value]
FROM [SplitEntityOneMain] AS [s]
INNER JOIN [SplitEntityOneOther] AS [s0] ON [s].[Id] = [s0].[Id]");
    }

    public override async Task Can_query_entity_which_is_split_selecting_only_main_properties(bool async)
    {
        await base.Can_query_entity_which_is_split_selecting_only_main_properties(async);

        AssertSql(
            @"SELECT [s].[Id], [s].[SharedValue], [s].[Value]
FROM [SplitEntityOneMain] AS [s]");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
