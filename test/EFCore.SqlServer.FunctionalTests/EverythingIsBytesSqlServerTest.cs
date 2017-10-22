// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
            const string query
                = @"SELECT
                        TABLE_NAME,
                        COLUMN_NAME,
                        DATA_TYPE,
                        IS_NULLABLE,
                        CHARACTER_MAXIMUM_LENGTH,
                        NUMERIC_PRECISION,
                        NUMERIC_SCALE,
                        DATETIME_PRECISION
                    FROM INFORMATION_SCHEMA.COLUMNS";

            var columns = new List<BuiltInDataTypesSqlServerTest.ColumnInfo>();

            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                var command = connection.CreateCommand();
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnInfo = new BuiltInDataTypesSqlServerTest.ColumnInfo
                        {
                            TableName = reader.GetString(0),
                            ColumnName = reader.GetString(1),
                            DataType = reader.GetString(2),
                            IsNullable = reader.IsDBNull(3) ? null : (bool?)(reader.GetString(3) == "YES"),
                            MaxLength = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                            NumericPrecision = reader.IsDBNull(5) ? null : (int?)reader.GetByte(5),
                            NumericScale = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            DateTimePrecision = reader.IsDBNull(7) ? null : (int?)reader.GetInt16(7)
                        };

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
