// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

public abstract class MigrationsSqlGeneratorTestBase
{
    protected static string EOL
        => Environment.NewLine;

    protected virtual string Sql { get; set; }

    [ConditionalFact]
    public void All_tests_must_be_overriden()
    {
        var baseTests = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.IsVirtual && !method.IsFinal && method.DeclaringType == typeof(MigrationsSqlGeneratorTestBase))
            .ToList();

        Assert.True(
            baseTests.Count == 0, $"{GetType().ShortDisplayName()} should override the following methods to assert the generated SQL:"
            + EOL
            + string.Join(EOL, baseTests.Select(m => m.Name)));
    }

    [ConditionalFact]
    public virtual void AddColumnOperation_without_column_type()
        => Generate(
            new AddColumnOperation
            {
                Table = "People",
                Name = "Alias",
                ClrType = typeof(string)
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_unicode_overridden()
        => Generate(
            modelBuilder => modelBuilder.Entity<Person>().Property<string>("Name").IsUnicode(false),
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Name",
                ClrType = typeof(string),
                IsUnicode = true,
                IsNullable = true
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_unicode_no_model()
        => Generate(
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Name",
                ClrType = typeof(string),
                IsUnicode = false,
                IsNullable = true
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_fixed_length_no_model()
        => Generate(
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Name",
                ClrType = typeof(string),
                IsUnicode = false,
                IsNullable = true,
                IsFixedLength = true,
                MaxLength = 100
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_maxLength_overridden()
        => Generate(
            modelBuilder => modelBuilder.Entity<Person>().Property<string>("Name").HasMaxLength(30),
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Name",
                ClrType = typeof(string),
                MaxLength = 32,
                IsNullable = true
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_maxLength_no_model()
        => Generate(
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Name",
                ClrType = typeof(string),
                MaxLength = 30,
                IsNullable = true
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_precision_and_scale_overridden()
        => Generate(
            modelBuilder => modelBuilder.Entity<Person>().Property<decimal>("Pi").HasPrecision(30, 17),
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Pi",
                ClrType = typeof(decimal),
                Precision = 15,
                Scale = 10
            });

    [ConditionalFact]
    public virtual void AddColumnOperation_with_precision_and_scale_no_model()
        => Generate(
            new AddColumnOperation
            {
                Table = "Person",
                Name = "Pi",
                ClrType = typeof(decimal),
                Precision = 20,
                Scale = 7
            });

    [ConditionalFact]
    public virtual void AddForeignKeyOperation_without_principal_columns()
        => Generate(
            new AddForeignKeyOperation
            {
                Table = "People",
                Columns = ["SpouseId"],
                PrincipalTable = "People"
            });

    [ConditionalFact]
    public virtual void AlterColumnOperation_without_column_type()
        => Generate(
            new AlterColumnOperation
            {
                Table = "People",
                Name = "LuckyNumber",
                ClrType = typeof(int)
            });

    [ConditionalFact]
    public virtual void RenameTableOperation_legacy()
        => Generate(
            new RenameTableOperation
            {
                Name = "People",
                Schema = "dbo",
                NewName = "Person"
            });

    [ConditionalFact]
    public virtual void RenameTableOperation()
        => Generate(
            modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
            new RenameTableOperation
            {
                Name = "People",
                Schema = "dbo",
                NewName = "Person",
                NewSchema = "dbo"
            });

    [ConditionalFact]
    public virtual void SqlOperation()
        => Generate(
            new SqlOperation { Sql = "-- I <3 DDL" });

    private static readonly LineString _lineString1 = new(
        [new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(7.1, 7.2)]) { SRID = 4326 };

    private static readonly LineString _lineString2 = new(
        [new Coordinate(7.1, 7.2), new Coordinate(20.2, 20.2), new Coordinate(20.20, 1.1), new Coordinate(70.1, 70.2)])
    {
        SRID = 4326
    };

    private static readonly MultiPoint _multiPoint = new(
        [new Point(1.1, 2.2), new Point(2.2, 2.2), new Point(2.2, 1.1)]) { SRID = 4326 };

    private static readonly Polygon _polygon1 = new(
        new LinearRing(
            [new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(1.1, 2.2)]))
    {
        SRID = 4326
    };

    private static readonly Polygon _polygon2 = new(
        new LinearRing(
            [new Coordinate(10.1, 20.2), new Coordinate(20.2, 20.2), new Coordinate(20.2, 10.1), new Coordinate(10.1, 20.2)]))
    {
        SRID = 4326
    };

    private static readonly Point _point1 = new(1.1, 2.2, 3.3) { SRID = 4326 };

    private static readonly MultiLineString _multiLineString = new(
        [_lineString1, _lineString2]) { SRID = 4326 };

    private static readonly MultiPolygon _multiPolygon = new(
        [_polygon2, _polygon1]) { SRID = 4326 };

    private static readonly GeometryCollection _geometryCollection = new(
        [_lineString1, _lineString2, _multiPoint, _polygon1, _polygon2, _point1, _multiLineString, _multiPolygon])
    {
        SRID = 4326
    };

    [ConditionalFact]
    public virtual void InsertDataOperation_all_args_spatial()
        => Generate(
            new InsertDataOperation
            {
                Schema = "dbo",
                Table = "People",
                Columns = ["Id", "Full Name", "Geometry"],
                ColumnTypes = ["int", "varchar(40)", GetGeometryCollectionStoreType()],
                Values = new object[,]
                {
                    { 0, null, null },
                    { 1, "Daenerys Targaryen", null },
                    { 2, "John Snow", null },
                    { 3, "Arya Stark", null },
                    { 4, "Harry Strickland", null },
                    { 5, "The Imp", null },
                    { 6, "The Kingslayer", null },
                    { 7, "Aemon Targaryen", _geometryCollection }
                }
            });

    protected abstract string GetGeometryCollectionStoreType();

    [ConditionalFact]
    public virtual void InsertDataOperation_required_args()
        => Generate(
            CreateGotModel,
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["First Name"],
                Values = new object[,] { { "John" } }
            });

    [ConditionalFact]
    public virtual void InsertDataOperation_required_args_composite()
        => Generate(
            CreateGotModel,
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["First Name", "Last Name"],
                Values = new object[,] { { "John", "Snow" } }
            });

    [ConditionalFact]
    public virtual void InsertDataOperation_required_args_multiple_rows()
        => Generate(
            CreateGotModel,
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["First Name"],
                Values = new object[,] { { "John" }, { "Daenerys" } }
            });

    [ConditionalFact]
    public void InsertDataOperation_throws_for_missing_column_types()
        => Assert.Equal(
            RelationalStrings.InsertDataOperationNoModel("dbo.People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new InsertDataOperation
                        {
                            Table = "People",
                            Schema = "dbo",
                            Columns = ["First Name"],
                            Values = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public virtual void InsertDataOperation_throws_for_unsupported_column_types()
        => Assert.Equal(
            RelationalStrings.UnsupportedDataOperationStoreType("char[]", "dbo.People.First Name"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new InsertDataOperation
                        {
                            Table = "People",
                            Schema = "dbo",
                            Columns = ["First Name"],
                            ColumnTypes = ["char[]"],
                            Values = new object[,] { { null } }
                        })).Message);

    [ConditionalFact]
    public void InsertDataOperation_throws_for_values_count_mismatch()
        => Assert.Equal(
            RelationalStrings.InsertDataOperationValuesCountMismatch(1, 2, "dbo.People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new InsertDataOperation
                        {
                            Table = "People",
                            Columns = ["First Name", "Last Name"],
                            Values = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public void InsertDataOperation_throws_for_types_count_mismatch()
        => Assert.Equal(
            RelationalStrings.InsertDataOperationTypesCountMismatch(2, 1, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new InsertDataOperation
                        {
                            Table = "People",
                            Columns = ["First Name"],
                            ColumnTypes = ["string", "string"],
                            Values = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public void InsertDataOperation_throws_for_missing_entity_type()
        => Assert.Equal(
            RelationalStrings.DataOperationNoTable("dbo1.People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new InsertDataOperation
                        {
                            Table = "People",
                            Schema = "dbo1",
                            Columns = ["First Name"],
                            Values = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public void InsertDataOperation_throws_for_missing_property()
        => Assert.Equal(
            RelationalStrings.DataOperationNoProperty("People", "Name"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new InsertDataOperation
                        {
                            Table = "People",
                            Columns = ["Name"],
                            Values = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public virtual void DeleteDataOperation_all_args()
        => Generate(
            CreateGotModel,
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Hodor" }, { "Daenerys" }, { "John" }, { "Arya" }, { "Harry" } }
            });

    [ConditionalFact]
    public virtual void DeleteDataOperation_all_args_composite()
        => Generate(
            CreateGotModel,
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,]
                {
                    { "Hodor", null }, { "Daenerys", "Targaryen" }, { "John", "Snow" }, { "Arya", "Stark" }, { "Harry", "Strickland" }
                }
            });

    [ConditionalFact]
    public virtual void DeleteDataOperation_required_args()
        => Generate(
            CreateGotModel,
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["Last Name"],
                KeyValues = new object[,] { { "Snow" } }
            });

    [ConditionalFact]
    public virtual void DeleteDataOperation_required_args_composite()
        => Generate(
            CreateGotModel,
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "John", "Snow" } }
            });

    [ConditionalFact]
    public void DeleteDataOperation_throws_for_missing_column_types()
        => Assert.Equal(
            RelationalStrings.DeleteDataOperationNoModel("People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new DeleteDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyValues = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public void DeleteDataOperation_throws_for_values_count_mismatch()
        => Assert.Equal(
            RelationalStrings.DeleteDataOperationValuesCountMismatch(1, 2, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new DeleteDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name", "Last Name"],
                            KeyValues = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public void DeleteDataOperation_throws_for_types_count_mismatch()
        => Assert.Equal(
            RelationalStrings.DeleteDataOperationTypesCountMismatch(2, 1, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new DeleteDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyColumnTypes = ["string", "string"],
                            KeyValues = new object[,] { { "John" } }
                        })).Message);

    [ConditionalFact]
    public virtual void UpdateDataOperation_all_args()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Hodor" }, { "Daenerys" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Winterfell", "Stark", "Northmen" }, { "Dragonstone", "Targaryen", "Valyrian" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_all_args_composite()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Hodor", null }, { "Daenerys", "Targaryen" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Stark" }, { "Targaryen" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_all_args_composite_multi()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Hodor", null }, { "Daenerys", "Targaryen" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Winterfell", "Stark", "Northmen" }, { "Dragonstone", "Targaryen", "Valyrian" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_all_args_multi()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Daenerys" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Dragonstone", "Targaryen", "Valyrian" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_required_args()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Daenerys" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Targaryen" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_required_args_multiple_rows()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Hodor" }, { "Daenerys" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Stark" }, { "Targaryen" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_required_args_composite()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Daenerys", "Targaryen" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Targaryen" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_required_args_composite_multi()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Daenerys", "Targaryen" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Dragonstone", "Targaryen", "Valyrian" } }
            });

    [ConditionalFact]
    public virtual void UpdateDataOperation_required_args_multi()
        => Generate(
            CreateGotModel,
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Daenerys" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Dragonstone", "Targaryen", "Valyrian" } }
            });

    [ConditionalFact]
    public void UpdateDataOperation_throws_for_missing_column_types()
        => Assert.Equal(
            RelationalStrings.UpdateDataOperationNoModel("People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new UpdateDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyValues = new object[,] { { "Daenerys" } },
                            Columns = ["House Allegiance"],
                            Values = new object[,] { { "Targaryen" } }
                        })).Message);

    [ConditionalFact]
    public void UpdateDataOperation_throws_for_row_count_mismatch()
        => Assert.Equal(
            RelationalStrings.UpdateDataOperationRowCountMismatch(1, 2, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new UpdateDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyColumnTypes = ["string"],
                            KeyValues = new object[,] { { "Daenerys" }, { "John" } },
                            Columns = ["House Allegiance"],
                            Values = new object[,] { { "Targaryen" } }
                        })).Message);

    [ConditionalFact]
    public void UpdateDataOperation_throws_for_key_values_count_mismatch()
        => Assert.Equal(
            RelationalStrings.UpdateDataOperationKeyValuesCountMismatch(1, 2, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new UpdateDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name", "Last Name"],
                            KeyValues = new object[,] { { "Daenerys" } },
                            Columns = ["House Allegiance"],
                            Values = new object[,] { { "Targaryen" } }
                        })).Message);

    [ConditionalFact]
    public void UpdateDataOperation_throws_for_key_types_count_mismatch()
        => Assert.Equal(
            RelationalStrings.UpdateDataOperationKeyTypesCountMismatch(2, 1, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new UpdateDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyColumnTypes = ["string", "string"],
                            KeyValues = new object[,] { { "Daenerys" } },
                            Columns = ["House Allegiance"],
                            Values = new object[,] { { "Targaryen" } }
                        })).Message);

    [ConditionalFact]
    public void UpdateDataOperation_throws_for_values_count_mismatch()
        => Assert.Equal(
            RelationalStrings.UpdateDataOperationValuesCountMismatch(1, 2, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        CreateGotModel,
                        new UpdateDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyValues = new object[,] { { "Daenerys" } },
                            Columns = ["House Allegiance", "Culture"],
                            Values = new object[,] { { "Targaryen" } }
                        })).Message);

    [ConditionalFact]
    public void UpdateDataOperation_throws_for_types_count_mismatch()
        => Assert.Equal(
            RelationalStrings.UpdateDataOperationTypesCountMismatch(2, 1, "People"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new UpdateDataOperation
                        {
                            Table = "People",
                            KeyColumns = ["First Name"],
                            KeyValues = new object[,] { { "Daenerys" } },
                            Columns = ["House Allegiance"],
                            ColumnTypes = ["string", "string"],
                            Values = new object[,] { { "Targaryen" } }
                        })).Message);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void DefaultValue_with_line_breaks(bool isUnicode)
        => Generate(
            new CreateTableOperation
            {
                Name = "TestLineBreaks",
                Schema = "dbo",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "TestDefaultValue",
                        Table = "TestLineBreaks",
                        Schema = "dbo",
                        ClrType = typeof(string),
                        DefaultValue = "\r\nVarious Line\rBreaks\n",
                        IsUnicode = isUnicode
                    }
                }
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void DefaultValue_with_line_breaks_2(bool isUnicode)
    {
        var defaultValue = Enumerable.Range(0, 300).Select(e => e.ToString())
            .Select(e => e + "\r\n")
            .Join("");

        Generate(
            new CreateTableOperation
            {
                Name = "TestLineBreaks",
                Schema = "dbo",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "TestDefaultValue",
                        Table = "TestLineBreaks",
                        Schema = "dbo",
                        ClrType = typeof(string),
                        DefaultValue = defaultValue,
                        IsUnicode = isUnicode
                    }
                }
            });
    }

    [ConditionalTheory]
    [InlineData(3L)]
    [InlineData(null)]
    public virtual void Sequence_restart_operation(long? startsAt)
        => Generate(
            new RestartSequenceOperation
            {
                Name = "TestRestartSequenceOperation",
                Schema = "dbo",
                StartValue = startsAt
            });

    private static void CreateGotModel(ModelBuilder b)
        => b.HasDefaultSchema("dbo").Entity(
            "Person", pb =>
            {
                pb.ToTable("People");
                pb.Property<string>("FirstName").HasColumnName("First Name");
                pb.Property<string>("LastName").HasColumnName("Last Name");
                pb.Property<string>("Birthplace").HasColumnName("Birthplace");
                pb.Property<string>("Allegiance").HasColumnName("House Allegiance");
                pb.Property<string>("Culture").HasColumnName("Culture");
                pb.HasKey("FirstName", "LastName");
            });

    protected TestHelpers TestHelpers { get; }
    protected DbContextOptions ContextOptions { get; }
    protected IServiceCollection CustomServices { get; }

    protected MigrationsSqlGeneratorTestBase(
        TestHelpers testHelpers,
        IServiceCollection customServices = null,
        DbContextOptions options = null)
    {
        TestHelpers = testHelpers;
        CustomServices = customServices;
        ContextOptions = options;
    }

    protected virtual void Generate(MigrationOperation operation, MigrationsSqlGenerationOptions options)
        => Generate(null, [operation], options);

    protected virtual void Generate(params MigrationOperation[] operation)
        => Generate(null, operation);

    protected virtual void Generate(
        Action<ModelBuilder> buildAction,
        Action<MigrationBuilder> migrateAction,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        var migrationBuilder = new MigrationBuilder(activeProvider: null);
        migrateAction(migrationBuilder);

        Generate(buildAction, migrationBuilder.Operations.ToArray(), options);
    }

    protected virtual void Generate(Action<ModelBuilder> buildAction, params MigrationOperation[] operation)
        => Generate(buildAction, operation, MigrationsSqlGenerationOptions.Default);

    protected virtual void Generate(
        Action<ModelBuilder> buildAction,
        MigrationOperation[] operation,
        MigrationsSqlGenerationOptions options)
    {
        IModel model = null;
        if (buildAction != null)
        {
            var modelBuilder = TestHelpers.CreateConventionBuilder();
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
            buildAction(modelBuilder);

            model = modelBuilder.FinalizeModel(designTime: true, skipValidation: true);
        }

        var services = ContextOptions != null
            ? TestHelpers.CreateContextServices(CustomServices, ContextOptions)
            : TestHelpers.CreateContextServices(CustomServices);
        var batch = services.GetRequiredService<IMigrationsSqlGenerator>().Generate(operation, model, options);

        Sql = string.Join(
            "GO" + EOL + EOL,
            batch.Select(b => b.CommandText));
    }

    protected void AssertSql(string expected)
        => Assert.Equal(expected.TrimEnd(), Sql.TrimEnd(), ignoreLineEndingDifferences: true);

    protected class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Pi { get; set; }
    }
}
