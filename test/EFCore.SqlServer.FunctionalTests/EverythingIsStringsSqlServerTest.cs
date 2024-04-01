// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
public class EverythingIsStringsSqlServerTest(EverythingIsStringsSqlServerTest.EverythingIsStringsSqlServerFixture fixture)
    : BuiltInDataTypesTestBase<
        EverythingIsStringsSqlServerTest.EverythingIsStringsSqlServerFixture>(fixture)
{
    [ConditionalFact]
    public virtual void Columns_have_expected_data_types()
    {
        var actual = BuiltInDataTypesSqlServerTest.QueryForColumnTypes(
            CreateContext(),
            nameof(ObjectBackedDataTypes),
            nameof(NullableBackedDataTypes),
            nameof(NonNullableBackedDataTypes),
            nameof(Animal),
            nameof(AnimalDetails),
            nameof(AnimalIdentification));

        const string expected =
            """
BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
BinaryForeignKeyDataType.Id ---> [nvarchar] [MaxLength = 64]
BinaryKeyDataType.Ex ---> [nullable nvarchar] [MaxLength = -1]
BinaryKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
BuiltInDataTypes.Enum16 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.Enum32 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.Enum64 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.Enum8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.EnumS8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.EnumU16 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.EnumU32 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.EnumU64 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.Id ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.PartitionId ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestBoolean ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypes.TestByte ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestCharacter ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypes.TestDateOnly ---> [nvarchar] [MaxLength = 10]
BuiltInDataTypes.TestDateTime ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestDateTimeOffset ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestDecimal ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestDouble ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestInt16 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestInt32 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestInt64 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestSignedByte ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestSingle ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestTimeOnly ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestTimeSpan ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestUnsignedInt16 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestUnsignedInt32 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestUnsignedInt64 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.Enum16 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.Enum32 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.Enum64 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.Enum8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumS8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumU16 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumU32 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumU64 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.Id ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.PartitionId ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestBoolean ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypesShadow.TestByte ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestCharacter ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypesShadow.TestDateOnly ---> [nvarchar] [MaxLength = 10]
BuiltInDataTypesShadow.TestDateTime ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypesShadow.TestDecimal ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestDouble ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestInt16 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestInt32 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestInt64 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestSignedByte ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestSingle ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestTimeOnly ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypesShadow.TestTimeSpan ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.Enum16 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.Enum32 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.Enum64 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.Enum8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumS8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumU16 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumU32 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumU64 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.Id ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.PartitionId ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestByteArray ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableDateOnly ---> [nullable nvarchar] [MaxLength = 10]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableTimeOnly ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum16 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum32 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum64 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.EnumS8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.EnumU16 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.EnumU32 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.EnumU64 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Id ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.PartitionId ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestByteArray ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableDateOnly ---> [nullable nvarchar] [MaxLength = 10]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableTimeOnly ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypesShadow.TestString ---> [nullable nvarchar] [MaxLength = -1]
DateTimeEnclosure.DateTimeOffset ---> [nullable nvarchar] [MaxLength = 48]
DateTimeEnclosure.Id ---> [nvarchar] [MaxLength = 64]
EmailTemplate.Id ---> [nvarchar] [MaxLength = 36]
EmailTemplate.TemplateType ---> [nvarchar] [MaxLength = -1]
MaxLengthDataTypes.ByteArray5 ---> [nullable nvarchar] [MaxLength = 8]
MaxLengthDataTypes.ByteArray9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [nvarchar] [MaxLength = 64]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.StringUnbounded ---> [nullable nvarchar] [MaxLength = -1]
StringEnclosure.Id ---> [nvarchar] [MaxLength = 64]
StringEnclosure.Value ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [nvarchar] [MaxLength = 64]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
UnicodeDataTypes.Id ---> [nvarchar] [MaxLength = 64]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]

""";

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    public override Task Can_read_back_mapped_enum_from_collection_first_or_default()
        // The query needs to generate TOP(1)
        => Task.CompletedTask;

    public override Task Can_read_back_bool_mapped_as_int_through_navigation()
        // Column is mapped as int rather than string
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_constant()
        // Column is mapped as int rather than string
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_parameter()
        // Column is mapped as int rather than string
        => Task.CompletedTask;

    public class EverythingIsStringsSqlServerFixture : BuiltInDataTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        public override bool PreservesDateTimeKind
            => false;

        protected override string StoreName
            => "EverythingIsStrings";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerStringsTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(
                    c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<MaxLengthDataTypes>().Property(e => e.ByteArray5).HasMaxLength(8);

            modelBuilder.Ignore<Animal>();
            modelBuilder.Ignore<AnimalIdentification>();
            modelBuilder.Ignore<AnimalDetails>();
        }
    }

    public class SqlServerStringsTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerStringsTestStoreFactory Instance { get; } = new();

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => base.AddProviderServices(
                serviceCollection.AddSingleton<IRelationalTypeMappingSource, SqlServerStringsTypeMappingSource>());
    }

    public class SqlServerStringsTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly SqlServerStringTypeMapping _fixedLengthUnicodeString = new(unicode: true, fixedLength: true);
        private readonly SqlServerStringTypeMapping _variableLengthUnicodeString = new(unicode: true);
        private readonly SqlServerStringTypeMapping _fixedLengthAnsiString = new(fixedLength: true);
        private readonly SqlServerStringTypeMapping _variableLengthAnsiString = new();
        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        public SqlServerStringsTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "char varying", _variableLengthAnsiString },
                    { "char", _fixedLengthAnsiString },
                    { "character varying", _variableLengthAnsiString },
                    { "character", _fixedLengthAnsiString },
                    { "national char varying", _variableLengthUnicodeString },
                    { "national character varying", _variableLengthUnicodeString },
                    { "national character", _fixedLengthUnicodeString },
                    { "nchar", _fixedLengthUnicodeString },
                    { "ntext", _variableLengthUnicodeString },
                    { "nvarchar", _variableLengthUnicodeString },
                    { "text", _variableLengthAnsiString },
                    { "varchar", _variableLengthAnsiString }
                };
        }

        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => FindRawMapping(mappingInfo)?.WithTypeMappingInfo(mappingInfo);

        private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
                {
                    return clrType == null
                        || mapping.ClrType == clrType
                            ? mapping
                            : null;
                }
            }

            if (clrType == typeof(string))
            {
                var isAnsi = mappingInfo.IsUnicode == false;
                var isFixedLength = mappingInfo.IsFixedLength == true;
                var baseName = isAnsi ? "varchar" : "nvarchar";
                var maxSize = isAnsi ? 8000 : 4000;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);
                if (size > maxSize)
                {
                    size = isFixedLength ? maxSize : null;
                }

                return new SqlServerStringTypeMapping(
                    baseName + "(" + (size == null ? "max" : size.ToString()) + ")",
                    !isAnsi,
                    size,
                    isFixedLength,
                    storeTypePostfix: size == null ? StoreTypePostfix.None : null);
            }

            return null;
        }
    }
}
