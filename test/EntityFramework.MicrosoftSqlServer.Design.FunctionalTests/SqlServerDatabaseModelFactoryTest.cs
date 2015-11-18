// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Scaffolding;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Design.FunctionalTests
{
    public class SqlServerDatabaseModelFactoryTest : IClassFixture<SqlServerDatabaseModelFixture>
    {
        [Fact]
        public void It_reads_tables()
        {
            var sql = @"
CREATE TABLE [dbo].[Everest] ( id int );
CREATE TABLE [dbo].[Denali] ( id int );";
            var dbInfo = CreateModel(sql, new TableSelectionSet(new List<string> { "Everest", "Denali" }));

            Assert.Collection(dbInfo.Tables.OrderBy(t => t.Name),
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
        public void It_reads_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db2");
            var sql = "CREATE TABLE dbo.Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE db2.Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES Ranges(Id) ON DELETE CASCADE)";
            var dbInfo = CreateModel(sql, new TableSelectionSet(new List<string> { "Ranges", "Mountains" }));

            var fk = Assert.Single(dbInfo.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("db2", fk.Table.SchemaName);
            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.SchemaName);
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
            var dbInfo = CreateModel(sql, new TableSelectionSet(new List<string> { "Ranges1", "Mountains1" }));

            var fk = Assert.Single(dbInfo.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("db3", fk.Table.SchemaName);
            Assert.Equal("Mountains1", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.SchemaName);
            Assert.Equal("Ranges1", fk.PrincipalTable.Name);
            Assert.Equal(new[] { "RangeId", "RangeAltId" }, fk.Columns.Select(c => c.Name).ToArray());
            Assert.Equal(new[] { "Id", "AltId" }, fk.PrincipalColumns.Select(c => c.Name).ToArray());
            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Place ( Id int PRIMARY KEY NONCLUSTERED, Name int UNIQUE, Location int );" +
                      "CREATE CLUSTERED INDEX IX_Location_Name ON Place (Location, Name);" +
                      "CREATE NONCLUSTERED INDEX IX_Location ON Place (Location);";
            var dbInfo = CreateModel(sql, new TableSelectionSet(new List<string> { "Place" }));

            var indexes = dbInfo.Tables.Single().Indexes;

            Assert.All(indexes, c =>
                {
                    Assert.Equal("dbo", c.Table.SchemaName);
                    Assert.Equal("Place", c.Table.Name);
                });

            Assert.Collection(indexes.OfType<SqlServerIndexModel>(),
                nonClustered =>
                    {
                        Assert.Equal("IX_Location", nonClustered.Name);
                        Assert.False(nonClustered.IsClustered);
                        Assert.Equal("Location", nonClustered.Columns.Select(c => c.Name).Single());
                    },
                clusteredIndex =>
                    {
                        Assert.Equal("IX_Location_Name", clusteredIndex.Name);
                        Assert.False(clusteredIndex.IsUnique);
                        Assert.True(clusteredIndex.IsClustered);
                        Assert.Equal(new List<string> { "Location", "Name" }, clusteredIndex.Columns.Select(c => c.Name).ToList());
                    },
                unique =>
                    {
                        Assert.True(unique.IsUnique);
                        Assert.Equal("Name", unique.Columns.Single().Name);
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
    Sum AS Latitude + 1.0,
    Modified rowversion,
    Primary Key (Name, Id)
);";
            var dbInfo = CreateModel(sql, new TableSelectionSet(new List<string> { "MountainsColumns" }));

            var columns = dbInfo.Tables.Single().Columns.OrderBy(c => c.Ordinal);

            Assert.All(columns, c =>
                {
                    Assert.Equal("dbo", c.Table.SchemaName);
                    Assert.Equal("MountainsColumns", c.Table.Name);
                });

            Assert.Collection(columns.OfType<SqlServerColumnModel>(),
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
                    },
                created =>
                    {
                        Assert.Equal("Created", created.Name);
                        Assert.Null(created.Scale);
                        Assert.Equal(6, created.DateTimePrecision);
                        Assert.Equal("('October 20, 2015 11am')", created.DefaultValue);
                    },
                discovered =>
                     {
                         Assert.Equal("DiscoveredDate", discovered.Name);
                         Assert.Equal(7, discovered.DateTimePrecision);
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
            var dbInfo = CreateModel(
                @"IF OBJECT_ID('dbo.Identities', 'U') IS NOT NULL 
    DROP TABLE [dbo].[Identities];
CREATE TABLE [dbo].[Identities] ( Id INT " + (isIdentity ? "IDENTITY(1,1)" : "") + ")",
                new TableSelectionSet(new List<string> { "Identities" }));

            var column = Assert.IsType<SqlServerColumnModel>(Assert.Single(dbInfo.Tables.Single().Columns));
            Assert.Equal(isIdentity, column.IsIdentity);
            Assert.Equal(isIdentity ? ValueGenerated.OnAdd : default(ValueGenerated?), column.ValueGenerated);
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES K2 (A) );";

            var selectionSet = new TableSelectionSet(new List<string>{ "K2" });

            var dbInfo = CreateModel(sql, selectionSet);
            var table = Assert.Single(dbInfo.Tables);
            Assert.Equal("K2", table.Name);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.Indexes.Count);
            Assert.Empty(table.ForeignKeys);
        }

        private readonly SqlServerDatabaseModelFixture _fixture;

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null)
            => _fixture.CreateModel(createSql, selection);

        public SqlServerDatabaseModelFactoryTest(SqlServerDatabaseModelFixture fixture)
        {
            _fixture = fixture;
        }
    }

    public class SqlServerDatabaseModelFixture : IDisposable
    {
        private readonly SqlServerTestStore _testStore;

        public SqlServerDatabaseModelFixture()
        {
            _testStore = SqlServerTestStore.CreateScratch();
        }

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null)
        {
            _testStore.ExecuteNonQuery(createSql);

            var reader = new SqlServerDatabaseModelFactory(new LoggerFactory());

            return reader.Create(_testStore.Connection.ConnectionString, selection ?? TableSelectionSet.All);
        }

        public void ExecuteNonQuery(string sql) => _testStore.ExecuteNonQuery(sql);

        public void Dispose() => _testStore.Dispose();
    }
}
