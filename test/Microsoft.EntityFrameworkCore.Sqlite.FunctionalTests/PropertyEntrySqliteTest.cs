// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class PropertyEntrySqliteTest : PropertyEntryTestBase<SqliteTestStore, F1SqliteFixture>
    {
        public PropertyEntrySqliteTest(F1SqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void Property_entry_original_value_is_set()
        {
            base.Property_entry_original_value_is_set();

            Assert.Contains(
                @"SELECT ""e"".""Id"", ""e"".""EngineSupplierId"", ""e"".""Name""
FROM ""Engines"" AS ""e""
LIMIT 1",
                Sql);

            Assert.Contains(
                @"UPDATE ""Engines"" SET ""Name"" = @p0
WHERE ""Id"" = @p1 AND ""EngineSupplierId"" = @p2 AND ""Name"" = @p3;
SELECT changes();",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
