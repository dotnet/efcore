// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
public class ConvertToProviderTypesSqlServerTest(ConvertToProviderTypesSqlServerTest.ConvertToProviderTypesSqlServerFixture fixture)
    : ConvertToProviderTypesTestBase<
        ConvertToProviderTypesSqlServerTest.ConvertToProviderTypesSqlServerFixture>(fixture)
{
    [ConditionalFact]
    public virtual void Columns_have_expected_data_types()
    {
        var actual = BuiltInDataTypesSqlServerTest.QueryForColumnTypes(
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
AnimalIdentification.Method ---> [nvarchar] [MaxLength = 6]
BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
BinaryForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
BinaryKeyDataType.Ex ---> [nullable nvarchar] [MaxLength = -1]
BinaryKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
BuiltInDataTypes.Enum16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [nchar] [MaxLength = 17]
BuiltInDataTypes.EnumS8 ---> [varchar] [MaxLength = -1]
BuiltInDataTypes.EnumU16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.EnumU32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.EnumU64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypes.TestByte ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestCharacter ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDateOnly ---> [date] [Precision = 0]
BuiltInDataTypes.TestDateTime ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDateTimeOffset ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypes.TestDouble ---> [decimal] [Precision = 38 Scale = 17]
BuiltInDataTypes.TestInt16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSignedByte ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSingle ---> [decimal] [Precision = 38 Scale = 17]
BuiltInDataTypes.TestTimeOnly ---> [time] [Precision = 7]
BuiltInDataTypes.TestTimeSpan ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestUnsignedInt16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.TestUnsignedInt32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.TestUnsignedInt64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Enum16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumS8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumU16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.EnumU32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.EnumU64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestBoolean ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypesShadow.TestByte ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestCharacter ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDateOnly ---> [date] [Precision = 0]
BuiltInDataTypesShadow.TestDateTime ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypesShadow.TestDouble ---> [decimal] [Precision = 38 Scale = 17]
BuiltInDataTypesShadow.TestInt16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestSignedByte ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestSingle ---> [decimal] [Precision = 38 Scale = 17]
BuiltInDataTypesShadow.TestTimeOnly ---> [time] [Precision = 7]
BuiltInDataTypesShadow.TestTimeSpan ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Enum16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumS8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumU16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.EnumU32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.EnumU64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateOnly ---> [nullable date] [Precision = 0]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable decimal] [Precision = 38 Scale = 17]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable decimal] [Precision = 38 Scale = 17]
BuiltInNullableDataTypes.TestNullableTimeOnly ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Enum32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Enum8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.EnumS8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.EnumU16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableDateOnly ---> [nullable date] [Precision = 0]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable decimal] [Precision = 38 Scale = 17]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable decimal] [Precision = 38 Scale = 17]
BuiltInNullableDataTypesShadow.TestNullableTimeOnly ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestString ---> [nullable nvarchar] [MaxLength = -1]
DateTimeEnclosure.DateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
DateTimeEnclosure.Id ---> [int] [Precision = 10 Scale = 0]
EmailTemplate.Id ---> [uniqueidentifier]
EmailTemplate.TemplateType ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.ByteArray5 ---> [nullable nvarchar] [MaxLength = 8]
MaxLengthDataTypes.ByteArray9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable varbinary] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.StringUnbounded ---> [nullable varbinary] [MaxLength = -1]
StringEnclosure.Id ---> [int] [Precision = 10 Scale = 0]
StringEnclosure.Value ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
StringKeyDataType.Id ---> [varbinary] [MaxLength = 900]
UnicodeDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]

""";

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    public override Task Object_to_string_conversion()
        // Return values are not string
        => Task.CompletedTask;

    public class ConvertToProviderTypesSqlServerFixture : ConvertToProviderTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(
                    c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.Enum8).IsFixedLength();
        }
    }
}
