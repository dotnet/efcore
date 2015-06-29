// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class DataAnnotationSqliteTest : DataAnnotationTestBase<SqliteTestStore, DataAnnotationSqliteFixture>
    {
        public DataAnnotationSqliteTest(DataAnnotationSqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void RequiredAttribute_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_throws_while_inserting_null_value();

            Assert.Equal(@"@p0: ValidString
@p1: 00000000-0000-0000-0000-000000000001

INSERT INTO ""Sample"" (""Name"", ""RowVersion"")
VALUES (@p0, @p1);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();

@p0: 
@p1: 00000000-0000-0000-0000-000000000002

INSERT INTO ""Sample"" (""Name"", ""RowVersion"")
VALUES (@p0, @p1);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();", Sql);
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Equal(@"SELECT ""r"".""UniqueNo"", ""r"".""Name"", ""r"".""RowVersion""
FROM ""Sample"" AS ""r""
WHERE ""r"".""UniqueNo"" = 1
LIMIT 1

SELECT ""r"".""UniqueNo"", ""r"".""Name"", ""r"".""RowVersion""
FROM ""Sample"" AS ""r""
WHERE ""r"".""UniqueNo"" = 1
LIMIT 1

@p2: 1
@p0: ModifiedData
@p1: 00000000-0000-0000-0003-000000000001
@p3: 00000001-0000-0000-0000-000000000001

UPDATE ""Sample"" SET ""Name"" = @p0, ""RowVersion"" = @p1
WHERE ""UniqueNo"" = @p2 AND ""RowVersion"" = @p3;
SELECT changes();

@p2: 1
@p0: ChangedData
@p1: 00000000-0000-0000-0002-000000000001
@p3: 00000001-0000-0000-0000-000000000001

UPDATE ""Sample"" SET ""Name"" = @p0, ""RowVersion"" = @p1
WHERE ""UniqueNo"" = @p2 AND ""RowVersion"" = @p3;
SELECT changes();", Sql);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Equal(@"@p0: Third
@p1: 00000000-0000-0000-0000-000000000003

INSERT INTO ""Sample"" (""Name"", ""RowVersion"")
VALUES (@p0, @p1);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();", Sql);
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
