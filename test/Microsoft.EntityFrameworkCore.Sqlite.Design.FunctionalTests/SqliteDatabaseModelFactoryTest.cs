// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Design.FunctionalTests
{
    public class SqliteDatabaseModelFactoryTest : IDisposable
    {
        private readonly SqliteTestStore _testStore;
        private readonly SqliteDatabaseModelFactory _factory;

        public SqliteDatabaseModelFactoryTest()
        {
            _testStore = SqliteTestStore.CreateScratch();

            var serviceCollection = new ServiceCollection().AddLogging();
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = new TestLogger();
            serviceProvider.GetService<ILoggerFactory>().AddProvider(new TestLoggerProvider(logger));

            _factory = serviceProvider
                .GetService<IDatabaseModelFactory>() as SqliteDatabaseModelFactory;
        }

        [Fact]
        public void It_reads_tables()
        {
            var sql = @"CREATE TABLE [Everest] ( id int );  CREATE TABLE [Denali] ( id int );";

            var dbModel = CreateModel(sql);

            Assert.Collection(dbModel.Tables.OrderBy(t => t.Name),
                d => Assert.Equal("Denali", d.Name),
                e => Assert.Equal("Everest", e.Name));
        }

        [Fact]
        public void It_reads_foreign_keys()
        {
            var sql = "CREATE TABLE Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES Ranges(Id) ON DELETE CASCADE)";

            var dbModel = CreateModel(sql);

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("Ranges", fk.PrincipalTable.Name);
            Assert.Equal(0, fk.Columns.Single().Ordinal);
            Assert.Equal("RangeId", fk.Columns.Single().Column.Name);
            Assert.Equal("Id", fk.Columns.Single().PrincipalColumn.Name);
            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
        }

        [Fact]
        public void It_reads_composite_foreign_keys()
        {
            var sql = "CREATE TABLE Ranges ( Id INT IDENTITY (1,1), AltId INT, PRIMARY KEY(Id, AltId));" +
                      "CREATE TABLE Mountains ( RangeId INT NOT NULL, RangeAltId INT NOT NULL, FOREIGN KEY (RangeId, RangeAltId) " +
                      " REFERENCES Ranges(Id, AltId) ON DELETE NO ACTION)";
            var dbModel = CreateModel(sql);

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("Mountains", fk.Table.Name);
            Assert.Equal("Ranges", fk.PrincipalTable.Name);
            Assert.Equal(0, fk.Columns.First().Ordinal);
            Assert.Equal(1, fk.Columns.Last().Ordinal);
            Assert.Equal(new[] { "RangeId", "RangeAltId" }, fk.Columns.Select(c => c.Column.Name).ToArray());
            Assert.Equal(new[] { "Id", "AltId" }, fk.Columns.Select(c => c.PrincipalColumn.Name).ToArray());
            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Place ( Id int PRIMARY KEY, Name int UNIQUE, Location int);" +
                      "CREATE INDEX IX_Location_Name ON Place (Location, Name);";

            var dbModel = CreateModel(sql);

            var indexes = dbModel.Tables.Single().Indexes;

            Assert.All(indexes, c => { Assert.Equal("Place", c.Table.Name); });

            Assert.Collection(indexes.OrderBy(i => i.Name),
                index =>
                    {
                        Assert.Equal("IX_Location_Name", index.Name);
                        Assert.False(index.IsUnique);
                        Assert.Equal(new List<string> { "Location", "Name" }, index.IndexColumns.Select(ic => ic.Column.Name).ToList());
                        Assert.Equal(new List<int> { 0, 1 }, index.IndexColumns.Select(ic => ic.Ordinal).ToList());
                    },
                pkIndex =>
                    {
                        Assert.True(pkIndex.IsUnique);
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
CREATE TABLE [MountainsColumns] (
    Id integer primary key,
    Name string NOT NULL,
    Latitude numeric DEFAULT 0.0,
    Created datetime DEFAULT('October 20, 2015 11am')
);";
            var dbModel = CreateModel(sql);

            var columns = dbModel.Tables.Single().Columns.OrderBy(c => c.Ordinal);

            Assert.All(columns, c => { Assert.Equal("MountainsColumns", c.Table.Name); });

            Assert.Collection(columns,
                id =>
                    {
                        Assert.Equal("Id", id.Name);
                        Assert.Equal("integer", id.DataType);
                        Assert.Equal(1, id.PrimaryKeyOrdinal);
                        Assert.False(id.IsNullable);
                        Assert.Equal(0, id.Ordinal);
                        Assert.Null(id.DefaultValue);
                    },
                name =>
                    {
                        Assert.Equal("Name", name.Name);
                        Assert.Equal("string", name.DataType);
                        Assert.Null(name.PrimaryKeyOrdinal);
                        Assert.False(name.IsNullable);
                        Assert.Equal(1, name.Ordinal);
                        Assert.Null(name.DefaultValue);
                    },
                lat =>
                    {
                        Assert.Equal("Latitude", lat.Name);
                        Assert.Equal("numeric", lat.DataType);
                        Assert.Null(lat.PrimaryKeyOrdinal);
                        Assert.True(lat.IsNullable);
                        Assert.Equal(2, lat.Ordinal);
                        Assert.Equal("0.0", lat.DefaultValue);
                        Assert.Null(lat.Precision);
                        Assert.Null(lat.Scale);
                        Assert.Null(lat.MaxLength);
                    },
                created =>
                    {
                        Assert.Equal("Created", created.Name);
                        Assert.Equal("datetime", created.DataType);
                        Assert.Equal("'October 20, 2015 11am'", created.DefaultValue);
                    });
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE [K2] ( Id int);
CREATE TABLE [Kilimanjaro] ( Id int);";

            var selectionSet = new TableSelectionSet(new List<string> { "K2" });

            var dbModel = CreateModel(sql, selectionSet);
            var table = Assert.Single(dbModel.Tables);
            Assert.Equal("K2", table.Name);
        }

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null)
        {
            _testStore.ExecuteNonQuery(createSql);

            return _factory.Create(_testStore.ConnectionString, selection ?? TableSelectionSet.All);
        }

        public void Dispose() => _testStore.Dispose();
    }
}
