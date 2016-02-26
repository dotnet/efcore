// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;
using Xunit;

#if NETSTANDARDAPP1_5
using System.Threading;
#endif

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class QuerySqliteTest : QueryTestBase<NorthwindQuerySqliteFixture>
    {
        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Contains(
                @"SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""ContactName""
    LIMIT @__p_0
) AS ""t""
ORDER BY ""t"".""ContactName""
LIMIT -1 OFFSET @__p_1",
                Sql);
        }

        public QuerySqliteTest(NorthwindQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
