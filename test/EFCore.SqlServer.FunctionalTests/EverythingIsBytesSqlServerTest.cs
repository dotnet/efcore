// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public class EverythingIsBytesSqlServerTest : BuiltInDataTypesTestBase<EverythingIsBytesSqlServerTest.EverythingIsBytesSqlServerFixture>
    {
        public EverythingIsBytesSqlServerTest(EverythingIsBytesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Columns_have_expected_data_types()
        {
            var actual = BuiltInDataTypesSqlServerTest.QueryForColumnTypes(CreateContext());

            const string expected = @"BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [varbinary] [MaxLength = 4]
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
BuiltInDataTypes.TestDateTime ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestDateTimeOffset ---> [varbinary] [MaxLength = 12]
BuiltInDataTypes.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypes.TestDouble ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestInt16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypes.TestInt32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypes.TestInt64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypes.TestSignedByte ---> [varbinary] [MaxLength = 1]
BuiltInDataTypes.TestSingle ---> [varbinary] [MaxLength = 4]
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
BuiltInDataTypesShadow.TestDateTime ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [varbinary] [MaxLength = 12]
BuiltInDataTypesShadow.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypesShadow.TestDouble ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestInt16 ---> [varbinary] [MaxLength = 2]
BuiltInDataTypesShadow.TestInt32 ---> [varbinary] [MaxLength = 4]
BuiltInDataTypesShadow.TestInt64 ---> [varbinary] [MaxLength = 8]
BuiltInDataTypesShadow.TestSignedByte ---> [varbinary] [MaxLength = 1]
BuiltInDataTypesShadow.TestSingle ---> [varbinary] [MaxLength = 4]
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
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable varbinary] [MaxLength = 12]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable varbinary] [MaxLength = 4]
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
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable varbinary] [MaxLength = 12]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable varbinary] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable varbinary] [MaxLength = 2]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable varbinary] [MaxLength = 4]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable varbinary] [MaxLength = 8]
BuiltInNullableDataTypesShadow.TestString ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [varbinary] [MaxLength = 4]
MaxLengthDataTypes.String3 ---> [nullable varbinary] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable varbinary] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [varbinary] [MaxLength = 4]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
StringKeyDataType.Id ---> [varbinary] [MaxLength = 900]
UnicodeDataTypes.Id ---> [varbinary] [MaxLength = 4]
UnicodeDataTypes.StringAnsi ---> [nullable varbinary] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varbinary] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varbinary] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable varbinary] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable varbinary] [MaxLength = -1]
";

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
        }

        public class EverythingIsBytesSqlServerFixture : BuiltInDataTypesFixtureBase
        {
            protected override string StoreName { get; } = "EverythingIsBytes";

            protected override ITestStoreFactory TestStoreFactory => SqlServerBytesTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base
                    .AddOptions(builder)
                    .ConfigureWarnings(
                        c => c.Log(RelationalEventId.QueryClientEvaluationWarning)
                            .Log(SqlServerEventId.DecimalTypeDefaultWarning));
        }

        public class SqlServerBytesTestStoreFactory : SqlServerTestStoreFactory
        {
            public new static SqlServerBytesTestStoreFactory Instance { get; } = new SqlServerBytesTestStoreFactory();

            public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
                => base.AddProviderServices(
                    serviceCollection.AddSingleton<IRelationalTypeMapper, SqlServerBytesTypeMapper>());
        }

        public class SqlServerBytesTypeMapper : RelationalTypeMapper
        {
            private readonly SqlServerByteArrayTypeMapping _unboundedBinary
                = new SqlServerByteArrayTypeMapping("varbinary(max)");

            private readonly SqlServerByteArrayTypeMapping _keyBinary
                = new SqlServerByteArrayTypeMapping("varbinary(900)", dbType: DbType.Binary, size: 900);

            private readonly SqlServerByteArrayTypeMapping _rowversion
                = new SqlServerByteArrayTypeMapping("rowversion", dbType: DbType.Binary, size: 8);

            private readonly SqlServerByteArrayTypeMapping _variableLengthBinary
                = new SqlServerByteArrayTypeMapping("varbinary");

            private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary
                = new SqlServerByteArrayTypeMapping("binary");

            private readonly Dictionary<string, IList<RelationalTypeMapping>> _storeTypeMappings;
            private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

            public SqlServerBytesTypeMapper(
                CoreTypeMapperDependencies coreDependencies,
                RelationalTypeMapperDependencies relationalDependencies)
                : base(coreDependencies, relationalDependencies)
            {
                _storeTypeMappings
                    = new Dictionary<string, IList<RelationalTypeMapping>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "binary varying", new List<RelationalTypeMapping> { _variableLengthBinary } },
                        { "binary", new List<RelationalTypeMapping> { _fixedLengthBinary } },
                        { "image", new List<RelationalTypeMapping> { _variableLengthBinary } },
                        { "rowversion", new List<RelationalTypeMapping> { _rowversion } },
                        { "timestamp", new List<RelationalTypeMapping> { _rowversion } },
                        { "varbinary", new List<RelationalTypeMapping> { _variableLengthBinary } }
                    };

                _clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>();

                ByteArrayMapper
                    = new ByteArrayRelationalTypeMapper(
                        maxBoundedLength: 8000,
                        defaultMapping: _unboundedBinary,
                        unboundedMapping: _unboundedBinary,
                        keyMapping: _keyBinary,
                        rowVersionMapping: _rowversion,
                        createBoundedMapping: size => new SqlServerByteArrayTypeMapping(
                            "varbinary(" + size + ")",
                            DbType.Binary,
                            size));
            }

            public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
                => _clrTypeMappings;

            protected override IReadOnlyDictionary<string, IList<RelationalTypeMapping>> GetMultipleStoreTypeMappings()
                => _storeTypeMappings;

            public override RelationalTypeMapping FindMapping(Type clrType)
                => clrType == typeof(byte[])
                    ? _unboundedBinary
                    : base.FindMapping(clrType);

            protected override bool RequiresKeyMapping(IProperty property)
                => base.RequiresKeyMapping(property) || property.IsIndex();
        }
    }
}
