// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class PropertyEntrySqlServerTest : PropertyEntryTestBase<SqlServerTestStore, F1SqlServerFixture>
    {
        public PropertyEntrySqlServerTest(F1SqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override void Property_entry_original_value_is_set()
        {
            base.Property_entry_original_value_is_set();

            Assert.Contains(
                @"SELECT TOP(1) [e].[Id], [e].[EngineSupplierId], [e].[Name]
FROM [Engines] AS [e]",
                Sql);

            Assert.Contains(
                @"SET NOCOUNT ON;
UPDATE [Engines] SET [Name] = @p0
WHERE [Id] = @p1 AND [EngineSupplierId] = @p2 AND [Name] = @p3;
SELECT @@ROWCOUNT;",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
