// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Scaffolding;

#nullable disable

public class SqliteDatabaseModelFactoryTest : IClassFixture<SqliteDatabaseModelFactoryTest.SqliteDatabaseModelFixture>
{
    protected SqliteDatabaseModelFixture Fixture { get; }

    public SqliteDatabaseModelFactoryTest(SqliteDatabaseModelFixture fixture)
    {
        Fixture = fixture;
        Fixture.ListLoggerFactory.Clear();
    }

    private void Test(
        string createSql,
        IEnumerable<string> tables,
        IEnumerable<string> schemas,
        Action<DatabaseModel> asserter,
        string cleanupSql)
    {
        Fixture.TestStore.ExecuteNonQuery(createSql);

        try
        {
            // NOTE: You may need to update AddEntityFrameworkDesignTimeServices() too
            var services = new ServiceCollection()
                .AddSingleton<TypeMappingSourceDependencies>()
                .AddSingleton<RelationalTypeMappingSourceDependencies>()
                .AddSingleton<ValueConverterSelectorDependencies>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<LoggingDefinitions, SqliteLoggingDefinitions>()
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                .AddSingleton<IValueConverterSelector, ValueConverterSelector>()
                .AddSingleton<ILoggerFactory>(Fixture.ListLoggerFactory)
                .AddSingleton<IDbContextLogger, NullDbContextLogger>();

            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
            new SqliteNetTopologySuiteDesignTimeServices().ConfigureDesignTimeServices(services);

            var databaseModelFactory = services
                .BuildServiceProvider() // No scope validation; design services only resolved once
                .GetRequiredService<IDatabaseModelFactory>();

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

    #region FilteringSchemaTable

    [ConditionalFact]
    public void Filter_tables()
        => Test(
            @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
            new[] { "Everest" },
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var table = Assert.Single(dbModel.Tables);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("Everest", table.Name);
            },
            @"
DROP TABLE Everest;
DROP TABLE Denali;");

    [ConditionalFact]
    public void Filter_tables_is_case_insensitive()
        => Test(
            @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
            new[] { "eVeReSt" },
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var table = Assert.Single(dbModel.Tables);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("Everest", table.Name);
            },
            @"
DROP TABLE Everest;
DROP TABLE Denali;");

    #endregion

    #region Table

    [ConditionalFact]
    public void Create_tables()
        => Test(
            @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d => Assert.Equal("Denali", d.Name),
                    e => Assert.Equal("Everest", e.Name));
            },
            @"
DROP TABLE Everest;
DROP TABLE Denali;");

    [ConditionalFact]
    public void Create_columns()
        => Test(
            @"
CREATE TABLE MountainsColumns (
    Id integer primary key,
    Name text NOT NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Columns.Count);
                Assert.All(
                    table.Columns, c => Assert.Equal("MountainsColumns", c.Table.Name));

                Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                Assert.Single(table.Columns.Where(c => c.Name == "Name"));
            },
            "DROP TABLE MountainsColumns;");

