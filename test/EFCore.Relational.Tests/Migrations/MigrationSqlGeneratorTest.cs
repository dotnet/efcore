// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        public override void AddColumnOperation_with_defaultValue()
        {
            base.AddColumnOperation_with_defaultValue();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" ADD ""Name"" varchar(30) NOT NULL DEFAULT 'John Doe';
");
        }

        public override void AddColumnOperation_with_defaultValueSql()
        {
            base.AddColumnOperation_with_defaultValueSql();

            AssertSql(@"ALTER TABLE ""People"" ADD ""Birthday"" date NULL DEFAULT (CURRENT_TIMESTAMP);
");
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(@"ALTER TABLE ""People"" ADD ""Alias"" just_string(max) NOT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            AssertSql(@"ALTER TABLE ""Person"" ADD ""Name"" just_string(30) NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            AssertSql(@"ALTER TABLE ""Person"" ADD ""Name"" just_string(30) NULL;
");
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            AssertSql(@"ALTER TABLE ""Base"" ADD ""Foo"" just_string(max) NULL;
");
        }

        public override void AddForeignKeyOperation_with_name()
        {
            base.AddForeignKeyOperation_with_name();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" ADD CONSTRAINT ""FK_People_Companies"" FOREIGN KEY (""EmployerId1"", ""EmployerId2"") REFERENCES ""hr"".""Companies"" (""Id1"", ""Id2"") ON DELETE CASCADE;
");
        }

        public override void AddForeignKeyOperation_without_name()
        {
            base.AddForeignKeyOperation_without_name();

            AssertSql(@"ALTER TABLE ""People"" ADD FOREIGN KEY (""SpouseId"") REFERENCES ""People"" (""Id"");
");
        }

        public override void AddForeignKeyOperation_without_principal_columns()
        {
            base.AddForeignKeyOperation_without_principal_columns();

            AssertSql(@"ALTER TABLE ""People"" ADD FOREIGN KEY (""SpouseId"") REFERENCES ""People"";
");
        }

        public override void AddPrimaryKeyOperation_with_name()
        {
            base.AddPrimaryKeyOperation_with_name();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" ADD CONSTRAINT ""PK_People"" PRIMARY KEY (""Id1"", ""Id2"");
");
        }

        public override void AddPrimaryKeyOperation_without_name()
        {
            base.AddPrimaryKeyOperation_without_name();

            AssertSql(@"ALTER TABLE ""People"" ADD PRIMARY KEY (""Id"");
");
        }

        public override void AddUniqueConstraintOperation_with_name()
        {
            base.AddUniqueConstraintOperation_with_name();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" ADD CONSTRAINT ""AK_People_DriverLicense"" UNIQUE (""DriverLicense_State"", ""DriverLicense_Number"");
");
        }

        public override void AddUniqueConstraintOperation_without_name()
        {
            base.AddUniqueConstraintOperation_without_name();

            AssertSql(@"ALTER TABLE ""People"" ADD UNIQUE (""SSN"");
");
        }

        public override void CreateCheckConstraintOperation_with_name()
        {
            base.CreateCheckConstraintOperation_with_name();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" ADD CONSTRAINT ""CK_People_DriverLicense"" CHECK (DriverLicense_Number > 0);
");
        }

        public override void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_with_minValue_and_maxValue();

            AssertSql(@"ALTER SEQUENCE ""dbo"".""EntityFrameworkHiLoSequence"" INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_without_minValue_and_maxValue();

            AssertSql(@"ALTER SEQUENCE ""EntityFrameworkHiLoSequence"" INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
");
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            AssertSql(@"CREATE UNIQUE INDEX ""IX_People_Name"" ON ""dbo"".""People"" (""FirstName"", ""LastName"");
");
        }

        public override void CreateIndexOperation_nonunique()
        {
            base.CreateIndexOperation_nonunique();

            AssertSql(@"CREATE INDEX ""IX_People_Name"" ON ""People"" (""Name"");
");
        }

        public override void CreateIndexOperation_with_where_clauses()
        {
            base.CreateIndexOperation_with_where_clauses();

            AssertSql(@"CREATE INDEX ""IX_People_Name"" ON ""People"" (""Name"") WHERE [Id] > 2;
");
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue();

            AssertSql(@"CREATE SEQUENCE ""dbo"".""EntityFrameworkHiLoSequence"" START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue_not_long();

            AssertSql(@"CREATE SEQUENCE ""dbo"".""EntityFrameworkHiLoSequence"" AS default_int_mapping START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");
        }

        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_without_minValue_and_maxValue();

            AssertSql(@"CREATE SEQUENCE ""EntityFrameworkHiLoSequence"" START WITH 3 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
");
        }

        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            AssertSql(
                @"CREATE TABLE ""dbo"".""People"" (
    ""Id"" default_int_mapping NOT NULL,
    ""EmployerId"" default_int_mapping NULL,
    ""SSN"" char(11) NULL,
    PRIMARY KEY (""Id""),
    UNIQUE (""SSN""),
    CHECK (SSN > 0),
    FOREIGN KEY (""EmployerId"") REFERENCES ""Companies"" (""Id"")
);
");
        }

        public override void CreateTableOperation_no_key()
        {
            base.CreateTableOperation_no_key();

            AssertSql(
                @"CREATE TABLE ""Anonymous"" (
    ""Value"" default_int_mapping NOT NULL
);
");
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" DROP COLUMN ""LuckyNumber"";
");
        }

        public override void DropForeignKeyOperation()
        {
            base.DropForeignKeyOperation();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" DROP CONSTRAINT ""FK_People_Companies"";
");
        }

        public override void DropPrimaryKeyOperation()
        {
            base.DropPrimaryKeyOperation();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" DROP CONSTRAINT ""PK_People"";
");
        }

        public override void DropSequenceOperation()
        {
            base.DropSequenceOperation();

            AssertSql(@"DROP SEQUENCE ""dbo"".""EntityFrameworkHiLoSequence"";
");
        }

        public override void DropTableOperation()
        {
            base.DropTableOperation();

            AssertSql(@"DROP TABLE ""dbo"".""People"";
");
        }

        public override void DropUniqueConstraintOperation()
        {
            base.DropUniqueConstraintOperation();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" DROP CONSTRAINT ""AK_People_SSN"";
");
        }

        public override void DropCheckConstraintOperation()
        {
            base.DropCheckConstraintOperation();

            AssertSql(@"ALTER TABLE ""dbo"".""People"" DROP CONSTRAINT ""CK_People_SSN"";
");
        }

        public override void SqlOperation()
        {
            base.SqlOperation();

            AssertSql(@"-- I <3 DDL
");
        }

        [ConditionalFact]
        public void Generate_doesnt_batch_by_default()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "SELECT 1;"
                },
                new SqlOperation
                {
                    Sql = "SELECT 2;"
                });

            AssertSql(
                @"SELECT 1;
GO

SELECT 2;
");
        }

        public override void InsertDataOperation()
        {
            base.InsertDataOperation();

            AssertSql(
                @"INSERT INTO ""People"" (""Id"", ""Full Name"")
VALUES (0, NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"")
VALUES (1, 'Daenerys Targaryen');
INSERT INTO ""People"" (""Id"", ""Full Name"")
VALUES (2, 'John Snow');
INSERT INTO ""People"" (""Id"", ""Full Name"")
VALUES (3, 'Arya Stark');
INSERT INTO ""People"" (""Id"", ""Full Name"")
VALUES (4, 'Harry Strickland');
");
        }

        public override void DeleteDataOperation_simple_key()
        {
            base.DeleteDataOperation_simple_key();

            // TODO remove rowcount
            AssertSql(
                @"DELETE FROM ""People""
WHERE ""Id"" = 2;
SELECT provider_specific_rowcount();

DELETE FROM ""People""
WHERE ""Id"" = 4;
SELECT provider_specific_rowcount();

");
        }

        public override void DeleteDataOperation_composite_key()
        {
            base.DeleteDataOperation_composite_key();

            // TODO remove rowcount
            AssertSql(
                @"DELETE FROM ""People""
WHERE ""First Name"" = 'Hodor' AND ""Last Name"" IS NULL;
SELECT provider_specific_rowcount();

DELETE FROM ""People""
WHERE ""First Name"" = 'Daenerys' AND ""Last Name"" = 'Targaryen';
SELECT provider_specific_rowcount();

");
        }

        public override void UpdateDataOperation_simple_key()
        {
            base.UpdateDataOperation_simple_key();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE ""People"" SET ""Full Name"" = 'Daenerys Stormborn'
WHERE ""Id"" = 1;
SELECT provider_specific_rowcount();

UPDATE ""People"" SET ""Full Name"" = 'Homeless Harry Strickland'
WHERE ""Id"" = 4;
SELECT provider_specific_rowcount();

");
        }

        public override void UpdateDataOperation_composite_key()
        {
            base.UpdateDataOperation_composite_key();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE ""People"" SET ""First Name"" = 'Hodor'
WHERE ""Id"" = 0 AND ""Last Name"" IS NULL;
SELECT provider_specific_rowcount();

UPDATE ""People"" SET ""First Name"" = 'Harry'
WHERE ""Id"" = 4 AND ""Last Name"" = 'Strickland';
SELECT provider_specific_rowcount();

");
        }

        public override void UpdateDataOperation_multiple_columns()
        {
            base.UpdateDataOperation_multiple_columns();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE ""People"" SET ""First Name"" = 'Daenerys', ""Nickname"" = 'Dany'
WHERE ""Id"" = 1;
SELECT provider_specific_rowcount();

UPDATE ""People"" SET ""First Name"" = 'Harry', ""Nickname"" = 'Homeless'
WHERE ""Id"" = 4;
SELECT provider_specific_rowcount();

");
        }

        public MigrationSqlGeneratorTest()
            : base(RelationalTestHelpers.Instance)
        {
        }
    }
}
