// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FunkyDataQuerySqlServerTest : FunkyDataQueryTestBase<FunkyDataQuerySqlServerTest.FunkyDataQuerySqlServerFixture>
    {
        public FunkyDataQuerySqlServerTest(FunkyDataQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void String_ends_with_equals_nullable_column()
        {
            base.String_ends_with_equals_nullable_column();

            Assert.Equal(
                @"SELECT [c].[Id], [c].[FirstName], [c].[LastName], [c].[NullableBool], [c2].[Id], [c2].[FirstName], [c2].[LastName], [c2].[NullableBool]
FROM [FunkyCustomers] AS [c]
CROSS JOIN [FunkyCustomers] AS [c2]
WHERE CASE
    WHEN (RIGHT([c].[FirstName], LEN([c2].[LastName])) = [c2].[LastName]) OR ([c2].[LastName] = N'')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = [c].[NullableBool]",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_ends_with_not_equals_nullable_column()
        {
            base.String_ends_with_not_equals_nullable_column();

            Assert.Equal(
                @"SELECT [c].[Id], [c].[FirstName], [c].[LastName], [c].[NullableBool], [c2].[Id], [c2].[FirstName], [c2].[LastName], [c2].[NullableBool]
FROM [FunkyCustomers] AS [c]
CROSS JOIN [FunkyCustomers] AS [c2]
WHERE (CASE
    WHEN (RIGHT([c].[FirstName], LEN([c2].[LastName])) = [c2].[LastName]) OR ([c2].[LastName] = N'')
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> [c].[NullableBool]) OR [c].[NullableBool] IS NULL",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class FunkyDataQuerySqlServerFixture : FunkyDataQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            public override FunkyDataContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }
        }
    }
}
