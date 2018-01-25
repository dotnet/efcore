// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !Test20
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public class CustomConvertersSqlServerTest : CustomConvertersTestBase<CustomConvertersSqlServerTest.CustomConvertersSqlServerFixture>
    {
        public CustomConvertersSqlServerTest(CustomConvertersSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Columns_have_expected_data_types()
        {
            var actual = BuiltInDataTypesSqlServerTest.QueryForColumnTypes(CreateContext());

            const string expected = @"BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
BinaryKeyDataType.Id ---> [varbinary] [MaxLength = 900]
BuiltInDataTypes.Enum16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.EnumS8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.EnumU16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.EnumU32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.EnumU64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.TestByte ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestCharacter ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestDateTime ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDateTimeOffset ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypes.TestDouble ---> [decimal] [Precision = 26 Scale = 16]
BuiltInDataTypes.TestInt16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSignedByte ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypes.TestSingle ---> [float] [Precision = 53]
BuiltInDataTypes.TestTimeSpan ---> [float] [Precision = 53]
BuiltInDataTypes.TestUnsignedInt16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.TestUnsignedInt32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.TestUnsignedInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumS8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumU16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.EnumU32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.EnumU64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestBoolean ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.TestByte ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestCharacter ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestDateTime ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypesShadow.TestDouble ---> [decimal] [Precision = 26 Scale = 16]
BuiltInDataTypesShadow.TestInt16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestSignedByte ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypesShadow.TestSingle ---> [float] [Precision = 53]
BuiltInDataTypesShadow.TestTimeSpan ---> [float] [Precision = 53]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumS8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumU16 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.EnumU32 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.EnumU64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable decimal] [Precision = 26 Scale = 16]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
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
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable datetime2] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable real] [Precision = 24]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestString ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 7]
MaxLengthDataTypes.ByteArray9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 12]
MaxLengthDataTypes.String9000 ---> [nullable varbinary] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
UnicodeDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]
User.Email ---> [nullable nvarchar] [MaxLength = -1]
User.Id ---> [uniqueidentifier]
";

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
        }

        public class CustomConvertersSqlServerFixture : CustomConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base
                    .AddOptions(builder)
                    .ConfigureWarnings(
                        c => c.Log(RelationalEventId.QueryClientEvaluationWarning)
                            .Log(SqlServerEventId.DecimalTypeDefaultWarning));
        }
    }
}
#endif