    [ConditionalFact]
    public void Create_view_columns()
        => Test(
            @"
CREATE VIEW MountainsColumnsView
 AS
SELECT
 CAST(100 AS integer) AS Id,
 CAST('' AS text) AS Name;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var table = Assert.IsType<DatabaseView>(dbModel.Tables.Single());

                Assert.Equal(2, table.Columns.Count);
                Assert.Null(table.PrimaryKey);
                Assert.All(
                    table.Columns, c => Assert.Equal("MountainsColumnsView", c.Table.Name));

                Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                Assert.Single(table.Columns.Where(c => c.Name == "Name"));
            },
            "DROP VIEW MountainsColumnsView;");

    [ConditionalFact]
    public void Create_primary_key()
        => Test(
            "CREATE TABLE Place ( Id int PRIMARY KEY );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("Place", pk.Table.Name);
                Assert.Equal(
                    ["Id"], pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE Place;");

    [ConditionalFact]
    public void Create_unique_constraints()
        => Test(
            @"
CREATE TABLE Place (
    Id int PRIMARY KEY,
    Name int UNIQUE,
    Location int
);

CREATE INDEX IX_Location_Name ON Place (Location, Name);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("Place", uniqueConstraint.Table.Name);
                Assert.Equal(
                    ["Name"], uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE Place;");

    [ConditionalFact]
    public void Create_indexes()
        => Test(
            @"
CREATE TABLE IndexTable (
    Id int,
    Name int,
    IndexProperty int
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
                    table.Indexes, c => Assert.Equal("IndexTable", c.Table.Name));

                Assert.Single(table.Indexes.Where(c => c.Name == "IX_NAME"));
                Assert.Single(table.Indexes.Where(c => c.Name == "IX_INDEX"));
            },
            "DROP TABLE IndexTable;");

    [ConditionalFact]
    public void Create_foreign_keys()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE FirstDependent (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE
);

CREATE TABLE SecondDependent (
    Id int PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES PrincipalTable(Id) ON DELETE NO ACTION
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("FirstDependent", firstFk.Table.Name);
                Assert.Equal("PrincipalTable", firstFk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId"], firstFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id"], firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, firstFk.OnDelete);

                var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("SecondDependent", secondFk.Table.Name);
                Assert.Equal("PrincipalTable", secondFk.PrincipalTable.Name);
                Assert.Equal(
                    ["Id"], secondFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id"], secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);
            },
            @"
DROP TABLE SecondDependent;
DROP TABLE FirstDependent;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Create_composite_foreign_key_with_default_columns()
        => Test(
            @"
                    CREATE TABLE MinimalFKTest1 (
                        Id1 INTEGER,
                        Id2 INTEGER,
                        Id3 INTEGER,
                        PRIMARY KEY (Id2, Id3, Id1)
                    );

                    CREATE TABLE MinimalFKTest2 (
                        Id3 INTEGER,
                        Id2 INTEGER,
                        Id1 INTEGER,
                        FOREIGN KEY (Id3, Id1, Id2) REFERENCES MinimalFKTest1
                    )
                ",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                Assert.Equal(2, dbModel.Tables.Count);

                var table = dbModel.Tables.Single(t => t.Name == "MinimalFKTest2");

                var foreignKey = Assert.Single(table.ForeignKeys);
                Assert.Equal(new[] { "Id3", "Id1", "Id2" }, foreignKey.Columns.Select(c => c.Name));
                Assert.Equal("MinimalFKTest1", foreignKey.PrincipalTable.Name);
                Assert.Equal(new[] { "Id2", "Id3", "Id1" }, foreignKey.PrincipalColumns.Select(c => c.Name));
            },
            @"
                    DROP TABLE MinimalFKTest2;
                    DROP TABLE MinimalFKTest1;
                ");

    #endregion

    #region ColumnFacets

    [ConditionalFact]
    public void Column_storetype_is_set()
        => Test(
            @"
CREATE TABLE StoreType (
    IntegerProperty integer,
    RealProperty real,
    TextProperty text,
    BlobProperty blob,
    RandomProperty randomType
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("integer", columns.Single(c => c.Name == "IntegerProperty").StoreType, ignoreCase: true);
                Assert.Equal("real", columns.Single(c => c.Name == "RealProperty").StoreType, ignoreCase: true);
                Assert.Equal("text", columns.Single(c => c.Name == "TextProperty").StoreType, ignoreCase: true);
                Assert.Equal("blob", columns.Single(c => c.Name == "BlobProperty").StoreType, ignoreCase: true);
                Assert.Equal("randomType", columns.Single(c => c.Name == "RandomProperty").StoreType);
            },
            "DROP TABLE StoreType;");

    [ConditionalTheory]
    [InlineData("BIT", typeof(bool))]
    [InlineData("BIT(1)", typeof(bool))]
    [InlineData("BOOL", typeof(bool))]
    [InlineData("BOOLEAN", typeof(bool))]
    [InlineData("LOGICAL", typeof(bool))]
    [InlineData("YESNO", typeof(bool))]
    [InlineData("TINYINT", typeof(byte))]
    [InlineData("UINT8", typeof(byte))]
    [InlineData("UNSIGNEDINTEGER8", typeof(byte))]
    [InlineData("BYTE", typeof(byte))]
    [InlineData("SMALLINT", typeof(short))]
    [InlineData("INT16", typeof(short))]
    [InlineData("INTEGER16", typeof(short))]
    [InlineData("SHORT", typeof(short))]
    [InlineData("MEDIUMINT", typeof(int))]
    [InlineData("INT", typeof(int))]
    [InlineData("INT32", typeof(int))]
    [InlineData("INTEGER", typeof(int))]
    [InlineData("INTEGER32", typeof(int))]
    [InlineData("BIGINT", null)]
    [InlineData("INT64", null)]
    [InlineData("INTEGER64", null)]
    [InlineData("LONG", null)]
    [InlineData("TINYSINT", typeof(sbyte))]
    [InlineData("INT8", typeof(sbyte))]
    [InlineData("INTEGER8", typeof(sbyte))]
    [InlineData("SBYTE", typeof(sbyte))]
    [InlineData("SMALLUINT", typeof(ushort))]
    [InlineData("UINT16", typeof(ushort))]
    [InlineData("UNSIGNEDINTEGER16", typeof(ushort))]
    [InlineData("USHORT", typeof(ushort))]
    [InlineData("MEDIUMUINT", typeof(uint))]
    [InlineData("UINT", typeof(uint))]
    [InlineData("UINT32", typeof(uint))]
    [InlineData("UNSIGNEDINTEGER32", typeof(uint))]
    [InlineData("BIGUINT", typeof(ulong))]
    [InlineData("UINT64", typeof(ulong))]
    [InlineData("UNSIGNEDINTEGER", typeof(ulong))]
    [InlineData("UNSIGNEDINTEGER64", typeof(ulong))]
    [InlineData("ULONG", typeof(ulong))]
    [InlineData("REAL", null)]
    [InlineData("DOUBLE", null)]
    [InlineData("FLOAT", null)]
    [InlineData("SINGLE", typeof(float))]
    [InlineData("TEXT", null)]
    [InlineData("NTEXT", null)]
    [InlineData("CHAR(1)", null)]
    [InlineData("NCHAR(1)", null)]
    [InlineData("VARCHAR(1)", null)]
    [InlineData("VARCHAR2(1)", null)]
    [InlineData("NVARCHAR(1)", null)]
    [InlineData("CLOB", null)]
    [InlineData("STRING", typeof(string))]
    [InlineData("JSON", typeof(string))]
    [InlineData("XML", typeof(string))]
    [InlineData("DATEONLY", typeof(DateOnly))]
    [InlineData("DATE", typeof(DateTime))]
    [InlineData("DATETIME", typeof(DateTime))]
    [InlineData("DATETIME2", typeof(DateTime))]
    [InlineData("SMALLDATE", typeof(DateTime))]
    [InlineData("TIMESTAMP(7)", typeof(DateTime))]
    [InlineData("DATETIMEOFFSET", typeof(DateTimeOffset))]
    [InlineData("CURRENCY", typeof(decimal))]
    [InlineData("DECIMAL(18, 0)", typeof(decimal))]
    [InlineData("MONEY", typeof(decimal))]
    [InlineData("SMALLMONEY", typeof(decimal))]
    [InlineData("NUMBER(18, 0)", typeof(decimal))]
    [InlineData("NUMERIC(18, 0)", typeof(decimal))]
    [InlineData("GUID", typeof(Guid))]
    [InlineData("UNIQUEIDENTIFIER", typeof(Guid))]
    [InlineData("UUID", typeof(Guid))]
    [InlineData("TIMEONLY", typeof(TimeOnly))]
    [InlineData("TIME(7)", typeof(TimeSpan))]
    [InlineData("TIMESPAN", typeof(TimeSpan))]
    [InlineData("BLOB", null)]
    [InlineData("BINARY(10)", null)]
    [InlineData("VARBINARY(10)", null)]
    [InlineData("IMAGE", null)]
    [InlineData("RAW(10)", null)]
    [InlineData("GEOMETRY", null)]
    [InlineData("GEOMETRYZ", null)]
    [InlineData("GEOMETRYM", null)]
    [InlineData("GEOMETRYZM", null)]
    [InlineData("GEOMETRYCOLLECTION", null)]
    [InlineData("GEOMETRYCOLLECTIONZ", null)]
    [InlineData("GEOMETRYCOLLECTIONM", null)]
    [InlineData("GEOMETRYCOLLECTIONZM", null)]
    [InlineData("LINESTRING", null)]
    [InlineData("LINESTRINGZ", null)]
    [InlineData("LINESTRINGM", null)]
    [InlineData("LINESTRINGZM", null)]
    [InlineData("MULTILINESTRING", null)]
    [InlineData("MULTILINESTRINGZ", null)]
    [InlineData("MULTILINESTRINGM", null)]
    [InlineData("MULTILINESTRINGZM", null)]
    [InlineData("MULTIPOINT", null)]
    [InlineData("MULTIPOINTZ", null)]
    [InlineData("MULTIPOINTM", null)]
    [InlineData("MULTIPOINTZM", null)]
    [InlineData("MULTIPOLYGON", null)]
    [InlineData("MULTIPOLYGONZ", null)]
    [InlineData("MULTIPOLYGONM", null)]
    [InlineData("MULTIPOLYGONZM", null)]
    [InlineData("POINT", null)]
    [InlineData("POINTZ", null)]
    [InlineData("POINTM", null)]
    [InlineData("POINTZM", null)]
    [InlineData("POLYGON", null)]
    [InlineData("POLYGONZ", null)]
    [InlineData("POLYGONM", null)]
    [InlineData("POLYGONZM", null)]
    public void Column_ClrType_is_set_when_no_data(string storeType, Type expected)
        => Test(
            $@"
CREATE TABLE ClrType (
    EmptyColumn {storeType}
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns);
                Assert.Equal(expected, (Type)column[ScaffoldingAnnotationNames.ClrType]);
            },
            "DROP TABLE ClrType");

    [ConditionalTheory]
    [InlineData("INTEGER", "1", typeof(int))]
    [InlineData("INTEGER", "2147483648", null)]
    [InlineData("BIT", "1", typeof(bool))]
    [InlineData("TINYINT", "1", typeof(byte))]
    [InlineData("SMALLINT", "1", typeof(short))]
    [InlineData("BIGINT", "1", null)]
    [InlineData("INT8", "1", typeof(sbyte))]
    [InlineData("UINT16", "1", typeof(ushort))]
    [InlineData("UINT", "1", typeof(uint))]
    [InlineData("UINT64", "1", typeof(ulong))]
    [InlineData("UINT64", "-1", typeof(ulong))]
    [InlineData("REAL", "0.1", null)]
    [InlineData("SINGLE", "0.1", typeof(float))]
    [InlineData("TEXT", "'A'", null)]
    [InlineData("TEXT", "'2023-01-20'", typeof(DateOnly))]
    [InlineData("TEXT", "'2023-01-20 13:37:00'", typeof(DateTime))]
    [InlineData("TEXT", "'2023-01-20 13:42:00-08:00'", typeof(DateTimeOffset))]
    [InlineData("TEXT", "'0.1'", typeof(decimal))]
    [InlineData("DECIMAL", "'0.1'", typeof(decimal))]
    [InlineData("TEXT", "'00000000-0000-0000-0000-000000000000'", typeof(Guid))]
    [InlineData("TEXT", "'13:44:00'", typeof(TimeSpan))]
    [InlineData("TIMEONLY", "'14:34:00'", typeof(TimeOnly))]
    [InlineData("BLOB", "x'01'", null)]
    [InlineData(
        "GEOMETRY",
        "x'00010000000000000000000000000000000000000000000000000000000000000000000000007C0100000000000000000000000000000000000000FE'",
        null)]
    [InlineData(
        "POINT",
        "x'00010000000000000000000000000000000000000000000000000000000000000000000000007C0100000000000000000000000000000000000000FE'",
        null)]
    public void Column_ClrType_is_set_when_data(string storeType, string value, Type expected)
        => Test(
            $@"
CREATE TABLE IF NOT EXISTS ClrTypeWithData (
    ColumnWithData {storeType}
);

INSERT INTO ClrTypeWithData VALUES ({value});",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns);
                Assert.Equal(expected, (Type)column[ScaffoldingAnnotationNames.ClrType]);
            },
            "DROP TABLE ClrTypeWithData");

    [ConditionalTheory]
    [InlineData("INTEGER", "0.1", typeof(double))]
    [InlineData("BIT", "2", typeof(int))]
    [InlineData("TINYINT", "-1", typeof(int))]
    [InlineData("TINYINT", "256", typeof(int))]
    [InlineData("SMALLINT", "32768", typeof(int))]
    [InlineData("MEDIUMINT", "2147483648", null)]
    [InlineData("INT8", "128", typeof(int))]
    [InlineData("UINT16", "-1", typeof(int))]
    [InlineData("UINT16", "65536", typeof(int))]
    [InlineData("UINT", "4294967296", null)]
    [InlineData("REAL", "'A'", null)]
    [InlineData("SINGLE", "3.402824E+38", typeof(double))]
    [InlineData("TEXT", "x'00'", typeof(byte[]))]
    [InlineData("DATE", "'A'", typeof(string))]
    [InlineData("DATEONLY", "'A'", typeof(string))]
    [InlineData("DATETIME", "'A'", typeof(string))]
    [InlineData("DATETIMEOFFSET", "'A'", typeof(string))]
    [InlineData("DECIMAL", "'A'", typeof(string))]
    [InlineData("DECIMAL", "0.1", typeof(decimal))]
    [InlineData("GUID", "'A'", typeof(string))]
    [InlineData("TIME", "'A'", typeof(string))]
    [InlineData("TIMEONLY", "'A'", typeof(string))]
    [InlineData("TIMEONLY", "'24:00:00'", typeof(TimeSpan))]
    [InlineData("BLOB", "1", null)]
    [InlineData("GEOMETRY", "1", null)]
    [InlineData("POINT", "1", null)]
    public void Column_ClrType_is_set_when_insane(string storeType, string value, Type expected)
        => Test(
            $@"
CREATE TABLE IF NOT EXISTS ClrTypeWithData (
    ColumnWithData {storeType}
);

INSERT INTO ClrTypeWithData VALUES ({value});",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns);
                Assert.Equal(expected, (Type)column[ScaffoldingAnnotationNames.ClrType]);
            },
            "DROP TABLE ClrTypeWithData");

    [ConditionalFact]
    public void Column_nullability_is_set()
        => Test(
            @"
CREATE TABLE Nullable (
    Id int,
    NullableInt int NULL,
    NonNullString text NOT NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.True(columns.Single(c => c.Name == "NullableInt").IsNullable);
                Assert.False(columns.Single(c => c.Name == "NonNullString").IsNullable);
            },
            "DROP TABLE Nullable;");

    [ConditionalFact]
    public void Column_default_value_is_set()
        => Test(
            @"
CREATE TABLE DefaultValue (
    Id int,
    SomeText text DEFAULT 'Something',
    RealColumn real DEFAULT 3.14,
    Created datetime DEFAULT('October 20, 2015 11am')
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("'Something'", columns.Single(c => c.Name == "SomeText").DefaultValueSql);
                Assert.Equal("3.14", columns.Single(c => c.Name == "RealColumn").DefaultValueSql);
                Assert.Equal("'October 20, 2015 11am'", columns.Single(c => c.Name == "Created").DefaultValueSql);
            },
            "DROP TABLE DefaultValue;");

    [ConditionalFact]
    public void Column_computed_column_sql_is_set()
        => Test(
            @"
CREATE TABLE ComputedColumnSql (
    Id int,
    GeneratedColumn AS (1 + 2),
    GeneratedColumnStored AS (1 + 2) STORED
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var generatedColumn = columns.Single(c => c.Name == "GeneratedColumn");
                Assert.NotNull(generatedColumn.ComputedColumnSql);
                Assert.Null(generatedColumn.IsStored);

                var generatedColumnStored = columns.Single(c => c.Name == "GeneratedColumnStored");
                Assert.NotNull(generatedColumnStored.ComputedColumnSql);
                Assert.True(generatedColumnStored.IsStored);
            },
            "DROP TABLE ComputedColumnSql;");

    [ConditionalFact]
    public void Simple_int_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A int DEFAULT -1,
    B int DEFAULT 0,
    C int DEFAULT (0),
    D int DEFAULT (-2),
    E int DEFAULT ( 2),
    F int DEFAULT (3 ),
    G int DEFAULT ((4)));

