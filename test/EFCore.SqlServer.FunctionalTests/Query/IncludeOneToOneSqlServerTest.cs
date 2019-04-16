// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneSqlServerTest : IncludeOneToOneTestBase<IncludeOneToOneSqlServerTest.OneToOneQuerySqlServerFixture>
    {
        public IncludeOneToOneSqlServerTest(OneToOneQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Include_person()
        {
            base.Include_person();

            Assert.Equal(
                @"SELECT [a].[Id], [a].[City], [a].[Street], [a.Resident].[Id], [a.Resident].[Name]
FROM [Address] AS [a]
INNER JOIN [Person] AS [a.Resident] ON [a].[Id] = [a.Resident].[Id]",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Include_person_shadow()
        {
            base.Include_person_shadow();

            Assert.Equal(
                @"SELECT [a].[Id], [a].[City], [a].[PersonId], [a].[Street], [a.Resident].[Id], [a.Resident].[Name]
FROM [Address2] AS [a]
INNER JOIN [Person2] AS [a.Resident] ON [a].[PersonId] = [a.Resident].[Id]",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Include_address()
        {
            base.Include_address();

            Assert.Equal(
                @"SELECT [p].[Id], [p].[Name], [p.Address].[Id], [p.Address].[City], [p.Address].[Street]
FROM [Person] AS [p]
LEFT JOIN [Address] AS [p.Address] ON [p].[Id] = [p.Address].[Id]",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Include_address_shadow()
        {
            base.Include_address_shadow();

            Assert.Equal(
                @"SELECT [p].[Id], [p].[Name], [p.Address].[Id], [p.Address].[City], [p.Address].[PersonId], [p.Address].[Street]
FROM [Person2] AS [p]
LEFT JOIN [Address2] AS [p.Address] ON [p].[Id] = [p.Address].[PersonId]",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        private string Sql => Fixture.TestSqlLoggerFactory.SqlStatements.Last();

        public class OneToOneQuerySqlServerFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
