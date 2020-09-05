// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyLoadSqlServerTest : ManyToManyLoadTestBase<ManyToManyLoadSqlServerTest.ManyToManyLoadSqlServerFixture>
    {
        public ManyToManyLoadSqlServerTest(ManyToManyLoadSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override async Task Load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
        {
            await base.Load_collection(state, queryTrackingBehavior, async);

            AssertSql(
                @"@__p_0='3'

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
ORDER BY [e].[Id], [t].[OneId], [t].[TwoId], [t].[Id], [t0].[OneId], [t0].[TwoId], [t0].[Id]");
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
                    StringSplitOptions.RemoveEmptyEntries)[2].Substring(6);

                var testName = methodCallLine.Substring(0, methodCallLine.IndexOf(')') + 1);
                var lineIndex = methodCallLine.LastIndexOf("line", StringComparison.Ordinal);
                var lineNumber = lineIndex > 0 ? methodCallLine.Substring(lineIndex) : "";

                var currentDirectory = Directory.GetCurrentDirectory();
                var logFile = currentDirectory.Substring(
                        0,
                        currentDirectory.LastIndexOf("\\artifacts\\", StringComparison.Ordinal) + 1)
                    + "QueryBaseline.txt";

                var testInfo = testName + " : " + lineNumber + FileNewLine;

                var newBaseLine = $@"            AssertSql(
                {"@\"" + Sql.Replace("\"", "\"\"") + "\""});

";

                var contents = testInfo + newBaseLine + FileNewLine + FileNewLine;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }

        private string Sql { get; set; }

        public class ManyToManyLoadSqlServerFixture : ManyToManyLoadFixtureBase
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
}
