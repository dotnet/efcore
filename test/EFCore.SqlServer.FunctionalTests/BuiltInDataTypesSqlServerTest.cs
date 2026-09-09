// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.Data.SqlClient;

// ReSharper disable InconsistentNaming
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable UnusedParameter.Local
// ReSharper disable PossibleInvalidOperationException
namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotSqlAzure | SqlServerCondition.SupportsUtf8)]
public class BuiltInDataTypesSqlServerTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqlServerTest.BuiltInDataTypesSqlServerFixture>
{
    private static readonly string _eol = Environment.NewLine;

    public BuiltInDataTypesSqlServerTest(BuiltInDataTypesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_BuiltInDataTypes()
    {
        using var context = CreateContext();

        context.Add(
            new BuiltInDataTypes
            {
                Id = 54,
                PartitionId = 1,
                TestInt16 = -1234,
                TestInt32 = -123456789,
                TestInt64 = -1234567890123456789L,
                TestDouble = -1.23456789,
                TestDecimal = -1234567890.01M,
                TestDateTime = Fixture.DefaultDateTime,
                TestDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                TestDateOnly = new DateOnly(2020, 3, 1),
                TestTimeOnly = new TimeOnly(12, 30, 45, 123),
                TestSingle = -1.234F,
                TestBoolean = true,
                TestByte = 255,
                TestUnsignedInt16 = 1234,
                TestUnsignedInt32 = 1234565789U,
                TestUnsignedInt64 = 1234567890123456789UL,
                TestCharacter = 'a',
                TestSignedByte = -128,
                Enum64 = Enum64.SomeValue,
                Enum32 = Enum32.SomeValue,
                Enum16 = Enum16.SomeValue,
                Enum8 = Enum8.SomeValue,
                EnumU64 = EnumU64.SomeValue,
                EnumU32 = EnumU32.SomeValue,
                EnumU16 = EnumU16.SomeValue,
                EnumS8 = EnumS8.SomeValue
            });

        context.SaveChanges();
        var set = context.Set<BuiltInDataTypes>();

        var param1 = (short)-1234;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestInt16 == param1));

        var param2 = -123456789;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestInt32 == param2));

        var param3 = -1234567890123456789L;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestInt64 == param3));

        double? param4 = -1.23456789;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestDouble == param4));

        var param5 = -1234567890.01M;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestDecimal == param5));

        var param6 = Fixture.DefaultDateTime;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestDateTime == param6));

        var param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestDateTimeOffset == param7));

        var param8 = new TimeSpan(0, 10, 9, 8, 7);
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestTimeSpan == param8));

        var param9 = new DateOnly(2020, 3, 1);
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestDateOnly == param9));

        var param10 = new TimeOnly(12, 30, 45, 123);
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestTimeOnly == param10));

        var param11 = -1.234F;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestSingle == param11));

        var param12 = true;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestBoolean == param12));

        var param13 = (byte)255;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestByte == param13));

        var param14 = Enum64.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum64 == param14));

        var param15 = Enum32.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum32 == param15));

        var param16 = Enum16.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum16 == param16));

        var param17 = Enum8.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum8 == param17));

        var param18 = (ushort)1234;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestUnsignedInt16 == param18));

        var param19 = 1234565789U;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestUnsignedInt32 == param19));

        var param20 = 1234567890123456789UL;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestUnsignedInt64 == param20));

        var param21 = 'a';
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestCharacter == param21));

        var param22 = (sbyte)-128;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.TestSignedByte == param22));

        var param23 = EnumU64.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.EnumU64 == param23));

        var param24 = EnumU32.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.EnumU32 == param24));

        var param25 = EnumU16.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.EnumU16 == param25));

        var param26 = EnumS8.SomeValue;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.EnumS8 == param26));

        var param27 = 1;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum64 == (Enum64)param27));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && (int)e.Enum64 == param27));

        var param28 = 1;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum32 == (Enum32)param28));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && (int)e.Enum32 == param28));

        var param29 = 1;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum16 == (Enum16)param29));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && (int)e.Enum16 == param29));

        var param30 = 1;
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.Enum8 == (Enum8)param30));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && (int)e.Enum8 == param30));
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_MaxLengthDataTypes()
    {
        using var context = CreateContext();

        var shortString = "Sky";
        var shortBinary = new byte[] { 8, 8, 7, 8, 7 };
        var longString = new string('X', Fixture.LongStringLength);
        var longBinary = new byte[Fixture.LongStringLength];
        for (var i = 0; i < longBinary.Length; i++)
        {
            longBinary[i] = (byte)i;
        }

        context.Add(
            new MaxLengthDataTypes
            {
                Id = 54,
                String3 = shortString,
                ByteArray5 = shortBinary,
                String9000 = longString,
                StringUnbounded = longString,
                ByteArray9000 = longBinary
            });

        context.SaveChanges();
        var set = context.Set<MaxLengthDataTypes>();

        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.String3 == shortString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.ByteArray5 == shortBinary));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.String9000 == longString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.StringUnbounded == longString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.ByteArray9000 == longBinary));
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_UnicodeDataTypes()
    {
        using var context = CreateContext();

        var shortString = Fixture.SupportsUnicodeToAnsiConversion ? "Ϩky" : "sky";
        var longString = Fixture.SupportsUnicodeToAnsiConversion
            ? new string('Ϩ', Fixture.LongStringLength)
            : new string('s', Fixture.LongStringLength);

        context.Add(
            new UnicodeDataTypes
            {
                Id = 54,
                StringDefault = shortString,
                StringAnsi = shortString,
                StringAnsi3 = shortString,
                StringAnsi9000 = longString,
                StringUnicode = shortString
            });

        context.SaveChanges();
        var set = context.Set<UnicodeDataTypes>();

        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.StringDefault == shortString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.StringAnsi == shortString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.StringAnsi3 == shortString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.StringUnicode == shortString));
        ExecuteQueryString(context, 54, set.Where(e => e.Id == 54 && e.StringAnsi9000 == longString));
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_MappedDataTypesWithIdentity()
    {
        using var context = CreateContext();

        var longAsBigint = 78L;
        var shortAsSmallint = (short)79;
        var byteAsTinyint = (byte)80;
        var uintAsInt = uint.MaxValue;
        var ulongAsBigint = ulong.MaxValue;
        var uShortAsSmallint = ushort.MaxValue;
        var sbyteAsTinyint = sbyte.MinValue;
        var boolAsBit = true;
        var decimalAsMoney = 81.1m;
        var decimalAsSmallmoney = 82.2m;
        var doubleAsFloat = 83.3;
        var floatAsReal = 84.4f;
        var doubleAsDoublePrecision = 85.5;
        var dateOnlyAsDate = new DateOnly(2020, 3, 1);
        var dateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12);
        var dateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(7654321), TimeSpan.Zero);
        var dateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(7654321);
        var dateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12);
        var dateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12);
        var timeOnlyAsTime = new TimeOnly(12, 30, 45, 123);
        var timeSpanAsTime = new TimeSpan(11, 15, 12);
        var stringAsVarcharMax = "C";
        var stringAsCharVaryingMax = "Your";
        var stringAsCharacterVaryingMax = "strong";
        var stringAsNvarcharMax = "don't";
        var stringAsNationalCharVaryingMax = "help";
        var stringAsNationalCharacterVaryingMax = "anyone!";
        var stringAsVarcharMaxUtf8 = "short";
        var stringAsCharVaryingMaxUtf8 = "And now";
        var stringAsCharacterVaryingMaxUtf8 = "this...";
        var stringAsText = "Gumball Rules!";
        var gumballRulesOk = "Gumball Rules OK!";
        var bytesAsVarbinaryMax = new byte[] { 89, 90, 91, 92 };
        var bytesAsBinaryVaryingMax = new byte[] { 93, 94, 95, 96 };
        var bytesAsImage = new byte[] { 97, 98, 99, 100 };
        var @decimal = 101m;
        var decimalAsDec = 102m;
        var decimalAsNumeric = 103m;
        var guidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47");
        var uintAsBigint = uint.MaxValue;
        var ulongAsDecimal200 = ulong.MaxValue;
        var uShortAsInt = ushort.MaxValue;
        var sByteAsSmallint = sbyte.MinValue;
        var CharAsVarchar = 'A';
        var CharAsAsCharVarying = 'B';
        var charAsCharacterVaryingMax = 'C';
        var CharAsNvarchar = 'D';
        var CharAsNationalCharVarying = 'E';
        var charAsNationalCharacterVaryingMax = 'F';
        var charAsText = 'G';
        var charAsNtext = 'H';
        var charAsInt = 'I';
        var enumAsNvarchar20 = StringEnumU16.Value4;
        var enumAsVarcharMax = StringEnum16.Value2;
        var sqlVariantString = "Bang!";
        var sqlVariantInt = 887876;

        var entity = context.Add(
            new MappedDataTypesWithIdentity
            {
                LongAsBigint = longAsBigint,
                ShortAsSmallint = shortAsSmallint,
                ByteAsTinyint = byteAsTinyint,
                UintAsInt = uintAsInt,
                UlongAsBigint = ulongAsBigint,
                UShortAsSmallint = uShortAsSmallint,
                SbyteAsTinyint = sbyteAsTinyint,
                BoolAsBit = boolAsBit,
                DecimalAsMoney = decimalAsMoney,
                DecimalAsSmallmoney = decimalAsSmallmoney,
                DoubleAsFloat = doubleAsFloat,
                FloatAsReal = floatAsReal,
                DoubleAsDoublePrecision = doubleAsDoublePrecision,
                DateOnlyAsDate = dateOnlyAsDate,
                DateTimeAsDate = dateTimeAsDate,
                DateTimeOffsetAsDatetimeoffset = dateTimeOffsetAsDatetimeoffset,
                DateTimeAsDatetime2 = dateTimeAsDatetime2,
                DateTimeAsSmalldatetime = dateTimeAsSmalldatetime,
                DateTimeAsDatetime = dateTimeAsDatetime,
                TimeOnlyAsTime = timeOnlyAsTime,
                TimeSpanAsTime = timeSpanAsTime,
                StringAsVarcharMax = stringAsVarcharMax,
                StringAsCharVaryingMax = stringAsCharVaryingMax,
                StringAsCharacterVaryingMax = stringAsCharacterVaryingMax,
                StringAsNvarcharMax = stringAsNvarcharMax,
                StringAsNationalCharVaryingMax = stringAsNationalCharVaryingMax,
                StringAsNationalCharacterVaryingMax = stringAsNationalCharacterVaryingMax,
                StringAsVarcharMaxUtf8 = stringAsVarcharMaxUtf8,
                StringAsCharVaryingMaxUtf8 = stringAsCharVaryingMaxUtf8,
                StringAsCharacterVaryingMaxUtf8 = stringAsCharacterVaryingMaxUtf8,
                StringAsText = stringAsText,
                StringAsNtext = gumballRulesOk,
                BytesAsVarbinaryMax = bytesAsVarbinaryMax,
                BytesAsBinaryVaryingMax = bytesAsBinaryVaryingMax,
                BytesAsImage = bytesAsImage,
                Decimal = @decimal,
                DecimalAsDec = decimalAsDec,
                DecimalAsNumeric = decimalAsNumeric,
                GuidAsUniqueidentifier = guidAsUniqueidentifier,
                UintAsBigint = uintAsBigint,
                UlongAsDecimal200 = ulongAsDecimal200,
                UShortAsInt = uShortAsInt,
                SByteAsSmallint = sByteAsSmallint,
                CharAsVarchar = CharAsVarchar,
                CharAsAsCharVarying = CharAsAsCharVarying,
                CharAsCharacterVaryingMax = charAsCharacterVaryingMax,
                CharAsNvarchar = CharAsNvarchar,
                CharAsNationalCharVarying = CharAsNationalCharVarying,
                CharAsNationalCharacterVaryingMax = charAsNationalCharacterVaryingMax,
                CharAsText = charAsText,
                CharAsNtext = charAsNtext,
                CharAsInt = charAsInt,
                EnumAsNvarchar20 = enumAsNvarchar20,
                EnumAsVarcharMax = enumAsVarcharMax,
                SqlVariantString = sqlVariantString,
                SqlVariantInt = sqlVariantInt
            }).Entity;

        context.SaveChanges();
        var id = entity.Id;
        var set = context.Set<MappedDataTypesWithIdentity>();

        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.LongAsBigint == longAsBigint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.ShortAsSmallint == shortAsSmallint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.ByteAsTinyint == byteAsTinyint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.UintAsInt == uintAsInt));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.UlongAsBigint == ulongAsBigint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.UShortAsSmallint == uShortAsSmallint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.SbyteAsTinyint == sbyteAsTinyint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BoolAsBit == boolAsBit));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsMoney == decimalAsMoney));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsSmallmoney == decimalAsSmallmoney));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DoubleAsFloat == doubleAsFloat));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.FloatAsReal == floatAsReal));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DoubleAsDoublePrecision == doubleAsDoublePrecision));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateOnlyAsDate == dateOnlyAsDate));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeAsDate == dateTimeAsDate));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeOffsetAsDatetimeoffset == dateTimeOffsetAsDatetimeoffset));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeAsDatetime2 == dateTimeAsDatetime2));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeAsSmalldatetime == dateTimeAsSmalldatetime));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeAsDatetime == dateTimeAsDatetime));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.TimeOnlyAsTime == timeOnlyAsTime));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.TimeSpanAsTime == timeSpanAsTime));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsVarcharMax == stringAsVarcharMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharVaryingMax == stringAsCharVaryingMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharacterVaryingMax == stringAsCharacterVaryingMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNvarcharMax == stringAsNvarcharMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNationalCharVaryingMax == stringAsNationalCharVaryingMax));
        ExecuteQueryString(
            context, id, set.Where(e => e.Id == id && e.StringAsNationalCharacterVaryingMax == stringAsNationalCharacterVaryingMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsVarcharMaxUtf8 == stringAsVarcharMaxUtf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharVaryingMaxUtf8 == stringAsCharVaryingMaxUtf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharacterVaryingMaxUtf8 == stringAsCharacterVaryingMaxUtf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BytesAsVarbinaryMax == bytesAsVarbinaryMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BytesAsBinaryVaryingMax == bytesAsBinaryVaryingMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.Decimal == @decimal));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsDec == decimalAsDec));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsNumeric == decimalAsNumeric));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.GuidAsUniqueidentifier == guidAsUniqueidentifier));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.UintAsBigint == uintAsBigint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.UlongAsDecimal200 == ulongAsDecimal200));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.UShortAsInt == uShortAsInt));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.SByteAsSmallint == sByteAsSmallint));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsVarchar == CharAsVarchar));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsAsCharVarying == CharAsAsCharVarying));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsCharacterVaryingMax == charAsCharacterVaryingMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsNvarchar == CharAsNvarchar));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsNationalCharVarying == CharAsNationalCharVarying));
        ExecuteQueryString(
            context, id, set.Where(e => e.Id == id && e.CharAsNationalCharacterVaryingMax == charAsNationalCharacterVaryingMax));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsInt == charAsInt));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.EnumAsNvarchar20 == enumAsNvarchar20));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.EnumAsVarcharMax == enumAsVarcharMax));

        // The text, ntext, and image data types are invalid for local variables.
        Assert.Contains(
            "text",
            Assert.Throws<SqlException>(
                () => ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsText == stringAsText))).Message);

        Assert.Contains(
            "ntext",
            Assert.Throws<SqlException>(
                () => ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNtext == gumballRulesOk))).Message);

        Assert.Contains(
            "image",
            Assert.Throws<SqlException>(
                () => ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BytesAsImage == bytesAsImage))).Message);

        Assert.Contains(
            "text",
            Assert.Throws<SqlException>(
                () => ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsText == charAsText))).Message);

        Assert.Contains(
            "ntext",
            Assert.Throws<SqlException>(
                () => ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsNtext == charAsNtext))).Message);
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_MappedSizedDataTypes()
    {
        using var context = CreateContext();

        var stringAsChar3 = "Wor";
        var stringAsCharacter3 = "Lon";
        var stringAsVarchar3 = "Tha";
        var stringAsCharVarying3 = "Thr";
        var stringAsCharacterVarying3 = "Let";
        var stringAsNchar3 = "Won";
        var stringAsNationalCharacter3 = "Squ";
        var stringAsNvarchar3 = "Int";
        var stringAsNationalCharVarying3 = "The";
        var stringAsNationalCharacterVarying3 = "Col";
        var stringAsChar3Utf8 = "Wha";
        var stringAsCharacter3Utf8 = "doe";
        var stringAsVarchar3Utf8 = "the";
        var stringAsCharVarying3Utf8 = "tex";
        var stringAsCharacterVarying3Utf8 = "men";
        var bytesAsBinary3 = new byte[] { 10, 11, 12 };
        var bytesAsVarbinary3 = new byte[] { 11, 12, 13 };
        var bytesAsBinaryVarying3 = new byte[] { 12, 13, 14 };
        var charAsVarchar3 = 'A';
        var charAsAsCharVarying3 = 'B';
        var charAsCharacterVarying3 = 'C';
        var charAsNvarchar3 = 'D';
        var charAsNationalCharVarying3 = 'E';
        var charAsNationalCharacterVarying3 = 'F';

        var entity = context.Add(
            new MappedSizedDataTypes
            {
                Id = 54,
                StringAsChar3 = stringAsChar3,
                StringAsCharacter3 = stringAsCharacter3,
                StringAsVarchar3 = stringAsVarchar3,
                StringAsCharVarying3 = stringAsCharVarying3,
                StringAsCharacterVarying3 = stringAsCharacterVarying3,
                StringAsNchar3 = stringAsNchar3,
                StringAsNationalCharacter3 = stringAsNationalCharacter3,
                StringAsNvarchar3 = stringAsNvarchar3,
                StringAsNationalCharVarying3 = stringAsNationalCharVarying3,
                StringAsNationalCharacterVarying3 = stringAsNationalCharacterVarying3,
                StringAsChar3Utf8 = stringAsChar3Utf8,
                StringAsCharacter3Utf8 = stringAsCharacter3Utf8,
                StringAsVarchar3Utf8 = stringAsVarchar3Utf8,
                StringAsCharVarying3Utf8 = stringAsCharVarying3Utf8,
                StringAsCharacterVarying3Utf8 = stringAsCharacterVarying3Utf8,
                BytesAsBinary3 = bytesAsBinary3,
                BytesAsVarbinary3 = bytesAsVarbinary3,
                BytesAsBinaryVarying3 = bytesAsBinaryVarying3,
                CharAsVarchar3 = charAsVarchar3,
                CharAsAsCharVarying3 = charAsAsCharVarying3,
                CharAsCharacterVarying3 = charAsCharacterVarying3,
                CharAsNvarchar3 = charAsNvarchar3,
                CharAsNationalCharVarying3 = charAsNationalCharVarying3,
                CharAsNationalCharacterVarying3 = charAsNationalCharacterVarying3
            }).Entity;

        context.SaveChanges();
        var id = entity.Id;
        var set = context.Set<MappedSizedDataTypes>();

        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsChar3 == stringAsChar3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharacter3 == stringAsCharacter3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsVarchar3 == stringAsVarchar3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharVarying3 == stringAsCharVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharacterVarying3 == stringAsCharacterVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNchar3 == stringAsNchar3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNationalCharacter3 == stringAsNationalCharacter3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNvarchar3 == stringAsNvarchar3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsNationalCharVarying3 == stringAsNationalCharVarying3));
        ExecuteQueryString(
            context, id, set.Where(e => e.Id == id && e.StringAsNationalCharacterVarying3 == stringAsNationalCharacterVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsChar3Utf8 == stringAsChar3Utf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharacter3Utf8 == stringAsCharacter3Utf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsVarchar3Utf8 == stringAsVarchar3Utf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharVarying3Utf8 == stringAsCharVarying3Utf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.StringAsCharacterVarying3Utf8 == stringAsCharacterVarying3Utf8));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BytesAsBinary3 == bytesAsBinary3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BytesAsVarbinary3 == bytesAsVarbinary3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.BytesAsBinaryVarying3 == bytesAsBinaryVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsVarchar3 == charAsVarchar3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsAsCharVarying3 == charAsAsCharVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsCharacterVarying3 == charAsCharacterVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsNvarchar3 == charAsNvarchar3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsNationalCharVarying3 == charAsNationalCharVarying3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.CharAsNationalCharacterVarying3 == charAsNationalCharacterVarying3));
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_MappedScaledDataTypes()
    {
        using var context = CreateContext();

        var floatAsFloat3 = 83.3f;
        var floatAsDoublePrecision3 = 85.5f;
        var floatAsFloat25 = 83.33f;
        var floatAsDoublePrecision25 = 85.55f;
        var dateTimeOffsetAsDatetimeoffset3 = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 765), TimeSpan.Zero);
        var dateTimeAsDatetime23 = new DateTime(2017, 1, 2, 12, 11, 12, 321);
        var decimalAsDecimal3 = 101m;
        var decimalAsDec3 = 102m;
        var decimalAsNumeric3 = 103m;
        var timeOnlyAsTime3 = TimeOnly.Parse("12:34:56.7890123", CultureInfo.InvariantCulture);
        var timeSpanAsTime3 = TimeSpan.Parse("12:34:56.7890123", CultureInfo.InvariantCulture);
        var entity = context.Add(
            new MappedScaledDataTypes
            {
                Id = 54,
                FloatAsFloat3 = floatAsFloat3,
                FloatAsDoublePrecision3 = floatAsDoublePrecision3,
                FloatAsFloat25 = floatAsFloat25,
                FloatAsDoublePrecision25 = floatAsDoublePrecision25,
                DateTimeOffsetAsDatetimeoffset3 = dateTimeOffsetAsDatetimeoffset3,
                DateTimeAsDatetime23 = dateTimeAsDatetime23,
                DecimalAsDecimal3 = decimalAsDecimal3,
                DecimalAsDec3 = decimalAsDec3,
                DecimalAsNumeric3 = decimalAsNumeric3,
                TimeOnlyAsTime3 = timeOnlyAsTime3,
                TimeSpanAsTime3 = timeSpanAsTime3
            }).Entity;

        context.SaveChanges();
        var id = entity.Id;
        var set = context.Set<MappedScaledDataTypes>();

        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.FloatAsFloat3 == floatAsFloat3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.FloatAsDoublePrecision3 == floatAsDoublePrecision3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.FloatAsFloat25 == floatAsFloat25));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.FloatAsDoublePrecision25 == floatAsDoublePrecision25));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeOffsetAsDatetimeoffset3 == dateTimeOffsetAsDatetimeoffset3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DateTimeAsDatetime23 == dateTimeAsDatetime23));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsDecimal3 == decimalAsDecimal3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsDec3 == decimalAsDec3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsNumeric3 == decimalAsNumeric3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.TimeOnlyAsTime3 == timeOnlyAsTime3));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.TimeSpanAsTime3 == timeSpanAsTime3));
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_MappedPrecisionAndScaledDataTypes()
    {
        using var context = CreateContext();

        var decimalAsDecimal52 = 101.1m;
        var decimalAsDec52 = 102.2m;
        var decimalAsNumeric52 = 103.3m;
        var entity = context.Add(
            new MappedPrecisionAndScaledDataTypes
            {
                Id = 54,
                DecimalAsDecimal52 = decimalAsDecimal52,
                DecimalAsDec52 = decimalAsDec52,
                DecimalAsNumeric52 = decimalAsNumeric52
            }).Entity;

        context.SaveChanges();
        var id = entity.Id;
        var set = context.Set<MappedPrecisionAndScaledDataTypes>();

        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsDecimal52 == decimalAsDecimal52));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsDec52 == decimalAsDec52));
        ExecuteQueryString(context, id, set.Where(e => e.Id == id && e.DecimalAsNumeric52 == decimalAsNumeric52));
    }

    [ConditionalFact]
    public virtual void Can_query_using_debug_string_for_non_integer_values()
    {
        using var context = CreateContext();

        var accumulator0 = 1L;
        var accumulator1 = 1L;
        for (var i = 0; i < 100; i++)
        {
            var temp = accumulator1;
            accumulator1 += accumulator0;
            accumulator0 = temp;

            var @double = ((double)accumulator1) / accumulator0;
            var @float = ((float)accumulator1) / accumulator0;

            var entity = context.Add(
                new MappedNullableDataTypesWithIdentity { DoubleAsFloat = @double, FloatAsReal = @float }).Entity;

            context.SaveChanges();
            var id = entity.Id;

            ExecuteQueryString(
                context, id, context.Set<MappedNullableDataTypesWithIdentity>().Where(
                    e => e.Id == id && e.DoubleAsFloat == @double && e.FloatAsReal == @float));
        }
    }

    private void ExecuteQueryString(DbContext context, int expectedId, IQueryable queryable)
    {
        var queryString = queryable.ToQueryString();
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = queryString;
        using var reader = command.ExecuteReader();

        Assert.True(reader.HasRows);
        Assert.True(reader.Read());
        Assert.Equal(expectedId, reader.GetFieldValue<int>(reader.GetOrdinal("Id")));
        Assert.False(reader.Read());
        reader.Close();
    }

    [ConditionalFact]
    public void Sql_translation_uses_type_mapper_when_constant()
    {
        using var context = CreateContext();
        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => e.TimeSpanAsTime == new TimeSpan(0, 1, 2))
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);

        AssertSql(
            """
SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE [m].[TimeSpanAsTime] = '00:01:02'
""");
    }

    [ConditionalFact]
    public void Sql_translation_uses_type_mapper_when_parameter()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => e.TimeSpanAsTime == timeSpan)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_0='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE [m].[TimeSpanAsTime] = @__timeSpan_0
