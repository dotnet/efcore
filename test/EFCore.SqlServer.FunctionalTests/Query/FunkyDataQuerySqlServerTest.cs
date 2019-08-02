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

            AssertSql(
                @"SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool], [f0].[Id], [f0].[FirstName], [f0].[LastName], [f0].[NullableBool]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE (CASE
    WHEN (([f0].[LastName] = N'') AND [f0].[LastName] IS NOT NULL) OR ([f].[FirstName] IS NOT NULL AND ([f0].[LastName] IS NOT NULL AND (((RIGHT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName]) AND (RIGHT([f].[FirstName], LEN([f0].[LastName])) IS NOT NULL AND [f0].[LastName] IS NOT NULL)) OR (RIGHT([f].[FirstName], LEN([f0].[LastName])) IS NULL AND [f0].[LastName] IS NULL)))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = [f].[NullableBool]) AND [f].[NullableBool] IS NOT NULL");
        }

        public override void String_ends_with_not_equals_nullable_column()
        {
            base.String_ends_with_not_equals_nullable_column();

            AssertSql(
                @"SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool], [f0].[Id], [f0].[FirstName], [f0].[LastName], [f0].[NullableBool]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE (CASE
    WHEN (([f0].[LastName] = N'') AND [f0].[LastName] IS NOT NULL) OR ([f].[FirstName] IS NOT NULL AND ([f0].[LastName] IS NOT NULL AND (((RIGHT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName]) AND (RIGHT([f].[FirstName], LEN([f0].[LastName])) IS NOT NULL AND [f0].[LastName] IS NOT NULL)) OR (RIGHT([f].[FirstName], LEN([f0].[LastName])) IS NULL AND [f0].[LastName] IS NULL)))) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> [f].[NullableBool]) OR [f].[NullableBool] IS NULL");
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

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
