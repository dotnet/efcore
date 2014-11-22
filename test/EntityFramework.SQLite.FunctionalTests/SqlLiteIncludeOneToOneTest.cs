// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;
using System.Linq;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    // TODO: Blocked by #1127
    internal class SQLiteIncludeOneToOneTest : IncludeOneToOneTestBase, IClassFixture<SQLiteOneToOneQueryFixture>
    {
        public override void Include_person()
        {
            base.Include_person();

            Assert.Equal(
                @"SELECT ""a"".""City"", ""a"".""Id"", ""a"".""Street"", ""p"".""Id"", ""p"".""Name""
FROM ""Address"" AS ""a""
INNER JOIN ""Person"" AS ""p"" ON ""a"".""Id"" = ""p"".""Id""",
                Sql);
        }

        public override void Include_person_shadow()
        {
            base.Include_person_shadow();

            Assert.Equal(
                @"SELECT ""a"".""City"", ""a"".""Id"", ""a"".""PersonId"", ""a"".""Street"", ""p"".""Id"", ""p"".""Name""
FROM ""Address2"" AS ""a""
INNER JOIN ""Person2"" AS ""p"" ON ""a"".""PersonId"" = ""p"".""Id""",
                Sql);
        }

        public override void Include_address()
        {
            base.Include_address();

            Assert.Equal(
                @"SELECT ""p"".""Id"", ""p"".""Name"", ""a"".""City"", ""a"".""Id"", ""a"".""Street""
FROM ""Person"" AS ""p""
INNER JOIN ""Address"" AS ""a"" ON ""a"".""Id"" = ""p"".""Id""",
                Sql);
        }

        public override void Include_address_shadow()
        {
            base.Include_address_shadow();

            Assert.Equal(
                @"SELECT ""p"".""Id"", ""p"".""Name"", ""a"".""City"", ""a"".""Id"", ""a"".""PersonId"", ""a"".""Street""
FROM ""Person2"" AS ""p""
INNER JOIN ""Address2"" AS ""a"" ON ""a"".""PersonId"" = ""p"".""Id""",
                Sql);
        }

        private readonly SQLiteOneToOneQueryFixture _fixture;

        public SQLiteIncludeOneToOneTest(SQLiteOneToOneQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.SqlStatements.Last(); }
        }
    }
}
