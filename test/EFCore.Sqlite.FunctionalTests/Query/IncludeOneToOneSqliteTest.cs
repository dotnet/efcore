// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneSqliteTest : IncludeOneToOneTestBase, IClassFixture<OneToOneQuerySqliteFixture>
    {
        private readonly OneToOneQuerySqliteFixture _fixture;

        public IncludeOneToOneSqliteTest(OneToOneQuerySqliteFixture fixture)
        {
            _fixture = fixture;
            _fixture.TestSqlLoggerFactory.Clear();
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();

        private const string FileLineEnding = @"
";

        private string Sql => _fixture.TestSqlLoggerFactory.SqlStatements.Last().Replace(Environment.NewLine, FileLineEnding);
    }
}
