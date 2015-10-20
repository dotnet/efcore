// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Design.FunctionalTests
{
    public class SqlServerMetadataReaderTest : IDisposable
    {
        [Fact]
        public void It_reads_tables()
        {
            var sql = @"
CREATE TABLE [dbo].[Everest] ( id int );
CREATE TABLE [dbo].[Denali] ( id int );";
            var dbInfo = GetDatabaseInfo(sql);

            Assert.Collection(dbInfo.Tables,
                e =>
                    {
                        Assert.Equal("dbo", e.SchemaName);
                        Assert.Equal("Everest", e.Name);
                    },
                d =>
                    {
                        Assert.Equal("dbo", d.SchemaName);
                        Assert.Equal("Denali", d.Name);
                    });
        }

        [Fact]
        public void It_reads_foreign_keys()
        {
            _testStore.ExecuteNonQuery("CREATE SCHEMA db2");
            var sql = "CREATE TABLE dbo.Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE db2.Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES Ranges(Id) ON DELETE CASCADE)";
            var dbInfo = GetDatabaseInfo(sql);

            var fk = Assert.Single(dbInfo.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("db2", fk.Table.SchemaName);
            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.SchemaName);
            Assert.Equal("Ranges", fk.PrincipalTable.Name);
            Assert.Equal("RangeId", fk.From.Single().Name);
            Assert.Equal("Id", fk.To.Single().Name);
            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
        }

        [Fact]
        public void It_reads_composite_foreign_keys()
        {
            _testStore.ExecuteNonQuery("CREATE SCHEMA db2");
            var sql = "CREATE TABLE dbo.Ranges ( Id INT IDENTITY (1,1), AltId INT, PRIMARY KEY(Id, AltId));" +
                      "CREATE TABLE db2.Mountains ( RangeId INT NOT NULL, RangeAltId INT NOT NULL, FOREIGN KEY (RangeId, RangeAltId) REFERENCES Ranges(Id, AltId) ON DELETE NO ACTION)";
            var dbInfo = GetDatabaseInfo(sql);

            var fk = Assert.Single(dbInfo.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("db2", fk.Table.SchemaName);
            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("dbo", fk.PrincipalTable.SchemaName);
            Assert.Equal("Ranges", fk.PrincipalTable.Name);
            Assert.Equal(new[] { "RangeId", "RangeAltId" }, fk.From.Select(c => c.Name).ToArray());
            Assert.Equal(new[] { "Id", "AltId" }, fk.To.Select(c => c.Name).ToArray());
            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Ranges ( Name int UNIQUE, Location int );" +
                      "CREATE INDEX loc_idx ON Ranges (Location, Name);";
            var dbInfo = GetDatabaseInfo(sql);

            var indexes = dbInfo.Tables.Single().Indexes;

            Assert.All(indexes, c =>
                {
                    Assert.Equal("dbo", c.Table.SchemaName);
                    Assert.Equal("Ranges", c.Table.Name);
                });

            Assert.Collection(indexes,
                index =>
                    {
                        Assert.Equal("loc_idx", index.Name);
                        Assert.False(index.IsUnique);
                        Assert.Equal(new List<string> { "Location", "Name" }, index.Columns.Select(c => c.Name).ToList());
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
CREATE TABLE [dbo].[Mountains] (
    Id int,
    Name nvarchar(100) NOT NULL,
    Latitude decimal( 5, 2 ) DEFAULT 0.0,
    Created datetime2(6),
    Sum AS Latitude + 1.0,
    Modified rowversion,
    Primary Key (Name, Id)
);";
            var dbInfo = GetDatabaseInfo(sql);

            var columns = dbInfo.Tables.Single().Columns.OrderBy(c => c.Ordinal);

            Assert.All(columns, c =>
                {
                    Assert.Equal("dbo", c.Table.SchemaName);
                    Assert.Equal("Mountains", c.Table.Name);
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
                    },
                created =>
                    {
                        Assert.Equal("Created", created.Name);
                        Assert.Equal(6, created.Scale);
                    },
                sum =>
                    {
                        Assert.Equal("Sum", sum.Name);
                        Assert.True(sum.IsComputed);
                    },
                modified =>
                    {
                        Assert.Equal("Modified", modified.Name);
                        Assert.True(modified.IsComputed);
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
            var sql = "CREATE TABLE [dbo].[Mountains] ( CharColumn " + type + ");";
            var db = GetDatabaseInfo(sql);

            Assert.Equal(length, db.Tables.Single().Columns.Single().MaxLength);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void It_reads_identity(bool isIdentity)
        {
            var dbInfo = GetDatabaseInfo(@"CREATE TABLE [dbo].[Mountains] ( Id INT " + (isIdentity ? "IDENTITY(1,1)" : "") + ")");

            Assert.Equal(isIdentity, dbInfo.Tables.Single().Columns.Single().IsIdentity.Value);
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE [dbo].[Kilimanjaro] ( Id int,B varchar, UNIQUE (B ), FOREIGN KEY (B) REFERENCES K2 (A) );";

            var selectionSet = new TableSelectionSet
            {
                Tables = { "K2" }
            };

            var dbInfo = GetDatabaseInfo(sql, selectionSet);
            var table = Assert.Single(dbInfo.Tables);
            Assert.Equal("K2", table.Name);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.Indexes.Count);
            Assert.Empty(table.ForeignKeys);
        }

        public SchemaInfo GetDatabaseInfo(string createSql, TableSelectionSet selection = null)
        {
            _testStore.ExecuteNonQuery(createSql);

            var reader = new SqlServerMetadataReader();

            return reader.GetSchema(_testStore.Connection.ConnectionString, selection ?? TableSelectionSet.InclusiveAll);
        }

        private readonly SqlServerTestStore _testStore;

        public SqlServerMetadataReaderTest()
        {
            _testStore = SqlServerTestStore.CreateScratch();
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
