// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Sql
{
    public class MigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationSqlGenerator SqlGenerator
        {
            get
            {
                var sqlGenerator = new Mock<UpdateSqlGenerator>() { CallBase = true };
                var migrationSqlGenerator = new Mock<MigrationSqlGenerator>(sqlGenerator.Object) { CallBase = true };

                return migrationSqlGenerator.Object;
            }
        }

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
                "ALTER TABLE \"People\" ADD \"Birthday\" date DEFAULT (CURRENT_TIMESTAMP);" + EOL,
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
                "ALTER SEQUENCE \"dbo\".\"DefaultSequence\" INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "ALTER SEQUENCE \"DefaultSequence\" INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
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

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"DefaultSequence\" AS bigint START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE \"DefaultSequence\" START WITH 3 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            Assert.Equal(
                "CREATE TABLE \"dbo\".\"People\" (" + EOL +
                "    \"Id\" int NOT NULL," + EOL +
                "    \"EmployerId\" int," + EOL +
                "    \"SSN\" char(11)," + EOL +
                "    PRIMARY KEY (\"Id\")," + EOL +
                "    UNIQUE (\"SSN\")," + EOL +
                "    FOREIGN KEY (\"EmployerId\") REFERENCES \"Companies\"" + EOL +
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
                "DROP SEQUENCE \"dbo\".\"DefaultSequence\";" + EOL,
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
                "-- I <3 DDL;" + EOL,
                Sql);
        }
    }
}