""");
    }

    [ConditionalFact]
    public void String_indexOf_over_varchar_max()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(
                new MappedNullableDataTypes { Int = 81, StringAsVarcharMax = string.Concat(Enumerable.Repeat("C", 8001)) });

            Assert.Equal(1, context.SaveChanges());
        }

        Fixture.TestSqlLoggerFactory.Clear();

        using (var context = CreateContext())
        {
            var results = context.Set<MappedNullableDataTypes>()
                .Where(e => e.Int == 81)
                .Select(m => m.StringAsVarcharMax.IndexOf("a"))
                .ToList();

            Assert.Equal(-1, Assert.Single(results));
            AssertSql(
                """
SELECT CAST(CHARINDEX('a', [m].[StringAsVarcharMax]) AS int) - 1
FROM [MappedNullableDataTypes] AS [m]
WHERE [m].[Int] = 81
""");
        }
    }

    [ConditionalFact]
    public virtual void Can_query_using_DateDiffHour_using_TimeSpan()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => EF.Functions.DateDiffHour(e.TimeSpanAsTime, timeSpan) == 0)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_1='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE DATEDIFF(hour, [m].[TimeSpanAsTime], @__timeSpan_1) = 0
""");
    }

    [ConditionalFact]
    public virtual void Can_query_using_DateDiffMinute_using_TimeSpan()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => EF.Functions.DateDiffMinute(e.TimeSpanAsTime, timeSpan) == 0)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_1='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE DATEDIFF(minute, [m].[TimeSpanAsTime], @__timeSpan_1) = 0
""");
    }

    [ConditionalFact]
    public virtual void Can_query_using_DateDiffSecond_using_TimeSpan()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => EF.Functions.DateDiffSecond(e.TimeSpanAsTime, timeSpan) == 0)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_1='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE DATEDIFF(second, [m].[TimeSpanAsTime], @__timeSpan_1) = 0
""");
    }

    [ConditionalFact]
    public virtual void Can_query_using_DateDiffMillisecond_using_TimeSpan()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => EF.Functions.DateDiffMillisecond(e.TimeSpanAsTime, timeSpan) == 0)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_1='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE DATEDIFF(millisecond, [m].[TimeSpanAsTime], @__timeSpan_1) = 0
