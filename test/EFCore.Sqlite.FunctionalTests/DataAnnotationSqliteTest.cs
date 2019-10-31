// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationSqliteTest : DataAnnotationTestBase<DataAnnotationSqliteTest.DataAnnotationSqliteFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public DataAnnotationSqliteTest(DataAnnotationSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public override ModelBuilder Non_public_annotations_are_enabled()
        {
            var modelBuilder = base.Non_public_annotations_are_enabled();

            var relational = GetProperty<PrivateMemberAnnotationClass>(modelBuilder, "PersonFirstName");
            Assert.Equal("dsdsd", relational.GetColumnName());
            Assert.Equal("nvarchar(128)", relational.GetColumnType());

            return modelBuilder;
        }

        public override ModelBuilder Field_annotations_are_enabled()
        {
            var modelBuilder = base.Field_annotations_are_enabled();

            var relational = GetProperty<FieldAnnotationClass>(modelBuilder, "_personFirstName");
            Assert.Equal("dsdsd", relational.GetColumnName());
            Assert.Equal("nvarchar(128)", relational.GetColumnType());

            return modelBuilder;
        }

        public override ModelBuilder Key_and_column_work_together()
        {
            var modelBuilder = base.Key_and_column_work_together();

            var relational = GetProperty<ColumnKeyAnnotationClass1>(modelBuilder, "PersonFirstName");
            Assert.Equal("dsdsd", relational.GetColumnName());
            Assert.Equal("nvarchar(128)", relational.GetColumnType());

            return modelBuilder;
        }

        public override ModelBuilder Key_and_MaxLength_64_produce_nvarchar_64()
        {
            var modelBuilder = base.Key_and_MaxLength_64_produce_nvarchar_64();

            var property = GetProperty<ColumnKeyAnnotationClass2>(modelBuilder, "PersonFirstName");

            var storeType = property.GetRelationalTypeMapping().StoreType;

            Assert.Equal("TEXT", storeType);

            return modelBuilder;
        }

        public override ModelBuilder Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = base.Timestamp_takes_precedence_over_MaxLength();

            var property = GetProperty<TimestampAndMaxlen>(modelBuilder, "MaxTimestamp");

            var storeType = property.GetRelationalTypeMapping().StoreType;

            Assert.Equal("BLOB", storeType);

            return modelBuilder;
        }

        public override ModelBuilder TableNameAttribute_affects_table_name_in_TPH()
        {
            var modelBuilder = base.TableNameAttribute_affects_table_name_in_TPH();

            var relational = modelBuilder.Model.FindEntityType(typeof(TNAttrBase));
            Assert.Equal("A", relational.GetTableName());

            return modelBuilder;
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            AssertSql(
                @"SELECT ""s"".""UniqueNo"", ""s"".""MaxLengthProperty"", ""s"".""Name"", ""s"".""RowVersion"", ""t"".""UniqueNo"", ""t"".""AdditionalDetails_Name"", ""t0"".""UniqueNo"", ""t0"".""Details_Name""
FROM ""Sample"" AS ""s""
LEFT JOIN (
    SELECT ""s0"".""UniqueNo"", ""s0"".""AdditionalDetails_Name"", ""s1"".""UniqueNo"" AS ""UniqueNo0""
    FROM ""Sample"" AS ""s0""
    INNER JOIN ""Sample"" AS ""s1"" ON ""s0"".""UniqueNo"" = ""s1"".""UniqueNo""
    WHERE ""s0"".""AdditionalDetails_Name"" IS NOT NULL
) AS ""t"" ON ""s"".""UniqueNo"" = ""t"".""UniqueNo""
LEFT JOIN (
    SELECT ""s2"".""UniqueNo"", ""s2"".""Details_Name"", ""s3"".""UniqueNo"" AS ""UniqueNo0""
    FROM ""Sample"" AS ""s2""
    INNER JOIN ""Sample"" AS ""s3"" ON ""s2"".""UniqueNo"" = ""s3"".""UniqueNo""
    WHERE ""s2"".""Details_Name"" IS NOT NULL
) AS ""t0"" ON ""s"".""UniqueNo"" = ""t0"".""UniqueNo""
WHERE ""s"".""UniqueNo"" = 1
LIMIT 1",
                //
                @"SELECT ""s"".""UniqueNo"", ""s"".""MaxLengthProperty"", ""s"".""Name"", ""s"".""RowVersion"", ""t"".""UniqueNo"", ""t"".""AdditionalDetails_Name"", ""t0"".""UniqueNo"", ""t0"".""Details_Name""
FROM ""Sample"" AS ""s""
LEFT JOIN (
    SELECT ""s0"".""UniqueNo"", ""s0"".""AdditionalDetails_Name"", ""s1"".""UniqueNo"" AS ""UniqueNo0""
    FROM ""Sample"" AS ""s0""
    INNER JOIN ""Sample"" AS ""s1"" ON ""s0"".""UniqueNo"" = ""s1"".""UniqueNo""
    WHERE ""s0"".""AdditionalDetails_Name"" IS NOT NULL
) AS ""t"" ON ""s"".""UniqueNo"" = ""t"".""UniqueNo""
LEFT JOIN (
    SELECT ""s2"".""UniqueNo"", ""s2"".""Details_Name"", ""s3"".""UniqueNo"" AS ""UniqueNo0""
    FROM ""Sample"" AS ""s2""
    INNER JOIN ""Sample"" AS ""s3"" ON ""s2"".""UniqueNo"" = ""s3"".""UniqueNo""
    WHERE ""s2"".""Details_Name"" IS NOT NULL
) AS ""t0"" ON ""s"".""UniqueNo"" = ""t0"".""UniqueNo""
WHERE ""s"".""UniqueNo"" = 1
LIMIT 1",
                //
                @"@p2='1' (DbType = String)
@p0='ModifiedData' (Nullable = false) (Size = 12)
@p1='00000000-0000-0000-0003-000000000001' (DbType = String)
@p3='00000001-0000-0000-0000-000000000001' (DbType = String)

UPDATE ""Sample"" SET ""Name"" = @p0, ""RowVersion"" = @p1
WHERE ""UniqueNo"" = @p2 AND ""RowVersion"" = @p3;
SELECT changes();",
                //
                @"@p2='1' (DbType = String)
@p0='ChangedData' (Nullable = false) (Size = 11)
@p1='00000000-0000-0000-0002-000000000001' (DbType = String)
@p3='00000001-0000-0000-0000-000000000001' (DbType = String)

UPDATE ""Sample"" SET ""Name"" = @p0, ""RowVersion"" = @p1
WHERE ""UniqueNo"" = @p2 AND ""RowVersion"" = @p3;
SELECT changes();");
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            AssertSql(
                @"@p0=NULL
@p1='Third' (Nullable = false) (Size = 5)
@p2='00000000-0000-0000-0000-000000000003' (DbType = String)
@p3='Third Additional Name' (Size = 21)
@p4='Third Name' (Size = 10)

INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""rowid"" = last_insert_rowid();");
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

            AssertSql(
                @"@p0=NULL
@p1='1' (DbType = String)

INSERT INTO ""BookDetails"" (""AdditionalBookDetailsId"", ""AnotherBookId"")
VALUES (@p0, @p1);
SELECT ""Id""
FROM ""BookDetails""
WHERE changes() = 1 AND ""rowid"" = last_insert_rowid();",
                //
                @"@p0=NULL
@p1=NULL (Nullable = false)

INSERT INTO ""BookDetails"" (""AdditionalBookDetailsId"", ""AnotherBookId"")
VALUES (@p0, @p1);
SELECT ""Id""
FROM ""BookDetails""
WHERE changes() = 1 AND ""rowid"" = last_insert_rowid();");
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            AssertSql(
                @"@p0=NULL
@p1='ValidString' (Nullable = false) (Size = 11)
@p2='00000000-0000-0000-0000-000000000001' (DbType = String)
@p3='Two' (Size = 3)
@p4='One' (Size = 3)

INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""rowid"" = last_insert_rowid();",
                //
                @"@p0=NULL
@p1=NULL (Nullable = false)
@p2='00000000-0000-0000-0000-000000000002' (DbType = String)
@p3='Two' (Size = 3)
@p4='One' (Size = 3)

INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT ""UniqueNo""
FROM ""Sample""
WHERE changes() = 1 AND ""rowid"" = last_insert_rowid();");
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

        private static readonly string _eol = Environment.NewLine;

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class DataAnnotationSqliteFixture : DataAnnotationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
