// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class PropertyEntrySqliteTest : PropertyEntryTestBase<F1SqliteFixture>
    {
        public PropertyEntrySqliteTest(F1SqliteFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Property_entry_original_value_is_set()
        {
            base.Property_entry_original_value_is_set();

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""EngineSupplierId"", ""e"".""Name"", ""e"".""StorageLocation_Latitude"", ""e"".""StorageLocation_Longitude""
FROM ""Engines"" AS ""e""
ORDER BY ""e"".""Id""
LIMIT 1",
                //
                @"@p1='1' (DbType = String)
@p2='1' (DbType = String)
@p0='FO 108X' (Size = 7)
@p3='ChangedEngine' (Size = 13)
@p4='47.64491' (Nullable = true) (DbType = String)
@p5='-122.128101' (Nullable = true) (DbType = String)

UPDATE ""Engines"" SET ""Name"" = @p0
WHERE ""Id"" = @p1 AND ""EngineSupplierId"" = @p2 AND ""Name"" = @p3 AND ""StorageLocation_Latitude"" = @p4 AND ""StorageLocation_Longitude"" = @p5;
SELECT changes();");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
