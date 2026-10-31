// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqliteTypeMappingTest : RelationalTypeMappingTest
{
    private class YouNoTinyContext(SqliteConnection connection) : DbContext
    {
        private readonly SqliteConnection _connection = connection;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(_connection);

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public DbSet<NoTiny> NoTinnies { get; set; } = null!;
    }

    private enum TinyState : byte
    {
        One,
        Two,
        Three
    }

    private class NoTiny
    {
        [Key]
        public int Id { get; set; }

        [Required, Column(TypeName = "tinyint")]
        public TinyState TinyState { get; set; }
    }

    [Fact]
    public void SQLite_type_mapping_works_even_when_using_non_SQLite_store_type()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using (var context = new YouNoTinyContext(connection))
        {
            context.Database.EnsureCreated();

            context.Add(
                new NoTiny { TinyState = TinyState.Two });
            context.SaveChanges();
        }

        using (var context = new YouNoTinyContext(connection))
        {
            var tiny = context.NoTinnies.Single();
            Assert.Equal(TinyState.Two, tiny.TinyState);
        }

        connection.Close();
    }

    protected override DbCommand CreateTestCommand()
        => new SqliteCommand();

    [Theory, InlineData(typeof(SqliteDateTimeOffsetTypeMapping), typeof(DateTimeOffset)),
     InlineData(typeof(SqliteDateTimeTypeMapping), typeof(DateTime)), InlineData(typeof(SqliteDecimalTypeMapping), typeof(decimal)),
     InlineData(typeof(SqliteGuidTypeMapping), typeof(Guid)), InlineData(typeof(SqliteHalfTypeMapping), typeof(Half)),
     InlineData(typeof(SqliteULongTypeMapping), typeof(ulong))]
    public override void Create_and_clone_with_converter(Type mappingType, Type type)
        => base.Create_and_clone_with_converter(mappingType, type);

    [Theory, InlineData("TEXT", typeof(string)), InlineData("Integer", typeof(long)), InlineData("Blob", typeof(byte[])),
     InlineData("numeric", typeof(byte[])), InlineData("real", typeof(double)), InlineData("doub", typeof(double)),
     InlineData("int", typeof(long)), InlineData("SMALLINT", typeof(long)), InlineData("UNSIGNED BIG INT", typeof(long)),
     InlineData("VARCHAR(255)", typeof(string)), InlineData("nchar(55)", typeof(string)), InlineData("datetime", typeof(byte[])),
     InlineData("decimal(10,4)", typeof(byte[])), InlineData("boolean", typeof(byte[])), InlineData("unknown_type", typeof(byte[])),
     InlineData("", typeof(byte[]))]
    public void It_maps_strings_to_not_null_types(string typeName, Type type)
        => Assert.Equal(type, CreateTypeMapper().FindMapping(typeName)?.ClrType);

    private static IRelationalTypeMappingSource CreateTypeMapper()
        => TestServiceFactory.Instance.Create<SqliteTypeMappingSource>();

    public static RelationalTypeMapping GetMapping(Type type)
        => Assert.IsAssignableFrom<RelationalTypeMapping>(CreateTypeMapper().FindMapping(type));

    public override void DateTimeOffset_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateTimeOffset)),
            new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0)),
            "'2015-03-12 13:36:37.371-07:00'");

    public override void DateTime_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateTime)),
            new DateTime(2015, 3, 12, 13, 36, 37, 371, DateTimeKind.Utc),
            "'2015-03-12 13:36:37.371'");

    [Theory, InlineData(null), InlineData("custom_blob")]
    public void JSON_byte_array_mapping_preserves_text_parameters_and_reuses_default(string? storeType)
    {
        var modelBuilder = SqliteTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity("Owner").Property<int>("Id");
        modelBuilder.Entity("Owner").OwnsOne(
            "JsonData", "Data", owned =>
            {
                owned.ToJson();
                var property = owned.Property<byte[]>("Value");
                if (storeType != null)
                {
                    property.HasColumnType(storeType);
                }
            });
        var model = modelBuilder.FinalizeModel();
        var property = model.FindEntityType("Owner")!.FindNavigation("Data")!.TargetEntityType.FindProperty("Value")!;
        var mapping = Assert.IsType<SqliteJsonByteArrayTypeMapping>(CreateTypeMapper().FindMapping(property));
        if (storeType == null)
        {
            Assert.Same(SqliteJsonByteArrayTypeMapping.Default, mapping);
        }
        else
        {
            Assert.Equal(storeType, mapping.StoreType);
        }

        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var command = connection.CreateCommand();
        byte[] value = [1, 2, 255];
        var parameter = Assert.IsType<SqliteParameter>(mapping.CreateParameter(command, "p", value));
        Assert.Equal(SqliteType.Text, parameter.SqliteType);
        Assert.Equal(value, parameter.Value);
        Assert.Equal("X'0102FF'", mapping.GenerateSqlLiteral(value));
        Assert.Equal(DBNull.Value, mapping.CreateParameter(command, "p", null, nullable: true).Value);

        command.CommandText = "SELECT typeof(@p), @p";
        command.Parameters.Add(parameter);
        using (var reader = command.ExecuteReader())
        {
            Assert.True(reader.Read());
            Assert.Equal("text", reader.GetString(0));
            Assert.Equal("0102FF", reader.GetString(1));
        }

        var nativeParameter = Assert.IsType<SqliteParameter>(SqliteByteArrayTypeMapping.Default.CreateParameter(command, "p", value));
        Assert.Equal(SqliteType.Blob, nativeParameter.SqliteType);
        Assert.Equal(value, nativeParameter.Value);

        var clone = Assert.IsType<SqliteJsonByteArrayTypeMapping>(
            mapping.Clone(new RelationalTypeMappingInfo(storeTypeName: "another_blob", size: 32)));
        Assert.Equal("another_blob", clone.StoreType);
        Assert.Equal(32, clone.Size);
        Assert.Equal(SqliteType.Text, Assert.IsType<SqliteParameter>(clone.CreateParameter(command, "p", value)).SqliteType);
    }

    [Fact]
    public void JSON_byte_array_mapping_preserves_converter_and_reader_writer()
    {
        var converter = new NumberToBytesConverter<long>();
        var nativeMapping = (SqliteByteArrayTypeMapping)SqliteByteArrayTypeMapping.Default.WithComposedConverter(converter);
        var mapping = new SqliteJsonByteArrayTypeMapping(nativeMapping);

        Assert.Same(nativeMapping.Converter, mapping.Converter);
        Assert.Same(nativeMapping.JsonValueReaderWriter, mapping.JsonValueReaderWriter);

        using var command = new SqliteCommand();
        var parameter = Assert.IsType<SqliteParameter>(mapping.CreateParameter(command, "p", 42L));
        Assert.Equal(SqliteType.Text, parameter.SqliteType);
        Assert.Equal((byte[])converter.ConvertToProvider(42L)!, Assert.IsType<byte[]>(parameter.Value));
    }

    [Theory, InlineData(DateTimeKind.Utc), InlineData(DateTimeKind.Unspecified)]
    public void JSON_DateTime_mapping_formats_literals_and_parameters(DateTimeKind kind)
    {
        var mapping = (SqliteDateTimeTypeMapping)SqliteDateTimeTypeMapping.Default.WithComposedConverter(
            converter: null, jsonValueReaderWriter: LegacyDateTimeJsonReaderWriter.Instance);
        var jsonMapping = GetJsonDateTimeMapping(customReaderWriter: true);
        var value = new DateTime(2026, 1, 1, 0, 0, 0, 123, kind);
        var expected = "2026-01-01T00:00:00.123" + (kind == DateTimeKind.Utc ? "Z" : "");

        using var command = new SqliteCommand();
        Assert.Equal(expected, jsonMapping.CreateParameter(command, "p", value).Value);
        Assert.Equal(DbType.String, jsonMapping.DbType);
        Assert.Equal("'" + expected + "'", jsonMapping.GenerateSqlLiteral(value));
        Assert.Equal(DBNull.Value, jsonMapping.CreateParameter(command, "p", null, nullable: true).Value);
        Assert.Equal("NULL", jsonMapping.GenerateSqlLiteral(null));

        Assert.Equal(value, mapping.CreateParameter(command, "p", value).Value);
        Assert.Equal(DbType.DateTime, mapping.DbType);
        Assert.Equal("'2026-01-01 00:00:00.123'", mapping.GenerateSqlLiteral(value));

        var clone = jsonMapping.Clone(new RelationalTypeMappingInfo(storeTypeName: "datetime"));
        Assert.IsType<SqliteJsonDateTimeTypeMapping>(clone);
        Assert.Equal(DbType.String, clone.DbType);
        Assert.Equal(expected, clone.CreateParameter(command, "p", value).Value);
        Assert.Equal("'" + expected + "'", clone.GenerateSqlLiteral(value));

        var reconstructed = SqliteJsonDateTimeTypeMapping.Default.Clone(jsonValueReaderWriter: jsonMapping.JsonValueReaderWriter);
        Assert.IsType<SqliteJsonDateTimeTypeMapping>(reconstructed);
        Assert.Equal(expected, reconstructed.CreateParameter(command, "p", value).Value);
        Assert.Equal("'" + expected + "'", reconstructed.GenerateSqlLiteral(value));
    }

    [Fact]
    public void Native_DateTime_mapping_with_string_DbType_does_not_use_JSON_formatter()
    {
        var mapping = new SqliteDateTimeTypeMapping("TEXT", DbType.String)
            .Clone(jsonValueReaderWriter: LegacyDateTimeJsonReaderWriter.Instance);
        var value = new DateTime(2026, 1, 1, 0, 0, 0, 123, DateTimeKind.Utc);

        using var command = new SqliteCommand();
        Assert.Equal(value, mapping.CreateParameter(command, "p", value).Value);
        Assert.Equal("'2026-01-01 00:00:00.123'", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void JSON_DateTime_mapping_converts_values_once()
    {
        var mapping = GetJsonDateTimeMapping(customReaderWriter: true, converted: true);
        var value = new DateTime(2026, 1, 1, 0, 0, 0, 123, DateTimeKind.Utc).Ticks;

        using var command = new SqliteCommand();
        Assert.Equal("2026-01-01T00:00:00.123Z", mapping.CreateParameter(command, "p", value).Value);
        Assert.Equal("'2026-01-01T00:00:00.123Z'", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void Default_JSON_DateTime_mapping_preserves_native_format()
    {
        var mapping = GetJsonDateTimeMapping(customReaderWriter: false);
        Assert.Equal(DbType.DateTime, mapping.DbType);
        Assert.Equal("'2026-01-01 00:00:00.123'", mapping.GenerateSqlLiteral(new DateTime(2026, 1, 1, 0, 0, 0, 123)));
    }

    private static RelationalTypeMapping GetJsonDateTimeMapping(bool customReaderWriter, bool converted = false)
    {
        var modelBuilder = SqliteTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity("Owner").Property<int>("Id");
        modelBuilder.Entity("Owner").OwnsOne(
            "JsonData", "Data", owned =>
            {
                owned.ToJson();
                if (converted)
                {
                    owned.Property<long>("Value").HasConversion(
                        new ValueConverter<long, DateTime>(
                            ticks => new DateTime(ticks, DateTimeKind.Utc),
                            date => date.Ticks));
                }
                else
                {
                    owned.Property<DateTime>("Value");
                }

                if (customReaderWriter)
                {
                    owned.Property("Value").HasJsonValueReaderWriter<LegacyDateTimeJsonReaderWriter>();
                }
            });
        var model = modelBuilder.FinalizeModel();
        var property = model.FindEntityType("Owner")!.FindNavigation("Data")!.TargetEntityType.FindProperty("Value")!;
        var mapping = CreateTypeMapper().FindMapping(property);
        return customReaderWriter
            ? Assert.IsType<SqliteJsonDateTimeTypeMapping>(mapping)
            : Assert.IsType<SqliteDateTimeTypeMapping>(mapping);
    }

    public sealed class LegacyDateTimeJsonReaderWriter : JsonValueReaderWriter<DateTime>
    {
        private static readonly JsonDateTimeReaderWriter ReaderWriter = new("yyyy-MM-dd'T'HH:mm:ss.fffK");

        public static LegacyDateTimeJsonReaderWriter Instance { get; } = new();

        public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => ReaderWriter.FromJsonTyped(ref manager, existingObject);

        public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
            => ReaderWriter.ToJsonTyped(writer, value);

        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(LegacyDateTimeJsonReaderWriter), nameof(Instance));
    }

    [Fact]
    public override void DateOnly_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateOnly)),
            new DateOnly(2015, 3, 12),
            "'2015-03-12'");

    [Fact]
    public override void TimeOnly_literal_generated_correctly()
    {
        var typeMapping = GetMapping(typeof(TimeOnly));

        Test_GenerateSqlLiteral_helper(typeMapping, new TimeOnly(13, 10, 15), "'13:10:15'");
        Test_GenerateSqlLiteral_helper(typeMapping, new TimeOnly(13, 10, 15, 120), "'13:10:15.1200000'");
        Test_GenerateSqlLiteral_helper(typeMapping, new TimeOnly(13, 10, 15, 120, 20), "'13:10:15.1200200'");
    }

    public override void Decimal_literal_generated_correctly()
    {
        var typeMapping = new SqliteDecimalTypeMapping("TEXT");

        Test_GenerateSqlLiteral_helper(typeMapping, decimal.MinValue, "'-79228162514264337593543950335.0'");
        Test_GenerateSqlLiteral_helper(typeMapping, decimal.MaxValue, "'79228162514264337593543950335.0'");
    }

    public override void Guid_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(Guid)),
            new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292"),
            "'C6F43A9E-91E1-45EF-A320-832EA23B7292'");

    public override void ULong_literal_generated_correctly()
    {
        var typeMapping = new SqliteULongTypeMapping("INTEGER");

        Test_GenerateSqlLiteral_helper(typeMapping, ulong.MinValue, "0");
        Test_GenerateSqlLiteral_helper(typeMapping, ulong.MaxValue, "-1");
        Test_GenerateSqlLiteral_helper(typeMapping, long.MaxValue + 1ul, "-9223372036854775808");
    }

    [Fact]
    public void Half_literal_generated_correctly()
    {
        var typeMapping = SqliteHalfTypeMapping.Default;

        Test_GenerateSqlLiteral_helper(typeMapping, Half.MinValue, "-65504");
        Test_GenerateSqlLiteral_helper(typeMapping, Half.MaxValue, "65504");
        Test_GenerateSqlLiteral_helper(typeMapping, (Half)3.14f, "3.140625");
    }

    protected override DbContextOptions ContextOptions { get; }
        = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true))
            .UseSqlite("Filename=dummy.db").Options;
}
