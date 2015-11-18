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

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Contains(@"SELECT ""r"".""UniqueNo"", ""r"".""MaxLengthProperty"", ""r"".""Name"", ""r"".""RowVersion""
FROM ""Sample"" AS ""r""
WHERE ""r"".""UniqueNo"" = 1
LIMIT 1",
                Sql);

            Assert.Contains(@"SELECT ""r"".""UniqueNo"", ""r"".""MaxLengthProperty"", ""r"".""Name"", ""r"".""RowVersion""
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
SELECT changes();",
                Sql);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Contains(@"@p0: 
@p1: Third
@p2: 00000000-0000-0000-0000-000000000003

INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"")
VALUES (@p0, @p1, @p2);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();",
                Sql);
        }

        // Sqlite does not support length
        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(10, context.Model.FindEntityType(typeof(One)).FindProperty("MaxLengthProperty").GetMaxLength());
            }
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            Assert.Contains(@"@p0: 0
@p1: Book1

INSERT INTO ""BookDetail"" (""Id"", ""BookId"")
VALUES (@p0, @p1);
SELECT changes();",
                Sql);

            Assert.Contains(@"@p0: 0
@p1: 

INSERT INTO ""BookDetail"" (""Id"", ""BookId"")
VALUES (@p0, @p1);
SELECT changes();",
                Sql);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            Assert.Contains(@"@p0: 
@p1: ValidString
@p2: 00000000-0000-0000-0000-000000000001

INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"")
VALUES (@p0, @p1, @p2);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();",
                Sql);

            Assert.Contains(@"@p0: 
@p1: 
@p2: 00000000-0000-0000-0000-000000000002

INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"")
VALUES (@p0, @p1, @p2);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();",
                Sql);
        }

        // Sqlite does not support length
        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(16, context.Model.FindEntityType(typeof(Two)).FindProperty("Data").GetMaxLength());
            }
        }

        // Sqlite does not support rowversion. See issue #2195
        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Model.FindEntityType(typeof(Two)).FindProperty("Timestamp").IsConcurrencyToken);
            }
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
