// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationOracleTest : DataAnnotationTestBase<DataAnnotationOracleTest.DataAnnotationOracleFixture>
    {
        public DataAnnotationOracleTest(DataAnnotationOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        [Fact]
        public virtual ModelBuilder Default_for_key_string_column_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login1>().Property(l => l.UserName).HasDefaultValue("default");
            modelBuilder.Ignore<Profile1>();

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.ModelValidationKeyDefaultValueWarning,
                    RelationalStrings.LogKeyHasDefaultValue.GenerateMessage(nameof(Login1.UserName), nameof(Login1)),
                    "RelationalEventId.ModelValidationKeyDefaultValueWarning"),
                Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);

            return modelBuilder;
        }

        public override ModelBuilder Non_public_annotations_are_enabled()
        {
            var modelBuilder = base.Non_public_annotations_are_enabled();

            var relational = GetProperty<PrivateMemberAnnotationClass>(modelBuilder, "PersonFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Field_annotations_are_enabled()
        {
            var modelBuilder = base.Field_annotations_are_enabled();

            var relational = GetProperty<FieldAnnotationClass>(modelBuilder, "_personFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Key_and_column_work_together()
        {
            var modelBuilder = base.Key_and_column_work_together();

            var relational = GetProperty<ColumnKeyAnnotationClass1>(modelBuilder, "PersonFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Key_and_MaxLength_64_produce_nvarchar_64()
        {
            var modelBuilder = base.Key_and_MaxLength_64_produce_nvarchar_64();

            var property = GetProperty<ColumnKeyAnnotationClass2>(modelBuilder, "PersonFirstName");
            Assert.Equal("NVARCHAR2(64)", TestServiceFactory.Instance.Create<OracleTypeMappingSource>().GetMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = base.Timestamp_takes_precedence_over_MaxLength();

            var property = GetProperty<TimestampAndMaxlen>(modelBuilder, "MaxTimestamp");
            Assert.Equal("RAW(8)", TestServiceFactory.Instance.Create<OracleTypeMappingSource>().GetMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder TableNameAttribute_affects_table_name_in_TPH()
        {
            var modelBuilder = base.TableNameAttribute_affects_table_name_in_TPH();

            var relational = modelBuilder.Model.FindEntityType(typeof(TNAttrBase)).Relational();
            Assert.Equal("A", relational.TableName);

            return modelBuilder;
        }

        public override ModelBuilder DatabaseGeneratedOption_configures_the_property_correctly()
        {
            var modelBuilder = base.DatabaseGeneratedOption_configures_the_property_correctly();

            var identity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntity)).FindProperty(nameof(GeneratedEntity.Identity));
            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, identity.Oracle().ValueGenerationStrategy);

            return modelBuilder;
        }

        public override ModelBuilder DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties()
        {
            var modelBuilder = base.DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties();

            var entity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntityNonInteger));

            var stringProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.String));
            Assert.Null(stringProperty.Oracle().ValueGenerationStrategy);

            var dateTimeProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.DateTime));
            Assert.Null(dateTimeProperty.Oracle().ValueGenerationStrategy);

            var guidProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.Guid));
            Assert.Null(guidProperty.Oracle().ValueGenerationStrategy);

            return modelBuilder;
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Equal(
                @"SELECT ""r"".""UniqueNo"", ""r"".""MaxLengthProperty"", ""r"".""Name"", ""r"".""RowVersion"", ""r"".""UniqueNo"", ""r"".""Details_Name"", ""r"".""UniqueNo"", ""r"".""AdditionalDetails_Name""
FROM ""Sample"" ""r""
WHERE ""r"".""UniqueNo"" = 1
FETCH FIRST 1 ROWS ONLY

SELECT ""r"".""UniqueNo"", ""r"".""MaxLengthProperty"", ""r"".""Name"", ""r"".""RowVersion"", ""r"".""UniqueNo"", ""r"".""Details_Name"", ""r"".""UniqueNo"", ""r"".""AdditionalDetails_Name""
FROM ""Sample"" ""r""
WHERE ""r"".""UniqueNo"" = 1
FETCH FIRST 1 ROWS ONLY

:p2='1'
:p0='ModifiedData' (Nullable = false) (Size = 2000)
:p1='0x00000000000000000003000000000001' (Nullable = false) (Size = 16)
:p3='0x01000000000000000000000000000001' (Nullable = false) (Size = 16)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
v_RowCount INTEGER;
BEGIN
UPDATE ""Sample"" SET ""Name"" = :p0, ""RowVersion"" = :p1
WHERE ""UniqueNo"" = :p2 AND ""RowVersion"" = :p3;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur1 FOR SELECT v_RowCount FROM DUAL;

END;

:p2='1'
:p0='ChangedData' (Nullable = false) (Size = 2000)
:p1='0x00000000000000000002000000000001' (Nullable = false) (Size = 16)
:p3='0x01000000000000000000000000000001' (Nullable = false) (Size = 16)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
v_RowCount INTEGER;
BEGIN
UPDATE ""Sample"" SET ""Name"" = :p0, ""RowVersion"" = :p1
WHERE ""UniqueNo"" = :p2 AND ""RowVersion"" = :p3;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur1 FOR SELECT v_RowCount FROM DUAL;

END;",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Equal(
                @":p0='' (Size = 10)
:p1='Third' (Nullable = false) (Size = 2000)
:p2='0x00000000000000000000000000000003' (Nullable = false) (Size = 16)
:p3='Third Additional Name' (Size = 2000)
:p4='Third Name' (Size = 2000)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowSample_0 IS RECORD
(
UniqueNo NUMBER(10)
);
TYPE efSample_0 IS TABLE OF efRowSample_0;
listSample_0 efSample_0;
v_RowCount INTEGER;
BEGIN

listSample_0 := efSample_0();
listSample_0.extend(1);
INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (:p0, :p1, :p2, :p3, :p4)
RETURNING ""UniqueNo"" INTO listSample_0(1);
OPEN :cur1 FOR SELECT listSample_0(1).UniqueNo FROM DUAL;
END;",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(
                @":p0='Short' (Size = 10)
:p1='ValidString' (Nullable = false) (Size = 2000)
:p2='0x00000000000000000000000000000001' (Nullable = false) (Size = 16)
:p3='Third Additional Name' (Size = 2000)
:p4='Third Name' (Size = 2000)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowSample_0 IS RECORD
(
UniqueNo NUMBER(10)
);
TYPE efSample_0 IS TABLE OF efRowSample_0;
listSample_0 efSample_0;
v_RowCount INTEGER;
BEGIN

listSample_0 := efSample_0();
listSample_0.extend(1);
INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (:p0, :p1, :p2, :p3, :p4)
RETURNING ""UniqueNo"" INTO listSample_0(1);
OPEN :cur1 FOR SELECT listSample_0(1).UniqueNo FROM DUAL;
END;

:p0='VeryVeryVeryVeryVeryVeryLongString'
:p1='ValidString' (Nullable = false) (Size = 2000)
:p2='0x00000000000000000000000000000002' (Nullable = false) (Size = 16)
:p3='Third Additional Name' (Size = 2000)
:p4='Third Name' (Size = 2000)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowSample_0 IS RECORD
(
UniqueNo NUMBER(10)
);
TYPE efSample_0 IS TABLE OF efRowSample_0;
listSample_0 efSample_0;
v_RowCount INTEGER;
BEGIN

listSample_0 := efSample_0();
listSample_0.extend(1);
INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (:p0, :p1, :p2, :p3, :p4)
RETURNING ""UniqueNo"" INTO listSample_0(1);
OPEN :cur1 FOR SELECT listSample_0(1).UniqueNo FROM DUAL;
END;",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            Assert.Contains(
                ":p0='' (DbType = Int32)" + _eol,
                Sql);

            Assert.Contains(
                ":p1='' (Nullable = false) (DbType = Int32)" + _eol,
                Sql);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            Assert.Equal(
                @":p0='' (Size = 10)
:p1='ValidString' (Nullable = false) (Size = 2000)
:p2='0x00000000000000000000000000000001' (Nullable = false) (Size = 16)
:p3='Two' (Size = 2000)
:p4='One' (Size = 2000)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowSample_0 IS RECORD
(
UniqueNo NUMBER(10)
);
TYPE efSample_0 IS TABLE OF efRowSample_0;
listSample_0 efSample_0;
v_RowCount INTEGER;
BEGIN

listSample_0 := efSample_0();
listSample_0.extend(1);
INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (:p0, :p1, :p2, :p3, :p4)
RETURNING ""UniqueNo"" INTO listSample_0(1);
OPEN :cur1 FOR SELECT listSample_0(1).UniqueNo FROM DUAL;
END;

:p0='' (Size = 10)
:p1='' (Nullable = false) (Size = 2000)
:p2='0x00000000000000000000000000000002' (Nullable = false) (Size = 16)
:p3='Two' (Size = 2000)
:p4='One' (Size = 2000)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowSample_0 IS RECORD
(
UniqueNo NUMBER(10)
);
TYPE efSample_0 IS TABLE OF efRowSample_0;
listSample_0 efSample_0;
v_RowCount INTEGER;
BEGIN

listSample_0 := efSample_0();
listSample_0.extend(1);
INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")
VALUES (:p0, :p1, :p2, :p3, :p4)
RETURNING ""UniqueNo"" INTO listSample_0(1);
OPEN :cur1 FOR SELECT listSample_0(1).UniqueNo FROM DUAL;
END;",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.StringLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(
                @":p0='ValidString' (Size = 16)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowTwo_0 IS RECORD
(
Id NUMBER(10)
,Timestamp RAW(8)
);
TYPE efTwo_0 IS TABLE OF efRowTwo_0;
listTwo_0 efTwo_0;
v_RowCount INTEGER;
BEGIN

listTwo_0 := efTwo_0();
listTwo_0.extend(1);
INSERT INTO ""Two"" (""Data"")
VALUES (:p0)
RETURNING ""Id"", ""Timestamp"" INTO listTwo_0(1);
OPEN :cur1 FOR SELECT listTwo_0(1).Id,listTwo_0(1).Timestamp FROM DUAL;
END;

:p0='ValidButLongString'
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)

DECLARE
TYPE efRowTwo_0 IS RECORD
(
Id NUMBER(10)
,Timestamp RAW(8)
);
TYPE efTwo_0 IS TABLE OF efRowTwo_0;
listTwo_0 efTwo_0;
v_RowCount INTEGER;
BEGIN

listTwo_0 := efTwo_0();
listTwo_0.extend(1);
INSERT INTO ""Two"" (""Data"")
VALUES (:p0)
RETURNING ""Id"", ""Timestamp"" INTO listTwo_0(1);
OPEN :cur1 FOR SELECT listTwo_0(1).Id,listTwo_0(1).Timestamp FROM DUAL;
END;",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        private static readonly string _eol = Environment.NewLine;

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class DataAnnotationOracleFixture : DataAnnotationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
