// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
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

            Assert.Equal(
@"SELECT TOP(1) [e].[EngineSupplierId], [e].[Id], [e].[Name]
FROM [Engines] AS [e]

SET NOCOUNT OFF;
UPDATE [Engines] SET [Name] = @p2
WHERE [EngineSupplierId] = @p0 AND [Id] = @p1 AND [Name] = @p3;
SELECT @@ROWCOUNT;
", Sql);
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
