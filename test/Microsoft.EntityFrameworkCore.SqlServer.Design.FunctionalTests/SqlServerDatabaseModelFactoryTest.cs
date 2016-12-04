// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.FunctionalTests
{
    public class SqlServerDatabaseModelFactoryTest : IClassFixture<SqlServerDatabaseModelFixture>
    {
        [Fact]
        public void It_reads_tables()
        {
            var sql = @"
CREATE TABLE [dbo].[Everest] ( id int );
CREATE TABLE [dbo].[Denali] ( id int );";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Everest", "Denali" }));

            Assert.Collection(dbModel.Tables.OrderBy(t => t.Name),
                d =>
                    {
                        Assert.Equal("dbo", d.SchemaName);
                        Assert.Equal("Denali", d.Name);
                    },
                e =>
                    {
                        Assert.Equal("dbo", e.SchemaName);
                        Assert.Equal("Everest", e.Name);
                    });
        }

        [Fact]
        public void It_filters_views()
        {
            _fixture.ExecuteNonQuery(@"CREATE TABLE [dbo].[Hills] (Id int PRIMARY KEY, Name nvarchar(100), Height int);");
            _fixture.ExecuteNonQuery(@"CREATE VIEW [dbo].[ShortHills] WITH SCHEMABINDING AS SELECT Name FROM [dbo].[Hills] WHERE Height < 100;");
            var sql = "CREATE UNIQUE CLUSTERED INDEX IX_ShortHills_Names ON ShortHills (Name);";

            var selectionSet = new TableSelectionSet(new List<string> { "Hills", "ShortHills" });
            var logger = new SqlServerDatabaseModelFixture.TestLogger();

            var dbModel = CreateModel(sql, selectionSet, logger);
            Assert.Single(dbModel.Tables);
            Assert.DoesNotContain(logger.Items, i => i.logLevel == LogLevel.Warning);
        }

        [Fact]
        public void It_reads_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db2");
            var sql = "CREATE TABLE dbo.Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE db2.Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES Ranges(Id) ON DELETE CASCADE)";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Ranges", "Mountains" }));

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("db2", fk.Table.SchemaName);
            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.SchemaName);
            Assert.Equal("Ranges", fk.PrincipalTable.Name);
            Assert.Equal("RangeId", fk.Columns.Single().Column.Name);
            Assert.Equal("Id", fk.Columns.Single().PrincipalColumn.Name);
            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
        }

        [Fact]
        public void It_reads_composite_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db3");
            var sql = "CREATE TABLE dbo.Ranges1 ( Id INT IDENTITY (1,1), AltId INT, PRIMARY KEY(Id, AltId));" +
                      "CREATE TABLE db3.Mountains1 ( RangeId INT NOT NULL, RangeAltId INT NOT NULL, FOREIGN KEY (RangeId, RangeAltId) REFERENCES Ranges1(Id, AltId) ON DELETE NO ACTION)";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Ranges1", "Mountains1" }));

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("db3", fk.Table.SchemaName);
            Assert.Equal("Mountains1", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.SchemaName);
            Assert.Equal("Ranges1", fk.PrincipalTable.Name);
            Assert.Equal(new[] { "RangeId", "RangeAltId" }, fk.Columns.Select(c => c.Column.Name).ToArray());
            Assert.Equal(new[] { "Id", "AltId" }, fk.Columns.Select(c => c.PrincipalColumn.Name).ToArray());
            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Place ( Id int PRIMARY KEY NONCLUSTERED, Name int UNIQUE, Location int);" +
                      "CREATE CLUSTERED INDEX IX_Location_Name ON Place (Location, Name);" +
                      "CREATE NONCLUSTERED INDEX IX_Location ON Place (Location);";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Place" }));

            var indexes = dbModel.Tables.Single().Indexes;

            Assert.All(indexes, c =>
                {
                    Assert.Equal("dbo", c.Table.SchemaName);
                    Assert.Equal("Place", c.Table.Name);
                });

            Assert.Collection(indexes.OrderBy(i => i.Name),
                nonClustered =>
                    {
                        Assert.Equal("IX_Location", nonClustered.Name);
                        Assert.False(nonClustered.SqlServer().IsClustered);
                        Assert.Equal("Location", nonClustered.IndexColumns.Select(ic => ic.Column.Name).Single());
                    },
                clusteredIndex =>
                    {
                        Assert.Equal("IX_Location_Name", clusteredIndex.Name);
                        Assert.False(clusteredIndex.IsUnique);
                        Assert.True(clusteredIndex.SqlServer().IsClustered);
                        Assert.Equal(new List<string> { "Location", "Name" }, clusteredIndex.IndexColumns.Select(ic => ic.Column.Name).ToList());
                        Assert.Equal(new List<int> { 1, 2 }, clusteredIndex.IndexColumns.Select(ic => ic.Ordinal).ToList());
                    },
                pkIndex =>
                    {
                        Assert.StartsWith("PK__Place", pkIndex.Name);
                        Assert.True(pkIndex.IsUnique);
                        Assert.False(pkIndex.SqlServer().IsClustered);
                        Assert.Equal(new List<string> { "Id" }, pkIndex.IndexColumns.Select(ic => ic.Column.Name).ToList());
                    },
                unique =>
                    {
                        Assert.True(unique.IsUnique);
                        Assert.Equal("Name", unique.IndexColumns.Single().Column.Name);
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
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "MountainsColumns" }));

            var columns = dbModel.Tables.Single().Columns.OrderBy(c => c.Ordinal);

            Assert.All(columns, c =>
                {
                    Assert.Equal("dbo", c.Table.SchemaName);
                    Assert.Equal("MountainsColumns", c.Table.Name);
                });

            Assert.Collection(columns,
                id =>
                    {
                        Assert.Equal("Id", id.Name);
                        Assert.Equal("int", id.DataType);
                        Assert.Equal(2, id.PrimaryKeyOrdinal);
                        Assert.False(id.IsNullable);
                        Assert.Equal(0, id.Ordinal);
                        Assert.Null(id.DefaultValue);
                    },
                name =>
                    {
                        Assert.Equal("Name", name.Name);
                        Assert.Equal("nvarchar", name.DataType);
                        Assert.Equal(1, name.PrimaryKeyOrdinal);
                        Assert.False(name.IsNullable);
                        Assert.Equal(1, name.Ordinal);
                        Assert.Null(name.DefaultValue);
                        Assert.Equal(100, name.MaxLength);
                    },
                lat =>
                    {
                        Assert.Equal("Latitude", lat.Name);
                        Assert.Equal("decimal", lat.DataType);
                        Assert.Null(lat.PrimaryKeyOrdinal);
                        Assert.True(lat.IsNullable);
                        Assert.Equal(2, lat.Ordinal);
                        Assert.Equal("((0.0))", lat.DefaultValue);
                        Assert.Equal(5, lat.Precision);
                        Assert.Equal(2, lat.Scale);
                        Assert.Null(lat.MaxLength);
                        Assert.Null(lat.SqlServer().DateTimePrecision);
                    },
                created =>
                    {
                        Assert.Equal("Created", created.Name);
                        Assert.Null(created.Scale);
                        Assert.Equal(6, created.SqlServer().DateTimePrecision);
                        Assert.Equal("('October 20, 2015 11am')", created.DefaultValue);
                    },
                discovered =>
                    {
                        Assert.Equal("DiscoveredDate", discovered.Name);
                        Assert.Equal(7, discovered.SqlServer().DateTimePrecision);
                    },
                current =>
                    {
                        Assert.Equal("CurrentDate", current.Name);
                        Assert.Equal("datetime", current.DataType);
                        Assert.Equal("(getdate())", current.ComputedValue);
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
                        Assert.Equal("timestamp", modified.DataType); // intentional - testing the alias
                    });
        }

        [Theory]
        [InlineData("nvarchar(55)", 55)]
        [InlineData("varchar(341)", 341)]
        [InlineData("nchar(14)", 14)]
        [InlineData("char(89)", 89)]
        [InlineData("varchar(max)", null)]
        public void It_reads_max_length(string type, int? length)
        {
            var sql = @"IF OBJECT_ID('dbo.Strings', 'U') IS NOT NULL
    DROP TABLE [dbo].[Strings];" +
                      "CREATE TABLE [dbo].[Strings] ( CharColumn " + type + ");";
            var db = CreateModel(sql, new TableSelectionSet(new List<string> { "Strings" }));

            Assert.Equal(length, db.Tables.Single().Columns.Single().MaxLength);
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
                new TableSelectionSet(new List<string> { "Identities" }));

            var column = Assert.Single(dbModel.Tables.Single().Columns);
            Assert.Equal(isIdentity, column.SqlServer().IsIdentity);
            Assert.Equal(isIdentity ? ValueGenerated.OnAdd : default(ValueGenerated?), column.ValueGenerated);
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES K2 (A) );";

            var selectionSet = new TableSelectionSet(new List<string> { "K2" });

            var dbModel = CreateModel(sql, selectionSet);
            var table = Assert.Single(dbModel.Tables);
            Assert.Equal("K2", table.Name);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.Indexes.Count);
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
            Assert.Collection(dbModel.Sequences.Where(s => s.Name.EndsWith("_read")).OrderBy(s => s.Name),
                c =>
                    {
                        Assert.Equal(c.Name, "CustomSequence_read");
                        Assert.Equal(c.SchemaName, "dbo");
                        Assert.Equal(c.DataType, "numeric");
                        Assert.Equal(1, c.Start);
                        Assert.Equal(2, c.IncrementBy);
                        Assert.Equal(8, c.Max);
                        Assert.Equal(-3, c.Min);
                        Assert.True(c.IsCyclic);
                    },
                d =>
                    {
                        Assert.Equal(d.Name, "DefaultValues_read");
                        Assert.Equal(d.SchemaName, "dbo");
                        Assert.Equal(d.DataType, "bigint");
                        Assert.Equal(1, d.IncrementBy);
                        Assert.False(d.IsCyclic);
                        Assert.Null(d.Max);
                        Assert.Null(d.Min);
                        Assert.Null(d.Start);
                    });
        }

        [Fact]
        public async Task It_reads_default_schema()
        {
            var defaultSchema = await _fixture.TestStore.ExecuteScalarAsync<string>("SELECT SCHEMA_NAME()");

            var model = _fixture.CreateModel("SELECT 1");
            Assert.Equal(defaultSchema, model.DefaultSchemaName);
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
            Assert.All(dbModel.Sequences.Where(s => s.Name.EndsWith("_defaults")), s =>
                {
                    Assert.Null(s.Start);
                    Assert.Null(s.Min);
                    Assert.Null(s.Max);
                });
        }

        private readonly SqlServerDatabaseModelFixture _fixture;

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null, ILogger logger = null)
            => _fixture.CreateModel(createSql, selection, logger);

        public SqlServerDatabaseModelFactoryTest(SqlServerDatabaseModelFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
