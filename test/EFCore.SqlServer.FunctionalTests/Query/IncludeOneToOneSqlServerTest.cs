// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneSqlServerTest : IncludeOneToOneTestBase, IClassFixture<OneToOneQuerySqlServerFixture>
    {
        public override void Include_person()
        {
            base.Include_person();

            Assert.Equal(
                @"SELECT [a].[Id], [a].[City], [a].[Street], [a.Resident].[Id], [a.Resident].[Name]
FROM [Address] AS [a]
INNER JOIN [Person] AS [a.Resident] ON [a].[Id] = [a.Resident].[Id]",
                Sql);
        }

        public override void Include_person_shadow()
        {
            base.Include_person_shadow();

            Assert.Equal(
                @"SELECT [a].[Id], [a].[City], [a].[PersonId], [a].[Street], [a.Resident].[Id], [a.Resident].[Name]
FROM [Address2] AS [a]
INNER JOIN [Person2] AS [a.Resident] ON [a].[PersonId] = [a.Resident].[Id]",
                Sql);
        }

        public override void Include_address()
        {
            base.Include_address();

            Assert.Equal(
                @"SELECT [p].[Id], [p].[Name], [p.Address].[Id], [p.Address].[City], [p.Address].[Street]
FROM [Person] AS [p]
LEFT JOIN [Address] AS [p.Address] ON [p].[Id] = [p.Address].[Id]",
                Sql);
        }

        public override void Include_address_shadow()
        {
            base.Include_address_shadow();

            Assert.Equal(
                @"SELECT [p].[Id], [p].[Name], [p.Address].[Id], [p.Address].[City], [p.Address].[PersonId], [p.Address].[Street]
FROM [Person2] AS [p]
LEFT JOIN [Address2] AS [p.Address] ON [p].[Id] = [p.Address].[PersonId]",
                Sql);
        }

        private readonly OneToOneQuerySqlServerFixture _fixture;

        public IncludeOneToOneSqlServerTest(OneToOneQuerySqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();

        private const string FileLineEnding = @"
";

        private string Sql => _fixture.TestSqlLoggerFactory.SqlStatements.Last().Replace(Environment.NewLine, FileLineEnding);
    }
}
