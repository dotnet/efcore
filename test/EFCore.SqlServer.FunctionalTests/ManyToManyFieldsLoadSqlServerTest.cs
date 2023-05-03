// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyFieldsLoadSqlServerTest : ManyToManyFieldsLoadTestBase<
    ManyToManyFieldsLoadSqlServerTest.ManyToManyFieldsLoadSqlServerFixture>
{
    public ManyToManyFieldsLoadSqlServerTest(ManyToManyFieldsLoadSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public override async Task Load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
    {
        await base.Load_collection(state, queryTrackingBehavior, async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneId], [t].[TwoId], [t0].[OneId], [t0].[TwoId], [t0].[Id], [t0].[Name]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e0].[Id], [e0].[CollectionInverseId], [e0].[Name], [e0].[ReferenceInverseId], [j].[OneId], [j].[TwoId]
    FROM [JoinOneToTwo] AS [j]
    INNER JOIN [EntityTwos] AS [e0] ON [j].[TwoId] = [e0].[Id]
) AS [t] ON [e].[Id] = [t].[OneId]
LEFT JOIN (
    SELECT [j0].[OneId], [j0].[TwoId], [e1].[Id], [e1].[Name]
    FROM [JoinOneToTwo] AS [j0]
    INNER JOIN [EntityOnes] AS [e1] ON [j0].[OneId] = [e1].[Id]
    WHERE [e1].[Id] = @__p_0
) AS [t0] ON [t].[Id] = [t0].[TwoId]
WHERE [e].[Id] = @__p_0
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t0].[OneId], [t0].[TwoId]
""");
    }

    public override async Task Load_collection_using_Query_with_Include_for_inverse(bool async)
    {
        await base.Load_collection_using_Query_with_Include_for_inverse(async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t0].[Name]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], [e0].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[OneSkipSharedId]
LEFT JOIN (
    SELECT [e2].[OneSkipSharedId], [e2].[TwoSkipSharedId], [e3].[Id], [e3].[Name]
    FROM [EntityOneEntityTwo] AS [e2]
    INNER JOIN [EntityOnes] AS [e3] ON [e2].[OneSkipSharedId] = [e3].[Id]
    WHERE [e3].[Id] = @__p_0
) AS [t0] ON [t].[Id] = [t0].[TwoSkipSharedId]
WHERE [e].[Id] = @__p_0
ORDER BY [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId]
""");
    }

    public override async Task Load_collection_using_Query_with_Include_for_same_collection(bool async)
    {
        await base.Load_collection_using_Query_with_Include_for_same_collection(async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t0].[Name], [t0].[OneSkipSharedId0], [t0].[TwoSkipSharedId0], [t0].[Id0], [t0].[CollectionInverseId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], [e0].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[OneSkipSharedId]
LEFT JOIN (
    SELECT [e2].[OneSkipSharedId], [e2].[TwoSkipSharedId], [e3].[Id], [e3].[Name], [t1].[OneSkipSharedId] AS [OneSkipSharedId0], [t1].[TwoSkipSharedId] AS [TwoSkipSharedId0], [t1].[Id] AS [Id0], [t1].[CollectionInverseId], [t1].[Name] AS [Name0], [t1].[ReferenceInverseId]
    FROM [EntityOneEntityTwo] AS [e2]
    INNER JOIN [EntityOnes] AS [e3] ON [e2].[OneSkipSharedId] = [e3].[Id]
    LEFT JOIN (
        SELECT [e4].[OneSkipSharedId], [e4].[TwoSkipSharedId], [e5].[Id], [e5].[CollectionInverseId], [e5].[Name], [e5].[ReferenceInverseId]
        FROM [EntityOneEntityTwo] AS [e4]
        INNER JOIN [EntityTwos] AS [e5] ON [e4].[TwoSkipSharedId] = [e5].[Id]
    ) AS [t1] ON [e3].[Id] = [t1].[OneSkipSharedId]
    WHERE [e3].[Id] = @__p_0
) AS [t0] ON [t].[Id] = [t0].[TwoSkipSharedId]
WHERE [e].[Id] = @__p_0
ORDER BY [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t0].[OneSkipSharedId0], [t0].[TwoSkipSharedId0]
""");
    }

    public override async Task Load_collection_using_Query_with_Include(bool async)
    {
        await base.Load_collection_using_Query_with_Include(async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t0].[Name], [t1].[ThreeId], [t1].[TwoId], [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], [e0].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[OneSkipSharedId]
LEFT JOIN (
    SELECT [e2].[OneSkipSharedId], [e2].[TwoSkipSharedId], [e3].[Id], [e3].[Name]
    FROM [EntityOneEntityTwo] AS [e2]
    INNER JOIN [EntityOnes] AS [e3] ON [e2].[OneSkipSharedId] = [e3].[Id]
    WHERE [e3].[Id] = @__p_0
) AS [t0] ON [t].[Id] = [t0].[TwoSkipSharedId]
LEFT JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e4].[Id], [e4].[CollectionInverseId], [e4].[Name], [e4].[ReferenceInverseId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e4] ON [j].[ThreeId] = [e4].[Id]
) AS [t1] ON [t].[Id] = [t1].[TwoId]
WHERE [e].[Id] = @__p_0
ORDER BY [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t1].[ThreeId], [t1].[TwoId]
""");
    }

    public override async Task Load_collection_using_Query_with_filtered_Include(bool async)
    {
        await base.Load_collection_using_Query_with_filtered_Include(async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t0].[Name], [t1].[ThreeId], [t1].[TwoId], [t1].[Id], [t1].[CollectionInverseId], [t1].[Name], [t1].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], [e0].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[OneSkipSharedId]
