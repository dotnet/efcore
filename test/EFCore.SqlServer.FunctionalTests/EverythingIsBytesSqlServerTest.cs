// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
public class EverythingIsBytesSqlServerTest(EverythingIsBytesSqlServerTest.EverythingIsBytesSqlServerFixture fixture)
    : BuiltInDataTypesTestBase<EverythingIsBytesSqlServerTest.EverythingIsBytesSqlServerFixture>(fixture)
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
BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [varbinary] [MaxLength = 4]
BinaryKeyDataType.Ex ---> [nullable varbinary] [MaxLength = -1]
BinaryKeyDataType.Id ---> [varbinary] [MaxLength = 900]
BuiltInDataTypes.Enum16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypes.Enum32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.Enum64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.Enum8 ---> [varbinary] [MaxLength = 1]
BuiltInDataTypes.EnumS8 ---> [varbinary] [MaxLength = 1]
BuiltInDataTypes.EnumU16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypes.EnumU32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.EnumU64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.Id ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.PartitionId ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.TestBoolean ---> [varbinary] [MaxLength = 1]
BuiltInDataTypes.TestByte ---> [varbinary] [MaxLength = 1]
BuiltInDataTypes.TestCharacter ---> [varbinary] [MaxLength = 2]
BuiltInDataTypes.TestDateOnly ---> [varbinary] [MaxLength = -1]
BuiltInDataTypes.TestDateTime ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestDateTimeOffset ---> [varbinary] [MaxLength = 12]
BuiltInDataTypes.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypes.TestDouble ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestInt16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypes.TestInt32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.TestInt64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestSignedByte ---> [varbinary] [MaxLength = 1]
BuiltInDataTypes.TestSingle ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.TestTimeOnly ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestTimeSpan ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestUnsignedInt16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypes.TestUnsignedInt32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.TestUnsignedInt64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.Enum16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypesShadow.Enum32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.Enum64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.Enum8 ---> [varbinary] [MaxLength = 1]
BuiltInDataTypesShadow.EnumS8 ---> [varbinary] [MaxLength = 1]
BuiltInDataTypesShadow.EnumU16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypesShadow.EnumU32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.EnumU64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.Id ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.PartitionId ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.TestBoolean ---> [varbinary] [MaxLength = 1]
BuiltInDataTypesShadow.TestByte ---> [varbinary] [MaxLength = 1]
BuiltInDataTypesShadow.TestCharacter ---> [varbinary] [MaxLength = 2]
BuiltInDataTypesShadow.TestDateOnly ---> [varbinary] [MaxLength = -1]
BuiltInDataTypesShadow.TestDateTime ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [varbinary] [MaxLength = 12]
BuiltInDataTypesShadow.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypesShadow.TestDouble ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestInt16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypesShadow.TestInt32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.TestInt64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestSignedByte ---> [varbinary] [MaxLength = 1]
BuiltInDataTypesShadow.TestSingle ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.TestTimeOnly ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestTimeSpan ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.Enum16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypes.Enum32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.Enum64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.Enum8 ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypes.EnumS8 ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypes.EnumU16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypes.EnumU32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.EnumU64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.Id ---> [varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.PartitionId ---> [varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypes.TestNullableDateOnly ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable varbinary] [MaxLength = 12]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.TestNullableTimeOnly ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestString ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.Enum32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.Enum64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.Enum8 ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypesShadow.EnumS8 ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypesShadow.EnumU16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.EnumU32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.EnumU64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.Id ---> [varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.PartitionId ---> [varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.TestNullableDateOnly ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable varbinary] [MaxLength = 12]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestNullableTimeOnly ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestString ---> [nullable varbinary] [MaxLength = -1]
DateTimeEnclosure.DateTimeOffset ---> [nullable varbinary] [MaxLength = 12]
DateTimeEnclosure.Id ---> [varbinary] [MaxLength = 4]
EmailTemplate.Id ---> [varbinary] [MaxLength = 16]
EmailTemplate.TemplateType ---> [varbinary] [MaxLength = 4]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [varbinary] [MaxLength = 4]
MaxLengthDataTypes.String3 ---> [nullable varbinary] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.StringUnbounded ---> [nullable varbinary] [MaxLength = -1]
StringEnclosure.Id ---> [varbinary] [MaxLength = 4]
StringEnclosure.Value ---> [nullable varbinary] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [varbinary] [MaxLength = 4]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
StringKeyDataType.Id ---> [varbinary] [MaxLength = 900]
UnicodeDataTypes.Id ---> [varbinary] [MaxLength = 4]
UnicodeDataTypes.StringAnsi ---> [nullable varbinary] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varbinary] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varbinary] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable varbinary] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable varbinary] [MaxLength = -1]

""";

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    public override Task Can_read_back_mapped_enum_from_collection_first_or_default()
        // The query needs to generate TOP(1)
        => Task.CompletedTask;

    public override Task Can_read_back_bool_mapped_as_int_through_navigation()
        // Column is mapped as int rather than byte[]
        => Task.CompletedTask;

    public override Task Object_to_string_conversion()
        // Return values are string which byte[] cannot read
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_constant()
        // Column is mapped as int rather than byte[]
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_parameter()
        // Column is mapped as int rather than byte[]
        => Task.CompletedTask;

    public class EverythingIsBytesSqlServerFixture : BuiltInDataTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => false;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override string StoreName
            => "EverythingIsBytes";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerBytesTestStoreFactory.Instance;

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

            modelBuilder.Ignore<Animal>();
            modelBuilder.Ignore<AnimalIdentification>();
            modelBuilder.Ignore<AnimalDetails>();
        }
    }

    public class SqlServerBytesTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerBytesTestStoreFactory Instance { get; } = new();

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => base.AddProviderServices(
                serviceCollection.AddSingleton<IRelationalTypeMappingSource, SqlServerBytesTypeMappingSource>());
    }

    public class SqlServerBytesTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly SqlServerByteArrayTypeMapping _rowversion = new("rowversion", size: 8);
        private readonly SqlServerByteArrayTypeMapping _variableLengthBinary = new();
        private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary = new(fixedLength: true);
        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        public SqlServerBytesTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "binary varying", _variableLengthBinary },
                    { "binary", _fixedLengthBinary },
                    { "image", _variableLengthBinary },
                    { "rowversion", _rowversion },
                    { "varbinary", _variableLengthBinary }
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

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return _rowversion;
                }

                var isFixedLength = mappingInfo.IsFixedLength == true;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                if (size > 8000)
                {
                    size = isFixedLength ? 8000 : null;
                }

                return new SqlServerByteArrayTypeMapping(
                    "varbinary(" + (size == null ? "max" : size.ToString()) + ")",
                    size,
                    isFixedLength,
                    storeTypePostfix: size == null ? StoreTypePostfix.None : null);
            }

            return null;
        }
    }
}