""");
    }

    [ConditionalFact]
    public virtual void Can_query_using_DateDiffMicrosecond_using_TimeSpan()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => EF.Functions.DateDiffMicrosecond(e.TimeSpanAsTime, timeSpan) == 0)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_1='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE DATEDIFF(microsecond, [m].[TimeSpanAsTime], @__timeSpan_1) = 0
""");
    }

    [ConditionalFact]
    public virtual void Can_query_using_DateDiffNanosecond_using_TimeSpan()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);

        var results
            = context.Set<MappedNullableDataTypes>()
                .Where(e => EF.Functions.DateDiffNanosecond(e.TimeSpanAsTime, timeSpan) == 0)
                .Select(e => e.Int)
                .ToList();

        Assert.Empty(results);
        AssertSql(
            """
@__timeSpan_1='02:01:00' (Nullable = true)

SELECT [m].[Int]
FROM [MappedNullableDataTypes] AS [m]
WHERE DATEDIFF(nanosecond, [m].[TimeSpanAsTime], @__timeSpan_1) = 0
""");
    }

    [ConditionalFact]
    public virtual void Can_query_using_any_mapped_data_type()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(
                new MappedNullableDataTypes
                {
                    Int = 999,
                    LongAsBigint = 78L,
                    ShortAsSmallint = 79,
                    ByteAsTinyint = 80,
                    UintAsInt = uint.MaxValue,
                    UlongAsBigint = ulong.MaxValue,
                    UShortAsSmallint = ushort.MaxValue,
                    SbyteAsTinyint = sbyte.MinValue,
                    BoolAsBit = true,
                    DecimalAsMoney = 81.1m,
                    DecimalAsSmallmoney = 82.2m,
                    DoubleAsFloat = 83.3,
                    FloatAsReal = 84.4f,
                    DoubleAsDoublePrecision = 85.5,
                    DateOnlyAsDate = new DateOnly(1605, 1, 2),
                    DateTimeAsDate = new DateTime(1605, 1, 2, 10, 11, 12),
                    DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(), TimeSpan.Zero),
                    DateTimeAsDatetime2 = new DateTime(),
                    DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                    DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                    TimeOnlyAsTime = new TimeOnly(11, 15, 12, 2),
                    TimeSpanAsTime = new TimeSpan(0, 11, 15, 12, 2),
                    StringAsVarcharMax = "C",
                    StringAsCharVaryingMax = "Your",
                    StringAsCharacterVaryingMax = "strong",
                    StringAsNvarcharMax = "don't",
                    StringAsNationalCharVaryingMax = "help",
                    StringAsNationalCharacterVaryingMax = "anyone!",
                    StringAsText = "Gumball Rules!",
                    StringAsNtext = "Gumball Rules OK!",
                    BytesAsVarbinaryMax = [89, 90, 91, 92],
                    BytesAsBinaryVaryingMax = [93, 94, 95, 96],
                    BytesAsImage = [97, 98, 99, 100],
                    Decimal = 101.7m,
                    DecimalAsDec = 102.8m,
                    DecimalAsNumeric = 103.9m,
                    GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                    UintAsBigint = uint.MaxValue,
                    UlongAsDecimal200 = ulong.MaxValue,
                    UShortAsInt = ushort.MaxValue,
                    SByteAsSmallint = sbyte.MinValue,
                    CharAsVarchar = 'A',
                    CharAsAsCharVarying = 'B',
                    CharAsCharacterVaryingMax = 'C',
                    CharAsNvarchar = 'D',
                    CharAsNationalCharVarying = 'E',
                    CharAsNationalCharacterVaryingMax = 'F',
                    CharAsText = 'G',
                    CharAsNtext = 'H',
                    CharAsInt = 'I',
                    EnumAsNvarchar20 = StringEnumU16.Value4,
                    EnumAsVarcharMax = StringEnum16.Value2,
                    SqlVariantString = "Bang!",
                    SqlVariantInt = 887876
                });

            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999);

            long? param1 = 78L;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.LongAsBigint == param1));

            short? param2 = 79;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ShortAsSmallint == param2));

            byte? param3 = 80;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ByteAsTinyint == param3));

            bool? param4 = true;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BoolAsBit == param4));

            decimal? param5 = 81.1m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsMoney == param5));

            decimal? param6 = 82.2m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsSmallmoney == param6));

            double? param7a = 83.3;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DoubleAsFloat == param7a));

            float? param7b = 84.4f;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.FloatAsReal == param7b));

            double? param7c = 85.5;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DoubleAsDoublePrecision == param7c));

            DateOnly? param8a = new DateOnly(1605, 1, 2);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateOnlyAsDate == param8a));

            DateTime? param8b = new DateTime(1605, 1, 2);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDate == param8b));

            DateTimeOffset? param9 = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeOffsetAsDatetimeoffset == param9));

            DateTime? param10 = new DateTime();
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDatetime2 == param10));

            DateTime? param11 = new DateTime(2019, 1, 2, 14, 11, 12);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDatetime == param11));

            DateTime? param12 = new DateTime(2018, 1, 2, 13, 11, 0);
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsSmalldatetime == param12));

            TimeOnly? param13a = new TimeOnly(11, 15, 12, 2);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeOnlyAsTime == param13a));

            TimeSpan? param13b = new TimeSpan(0, 11, 15, 12, 2);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeSpanAsTime == param13b));

            var param19 = "C";
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsVarcharMax == param19));

            var param20 = "Your";
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsCharVaryingMax == param20));

            var param21 = "strong";
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsCharacterVaryingMax == param21));

            var param27 = "don't";
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsNvarcharMax == param27));

            var param28 = "help";
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsNationalCharVaryingMax == param28));

            var param29 = "anyone!";
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsNationalCharacterVaryingMax == param29));

            var param35 = new byte[] { 89, 90, 91, 92 };
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BytesAsVarbinaryMax == param35));

            var param36 = new byte[] { 93, 94, 95, 96 };
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BytesAsBinaryVaryingMax == param36));

            decimal? param38 = 102m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Decimal == param38));

            decimal? param39 = 103m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsDec == param39));

            decimal? param40 = 104m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsNumeric == param40));

            uint? param41 = uint.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsInt == param41));

            ulong? param42 = ulong.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UlongAsBigint == param42));

            ushort? param43 = ushort.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsSmallint == param43));

            sbyte? param44 = sbyte.MinValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SbyteAsTinyint == param44));

            uint? param45 = uint.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsBigint == param45));

            ulong? param46 = ulong.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UlongAsDecimal200 == param46));

            ushort? param47 = ushort.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsInt == param47));

            sbyte? param48 = sbyte.MinValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SByteAsSmallint == param48));

            Guid? param49 = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47");
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.GuidAsUniqueidentifier == param49));

            char? param50 = 'A';
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsVarchar == param50));

            char? param51 = 'B';
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsAsCharVarying == param51));

            char? param52 = 'C';
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsCharacterVaryingMax == param52));

            char? param53 = 'D';
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsNvarchar == param53));

            char? param54 = 'E';
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsNationalCharVarying == param54));

            char? param55 = 'F';
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsNationalCharacterVaryingMax == param55));

            char? param58 = 'I';
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsInt == param58));

            StringEnumU16? param59 = StringEnumU16.Value4;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.EnumAsNvarchar20 == param59));

            StringEnum16? param60 = StringEnum16.Value2;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.EnumAsVarcharMax == param60));

            object param61 = "Bang!";
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SqlVariantString.Equals(param61)));

            object param62 = 887876;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SqlVariantInt.Equals(param62)));
        }
    }

    [ConditionalFact]
    public virtual void Can_query_using_any_mapped_data_types_with_nulls()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(
                new MappedNullableDataTypes { Int = 911 });

            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911);

            long? param1 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.LongAsBigint == param1));

            short? param2 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ShortAsSmallint == param2));
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)e.ShortAsSmallint == param2));

            byte? param3 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ByteAsTinyint == param3));

            bool? param4 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BoolAsBit == param4));

            decimal? param5 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsMoney == param5));

            decimal? param6 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsSmallmoney == param6));

            double? param7a = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsFloat == param7a));

            float? param7b = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.FloatAsReal == param7b));

            double? param7c = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsDoublePrecision == param7c));

            DateOnly? param8a = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateOnlyAsDate == param8a));

            DateTime? param8b = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDate == param8b));

            DateTimeOffset? param9 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeOffsetAsDatetimeoffset == param9));

            DateTime? param10 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDatetime2 == param10));

            DateTime? param11 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDatetime == param11));

            DateTime? param12 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsSmalldatetime == param12));

            TimeOnly? param13a = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeOnlyAsTime == param13a));

            TimeSpan? param13b = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsTime == param13b));

            string param19 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsVarcharMax == param19));

            string param20 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsCharVaryingMax == param20));

            string param21 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsCharacterVaryingMax == param21));

            string param27 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNvarcharMax == param27));

            string param28 = null;
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNationalCharVaryingMax == param28));

            string param29 = null;
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNationalCharacterVaryingMax == param29));

            string param30 = null;

            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsText == param30));

            string param31 = null;
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNtext == param31));

            byte[] param35 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsVarbinaryMax == param35));

            byte[] param36 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsBinaryVaryingMax == param36));

            byte[] param37 = null;
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsImage == param37));

            decimal? param38 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Decimal == param38));

            decimal? param39 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsDec == param39));

            decimal? param40 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsNumeric == param40));

            uint? param41 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsInt == param41));

            ulong? param42 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsBigint == param42));

            ushort? param43 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsSmallint == param43));

            sbyte? param44 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SbyteAsTinyint == param44));

            uint? param45 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsBigint == param45));

            ulong? param46 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsDecimal200 == param46));

            ushort? param47 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsInt == param47));

            sbyte? param48 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SByteAsSmallint == param48));

            Guid? param49 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.GuidAsUniqueidentifier == param49));

            char? param50 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsVarchar == param50));

            char? param51 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsAsCharVarying == param51));

            char? param52 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsCharacterVaryingMax == param52));

            char? param53 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNvarchar == param53));

            char? param54 = null;
            Assert.Same(
                entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNationalCharVarying == param54));

            char? param55 = null;
            Assert.Same(
                entity,
                context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNationalCharacterVaryingMax == param55));

            //char? param56 = null;
            //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsText == param56));

            //char? param57 = null;
            //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNtext == param57));

            char? param58 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsInt == param58));

            StringEnumU16? param59 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsNvarchar20 == param59));

            StringEnum16? param60 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsVarcharMax == param60));

            object param61 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SqlVariantString == param61));
            object param62 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SqlVariantInt == param62));
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types()
    {
        var entity = CreateMappedDataTypes(77);
        using (var context = CreateContext())
        {
            context.Set<MappedDataTypes>().Add(entity);

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            $"""
@p0='77'
@p1='True'
@p2='80' (Size = 1)
@p3='0x5D5E5F60' (Nullable = false) (Size = 8000)
@p4='0x61626364' (Nullable = false) (Size = 8000)
@p5='0x595A5B5C' (Nullable = false) (Size = 8000)
@p6='B' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p7='C' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p8='73'
@p9='E' (Nullable = false) (Size = 1)
@p10='F' (Nullable = false) (Size = 4000)
@p11='H' (Nullable = false) (Size = 1)
@p12='D' (Nullable = false) (Size = 1)
@p13='G' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p14='A' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p15='01/02/2015' (DbType = Date)
@p16='2015-01-02T10:11:12.0000000' (DbType = Date)
@p17='2019-01-02T14:11:12.0000000' (DbType = DateTime)
@p18='2017-01-02T12:11:12.1234567'
@p19='2018-01-02T13:11:12.0000000' (DbType = DateTime)
@p20='2016-01-02T11:11:12.1234567+00:00'
@p21='101' (Precision = 18)
@p22='102' (Precision = 18)
@p23='81.1' (DbType = Currency)
@p24='103' (Precision = 18)
@p25='82.2' (DbType = Currency)
@p26='85.5'
@p27='83.3'
@p28='Value4' (Nullable = false) (Size = 20)
@p29='Value2' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p30='84.4'
@p31='a8f9f951-145f-4545-ac60-b92ff57ada47'
@p32='78'
@p33='-128'
@p34='128' (Size = 1)
@p35='79'
@p36='887876' (DbType = Object)
@p37='Bang!' (Nullable = false) (Size = 5) (DbType = Object)
@p38='Your' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p39='And now' (Nullable = false) (Size = 4000)
@p40='strong' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p41='this...' (Nullable = false) (Size = 4000)
@p42='help' (Nullable = false) (Size = 4000)
@p43='anyone!' (Nullable = false) (Size = 4000)
@p44='Gumball Rules OK!' (Nullable = false) (Size = 4000)
@p45='{entity.StringAsNvarcharMax}' (Nullable = false) (Size = -1)
@p46='Gumball Rules!' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p47='{entity.StringAsVarcharMax}' (Nullable = false) (Size = -1) (DbType = AnsiString)
@p48='{entity.StringAsVarcharMaxUtf8}' (Nullable = false) (Size = -1)
@p49='11:15' (DbType = Time)
@p50='11:15:12'
@p51='65535'
@p52='-1'
@p53='4294967295'
@p54='-1'
@p55='-1'
@p56='18446744073709551615' (Precision = 20)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 77), 77);
        }
    }

    private string DumpParameters()
        => Fixture.TestSqlLoggerFactory.Parameters.Single().Replace(", ", _eol);

    private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
    {
        var expected = CreateMappedDataTypes(id);
        Assert.Equal(id, entity.Int);
        Assert.Equal(78, entity.LongAsBigInt);
        Assert.Equal(79, entity.ShortAsSmallint);
        Assert.Equal(80, entity.ByteAsTinyint);
        Assert.Equal(uint.MaxValue, entity.UintAsInt);
        Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
        Assert.Equal(sbyte.MinValue, entity.SByteAsTinyint);
        Assert.True(entity.BoolAsBit);
        Assert.Equal(81.1m, entity.DecimalAsMoney);
        Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
        Assert.Equal(83.3, entity.DoubleAsFloat);
        Assert.Equal(84.4f, entity.FloatAsReal);
        Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
        Assert.Equal(new DateOnly(2015, 1, 2), entity.DateOnlyAsDate);
        Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(1234567), TimeSpan.Zero),
            entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(1234567), entity.DateTimeAsDatetime2);
        Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
        Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
        Assert.Equal(new TimeOnly(11, 15, 12), entity.TimeOnlyAsTime);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
        Assert.Equal(expected.StringAsVarcharMax, entity.StringAsVarcharMax);
        Assert.Equal("Your", entity.StringAsCharVaryingMax);
        Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
        Assert.Equal(expected.StringAsNvarcharMax, entity.StringAsNvarcharMax);
        Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
        Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
        Assert.Equal(expected.StringAsVarcharMaxUtf8, entity.StringAsVarcharMaxUtf8);
        Assert.Equal("And now", entity.StringAsCharVaryingMaxUtf8);
        Assert.Equal("this...", entity.StringAsCharacterVaryingMaxUtf8);
        Assert.Equal("Gumball Rules!", entity.StringAsText);
        Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
        Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
        Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinaryVaryingMax);
        Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
        Assert.Equal(101m, entity.Decimal);
        Assert.Equal(102m, entity.DecimalAsDec);
        Assert.Equal(103m, entity.DecimalAsNumeric);
        Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
        Assert.Equal(uint.MaxValue, entity.UintAsBigint);
        Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
        Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
        Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
        Assert.Equal('A', entity.CharAsVarchar);
        Assert.Equal('B', entity.CharAsAsCharVarying);
        Assert.Equal('C', entity.CharAsCharacterVaryingMax);
        Assert.Equal('D', entity.CharAsNvarchar);
        Assert.Equal('E', entity.CharAsNationalCharVarying);
        Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
        Assert.Equal('G', entity.CharAsText);
        Assert.Equal('H', entity.CharAsNtext);
        Assert.Equal('I', entity.CharAsInt);
        Assert.Equal(StringEnum16.Value2, entity.EnumAsVarcharMax);
        Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
        Assert.Equal("Bang!", entity.SqlVariantString);
        Assert.Equal(887876, entity.SqlVariantInt);
    }

    private static MappedDataTypes CreateMappedDataTypes(int id)
        => new()
        {
            Int = id,
            LongAsBigInt = 78L,
            ShortAsSmallint = 79,
            ByteAsTinyint = 80,
            UintAsInt = uint.MaxValue,
            UlongAsBigint = ulong.MaxValue,
            UShortAsSmallint = ushort.MaxValue,
            SByteAsTinyint = sbyte.MinValue,
            BoolAsBit = true,
            DecimalAsMoney = 81.1m,
            DecimalAsSmallmoney = 82.2m,
            DoubleAsFloat = 83.3,
            FloatAsReal = 84.4f,
            DoubleAsDoublePrecision = 85.5,
            DateOnlyAsDate = new DateOnly(2015, 1, 2),
            DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
            DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(1234567), TimeSpan.Zero),
            DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(1234567),
            DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
            DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
            TimeOnlyAsTime = new TimeOnly(11, 15, 12),
            TimeSpanAsTime = new TimeSpan(11, 15, 12),
            StringAsVarcharMax = string.Concat(Enumerable.Repeat("C", 8001)),
            StringAsCharVaryingMax = "Your",
            StringAsCharacterVaryingMax = "strong",
            StringAsNvarcharMax = string.Concat(Enumerable.Repeat("D", 4001)),
            StringAsNationalCharVaryingMax = "help",
            StringAsNationalCharacterVaryingMax = "anyone!",
            StringAsVarcharMaxUtf8 = string.Concat(Enumerable.Repeat("E", 4001)),
            StringAsCharVaryingMaxUtf8 = "And now",
            StringAsCharacterVaryingMaxUtf8 = "this...",
            StringAsText = "Gumball Rules!",
            StringAsNtext = "Gumball Rules OK!",
            BytesAsVarbinaryMax = [89, 90, 91, 92],
            BytesAsBinaryVaryingMax = [93, 94, 95, 96],
            BytesAsImage = [97, 98, 99, 100],
            Decimal = 101m,
            DecimalAsDec = 102m,
            DecimalAsNumeric = 103m,
            GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
            UintAsBigint = uint.MaxValue,
            UlongAsDecimal200 = ulong.MaxValue,
            UShortAsInt = ushort.MaxValue,
            SByteAsSmallint = sbyte.MinValue,
            CharAsVarchar = 'A',
            CharAsAsCharVarying = 'B',
            CharAsCharacterVaryingMax = 'C',
            CharAsNvarchar = 'D',
            CharAsNationalCharVarying = 'E',
            CharAsNationalCharacterVaryingMax = 'F',
            CharAsText = 'G',
            CharAsNtext = 'H',
            CharAsInt = 'I',
            EnumAsNvarchar20 = StringEnumU16.Value4,
            EnumAsVarcharMax = StringEnum16.Value2,
            SqlVariantString = "Bang!",
            SqlVariantInt = 887876
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_square_brackets()
    {
        var entity = CreateMappedSquareDataTypes(77);
        using (var context = CreateContext())
        {
            context.Set<MappedSquareDataTypes>().Add(entity);

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            $"""
@p0='77'
@p1='True'
@p2='80' (Size = 1)
@p3='0x61626364' (Nullable = false) (Size = 8000)
@p4='0x595A5B5C' (Nullable = false) (Size = 8000)
@p5='73'
@p6='H' (Nullable = false) (Size = 1)
@p7='D' (Nullable = false) (Size = 1)
@p8='G' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p9='A' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p10='01/02/2015' (DbType = Date)
@p11='2015-01-02T10:11:12.0000000' (DbType = Date)
@p12='2019-01-02T14:11:12.0000000' (DbType = DateTime)
@p13='2017-01-02T12:11:12.1234567'
@p14='2018-01-02T13:11:12.0000000' (DbType = DateTime)
@p15='2016-01-02T11:11:12.1234567+00:00'
@p16='101' (Precision = 18)
@p17='102' (Precision = 18)
@p18='81.1' (DbType = Currency)
@p19='103' (Precision = 18)
@p20='82.2' (DbType = Currency)
@p21='83.3'
@p22='Value4' (Nullable = false) (Size = 20)
@p23='Value2' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p24='84.4'
@p25='a8f9f951-145f-4545-ac60-b92ff57ada47'
@p26='78'
@p27='-128'
@p28='128' (Size = 1)
@p29='79'
@p30='887876' (DbType = Object)
@p31='Bang!' (Nullable = false) (Size = 5) (DbType = Object)
@p32='Gumball Rules OK!' (Nullable = false) (Size = 4000)
@p33='{entity.StringAsNvarcharMax}' (Nullable = false) (Size = -1)
@p34='Gumball Rules!' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p35='{entity.StringAsVarcharMax}' (Nullable = false) (Size = -1) (DbType = AnsiString)
@p36='11:15' (DbType = Time)
@p37='11:15:12'
@p38='65535'
@p39='-1'
@p40='4294967295'
@p41='-1'
@p42='-1'
@p43='18446744073709551615' (Precision = 20)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedSquareDataTypes(context.Set<MappedSquareDataTypes>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedSquareDataTypes(MappedSquareDataTypes entity, int id)
    {
        var expected = CreateMappedSquareDataTypes(id);
        Assert.Equal(id, entity.Int);
        Assert.Equal(78, entity.LongAsBigInt);
        Assert.Equal(79, entity.ShortAsSmallint);
        Assert.Equal(80, entity.ByteAsTinyint);
        Assert.Equal(uint.MaxValue, entity.UintAsInt);
        Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
        Assert.Equal(sbyte.MinValue, entity.SByteAsTinyint);
        Assert.True(entity.BoolAsBit);
        Assert.Equal(81.1m, entity.DecimalAsMoney);
        Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
        Assert.Equal(83.3, entity.DoubleAsFloat);
        Assert.Equal(84.4f, entity.FloatAsReal);
        Assert.Equal(new DateOnly(2015, 1, 2), entity.DateOnlyAsDate);
        Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(1234567), TimeSpan.Zero),
            entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(1234567), entity.DateTimeAsDatetime2);
        Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
        Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
        Assert.Equal(new TimeOnly(11, 15, 12), entity.TimeOnlyAsTime);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
        Assert.Equal(expected.StringAsVarcharMax, entity.StringAsVarcharMax);
        Assert.Equal(expected.StringAsNvarcharMax, entity.StringAsNvarcharMax);
        Assert.Equal("Gumball Rules!", entity.StringAsText);
        Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
        Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
        Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
        Assert.Equal(101m, entity.Decimal);
        Assert.Equal(102m, entity.DecimalAsDec);
        Assert.Equal(103m, entity.DecimalAsNumeric);
        Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
        Assert.Equal(uint.MaxValue, entity.UintAsBigint);
        Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
        Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
        Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
        Assert.Equal('A', entity.CharAsVarchar);
        Assert.Equal('D', entity.CharAsNvarchar);
        Assert.Equal('G', entity.CharAsText);
        Assert.Equal('H', entity.CharAsNtext);
        Assert.Equal('I', entity.CharAsInt);
        Assert.Equal(StringEnum16.Value2, entity.EnumAsVarcharMax);
        Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
        Assert.Equal("Bang!", entity.SqlVariantString);
        Assert.Equal(887876, entity.SqlVariantInt);
    }

    private static MappedSquareDataTypes CreateMappedSquareDataTypes(int id)
        => new()
        {
            Int = id,
            LongAsBigInt = 78L,
            ShortAsSmallint = 79,
            ByteAsTinyint = 80,
            UintAsInt = uint.MaxValue,
            UlongAsBigint = ulong.MaxValue,
            UShortAsSmallint = ushort.MaxValue,
            SByteAsTinyint = sbyte.MinValue,
            BoolAsBit = true,
            DecimalAsMoney = 81.1m,
            DecimalAsSmallmoney = 82.2m,
            DoubleAsFloat = 83.3,
            FloatAsReal = 84.4f,
            DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
            DateOnlyAsDate = new DateOnly(2015, 1, 2),
            DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(1234567), TimeSpan.Zero),
            DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(1234567),
            DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
            DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
            TimeSpanAsTime = new TimeSpan(11, 15, 12),
            TimeOnlyAsTime = new TimeOnly(11, 15, 12),
            StringAsVarcharMax = string.Concat(Enumerable.Repeat("C", 8001)),
            StringAsNvarcharMax = string.Concat(Enumerable.Repeat("D", 4001)),
            StringAsText = "Gumball Rules!",
            StringAsNtext = "Gumball Rules OK!",
            BytesAsVarbinaryMax = [89, 90, 91, 92],
            BytesAsImage = [97, 98, 99, 100],
            Decimal = 101m,
            DecimalAsDec = 102m,
            DecimalAsNumeric = 103m,
            GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
            UintAsBigint = uint.MaxValue,
            UlongAsDecimal200 = ulong.MaxValue,
            UShortAsInt = ushort.MaxValue,
            SByteAsSmallint = sbyte.MinValue,
            CharAsVarchar = 'A',
            CharAsNvarchar = 'D',
            CharAsText = 'G',
            CharAsNtext = 'H',
            CharAsInt = 'I',
            EnumAsNvarchar20 = StringEnumU16.Value4,
            EnumAsVarcharMax = StringEnum16.Value2,
            SqlVariantString = "Bang!",
            SqlVariantInt = 887876
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='77'
@p1='True' (Nullable = true)
@p2='80' (Nullable = true) (Size = 1)
@p3='0x5D5E5F60' (Size = 8000)
@p4='0x61626364' (Size = 8000)
@p5='0x595A5B5C' (Size = 8000)
@p6='B' (Size = 1) (DbType = AnsiString)
@p7='C' (Size = 8000) (DbType = AnsiString)
@p8='73' (Nullable = true)
@p9='E' (Size = 1)
@p10='F' (Size = 4000)
@p11='H' (Size = 1)
@p12='D' (Size = 1)
@p13='G' (Size = 1) (DbType = AnsiString)
@p14='A' (Size = 1) (DbType = AnsiString)
@p15='01/02/2015' (Nullable = true) (DbType = Date)
@p16='2015-01-02T10:11:12.0000000' (Nullable = true) (DbType = Date)
@p17='2019-01-02T14:11:12.0000000' (Nullable = true) (DbType = DateTime)
@p18='2017-01-02T12:11:12.9876543' (Nullable = true)
@p19='2018-01-02T13:11:12.0000000' (Nullable = true) (DbType = DateTime)
@p20='2016-01-02T11:11:12.9876543+00:00' (Nullable = true)
@p21='101' (Nullable = true) (Precision = 18)
@p22='102' (Nullable = true) (Precision = 18)
@p23='81.1' (Nullable = true) (DbType = Currency)
@p24='103' (Nullable = true) (Precision = 18)
@p25='82.2' (Nullable = true) (DbType = Currency)
@p26='85.5' (Nullable = true)
@p27='83.3' (Nullable = true)
@p28='Value4' (Size = 20)
@p29='Value2' (Size = 8000) (DbType = AnsiString)
@p30='84.4' (Nullable = true)
@p31='a8f9f951-145f-4545-ac60-b92ff57ada47' (Nullable = true)
@p32='78' (Nullable = true)
@p33='-128' (Nullable = true)
@p34='128' (Nullable = true) (Size = 1)
@p35='79' (Nullable = true)
@p36='887876' (Nullable = true) (DbType = Object)
@p37='Bang!' (Size = 5) (DbType = Object)
@p38='Your' (Size = 8000) (DbType = AnsiString)
@p39='And now' (Size = 4000)
@p40='strong' (Size = 8000) (DbType = AnsiString)
@p41='this...' (Size = 4000)
@p42='help' (Size = 4000)
@p43='anyone!' (Size = 4000)
@p44='Gumball Rules OK!' (Size = 4000)
@p45='don't' (Size = 4000)
@p46='Gumball Rules!' (Size = 8000) (DbType = AnsiString)
@p47='C' (Size = 8000) (DbType = AnsiString)
@p48='short' (Size = 4000)
@p49='11:15' (Nullable = true) (DbType = Time)
@p50='11:15:12' (Nullable = true)
@p51='65535' (Nullable = true)
@p52='-1' (Nullable = true)
@p53='4294967295' (Nullable = true)
@p54='-1' (Nullable = true)
@p55='-1' (Nullable = true)
@p56='18446744073709551615' (Nullable = true) (Precision = 20)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Equal(78, entity.LongAsBigint);
        Assert.Equal(79, entity.ShortAsSmallint.Value);
        Assert.Equal(80, entity.ByteAsTinyint.Value);
        Assert.Equal(uint.MaxValue, entity.UintAsInt);
        Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
        Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
        Assert.True(entity.BoolAsBit);
        Assert.Equal(81.1m, entity.DecimalAsMoney);
        Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
        Assert.Equal(83.3, entity.DoubleAsFloat);
        Assert.Equal(84.4f, entity.FloatAsReal);
        Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
        Assert.Equal(new DateOnly(2015, 1, 2), entity.DateOnlyAsDate);
        Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(9876543), TimeSpan.Zero),
            entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(9876543), entity.DateTimeAsDatetime2);
        Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
        Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
        Assert.Equal(new TimeOnly(11, 15, 12), entity.TimeOnlyAsTime);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
        Assert.Equal("C", entity.StringAsVarcharMax);
        Assert.Equal("Your", entity.StringAsCharVaryingMax);
        Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
        Assert.Equal("don't", entity.StringAsNvarcharMax);
        Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
        Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
        Assert.Equal("Gumball Rules!", entity.StringAsText);
        Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
        Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
        Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinaryVaryingMax);
        Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
        Assert.Equal(101m, entity.Decimal);
        Assert.Equal(102m, entity.DecimalAsDec);
        Assert.Equal(103m, entity.DecimalAsNumeric);
        Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
        Assert.Equal(uint.MaxValue, entity.UintAsBigint);
        Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
        Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
        Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
        Assert.Equal('A', entity.CharAsVarchar);
        Assert.Equal('B', entity.CharAsAsCharVarying);
        Assert.Equal('C', entity.CharAsCharacterVaryingMax);
        Assert.Equal('D', entity.CharAsNvarchar);
        Assert.Equal('E', entity.CharAsNationalCharVarying);
        Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
        Assert.Equal('G', entity.CharAsText);
        Assert.Equal('H', entity.CharAsNtext);
        Assert.Equal('I', entity.CharAsInt);
        Assert.Equal(StringEnum16.Value2, entity.EnumAsVarcharMax);
        Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
        Assert.Equal("Bang!", entity.SqlVariantString);
        Assert.Equal(887876, entity.SqlVariantInt);
    }

    private static MappedNullableDataTypes CreateMappedNullableDataTypes(int id)
        => new()
        {
            Int = id,
            LongAsBigint = 78L,
            ShortAsSmallint = 79,
            ByteAsTinyint = 80,
            UintAsInt = uint.MaxValue,
            UlongAsBigint = ulong.MaxValue,
            UShortAsSmallint = ushort.MaxValue,
            SbyteAsTinyint = sbyte.MinValue,
            BoolAsBit = true,
            DecimalAsMoney = 81.1m,
            DecimalAsSmallmoney = 82.2m,
            DoubleAsFloat = 83.3,
            FloatAsReal = 84.4f,
            DoubleAsDoublePrecision = 85.5,
            DateOnlyAsDate = new DateOnly(2015, 1, 2),
            DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
            DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(9876543), TimeSpan.Zero),
            DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(9876543),
            DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
            DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
            TimeOnlyAsTime = new TimeOnly(11, 15, 12),
            TimeSpanAsTime = new TimeSpan(11, 15, 12),
            StringAsVarcharMax = "C",
            StringAsCharVaryingMax = "Your",
            StringAsCharacterVaryingMax = "strong",
            StringAsNvarcharMax = "don't",
            StringAsNationalCharVaryingMax = "help",
            StringAsNationalCharacterVaryingMax = "anyone!",
            StringAsVarcharMaxUtf8 = "short",
            StringAsCharVaryingMaxUtf8 = "And now",
            StringAsCharacterVaryingMaxUtf8 = "this...",
            StringAsText = "Gumball Rules!",
            StringAsNtext = "Gumball Rules OK!",
            BytesAsVarbinaryMax = [89, 90, 91, 92],
            BytesAsBinaryVaryingMax = [93, 94, 95, 96],
            BytesAsImage = [97, 98, 99, 100],
            Decimal = 101m,
            DecimalAsDec = 102m,
            DecimalAsNumeric = 103m,
            GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
            UintAsBigint = uint.MaxValue,
            UlongAsDecimal200 = ulong.MaxValue,
            UShortAsInt = ushort.MaxValue,
            SByteAsSmallint = sbyte.MinValue,
            CharAsVarchar = 'A',
            CharAsAsCharVarying = 'B',
            CharAsCharacterVaryingMax = 'C',
            CharAsNvarchar = 'D',
            CharAsNationalCharVarying = 'E',
            CharAsNationalCharacterVaryingMax = 'F',
            CharAsText = 'G',
            CharAsNtext = 'H',
            CharAsInt = 'I',
            EnumAsNvarchar20 = StringEnumU16.Value4,
            EnumAsVarcharMax = StringEnum16.Value2,
            SqlVariantString = "Bang!",
            SqlVariantInt = 887876
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 78 });

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='78'
@p1=NULL (DbType = Boolean)
@p2=NULL (DbType = Byte)
@p3=NULL (Size = 8000) (DbType = Binary)
@p4=NULL (Size = 8000) (DbType = Binary)
@p5=NULL (Size = 8000) (DbType = Binary)
@p6=NULL (Size = 1) (DbType = AnsiString)
@p7=NULL (Size = 8000) (DbType = AnsiString)
@p8=NULL (DbType = Int32)
@p9=NULL (Size = 1)
@p10=NULL (Size = 4000)
@p11=NULL (Size = 1)
@p12=NULL (Size = 1)
@p13=NULL (Size = 1) (DbType = AnsiString)
@p14=NULL (Size = 1) (DbType = AnsiString)
@p15=NULL (DbType = Date)
@p16=NULL (DbType = Date)
@p17=NULL (DbType = DateTime)
@p18=NULL (DbType = DateTime2)
@p19=NULL (DbType = DateTime)
@p20=NULL (DbType = DateTimeOffset)
@p21=NULL (Precision = 18) (DbType = Decimal)
@p22=NULL (Precision = 18) (DbType = Decimal)
@p23=NULL (DbType = Currency)
@p24=NULL (Precision = 18) (DbType = Decimal)
@p25=NULL (DbType = Currency)
@p26=NULL (DbType = Double)
@p27=NULL (DbType = Double)
@p28=NULL (Size = 20)
@p29=NULL (Size = 8000) (DbType = AnsiString)
@p30=NULL (DbType = Single)
@p31=NULL (DbType = Guid)
@p32=NULL (DbType = Int64)
@p33=NULL (DbType = Int16)
@p34=NULL (DbType = Byte)
@p35=NULL (DbType = Int16)
@p36=NULL (DbType = Object)
@p37=NULL (DbType = Object)
@p38=NULL (Size = 8000) (DbType = AnsiString)
@p39=NULL (Size = 4000)
@p40=NULL (Size = 8000) (DbType = AnsiString)
@p41=NULL (Size = 4000)
@p42=NULL (Size = 4000)
@p43=NULL (Size = 4000)
@p44=NULL (Size = 4000)
@p45=NULL (Size = 4000)
@p46=NULL (Size = 8000) (DbType = AnsiString)
@p47=NULL (Size = 8000) (DbType = AnsiString)
@p48=NULL (Size = 4000)
@p49=NULL (DbType = Time)
@p50=NULL (DbType = Time)
@p51=NULL (DbType = Int32)
@p52=NULL (DbType = Int16)
@p53=NULL (DbType = Int64)
@p54=NULL (DbType = Int32)
@p55=NULL (DbType = Int64)
@p56=NULL (Precision = 20) (DbType = Decimal)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78), 78);
        }
    }

    private static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Null(entity.LongAsBigint);
        Assert.Null(entity.ShortAsSmallint);
        Assert.Null(entity.ByteAsTinyint);
        Assert.Null(entity.UintAsInt);
        Assert.Null(entity.UlongAsBigint);
        Assert.Null(entity.UShortAsSmallint);
        Assert.Null(entity.SbyteAsTinyint);
        Assert.Null(entity.BoolAsBit);
        Assert.Null(entity.DecimalAsMoney);
        Assert.Null(entity.DecimalAsSmallmoney);
        Assert.Null(entity.DoubleAsFloat);
        Assert.Null(entity.FloatAsReal);
        Assert.Null(entity.DoubleAsDoublePrecision);
        Assert.Null(entity.DateOnlyAsDate);
        Assert.Null(entity.DateTimeAsDate);
        Assert.Null(entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Null(entity.DateTimeAsDatetime2);
        Assert.Null(entity.DateTimeAsSmalldatetime);
        Assert.Null(entity.DateTimeAsDatetime);
        Assert.Null(entity.TimeOnlyAsTime);
        Assert.Null(entity.TimeSpanAsTime);
        Assert.Null(entity.StringAsVarcharMax);
        Assert.Null(entity.StringAsCharVaryingMax);
        Assert.Null(entity.StringAsCharacterVaryingMax);
        Assert.Null(entity.StringAsNvarcharMax);
        Assert.Null(entity.StringAsNationalCharVaryingMax);
        Assert.Null(entity.StringAsNationalCharacterVaryingMax);
        Assert.Null(entity.StringAsText);
        Assert.Null(entity.StringAsNtext);
        Assert.Null(entity.BytesAsVarbinaryMax);
        Assert.Null(entity.BytesAsBinaryVaryingMax);
        Assert.Null(entity.BytesAsImage);
        Assert.Null(entity.Decimal);
        Assert.Null(entity.DecimalAsDec);
        Assert.Null(entity.DecimalAsNumeric);
        Assert.Null(entity.GuidAsUniqueidentifier);
        Assert.Null(entity.UintAsBigint);
        Assert.Null(entity.UlongAsDecimal200);
        Assert.Null(entity.UShortAsInt);
        Assert.Null(entity.SByteAsSmallint);
        Assert.Null(entity.CharAsVarchar);
        Assert.Null(entity.CharAsAsCharVarying);
        Assert.Null(entity.CharAsCharacterVaryingMax);
        Assert.Null(entity.CharAsNvarchar);
        Assert.Null(entity.CharAsNationalCharVarying);
        Assert.Null(entity.CharAsNationalCharacterVaryingMax);
        Assert.Null(entity.CharAsText);
        Assert.Null(entity.CharAsNtext);
        Assert.Null(entity.CharAsInt);
        Assert.Null(entity.EnumAsNvarchar20);
        Assert.Null(entity.EnumAsVarcharMax);
        Assert.Null(entity.SqlVariantString);
        Assert.Null(entity.SqlVariantInt);
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='77'
@p1='0x0A0B0C' (Size = 3)
@p2='0x0C0D0E' (Size = 3)
@p3='0x0B0C0D' (Size = 3)
@p4='B' (Size = 3) (DbType = AnsiString)
@p5='C' (Size = 3) (DbType = AnsiString)
@p6='E' (Size = 3)
@p7='F' (Size = 3)
@p8='D' (Size = 3)
@p9='A' (Size = 3) (DbType = AnsiString)
@p10='Wor' (Size = 3) (DbType = AnsiStringFixedLength)
@p11='Wha' (Size = 3) (DbType = StringFixedLength)
@p12='Thr' (Size = 3) (DbType = AnsiString)
@p13='tex' (Size = 3)
@p14='Lon' (Size = 3) (DbType = AnsiStringFixedLength)
@p15='doe' (Size = 3) (DbType = StringFixedLength)
@p16='Let' (Size = 3) (DbType = AnsiString)
@p17='men' (Size = 3)
@p18='The' (Size = 3)
@p19='Squ' (Size = 3) (DbType = StringFixedLength)
@p20='Col' (Size = 3)
@p21='Won' (Size = 3) (DbType = StringFixedLength)
@p22='Int' (Size = 3)
@p23='Tha' (Size = 3) (DbType = AnsiString)
@p24='the' (Size = 3)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal("Wor", entity.StringAsChar3);
        Assert.Equal("Lon", entity.StringAsCharacter3);
        Assert.Equal("Tha", entity.StringAsVarchar3);
        Assert.Equal("Thr", entity.StringAsCharVarying3);
        Assert.Equal("Let", entity.StringAsCharacterVarying3);
        Assert.Equal("Won", entity.StringAsNchar3);
        Assert.Equal("Squ", entity.StringAsNationalCharacter3);
        Assert.Equal("Int", entity.StringAsNvarchar3);
        Assert.Equal("The", entity.StringAsNationalCharVarying3);
        Assert.Equal("Col", entity.StringAsNationalCharacterVarying3);
        Assert.Equal("Wha", entity.StringAsChar3Utf8);
        Assert.Equal("doe", entity.StringAsCharacter3Utf8);
        Assert.Equal("the", entity.StringAsVarchar3Utf8);
        Assert.Equal("tex", entity.StringAsCharVarying3Utf8);
        Assert.Equal("men", entity.StringAsCharacterVarying3Utf8);
        Assert.Equal(new byte[] { 10, 11, 12 }, entity.BytesAsBinary3);
        Assert.Equal(new byte[] { 11, 12, 13 }, entity.BytesAsVarbinary3);
        Assert.Equal(new byte[] { 12, 13, 14 }, entity.BytesAsBinaryVarying3);
        Assert.Equal('A', entity.CharAsVarchar3);
        Assert.Equal('B', entity.CharAsAsCharVarying3);
        Assert.Equal('C', entity.CharAsCharacterVarying3);
        Assert.Equal('D', entity.CharAsNvarchar3);
        Assert.Equal('E', entity.CharAsNationalCharVarying3);
        Assert.Equal('F', entity.CharAsNationalCharacterVarying3);
    }

    private static MappedSizedDataTypes CreateMappedSizedDataTypes(int id)
        => new()
        {
            Id = id,
            StringAsChar3 = "Wor",
            StringAsCharacter3 = "Lon",
            StringAsVarchar3 = "Tha",
            StringAsCharVarying3 = "Thr",
            StringAsCharacterVarying3 = "Let",
            StringAsNchar3 = "Won",
            StringAsNationalCharacter3 = "Squ",
            StringAsNvarchar3 = "Int",
            StringAsNationalCharVarying3 = "The",
            StringAsNationalCharacterVarying3 = "Col",
            StringAsChar3Utf8 = "Wha",
            StringAsCharacter3Utf8 = "doe",
            StringAsVarchar3Utf8 = "the",
            StringAsCharVarying3Utf8 = "tex",
            StringAsCharacterVarying3Utf8 = "men",
            BytesAsBinary3 = [10, 11, 12],
            BytesAsVarbinary3 = [11, 12, 13],
            BytesAsBinaryVarying3 = [12, 13, 14],
            CharAsVarchar3 = 'A',
            CharAsAsCharVarying3 = 'B',
            CharAsCharacterVarying3 = 'C',
            CharAsNvarchar3 = 'D',
            CharAsNationalCharVarying3 = 'E',
            CharAsNationalCharacterVarying3 = 'F'
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 78 });

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='78'
@p1=NULL (Size = 3) (DbType = Binary)
@p2=NULL (Size = 3) (DbType = Binary)
@p3=NULL (Size = 3) (DbType = Binary)
@p4=NULL (Size = 3) (DbType = AnsiString)
@p5=NULL (Size = 3) (DbType = AnsiString)
@p6=NULL (Size = 3)
@p7=NULL (Size = 3)
@p8=NULL (Size = 3)
@p9=NULL (Size = 3) (DbType = AnsiString)
@p10=NULL (Size = 3) (DbType = AnsiStringFixedLength)
@p11=NULL (Size = 3) (DbType = StringFixedLength)
@p12=NULL (Size = 3) (DbType = AnsiString)
@p13=NULL (Size = 3)
@p14=NULL (Size = 3) (DbType = AnsiStringFixedLength)
@p15=NULL (Size = 3) (DbType = StringFixedLength)
@p16=NULL (Size = 3) (DbType = AnsiString)
@p17=NULL (Size = 3)
@p18=NULL (Size = 3)
@p19=NULL (Size = 3) (DbType = StringFixedLength)
@p20=NULL (Size = 3)
@p21=NULL (Size = 3) (DbType = StringFixedLength)
@p22=NULL (Size = 3)
@p23=NULL (Size = 3) (DbType = AnsiString)
@p24=NULL (Size = 3)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 78), 78);
        }
    }

    private static void AssertNullMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Null(entity.StringAsChar3);
        Assert.Null(entity.StringAsCharacter3);
        Assert.Null(entity.StringAsVarchar3);
        Assert.Null(entity.StringAsCharVarying3);
        Assert.Null(entity.StringAsCharacterVarying3);
        Assert.Null(entity.StringAsNchar3);
        Assert.Null(entity.StringAsNationalCharacter3);
        Assert.Null(entity.StringAsNvarchar3);
        Assert.Null(entity.StringAsNationalCharVarying3);
        Assert.Null(entity.StringAsNationalCharacterVarying3);
        Assert.Null(entity.StringAsChar3Utf8);
        Assert.Null(entity.StringAsCharacter3Utf8);
        Assert.Null(entity.StringAsVarchar3Utf8);
        Assert.Null(entity.StringAsCharVarying3Utf8);
        Assert.Null(entity.StringAsCharacterVarying3Utf8);
        Assert.Null(entity.BytesAsBinary3);
        Assert.Null(entity.BytesAsVarbinary3);
        Assert.Null(entity.BytesAsBinaryVarying3);
        Assert.Null(entity.CharAsVarchar3);
        Assert.Null(entity.CharAsAsCharVarying3);
        Assert.Null(entity.CharAsCharacterVarying3);
        Assert.Null(entity.CharAsNvarchar3);
        Assert.Null(entity.CharAsNationalCharVarying3);
        Assert.Null(entity.CharAsNationalCharacterVarying3);
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_sized_separately()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedSeparatelyDataTypes>().Add(CreateMappedSizedSeparatelyDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='77'
@p1='0x0A0B0C' (Size = 3)
@p2='0x0C0D0E' (Size = 3)
@p3='0x0B0C0D' (Size = 3)
@p4='B' (Size = 3) (DbType = AnsiString)
@p5='C' (Size = 3) (DbType = AnsiString)
@p6='E' (Size = 3)
@p7='F' (Size = 3)
@p8='D' (Size = 3)
@p9='A' (Size = 3) (DbType = AnsiString)
@p10='Wor' (Size = 3) (DbType = AnsiStringFixedLength)
@p11='Wha' (Size = 3) (DbType = AnsiStringFixedLength)
@p12='Thr' (Size = 3) (DbType = AnsiString)
@p13='tex' (Size = 3) (DbType = AnsiString)
@p14='Lon' (Size = 3) (DbType = AnsiStringFixedLength)
@p15='doe' (Size = 3) (DbType = AnsiStringFixedLength)
@p16='Let' (Size = 3) (DbType = AnsiString)
@p17='men' (Size = 3) (DbType = AnsiString)
@p18='The' (Size = 3)
@p19='Squ' (Size = 3) (DbType = StringFixedLength)
@p20='Col' (Size = 3)
@p21='Won' (Size = 3) (DbType = StringFixedLength)
@p22='Int' (Size = 3)
@p23='Tha' (Size = 3) (DbType = AnsiString)
@p24='the' (Size = 3) (DbType = AnsiString)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedSizedSeparatelyDataTypes(context.Set<MappedSizedSeparatelyDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertMappedSizedSeparatelyDataTypes(MappedSizedSeparatelyDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal("Wor", entity.StringAsChar3);
        Assert.Equal("Lon", entity.StringAsCharacter3);
        Assert.Equal("Tha", entity.StringAsVarchar3);
        Assert.Equal("Thr", entity.StringAsCharVarying3);
        Assert.Equal("Let", entity.StringAsCharacterVarying3);
        Assert.Equal("Won", entity.StringAsNchar3);
        Assert.Equal("Squ", entity.StringAsNationalCharacter3);
        Assert.Equal("Int", entity.StringAsNvarchar3);
        Assert.Equal("The", entity.StringAsNationalCharVarying3);
        Assert.Equal("Col", entity.StringAsNationalCharacterVarying3);
        Assert.Equal(new byte[] { 10, 11, 12 }, entity.BytesAsBinary3);
        Assert.Equal(new byte[] { 11, 12, 13 }, entity.BytesAsVarbinary3);
        Assert.Equal(new byte[] { 12, 13, 14 }, entity.BytesAsBinaryVarying3);
        Assert.Equal('A', entity.CharAsVarchar3);
        Assert.Equal('B', entity.CharAsAsCharVarying3);
        Assert.Equal('C', entity.CharAsCharacterVarying3);
        Assert.Equal('D', entity.CharAsNvarchar3);
        Assert.Equal('E', entity.CharAsNationalCharVarying3);
        Assert.Equal('F', entity.CharAsNationalCharacterVarying3);
    }

    private static MappedSizedSeparatelyDataTypes CreateMappedSizedSeparatelyDataTypes(int id)
        => new()
        {
            Id = id,
            StringAsChar3 = "Wor",
            StringAsCharacter3 = "Lon",
            StringAsVarchar3 = "Tha",
            StringAsCharVarying3 = "Thr",
            StringAsCharacterVarying3 = "Let",
            StringAsNchar3 = "Won",
            StringAsNationalCharacter3 = "Squ",
            StringAsNvarchar3 = "Int",
            StringAsNationalCharVarying3 = "The",
            StringAsNationalCharacterVarying3 = "Col",
            StringAsChar3Utf8 = "Wha",
            StringAsCharacter3Utf8 = "doe",
            StringAsVarchar3Utf8 = "the",
            StringAsCharVarying3Utf8 = "tex",
            StringAsCharacterVarying3Utf8 = "men",
            BytesAsBinary3 = [10, 11, 12],
            BytesAsVarbinary3 = [11, 12, 13],
            BytesAsBinaryVarying3 = [12, 13, 14],
            CharAsVarchar3 = 'A',
            CharAsAsCharVarying3 = 'B',
            CharAsCharacterVarying3 = 'C',
            CharAsNvarchar3 = 'D',
            CharAsNationalCharVarying3 = 'E',
            CharAsNationalCharacterVarying3 = 'F'
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='77'
@p1='2017-01-02T12:11:12.3210000' (Scale = 3)
@p2='2016-01-02T11:11:12.7650000+00:00' (Scale = 3)
@p3='102' (Precision = 3)
@p4='101' (Precision = 3)
@p5='103' (Precision = 3)
@p6='85.55000305175781' (Size = 25)
@p7='85.5' (Size = 3)
@p8='83.33000183105469' (Size = 25)
@p9='83.3' (Size = 3)
@p10='12:34' (Scale = 3) (DbType = Time)
@p11='12:34:56.7890123' (Scale = 3)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertMappedScaledDataTypes(MappedScaledDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal(83.3f, entity.FloatAsFloat3);
        Assert.Equal(85.5f, entity.FloatAsDoublePrecision3);
        Assert.Equal(83.33f, entity.FloatAsFloat25);
        Assert.Equal(85.55f, entity.FloatAsDoublePrecision25);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 765), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset3);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12, 321), entity.DateTimeAsDatetime23);
        Assert.Equal(TimeOnly.Parse("12:34:56.789", CultureInfo.InvariantCulture), entity.TimeOnlyAsTime3);
        Assert.Equal(TimeSpan.Parse("12:34:56.789", CultureInfo.InvariantCulture), entity.TimeSpanAsTime3);
        Assert.Equal(101m, entity.DecimalAsDecimal3);
        Assert.Equal(102m, entity.DecimalAsDec3);
        Assert.Equal(103m, entity.DecimalAsNumeric3);
    }

    private static MappedScaledDataTypes CreateMappedScaledDataTypes(int id)
        => new()
        {
            Id = id,
            FloatAsFloat3 = 83.3f,
            FloatAsDoublePrecision3 = 85.5f,
            FloatAsFloat25 = 83.33f,
            FloatAsDoublePrecision25 = 85.55f,
            DateTimeOffsetAsDatetimeoffset3 = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 765), TimeSpan.Zero),
            DateTimeAsDatetime23 = new DateTime(2017, 1, 2, 12, 11, 12, 321),
            DecimalAsDecimal3 = 101m,
            DecimalAsDec3 = 102m,
            DecimalAsNumeric3 = 103m,
            TimeOnlyAsTime3 = TimeOnly.Parse("12:34:56.7890123", CultureInfo.InvariantCulture),
            TimeSpanAsTime3 = TimeSpan.Parse("12:34:56.7890123", CultureInfo.InvariantCulture)
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_separately()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedScaledSeparatelyDataTypes>().Add(CreateMappedScaledSeparatelyDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='77'
@p1='2017-01-02T12:11:12.3210000' (Scale = 3)
@p2='2016-01-02T11:11:12.7650000+00:00' (Scale = 3)
@p3='102' (Precision = 3)
@p4='101' (Precision = 3)
@p5='103' (Precision = 3)
@p6='85.55000305175781' (Size = 25)
@p7='85.5' (Size = 3)
@p8='83.33000183105469' (Size = 25)
@p9='83.3' (Size = 3)
@p10='12:34' (Scale = 3) (DbType = Time)
@p11='12:34:56.7890000' (Scale = 3)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedScaledSeparatelyDataTypes(context.Set<MappedScaledSeparatelyDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertMappedScaledSeparatelyDataTypes(MappedScaledSeparatelyDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal(83.3f, entity.FloatAsFloat3);
        Assert.Equal(85.5f, entity.FloatAsDoublePrecision3);
        Assert.Equal(83.33f, entity.FloatAsFloat25);
        Assert.Equal(85.55f, entity.FloatAsDoublePrecision25);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 765), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset3);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12, 321), entity.DateTimeAsDatetime23);
        Assert.Equal(101m, entity.DecimalAsDecimal3);
        Assert.Equal(102m, entity.DecimalAsDec3);
        Assert.Equal(103m, entity.DecimalAsNumeric3);
        Assert.Equal(TimeOnly.Parse("12:34:56.789", CultureInfo.InvariantCulture), entity.TimeOnlyAsTime3);
        Assert.Equal(TimeSpan.Parse("12:34:56.789", CultureInfo.InvariantCulture), entity.TimeSpanAsTime3);
    }

    private static MappedScaledSeparatelyDataTypes CreateMappedScaledSeparatelyDataTypes(int id)
        => new()
        {
            Id = id,
            FloatAsFloat3 = 83.3f,
            FloatAsDoublePrecision3 = 85.5f,
            FloatAsFloat25 = 83.33f,
            FloatAsDoublePrecision25 = 85.55f,
            DateTimeOffsetAsDatetimeoffset3 = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 765), TimeSpan.Zero),
            DateTimeAsDatetime23 = new DateTime(2017, 1, 2, 12, 11, 12, 321),
            DecimalAsDecimal3 = 101m,
            DecimalAsDec3 = 102m,
            DecimalAsNumeric3 = 103m,
            TimeOnlyAsTime3 = TimeOnly.Parse("12:34:56.789", CultureInfo.InvariantCulture),
            TimeSpanAsTime3 = TimeSpan.Parse("12:34:56.789", CultureInfo.InvariantCulture)
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_double_types_with_precision()
    {
        using (var context = CreateContext())
        {
            context.Set<DoubleDataTypes>().Add(CreateDoubleDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='77'
@p1='83.33000183105469' (Size = 25)
@p2='83.30000305175781' (Size = 3)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertDoubleDataTypes(context.Set<DoubleDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertDoubleDataTypes(DoubleDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal(83.3f, entity.Double3);
        Assert.Equal(83.33f, entity.Double25);
    }

    private static DoubleDataTypes CreateDoubleDataTypes(int id)
        => new()
        {
            Id = id,
            Double3 = 83.3f,
            Double25 = 83.33f
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='77'
@p1='102.2' (Precision = 5) (Scale = 2)
@p2='101.1' (Precision = 5) (Scale = 2)
@p3='103.3' (Precision = 5) (Scale = 2)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertMappedPrecisionAndScaledDataTypes(MappedPrecisionAndScaledDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal(101.1m, entity.DecimalAsDecimal52);
        Assert.Equal(102.2m, entity.DecimalAsDec52);
        Assert.Equal(103.3m, entity.DecimalAsNumeric52);
    }

    private static MappedPrecisionAndScaledDataTypes CreateMappedPrecisionAndScaledDataTypes(int id)
        => new()
        {
            Id = id,
            DecimalAsDecimal52 = 101.1m,
            DecimalAsDec52 = 102.2m,
            DecimalAsNumeric52 = 103.3m
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_separately()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedPrecisionAndScaledSeparatelyDataTypes>().Add(CreateMappedPrecisionAndScaledSeparatelyDataTypes(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='77'
@p1='102.2' (Precision = 5) (Scale = 2)
@p2='101.1' (Precision = 5) (Scale = 2)
@p3='103.3' (Precision = 5) (Scale = 2)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedPrecisionAndScaledSeparatelyDataTypes(
                context.Set<MappedPrecisionAndScaledSeparatelyDataTypes>().Single(e => e.Id == 77), 77);
        }
    }

    private static void AssertMappedPrecisionAndScaledSeparatelyDataTypes(MappedPrecisionAndScaledSeparatelyDataTypes entity, int id)
    {
        Assert.Equal(id, entity.Id);
        Assert.Equal(101.1m, entity.DecimalAsDecimal52);
        Assert.Equal(102.2m, entity.DecimalAsDec52);
        Assert.Equal(103.3m, entity.DecimalAsNumeric52);
    }

    private static MappedPrecisionAndScaledSeparatelyDataTypes CreateMappedPrecisionAndScaledSeparatelyDataTypes(int id)
        => new()
        {
            Id = id,
            DecimalAsDecimal52 = 101.1m,
            DecimalAsDec52 = 102.2m,
            DecimalAsNumeric52 = 103.3m
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='True'
@p1='80' (Size = 1)
@p2='0x5D5E5F60' (Nullable = false) (Size = 8000)
@p3='0x61626364' (Nullable = false) (Size = 8000)
@p4='0x595A5B5C' (Nullable = false) (Size = 8000)
@p5='B' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p6='C' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p7='73'
@p8='E' (Nullable = false) (Size = 1)
@p9='F' (Nullable = false) (Size = 4000)
@p10='H' (Nullable = false) (Size = 1)
@p11='D' (Nullable = false) (Size = 1)
@p12='G' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p13='A' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p14='01/02/2015' (DbType = Date)
@p15='2015-01-02T10:11:12.0000000' (DbType = Date)
@p16='2019-01-02T14:11:12.0000000' (DbType = DateTime)
@p17='2017-01-02T12:11:12.7654321'
@p18='2018-01-02T13:11:12.0000000' (DbType = DateTime)
@p19='2016-01-02T11:11:12.7654321+00:00'
@p20='101' (Precision = 18)
@p21='102' (Precision = 18)
@p22='81.1' (DbType = Currency)
@p23='103' (Precision = 18)
@p24='82.2' (DbType = Currency)
@p25='85.5'
@p26='83.3'
@p27='Value4' (Nullable = false) (Size = 20)
@p28='Value2' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p29='84.4'
@p30='a8f9f951-145f-4545-ac60-b92ff57ada47'
@p31='77'
@p32='78'
@p33='-128'
@p34='128' (Size = 1)
@p35='79'
@p36='887876' (DbType = Object)
@p37='Bang!' (Nullable = false) (Size = 5) (DbType = Object)
@p38='Your' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p39='And now' (Nullable = false) (Size = 4000)
@p40='strong' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p41='this...' (Nullable = false) (Size = 4000)
@p42='help' (Nullable = false) (Size = 4000)
@p43='anyone!' (Nullable = false) (Size = 4000)
@p44='Gumball Rules OK!' (Nullable = false) (Size = 4000)
@p45='don't' (Nullable = false) (Size = 4000)
@p46='Gumball Rules!' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p47='C' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p48='short' (Nullable = false) (Size = 4000)
@p49='11:15' (DbType = Time)
@p50='11:15:12'
@p51='65535'
@p52='-1'
@p53='4294967295'
@p54='-1'
@p55='-1'
@p56='18446744073709551615' (Precision = 20)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedDataTypesWithIdentity(MappedDataTypesWithIdentity entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Equal(78, entity.LongAsBigint);
        Assert.Equal(79, entity.ShortAsSmallint);
        Assert.Equal(80, entity.ByteAsTinyint);
        Assert.Equal(uint.MaxValue, entity.UintAsInt);
        Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
        Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
        Assert.True(entity.BoolAsBit);
        Assert.Equal(81.1m, entity.DecimalAsMoney);
        Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
        Assert.Equal(83.3, entity.DoubleAsFloat);
        Assert.Equal(84.4f, entity.FloatAsReal);
        Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
        Assert.Equal(new DateOnly(2015, 1, 2), entity.DateOnlyAsDate);
        Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(7654321), TimeSpan.Zero),
            entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(7654321), entity.DateTimeAsDatetime2);
        Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
        Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
        Assert.Equal(new TimeOnly(11, 15, 12), entity.TimeOnlyAsTime);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
        Assert.Equal("C", entity.StringAsVarcharMax);
        Assert.Equal("Your", entity.StringAsCharVaryingMax);
        Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
        Assert.Equal("don't", entity.StringAsNvarcharMax);
        Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
        Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
        Assert.Equal("short", entity.StringAsVarcharMaxUtf8);
        Assert.Equal("And now", entity.StringAsCharVaryingMaxUtf8);
        Assert.Equal("this...", entity.StringAsCharacterVaryingMaxUtf8);
        Assert.Equal("Gumball Rules!", entity.StringAsText);
        Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
        Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
        Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinaryVaryingMax);
        Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
        Assert.Equal(101m, entity.Decimal);
        Assert.Equal(102m, entity.DecimalAsDec);
        Assert.Equal(103m, entity.DecimalAsNumeric);
        Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
        Assert.Equal(uint.MaxValue, entity.UintAsBigint);
        Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
        Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
        Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
        Assert.Equal('A', entity.CharAsVarchar);
        Assert.Equal('B', entity.CharAsAsCharVarying);
        Assert.Equal('C', entity.CharAsCharacterVaryingMax);
        Assert.Equal('D', entity.CharAsNvarchar);
        Assert.Equal('E', entity.CharAsNationalCharVarying);
        Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
        Assert.Equal('G', entity.CharAsText);
        Assert.Equal('H', entity.CharAsNtext);
        Assert.Equal('I', entity.CharAsInt);
        Assert.Equal(StringEnum16.Value2, entity.EnumAsVarcharMax);
        Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
        Assert.Equal("Bang!", entity.SqlVariantString);
        Assert.Equal(887876, entity.SqlVariantInt);
    }

    private static MappedDataTypesWithIdentity CreateMappedDataTypesWithIdentity(int id)
        => new()
        {
            Int = id,
            LongAsBigint = 78L,
            ShortAsSmallint = 79,
            ByteAsTinyint = 80,
            UintAsInt = uint.MaxValue,
            UlongAsBigint = ulong.MaxValue,
            UShortAsSmallint = ushort.MaxValue,
            SbyteAsTinyint = sbyte.MinValue,
            BoolAsBit = true,
            DecimalAsMoney = 81.1m,
            DecimalAsSmallmoney = 82.2m,
            DoubleAsFloat = 83.3,
            FloatAsReal = 84.4f,
            DoubleAsDoublePrecision = 85.5,
            DateOnlyAsDate = new DateOnly(2015, 1, 2),
            DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
            DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(7654321), TimeSpan.Zero),
            DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(7654321),
            DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
            DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
            TimeOnlyAsTime = new TimeOnly(11, 15, 12),
            TimeSpanAsTime = new TimeSpan(11, 15, 12),
            StringAsVarcharMax = "C",
            StringAsCharVaryingMax = "Your",
            StringAsCharacterVaryingMax = "strong",
            StringAsNvarcharMax = "don't",
            StringAsNationalCharVaryingMax = "help",
            StringAsNationalCharacterVaryingMax = "anyone!",
            StringAsVarcharMaxUtf8 = "short",
            StringAsCharVaryingMaxUtf8 = "And now",
            StringAsCharacterVaryingMaxUtf8 = "this...",
            StringAsText = "Gumball Rules!",
            StringAsNtext = "Gumball Rules OK!",
            BytesAsVarbinaryMax = [89, 90, 91, 92],
            BytesAsBinaryVaryingMax = [93, 94, 95, 96],
            BytesAsImage = [97, 98, 99, 100],
            Decimal = 101m,
            DecimalAsDec = 102m,
            DecimalAsNumeric = 103m,
            GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
            UintAsBigint = uint.MaxValue,
            UlongAsDecimal200 = ulong.MaxValue,
            UShortAsInt = ushort.MaxValue,
            SByteAsSmallint = sbyte.MinValue,
            CharAsVarchar = 'A',
            CharAsAsCharVarying = 'B',
            CharAsCharacterVaryingMax = 'C',
            CharAsNvarchar = 'D',
            CharAsNationalCharVarying = 'E',
            CharAsNationalCharacterVaryingMax = 'F',
            CharAsText = 'G',
            CharAsNtext = 'H',
            CharAsInt = 'I',
            EnumAsNvarchar20 = StringEnumU16.Value4,
            EnumAsVarcharMax = StringEnum16.Value2,
            SqlVariantString = "Bang!",
            SqlVariantInt = 887876
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='True' (Nullable = true)
@p1='80' (Nullable = true) (Size = 1)
@p2='0x61626364' (Size = 8000)
@p3='0x595A5B5C' (Size = 8000)
@p4='0x5D5E5F60' (Size = 8000)
@p5='B' (Size = 1) (DbType = AnsiString)
@p6='C' (Size = 8000) (DbType = AnsiString)
@p7='73' (Nullable = true)
@p8='E' (Size = 1)
@p9='F' (Size = 4000)
@p10='H' (Size = 1)
@p11='D' (Size = 1)
@p12='G' (Size = 1) (DbType = AnsiString)
@p13='A' (Size = 1) (DbType = AnsiString)
@p14='01/02/2015' (Nullable = true) (DbType = Date)
@p15='2015-01-02T10:11:12.0000000' (Nullable = true) (DbType = Date)
@p16='2019-01-02T14:11:12.0000000' (Nullable = true) (DbType = DateTime)
@p17='2017-01-02T12:11:12.2345678' (Nullable = true)
@p18='2018-01-02T13:11:12.0000000' (Nullable = true) (DbType = DateTime)
@p19='2016-01-02T11:11:12.2345678+00:00' (Nullable = true)
@p20='101' (Nullable = true) (Precision = 18)
@p21='102' (Nullable = true) (Precision = 18)
@p22='81.1' (Nullable = true) (DbType = Currency)
@p23='103' (Nullable = true) (Precision = 18)
@p24='82.2' (Nullable = true) (DbType = Currency)
@p25='85.5' (Nullable = true)
@p26='83.3' (Nullable = true)
@p27='Value4' (Size = 20)
@p28='Value2' (Size = 8000) (DbType = AnsiString)
@p29='84.4' (Nullable = true)
@p30='a8f9f951-145f-4545-ac60-b92ff57ada47' (Nullable = true)
@p31='77' (Nullable = true)
@p32='78' (Nullable = true)
@p33='-128' (Nullable = true)
@p34='128' (Nullable = true) (Size = 1)
@p35='79' (Nullable = true)
@p36='887876' (Nullable = true) (DbType = Object)
@p37='Bang!' (Size = 5) (DbType = Object)
@p38='Your' (Size = 8000) (DbType = AnsiString)
@p39='And now' (Size = 4000)
@p40='strong' (Size = 8000) (DbType = AnsiString)
@p41='this...' (Size = 4000)
@p42='help' (Size = 4000)
@p43='anyone!' (Size = 4000)
@p44='Gumball Rules OK!' (Size = 4000)
@p45='don't' (Size = 4000)
@p46='Gumball Rules!' (Size = 8000) (DbType = AnsiString)
@p47='C' (Size = 8000) (DbType = AnsiString)
@p48='short' (Size = 4000)
@p49='11:15' (Nullable = true) (DbType = Time)
@p50='11:15:12' (Nullable = true)
@p51='65535' (Nullable = true)
@p52='4294967295' (Nullable = true)
@p53='-1' (Nullable = true)
@p54='-1' (Nullable = true)
@p55='18446744073709551615' (Nullable = true) (Precision = 20)
@p56='-1' (Nullable = true)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedNullableDataTypesWithIdentity(MappedNullableDataTypesWithIdentity entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Equal(78, entity.LongAsBigint);
        Assert.Equal(79, entity.ShortAsSmallint.Value);
        Assert.Equal(80, entity.ByteAsTinyint.Value);
        Assert.Equal(uint.MaxValue, entity.UintAsInt);
        Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UshortAsSmallint);
        Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
        Assert.True(entity.BoolAsBit);
        Assert.Equal(81.1m, entity.DecimalAsMoney);
        Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
        Assert.Equal(83.3, entity.DoubleAsFloat);
        Assert.Equal(84.4f, entity.FloatAsReal);
        Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
        Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
        Assert.Equal(new DateOnly(2015, 1, 2), entity.DateOnlyAsDate);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(2345678), TimeSpan.Zero),
            entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(2345678), entity.DateTimeAsDatetime2);
        Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
        Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
        Assert.Equal(new TimeOnly(11, 15, 12), entity.TimeOnlyAsTime);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
        Assert.Equal("C", entity.StringAsVarcharMax);
        Assert.Equal("Your", entity.StringAsCharVaryingMax);
        Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
        Assert.Equal("don't", entity.StringAsNvarcharMax);
        Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
        Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
        Assert.Equal("Gumball Rules!", entity.StringAsText);
        Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
        Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
        Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsVaryingMax);
        Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
        Assert.Equal(101m, entity.Decimal);
        Assert.Equal(102m, entity.DecimalAsDec);
        Assert.Equal(103m, entity.DecimalAsNumeric);
        Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
        Assert.Equal(uint.MaxValue, entity.UintAsBigint);
        Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
        Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
        Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
        Assert.Equal('A', entity.CharAsVarchar);
        Assert.Equal('B', entity.CharAsAsCharVarying);
        Assert.Equal('C', entity.CharAsCharacterVaryingMax);
        Assert.Equal('D', entity.CharAsNvarchar);
        Assert.Equal('E', entity.CharAsNationalCharVarying);
        Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
        Assert.Equal('G', entity.CharAsText);
        Assert.Equal('H', entity.CharAsNtext);
        Assert.Equal('I', entity.CharAsInt);
        Assert.Equal(StringEnum16.Value2, entity.EnumAsVarcharMax);
        Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
        Assert.Equal("Bang!", entity.SqlVariantString);
        Assert.Equal(887876, entity.SqlVariantInt);
    }

    private static MappedNullableDataTypesWithIdentity CreateMappedNullableDataTypesWithIdentity(int id)
        => new()
        {
            Int = id,
            LongAsBigint = 78L,
            ShortAsSmallint = 79,
            ByteAsTinyint = 80,
            UintAsInt = uint.MaxValue,
            UlongAsBigint = ulong.MaxValue,
            UshortAsSmallint = ushort.MaxValue,
            SbyteAsTinyint = sbyte.MinValue,
            BoolAsBit = true,
            DecimalAsMoney = 81.1m,
            DecimalAsSmallmoney = 82.2m,
            DoubleAsFloat = 83.3,
            FloatAsReal = 84.4f,
            DoubleAsDoublePrecision = 85.5,
            DateOnlyAsDate = new DateOnly(2015, 1, 2),
            DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
            DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12).AddTicks(2345678), TimeSpan.Zero),
            DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12).AddTicks(2345678),
            DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
            DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
            TimeOnlyAsTime = new TimeOnly(11, 15, 12),
            TimeSpanAsTime = new TimeSpan(11, 15, 12),
            StringAsVarcharMax = "C",
            StringAsCharVaryingMax = "Your",
            StringAsCharacterVaryingMax = "strong",
            StringAsNvarcharMax = "don't",
            StringAsNationalCharVaryingMax = "help",
            StringAsNationalCharacterVaryingMax = "anyone!",
            StringAsVarcharMaxUtf8 = "short",
            StringAsCharVaryingMaxUtf8 = "And now",
            StringAsCharacterVaryingMaxUtf8 = "this...",
            StringAsText = "Gumball Rules!",
            StringAsNtext = "Gumball Rules OK!",
            BytesAsVarbinaryMax = [89, 90, 91, 92],
            BytesAsVaryingMax = [93, 94, 95, 96],
            BytesAsImage = [97, 98, 99, 100],
            Decimal = 101m,
            DecimalAsDec = 102m,
            DecimalAsNumeric = 103m,
            GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
            UintAsBigint = uint.MaxValue,
            UlongAsDecimal200 = ulong.MaxValue,
            UShortAsInt = ushort.MaxValue,
            SByteAsSmallint = sbyte.MinValue,
            CharAsVarchar = 'A',
            CharAsAsCharVarying = 'B',
            CharAsCharacterVaryingMax = 'C',
            CharAsNvarchar = 'D',
            CharAsNationalCharVarying = 'E',
            CharAsNationalCharacterVaryingMax = 'F',
            CharAsText = 'G',
            CharAsNtext = 'H',
            CharAsInt = 'I',
            EnumAsNvarchar20 = StringEnumU16.Value4,
            EnumAsVarcharMax = StringEnum16.Value2,
            SqlVariantString = "Bang!",
            SqlVariantInt = 887876
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 78 });

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0=NULL (DbType = Boolean)
@p1=NULL (DbType = Byte)
@p2=NULL (Size = 8000) (DbType = Binary)
@p3=NULL (Size = 8000) (DbType = Binary)
@p4=NULL (Size = 8000) (DbType = Binary)
@p5=NULL (Size = 1) (DbType = AnsiString)
@p6=NULL (Size = 8000) (DbType = AnsiString)
@p7=NULL (DbType = Int32)
@p8=NULL (Size = 1)
@p9=NULL (Size = 4000)
@p10=NULL (Size = 1)
@p11=NULL (Size = 1)
@p12=NULL (Size = 1) (DbType = AnsiString)
@p13=NULL (Size = 1) (DbType = AnsiString)
@p14=NULL (DbType = Date)
@p15=NULL (DbType = Date)
@p16=NULL (DbType = DateTime)
@p17=NULL (DbType = DateTime2)
@p18=NULL (DbType = DateTime)
@p19=NULL (DbType = DateTimeOffset)
@p20=NULL (Precision = 18) (DbType = Decimal)
@p21=NULL (Precision = 18) (DbType = Decimal)
@p22=NULL (DbType = Currency)
@p23=NULL (Precision = 18) (DbType = Decimal)
@p24=NULL (DbType = Currency)
@p25=NULL (DbType = Double)
@p26=NULL (DbType = Double)
@p27=NULL (Size = 20)
@p28=NULL (Size = 8000) (DbType = AnsiString)
@p29=NULL (DbType = Single)
@p30=NULL (DbType = Guid)
@p31='78' (Nullable = true)
@p32=NULL (DbType = Int64)
@p33=NULL (DbType = Int16)
@p34=NULL (DbType = Byte)
@p35=NULL (DbType = Int16)
@p36=NULL (DbType = Object)
@p37=NULL (DbType = Object)
@p38=NULL (Size = 8000) (DbType = AnsiString)
@p39=NULL (Size = 4000)
@p40=NULL (Size = 8000) (DbType = AnsiString)
@p41=NULL (Size = 4000)
@p42=NULL (Size = 4000)
@p43=NULL (Size = 4000)
@p44=NULL (Size = 4000)
@p45=NULL (Size = 4000)
@p46=NULL (Size = 8000) (DbType = AnsiString)
@p47=NULL (Size = 8000) (DbType = AnsiString)
@p48=NULL (Size = 4000)
@p49=NULL (DbType = Time)
@p50=NULL (DbType = Time)
@p51=NULL (DbType = Int32)
@p52=NULL (DbType = Int64)
@p53=NULL (DbType = Int32)
@p54=NULL (DbType = Int64)
@p55=NULL (Precision = 20) (DbType = Decimal)
@p56=NULL (DbType = Int16)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertNullMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 78), 78);
        }
    }

    private static void AssertNullMappedNullableDataTypesWithIdentity(
        MappedNullableDataTypesWithIdentity entity,
        int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Null(entity.LongAsBigint);
        Assert.Null(entity.ShortAsSmallint);
        Assert.Null(entity.ByteAsTinyint);
        Assert.Null(entity.UintAsInt);
        Assert.Null(entity.UlongAsBigint);
        Assert.Null(entity.UshortAsSmallint);
        Assert.Null(entity.SbyteAsTinyint);
        Assert.Null(entity.BoolAsBit);
        Assert.Null(entity.DecimalAsMoney);
        Assert.Null(entity.DecimalAsSmallmoney);
        Assert.Null(entity.DoubleAsFloat);
        Assert.Null(entity.FloatAsReal);
        Assert.Null(entity.DoubleAsDoublePrecision);
        Assert.Null(entity.DateOnlyAsDate);
        Assert.Null(entity.DateTimeAsDate);
        Assert.Null(entity.DateTimeOffsetAsDatetimeoffset);
        Assert.Null(entity.DateTimeAsDatetime2);
        Assert.Null(entity.DateTimeAsSmalldatetime);
        Assert.Null(entity.DateTimeAsDatetime);
        Assert.Null(entity.TimeOnlyAsTime);
        Assert.Null(entity.TimeSpanAsTime);
        Assert.Null(entity.StringAsVarcharMax);
        Assert.Null(entity.StringAsCharVaryingMax);
        Assert.Null(entity.StringAsCharacterVaryingMax);
        Assert.Null(entity.StringAsNvarcharMax);
        Assert.Null(entity.StringAsNationalCharVaryingMax);
        Assert.Null(entity.StringAsNationalCharacterVaryingMax);
        Assert.Null(entity.StringAsText);
        Assert.Null(entity.StringAsNtext);
        Assert.Null(entity.BytesAsVarbinaryMax);
        Assert.Null(entity.BytesAsVaryingMax);
        Assert.Null(entity.BytesAsImage);
        Assert.Null(entity.Decimal);
        Assert.Null(entity.DecimalAsDec);
        Assert.Null(entity.DecimalAsNumeric);
        Assert.Null(entity.GuidAsUniqueidentifier);
        Assert.Null(entity.UintAsBigint);
        Assert.Null(entity.UlongAsDecimal200);
        Assert.Null(entity.UShortAsInt);
        Assert.Null(entity.SByteAsSmallint);
        Assert.Null(entity.CharAsVarchar);
        Assert.Null(entity.CharAsAsCharVarying);
        Assert.Null(entity.CharAsCharacterVaryingMax);
        Assert.Null(entity.CharAsNvarchar);
        Assert.Null(entity.CharAsNationalCharVarying);
        Assert.Null(entity.CharAsNationalCharacterVaryingMax);
        Assert.Null(entity.CharAsText);
        Assert.Null(entity.CharAsNtext);
        Assert.Null(entity.CharAsInt);
        Assert.Null(entity.EnumAsNvarchar20);
        Assert.Null(entity.EnumAsVarcharMax);
        Assert.Null(entity.SqlVariantString);
        Assert.Null(entity.SqlVariantInt);
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='0x0A0B0C' (Size = 3)
@p1='0x0C0D0E' (Size = 3)
@p2='0x0B0C0D' (Size = 3)
@p3='B' (Size = 3) (DbType = AnsiString)
@p4='C' (Size = 3) (DbType = AnsiString)
@p5='E' (Size = 3)
@p6='F' (Size = 3)
@p7='D' (Size = 3)
@p8='A' (Size = 3) (DbType = AnsiString)
@p9='77'
@p10='Wor' (Size = 3) (DbType = AnsiStringFixedLength)
@p11='Wha' (Size = 3) (DbType = StringFixedLength)
@p12='Thr' (Size = 3) (DbType = AnsiString)
@p13='tex' (Size = 3)
@p14='Lon' (Size = 3) (DbType = AnsiStringFixedLength)
@p15='doe' (Size = 3) (DbType = StringFixedLength)
@p16='Let' (Size = 3) (DbType = AnsiString)
@p17='men' (Size = 3)
@p18='The' (Size = 3)
@p19='Squ' (Size = 3) (DbType = StringFixedLength)
@p20='Col' (Size = 3)
@p21='Won' (Size = 3) (DbType = StringFixedLength)
@p22='Int' (Size = 3)
@p23='Tha' (Size = 3) (DbType = AnsiString)
@p24='the' (Size = 3)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Equal("Wor", entity.StringAsChar3);
        Assert.Equal("Lon", entity.StringAsCharacter3);
        Assert.Equal("Tha", entity.StringAsVarchar3);
        Assert.Equal("Thr", entity.StringAsCharVarying3);
        Assert.Equal("Let", entity.StringAsCharacterVarying3);
        Assert.Equal("Won", entity.StringAsNchar3);
        Assert.Equal("Squ", entity.StringAsNationalCharacter3);
        Assert.Equal("Int", entity.StringAsNvarchar3);
        Assert.Equal("The", entity.StringAsNationalCharVarying3);
        Assert.Equal("Col", entity.StringAsNationalCharacterVarying3);
        Assert.Equal("Wha", entity.StringAsChar3Utf8);
        Assert.Equal("doe", entity.StringAsCharacter3Utf8);
        Assert.Equal("the", entity.StringAsVarchar3Utf8);
        Assert.Equal("tex", entity.StringAsCharVarying3Utf8);
        Assert.Equal("men", entity.StringAsCharacterVarying3Utf8);
        Assert.Equal(new byte[] { 10, 11, 12 }, entity.BytesAsBinary3);
        Assert.Equal(new byte[] { 11, 12, 13 }, entity.BytesAsVarbinary3);
        Assert.Equal(new byte[] { 12, 13, 14 }, entity.BytesAsBinaryVarying3);
        Assert.Equal('A', entity.CharAsVarchar3);
        Assert.Equal('B', entity.CharAsAsCharVarying3);
        Assert.Equal('C', entity.CharAsCharacterVarying3);
        Assert.Equal('D', entity.CharAsNvarchar3);
        Assert.Equal('E', entity.CharAsNationalCharVarying3);
        Assert.Equal('F', entity.CharAsNationalCharacterVarying3);
    }

    private static MappedSizedDataTypesWithIdentity CreateMappedSizedDataTypesWithIdentity(int id)
        => new()
        {
            Int = id,
            StringAsChar3 = "Wor",
            StringAsCharacter3 = "Lon",
            StringAsVarchar3 = "Tha",
            StringAsCharVarying3 = "Thr",
            StringAsCharacterVarying3 = "Let",
            StringAsNchar3 = "Won",
            StringAsNationalCharacter3 = "Squ",
            StringAsNvarchar3 = "Int",
            StringAsNationalCharVarying3 = "The",
            StringAsNationalCharacterVarying3 = "Col",
            StringAsChar3Utf8 = "Wha",
            StringAsCharacter3Utf8 = "doe",
            StringAsVarchar3Utf8 = "the",
            StringAsCharVarying3Utf8 = "tex",
            StringAsCharacterVarying3Utf8 = "men",
            BytesAsBinary3 = [10, 11, 12],
            BytesAsVarbinary3 = [11, 12, 13],
            BytesAsBinaryVarying3 = [12, 13, 14],
            CharAsVarchar3 = 'A',
            CharAsAsCharVarying3 = 'B',
            CharAsCharacterVarying3 = 'C',
            CharAsNvarchar3 = 'D',
            CharAsNationalCharVarying3 = 'E',
            CharAsNationalCharacterVarying3 = 'F'
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 78 });

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0=NULL (Size = 3) (DbType = Binary)
@p1=NULL (Size = 3) (DbType = Binary)
@p2=NULL (Size = 3) (DbType = Binary)
@p3=NULL (Size = 3) (DbType = AnsiString)
@p4=NULL (Size = 3) (DbType = AnsiString)
@p5=NULL (Size = 3)
@p6=NULL (Size = 3)
@p7=NULL (Size = 3)
@p8=NULL (Size = 3) (DbType = AnsiString)
@p9='78'
@p10=NULL (Size = 3) (DbType = AnsiStringFixedLength)
@p11=NULL (Size = 3) (DbType = StringFixedLength)
@p12=NULL (Size = 3) (DbType = AnsiString)
@p13=NULL (Size = 3)
@p14=NULL (Size = 3) (DbType = AnsiStringFixedLength)
@p15=NULL (Size = 3) (DbType = StringFixedLength)
@p16=NULL (Size = 3) (DbType = AnsiString)
@p17=NULL (Size = 3)
@p18=NULL (Size = 3)
@p19=NULL (Size = 3) (DbType = StringFixedLength)
@p20=NULL (Size = 3)
@p21=NULL (Size = 3) (DbType = StringFixedLength)
@p22=NULL (Size = 3)
@p23=NULL (Size = 3) (DbType = AnsiString)
@p24=NULL (Size = 3)",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 78), 78);
        }
    }

    private static void AssertNullMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Null(entity.StringAsChar3);
        Assert.Null(entity.StringAsCharacter3);
        Assert.Null(entity.StringAsVarchar3);
        Assert.Null(entity.StringAsCharVarying3);
        Assert.Null(entity.StringAsCharacterVarying3);
        Assert.Null(entity.StringAsNchar3);
        Assert.Null(entity.StringAsNationalCharacter3);
        Assert.Null(entity.StringAsNvarchar3);
        Assert.Null(entity.StringAsNationalCharVarying3);
        Assert.Null(entity.StringAsNationalCharacterVarying3);
        Assert.Null(entity.StringAsChar3Utf8);
        Assert.Null(entity.StringAsCharacter3Utf8);
        Assert.Null(entity.StringAsVarchar3Utf8);
        Assert.Null(entity.StringAsCharVarying3Utf8);
        Assert.Null(entity.StringAsCharacterVarying3Utf8);
        Assert.Null(entity.BytesAsBinary3);
        Assert.Null(entity.BytesAsVarbinary3);
        Assert.Null(entity.BytesAsBinaryVarying3);
        Assert.Null(entity.CharAsVarchar3);
        Assert.Null(entity.CharAsAsCharVarying3);
        Assert.Null(entity.CharAsCharacterVarying3);
        Assert.Null(entity.CharAsNvarchar3);
        Assert.Null(entity.CharAsNationalCharVarying3);
        Assert.Null(entity.CharAsNationalCharacterVarying3);
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            """
@p0='2017-01-02T12:11:12.1230000' (Scale = 3)
@p1='2016-01-02T11:11:12.5670000+00:00' (Scale = 3)
@p2='102' (Precision = 3)
@p3='101' (Precision = 3)
@p4='103' (Precision = 3)
@p5='85.55000305175781' (Size = 25)
@p6='85.5' (Size = 3)
@p7='83.33000183105469' (Size = 25)
@p8='83.3' (Size = 3)
@p9='77'
@p10='12:34' (Scale = 3) (DbType = Time)
@p11='12:34:56.7890123' (Scale = 3)
""",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedScaledDataTypesWithIdentity(MappedScaledDataTypesWithIdentity entity, int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Equal(83.3f, entity.FloatAsFloat3);
        Assert.Equal(85.5f, entity.FloatAsDoublePrecision3);
        Assert.Equal(83.33f, entity.FloatAsFloat25);
        Assert.Equal(85.55f, entity.FloatAsDoublePrecision25);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 567), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset3);
        Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12, 123), entity.DateTimeAsDatetime23);
        Assert.Equal(101m, entity.DecimalAsDecimal3);
        Assert.Equal(102m, entity.DecimalAsDec3);
        Assert.Equal(103m, entity.DecimalAsNumeric3);
        Assert.Equal(TimeOnly.Parse("12:34:56.789", CultureInfo.InvariantCulture), entity.TimeOnlyAsTime3);
        Assert.Equal(TimeSpan.Parse("12:34:56.789", CultureInfo.InvariantCulture), entity.TimeSpanAsTime3);
    }

    private static MappedScaledDataTypesWithIdentity CreateMappedScaledDataTypesWithIdentity(int id)
        => new()
        {
            Int = id,
            FloatAsFloat3 = 83.3f,
            FloatAsDoublePrecision3 = 85.5f,
            FloatAsFloat25 = 83.33f,
            FloatAsDoublePrecision25 = 85.55f,
            DateTimeOffsetAsDatetimeoffset3 = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12, 567), TimeSpan.Zero),
            DateTimeAsDatetime23 = new DateTime(2017, 1, 2, 12, 11, 12, 123),
            DecimalAsDecimal3 = 101m,
            DecimalAsDec3 = 102m,
            DecimalAsNumeric3 = 103m,
            TimeOnlyAsTime3 = TimeOnly.Parse("12:34:56.7890123", CultureInfo.InvariantCulture),
            TimeSpanAsTime3 = TimeSpan.Parse("12:34:56.7890123", CultureInfo.InvariantCulture)
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                CreateMappedPrecisionAndScaledDataTypesWithIdentity(77));

            Assert.Equal(1, context.SaveChanges());
        }

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='102.2' (Precision = 5) (Scale = 2)
@p1='101.1' (Precision = 5) (Scale = 2)
@p2='103.3' (Precision = 5) (Scale = 2)
@p3='77'",
            parameters,
            ignoreLineEndingDifferences: true);

        using (var context = CreateContext())
        {
            AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
        }
    }

    private static void AssertMappedPrecisionAndScaledDataTypesWithIdentity(
        MappedPrecisionAndScaledDataTypesWithIdentity entity,
        int id)
    {
        Assert.Equal(id, entity.Int);
        Assert.Equal(101.1m, entity.DecimalAsDecimal52);
        Assert.Equal(102.2m, entity.DecimalAsDec52);
        Assert.Equal(103.3m, entity.DecimalAsNumeric52);
    }

    private static MappedPrecisionAndScaledDataTypesWithIdentity CreateMappedPrecisionAndScaledDataTypesWithIdentity(int id)
        => new()
        {
            Int = id,
            DecimalAsDecimal52 = 101.1m,
            DecimalAsDec52 = 102.2m,
            DecimalAsNumeric52 = 103.3m
        };

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(177));
            context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(178));
            context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 177), 177);
            AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 178), 178);
            AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(177));
            context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(178));
            context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 177), 177);
            AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 178), 178);
            AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 278 });
            context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 279 });
            context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 280 });

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 278), 278);
            AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 279), 279);
            AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 280), 280);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(177));
            context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(178));
            context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 177), 177);
            AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 178), 178);
            AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 278 });
            context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 279 });
            context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 280 });

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 278), 278);
            AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 279), 279);
            AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 280), 280);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(177));
            context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(178));
            context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 177), 177);
            AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 178), 178);
            AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(177));
            context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(178));
            context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 177), 177);
            AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 178), 178);
            AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(177));
            context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(178));
            context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
            AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
            AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(177));
            context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(178));
            context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
            AssertMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
            AssertMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 278 });
            context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 279 });
            context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 280 });

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertNullMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 278), 278);
            AssertNullMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 279), 279);
            AssertNullMappedNullableDataTypesWithIdentity(
                context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 280), 280);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(177));
            context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(178));
            context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
            AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
            AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 278 });
            context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 279 });
            context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 280 });

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 278), 278);
            AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 279), 279);
            AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 280), 280);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(177));
            context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(178));
            context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
            AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
            AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity_in_batch()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(177));
            context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(178));
            context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(179));

            Assert.Equal(3, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
            AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
            AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
        }
    }

    [ConditionalFact]
    public virtual void Columns_have_expected_data_types()
    {
        var actual = QueryForColumnTypes(
            CreateContext(),
            nameof(ObjectBackedDataTypes), nameof(NullableBackedDataTypes), nameof(NonNullableBackedDataTypes));

        const string expected =
            """
Animal.Id ---> [int] [Precision = 10 Scale = 0]
AnimalDetails.AnimalId ---> [nullable int] [Precision = 10 Scale = 0]
AnimalDetails.BoolField ---> [int] [Precision = 10 Scale = 0]
AnimalDetails.Id ---> [int] [Precision = 10 Scale = 0]
AnimalIdentification.AnimalId ---> [int] [Precision = 10 Scale = 0]
AnimalIdentification.Id ---> [int] [Precision = 10 Scale = 0]
AnimalIdentification.Method ---> [int] [Precision = 10 Scale = 0]
BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
BinaryKeyDataType.Ex ---> [nullable nvarchar] [MaxLength = -1]
BinaryKeyDataType.Id ---> [varbinary] [MaxLength = 900]
BuiltInDataTypes.Enum16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.Enum32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypes.EnumS8 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.EnumU16 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.EnumU32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.EnumU64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [bit]
BuiltInDataTypes.TestByte ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypes.TestCharacter ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypes.TestDateOnly ---> [date] [Precision = 0]
BuiltInDataTypes.TestDateTime ---> [datetime2] [Precision = 7]
BuiltInDataTypes.TestDateTimeOffset ---> [datetimeoffset] [Precision = 7]
BuiltInDataTypes.TestDecimal ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypes.TestDouble ---> [float] [Precision = 53]
BuiltInDataTypes.TestInt16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSignedByte ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.TestSingle ---> [real] [Precision = 24]
BuiltInDataTypes.TestTimeOnly ---> [time] [Precision = 7]
BuiltInDataTypes.TestTimeSpan ---> [time] [Precision = 7]
BuiltInDataTypes.TestUnsignedInt16 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestUnsignedInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestUnsignedInt64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Enum16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypesShadow.Enum32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum8 ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypesShadow.EnumS8 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypesShadow.EnumU16 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.EnumU32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.EnumU64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestBoolean ---> [bit]
BuiltInDataTypesShadow.TestByte ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypesShadow.TestCharacter ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypesShadow.TestDateOnly ---> [date] [Precision = 0]
BuiltInDataTypesShadow.TestDateTime ---> [datetime2] [Precision = 7]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [datetimeoffset] [Precision = 7]
BuiltInDataTypesShadow.TestDecimal ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypesShadow.TestDouble ---> [float] [Precision = 53]
BuiltInDataTypesShadow.TestInt16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypesShadow.TestInt32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestSignedByte ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypesShadow.TestSingle ---> [real] [Precision = 24]
BuiltInDataTypesShadow.TestTimeOnly ---> [time] [Precision = 7]
BuiltInDataTypesShadow.TestTimeSpan ---> [time] [Precision = 7]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Enum16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.EnumS8 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.EnumU16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.EnumU32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.EnumU64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable bit]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableDateOnly ---> [nullable date] [Precision = 0]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable datetime2] [Precision = 7]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable real] [Precision = 24]
BuiltInNullableDataTypes.TestNullableTimeOnly ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.Enum32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Enum8 ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypesShadow.EnumS8 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [nullable bit]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableDateOnly ---> [nullable date] [Precision = 0]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable datetime2] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable real] [Precision = 24]
BuiltInNullableDataTypesShadow.TestNullableTimeOnly ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestString ---> [nullable nvarchar] [MaxLength = -1]
DateTimeEnclosure.DateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
DateTimeEnclosure.Id ---> [int] [Precision = 10 Scale = 0]
DoubleDataTypes.Double25 ---> [float] [Precision = 53]
DoubleDataTypes.Double3 ---> [real] [Precision = 24]
DoubleDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
EmailTemplate.Id ---> [uniqueidentifier]
EmailTemplate.TemplateType ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.BoolAsBit ---> [bit]
MappedDataTypes.ByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypes.BytesAsBinaryVaryingMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.BytesAsImage ---> [image] [MaxLength = 2147483647]
MappedDataTypes.BytesAsVarbinaryMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.CharAsAsCharVarying ---> [varchar] [MaxLength = 1]
MappedDataTypes.CharAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.CharAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.CharAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.CharAsNationalCharVarying ---> [nvarchar] [MaxLength = 1]
MappedDataTypes.CharAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypes.CharAsNvarchar ---> [nvarchar] [MaxLength = 1]
MappedDataTypes.CharAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypes.CharAsVarchar ---> [varchar] [MaxLength = 1]
MappedDataTypes.DateOnlyAsDate ---> [date] [Precision = 0]
MappedDataTypes.DateTimeAsDate ---> [date] [Precision = 0]
MappedDataTypes.DateTimeAsDatetime ---> [datetime] [Precision = 3]
MappedDataTypes.DateTimeAsDatetime2 ---> [datetime2] [Precision = 7]
MappedDataTypes.DateTimeAsSmalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypes.DateTimeOffsetAsDatetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedDataTypes.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypes.DecimalAsDec ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypes.DecimalAsMoney ---> [money] [Precision = 19 Scale = 4]
MappedDataTypes.DecimalAsNumeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypes.DecimalAsSmallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypes.DoubleAsDoublePrecision ---> [float] [Precision = 53]
MappedDataTypes.DoubleAsFloat ---> [float] [Precision = 53]
MappedDataTypes.EnumAsNvarchar20 ---> [nvarchar] [MaxLength = 20]
MappedDataTypes.EnumAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.FloatAsReal ---> [real] [Precision = 24]
MappedDataTypes.GuidAsUniqueidentifier ---> [uniqueidentifier]
MappedDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.LongAsBigInt ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.SByteAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypes.SByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypes.ShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypes.SqlVariantInt ---> [sql_variant] [MaxLength = 0]
MappedDataTypes.SqlVariantString ---> [sql_variant] [MaxLength = 0]
MappedDataTypes.StringAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsCharacterVaryingMaxUtf8 ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsCharVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsCharVaryingMaxUtf8 ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.StringAsNationalCharVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.StringAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypes.StringAsNvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.StringAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypes.StringAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsVarcharMaxUtf8 ---> [varchar] [MaxLength = -1]
MappedDataTypes.TimeOnlyAsTime ---> [time] [Precision = 7]
MappedDataTypes.TimeSpanAsTime ---> [time] [Precision = 7]
MappedDataTypes.UintAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.UintAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.UlongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.UlongAsDecimal200 ---> [decimal] [Precision = 20 Scale = 0]
MappedDataTypes.UShortAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.UShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.BoolAsBit ---> [bit]
MappedDataTypesWithIdentity.ByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypesWithIdentity.BytesAsBinaryVaryingMax ---> [varbinary] [MaxLength = -1]
MappedDataTypesWithIdentity.BytesAsImage ---> [image] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.BytesAsVarbinaryMax ---> [varbinary] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsAsCharVarying ---> [varchar] [MaxLength = 1]
MappedDataTypesWithIdentity.CharAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.CharAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsNationalCharVarying ---> [nvarchar] [MaxLength = 1]
MappedDataTypesWithIdentity.CharAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypesWithIdentity.CharAsNvarchar ---> [nvarchar] [MaxLength = 1]
MappedDataTypesWithIdentity.CharAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.CharAsVarchar ---> [varchar] [MaxLength = 1]
MappedDataTypesWithIdentity.DateOnlyAsDate ---> [date] [Precision = 0]
MappedDataTypesWithIdentity.DateTimeAsDate ---> [date] [Precision = 0]
MappedDataTypesWithIdentity.DateTimeAsDatetime ---> [datetime] [Precision = 3]
MappedDataTypesWithIdentity.DateTimeAsDatetime2 ---> [datetime2] [Precision = 7]
MappedDataTypesWithIdentity.DateTimeAsSmalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypesWithIdentity.DateTimeOffsetAsDatetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedDataTypesWithIdentity.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.DecimalAsDec ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.DecimalAsMoney ---> [money] [Precision = 19 Scale = 4]
MappedDataTypesWithIdentity.DecimalAsNumeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.DecimalAsSmallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypesWithIdentity.DoubleAsDoublePrecision ---> [float] [Precision = 53]
MappedDataTypesWithIdentity.DoubleAsFloat ---> [float] [Precision = 53]
MappedDataTypesWithIdentity.EnumAsNvarchar20 ---> [nvarchar] [MaxLength = 20]
MappedDataTypesWithIdentity.EnumAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.FloatAsReal ---> [real] [Precision = 24]
MappedDataTypesWithIdentity.GuidAsUniqueidentifier ---> [uniqueidentifier]
MappedDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.LongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.SByteAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.SbyteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypesWithIdentity.ShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.SqlVariantInt ---> [sql_variant] [MaxLength = 0]
MappedDataTypesWithIdentity.SqlVariantString ---> [sql_variant] [MaxLength = 0]
MappedDataTypesWithIdentity.StringAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsCharacterVaryingMaxUtf8 ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsCharVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsCharVaryingMaxUtf8 ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsNationalCharVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypesWithIdentity.StringAsNvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.StringAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsVarcharMaxUtf8 ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.TimeOnlyAsTime ---> [time] [Precision = 7]
MappedDataTypesWithIdentity.TimeSpanAsTime ---> [time] [Precision = 7]
MappedDataTypesWithIdentity.UintAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.UintAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.UlongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.UlongAsDecimal200 ---> [decimal] [Precision = 20 Scale = 0]
MappedDataTypesWithIdentity.UShortAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.UShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.BoolAsBit ---> [nullable bit]
MappedNullableDataTypes.ByteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypes.BytesAsBinaryVaryingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.BytesAsImage ---> [nullable image] [MaxLength = 2147483647]
MappedNullableDataTypes.BytesAsVarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.CharAsAsCharVarying ---> [nullable varchar] [MaxLength = 1]
MappedNullableDataTypes.CharAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.CharAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsNationalCharVarying ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypes.CharAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypes.CharAsNvarchar ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypes.CharAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypes.CharAsVarchar ---> [nullable varchar] [MaxLength = 1]
MappedNullableDataTypes.DateOnlyAsDate ---> [nullable date] [Precision = 0]
MappedNullableDataTypes.DateTimeAsDate ---> [nullable date] [Precision = 0]
MappedNullableDataTypes.DateTimeAsDatetime ---> [nullable datetime] [Precision = 3]
MappedNullableDataTypes.DateTimeAsDatetime2 ---> [nullable datetime2] [Precision = 7]
MappedNullableDataTypes.DateTimeAsSmalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypes.DateTimeOffsetAsDatetimeoffset ---> [nullable datetimeoffset] [Precision = 7]
MappedNullableDataTypes.Decimal ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypes.DecimalAsDec ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypes.DecimalAsMoney ---> [nullable money] [Precision = 19 Scale = 4]
MappedNullableDataTypes.DecimalAsNumeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypes.DecimalAsSmallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypes.DoubleAsDoublePrecision ---> [nullable float] [Precision = 53]
MappedNullableDataTypes.DoubleAsFloat ---> [nullable float] [Precision = 53]
MappedNullableDataTypes.EnumAsNvarchar20 ---> [nullable nvarchar] [MaxLength = 20]
MappedNullableDataTypes.EnumAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.FloatAsReal ---> [nullable real] [Precision = 24]
MappedNullableDataTypes.GuidAsUniqueidentifier ---> [nullable uniqueidentifier]
MappedNullableDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.LongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.SByteAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.SbyteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypes.ShortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.SqlVariantInt ---> [nullable sql_variant] [MaxLength = 0]
MappedNullableDataTypes.SqlVariantString ---> [nullable sql_variant] [MaxLength = 0]
MappedNullableDataTypes.StringAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsCharacterVaryingMaxUtf8 ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsCharVaryingMaxUtf8 ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypes.StringAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypes.StringAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsVarcharMaxUtf8 ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.TimeOnlyAsTime ---> [nullable time] [Precision = 7]
MappedNullableDataTypes.TimeSpanAsTime ---> [nullable time] [Precision = 7]
MappedNullableDataTypes.UintAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.UintAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.UlongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.UlongAsDecimal200 ---> [nullable decimal] [Precision = 20 Scale = 0]
MappedNullableDataTypes.UShortAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.UShortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.BoolAsBit ---> [nullable bit]
MappedNullableDataTypesWithIdentity.ByteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypesWithIdentity.BytesAsImage ---> [nullable image] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.BytesAsVarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.BytesAsVaryingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsAsCharVarying ---> [nullable varchar] [MaxLength = 1]
MappedNullableDataTypesWithIdentity.CharAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.CharAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsNationalCharVarying ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypesWithIdentity.CharAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypesWithIdentity.CharAsNvarchar ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypesWithIdentity.CharAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.CharAsVarchar ---> [nullable varchar] [MaxLength = 1]
MappedNullableDataTypesWithIdentity.DateOnlyAsDate ---> [nullable date] [Precision = 0]
MappedNullableDataTypesWithIdentity.DateTimeAsDate ---> [nullable date] [Precision = 0]
MappedNullableDataTypesWithIdentity.DateTimeAsDatetime ---> [nullable datetime] [Precision = 3]
MappedNullableDataTypesWithIdentity.DateTimeAsDatetime2 ---> [nullable datetime2] [Precision = 7]
MappedNullableDataTypesWithIdentity.DateTimeAsSmalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypesWithIdentity.DateTimeOffsetAsDatetimeoffset ---> [nullable datetimeoffset] [Precision = 7]
MappedNullableDataTypesWithIdentity.Decimal ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.DecimalAsDec ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.DecimalAsMoney ---> [nullable money] [Precision = 19 Scale = 4]
MappedNullableDataTypesWithIdentity.DecimalAsNumeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.DecimalAsSmallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypesWithIdentity.DoubleAsDoublePrecision ---> [nullable float] [Precision = 53]
MappedNullableDataTypesWithIdentity.DoubleAsFloat ---> [nullable float] [Precision = 53]
MappedNullableDataTypesWithIdentity.EnumAsNvarchar20 ---> [nullable nvarchar] [MaxLength = 20]
MappedNullableDataTypesWithIdentity.EnumAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.FloatAsReal ---> [nullable real] [Precision = 24]
MappedNullableDataTypesWithIdentity.GuidAsUniqueidentifier ---> [nullable uniqueidentifier]
MappedNullableDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.Int ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.LongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.SByteAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.SbyteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypesWithIdentity.ShortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.SqlVariantInt ---> [nullable sql_variant] [MaxLength = 0]
MappedNullableDataTypesWithIdentity.SqlVariantString ---> [nullable sql_variant] [MaxLength = 0]
MappedNullableDataTypesWithIdentity.StringAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsCharacterVaryingMaxUtf8 ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsCharVaryingMaxUtf8 ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypesWithIdentity.StringAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.StringAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsVarcharMaxUtf8 ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.TimeOnlyAsTime ---> [nullable time] [Precision = 7]
MappedNullableDataTypesWithIdentity.TimeSpanAsTime ---> [nullable time] [Precision = 7]
MappedNullableDataTypesWithIdentity.UintAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.UintAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.UlongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.UlongAsDecimal200 ---> [nullable decimal] [Precision = 20 Scale = 0]
MappedNullableDataTypesWithIdentity.UShortAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.UshortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedPrecisionAndScaledDataTypes.DecimalAsDec52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.DecimalAsDecimal52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.DecimalAsNumeric52 ---> [numeric] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.DecimalAsDec52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.DecimalAsDecimal52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.DecimalAsNumeric52 ---> [numeric] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledSeparatelyDataTypes.DecimalAsDec52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledSeparatelyDataTypes.DecimalAsDecimal52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledSeparatelyDataTypes.DecimalAsNumeric52 ---> [numeric] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledSeparatelyDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypes.DateTimeAsDatetime23 ---> [datetime2] [Precision = 3]
MappedScaledDataTypes.DateTimeOffsetAsDatetimeoffset3 ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypes.DecimalAsDec3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.DecimalAsDecimal3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.DecimalAsNumeric3 ---> [numeric] [Precision = 3 Scale = 0]
MappedScaledDataTypes.FloatAsDoublePrecision25 ---> [float] [Precision = 53]
MappedScaledDataTypes.FloatAsDoublePrecision3 ---> [real] [Precision = 24]
MappedScaledDataTypes.FloatAsFloat25 ---> [float] [Precision = 53]
MappedScaledDataTypes.FloatAsFloat3 ---> [real] [Precision = 24]
MappedScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypes.TimeOnlyAsTime3 ---> [time] [Precision = 3]
MappedScaledDataTypes.TimeSpanAsTime3 ---> [time] [Precision = 3]
MappedScaledDataTypesWithIdentity.DateTimeAsDatetime23 ---> [datetime2] [Precision = 3]
MappedScaledDataTypesWithIdentity.DateTimeOffsetAsDatetimeoffset3 ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypesWithIdentity.DecimalAsDec3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.DecimalAsDecimal3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.DecimalAsNumeric3 ---> [numeric] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.FloatAsDoublePrecision25 ---> [float] [Precision = 53]
MappedScaledDataTypesWithIdentity.FloatAsDoublePrecision3 ---> [real] [Precision = 24]
MappedScaledDataTypesWithIdentity.FloatAsFloat25 ---> [float] [Precision = 53]
MappedScaledDataTypesWithIdentity.FloatAsFloat3 ---> [real] [Precision = 24]
MappedScaledDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.TimeOnlyAsTime3 ---> [time] [Precision = 3]
MappedScaledDataTypesWithIdentity.TimeSpanAsTime3 ---> [time] [Precision = 3]
MappedScaledSeparatelyDataTypes.DateTimeAsDatetime23 ---> [datetime2] [Precision = 3]
MappedScaledSeparatelyDataTypes.DateTimeOffsetAsDatetimeoffset3 ---> [datetimeoffset] [Precision = 3]
MappedScaledSeparatelyDataTypes.DecimalAsDec3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledSeparatelyDataTypes.DecimalAsDecimal3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledSeparatelyDataTypes.DecimalAsNumeric3 ---> [numeric] [Precision = 3 Scale = 0]
MappedScaledSeparatelyDataTypes.FloatAsDoublePrecision25 ---> [float] [Precision = 53]
MappedScaledSeparatelyDataTypes.FloatAsDoublePrecision3 ---> [real] [Precision = 24]
MappedScaledSeparatelyDataTypes.FloatAsFloat25 ---> [float] [Precision = 53]
MappedScaledSeparatelyDataTypes.FloatAsFloat3 ---> [real] [Precision = 24]
MappedScaledSeparatelyDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledSeparatelyDataTypes.TimeOnlyAsTime3 ---> [time] [Precision = 3]
MappedScaledSeparatelyDataTypes.TimeSpanAsTime3 ---> [time] [Precision = 3]
MappedSizedDataTypes.BytesAsBinary3 ---> [nullable binary] [MaxLength = 3]
MappedSizedDataTypes.BytesAsBinaryVarying3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypes.BytesAsVarbinary3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypes.CharAsAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypes.StringAsChar3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.StringAsChar3Utf8 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharacter3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharacter3Utf8 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharacterVarying3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharVarying3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNationalCharacter3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNchar3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsVarchar3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.BytesAsBinary3 ---> [nullable binary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.BytesAsBinaryVarying3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.BytesAsVarbinary3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.StringAsChar3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsChar3Utf8 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharacter3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharacter3Utf8 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharacterVarying3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharVarying3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNationalCharacter3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNchar3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsVarchar3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.BytesAsBinary3 ---> [nullable binary] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.BytesAsBinaryVarying3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.BytesAsVarbinary3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.CharAsAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.CharAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.CharAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.CharAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.CharAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.CharAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedSeparatelyDataTypes.StringAsChar3 ---> [nullable char] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsChar3Utf8 ---> [nullable char] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsCharacter3 ---> [nullable char] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsCharacter3Utf8 ---> [nullable char] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsCharacterVarying3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsCharVarying3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsNationalCharacter3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsNchar3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedSeparatelyDataTypes.StringAsVarchar3Utf8 ---> [nullable varchar] [MaxLength = 3]
MappedSquareDataTypes.BoolAsBit ---> [bit]
MappedSquareDataTypes.ByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedSquareDataTypes.BytesAsImage ---> [image] [MaxLength = 2147483647]
MappedSquareDataTypes.BytesAsVarbinaryMax ---> [varbinary] [MaxLength = -1]
MappedSquareDataTypes.CharAsInt ---> [int] [Precision = 10 Scale = 0]
MappedSquareDataTypes.CharAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedSquareDataTypes.CharAsNvarchar ---> [nvarchar] [MaxLength = 1]
MappedSquareDataTypes.CharAsText ---> [text] [MaxLength = 2147483647]
MappedSquareDataTypes.CharAsVarchar ---> [varchar] [MaxLength = 1]
MappedSquareDataTypes.DateOnlyAsDate ---> [date] [Precision = 0]
MappedSquareDataTypes.DateTimeAsDate ---> [date] [Precision = 0]
MappedSquareDataTypes.DateTimeAsDatetime ---> [datetime] [Precision = 3]
MappedSquareDataTypes.DateTimeAsDatetime2 ---> [datetime2] [Precision = 7]
MappedSquareDataTypes.DateTimeAsSmalldatetime ---> [smalldatetime] [Precision = 0]
MappedSquareDataTypes.DateTimeOffsetAsDatetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedSquareDataTypes.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedSquareDataTypes.DecimalAsDec ---> [decimal] [Precision = 18 Scale = 0]
MappedSquareDataTypes.DecimalAsMoney ---> [money] [Precision = 19 Scale = 4]
MappedSquareDataTypes.DecimalAsNumeric ---> [numeric] [Precision = 18 Scale = 0]
MappedSquareDataTypes.DecimalAsSmallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedSquareDataTypes.DoubleAsFloat ---> [float] [Precision = 53]
MappedSquareDataTypes.EnumAsNvarchar20 ---> [nvarchar] [MaxLength = 20]
MappedSquareDataTypes.EnumAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedSquareDataTypes.FloatAsReal ---> [real] [Precision = 24]
MappedSquareDataTypes.GuidAsUniqueidentifier ---> [uniqueidentifier]
MappedSquareDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedSquareDataTypes.LongAsBigInt ---> [bigint] [Precision = 19 Scale = 0]
MappedSquareDataTypes.SByteAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedSquareDataTypes.SByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedSquareDataTypes.ShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedSquareDataTypes.SqlVariantInt ---> [sql_variant] [MaxLength = 0]
MappedSquareDataTypes.SqlVariantString ---> [sql_variant] [MaxLength = 0]
MappedSquareDataTypes.StringAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedSquareDataTypes.StringAsNvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedSquareDataTypes.StringAsText ---> [text] [MaxLength = 2147483647]
MappedSquareDataTypes.StringAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedSquareDataTypes.TimeOnlyAsTime ---> [time] [Precision = 7]
MappedSquareDataTypes.TimeSpanAsTime ---> [time] [Precision = 7]
MappedSquareDataTypes.UintAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedSquareDataTypes.UintAsInt ---> [int] [Precision = 10 Scale = 0]
MappedSquareDataTypes.UlongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedSquareDataTypes.UlongAsDecimal200 ---> [decimal] [Precision = 20 Scale = 0]
MappedSquareDataTypes.UShortAsInt ---> [int] [Precision = 10 Scale = 0]
MappedSquareDataTypes.UShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.StringUnbounded ---> [nullable nvarchar] [MaxLength = -1]
StringEnclosure.Id ---> [int] [Precision = 10 Scale = 0]
StringEnclosure.Value ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
UnicodeDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]

""";

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void Can_get_column_types_from_built_model()
    {
        using var context = CreateContext();
        var typeMapper = context.GetService<IRelationalTypeMappingSource>();

        foreach (var property in context.Model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
        {
            var columnType = property.GetColumnType();
            Assert.NotNull(columnType);

            if (property[RelationalAnnotationNames.ColumnType] == null)
            {
                Assert.Equal(
                    columnType.ToLowerInvariant(),
                    typeMapper.FindMapping(property).StoreType.ToLowerInvariant());
            }
        }
    }

    public override async Task Object_to_string_conversion()
    {
        await base.Object_to_string_conversion();

        AssertSql(
            """
SELECT CONVERT(varchar(4), [b].[TestSignedByte]) AS [Sbyte], CONVERT(varchar(3), [b].[TestByte]) AS [Byte], CONVERT(varchar(6), [b].[TestInt16]) AS [Short], CONVERT(varchar(5), [b].[TestUnsignedInt16]) AS [Ushort], CONVERT(varchar(11), [b].[TestInt32]) AS [Int], CONVERT(varchar(10), [b].[TestUnsignedInt32]) AS [Uint], CONVERT(varchar(20), [b].[TestInt64]) AS [Long], CONVERT(varchar(20), [b].[TestUnsignedInt64]) AS [Ulong], CONVERT(varchar(100), [b].[TestSingle]) AS [Float], CONVERT(varchar(100), [b].[TestDouble]) AS [Double], CONVERT(varchar(100), [b].[TestDecimal]) AS [Decimal], CONVERT(varchar(1), [b].[TestCharacter]) AS [Char], CONVERT(varchar(100), [b].[TestDateTime]) AS [DateTime], CONVERT(varchar(100), [b].[TestDateTimeOffset]) AS [DateTimeOffset], CONVERT(varchar(100), [b].[TestTimeSpan]) AS [TimeSpan], CONVERT(varchar(100), [b].[TestDateOnly]) AS [DateOnly], CONVERT(varchar(100), [b].[TestTimeOnly]) AS [TimeOnly]
FROM [BuiltInDataTypes] AS [b]
WHERE [b].[Id] = 13
""");
    }

    public static string QueryForColumnTypes(DbContext context, params string[] tablesToIgnore)
    {
        const string query =
            """
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    DATETIME_PRECISION
FROM INFORMATION_SCHEMA.COLUMNS
""";

        var columns = new List<ColumnInfo>();

        using (context)
        {
            var connection = context.Database.GetDbConnection();

            var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var columnInfo = new ColumnInfo
                {
                    TableName = reader.GetString(0),
                    ColumnName = reader.GetString(1),
                    DataType = reader.GetString(2),
                    IsNullable = reader.IsDBNull(3) ? null : reader.GetString(3) == "YES",
                    MaxLength = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    NumericPrecision = reader.IsDBNull(5) ? null : reader.GetByte(5),
                    NumericScale = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    DateTimePrecision = reader.IsDBNull(7) ? null : reader.GetInt16(7)
                };

                if (!tablesToIgnore.Contains(columnInfo.TableName))
                {
                    columns.Add(columnInfo);
                }
            }
        }

        var builder = new StringBuilder();

        foreach (var column in columns.OrderBy(e => e.TableName).ThenBy(e => e.ColumnName))
        {
            builder.Append(column.TableName);
            builder.Append(".");
            builder.Append(column.ColumnName);
            builder.Append(" ---> [");

            if (column.IsNullable == true)
            {
                builder.Append("nullable ");
            }

            builder.Append(column.DataType);
            builder.Append("]");

            if (column.MaxLength.HasValue)
            {
                builder.Append(" [MaxLength = ");
                builder.Append(column.MaxLength);
                builder.Append("]");
            }

            if (column.NumericPrecision.HasValue)
            {
                builder.Append(" [Precision = ");
                builder.Append(column.NumericPrecision);
            }

            if (column.DateTimePrecision.HasValue)
            {
                builder.Append(" [Precision = ");
                builder.Append(column.DateTimePrecision);
            }

            if (column.NumericScale.HasValue)
            {
                builder.Append(" Scale = ");
                builder.Append(column.NumericScale);
            }

            if (column.NumericPrecision.HasValue
                || column.DateTimePrecision.HasValue
                || column.NumericScale.HasValue)
            {
                builder.Append("]");
            }

            builder.AppendLine();
        }

        var actual = builder.ToString();
        return actual;
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class BuiltInDataTypesSqlServerFixture : BuiltInDataTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override bool PreservesDateTimeKind
            => false;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<MappedDataTypes>(
                b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int).ValueGeneratedNever();
                    b.Property(e => e.StringAsVarcharMaxUtf8).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsCharVaryingMaxUtf8).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsCharacterVaryingMaxUtf8).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsNationalCharacterVaryingMax).HasMaxLength(100);
                });

            modelBuilder.Entity<MappedSquareDataTypes>(
                b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int).ValueGeneratedNever();
                });

            modelBuilder.Entity<MappedNullableDataTypes>(
                b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int).ValueGeneratedNever();
                });

            modelBuilder.Entity<MappedDataTypesWithIdentity>();
            modelBuilder.Entity<MappedNullableDataTypesWithIdentity>();

            modelBuilder.Entity<MappedSizedDataTypes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<MappedScaledDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.FloatAsDoublePrecision3).HasPrecision(5);
                    b.Property(e => e.FloatAsDoublePrecision3).HasPrecision(5);
                    b.Property(e => e.DecimalAsDec3).HasPrecision(5);
                    b.Property(e => e.TimeOnlyAsTime3).HasPrecision(5);
                    b.Property(e => e.TimeSpanAsTime3).HasPrecision(5);
                });

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.DecimalAsDec52).HasPrecision(7, 3);
                });

            MakeRequired<MappedDataTypes>(modelBuilder);
            MakeRequired<MappedSquareDataTypes>(modelBuilder);
            MakeRequired<MappedDataTypesWithIdentity>(modelBuilder);

            modelBuilder.Entity<MappedSizedDataTypesWithIdentity>();
            modelBuilder.Entity<MappedScaledDataTypesWithIdentity>();
            modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>();
            modelBuilder.Entity<MappedSizedDataTypesWithIdentity>();
            modelBuilder.Entity<MappedScaledDataTypesWithIdentity>();

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>(
                b =>
                {
                    b.Property(e => e.DecimalAsDecimal52).HasPrecision(7, 3);
                });

            modelBuilder.Entity<MappedSizedSeparatelyDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.StringAsChar3).HasMaxLength(3);
                    b.Property(e => e.StringAsCharacter3).HasMaxLength(3);
                    b.Property(e => e.StringAsVarchar3).HasMaxLength(3);
                    b.Property(e => e.StringAsCharVarying3).HasMaxLength(3);
                    b.Property(e => e.StringAsCharacterVarying3).HasMaxLength(3);
                    b.Property(e => e.StringAsNchar3).HasMaxLength(3);
                    b.Property(e => e.StringAsNationalCharacter3).HasMaxLength(3);
                    b.Property(e => e.StringAsNvarchar3).HasMaxLength(3);
                    b.Property(e => e.StringAsNationalCharVarying3).HasMaxLength(3);
                    b.Property(e => e.StringAsNationalCharacterVarying3).HasMaxLength(3);
                    b.Property(e => e.StringAsChar3Utf8).HasMaxLength(3).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsCharacter3Utf8).HasMaxLength(3).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsVarchar3Utf8).HasMaxLength(3).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsCharVarying3Utf8).HasMaxLength(3).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.StringAsCharacterVarying3Utf8).HasMaxLength(3).UseCollation("LATIN1_GENERAL_100_CI_AS_SC_UTF8");
                    b.Property(e => e.BytesAsBinary3).HasMaxLength(3);
                    b.Property(e => e.BytesAsVarbinary3).HasMaxLength(3);
                    b.Property(e => e.BytesAsBinaryVarying3).HasMaxLength(3);
                    b.Property(e => e.CharAsVarchar3).HasMaxLength(3);
                    b.Property(e => e.CharAsAsCharVarying3).HasMaxLength(3);
                    b.Property(e => e.CharAsCharacterVarying3).HasMaxLength(3);
                    b.Property(e => e.CharAsNvarchar3).HasMaxLength(3);
                    b.Property(e => e.CharAsNationalCharVarying3).HasMaxLength(3);
                    b.Property(e => e.CharAsNationalCharacterVarying3).HasMaxLength(3);
                });

            modelBuilder.Entity<MappedScaledSeparatelyDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.FloatAsFloat3).HasPrecision(3);
                    b.Property(e => e.FloatAsDoublePrecision3).HasPrecision(3);
                    b.Property(e => e.FloatAsFloat25).HasPrecision(25);
                    b.Property(e => e.FloatAsDoublePrecision25).HasPrecision(25);
                    b.Property(e => e.DateTimeOffsetAsDatetimeoffset3).HasPrecision(3);
                    b.Property(e => e.DateTimeAsDatetime23).HasPrecision(3);
                    b.Property(e => e.DecimalAsDecimal3).HasPrecision(3);
                    b.Property(e => e.DecimalAsDec3).HasPrecision(3);
                    b.Property(e => e.DecimalAsNumeric3).HasPrecision(3);
                });

            modelBuilder.Entity<DoubleDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Double3).HasPrecision(3);
                    b.Property(e => e.Double25).HasPrecision(25);
                });

            modelBuilder.Entity<MappedPrecisionAndScaledSeparatelyDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.DecimalAsDecimal52).HasPrecision(5, 2);
                    b.Property(e => e.DecimalAsDec52).HasPrecision(5, 2);
                    b.Property(e => e.DecimalAsNumeric52).HasPrecision(5, 2);
                });
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var options = base.AddOptions(builder).ConfigureWarnings(
                c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

            new SqlServerDbContextOptionsBuilder(options).MinBatchSize(1);

            return options;
        }

        public override bool SupportsBinaryKeys
            => true;

        public override DateTime DefaultDateTime
            => new();
    }

    [Flags]
    protected enum StringEnum16 : short
    {
        Value1 = 1,
        Value2 = 2,
        Value4 = 4
    }

    [Flags]
    protected enum StringEnumU16 : ushort
    {
        Value1 = 1,
        Value2 = 2,
        Value4 = 4
    }

    protected class MappedDataTypes
    {
        [Column(TypeName = "int")]
        public int Int { get; set; }

        [Column(TypeName = "bigint")]
        public long LongAsBigInt { get; set; }

        [Column(TypeName = "smallint")]
        public short ShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public byte ByteAsTinyint { get; set; }

        [Column(TypeName = "int")]
        public uint UintAsInt { get; set; }

        [Column(TypeName = "bigint")]
        public ulong UlongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public ushort UShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public sbyte SByteAsTinyint { get; set; }

        [Column(TypeName = "bit")]
        public bool BoolAsBit { get; set; }

        [Column(TypeName = "money")]
        public decimal DecimalAsMoney { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal DecimalAsSmallmoney { get; set; }

        [Column(TypeName = "float")]
        public double DoubleAsFloat { get; set; }

        [Column(TypeName = "real")]
        public float FloatAsReal { get; set; }

        [Column(TypeName = "double precision")]
        public double DoubleAsDoublePrecision { get; set; }

        [Column(TypeName = "date")]
        public DateOnly DateOnlyAsDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateTimeAsDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateTimeAsDatetime2 { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime DateTimeAsSmalldatetime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateTimeAsDatetime { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimeOnlyAsTime { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan TimeSpanAsTime { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string StringAsVarcharMax { get; set; }

        [Column(TypeName = "char varying(max)")]
        public string StringAsCharVaryingMax { get; set; }

        [Column(TypeName = "character varying(max)")]
        public string StringAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string StringAsNvarcharMax { get; set; }

        [Column(TypeName = "national char varying(max)")]
        public string StringAsNationalCharVaryingMax { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public string StringAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "varchar(max)")]
        [Unicode]
        public string StringAsVarcharMaxUtf8 { get; set; }

        [Column(TypeName = "char varying(max)")]
        [Unicode]
        public string StringAsCharVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "character varying(max)")]
        [Unicode]
        public string StringAsCharacterVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "text")]
        public string StringAsText { get; set; }

        [Column(TypeName = "ntext")]
        public string StringAsNtext { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] BytesAsVarbinaryMax { get; set; }

        [Column(TypeName = "binary varying(max)")]
        public byte[] BytesAsBinaryVaryingMax { get; set; }

        [Column(TypeName = "image")]
        public byte[] BytesAsImage { get; set; }

        [Column(TypeName = "decimal")]
        public decimal Decimal { get; set; }

        [Column(TypeName = "dec")]
        public decimal DecimalAsDec { get; set; }

        [Column(TypeName = "numeric")]
        public decimal DecimalAsNumeric { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid GuidAsUniqueidentifier { get; set; }

        [Column(TypeName = "bigint")]
        public uint UintAsBigint { get; set; }

        [Column(TypeName = "decimal(20,0)")]
        public ulong UlongAsDecimal200 { get; set; }

        [Column(TypeName = "int")]
        public ushort UShortAsInt { get; set; }

        [Column(TypeName = "smallint")]
        public sbyte SByteAsSmallint { get; set; }

        [Column(TypeName = "varchar")]
        public char CharAsVarchar { get; set; }

        [Column(TypeName = "char varying(1)")]
        public char CharAsAsCharVarying { get; set; }

        [Column(TypeName = "character varying(max)")]
        public char CharAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar")]
        public char CharAsNvarchar { get; set; }

        [Column(TypeName = "national char varying(1)")]
        public char CharAsNationalCharVarying { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public char CharAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "text")]
        public char CharAsText { get; set; }

        [Column(TypeName = "ntext")]
        public char CharAsNtext { get; set; }

        [Column(TypeName = "int")]
        public char CharAsInt { get; set; }

        [Column(TypeName = "varchar(max)")]
        public StringEnum16 EnumAsVarcharMax { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public StringEnumU16 EnumAsNvarchar20 { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantString { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantInt { get; set; }
    }

    protected class MappedSquareDataTypes
    {
        [Column(TypeName = "[int]")]
        public int Int { get; set; }

        [Column(TypeName = "[bigint]")]
        public long LongAsBigInt { get; set; }

        [Column(TypeName = "[smallint]")]
        public short ShortAsSmallint { get; set; }

        [Column(TypeName = "[tinyint]")]
        public byte ByteAsTinyint { get; set; }

        [Column(TypeName = "[int]")]
        public uint UintAsInt { get; set; }

        [Column(TypeName = "[bigint]")]
        public ulong UlongAsBigint { get; set; }

        [Column(TypeName = "[smallint]")]
        public ushort UShortAsSmallint { get; set; }

        [Column(TypeName = "[tinyint]")]
        public sbyte SByteAsTinyint { get; set; }

        [Column(TypeName = "[bit]")]
        public bool BoolAsBit { get; set; }

        [Column(TypeName = "[money]")]
        public decimal DecimalAsMoney { get; set; }

        [Column(TypeName = "[smallmoney]")]
        public decimal DecimalAsSmallmoney { get; set; }

        [Column(TypeName = "[float]")]
        public double DoubleAsFloat { get; set; }

        [Column(TypeName = "[real]")]
        public float FloatAsReal { get; set; }

        [Column(TypeName = "[date]")]
        public DateOnly DateOnlyAsDate { get; set; }

        [Column(TypeName = "[date]")]
        public DateTime DateTimeAsDate { get; set; }

        [Column(TypeName = "[datetimeoffset]")]
        public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

        [Column(TypeName = "[datetime2]")]
        public DateTime DateTimeAsDatetime2 { get; set; }

        [Column(TypeName = "[smalldatetime]")]
        public DateTime DateTimeAsSmalldatetime { get; set; }

        [Column(TypeName = "[datetime]")]
        public DateTime DateTimeAsDatetime { get; set; }

        [Column(TypeName = "[time]")]
        public TimeOnly TimeOnlyAsTime { get; set; }

        [Column(TypeName = "[time]")]
        public TimeSpan TimeSpanAsTime { get; set; }

        [Column(TypeName = "[varchar](max)")]
        public string StringAsVarcharMax { get; set; }

        [Column(TypeName = "[nvarchar](max)")]
        public string StringAsNvarcharMax { get; set; }

        [Column(TypeName = "[text]")]
        public string StringAsText { get; set; }

        [Column(TypeName = "[ntext]")]
        public string StringAsNtext { get; set; }

        [Column(TypeName = "[varbinary](max)")]
        public byte[] BytesAsVarbinaryMax { get; set; }

        [Column(TypeName = "[image]")]
        public byte[] BytesAsImage { get; set; }

        [Column(TypeName = "[decimal]")]
        public decimal Decimal { get; set; }

        [Column(TypeName = "[dec]")]
        public decimal DecimalAsDec { get; set; }

        [Column(TypeName = "[numeric]")]
        public decimal DecimalAsNumeric { get; set; }

        [Column(TypeName = "[uniqueidentifier]")]
        public Guid GuidAsUniqueidentifier { get; set; }

        [Column(TypeName = "[bigint]")]
        public uint UintAsBigint { get; set; }

        [Column(TypeName = "[decimal](20,0)")]
        public ulong UlongAsDecimal200 { get; set; }

        [Column(TypeName = "[int]")]
        public ushort UShortAsInt { get; set; }

        [Column(TypeName = "[smallint]")]
        public sbyte SByteAsSmallint { get; set; }

        [Column(TypeName = "[varchar](1)")]
        public char CharAsVarchar { get; set; }

        [Column(TypeName = "[nvarchar]")]
        public char CharAsNvarchar { get; set; }

        [Column(TypeName = "[text]")]
        public char CharAsText { get; set; }

        [Column(TypeName = "[ntext]")]
        public char CharAsNtext { get; set; }

        [Column(TypeName = "[int]")]
        public char CharAsInt { get; set; }

        [Column(TypeName = "[varchar](max)")]
        public StringEnum16 EnumAsVarcharMax { get; set; }

        [Column(TypeName = "[nvarchar](20)")]
        public StringEnumU16 EnumAsNvarchar20 { get; set; }

        [Column(TypeName = "[sql_variant]")]
        public object SqlVariantString { get; set; }

        [Column(TypeName = "[sql_variant]")]
        public object SqlVariantInt { get; set; }
    }

    protected class MappedSizedDataTypes
    {
        public int Id { get; set; }

        [Column(TypeName = "char(3)")]
        public string StringAsChar3 { get; set; }

        [Column(TypeName = "character(3)")]
        public string StringAsCharacter3 { get; set; }

        [Column(TypeName = "varchar(3)")]
        public string StringAsVarchar3 { get; set; }

        [Column(TypeName = "char varying(3)")]
        public string StringAsCharVarying3 { get; set; }

        [Column(TypeName = "character varying(3)")]
        public string StringAsCharacterVarying3 { get; set; }

        [Column(TypeName = "nchar(3)")]
        public string StringAsNchar3 { get; set; }

        [Column(TypeName = "national character(3)")]
        public string StringAsNationalCharacter3 { get; set; }

        [Column(TypeName = "nvarchar(3)")]
        public string StringAsNvarchar3 { get; set; }

        [Column(TypeName = "national char varying(3)")]
        public string StringAsNationalCharVarying3 { get; set; }

        [Column(TypeName = "national character varying(3)")]
        public string StringAsNationalCharacterVarying3 { get; set; }

        [Column(TypeName = "char(3)")]
        [Unicode]
        public string StringAsChar3Utf8 { get; set; }

        [Column(TypeName = "character(3)")]
        [Unicode]
        public string StringAsCharacter3Utf8 { get; set; }

        [Column(TypeName = "varchar(3)")]
        [Unicode]
        public string StringAsVarchar3Utf8 { get; set; }

        [Column(TypeName = "char varying(3)")]
        [Unicode]
        public string StringAsCharVarying3Utf8 { get; set; }

        [Column(TypeName = "character varying(3)")]
        [Unicode]
        public string StringAsCharacterVarying3Utf8 { get; set; }

        [Column(TypeName = "binary(3)")]
        public byte[] BytesAsBinary3 { get; set; }

        [Column(TypeName = "varbinary(3)")]
        public byte[] BytesAsVarbinary3 { get; set; }

        [Column(TypeName = "binary varying(3)")]
        public byte[] BytesAsBinaryVarying3 { get; set; }

        [Column(TypeName = "varchar(3)")]
        public char? CharAsVarchar3 { get; set; }

        [Column(TypeName = "char varying(3)")]
        public char? CharAsAsCharVarying3 { get; set; }

        [Column(TypeName = "character varying(3)")]
        public char? CharAsCharacterVarying3 { get; set; }

        [Column(TypeName = "nvarchar(3)")]
        public char? CharAsNvarchar3 { get; set; }

        [Column(TypeName = "national char varying(3)")]
        public char? CharAsNationalCharVarying3 { get; set; }

        [Column(TypeName = "national character varying(3)")]
        public char? CharAsNationalCharacterVarying3 { get; set; }
    }

    protected class MappedSizedSeparatelyDataTypes
    {
        public int Id { get; set; }

        [Column(TypeName = "char")]
        public string StringAsChar3 { get; set; }

        [Column(TypeName = "character")]
        public string StringAsCharacter3 { get; set; }

        [Column(TypeName = "varchar")]
        public string StringAsVarchar3 { get; set; }

        [Column(TypeName = "char varying")]
        public string StringAsCharVarying3 { get; set; }

        [Column(TypeName = "character varying")]
        public string StringAsCharacterVarying3 { get; set; }

        [Column(TypeName = "nchar")]
        public string StringAsNchar3 { get; set; }

        [Column(TypeName = "national character")]
        public string StringAsNationalCharacter3 { get; set; }

        [Column(TypeName = "nvarchar")]
        public string StringAsNvarchar3 { get; set; }

        [Column(TypeName = "national char varying")]
        public string StringAsNationalCharVarying3 { get; set; }

        [Column(TypeName = "national character varying")]
        public string StringAsNationalCharacterVarying3 { get; set; }

        [Column(TypeName = "char")]
        public string StringAsChar3Utf8 { get; set; }

        [Column(TypeName = "character")]
        public string StringAsCharacter3Utf8 { get; set; }

        [Column(TypeName = "varchar")]
        public string StringAsVarchar3Utf8 { get; set; }

        [Column(TypeName = "char varying")]
        public string StringAsCharVarying3Utf8 { get; set; }

        [Column(TypeName = "character varying")]
        public string StringAsCharacterVarying3Utf8 { get; set; }

        [Column(TypeName = "binary")]
        public byte[] BytesAsBinary3 { get; set; }

        [Column(TypeName = "varbinary")]
        public byte[] BytesAsVarbinary3 { get; set; }

        [Column(TypeName = "binary varying")]
        public byte[] BytesAsBinaryVarying3 { get; set; }

        [Column(TypeName = "varchar")]
        public char? CharAsVarchar3 { get; set; }

        [Column(TypeName = "char varying")]
        public char? CharAsAsCharVarying3 { get; set; }

        [Column(TypeName = "character varying")]
        public char? CharAsCharacterVarying3 { get; set; }

        [Column(TypeName = "nvarchar")]
        public char? CharAsNvarchar3 { get; set; }

        [Column(TypeName = "national char varying")]
        public char? CharAsNationalCharVarying3 { get; set; }

        [Column(TypeName = "national character varying")]
        public char? CharAsNationalCharacterVarying3 { get; set; }
    }

    protected class MappedScaledDataTypes
    {
        public int Id { get; set; }

        [Column(TypeName = "float(3)")]
        [Precision(5)]
        public float FloatAsFloat3 { get; set; }

        [Column(TypeName = "double precision(3)")]
        public float FloatAsDoublePrecision3 { get; set; }

        [Column(TypeName = "float(25)")]
        [Precision(5)]
        public float FloatAsFloat25 { get; set; }

        [Column(TypeName = "double precision(25)")]
        public float FloatAsDoublePrecision25 { get; set; }

        [Column(TypeName = "datetimeoffset(3)")]
        [Precision(5)]
        public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

        [Column(TypeName = "datetime2(3)")]
        [Precision(5)]
        public DateTime DateTimeAsDatetime23 { get; set; }

        [Column(TypeName = "decimal(3)")]
        [Precision(5)]
        public decimal DecimalAsDecimal3 { get; set; }

        [Column(TypeName = "dec(3)")]
        public decimal DecimalAsDec3 { get; set; }

        [Column(TypeName = "numeric(3)")]
        [Precision(5)]
        public decimal DecimalAsNumeric3 { get; set; }

        [Column(TypeName = "time(3)")]
        public TimeOnly TimeOnlyAsTime3 { get; set; }

        [Column(TypeName = "time(3)")]
        public TimeSpan TimeSpanAsTime3 { get; set; }
    }

    protected class MappedScaledSeparatelyDataTypes
    {
        public int Id { get; set; }

        [Column(TypeName = "float")]
        public float FloatAsFloat3 { get; set; }

        [Column(TypeName = "double precision")]
        public float FloatAsDoublePrecision3 { get; set; }

        [Column(TypeName = "float")]
        public float FloatAsFloat25 { get; set; }

        [Column(TypeName = "double precision")]
        public float FloatAsDoublePrecision25 { get; set; }

        [Column(TypeName = "datetimeoffset")]
        public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateTimeAsDatetime23 { get; set; }

        [Column(TypeName = "decimal")]
        public decimal DecimalAsDecimal3 { get; set; }

        [Column(TypeName = "dec")]
        public decimal DecimalAsDec3 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal DecimalAsNumeric3 { get; set; }

        [Column(TypeName = "time(3)")]
        public TimeOnly TimeOnlyAsTime3 { get; set; }

        [Column(TypeName = "time(3)")]
        public TimeSpan TimeSpanAsTime3 { get; set; }
    }

    protected class DoubleDataTypes
    {
        public int Id { get; set; }

        public double Double3 { get; set; }
        public double Double25 { get; set; }
    }

    protected class MappedPrecisionAndScaledDataTypes
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Precision(7, 3)]
        public decimal DecimalAsDecimal52 { get; set; }

        [Column(TypeName = "dec(5,2)")]
        public decimal DecimalAsDec52 { get; set; }

        [Column(TypeName = "numeric(5,2)")]
        public decimal DecimalAsNumeric52 { get; set; }
    }

    protected class MappedPrecisionAndScaledSeparatelyDataTypes
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal")]
        public decimal DecimalAsDecimal52 { get; set; }

        [Column(TypeName = "dec")]
        public decimal DecimalAsDec52 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal DecimalAsNumeric52 { get; set; }
    }

    protected class MappedNullableDataTypes
    {
        [Column(TypeName = "int")]
        public int? Int { get; set; }

        [Column(TypeName = "bigint")]
        public long? LongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public short? ShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public byte? ByteAsTinyint { get; set; }

        [Column(TypeName = "int")]
        public uint? UintAsInt { get; set; }

        [Column(TypeName = "bigint")]
        public ulong? UlongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public ushort? UShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public sbyte? SbyteAsTinyint { get; set; }

        [Column(TypeName = "bit")]
        public bool? BoolAsBit { get; set; }

        [Column(TypeName = "money")]
        public decimal? DecimalAsMoney { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal? DecimalAsSmallmoney { get; set; }

        [Column(TypeName = "float")]
        public double? DoubleAsFloat { get; set; }

        [Column(TypeName = "real")]
        public float? FloatAsReal { get; set; }

        [Column(TypeName = "double precision")]
        public double? DoubleAsDoublePrecision { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? DateOnlyAsDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateTimeAsDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        public DateTimeOffset? DateTimeOffsetAsDatetimeoffset { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateTimeAsDatetime2 { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? DateTimeAsSmalldatetime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateTimeAsDatetime { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly? TimeOnlyAsTime { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? TimeSpanAsTime { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string StringAsVarcharMax { get; set; }

        [Column(TypeName = "char varying(max)")]
        public string StringAsCharVaryingMax { get; set; }

        [Column(TypeName = "character varying(max)")]
        public string StringAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string StringAsNvarcharMax { get; set; }

        [Column(TypeName = "national char varying(max)")]
        [MaxLength(100)]
        public string StringAsNationalCharVaryingMax { get; set; }

        [Column(TypeName = "national character varying(max)")]
        [StringLength(100)]
        public string StringAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "varchar(max)")]
        [Unicode]
        public string StringAsVarcharMaxUtf8 { get; set; }

        [Column(TypeName = "char varying(max)")]
        [Unicode]
        public string StringAsCharVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "character varying(max)")]
        [Unicode]
        public string StringAsCharacterVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "text")]
        public string StringAsText { get; set; }

        [Column(TypeName = "ntext")]
        public string StringAsNtext { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] BytesAsVarbinaryMax { get; set; }

        [Column(TypeName = "binary varying(max)")]
        public byte[] BytesAsBinaryVaryingMax { get; set; }

        [Column(TypeName = "image")]
        public byte[] BytesAsImage { get; set; }

        [Column(TypeName = "decimal")]
        public decimal? Decimal { get; set; }

        [Column(TypeName = "dec")]
        public decimal? DecimalAsDec { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? DecimalAsNumeric { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid? GuidAsUniqueidentifier { get; set; }

        [Column(TypeName = "bigint")]
        public uint? UintAsBigint { get; set; }

        [Column(TypeName = "decimal(20,0)")]
        public ulong? UlongAsDecimal200 { get; set; }

        [Column(TypeName = "int")]
        public ushort? UShortAsInt { get; set; }

        [Column(TypeName = "smallint")]
        public sbyte? SByteAsSmallint { get; set; }

        [Column(TypeName = "varchar(1)")]
        public char? CharAsVarchar { get; set; }

        [Column(TypeName = "char varying")]
        public char? CharAsAsCharVarying { get; set; }

        [Column(TypeName = "character varying(max)")]
        public char? CharAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar")]
        public char? CharAsNvarchar { get; set; }

        [Column(TypeName = "national char varying(1)")]
        public char? CharAsNationalCharVarying { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public char? CharAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "text")]
        public char? CharAsText { get; set; }

        [Column(TypeName = "ntext")]
        public char? CharAsNtext { get; set; }

        [Column(TypeName = "int")]
        public char? CharAsInt { get; set; }

        [Column(TypeName = "varchar(max)")]
        public StringEnum16? EnumAsVarcharMax { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public StringEnumU16? EnumAsNvarchar20 { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantString { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantInt { get; set; }
    }

    protected class MappedDataTypesWithIdentity
    {
        public int Id { get; set; }

        [Column(TypeName = "int")]
        public int Int { get; set; }

        [Column(TypeName = "bigint")]
        public long LongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public short ShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public byte ByteAsTinyint { get; set; }

        [Column(TypeName = "int")]
        public uint UintAsInt { get; set; }

        [Column(TypeName = "bigint")]
        public ulong UlongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public ushort UShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public sbyte SbyteAsTinyint { get; set; }

        [Column(TypeName = "bit")]
        public bool BoolAsBit { get; set; }

        [Column(TypeName = "money")]
        public decimal DecimalAsMoney { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal DecimalAsSmallmoney { get; set; }

        [Column(TypeName = "float")]
        public double DoubleAsFloat { get; set; }

        [Column(TypeName = "real")]
        public float FloatAsReal { get; set; }

        [Column(TypeName = "double precision")]
        public double DoubleAsDoublePrecision { get; set; }

        [Column(TypeName = "date")]
        public DateOnly DateOnlyAsDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateTimeAsDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateTimeAsDatetime2 { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime DateTimeAsSmalldatetime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateTimeAsDatetime { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimeOnlyAsTime { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan TimeSpanAsTime { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string StringAsVarcharMax { get; set; }

        [Column(TypeName = "char varying(max)")]
        public string StringAsCharVaryingMax { get; set; }

        [Column(TypeName = "character varying(max)")]
        public string StringAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string StringAsNvarcharMax { get; set; }

        [Column(TypeName = "national char varying(max)")]
        public string StringAsNationalCharVaryingMax { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public string StringAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "varchar(max)")]
        [Unicode]
        public string StringAsVarcharMaxUtf8 { get; set; }

        [Column(TypeName = "char varying(max)")]
        [Unicode]
        public string StringAsCharVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "character varying(max)")]
        [Unicode]
        public string StringAsCharacterVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "text")]
        public string StringAsText { get; set; }

        [Column(TypeName = "ntext")]
        public string StringAsNtext { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] BytesAsVarbinaryMax { get; set; }

        [Column(TypeName = "binary varying(max)")]
        public byte[] BytesAsBinaryVaryingMax { get; set; }

        [Column(TypeName = "image")]
        public byte[] BytesAsImage { get; set; }

        [Column(TypeName = "decimal")]
        public decimal Decimal { get; set; }

        [Column(TypeName = "dec")]
        public decimal DecimalAsDec { get; set; }

        [Column(TypeName = "numeric")]
        public decimal DecimalAsNumeric { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid GuidAsUniqueidentifier { get; set; }

        [Column(TypeName = "bigint")]
        public uint UintAsBigint { get; set; }

        [Column(TypeName = "decimal(20,0)")]
        public ulong UlongAsDecimal200 { get; set; }

        [Column(TypeName = "int")]
        public ushort UShortAsInt { get; set; }

        [Column(TypeName = "smallint")]
        public sbyte SByteAsSmallint { get; set; }

        [Column(TypeName = "varchar(1)")]
        public char CharAsVarchar { get; set; }

        [Column(TypeName = "char varying")]
        public char CharAsAsCharVarying { get; set; }

        [Column(TypeName = "character varying(max)")]
        public char CharAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar")]
        public char CharAsNvarchar { get; set; }

        [Column(TypeName = "national char varying(1)")]
        public char CharAsNationalCharVarying { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public char CharAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "text")]
        public char CharAsText { get; set; }

        [Column(TypeName = "ntext")]
        public char CharAsNtext { get; set; }

        [Column(TypeName = "int")]
        public char CharAsInt { get; set; }

        [Column(TypeName = "varchar(max)")]
        public StringEnum16 EnumAsVarcharMax { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public StringEnumU16 EnumAsNvarchar20 { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantString { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantInt { get; set; }
    }

    protected class MappedSizedDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int Int { get; set; }

        [Column(TypeName = "char(3)")]
        public string StringAsChar3 { get; set; }

        [Column(TypeName = "character(3)")]
        public string StringAsCharacter3 { get; set; }

        [Column(TypeName = "varchar(3)")]
        public string StringAsVarchar3 { get; set; }

        [Column(TypeName = "char varying(3)")]
        public string StringAsCharVarying3 { get; set; }

        [Column(TypeName = "character varying(3)")]
        public string StringAsCharacterVarying3 { get; set; }

        [Column(TypeName = "nchar(3)")]
        public string StringAsNchar3 { get; set; }

        [Column(TypeName = "national character(3)")]
        public string StringAsNationalCharacter3 { get; set; }

        [Column(TypeName = "nvarchar(3)")]
        public string StringAsNvarchar3 { get; set; }

        [Column(TypeName = "national char varying(3)")]
        public string StringAsNationalCharVarying3 { get; set; }

        [Column(TypeName = "national character varying(3)")]
        public string StringAsNationalCharacterVarying3 { get; set; }

        [Column(TypeName = "char(3)")]
        [Unicode]
        public string StringAsChar3Utf8 { get; set; }

        [Column(TypeName = "character(3)")]
        [Unicode]
        public string StringAsCharacter3Utf8 { get; set; }

        [Column(TypeName = "varchar(3)")]
        [Unicode]
        public string StringAsVarchar3Utf8 { get; set; }

        [Column(TypeName = "char varying(3)")]
        [Unicode]
        public string StringAsCharVarying3Utf8 { get; set; }

        [Column(TypeName = "character varying(3)")]
        [Unicode]
        public string StringAsCharacterVarying3Utf8 { get; set; }

        [Column(TypeName = "binary(3)")]
        public byte[] BytesAsBinary3 { get; set; }

        [Column(TypeName = "varbinary(3)")]
        public byte[] BytesAsVarbinary3 { get; set; }

        [Column(TypeName = "binary varying(3)")]
        public byte[] BytesAsBinaryVarying3 { get; set; }

        [Column(TypeName = "varchar(3)")]
        public char? CharAsVarchar3 { get; set; }

        [Column(TypeName = "char varying(3)")]
        public char? CharAsAsCharVarying3 { get; set; }

        [Column(TypeName = "character varying(3)")]
        public char? CharAsCharacterVarying3 { get; set; }

        [Column(TypeName = "nvarchar(3)")]
        public char? CharAsNvarchar3 { get; set; }

        [Column(TypeName = "national char varying(3)")]
        public char? CharAsNationalCharVarying3 { get; set; }

        [Column(TypeName = "national character varying(3)")]
        public char? CharAsNationalCharacterVarying3 { get; set; }
    }

    protected class MappedScaledDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int Int { get; set; }

        [Column(TypeName = "float(3)")]
        public float FloatAsFloat3 { get; set; }

        [Column(TypeName = "double precision(3)")]
        public float FloatAsDoublePrecision3 { get; set; }

        [Column(TypeName = "float(25)")]
        public float FloatAsFloat25 { get; set; }

        [Column(TypeName = "double precision(25)")]
        public float FloatAsDoublePrecision25 { get; set; }

        [Column(TypeName = "datetimeoffset(3)")]
        public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

        [Column(TypeName = "datetime2(3)")]
        public DateTime DateTimeAsDatetime23 { get; set; }

        [Column(TypeName = "decimal(3)")]
        public decimal DecimalAsDecimal3 { get; set; }

        [Column(TypeName = "dec(3)")]
        public decimal DecimalAsDec3 { get; set; }

        [Column(TypeName = "numeric(3)")]
        public decimal DecimalAsNumeric3 { get; set; }

        [Column(TypeName = "time(3)")]
        public TimeOnly TimeOnlyAsTime3 { get; set; }

        [Column(TypeName = "time(3)")]
        public TimeSpan TimeSpanAsTime3 { get; set; }
    }

    protected class MappedPrecisionAndScaledDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int Int { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DecimalAsDecimal52 { get; set; }

        [Column(TypeName = "dec(5,2)")]
        [Precision(7, 3)]
        public decimal DecimalAsDec52 { get; set; }

        [Column(TypeName = "numeric(5,2)")]
        public decimal DecimalAsNumeric52 { get; set; }
    }

    protected class MappedNullableDataTypesWithIdentity
    {
        public int Id { get; set; }

        [Column(TypeName = "int")]
        public int? Int { get; set; }

        [Column(TypeName = "bigint")]
        public long? LongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public short? ShortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public byte? ByteAsTinyint { get; set; }

        [Column(TypeName = "int")]
        public uint? UintAsInt { get; set; }

        [Column(TypeName = "bigint")]
        public ulong? UlongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public ushort? UshortAsSmallint { get; set; }

        [Column(TypeName = "tinyint")]
        public sbyte? SbyteAsTinyint { get; set; }

        [Column(TypeName = "bit")]
        public bool? BoolAsBit { get; set; }

        [Column(TypeName = "money")]
        public decimal? DecimalAsMoney { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal? DecimalAsSmallmoney { get; set; }

        [Column(TypeName = "float")]
        public double? DoubleAsFloat { get; set; }

        [Column(TypeName = "real")]
        public float? FloatAsReal { get; set; }

        [Column(TypeName = "double precision")]
        public double? DoubleAsDoublePrecision { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? DateOnlyAsDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateTimeAsDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        public DateTimeOffset? DateTimeOffsetAsDatetimeoffset { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateTimeAsDatetime2 { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? DateTimeAsSmalldatetime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateTimeAsDatetime { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly? TimeOnlyAsTime { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? TimeSpanAsTime { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string StringAsVarcharMax { get; set; }

        [Column(TypeName = "char varying(max)")]
        public string StringAsCharVaryingMax { get; set; }

        [Column(TypeName = "character varying(max)")]
        public string StringAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string StringAsNvarcharMax { get; set; }

        [Column(TypeName = "national char varying(max)")]
        public string StringAsNationalCharVaryingMax { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public string StringAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "varchar(max)")]
        [Unicode]
        public string StringAsVarcharMaxUtf8 { get; set; }

        [Column(TypeName = "char varying(max)")]
        [Unicode]
        public string StringAsCharVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "character varying(max)")]
        [Unicode]
        public string StringAsCharacterVaryingMaxUtf8 { get; set; }

        [Column(TypeName = "text")]
        public string StringAsText { get; set; }

        [Column(TypeName = "ntext")]
        public string StringAsNtext { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] BytesAsVarbinaryMax { get; set; }

        [Column(TypeName = "binary varying(max)")]
        public byte[] BytesAsVaryingMax { get; set; }

        [Column(TypeName = "image")]
        public byte[] BytesAsImage { get; set; }

        [Column(TypeName = "decimal")]
        public decimal? Decimal { get; set; }

        [Column(TypeName = "dec")]
        public decimal? DecimalAsDec { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? DecimalAsNumeric { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid? GuidAsUniqueidentifier { get; set; }

        [Column(TypeName = "bigint")]
        public uint? UintAsBigint { get; set; }

        [Column(TypeName = "decimal(20,0)")]
        public ulong? UlongAsDecimal200 { get; set; }

        [Column(TypeName = "int")]
        public ushort? UShortAsInt { get; set; }

        [Column(TypeName = "smallint")]
        public sbyte? SByteAsSmallint { get; set; }

        [Column(TypeName = "varchar")]
        public char? CharAsVarchar { get; set; }

        [Column(TypeName = "char varying(1)")]
        public char? CharAsAsCharVarying { get; set; }

        [Column(TypeName = "character varying(max)")]
        public char? CharAsCharacterVaryingMax { get; set; }

        [Column(TypeName = "nvarchar(1)")]
        public char? CharAsNvarchar { get; set; }

        [Column(TypeName = "national char varying")]
        public char? CharAsNationalCharVarying { get; set; }

        [Column(TypeName = "national character varying(max)")]
        public char? CharAsNationalCharacterVaryingMax { get; set; }

        [Column(TypeName = "text")]
        public char? CharAsText { get; set; }

        [Column(TypeName = "ntext")]
        public char? CharAsNtext { get; set; }

        [Column(TypeName = "int")]
        public char? CharAsInt { get; set; }

        [Column(TypeName = "varchar(max)")]
        public StringEnum16? EnumAsVarcharMax { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public StringEnumU16? EnumAsNvarchar20 { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantString { get; set; }

        [Column(TypeName = "sql_variant")]
        public object SqlVariantInt { get; set; }
    }

    public class ColumnInfo
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool? IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public int? DateTimePrecision { get; set; }
    }
}
