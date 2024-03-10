// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqliteTypeMappingTest : RelationalTypeMappingTest
{
    private class YouNoTinyContext(SqliteConnection connection) : DbContext
    {
        private readonly SqliteConnection _connection = connection;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(_connection);

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
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

        [Required]
        [Column(TypeName = "tinyint")]
        public TinyState TinyState { get; set; }
    }

    [ConditionalFact]
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

    [ConditionalTheory]
    [InlineData(typeof(SqliteDateTimeOffsetTypeMapping), typeof(DateTimeOffset))]
    [InlineData(typeof(SqliteDateTimeTypeMapping), typeof(DateTime))]
    [InlineData(typeof(SqliteDecimalTypeMapping), typeof(decimal))]
    [InlineData(typeof(SqliteGuidTypeMapping), typeof(Guid))]
    [InlineData(typeof(SqliteULongTypeMapping), typeof(ulong))]
    public override void Create_and_clone_with_converter(Type mappingType, Type type)
        => base.Create_and_clone_with_converter(mappingType, type);

    [ConditionalTheory]
    [InlineData("TEXT", typeof(string))]
    [InlineData("Integer", typeof(long))]
    [InlineData("Blob", typeof(byte[]))]
    [InlineData("numeric", typeof(byte[]))]
    [InlineData("real", typeof(double))]
    [InlineData("doub", typeof(double))]
    [InlineData("int", typeof(long))]
    [InlineData("SMALLINT", typeof(long))]
    [InlineData("UNSIGNED BIG INT", typeof(long))]
    [InlineData("VARCHAR(255)", typeof(string))]
    [InlineData("nchar(55)", typeof(string))]
    [InlineData("datetime", typeof(byte[]))]
    [InlineData("decimal(10,4)", typeof(byte[]))]
    [InlineData("boolean", typeof(byte[]))]
    [InlineData("unknown_type", typeof(byte[]))]
    [InlineData("", typeof(byte[]))]
    public void It_maps_strings_to_not_null_types(string typeName, Type type)
        => Assert.Equal(type, CreateTypeMapper().FindMapping(typeName)?.ClrType);

    private static IRelationalTypeMappingSource CreateTypeMapper()
        => TestServiceFactory.Instance.Create<SqliteTypeMappingSource>();

    public static RelationalTypeMapping? GetMapping(Type type)
        => CreateTypeMapper().FindMapping(type);

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

    [ConditionalFact]
    public override void DateOnly_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateOnly)),
            new DateOnly(2015, 3, 12),
            "'2015-03-12'");

    [ConditionalFact]
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

    protected override DbContextOptions ContextOptions { get; }
        = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true))
            .UseSqlite("Filename=dummy.db").Options;
}
