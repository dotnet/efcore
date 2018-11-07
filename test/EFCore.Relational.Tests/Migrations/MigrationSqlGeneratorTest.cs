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

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" ADD \"Name\" varchar(30) NOT NULL DEFAULT 'John Doe';" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_defaultValueSql()
        {
            base.AddColumnOperation_with_defaultValueSql();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date NULL DEFAULT (CURRENT_TIMESTAMP);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_computed_column_SQL()
        {
            base.AddColumnOperation_with_computed_column_SQL();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Alias\" just_string(max) NOT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            Assert.Equal(
                "ALTER TABLE \"Person\" ADD \"Name\" just_string(30) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            Assert.Equal(
                "ALTER TABLE \"Person\" ADD \"Name\" just_string(30) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            Assert.Equal(
                "ALTER TABLE \"Base\" ADD \"Foo\" just_string(max) NULL;" + EOL,
                Sql);
        }

        public override void AddForeignKeyOperation_with_name()
        {
            base.AddForeignKeyOperation_with_name();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" ADD CONSTRAINT \"FK_People_Companies\" FOREIGN KEY (\"EmployerId1\", \"EmployerId2\") REFERENCES \"hr\".\"Companies\" (\"Id1\", \"Id2\") ON DELETE CASCADE;" + EOL,
                Sql);
        }

        public override void AddForeignKeyOperation_without_name()
        {
            base.AddForeignKeyOperation_without_name();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD FOREIGN KEY (\"SpouseId\") REFERENCES \"People\" (\"Id\");" + EOL,
                Sql);
        }

        public override void AddForeignKeyOperation_without_principal_columns()
        {
            base.AddForeignKeyOperation_without_principal_columns();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD FOREIGN KEY (\"SpouseId\") REFERENCES \"People\";" + EOL,
                Sql);
        }

        public override void AddPrimaryKeyOperation_with_name()
        {
            base.AddPrimaryKeyOperation_with_name();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" ADD CONSTRAINT \"PK_People\" PRIMARY KEY (\"Id1\", \"Id2\");" + EOL,
                Sql);
        }

        public override void AddPrimaryKeyOperation_without_name()
        {
            base.AddPrimaryKeyOperation_without_name();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD PRIMARY KEY (\"Id\");" + EOL,
                Sql);
        }

        public override void AddUniqueConstraintOperation_with_name()
        {
            base.AddUniqueConstraintOperation_with_name();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" ADD CONSTRAINT \"AK_People_DriverLicense\" UNIQUE (\"DriverLicense_State\", \"DriverLicense_Number\");" + EOL,
                Sql);
        }

        public override void AddUniqueConstraintOperation_without_name()
        {
            base.AddUniqueConstraintOperation_without_name();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD UNIQUE (\"SSN\");" + EOL,
                Sql);
        }

        public override void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_with_minValue_and_maxValue();

            Assert.Equal(
                "ALTER SEQUENCE \"dbo\".\"EntityFrameworkHiLoSequence\" INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "ALTER SEQUENCE \"EntityFrameworkHiLoSequence\" INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            Assert.Equal(
                "CREATE UNIQUE INDEX \"IX_People_Name\" ON \"dbo\".\"People\" (\"FirstName\", \"LastName\");" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_nonunique()
        {
            base.CreateIndexOperation_nonunique();

            Assert.Equal(
                "CREATE INDEX \"IX_People_Name\" ON \"People\" (\"Name\");" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_with_where_clauses()
        {
            base.CreateIndexOperation_with_where_clauses();

            Assert.Equal(
                "CREATE INDEX \"IX_People_Name\" ON \"People\" (\"Name\") WHERE [Id] > 2;" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"EntityFrameworkHiLoSequence\" START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue_not_long();

            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"EntityFrameworkHiLoSequence\" AS default_int_mapping START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE \"EntityFrameworkHiLoSequence\" START WITH 3 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            Assert.Equal(
                "CREATE TABLE \"dbo\".\"People\" (" + EOL +
                "    \"Id\" default_int_mapping NOT NULL," + EOL +
                "    \"EmployerId\" default_int_mapping NULL," + EOL +
                "    \"SSN\" char(11) NULL," + EOL +
                "    PRIMARY KEY (\"Id\")," + EOL +
                "    UNIQUE (\"SSN\")," + EOL +
                "    FOREIGN KEY (\"EmployerId\") REFERENCES \"Companies\" (\"Id\")" + EOL +
                ");" + EOL,
                Sql);
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP COLUMN \"LuckyNumber\";" + EOL,
                Sql);
        }

        public override void DropForeignKeyOperation()
        {
            base.DropForeignKeyOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP CONSTRAINT \"FK_People_Companies\";" + EOL,
                Sql);
        }

        public override void DropPrimaryKeyOperation()
        {
            base.DropPrimaryKeyOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP CONSTRAINT \"PK_People\";" + EOL,
                Sql);
        }

        public override void DropSequenceOperation()
        {
            base.DropSequenceOperation();

            Assert.Equal(
                "DROP SEQUENCE \"dbo\".\"EntityFrameworkHiLoSequence\";" + EOL,
                Sql);
        }

        public override void DropTableOperation()
        {
            base.DropTableOperation();

            Assert.Equal(
                "DROP TABLE \"dbo\".\"People\";" + EOL,
                Sql);
        }

        public override void DropUniqueConstraintOperation()
        {
            base.DropUniqueConstraintOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP CONSTRAINT \"AK_People_SSN\";" + EOL,
                Sql);
        }

        public override void SqlOperation()
        {
            base.SqlOperation();

            Assert.Equal(
                "-- I <3 DDL" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "SELECT 1;" + EOL +
                "GO" + EOL +
                EOL +
                "SELECT 2;" + EOL,
                Sql);
        }

        public override void InsertDataOperation()
        {
            base.InsertDataOperation();

            Assert.Equal(
                "INSERT INTO \"People\" (\"Id\", \"Full Name\")" + EOL +
                "VALUES (0, NULL);" + EOL +
                "INSERT INTO \"People\" (\"Id\", \"Full Name\")" + EOL +
                "VALUES (1, 'Daenerys Targaryen');" + EOL +
                "INSERT INTO \"People\" (\"Id\", \"Full Name\")" + EOL +
                "VALUES (2, 'John Snow');" + EOL +
                "INSERT INTO \"People\" (\"Id\", \"Full Name\")" + EOL +
                "VALUES (3, 'Arya Stark');" + EOL +
                "INSERT INTO \"People\" (\"Id\", \"Full Name\")" + EOL +
                "VALUES (4, 'Harry Strickland');" + EOL,
                Sql);
        }

        public override void DeleteDataOperation_simple_key()
        {
            base.DeleteDataOperation_simple_key();

            // TODO remove rowcount
            Assert.Equal(
                "DELETE FROM \"People\"" + EOL +
                "WHERE \"Id\" = 2;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL +
                "DELETE FROM \"People\"" + EOL +
                "WHERE \"Id\" = 4;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL,
                Sql);
        }

        public override void DeleteDataOperation_composite_key()
        {
            base.DeleteDataOperation_composite_key();

            // TODO remove rowcount
            Assert.Equal(
                "DELETE FROM \"People\"" + EOL +
                "WHERE \"First Name\" = 'Hodor' AND \"Last Name\" IS NULL;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL +
                "DELETE FROM \"People\"" + EOL +
                "WHERE \"First Name\" = 'Daenerys' AND \"Last Name\" = 'Targaryen';" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL,
                Sql);
        }

        public override void UpdateDataOperation_simple_key()
        {
            base.UpdateDataOperation_simple_key();

            // TODO remove rowcount
            Assert.Equal(
                "UPDATE \"People\" SET \"Full Name\" = 'Daenerys Stormborn'" + EOL +
                "WHERE \"Id\" = 1;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL +
                "UPDATE \"People\" SET \"Full Name\" = 'Homeless Harry Strickland'" + EOL +
                "WHERE \"Id\" = 4;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL,
                Sql);
        }

        public override void UpdateDataOperation_composite_key()
        {
            base.UpdateDataOperation_composite_key();

            // TODO remove rowcount
            Assert.Equal(
                "UPDATE \"People\" SET \"First Name\" = 'Hodor'" + EOL +
                "WHERE \"Id\" = 0 AND \"Last Name\" IS NULL;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL +
                "UPDATE \"People\" SET \"First Name\" = 'Harry'" + EOL +
                "WHERE \"Id\" = 4 AND \"Last Name\" = 'Strickland';" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL,
                Sql);
        }

        public override void UpdateDataOperation_multiple_columns()
        {
            base.UpdateDataOperation_multiple_columns();

            // TODO remove rowcount
            Assert.Equal(
                "UPDATE \"People\" SET \"First Name\" = 'Daenerys', \"Nickname\" = 'Dany'" + EOL +
                "WHERE \"Id\" = 1;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL +
                "UPDATE \"People\" SET \"First Name\" = 'Harry', \"Nickname\" = 'Homeless'" + EOL +
                "WHERE \"Id\" = 4;" + EOL +
                "SELECT provider_specific_rowcount();" + EOL + EOL,
                Sql);
        }

        public MigrationSqlGeneratorTest()
            : base(RelationalTestHelpers.Instance)
        {
        }
    }
}