INSERT INTO MyTable VALUES (1, 1, 1, 1, 1, 1, 1, 1);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1", column.DefaultValueSql);
                Assert.Equal(-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal(0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal(0, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("-2", column.DefaultValueSql);
                Assert.Equal(-2, column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("2", column.DefaultValueSql);
                Assert.Equal(2, column.DefaultValue);

                column = columns.Single(c => c.Name == "F");
                Assert.Equal("3", column.DefaultValueSql);
                Assert.Equal(3, column.DefaultValue);

                column = columns.Single(c => c.Name == "G");
                Assert.Equal("(4)", column.DefaultValueSql);
                Assert.Equal(4, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_short_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A smallint DEFAULT -1,
    B smallint DEFAULT (0));

INSERT INTO MyTable VALUES (1, 1, 1);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1", column.DefaultValueSql);
                Assert.Equal((short)-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((short)0, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_long_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @$"
CREATE TABLE MyTable (
    Id int,
    A bigint DEFAULT -1,
    B bigint DEFAULT (0));

INSERT INTO MyTable VALUES (1, {long.MaxValue}, {long.MaxValue});",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1", column.DefaultValueSql);
                Assert.Equal((long)-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((long)0, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_byte_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A tinyint DEFAULT 1,
    B tinyint DEFAULT (0));

INSERT INTO MyTable VALUES (1, 1, 1);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("1", column.DefaultValueSql);
                Assert.Equal((byte)1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((byte)0, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_double_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A float DEFAULT -1.1111,
    B float DEFAULT (0.0),
    C float DEFAULT (1.1000000000000001e+000));

INSERT INTO MyTable VALUES (1, 1.1, 1.2, 1.3);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1.1111", column.DefaultValueSql);
                Assert.Equal(-1.1111, (double)column.DefaultValue, 3);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0.0", column.DefaultValueSql);
                Assert.Equal(0, (double)column.DefaultValue, 3);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("1.1000000000000001e+000", column.DefaultValueSql);
                Assert.Equal(1.1000000000000001e+000, (double)column.DefaultValue, 3);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_float_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A single DEFAULT -1.1111,
    B single DEFAULT (0.0),
    C single DEFAULT (1.1000000000000001e+000));

INSERT INTO MyTable VALUES (1, '1.1', '1.2', '1.3');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1.1111", column.DefaultValueSql);
                Assert.Equal((float)-1.1111, (float)column.DefaultValue, 0.01);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0.0", column.DefaultValueSql);
                Assert.Equal((float)0, (float)column.DefaultValue, 0.01);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("1.1000000000000001e+000", column.DefaultValueSql);
                Assert.Equal((float)1.1000000000000001e+000, (float)column.DefaultValue, 0.01);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_decimal_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A decimal DEFAULT '-1.1111',
    B decimal DEFAULT ('0.0'),
    C decimal DEFAULT ('0'));

INSERT INTO MyTable VALUES (1, '1.1', '1.2', '1.3');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'-1.1111'", column.DefaultValueSql);
                Assert.Equal((decimal)-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("'0.0'", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("'0'", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_bool_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A bit DEFAULT 0,
    B bit DEFAULT 1,
    C bit DEFAULT (0),
    D bit DEFAULT (1));

INSERT INTO MyTable VALUES (1, 1, 1, 1, 1);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("1", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("1", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateTime_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A datetime DEFAULT '1973-09-03T12:00:01.0020000',
    B datetime2 DEFAULT ('1968-10-23'));

INSERT INTO MyTable VALUES (1, '2023-01-20 13:37:00', '2023-01-20 13:37:00');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'1973-09-03T12:00:01.0020000'", column.DefaultValueSql);
                Assert.Equal(new DateTime(1973, 9, 3, 12, 0, 1, 2, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("'1968-10-23'", column.DefaultValueSql);
                Assert.Equal(new DateTime(1968, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Non_literal_or_non_parsable_DateTime_default_values_are_passed_through()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A datetime2 DEFAULT CURRENT_TIMESTAMP,
    B datetime DEFAULT CURRENT_DATE);

INSERT INTO MyTable VALUES (1, '2023-01-20 13:37:00', '2023-01-20 13:37:00');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CURRENT_TIMESTAMP", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CURRENT_DATE", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateOnly_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A date DEFAULT ('1968-10-23'),
    B date DEFAULT (('1973-09-03T01:02:03')));

INSERT INTO MyTable VALUES (1, '2023-01-20', '2023-01-20');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'1968-10-23'", column.DefaultValueSql);
                Assert.Equal(new DateOnly(1968, 10, 23), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("('1973-09-03T01:02:03')", column.DefaultValueSql);
                Assert.Equal(new DateOnly(1973, 9, 3), column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_TimeOnly_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A timeonly DEFAULT ('12:00:01.0020000'));

INSERT INTO MyTable VALUES (1, '13:37:00.0000000');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'12:00:01.0020000'", column.DefaultValueSql);
                Assert.Equal(new TimeOnly(12, 0, 1, 2), column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateTimeOffset_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A datetimeoffset DEFAULT ('1973-09-03T12:00:01.0000000+10:00'));

INSERT INTO MyTable VALUES (1, '1973-09-03 12:00:01.0000000+10:00');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'1973-09-03T12:00:01.0000000+10:00'", column.DefaultValueSql);
                Assert.Equal(
                    new DateTimeOffset(new DateTime(1973, 9, 3, 12, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 10, 0, 0, 0)),
                    column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_Guid_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A uniqueidentifier DEFAULT ('0E984725-C51C-4BF4-9960-E1C80E27ABA0'));

INSERT INTO MyTable VALUES (1, '993CDD7A-F4DF-4C5E-A810-8F51A11E9B6D');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'0E984725-C51C-4BF4-9960-E1C80E27ABA0'", column.DefaultValueSql);
                Assert.Equal(new Guid("0E984725-C51C-4BF4-9960-E1C80E27ABA0"), column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_string_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A nvarchar DEFAULT 'Hot',
    B varchar DEFAULT ('Buttered'),
    C character(100) DEFAULT (''),
    D text DEFAULT (''),
    E nvarchar(100) DEFAULT  ( ' Toast! '));

INSERT INTO MyTable VALUES (1, 'A', 'Tale', 'Of', 'Two', 'Cities');",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'Hot'", column.DefaultValueSql);
                Assert.Equal("Hot", column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("'Buttered'", column.DefaultValueSql);
                Assert.Equal("Buttered", column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("''", column.DefaultValueSql);
                Assert.Equal("", column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("''", column.DefaultValueSql);
                Assert.Equal("", column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("' Toast! '", column.DefaultValueSql);
                Assert.Equal(" Toast! ", column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Column_ValueGenerated_is_set(bool autoIncrement)
        => Test(
            $@"
                    CREATE TABLE AutoIncTest (
                        Id INTEGER PRIMARY KEY {(autoIncrement ? "AUTOINCREMENT" : null)}
                    )
                ",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var table = Assert.Single(dbModel.Tables);
                Assert.Equal("AutoIncTest", table.Name);

                var column = Assert.Single(table.Columns);
                Assert.Equal("Id", column.Name);
                Assert.Equal(
                    autoIncrement
                        ? ValueGenerated.OnAdd
                        : default(ValueGenerated?),
                    column.ValueGenerated);
            },
            "DROP TABLE AutoIncTest");

    [ConditionalFact]
    public void Column_collation_is_set()
        => Test(
            @"
CREATE TABLE ColumnsWithCollation (
    Id int,
    DefaultCollation text,
    NonDefaultCollation text COLLATE NOCASE
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Null(columns.Single(c => c.Name == "DefaultCollation").Collation);
                Assert.Equal("NOCASE", columns.Single(c => c.Name == "NonDefaultCollation").Collation);
            },
            "DROP TABLE ColumnsWithCollation;");

    #endregion

    #region PrimaryKeyFacets

    [ConditionalFact]
    public void Create_composite_primary_key()
        => Test(
            @"
CREATE TABLE CompositePrimaryKey (
    Id1 int,
    Id2 text,
    PRIMARY KEY ( Id2, Id1 )
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("CompositePrimaryKey", pk.Table.Name);
                Assert.Equal(
                    ["Id2", "Id1"], pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE CompositePrimaryKey;");

    [ConditionalFact]
    public void Create_primary_key_when_integer_primary_key_aliased_to_rowid()
        => Test(
            @"
CREATE TABLE RowidPrimaryKey (
    Id integer PRIMARY KEY
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("RowidPrimaryKey", pk.Table.Name);
                Assert.Equal(
                    ["Id"], pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE RowidPrimaryKey;");

    [ConditionalFact(Skip = "See issue#8802")]
    public void Set_name_for_primary_key()
        => Test(
            @"
CREATE TABLE PrimaryKeyName (
    Id int,
    CONSTRAINT PK PRIMARY KEY (Id)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PrimaryKeyName", pk.Table.Name);
                Assert.Equal("PK", pk.Name);
                Assert.Equal(
                    ["Id"], pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE PrimaryKeyName;");

    #endregion

    #region UniqueConstraintFacets

    [ConditionalFact]
    public void Create_composite_unique_constraint()
        => Test(
            @"
CREATE TABLE CompositeUniqueConstraint (
    Id1 int,
    Id2 text,
    UNIQUE ( Id2, Id1 )
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var constraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("CompositeUniqueConstraint", constraint.Table.Name);
                Assert.Equal(
                    ["Id2", "Id1"], constraint.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE CompositeUniqueConstraint;");

    [ConditionalFact(Skip = "See issue#8802")]
    public void Set_name_for_unique_constraint()
        => Test(
            @"
CREATE TABLE UniqueConstraintName (
    Id int,
    CONSTRAINT UK UNIQUE (Id)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var constraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("UniqueConstraintName", constraint.Table.Name);
                Assert.Equal("UK", constraint.Name);
                Assert.Equal(
                    ["Id"], constraint.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE UniqueConstraintName;");

    #endregion

    #region IndexFacets

    [ConditionalFact]
    public void Create_composite_index()
        => Test(
            @"
CREATE TABLE CompositeIndex (
    Id1 int,
    Id2 text
);

CREATE INDEX IX_COMPOSITE on CompositeIndex (Id2, Id1);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("CompositeIndex", index.Table.Name);
                Assert.Equal("IX_COMPOSITE", index.Name);
                Assert.Equal(
                    ["Id2", "Id1"], index.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE CompositeIndex;");

    [ConditionalFact]
    public void Set_unique_for_unique_index()
        => Test(
            @"
CREATE TABLE UniqueIndex (
    Id1 int,
    Id2 text
);

CREATE UNIQUE INDEX IX_UNIQUE on UniqueIndex (Id2);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("UniqueIndex", index.Table.Name);
                Assert.Equal("IX_UNIQUE", index.Name);
                Assert.True(index.IsUnique);
                Assert.Equal(
                    ["Id2"], index.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE UniqueIndex;");

    #endregion

    #region ForeignKeyFacets

    [ConditionalFact]
    public void Create_composite_foreign_key()
        => Test(
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
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES PrincipalTable(Id1, Id2) ON DELETE CASCADE
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId1", "ForeignKeyId2"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id1", "Id2"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Create_multiple_foreign_key_in_same_table()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE AnotherPrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
    FOREIGN KEY (ForeignKeyId2) REFERENCES AnotherPrincipalTable(Id) ON DELETE CASCADE
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var foreignKeys = dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys;

                Assert.Equal(2, foreignKeys.Count);

                var principalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "PrincipalTable"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DependentTable", principalFk.Table.Name);
                Assert.Equal("PrincipalTable", principalFk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId1"], principalFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id"], principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, principalFk.OnDelete);

                var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "AnotherPrincipalTable"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DependentTable", anotherPrincipalFk.Table.Name);
                Assert.Equal("AnotherPrincipalTable", anotherPrincipalFk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId2"], anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id"], anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, anotherPrincipalFk.OnDelete);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE AnotherPrincipalTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Create_foreign_key_referencing_unique_constraint()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id1 int,
    Id2 int UNIQUE
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id2) ON DELETE CASCADE
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id2"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact(Skip = "See issue#8802")]
    public void Set_name_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                Assert.Equal("MYFK", fk.Name);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Set_referential_action_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE SET NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["ForeignKeyId"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["Id"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.SetNull, fk.OnDelete);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    #endregion

    #region Warnings

    [ConditionalFact]
    public void Warn_for_schema_filtering()
        => Test(
            "CREATE TABLE Everest ( id int );",
            Enumerable.Empty<string>(),
            new[] { "dbo" },
            dbModel =>
            {
                var (_, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                Assert.Equal(SqliteResources.LogUsingSchemaSelectionsWarning(new TestLogger<SqliteLoggingDefinitions>()).EventId, Id);
                Assert.Equal(
                    SqliteResources.LogUsingSchemaSelectionsWarning(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage(),
                    Message);
            },
            "DROP TABLE Everest;");

    [ConditionalFact]
    public void Warn_missing_table()
        => Test(
            "CREATE TABLE Blank ( Id int );",
            new[] { "MyTable" },
            Enumerable.Empty<string>(),
            dbModel =>
            {
                Assert.Empty(dbModel.Tables);

                var (Level, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                Assert.Equal(SqliteResources.LogMissingTable(new TestLogger<SqliteLoggingDefinitions>()).EventId, Id);
                Assert.Equal(
                    SqliteResources.LogMissingTable(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("MyTable"), Message);
            },
            "DROP TABLE Blank;");

    [ConditionalFact]
    public void Warn_missing_principal_table_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE
);",
            new[] { "DependentTable" },
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var (_, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                Assert.Equal(
                    SqliteResources.LogForeignKeyScaffoldErrorPrincipalTableNotFound(new TestLogger<SqliteLoggingDefinitions>())
                        .EventId, Id);
                Assert.Equal(
                    SqliteResources.LogForeignKeyScaffoldErrorPrincipalTableNotFound(new TestLogger<SqliteLoggingDefinitions>())
                        .GenerateMessage("0", "DependentTable", "PrincipalTable"), Message);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Warn_missing_principal_column_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(ImaginaryId) ON DELETE CASCADE
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            dbModel =>
            {
                var (_, Id, Message, _, _) = Assert.Single(Fixture.ListLoggerFactory.Log.Where(t => t.Level == LogLevel.Warning));

                Assert.Equal(SqliteResources.LogPrincipalColumnNotFound(new TestLogger<SqliteLoggingDefinitions>()).EventId, Id);
                Assert.Equal(
                    SqliteResources.LogPrincipalColumnNotFound(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage(
                        "0", "DependentTable", "ImaginaryId", "PrincipalTable"),
                    Message);
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    #endregion

    public class SqliteDatabaseModelFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => nameof(SqliteDatabaseModelFactoryTest);

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public new SqliteTestStore TestStore
            => (SqliteTestStore)base.TestStore;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Scaffolding.Name;
    }
}
