// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqlServerDatabaseModelFactoryTest : IClassFixture<SqlServerDatabaseModelFactoryTest.SqlServerDatabaseModelFixture>
    {
        protected SqlServerDatabaseModelFixture Fixture { get; }

        public SqlServerDatabaseModelFactoryTest(SqlServerDatabaseModelFixture fixture)
        {
            Fixture = fixture;
            Fixture.ListLoggerFactory.Clear();
        }

        #region Sequences

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Create_sequences_with_facets()
        {
            Test(
                @"
CREATE SEQUENCE DefaultFacetsSequence;

CREATE SEQUENCE db2.CustomFacetsSequence
    AS int
    START WITH 1
    INCREMENT BY 2
    MAXVALUE 8
    MINVALUE -3
    CYCLE;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var defaultSequence = dbModel.Sequences.First(ds => ds.Name == "DefaultFacetsSequence");
                    Assert.Equal("dbo", defaultSequence.Schema);
                    Assert.Equal("DefaultFacetsSequence", defaultSequence.Name);
                    Assert.Equal("bigint", defaultSequence.StoreType);
                    Assert.False(defaultSequence.IsCyclic);
                    Assert.Equal(1, defaultSequence.IncrementBy);
                    Assert.Null(defaultSequence.StartValue);
                    Assert.Null(defaultSequence.MinValue);
                    Assert.Null(defaultSequence.MaxValue);

                    var customSequence = dbModel.Sequences.First(ds => ds.Name == "CustomFacetsSequence");
                    Assert.Equal("db2", customSequence.Schema);
                    Assert.Equal("CustomFacetsSequence", customSequence.Name);
                    Assert.Equal("int", customSequence.StoreType);
                    Assert.True(customSequence.IsCyclic);
                    Assert.Equal(2, customSequence.IncrementBy);
                    Assert.Equal(1, customSequence.StartValue);
                    Assert.Equal(-3, customSequence.MinValue);
                    Assert.Equal(8, customSequence.MaxValue);
                },
                @"
DROP SEQUENCE DefaultFacetsSequence;

DROP SEQUENCE db2.CustomFacetsSequence");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Sequence_min_max_start_values_are_null_if_default()
        {
            Test(
                @"
CREATE SEQUENCE [TinyIntSequence] AS tinyint;

CREATE SEQUENCE [SmallIntSequence] AS smallint;

CREATE SEQUENCE [IntSequence] AS int;

CREATE SEQUENCE [BigIntSequence] AS bigint;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    Assert.All(
                        dbModel.Sequences,
                        s =>
                        {
                            Assert.Null(s.StartValue);
                            Assert.Null(s.MinValue);
                            Assert.Null(s.MaxValue);
                        });
                },
                @"
DROP SEQUENCE [TinyIntSequence];

DROP SEQUENCE [SmallIntSequence];

DROP SEQUENCE [IntSequence];

DROP SEQUENCE [BigIntSequence];");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Sequence_min_max_start_values_are_not_null_if_decimal()
        {
            Test(
                @"
CREATE SEQUENCE [DecimalSequence] AS decimal;

CREATE SEQUENCE [NumericSequence] AS numeric;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    Assert.All(
                        dbModel.Sequences,
                        s =>
                        {
                            Assert.NotNull(s.StartValue);
                            Assert.NotNull(s.MinValue);
                            Assert.NotNull(s.MaxValue);
                        });
                },
                @"
DROP SEQUENCE [DecimalSequence];

DROP SEQUENCE [NumericSequence];");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Sequence_using_type_alias()
        {
            Fixture.TestStore.ExecuteNonQuery(
                @"
CREATE TYPE [dbo].[TestTypeAlias] FROM int;");

            Test(
                @"
CREATE SEQUENCE [TypeAliasSequence] AS [dbo].[TestTypeAlias];",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var sequence = Assert.Single(dbModel.Sequences);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", sequence.Schema);
                    Assert.Equal("TypeAliasSequence", sequence.Name);
                    Assert.Equal("int", sequence.StoreType);
                    Assert.False(sequence.IsCyclic);
                    Assert.Equal(1, sequence.IncrementBy);
                    Assert.Null(sequence.StartValue);
                    Assert.Null(sequence.MinValue);
                    Assert.Null(sequence.MaxValue);
                },
                @"
DROP SEQUENCE [TypeAliasSequence];
DROP TYPE [dbo].[TestTypeAlias];");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Sequence_using_type_with_facets()
        {
            Test(
                @"
CREATE SEQUENCE [TypeFacetSequence] AS decimal(10, 0);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var sequence = Assert.Single(dbModel.Sequences);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", sequence.Schema);
                    Assert.Equal("TypeFacetSequence", sequence.Name);
                    Assert.Equal("decimal(10, 0)", sequence.StoreType);
                    Assert.False(sequence.IsCyclic);
                    Assert.Equal(1, sequence.IncrementBy);
                },
                @"
DROP SEQUENCE [TypeFacetSequence];");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Filter_sequences_based_on_schema()
        {
            Test(
                @"
CREATE SEQUENCE [dbo].[Sequence];

CREATE SEQUENCE [db2].[Sequence]",
                Enumerable.Empty<string>(),
                new[] { "db2" },
                dbModel =>
                {
                    var sequence = Assert.Single(dbModel.Sequences);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("db2", sequence.Schema);
                    Assert.Equal("Sequence", sequence.Name);
                    Assert.Equal("bigint", sequence.StoreType);
                    Assert.False(sequence.IsCyclic);
                    Assert.Equal(1, sequence.IncrementBy);
                },
                @"
DROP SEQUENCE [dbo].[Sequence];

DROP SEQUENCE [db2].[Sequence];");
        }

        #endregion

        #region Model

        [ConditionalFact]
        public void Set_default_schema()
        {
            Test(
                "SELECT 1",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var defaultSchema = Fixture.TestStore.ExecuteScalar<string>("SELECT SCHEMA_NAME()");
                    Assert.Equal(defaultSchema, dbModel.DefaultSchema);
                },
                null);
        }

        [ConditionalFact]
        public void Create_tables()
        {
            Test(
                @"
CREATE TABLE [dbo].[Everest] ( id int );

CREATE TABLE [dbo].[Denali] ( id int );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    Assert.Collection(
                        dbModel.Tables.OrderBy(t => t.Name),
                        d =>
                        {
                            Assert.Equal("dbo", d.Schema);
                            Assert.Equal("Denali", d.Name);
                        },
                        e =>
                        {
                            Assert.Equal("dbo", e.Schema);
                            Assert.Equal("Everest", e.Name);
                        });
                },
                @"
DROP TABLE [dbo].[Everest];

DROP TABLE [dbo].[Denali];");
        }

        #endregion

        #region FilteringSchemaTable

        [ConditionalFact]
        public void Filter_schemas()
        {
            Test(
                @"
CREATE TABLE [db2].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B));",
                Enumerable.Empty<string>(),
                new[] { "db2" },
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [db2].[K2];");
        }

        [ConditionalFact]
        public void Filter_tables()
        {
            Test(
                @"
CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES K2 (A) );",
                new[] { "K2" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K2];");
        }

        [ConditionalFact]
        public void Filter_tables_with_qualified_name()
        {
            Test(
                @"
CREATE TABLE [dbo].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
                new[] { "[K.2]" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K.2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K.2];");
        }

        [ConditionalFact]
        public void Filter_tables_with_schema_qualified_name1()
        {
            Test(
                @"
CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db2].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
                new[] { "dbo.K2" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K2];

DROP TABLE [db2].[K2];");
        }

        [ConditionalFact]
        public void Filter_tables_with_schema_qualified_name2()
        {
            Test(
                @"
CREATE TABLE [dbo].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
                new[] { "[db.2].[K.2]" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K.2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [db.2].[Kilimanjaro];

DROP TABLE [dbo].[K.2];

DROP TABLE [db.2].[K.2];");
        }

        [ConditionalFact]
        public void Filter_tables_with_schema_qualified_name3()
        {
            Test(
                @"
CREATE TABLE [dbo].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db2].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
                new[] { "dbo.[K.2]" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K.2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K.2];

DROP TABLE [db2].[K.2];");
        }

        [ConditionalFact]
        public void Filter_tables_with_schema_qualified_name4()
        {
            Test(
                @"
CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
                new[] { "[db.2].K2" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("K2", table.Name);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(1, table.UniqueConstraints.Count);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE [db.2].[Kilimanjaro];

DROP TABLE [dbo].[K2];

DROP TABLE [db.2].[K2];");
        }

        [ConditionalFact]
        public void Complex_filtering_validation()
        {
            Test(
                @"
CREATE SEQUENCE [dbo].[Sequence];
CREATE SEQUENCE [db2].[Sequence];

CREATE TABLE [db.2].[QuotedTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db.2].[Table.With.Dot] ( Id int PRIMARY KEY );
CREATE TABLE [db.2].[SimpleTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db.2].[JustTableName] ( Id int PRIMARY KEY );

CREATE TABLE [dbo].[QuotedTableName] ( Id int PRIMARY KEY );
CREATE TABLE [dbo].[Table.With.Dot] ( Id int PRIMARY KEY );
CREATE TABLE [dbo].[SimpleTableName] ( Id int PRIMARY KEY );
CREATE TABLE [dbo].[JustTableName] ( Id int PRIMARY KEY );

CREATE TABLE [db2].[QuotedTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db2].[Table.With.Dot] ( Id int PRIMARY KEY );
CREATE TABLE [db2].[SimpleTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db2].[JustTableName] ( Id int PRIMARY KEY );

CREATE TABLE [db2].[PrincipalTable] (
    Id int PRIMARY KEY,
    UC1 nvarchar(450),
    UC2 int,
    Index1 bit,
    Index2 bigint
    CONSTRAINT UX UNIQUE (UC1, UC2),
)

CREATE INDEX IX_COMPOSITE ON [db2].[PrincipalTable] ( Index2, Index1 );

CREATE TABLE [db2].[DependentTable] (
    Id int PRIMARY KEY,
    ForeignKeyId1 nvarchar(450),
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES [db2].[PrincipalTable](UC1, UC2) ON DELETE CASCADE,
);",
                new[]
                {
                    "[db.2].[QuotedTableName]", "[db.2].SimpleTableName", "dbo.[Table.With.Dot]", "dbo.SimpleTableName", "JustTableName"
                },
                new[] { "db2" },
                dbModel =>
                {
                    var sequence = Assert.Single(dbModel.Sequences);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("db2", sequence.Schema);

                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "QuotedTableName"));
                    Assert.Empty(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "Table.With.Dot"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "SimpleTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "JustTableName"));

                    Assert.Empty(dbModel.Tables.Where(t => t.Schema == "dbo" && t.Name == "QuotedTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "dbo" && t.Name == "Table.With.Dot"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "dbo" && t.Name == "SimpleTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "dbo" && t.Name == "JustTableName"));

                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "QuotedTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "Table.With.Dot"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "SimpleTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "JustTableName"));

                    var principalTable = Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "PrincipalTable"));
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.NotNull(principalTable.PrimaryKey);
                    Assert.Single(principalTable.UniqueConstraints);
                    Assert.Single(principalTable.Indexes);

                    var dependentTable = Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "DependentTable"));
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Single(dependentTable.ForeignKeys);
                },
                @"
DROP SEQUENCE [dbo].[Sequence];
DROP SEQUENCE [db2].[Sequence];

DROP TABLE [db.2].[QuotedTableName];
DROP TABLE [db.2].[Table.With.Dot];
DROP TABLE [db.2].[SimpleTableName];
DROP TABLE [db.2].[JustTableName];

DROP TABLE [dbo].[QuotedTableName];
DROP TABLE [dbo].[Table.With.Dot];
DROP TABLE [dbo].[SimpleTableName];
DROP TABLE [dbo].[JustTableName];

DROP TABLE [db2].[QuotedTableName];
DROP TABLE [db2].[Table.With.Dot];
DROP TABLE [db2].[SimpleTableName];
DROP TABLE [db2].[JustTableName];
DROP TABLE [db2].[DependentTable];
DROP TABLE [db2].[PrincipalTable];");
        }

        #endregion

        #region Table

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
        public void Set_memory_optimized_table_annotation()
        {
            Test(
                @"
IF SERVERPROPERTY('IsXTPSupported') = 1 AND SERVERPROPERTY('EngineEdition') <> 5
BEGIN
IF NOT EXISTS (
    SELECT 1 FROM [sys].[filegroups] [FG] JOIN [sys].[database_files] [F] ON [FG].[data_space_id] = [F].[data_space_id] WHERE [FG].[type] = N'FX' AND [F].[type] = 2)
    BEGIN
    DECLARE @db_name NVARCHAR(MAX) = DB_NAME();
    DECLARE @fg_name NVARCHAR(MAX);
    SELECT TOP(1) @fg_name = [name] FROM [sys].[filegroups] WHERE [type] = N'FX';

    IF @fg_name IS NULL
        BEGIN
        SET @fg_name = @db_name + N'_MODFG';
        EXEC(N'ALTER DATABASE CURRENT ADD FILEGROUP [' + @fg_name + '] CONTAINS MEMORY_OPTIMIZED_DATA;');
        END

    DECLARE @path NVARCHAR(MAX);
    SELECT TOP(1) @path = [physical_name] FROM [sys].[database_files] WHERE charindex('\', [physical_name]) > 0 ORDER BY [file_id];
    IF (@path IS NULL)
        SET @path = '\' + @db_name;

    DECLARE @filename NVARCHAR(MAX) = right(@path, charindex('\', reverse(@path)) - 1);
    SET @filename = REPLACE(left(@filename, len(@filename) - charindex('.', reverse(@filename))), '''', '''''') + N'_MOD';
    DECLARE @new_path NVARCHAR(MAX) = REPLACE(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(MAX)), '''', '''''') + @filename;

    EXEC(N'
        ALTER DATABASE CURRENT
        ADD FILE (NAME=''' + @filename + ''', filename=''' + @new_path + ''')
        TO FILEGROUP [' + @fg_name + '];')
    END
END

IF SERVERPROPERTY('IsXTPSupported') = 1
EXEC(N'ALTER DATABASE CURRENT SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON;');

CREATE TABLE [Blogs] (
    [Id] int NOT NULL IDENTITY,
    CONSTRAINT [PK_Blogs] PRIMARY KEY NONCLUSTERED ([Id])
) WITH (MEMORY_OPTIMIZED = ON);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "Blogs"));

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.True((bool)table[SqlServerAnnotationNames.MemoryOptimized]);
                },
                "DROP TABLE [Blogs]");
        }

        [ConditionalFact]
        public void Create_columns()
        {
            Test(
                @"
CREATE TABLE [dbo].[Blogs] (
    Id int,
    Name nvarchar(100) NOT NULL,
);
EXECUTE sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Blog table comment.
On multiple lines.',
    @level0type = N'SCHEMA', @level0name = 'dbo', 
	@level1type = N'TABLE', @level1name = 'Blogs';
EXECUTE sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Blog.Id column comment.',
    @level0type = N'SCHEMA', @level0name = 'dbo', 
	@level1type = N'TABLE', @level1name = 'Blogs',
	@level2type = N'COLUMN', @level2name = 'Id';
",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = dbModel.Tables.Single();

                    Assert.Equal(2, table.Columns.Count);
                    Assert.All(
                        table.Columns, c =>
                        {
                            Assert.Equal("dbo", c.Table.Schema);
                            Assert.Equal("Blogs", c.Table.Name);
                            Assert.Equal(@"Blog table comment.
On multiple lines.", c.Table.Comment);
                        });

                    Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                    Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    Assert.Single(table.Columns.Where(c => c.Comment == "Blog.Id column comment."));
                    Assert.Equal(1, table.Columns.Where(c => c.Comment != null).Count());
                },
                "DROP TABLE [dbo].[Blogs]");
        }

        [ConditionalFact]
        public void Create_view_columns()
        {
            Test(
                @"
CREATE VIEW [dbo].[BlogsView]
 AS
SELECT
 CAST(100 AS int) AS Id,
 CAST(N'' AS nvarchar(100)) AS Name;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = dbModel.Tables.Single();

                    Assert.Equal(2, table.Columns.Count);
                    Assert.Equal(null, table.PrimaryKey);
                    Assert.All(
                        table.Columns, c =>
                        {
                            Assert.Equal("dbo", c.Table.Schema);
                            Assert.Equal("BlogsView", c.Table.Name);
                        });

                    Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                    Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                },
                "DROP VIEW [dbo].[BlogsView];");
        }

        [ConditionalFact]
        public void Create_primary_key()
        {
            Test(
                @"
CREATE TABLE PrimaryKeyTable (
    Id int PRIMARY KEY
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("dbo", pk.Table.Schema);
                    Assert.Equal("PrimaryKeyTable", pk.Table.Name);
                    Assert.StartsWith("PK__PrimaryK", pk.Name);
                    Assert.Null(pk[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE PrimaryKeyTable;");
        }

        [ConditionalFact]
        public void Create_unique_constraints()
        {
            Test(
                @"
CREATE TABLE UniqueConstraint (
    Id int,
    Name int Unique,
    IndexProperty int,
);

CREATE INDEX IX_INDEX on UniqueConstraint ( IndexProperty );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                    Assert.Equal("UniqueConstraint", uniqueConstraint.Table.Name);
                    Assert.StartsWith("UQ__UniqueCo", uniqueConstraint.Name);
                    Assert.Null(uniqueConstraint[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Name"
                        }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE UniqueConstraint;");
        }

        [ConditionalFact]
        public void Create_indexes()
        {
            Test(
                @"
CREATE TABLE IndexTable (
    Id int,
    Name int,
    IndexProperty int,
);

CREATE INDEX IX_NAME on IndexTable ( Name );
CREATE INDEX IX_INDEX on IndexTable ( IndexProperty );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = dbModel.Tables.Single();

                    Assert.Equal(2, table.Indexes.Count);
                    Assert.All(
                        table.Indexes, c =>
                        {
                            Assert.Equal("dbo", c.Table.Schema);
                            Assert.Equal("IndexTable", c.Table.Name);
                        });

                    Assert.Single(table.Indexes.Where(c => c.Name == "IX_NAME"));
                    Assert.Single(table.Indexes.Where(c => c.Name == "IX_INDEX"));
                },
                "DROP TABLE IndexTable;");
        }

        [ConditionalFact]
        public void Create_foreign_keys()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE FirstDependent (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
);

CREATE TABLE SecondDependent (
    Id int PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES PrincipalTable(Id) ON DELETE NO ACTION,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent").ForeignKeys);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", firstFk.Table.Schema);
                    Assert.Equal("FirstDependent", firstFk.Table.Name);
                    Assert.Equal("dbo", firstFk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", firstFk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId"
                        }, firstFk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.Cascade, firstFk.OnDelete);

                    var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent").ForeignKeys);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", secondFk.Table.Schema);
                    Assert.Equal("SecondDependent", secondFk.Table.Name);
                    Assert.Equal("dbo", secondFk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", secondFk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, secondFk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);
                },
                @"
DROP TABLE SecondDependent;
DROP TABLE FirstDependent;
DROP TABLE PrincipalTable;");
        }

        #endregion

        #region ColumnFacets

        [ConditionalFact]
        public void Column_with_type_alias_assigns_underlying_store_type()
        {
            Fixture.TestStore.ExecuteNonQuery(
                @"
CREATE TYPE dbo.TestTypeAlias FROM nvarchar(max);
CREATE TYPE db2.TestTypeAlias FROM int;");

            Test(
                @"
CREATE TABLE TypeAlias (
    Id int,
    typeAliasColumn dbo.TestTypeAlias NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var column = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "typeAliasColumn"));

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("nvarchar(max)", column.StoreType);
                },
                @"
DROP TABLE TypeAlias;
DROP TYPE dbo.TestTypeAlias;
DROP TYPE db2.TestTypeAlias;");
        }

        [ConditionalFact]
        public void Column_with_sysname_assigns_underlying_store_type_and_nullability()
        {
            Test(
                @"
CREATE TABLE TypeAlias (
    Id int,
    typeAliasColumn sysname
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var column = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "typeAliasColumn"));

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("nvarchar(128)", column.StoreType);
                    Assert.False(column.IsNullable);
                },
                @"
DROP TABLE TypeAlias;");
        }

        [ConditionalFact]
        public void Decimal_numeric_types_have_precision_scale()
        {
            Test(
                @"
CREATE TABLE NumericColumns (
    Id int,
    decimalColumn decimal NOT NULL,
    decimal105Column decimal(10, 5) NOT NULL,
    decimalDefaultColumn decimal(18, 2) NOT NULL,
    numericColumn numeric NOT NULL,
    numeric152Column numeric(15, 2) NOT NULL,
    numericDefaultColumn numeric(18, 2) NOT NULL,
    numericDefaultPrecisionColumn numeric(38, 5) NOT NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("decimal(18, 0)", columns.Single(c => c.Name == "decimalColumn").StoreType);
                    Assert.Equal("decimal(10, 5)", columns.Single(c => c.Name == "decimal105Column").StoreType);
                    Assert.Equal("decimal(18, 2)", columns.Single(c => c.Name == "decimalDefaultColumn").StoreType);
                    Assert.Equal("numeric(18, 0)", columns.Single(c => c.Name == "numericColumn").StoreType);
                    Assert.Equal("numeric(15, 2)", columns.Single(c => c.Name == "numeric152Column").StoreType);
                    Assert.Equal("numeric(18, 2)", columns.Single(c => c.Name == "numericDefaultColumn").StoreType);
                    Assert.Equal("numeric(38, 5)", columns.Single(c => c.Name == "numericDefaultPrecisionColumn").StoreType);
                },
                "DROP TABLE NumericColumns;");
        }

        [ConditionalFact]
        public void Max_length_of_negative_one_translate_to_max_in_store_type()
        {
            Test(
                @"
CREATE TABLE MaxColumns (
    Id int,
    varcharMaxColumn varchar(max) NULL,
    nvarcharMaxColumn nvarchar(max) NULL,
    varbinaryMaxColumn varbinary(max) NULL,
    binaryVaryingMaxColumn binary varying(max) NULL,
    charVaryingMaxColumn char varying(max) NULL,
    characterVaryingMaxColumn character varying(max) NULL,
    nationalCharVaryingMaxColumn national char varying(max) NULL,
    nationalCharacterVaryingMaxColumn national char varying(max) NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("varchar(max)", columns.Single(c => c.Name == "varcharMaxColumn").StoreType);
                    Assert.Equal("nvarchar(max)", columns.Single(c => c.Name == "nvarcharMaxColumn").StoreType);
                    Assert.Equal("varbinary(max)", columns.Single(c => c.Name == "varbinaryMaxColumn").StoreType);
                    Assert.Equal("varbinary(max)", columns.Single(c => c.Name == "binaryVaryingMaxColumn").StoreType);
                    Assert.Equal("varchar(max)", columns.Single(c => c.Name == "charVaryingMaxColumn").StoreType);
                    Assert.Equal("nvarchar(max)", columns.Single(c => c.Name == "nationalCharVaryingMaxColumn").StoreType);
                    Assert.Equal("nvarchar(max)", columns.Single(c => c.Name == "nationalCharacterVaryingMaxColumn").StoreType);
                },
                "DROP TABLE MaxColumns;");
        }

        [ConditionalFact]
        public void Specific_max_length_are_add_to_store_type()
        {
            Test(
                @"
CREATE TABLE LengthColumns (
    Id int,
    char10Column char(10) NULL,
    varchar66Column varchar(66) NULL,
    nchar99Column nchar(99) NULL,
    nvarchar100Column nvarchar(100) NULL,
    binary111Column binary(111) NULL,
    varbinary123Column varbinary(123) NULL,
    binaryVarying133Column binary varying(133) NULL,
    charVarying144Column char varying(144) NULL,
    character155Column character(155) NULL,
    characterVarying166Column character varying(166) NULL,
    nationalCharacter171Column national character(171) NULL,
    nationalCharVarying177Column national char varying(177) NULL,
    nationalCharacterVarying188Column national char varying(188) NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("char(10)", columns.Single(c => c.Name == "char10Column").StoreType);
                    Assert.Equal("varchar(66)", columns.Single(c => c.Name == "varchar66Column").StoreType);
                    Assert.Equal("nchar(99)", columns.Single(c => c.Name == "nchar99Column").StoreType);
                    Assert.Equal("nvarchar(100)", columns.Single(c => c.Name == "nvarchar100Column").StoreType);
                    Assert.Equal("binary(111)", columns.Single(c => c.Name == "binary111Column").StoreType);
                    Assert.Equal("varbinary(123)", columns.Single(c => c.Name == "varbinary123Column").StoreType);
                    Assert.Equal("varbinary(133)", columns.Single(c => c.Name == "binaryVarying133Column").StoreType);
                    Assert.Equal("varchar(144)", columns.Single(c => c.Name == "charVarying144Column").StoreType);
                    Assert.Equal("char(155)", columns.Single(c => c.Name == "character155Column").StoreType);
                    Assert.Equal("varchar(166)", columns.Single(c => c.Name == "characterVarying166Column").StoreType);
                    Assert.Equal("nchar(171)", columns.Single(c => c.Name == "nationalCharacter171Column").StoreType);
                    Assert.Equal("nvarchar(177)", columns.Single(c => c.Name == "nationalCharVarying177Column").StoreType);
                    Assert.Equal("nvarchar(188)", columns.Single(c => c.Name == "nationalCharacterVarying188Column").StoreType);
                },
                "DROP TABLE LengthColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_binary_varbinary()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthBinaryColumns (
    Id int,
    binaryColumn binary(8000),
    binaryVaryingColumn binary varying(8000),
    varbinaryColumn varbinary(8000)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("binary(8000)", columns.Single(c => c.Name == "binaryColumn").StoreType);
                    Assert.Equal("varbinary(8000)", columns.Single(c => c.Name == "binaryVaryingColumn").StoreType);
                    Assert.Equal("varbinary(8000)", columns.Single(c => c.Name == "varbinaryColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthBinaryColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_char_1()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthCharColumns (
    Id int,
    charColumn char(8000)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("char(8000)", columns.Single(c => c.Name == "charColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthCharColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_char_2()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthCharColumns (
    Id int,
    characterColumn character(8000)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("char(8000)", columns.Single(c => c.Name == "characterColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthCharColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_varchar()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthVarcharColumns (
    Id int,
    charVaryingColumn char varying(8000),
    characterVaryingColumn character varying(8000),
    varcharColumn varchar(8000)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "charVaryingColumn").StoreType);
                    Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "characterVaryingColumn").StoreType);
                    Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "varcharColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthVarcharColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_nchar_1()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    nationalCharColumn national char(4000),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "nationalCharColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthNcharColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_nchar_2()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    nationalCharacterColumn national character(4000),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "nationalCharacterColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthNcharColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_nchar_3()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    ncharColumn nchar(4000),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "ncharColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthNcharColumns;");
        }

        [ConditionalFact]
        public void Default_max_length_are_added_to_nvarchar()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthNvarcharColumns (
    Id int,
    nationalCharVaryingColumn national char varying(4000),
    nationalCharacterVaryingColumn national character varying(4000),
    nvarcharColumn nvarchar(4000)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nationalCharVaryingColumn").StoreType);
                    Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nationalCharacterVaryingColumn").StoreType);
                    Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nvarcharColumn").StoreType);
                },
                "DROP TABLE DefaultRequiredLengthNvarcharColumns;");
        }

        [ConditionalFact]
        public void Datetime_types_have_precision_if_non_null_scale()
        {
            Test(
                @"
CREATE TABLE LengthColumns (
    Id int,
    time4Column time(4) NULL,
    datetime24Column datetime2(4) NULL,
    datetimeoffset5Column datetimeoffset(5) NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("time(4)", columns.Single(c => c.Name == "time4Column").StoreType);
                    Assert.Equal("datetime2(4)", columns.Single(c => c.Name == "datetime24Column").StoreType);
                    Assert.Equal("datetimeoffset(5)", columns.Single(c => c.Name == "datetimeoffset5Column").StoreType);
                },
                "DROP TABLE LengthColumns;");
        }

        [ConditionalFact]
        public void Types_with_required_length_uses_length_of_one()
        {
            Test(
                @"
CREATE TABLE OneLengthColumns (
    Id int,
    binaryColumn binary NULL,
    binaryVaryingColumn binary varying NULL,
    characterColumn character NULL,
    characterVaryingColumn character varying NULL,
    charColumn char NULL,
    charVaryingColumn char varying NULL,
    nationalCharColumn national char NULL,
    nationalCharacterColumn national character NULL,
    nationalCharacterVaryingColumn national char varying NULL,
    nationalCharVaryingColumn national char varying NULL,
    ncharColumn nchar NULL,
    nvarcharColumn nvarchar NULL,
    varbinaryColumn varbinary NULL,
    varcharColumn varchar NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("binary(1)", columns.Single(c => c.Name == "binaryColumn").StoreType);
                    Assert.Equal("varbinary(1)", columns.Single(c => c.Name == "binaryVaryingColumn").StoreType);
                    Assert.Equal("char(1)", columns.Single(c => c.Name == "characterColumn").StoreType);
                    Assert.Equal("varchar(1)", columns.Single(c => c.Name == "characterVaryingColumn").StoreType);
                    Assert.Equal("char(1)", columns.Single(c => c.Name == "charColumn").StoreType);
                    Assert.Equal("varchar(1)", columns.Single(c => c.Name == "charVaryingColumn").StoreType);
                    Assert.Equal("nchar(1)", columns.Single(c => c.Name == "nationalCharColumn").StoreType);
                    Assert.Equal("nchar(1)", columns.Single(c => c.Name == "nationalCharacterColumn").StoreType);
                    Assert.Equal("nvarchar(1)", columns.Single(c => c.Name == "nationalCharacterVaryingColumn").StoreType);
                    Assert.Equal("nvarchar(1)", columns.Single(c => c.Name == "nationalCharVaryingColumn").StoreType);
                    Assert.Equal("nchar(1)", columns.Single(c => c.Name == "ncharColumn").StoreType);
                    Assert.Equal("nvarchar(1)", columns.Single(c => c.Name == "nvarcharColumn").StoreType);
                    Assert.Equal("varbinary(1)", columns.Single(c => c.Name == "varbinaryColumn").StoreType);
                    Assert.Equal("varchar(1)", columns.Single(c => c.Name == "varcharColumn").StoreType);
                },
                "DROP TABLE OneLengthColumns;");
        }

        [ConditionalFact]
        public void Store_types_without_any_facets()
        {
            Test(
                @"
CREATE TABLE NoFacetTypes (
    Id int,
    bigintColumn bigint NOT NULL,
    bitColumn bit NOT NULL,
    dateColumn date NOT NULL,
    datetime2Column datetime2 NULL,
    datetimeColumn datetime NULL,
    datetimeoffsetColumn datetimeoffset NULL,
    floatColumn float NOT NULL,
    geographyColumn geography NULL,
    geometryColumn geometry NULL,
    hierarchyidColumn hierarchyid NULL,
    imageColumn image NULL,
    intColumn int NOT NULL,
    moneyColumn money NOT NULL,
    ntextColumn ntext NULL,
    realColumn real NULL,
    smalldatetimeColumn smalldatetime NULL,
    smallintColumn smallint NOT NULL,
    smallmoneyColumn smallmoney NOT NULL,
    sql_variantColumn sql_variant NULL,
    textColumn text NULL,
    timeColumn time NULL,
    timestampColumn timestamp NULL,
    tinyintColumn tinyint NOT NULL,
    uniqueidentifierColumn uniqueidentifier NULL,
    xmlColumn xml NULL,
)

CREATE TABLE RowversionType (
    Id int,
    rowversionColumn rowversion NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single(t => t.Name == "NoFacetTypes").Columns;

                    Assert.Equal("bigint", columns.Single(c => c.Name == "bigintColumn").StoreType);
                    Assert.Equal("bit", columns.Single(c => c.Name == "bitColumn").StoreType);
                    Assert.Equal("date", columns.Single(c => c.Name == "dateColumn").StoreType);
                    Assert.Equal("datetime2", columns.Single(c => c.Name == "datetime2Column").StoreType);
                    Assert.Equal("datetime", columns.Single(c => c.Name == "datetimeColumn").StoreType);
                    Assert.Equal("datetimeoffset", columns.Single(c => c.Name == "datetimeoffsetColumn").StoreType);
                    Assert.Equal("float", columns.Single(c => c.Name == "floatColumn").StoreType);
                    Assert.Equal("geography", columns.Single(c => c.Name == "geographyColumn").StoreType);
                    Assert.Equal("geometry", columns.Single(c => c.Name == "geometryColumn").StoreType);
                    Assert.Equal("hierarchyid", columns.Single(c => c.Name == "hierarchyidColumn").StoreType);
                    Assert.Equal("image", columns.Single(c => c.Name == "imageColumn").StoreType);
                    Assert.Equal("int", columns.Single(c => c.Name == "intColumn").StoreType);
                    Assert.Equal("money", columns.Single(c => c.Name == "moneyColumn").StoreType);
                    Assert.Equal("ntext", columns.Single(c => c.Name == "ntextColumn").StoreType);
                    Assert.Equal("real", columns.Single(c => c.Name == "realColumn").StoreType);
                    Assert.Equal("smalldatetime", columns.Single(c => c.Name == "smalldatetimeColumn").StoreType);
                    Assert.Equal("smallint", columns.Single(c => c.Name == "smallintColumn").StoreType);
                    Assert.Equal("smallmoney", columns.Single(c => c.Name == "smallmoneyColumn").StoreType);
                    Assert.Equal("sql_variant", columns.Single(c => c.Name == "sql_variantColumn").StoreType);
                    Assert.Equal("text", columns.Single(c => c.Name == "textColumn").StoreType);
                    Assert.Equal("time", columns.Single(c => c.Name == "timeColumn").StoreType);
                    Assert.Equal("tinyint", columns.Single(c => c.Name == "tinyintColumn").StoreType);
                    Assert.Equal("uniqueidentifier", columns.Single(c => c.Name == "uniqueidentifierColumn").StoreType);
                    Assert.Equal("xml", columns.Single(c => c.Name == "xmlColumn").StoreType);

                    Assert.Equal(
                        "rowversion",
                        dbModel.Tables.Single(t => t.Name == "RowversionType").Columns.Single(c => c.Name == "rowversionColumn").StoreType);
                },
                @"
DROP TABLE NoFacetTypes;
DROP TABLE RowversionType;");
        }

        [ConditionalFact]
        public void Default_and_computed_values_are_stored()
        {
            Test(
                @"
CREATE TABLE DefaultComputedValues (
    Id int,
    FixedDefaultValue datetime2 NOT NULL DEFAULT ('October 20, 2015 11am'),
    ComputedValue AS GETDATE(),
    A int NOT NULL,
    B int NOT NULL,
    SumOfAAndB AS A + B PERSISTED,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal("('October 20, 2015 11am')", columns.Single(c => c.Name == "FixedDefaultValue").DefaultValueSql);
                    Assert.Equal(null, columns.Single(c => c.Name == "FixedDefaultValue").ComputedColumnSql);

                    Assert.Equal(null, columns.Single(c => c.Name == "ComputedValue").DefaultValueSql);
                    Assert.Equal("(getdate())", columns.Single(c => c.Name == "ComputedValue").ComputedColumnSql);

                    Assert.Equal(null, columns.Single(c => c.Name == "SumOfAAndB").DefaultValueSql);
                    Assert.Equal("([A]+[B])", columns.Single(c => c.Name == "SumOfAAndB").ComputedColumnSql);
                },
                "DROP TABLE DefaultComputedValues;");
        }

        [ConditionalFact]
        public void Default_value_matching_clr_default_is_not_stored()
        {
            Fixture.TestStore.ExecuteNonQuery(
                @"
CREATE TYPE datetime2Alias FROM datetime2(6);
CREATE TYPE datetimeoffsetAlias FROM datetimeoffset(6);
CREATE TYPE decimalAlias FROM decimal(17, 0);
CREATE TYPE numericAlias FROM numeric(17, 0);
CREATE TYPE timeAlias FROM time(6);");

            Test(
                @"
CREATE TABLE DefaultValues (
    IgnoredDefault1 int DEFAULT NULL,
    IgnoredDefault2 int NOT NULL DEFAULT NULL,
    IgnoredDefault3 bigint NOT NULL DEFAULT 0,
    IgnoredDefault4 bit NOT NULL DEFAULT 0,
    IgnoredDefault5 decimal NOT NULL DEFAULT 0,
    IgnoredDefault6 decimalAlias NOT NULL DEFAULT 0,
    IgnoredDefault7 float NOT NULL DEFAULT 0,
    IgnoredDefault9 int NOT NULL DEFAULT 0,
    IgnoredDefault10 money NOT NULL DEFAULT 0,
    IgnoredDefault11 numeric NOT NULL DEFAULT 0,
    IgnoredDefault12 numericAlias NOT NULL DEFAULT 0,
    IgnoredDefault13 real NOT NULL DEFAULT 0,
    IgnoredDefault14 smallint NOT NULL DEFAULT 0,
    IgnoredDefault15 smallmoney NOT NULL DEFAULT 0,
    IgnoredDefault16 tinyint NOT NULL DEFAULT 0,
    IgnoredDefault17 decimal NOT NULL DEFAULT 0.0,
    IgnoredDefault18 float NOT NULL DEFAULT 0.0,
    IgnoredDefault19 money NOT NULL DEFAULT 0.0,
    IgnoredDefault20 numeric NOT NULL DEFAULT 0.0,
    IgnoredDefault21 real NOT NULL DEFAULT 0.0,
    IgnoredDefault22 smallmoney NOT NULL DEFAULT 0.0,
    IgnoredDefault23 real NOT NULL DEFAULT CAST(0 AS real),
    IgnoredDefault24 float NOT NULL DEFAULT 0.0E0,
    IgnoredDefault25 date NOT NULL DEFAULT '0001-01-01',
    IgnoredDefault26 datetime NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    IgnoredDefault27 smalldatetime NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    IgnoredDefault28 datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.000',
    IgnoredDefault29 datetime2Alias NOT NULL DEFAULT '0001-01-01T00:00:00.000',
    IgnoredDefault30 datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.000+00:00',
    IgnoredDefault31 datetimeoffsetAlias NOT NULL DEFAULT '0001-01-01T00:00:00.000+00:00',
    IgnoredDefault32 time NOT NULL DEFAULT '00:00:00',
    IgnoredDefault33 timeAlias NOT NULL DEFAULT '00:00:00',
    IgnoredDefault34 uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.All(
                        columns,
                        t => Assert.Null(t.DefaultValueSql));
                },
                @"
DROP TABLE DefaultValues;
DROP TYPE datetime2Alias;
DROP TYPE datetimeoffsetAlias;
DROP TYPE decimalAlias;
DROP TYPE numericAlias;
DROP TYPE timeAlias;");
        }

        [ConditionalFact]
        public void ValueGenerated_is_set_for_identity_and_computed_column()
        {
            Test(
                @"
CREATE TABLE ValueGeneratedProperties (
    Id int IDENTITY(1, 1),
    NoValueGenerationColumn nvarchar(max),
    FixedDefaultValue datetime2 NOT NULL DEFAULT ('October 20, 2015 11am'),
    ComputedValue AS GETDATE(),
    rowversionColumn rowversion NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal(ValueGenerated.OnAdd, columns.Single(c => c.Name == "Id").ValueGenerated);
                    Assert.Null(columns.Single(c => c.Name == "NoValueGenerationColumn").ValueGenerated);
                    Assert.Null(columns.Single(c => c.Name == "FixedDefaultValue").ValueGenerated);
                    Assert.Null(columns.Single(c => c.Name == "ComputedValue").ValueGenerated);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, columns.Single(c => c.Name == "rowversionColumn").ValueGenerated);
                },
                "DROP TABLE ValueGeneratedProperties;");
        }

        [ConditionalFact]
        public void ConcurrencyToken_is_set_for_rowVersion()
        {
            Test(
                @"
CREATE TABLE RowVersionTable (
    Id int,
    rowversionColumn rowversion,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.True((bool)columns.Single(c => c.Name == "rowversionColumn")[ScaffoldingAnnotationNames.ConcurrencyToken]);
                },
                "DROP TABLE RowVersionTable;");
        }

        [ConditionalFact]
        public void Column_nullability_is_set()
        {
            Test(
                @"
CREATE TABLE NullableColumns (
    Id int,
    NullableInt int NULL,
    NonNullString nvarchar(max) NOT NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.True(columns.Single(c => c.Name == "NullableInt").IsNullable);
                    Assert.False(columns.Single(c => c.Name == "NonNullString").IsNullable);
                },
                "DROP TABLE NullableColumns;");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsHiddenColumns)]
        public void Hidden_columns_are_not_created()
        {
            Test(
                @"
CREATE TABLE dbo.HiddenColumnsTable
(
     Id int NOT NULL PRIMARY KEY CLUSTERED,
     Name varchar(50) NOT NULL,
     SysStartTime datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
     SysEndTime datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
     PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.HiddenColumnsTableHistory));
",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    Assert.Equal(2, columns.Count);
                    Assert.DoesNotContain(columns, c => c.Name == "SysStartTime");
                    Assert.DoesNotContain(columns, c => c.Name == "SysEndTime");
                },
                @"
ALTER TABLE dbo.HiddenColumnsTable SET (SYSTEM_VERSIONING = OFF);
DROP TABLE dbo.HiddenColumnsTableHistory;
DROP TABLE dbo.HiddenColumnsTable;
");
        }

        #endregion

        #region PrimaryKeyFacets

        [ConditionalFact]
        public void Create_composite_primary_key()
        {
            Test(
                @"
CREATE TABLE CompositePrimaryKeyTable (
    Id1 int,
    Id2 int,
    PRIMARY KEY (Id2, Id1)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("dbo", pk.Table.Schema);
                    Assert.Equal("CompositePrimaryKeyTable", pk.Table.Name);
                    Assert.StartsWith("PK__Composit", pk.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2",
                            "Id1"
                        }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE CompositePrimaryKeyTable;");
        }

        [ConditionalFact]
        public void Set_clustered_false_for_non_clustered_primary_key()
        {
            Test(
                @"
CREATE TABLE NonClusteredPrimaryKeyTable (
    Id1 int PRIMARY KEY NONCLUSTERED,
    Id2 int,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("dbo", pk.Table.Schema);
                    Assert.Equal("NonClusteredPrimaryKeyTable", pk.Table.Name);
                    Assert.StartsWith("PK__NonClust", pk.Name);
                    Assert.False((bool)pk[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id1"
                        }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE NonClusteredPrimaryKeyTable;");
        }

        [ConditionalFact]
        public void Set_clustered_false_for_primary_key_if_different_clustered_index()
        {
            Test(
                @"
CREATE TABLE NonClusteredPrimaryKeyTableWithClusteredIndex (
    Id1 int PRIMARY KEY NONCLUSTERED,
    Id2 int,
);

CREATE CLUSTERED INDEX ClusteredIndex ON NonClusteredPrimaryKeyTableWithClusteredIndex( Id2 );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("dbo", pk.Table.Schema);
                    Assert.Equal("NonClusteredPrimaryKeyTableWithClusteredIndex", pk.Table.Name);
                    Assert.StartsWith("PK__NonClust", pk.Name);
                    Assert.False((bool)pk[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id1"
                        }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE NonClusteredPrimaryKeyTableWithClusteredIndex;");
        }

        [ConditionalFact]
        public void Set_clustered_false_for_primary_key_if_different_clustered_constraint()
        {
            Test(
                @"
CREATE TABLE NonClusteredPrimaryKeyTableWithClusteredConstraint (
    Id1 int PRIMARY KEY,
    Id2 int,
    CONSTRAINT UK_Clustered UNIQUE CLUSTERED ( Id2 ),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("dbo", pk.Table.Schema);
                    Assert.Equal("NonClusteredPrimaryKeyTableWithClusteredConstraint", pk.Table.Name);
                    Assert.StartsWith("PK__NonClust", pk.Name);
                    Assert.False((bool)pk[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id1"
                        }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE NonClusteredPrimaryKeyTableWithClusteredConstraint;");
        }

        [ConditionalFact]
        public void Set_primary_key_name_from_index()
        {
            Test(
                @"
CREATE TABLE PrimaryKeyName (
    Id1 int,
    Id2 int,
    CONSTRAINT MyPK PRIMARY KEY ( Id2 ),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("dbo", pk.Table.Schema);
                    Assert.Equal("PrimaryKeyName", pk.Table.Name);
                    Assert.StartsWith("MyPK", pk.Name);
                    Assert.Null(pk[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE PrimaryKeyName;");
        }

        #endregion

        #region UniqueConstraintFacets

        [ConditionalFact]
        public void Create_composite_unique_constraint()
        {
            Test(
                @"
CREATE TABLE CompositeUniqueConstraintTable (
    Id1 int,
    Id2 int,
    CONSTRAINT UX UNIQUE (Id2, Id1)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                    Assert.Equal("CompositeUniqueConstraintTable", uniqueConstraint.Table.Name);
                    Assert.Equal("UX", uniqueConstraint.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2",
                            "Id1"
                        }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE CompositeUniqueConstraintTable;");
        }

        [ConditionalFact]
        public void Set_clustered_true_for_clustered_unique_constraint()
        {
            Test(
                @"
CREATE TABLE ClusteredUniqueConstraintTable (
    Id1 int,
    Id2 int UNIQUE CLUSTERED,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                    Assert.Equal("ClusteredUniqueConstraintTable", uniqueConstraint.Table.Name);
                    Assert.StartsWith("UQ__Clustere", uniqueConstraint.Name);
                    Assert.True((bool)uniqueConstraint[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE ClusteredUniqueConstraintTable;");
        }

        [ConditionalFact]
        public void Set_unique_constraint_name_from_index()
        {
            Test(
                @"
CREATE TABLE UniqueConstraintName (
    Id1 int,
    Id2 int,
    CONSTRAINT MyUC UNIQUE ( Id2 ),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                    Assert.Equal("UniqueConstraintName", uniqueConstraint.Table.Name);
                    Assert.Equal("MyUC", uniqueConstraint.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE UniqueConstraintName;");
        }

        #endregion

        #region IndexFacets

        [ConditionalFact]
        public void Create_composite_index()
        {
            Test(
                @"
CREATE TABLE CompositeIndexTable (
    Id1 int,
    Id2 int,
);

CREATE INDEX IX_COMPOSITE ON CompositeIndexTable ( Id2, Id1 );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", index.Table.Schema);
                    Assert.Equal("CompositeIndexTable", index.Table.Name);
                    Assert.Equal("IX_COMPOSITE", index.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2",
                            "Id1"
                        }, index.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE CompositeIndexTable;");
        }

        [ConditionalFact]
        public void Set_clustered_true_for_clustered_index()
        {
            Test(
                @"
CREATE TABLE ClusteredIndexTable (
    Id1 int,
    Id2 int,
);

CREATE CLUSTERED INDEX IX_CLUSTERED ON ClusteredIndexTable ( Id2 );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", index.Table.Schema);
                    Assert.Equal("ClusteredIndexTable", index.Table.Name);
                    Assert.Equal("IX_CLUSTERED", index.Name);
                    Assert.True((bool)index[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, index.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE ClusteredIndexTable;");
        }

        [ConditionalFact]
        public void Set_unique_true_for_unique_index()
        {
            Test(
                @"
CREATE TABLE UniqueIndexTable (
    Id1 int,
    Id2 int,
);

CREATE UNIQUE INDEX IX_UNIQUE ON UniqueIndexTable ( Id2 );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", index.Table.Schema);
                    Assert.Equal("UniqueIndexTable", index.Table.Name);
                    Assert.Equal("IX_UNIQUE", index.Name);
                    Assert.True(index.IsUnique);
                    Assert.Null(index.Filter);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, index.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE UniqueIndexTable;");
        }

        [ConditionalFact]
        public void Set_filter_for_filtered_index()
        {
            Test(
                @"
CREATE TABLE FilteredIndexTable (
    Id1 int,
    Id2 int NULL,
);

CREATE UNIQUE INDEX IX_UNIQUE ON FilteredIndexTable ( Id2 ) WHERE Id2 > 10;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", index.Table.Schema);
                    Assert.Equal("FilteredIndexTable", index.Table.Name);
                    Assert.Equal("IX_UNIQUE", index.Name);
                    Assert.Equal("([Id2]>(10))", index.Filter);
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, index.Columns.Select(ic => ic.Name).ToList());
                },
                "DROP TABLE FilteredIndexTable;");
        }

        #endregion

        #region ForeignKeyFacets

        [ConditionalFact]
        public void Create_composite_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id1 int,
    Id2 int,
    PRIMARY KEY (Id1, Id2)
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES PrincipalTable(Id1, Id2) ON DELETE CASCADE,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", fk.Table.Schema);
                    Assert.Equal("DependentTable", fk.Table.Name);
                    Assert.Equal("dbo", fk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId1",
                            "ForeignKeyId2"
                        }, fk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id1",
                            "Id2"
                        }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [ConditionalFact]
        public void Create_multiple_foreign_key_in_same_table()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE AnotherPrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
    FOREIGN KEY (ForeignKeyId2) REFERENCES AnotherPrincipalTable(Id) ON DELETE CASCADE,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var foreignKeys = dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys;

                    Assert.Equal(2, foreignKeys.Count);

                    var principalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "PrincipalTable"));

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", principalFk.Table.Schema);
                    Assert.Equal("DependentTable", principalFk.Table.Name);
                    Assert.Equal("dbo", principalFk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", principalFk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId1"
                        }, principalFk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.Cascade, principalFk.OnDelete);

                    var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "AnotherPrincipalTable"));

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", anotherPrincipalFk.Table.Schema);
                    Assert.Equal("DependentTable", anotherPrincipalFk.Table.Name);
                    Assert.Equal("dbo", anotherPrincipalFk.PrincipalTable.Schema);
                    Assert.Equal("AnotherPrincipalTable", anotherPrincipalFk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId2"
                        }, anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.Cascade, anotherPrincipalFk.OnDelete);
                },
                @"
DROP TABLE DependentTable;
DROP TABLE AnotherPrincipalTable;
DROP TABLE PrincipalTable;");
        }

        [ConditionalFact]
        public void Create_foreign_key_referencing_unique_constraint()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id1 int,
    Id2 int UNIQUE,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id2) ON DELETE CASCADE,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", fk.Table.Schema);
                    Assert.Equal("DependentTable", fk.Table.Name);
                    Assert.Equal("dbo", fk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId"
                        }, fk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id2"
                        }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [ConditionalFact]
        public void Set_name_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", fk.Table.Schema);
                    Assert.Equal("DependentTable", fk.Table.Name);
                    Assert.Equal("dbo", fk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId"
                        }, fk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    Assert.Equal("MYFK", fk.Name);
                },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [ConditionalFact]
        public void Set_referential_action_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE SET NULL,
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("dbo", fk.Table.Schema);
                    Assert.Equal("DependentTable", fk.Table.Name);
                    Assert.Equal("dbo", fk.PrincipalTable.Schema);
                    Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                    Assert.Equal(
                        new List<string>
                        {
                            "ForeignKeyId"
                        }, fk.Columns.Select(ic => ic.Name).ToList());
                    Assert.Equal(
                        new List<string>
                        {
                            "Id"
                        }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                    Assert.Equal(ReferentialAction.SetNull, fk.OnDelete);
                },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        #endregion

        #region Warnings

        [ConditionalFact]
        public void Warn_missing_schema()
        {
            Test(
                @"
CREATE TABLE Blank (
    Id int,
);",
                Enumerable.Empty<string>(),
                new[] { "MySchema" },
                dbModel =>
                {
                    Assert.Empty(dbModel.Tables);

                    var (_, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                    Assert.Equal(SqlServerResources.LogMissingSchema(new TestLogger<SqlServerLoggingDefinitions>()).EventId, Id);
                    Assert.Equal(SqlServerResources.LogMissingSchema(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("MySchema"), Message);
                },
                "DROP TABLE Blank;");
        }

        [ConditionalFact]
        public void Warn_missing_table()
        {
            Test(
                @"
CREATE TABLE Blank (
    Id int,
);",
                new[] { "MyTable" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    Assert.Empty(dbModel.Tables);

                    var (_, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                    Assert.Equal(SqlServerResources.LogMissingTable(new TestLogger<SqlServerLoggingDefinitions>()).EventId, Id);
                    Assert.Equal(SqlServerResources.LogMissingTable(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("MyTable"), Message);
                },
                "DROP TABLE Blank;");
        }

        [ConditionalFact]
        public void Warn_missing_principal_table_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
);",
                new[] { "DependentTable" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var (_, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                    Assert.Equal(SqlServerResources.LogPrincipalTableNotInSelectionSet(new TestLogger<SqlServerLoggingDefinitions>()).EventId, Id);
                    Assert.Equal(
                        SqlServerResources.LogPrincipalTableNotInSelectionSet(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage(
                            "MYFK", "dbo.DependentTable", "dbo.PrincipalTable"), Message);
                },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [ConditionalFact]
        public void Skip_reflexive_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
    CONSTRAINT MYFK FOREIGN KEY (Id) REFERENCES PrincipalTable(Id)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var (level, _, message, _, _) = Assert.Single(
                        Fixture.ListLoggerFactory.Log, t => t.Id == SqlServerEventId.ReflexiveConstraintIgnored);
                    Assert.Equal(LogLevel.Debug, level);
                    Assert.Equal(SqlServerResources.LogReflexiveConstraintIgnored(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("MYFK", "dbo.PrincipalTable"), message);

                    var table = Assert.Single(dbModel.Tables);
                    Assert.Empty(table.ForeignKeys);
                },
                @"
DROP TABLE PrincipalTable;");
        }

        #endregion

        private void Test(
            string createSql, IEnumerable<string> tables, IEnumerable<string> schemas, Action<DatabaseModel> asserter, string cleanupSql)
        {
            Fixture.TestStore.ExecuteNonQuery(createSql);

            try
            {
                var databaseModelFactory = new SqlServerDatabaseModelFactory(
                    new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                        Fixture.ListLoggerFactory,
                        new LoggingOptions(),
                        new DiagnosticListener("Fake"),
                        new SqlServerLoggingDefinitions()));

                var databaseModel = databaseModelFactory.Create(
                    Fixture.TestStore.ConnectionString,
                    new DatabaseModelFactoryOptions(tables, schemas));
                Assert.NotNull(databaseModel);
                asserter(databaseModel);
            }
            finally
            {
                if (!string.IsNullOrEmpty(cleanupSql))
                {
                    Fixture.TestStore.ExecuteNonQuery(cleanupSql);
                }
            }
        }

        public class SqlServerDatabaseModelFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = nameof(SqlServerDatabaseModelFactoryTest);
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public new SqlServerTestStore TestStore => (SqlServerTestStore)base.TestStore;

            public SqlServerDatabaseModelFixture()
            {
                TestStore.ExecuteNonQuery("CREATE SCHEMA db2");
                TestStore.ExecuteNonQuery("CREATE SCHEMA [db.2]");
            }

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Scaffolding.Name;
        }
    }
}