LEFT JOIN (
    SELECT [e2].[OneSkipSharedId], [e2].[TwoSkipSharedId], [e3].[Id], [e3].[Name]
    FROM [EntityOneEntityTwo] AS [e2]
    INNER JOIN [EntityOnes] AS [e3] ON [e2].[OneSkipSharedId] = [e3].[Id]
    WHERE [e3].[Id] = @__p_0
) AS [t0] ON [t].[Id] = [t0].[TwoSkipSharedId]
LEFT JOIN (
    SELECT [j].[ThreeId], [j].[TwoId], [e4].[Id], [e4].[CollectionInverseId], [e4].[Name], [e4].[ReferenceInverseId]
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e4] ON [j].[ThreeId] = [e4].[Id]
    WHERE [e4].[Id] IN (13, 11)
) AS [t1] ON [t].[Id] = [t1].[TwoId]
WHERE [e].[Id] = @__p_0
ORDER BY [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id], [t1].[ThreeId], [t1].[TwoId]
""");
    }

    public override async Task Load_collection_using_Query_with_filtered_Include_and_projection(bool async)
    {
        await base.Load_collection_using_Query_with_filtered_Include_and_projection(async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[Name], (
    SELECT COUNT(*)
    FROM [EntityOneEntityTwo] AS [e2]
    INNER JOIN [EntityOnes] AS [e3] ON [e2].[OneSkipSharedId] = [e3].[Id]
    WHERE [t].[Id] = [e2].[TwoSkipSharedId]) AS [Count1], (
    SELECT COUNT(*)
    FROM [JoinTwoToThree] AS [j]
    INNER JOIN [EntityThrees] AS [e4] ON [j].[ThreeId] = [e4].[Id]
    WHERE [t].[Id] = [j].[TwoId]) AS [Count3]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[Name], [e0].[OneSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[OneSkipSharedId]
WHERE [e].[Id] = @__p_0
ORDER BY [t].[Id]
""");
    }

    public override async Task Load_collection_using_Query_with_join(bool async)
    {
        await base.Load_collection_using_Query_with_join(async);

        AssertSql(
"""
@__p_0='3'

SELECT [t].[Id], [t].[CollectionInverseId], [t].[Name], [t].[ReferenceInverseId], [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t0].[Id], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id0], [t2].[OneSkipSharedId], [t2].[TwoSkipSharedId], [t2].[Id], [t2].[Name], [t0].[CollectionInverseId], [t0].[Name0], [t0].[ReferenceInverseId]
FROM [EntityOnes] AS [e]
INNER JOIN (
    SELECT [e1].[Id], [e1].[CollectionInverseId], [e1].[Name], [e1].[ReferenceInverseId], [e0].[OneSkipSharedId], [e0].[TwoSkipSharedId]
    FROM [EntityOneEntityTwo] AS [e0]
    INNER JOIN [EntityTwos] AS [e1] ON [e0].[TwoSkipSharedId] = [e1].[Id]
) AS [t] ON [e].[Id] = [t].[OneSkipSharedId]
INNER JOIN (
    SELECT [e2].[Id], [t1].[Id] AS [Id0], [t1].[CollectionInverseId], [t1].[Name] AS [Name0], [t1].[ReferenceInverseId], [t1].[OneSkipSharedId], [t1].[TwoSkipSharedId]
    FROM [EntityOnes] AS [e2]
    INNER JOIN (
        SELECT [e4].[Id], [e4].[CollectionInverseId], [e4].[Name], [e4].[ReferenceInverseId], [e3].[OneSkipSharedId], [e3].[TwoSkipSharedId]
        FROM [EntityOneEntityTwo] AS [e3]
        INNER JOIN [EntityTwos] AS [e4] ON [e3].[TwoSkipSharedId] = [e4].[Id]
    ) AS [t1] ON [e2].[Id] = [t1].[OneSkipSharedId]
) AS [t0] ON [t].[Id] = [t0].[Id0]
LEFT JOIN (
    SELECT [e5].[OneSkipSharedId], [e5].[TwoSkipSharedId], [e6].[Id], [e6].[Name]
    FROM [EntityOneEntityTwo] AS [e5]
    INNER JOIN [EntityOnes] AS [e6] ON [e5].[OneSkipSharedId] = [e6].[Id]
    WHERE [e6].[Id] = @__p_0
) AS [t2] ON [t].[Id] = [t2].[TwoSkipSharedId]
WHERE [e].[Id] = @__p_0
ORDER BY [e].[Id], [t].[OneSkipSharedId], [t].[TwoSkipSharedId], [t].[Id], [t0].[Id], [t0].[OneSkipSharedId], [t0].[TwoSkipSharedId], [t0].[Id0], [t2].[OneSkipSharedId], [t2].[TwoSkipSharedId]
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected override void RecordLog()
        => Sql = Fixture.TestSqlLoggerFactory.Sql;

    private const string FileNewLine = @"
";

    private void AssertSql(string expected)
    {
        try
        {
            Assert.Equal(
                expected,
                Sql,
                ignoreLineEndingDifferences: true);
        }
        catch
        {
            var methodCallLine = Environment.StackTrace.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries)[2][6..];

            var indexMethodEnding = methodCallLine.IndexOf(')') + 1;
            var testName = methodCallLine.Substring(0, indexMethodEnding);
            var parts = methodCallLine[indexMethodEnding..].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var fileName = parts[1][..^5];
            var lineNumber = int.Parse(parts[2]);

            var currentDirectory = Directory.GetCurrentDirectory();
            var logFile = currentDirectory.Substring(
                    0,
                    currentDirectory.LastIndexOf("\\artifacts\\", StringComparison.Ordinal) + 1)
                + "QueryBaseline.txt";

            var testInfo = testName + " : " + lineNumber + FileNewLine;

            var newBaseLine = $@"            AssertSql(
                {"@\"" + Sql.Replace("\"", "\"\"") + "\""});

";

            var contents = testInfo + newBaseLine + FileNewLine + "--------------------" + FileNewLine;

            File.AppendAllText(logFile, contents);

            throw;
        }
    }

    private string Sql { get; set; }

    public class ManyToManyFieldsLoadSqlServerFixture : ManyToManyFieldsLoadFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .Entity<JoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .IndexerProperty<string>("Payload")
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<JoinOneToThreePayloadFull>()
                .Property(e => e.Payload)
                .HasDefaultValue("Generated");
        }
    }
}
