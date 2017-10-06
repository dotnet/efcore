// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerDatabaseModelFactoryTest : IClassFixture<SqlServerDatabaseModelFixture>
    {
        [Fact]
        public void It_reads_tables()
        {
            var sql = @"
CREATE TABLE [dbo].[Everest] ( id int );
CREATE TABLE [dbo].[Denali] ( id int );";
            var dbModel = CreateModel(sql, new List<string> { "Everest", "Denali" });

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
        }

        [Fact]
        public void It_reads_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db2");
            var sql = "CREATE TABLE dbo.Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE db2.Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES Ranges(Id) ON DELETE CASCADE)";
            var dbModel = CreateModel(sql, new List<string> { "Ranges", "Mountains" });

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("db2", fk.Table.Schema);
            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.Schema);
            Assert.Equal("Ranges", fk.PrincipalTable.Name);
            Assert.Equal("RangeId", fk.Columns.Single().Name);
            Assert.Equal("Id", fk.PrincipalColumns.Single().Name);
            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
        }

        [Fact]
        public void It_reads_composite_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db3");
            var sql = "CREATE TABLE dbo.Ranges1 ( Id INT IDENTITY (1,1), AltId INT, PRIMARY KEY(Id, AltId));" +
                      "CREATE TABLE db3.Mountains1 ( RangeId INT NOT NULL, RangeAltId INT NOT NULL, FOREIGN KEY (RangeId, RangeAltId) REFERENCES Ranges1(Id, AltId) ON DELETE NO ACTION)";
            var dbModel = CreateModel(sql, new List<string> { "Ranges1", "Mountains1" });

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("db3", fk.Table.Schema);
            Assert.Equal("Mountains1", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.Schema);
            Assert.Equal("Ranges1", fk.PrincipalTable.Name);
            Assert.Equal(new[] { "RangeId", "RangeAltId" }, fk.Columns.Select(c => c.Name).ToArray());
            Assert.Equal(new[] { "Id", "AltId" }, fk.PrincipalColumns.Select(c => c.Name).ToArray());
            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
        }

        [Fact]
        public void It_reads_primary_keys()
        {
            var sql = "CREATE TABLE Place1 ( Id int PRIMARY KEY NONCLUSTERED, Name int UNIQUE, Location int);" +
                      "CREATE CLUSTERED INDEX IX_Location_Name ON Place1 (Location, Name);" +
                      "CREATE NONCLUSTERED INDEX IX_Location ON Place1 (Location);";
            var dbModel = CreateModel(sql, new List<string> { "Place1" });

            var pkIndex = dbModel.Tables.Single().PrimaryKey;

            Assert.Equal("dbo", pkIndex.Table.Schema);
            Assert.Equal("Place1", pkIndex.Table.Name);
            Assert.StartsWith("PK__Place1", pkIndex.Name);
            Assert.False((bool?)pkIndex[SqlServerAnnotationNames.Clustered]);
            Assert.Equal(new List<string> { "Id" }, pkIndex.Columns.Select(ic => ic.Name).ToList());
        }

        [Fact]
        public void It_reads_unique_constraints()
        {
            var sql = "CREATE TABLE Place2 ( Id int PRIMARY KEY NONCLUSTERED, Name int UNIQUE, Location int);" +
                      "CREATE CLUSTERED INDEX IX_Location_Name ON Place2 (Location, Name);" +
                      "CREATE NONCLUSTERED INDEX IX_Location ON Place2 (Location);";
            var dbModel = CreateModel(sql, new List<string> { "Place2" });

            var indexes = dbModel.Tables.Single().UniqueConstraints;

            Assert.All(
                indexes, c =>
                {
                    Assert.Equal("dbo", c.Table.Schema);
                    Assert.Equal("Place2", c.Table.Name);
                });

            Assert.Collection(
                indexes,
                unique =>
                {
                    Assert.Equal("Name", unique.Columns.Single().Name);
                });
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Place ( Id int PRIMARY KEY NONCLUSTERED, Name int UNIQUE, Location int);" +
                      "CREATE CLUSTERED INDEX IX_Location_Name ON Place (Location, Name);" +
                      "CREATE NONCLUSTERED INDEX IX_Location ON Place (Location);";
            var dbModel = CreateModel(sql, new List<string> { "Place" });

            var indexes = dbModel.Tables.Single().Indexes;

            Assert.All(
                indexes, c =>
                {
                    Assert.Equal("dbo", c.Table.Schema);
                    Assert.Equal("Place", c.Table.Name);
                });

            Assert.Collection(
                indexes.OrderBy(i => i.Name),
                nonClustered =>
                {
                    Assert.Equal("IX_Location", nonClustered.Name);
                    Assert.Null(nonClustered[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal("Location", nonClustered.Columns.Select(ic => ic.Name).Single());
                },
                clusteredIndex =>
                {
                    Assert.Equal("IX_Location_Name", clusteredIndex.Name);
                    Assert.False(clusteredIndex.IsUnique);
                    Assert.True((bool?)clusteredIndex[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(new List<string> { "Location", "Name" }, clusteredIndex.Columns.Select(ic => ic.Name).ToList());
                });
        }

        [Fact]
        public void It_reads_columns()
        {
            var sql = @"
CREATE TABLE [dbo].[MountainsColumns] (
    Id int,
    Name nvarchar(100) NOT NULL,
    Latitude decimal( 5, 2 ) DEFAULT 0.0,
    Created datetime2(6) DEFAULT('October 20, 2015 11am'),
    DiscoveredDate datetime2,
    CurrentDate AS GETDATE(),
    Sum AS Latitude + 1.0,
    Modified rowversion,
    Primary Key (Name, Id)
);";
            var dbModel = CreateModel(sql, new List<string> { "MountainsColumns" });

            var columns = dbModel.Tables.Single().Columns;

            Assert.All(
                columns, c =>
                    {
                        Assert.Equal("dbo", c.Table.Schema);
                        Assert.Equal("MountainsColumns", c.Table.Name);
                    });

            Assert.Collection(
                columns,
                id =>
                    {
                        Assert.Equal("Id", id.Name);
                        Assert.Equal("int", id.StoreType);
                        Assert.False(id.IsNullable);
                        Assert.Null(id.DefaultValueSql);
                    },
                name =>
                    {
                        Assert.Equal("Name", name.Name);
                        Assert.Equal("nvarchar(100)", name.StoreType);
                        Assert.False(name.IsNullable);
                        Assert.Null(name.DefaultValueSql);
                    },
                lat =>
                    {
                        Assert.Equal("Latitude", lat.Name);
                        Assert.Equal("decimal(5, 2)", lat.StoreType);
                        Assert.True(lat.IsNullable);
                        Assert.Equal("((0.0))", lat.DefaultValueSql);
                    },
                created =>
                    {
                        Assert.Equal("Created", created.Name);
                        Assert.Equal("datetime2(6)", created.StoreType);
                        Assert.True(created.IsNullable);
                        Assert.Equal("('October 20, 2015 11am')", created.DefaultValueSql);
                    },
                discovered =>
                    {
                        Assert.Equal("DiscoveredDate", discovered.Name);
                        Assert.Equal("datetime2", discovered.StoreType);
                        Assert.True(discovered.IsNullable);
                        Assert.Null(discovered.DefaultValueSql);

                    },
                current =>
                    {
                        Assert.Equal("CurrentDate", current.Name);
                        Assert.Equal("datetime", current.StoreType);
                        Assert.False(current.IsNullable);
                        Assert.Null(current.DefaultValueSql);
                        Assert.Equal("(getdate())", current.ComputedColumnSql);
                    },
                sum =>
                    {
                        Assert.Equal("Sum", sum.Name);
                        Assert.Equal(ValueGenerated.OnAddOrUpdate, sum.ValueGenerated);
                    },
                modified =>
                    {
                        Assert.Equal("Modified", modified.Name);
                        Assert.Equal(ValueGenerated.OnAddOrUpdate, modified.ValueGenerated);
                        Assert.Equal("rowversion", modified.StoreType);
                    });
        }

        [Theory]
        [InlineData("nvarchar(55)")]
        [InlineData("varchar(341)")]
        [InlineData("nchar(14)")]
        [InlineData("char(89)")]
        [InlineData("varchar(max)")]
        public void It_reads_max_length(string type)
        {
            var sql = @"IF OBJECT_ID('dbo.Strings', 'U') IS NOT NULL
    DROP TABLE [dbo].[Strings];" +
                      "CREATE TABLE [dbo].[Strings] ( CharColumn " + type + ");";
            var db = CreateModel(sql, new List<string> { "Strings" });

            Assert.Equal(type, db.Tables.Single().Columns.Single().StoreType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void It_reads_identity(bool isIdentity)
        {
            var dbModel = CreateModel(
                @"IF OBJECT_ID('dbo.Identities', 'U') IS NOT NULL
    DROP TABLE [dbo].[Identities];
CREATE TABLE [dbo].[Identities] ( Id INT " + (isIdentity ? "IDENTITY(1,1)" : "") + ")",
                new List<string> { "Identities" });

            var column = Assert.Single(dbModel.Tables.Single().Columns);
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Equal(isIdentity ? ValueGenerated.OnAdd : default(ValueGenerated?), column.ValueGenerated);
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES K2 (A) );";

            var selectionSet = new List<string> { "K2" };

            var dbModel = CreateModel(sql, selectionSet);
            var table = Assert.Single(dbModel.Tables);
            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("K2", table.Name);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.UniqueConstraints.Count);
            Assert.Empty(table.ForeignKeys);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void It_reads_sequences()
        {
            var sql = @"CREATE SEQUENCE DefaultValues_read;

CREATE SEQUENCE CustomSequence_read
    AS numeric
    START WITH 1
    INCREMENT BY 2
    MAXVALUE 8
    MINVALUE -3
    CYCLE;";

            var dbModel = CreateModel(sql);
            Assert.Collection(
                dbModel.Sequences.Where(s => s.Name.EndsWith("_read", StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Name),
                c =>
                    {
                        Assert.Equal("CustomSequence_read", c.Name);
                        Assert.Equal("dbo", c.Schema);
                        Assert.Equal("numeric", c.StoreType);
                        Assert.Equal(1, c.StartValue);
                        Assert.Equal(2, c.IncrementBy);
                        Assert.Equal(8, c.MaxValue);
                        Assert.Equal(-3, c.MinValue);
                        Assert.True(c.IsCyclic);
                    },
                d =>
                    {
                        Assert.Equal("DefaultValues_read", d.Name);
                        Assert.Equal("dbo", d.Schema);
                        Assert.Equal("bigint", d.StoreType);
                        Assert.Equal(1, d.IncrementBy);
                        Assert.False(d.IsCyclic);
                        Assert.Null(d.MaxValue);
                        Assert.Null(d.MinValue);
                        Assert.Null(d.StartValue);
                    });
        }

        [Fact]
        public async Task It_reads_default_schema()
        {
            var defaultSchema = await _fixture.TestStore.ExecuteScalarAsync<string>("SELECT SCHEMA_NAME()");

            var model = _fixture.CreateModel("SELECT 1");
            Assert.Equal(defaultSchema, model.DefaultSchema);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void SequenceModel_values_null_for_default_min_max_start()
        {
            var sql = @"CREATE SEQUENCE [TinyIntSequence_defaults]
    AS tinyint;
CREATE SEQUENCE [SmallIntSequence_defaults]
    AS smallint;
CREATE SEQUENCE [IntSequence_defaults]
    AS int;
CREATE SEQUENCE [DecimalSequence_defaults]
    AS decimal;
CREATE SEQUENCE [NumericSequence_defaults]
    AS numeric;";
            var dbModel = CreateModel(sql);
            Assert.All(
                dbModel.Sequences.Where(s => s.Name.EndsWith("_defaults", StringComparison.OrdinalIgnoreCase)), s =>
                    {
                        Assert.Null(s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    });
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthBinaryColumns").Columns;

                    Assert.Equal("binary(8000)", columns.Single(c => c.Name == "binaryColumn").StoreType);
                    Assert.Equal("varbinary(8000)", columns.Single(c => c.Name == "binaryVaryingColumn").StoreType);
                    Assert.Equal("varbinary(8000)", columns.Single(c => c.Name == "varbinaryColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthBinaryColumns;");
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthCharColumns").Columns;

                    Assert.Equal("char(8000)", columns.Single(c => c.Name == "charColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthCharColumns;");
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthCharColumns").Columns;

                    Assert.Equal("char(8000)", columns.Single(c => c.Name == "characterColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthCharColumns;");
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthVarcharColumns").Columns;

                    Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "charVaryingColumn").StoreType);
                    Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "characterVaryingColumn").StoreType);
                    Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "varcharColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthVarcharColumns;");
        }

        [Fact]
        public void Default_max_length_are_added_to_nchar_1()
        {
            Test(
                @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    natioanlCharColumn national char(4000),
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthNcharColumns").Columns;

                    Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "natioanlCharColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthNcharColumns;");
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthNcharColumns").Columns;

                    Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "nationalCharacterColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthNcharColumns;");
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthNcharColumns").Columns;

                    Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "ncharColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthNcharColumns;");
        }

        [Fact]
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
                    var columns = dbModel.Tables.Single(e => e.Name == "DefaultRequiredLengthNvarcharColumns").Columns;

                    Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nationalCharVaryingColumn").StoreType);
                    Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nationalCharacterVaryingColumn").StoreType);
                    Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nvarcharColumn").StoreType);
                },
                @"DROP TABLE DefaultRequiredLengthNvarcharColumns;");
        }

        private readonly List<Tuple<LogLevel, EventId, string>> Log = new List<Tuple<LogLevel, EventId, string>>();

        private void Test(string createSql, IEnumerable<string> tables, IEnumerable<string> schemas, Action<DatabaseModel> asserter, string cleanupSql)
        {
            _fixture.TestStore.ExecuteNonQuery(createSql);

            try
            {
                var databaseModelFactory = new SqlServerDatabaseModelFactory(
                    new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                        new ListLoggerFactory(Log),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake")));

                var databaseModel = databaseModelFactory.Create(_fixture.TestStore.ConnectionString, tables, schemas);
                Assert.NotNull(databaseModel);
                asserter(databaseModel);
            }
            finally
            {
                if (!string.IsNullOrEmpty(cleanupSql))
                {
                    _fixture.TestStore.ExecuteNonQuery(cleanupSql);
                }
            }
        }

        private readonly SqlServerDatabaseModelFixture _fixture;

        public DatabaseModel CreateModel(string createSql, IEnumerable<string> tables = null)
            => _fixture.CreateModel(createSql, tables);

        public SqlServerDatabaseModelFactoryTest(SqlServerDatabaseModelFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
