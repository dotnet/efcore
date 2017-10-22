// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class EverythingIsStringsSqlServerTest : BuiltInDataTypesTestBase<EverythingIsStringsSqlServerTest.EverythingIsStringsSqlServerFixture>
    {
        public EverythingIsStringsSqlServerTest(EverythingIsStringsSqlServerFixture fixture)
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

            const string expected = @"BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
BinaryForeignKeyDataType.Id ---> [nvarchar] [MaxLength = 64]
BinaryKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
BuiltInDataTypes.Enum16 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.Enum32 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.Enum64 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.Enum8 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.EnumS8 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.EnumU16 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.EnumU32 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.EnumU64 ---> [nvarchar] [MaxLength = 512]
BuiltInDataTypes.Id ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.PartitionId ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestBoolean ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypes.TestByte ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestCharacter ---> [nvarchar] [MaxLength = 1]
BuiltInDataTypes.TestDateTime ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestDateTimeOffset ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestDecimal ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestDouble ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestInt16 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestInt32 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestInt64 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestSignedByte ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestSingle ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestTimeSpan ---> [nvarchar] [MaxLength = 48]
BuiltInDataTypes.TestUnsignedInt16 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestUnsignedInt32 ---> [nvarchar] [MaxLength = 64]
BuiltInDataTypes.TestUnsignedInt64 ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.Enum16 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.Enum32 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.Enum64 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.Enum8 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.EnumS8 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.EnumU16 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.EnumU32 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.EnumU64 ---> [nullable nvarchar] [MaxLength = 512]
BuiltInNullableDataTypes.Id ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.PartitionId ---> [nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestByteArray ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable nvarchar] [MaxLength = 48]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable nvarchar] [MaxLength = 64]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.ByteArray5 ---> [nullable nvarchar] [MaxLength = 8]
MaxLengthDataTypes.ByteArray9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [nvarchar] [MaxLength = 64]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [nvarchar] [MaxLength = 64]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
UnicodeDataTypes.Id ---> [nvarchar] [MaxLength = 64]
UnicodeDataTypes.StringAnsi ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable nvarchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]
";

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
        }

        public class EverythingIsStringsSqlServerFixture : BuiltInDataTypesFixtureBase
        {
            protected override string StoreName { get; } = "EverythingIsStrings";

            protected override ITestStoreFactory TestStoreFactory => SqlServerStringsTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base
                    .AddOptions(builder)
                    .ConfigureWarnings(
                        c => c.Log(RelationalEventId.QueryClientEvaluationWarning)
                            .Log(SqlServerEventId.DecimalTypeDefaultWarning));

            //protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            //{
            //    base.OnModelCreating(modelBuilder, context);

            //    modelBuilder.Entity<MaxLengthDataTypes>(b =>
            //    {
            //        b.Property(e => e.ByteArray5).HasMaxLength(10); // Because Base64 encoding
            //    });
            //}
        }

        public class SqlServerStringsTestStoreFactory : SqlServerTestStoreFactory
        {
            public new static SqlServerStringsTestStoreFactory Instance { get; } = new SqlServerStringsTestStoreFactory();

            public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
                => base.AddProviderServices(
                    serviceCollection.AddSingleton<IRelationalTypeMapper, SqlServerStringsTypeMapper>());
        }

        public class SqlServerStringsTypeMapper : RelationalTypeMapper
        {
            private readonly SqlServerStringTypeMapping _variableLengthUnicodeString
                = new SqlServerStringTypeMapping("nvarchar", dbType: null, unicode: true);

            private readonly SqlServerStringTypeMapping _unboundedUnicodeString
                = new SqlServerStringTypeMapping("nvarchar(max)", dbType: null, unicode: true);

            private readonly SqlServerStringTypeMapping _keyUnicodeString
                = new SqlServerStringTypeMapping("nvarchar(450)", dbType: null, unicode: true, size: 450);

            private readonly Dictionary<string, IList<RelationalTypeMapping>> _storeTypeMappings;
            private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

            public SqlServerStringsTypeMapper(
                CoreTypeMapperDependencies coreDependencies,
                RelationalTypeMapperDependencies dependencies)
                : base(coreDependencies, dependencies)
            {
                _storeTypeMappings
                    = new Dictionary<string, IList<RelationalTypeMapping>>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            "national char varying", new List<RelationalTypeMapping>
                            {
                                _variableLengthUnicodeString
                            }
                        },
                        {
                            "national character varying", new List<RelationalTypeMapping>
                            {
                                _variableLengthUnicodeString
                            }
                        },
                        {
                            "nvarchar", new List<RelationalTypeMapping>
                            {
                                _variableLengthUnicodeString
                            }
                        }
                    };

                _clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>();

                StringMapper
                    = new StringRelationalTypeMapper(
                        maxBoundedAnsiLength: 4000,
                        defaultAnsiMapping: _unboundedUnicodeString,
                        unboundedAnsiMapping: _unboundedUnicodeString,
                        keyAnsiMapping: _keyUnicodeString,
                        createBoundedAnsiMapping: size => new SqlServerStringTypeMapping(
                            "nvarchar(" + size + ")",
                            dbType: null,
                            unicode: true,
                            size: size),
                        maxBoundedUnicodeLength: 4000,
                        defaultUnicodeMapping: _unboundedUnicodeString,
                        unboundedUnicodeMapping: _unboundedUnicodeString,
                        keyUnicodeMapping: _keyUnicodeString,
                        createBoundedUnicodeMapping: size => new SqlServerStringTypeMapping(
                            "nvarchar(" + size + ")",
                            dbType: null,
                            unicode: true,
                            size: size));
            }

            public override IStringRelationalTypeMapper StringMapper { get; }

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
                => _clrTypeMappings;

            protected override IReadOnlyDictionary<string, IList<RelationalTypeMapping>> GetMultipleStoreTypeMappings()
                => _storeTypeMappings;

            public override RelationalTypeMapping FindMapping(Type clrType)
                => clrType == typeof(string)
                    ? _unboundedUnicodeString
                    : base.FindMapping(clrType);

            protected override bool RequiresKeyMapping(IProperty property)
                => base.RequiresKeyMapping(property) || property.IsIndex();
        }
    }
}
