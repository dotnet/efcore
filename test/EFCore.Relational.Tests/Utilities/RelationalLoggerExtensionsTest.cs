// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Utilities
{
    public class RelationalLoggerExtensionsTest
    {
        [Fact]
        public void Short_byte_arrays_are_not_truncated()
        {
            var shortArray = new Guid("21EC2020-3AEA-4069-A2DD-08002B30309D").ToByteArray();
            var longerShortArray = shortArray.Concat(shortArray).ToArray();

            Assert.Equal(
                "'0x2020EC21EA3A6940A2DD08002B30309D'",
                new DbParameterLogData(
                    "@param", shortArray, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0).FormatParameter());

            Assert.Equal(
                "'0x2020EC21EA3A6940A2DD08002B30309D2020EC21EA3A6940A2DD08002B30309D'",
                new DbParameterLogData(
                    "@param", longerShortArray, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Long_byte_arrays_are_truncated()
        {
            var shortArray = new Guid("21EC2020-3AEA-4069-A2DD-08002B30309D").ToByteArray();
            var longArray = shortArray.Concat(shortArray).Concat(shortArray).ToArray();

            Assert.Equal(
                "'0x2020EC21EA3A6940A2DD08002B30309D2020EC21EA3A6940A2DD08002B30309D...'",
                new DbParameterLogData(
                    "@param", longArray, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_string_parameter()
        {
            Assert.Equal(
                "'Muffin'",
                new DbParameterLogData(
                    "@param", "Muffin", true, ParameterDirection.Input, DbType.String, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Format_parameter_with_direction()
        {
            Assert.Equal(
                "'Muffin' (Direction = Output)",
                new DbParameterLogData(
                    "@param", "Muffin", true, ParameterDirection.Output, DbType.String, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_non_nullable_string_parameter()
        {
            Assert.Equal(
                "'Muffin' (Nullable = false)",
                new DbParameterLogData(
                    "@param", "Muffin", true, ParameterDirection.Input, DbType.String, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_non_unicode_string_parameter()
        {
            Assert.Equal(
                "'Muffin' (DbType = AnsiString)",
                new DbParameterLogData(
                    "@param", "Muffin", true, ParameterDirection.Input, DbType.AnsiString, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_non_unicode_non_nullable_string_parameter()
        {
            Assert.Equal(
                "'Muffin' (Nullable = false) (DbType = AnsiString)",
                new DbParameterLogData(
                    "@param", "Muffin", true, ParameterDirection.Input, DbType.AnsiString, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_non_unicode_non_nullable_sized_string_parameter()
        {
            Assert.Equal(
                "'Muffin' (Nullable = false) (Size = 100) (DbType = AnsiString)",
                new DbParameterLogData(
                    "@param", "Muffin", true, ParameterDirection.Input, DbType.AnsiString, false, 100, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_null_string_parameter()
        {
            Assert.Equal(
                "'' (DbType = String)",
                new DbParameterLogData(
                    "@param", null, true, ParameterDirection.Input, DbType.String, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_null_non_unicode_string_parameter()
        {
            Assert.Equal(
                "'' (DbType = AnsiString)",
                new DbParameterLogData(
                    "@param", null, true, ParameterDirection.Input, DbType.AnsiString, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_sensitive_string_parameter()
        {
            Assert.Equal(
                "'?'",
                new DbParameterLogData(
                    "@param", "?", false, ParameterDirection.Input, DbType.String, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_sensitive_non_nullable_string_parameter()
        {
            Assert.Equal(
                "'?'",
                new DbParameterLogData(
                    "@param", "?", false, ParameterDirection.Input, DbType.String, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_int_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", 777, true, ParameterDirection.Input, DbType.Int32, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_nullable_int_parameter()
        {
            Assert.Equal(
                "'777' (Nullable = true)",
                new DbParameterLogData(
                    "@param", 777, true, ParameterDirection.Input, DbType.Int32, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_int_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", 777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_int_parameter_with_no_type()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", 777, true, ParameterDirection.Input, 0, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_null_nullable_int_parameter()
        {
            Assert.Equal(
                "'' (DbType = Int32)",
                new DbParameterLogData(
                    "@param", null, true, ParameterDirection.Input, DbType.Int32, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_sensitive_int_parameter()
        {
            Assert.Equal(
                "'?'",
                new DbParameterLogData(
                    "@param", "?", false, ParameterDirection.Input, DbType.Int32, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_sensitive_nullable_int_parameter()
        {
            Assert.Equal(
                "'?'",
                new DbParameterLogData(
                    "@param", "?", false, ParameterDirection.Input, DbType.Int32, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_short_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (short)777, true, ParameterDirection.Input, DbType.Int16, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_short_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (short)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_long_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (long)777, true, ParameterDirection.Input, DbType.Int64, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_long_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (long)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_byte_parameter()
        {
            Assert.Equal(
                "'77'",
                new DbParameterLogData(
                    "@param", (byte)77, true, ParameterDirection.Input, DbType.Byte, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_byte_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'77' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (byte)77, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_uint_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (uint)777, true, ParameterDirection.Input, DbType.UInt32, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_uint_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (uint)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_ushort_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (ushort)777, true, ParameterDirection.Input, DbType.UInt16, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_ushort_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (ushort)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_ulong_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (ulong)777, true, ParameterDirection.Input, DbType.UInt64, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_ulong_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (ulong)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_sbyte_parameter()
        {
            Assert.Equal(
                "'77'",
                new DbParameterLogData(
                    "@param", (sbyte)77, true, ParameterDirection.Input, DbType.SByte, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_sbyte_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'77' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (sbyte)77, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_binary_parameter()
        {
            Assert.Equal(
                "'0x0102'",
                new DbParameterLogData(
                    "@param", new byte[] { 1, 2 }, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_binary_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'0x0102' (DbType = Object)",
                new DbParameterLogData(
                    "@param", new byte[] { 1, 2 }, true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_bool_parameter()
        {
            Assert.Equal(
                "'True'",
                new DbParameterLogData(
                    "@param", true, true, ParameterDirection.Input, DbType.Boolean, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_bool_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'True' (DbType = Int32)",
                new DbParameterLogData(
                    "@param", true, true, ParameterDirection.Input, DbType.Int32, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_decimal_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (decimal)777, true, ParameterDirection.Input, DbType.Decimal, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_decimal_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (decimal)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_decimal_parameter_with_precision()
        {
            Assert.Equal(
                "'77.7' (Precision = 18)",
                new DbParameterLogData(
                    "@param", (decimal)77.7, true, ParameterDirection.Input, DbType.Decimal, false, 0, 18, 0).FormatParameter());
        }

        [Fact]
        public void Formats_decimal_parameter_with_scale()
        {
            Assert.Equal(
                "'77.7' (Scale = 2)",
                new DbParameterLogData(
                    "@param", (decimal)77.7, true, ParameterDirection.Input, DbType.Decimal, false, 0, 0, 2).FormatParameter());
        }

        [Fact]
        public void Formats_decimal_parameter_with_precision_and_scale()
        {
            Assert.Equal(
                "'77.7' (Precision = 18) (Scale = 2)",
                new DbParameterLogData(
                    "@param", (decimal)77.7, true, ParameterDirection.Input, DbType.Decimal, false, 0, 18, 2).FormatParameter());
        }

        [Fact]
        public void Formats_double_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (double)777, true, ParameterDirection.Input, DbType.Double, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_double_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (double)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_float_parameter()
        {
            Assert.Equal(
                "'777'",
                new DbParameterLogData(
                    "@param", (float)777, true, ParameterDirection.Input, DbType.Single, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_float_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'777' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", (float)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_Guid_parameter()
        {
            Assert.Equal(
                "'304afb2a-8b8c-49ac-996e-8561f7559a3f'",
                new DbParameterLogData(
                    "@param", Guid.Parse("304afb2a-8b8c-49ac-996e-8561f7559a3f"),
                    true, ParameterDirection.Input, DbType.Guid, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_Guid_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'304afb2a-8b8c-49ac-996e-8561f7559a3f' (DbType = Binary)",
                new DbParameterLogData(
                    "@param", Guid.Parse("304afb2a-8b8c-49ac-996e-8561f7559a3f"),
                    true, ParameterDirection.Input, DbType.Binary, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_object_parameter()
        {
            Assert.Equal(
                "'System.Object'",
                new DbParameterLogData(
                    "@param", new object(), true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_object_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'System.Object' (DbType = VarNumeric)",
                new DbParameterLogData(
                    "@param", new object(), true, ParameterDirection.Input, DbType.VarNumeric, true, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_DateTime_parameter()
        {
            Assert.Equal(
                "'09/03/1973 00:00:00'",
                new DbParameterLogData(
                    "@param", new DateTime(1973, 9, 3), true, ParameterDirection.Input, DbType.DateTime2, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_DateTime_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'09/03/1973 00:00:00' (DbType = DateTime)",
                new DbParameterLogData(
                    "@param", new DateTime(1973, 9, 3), true, ParameterDirection.Input, DbType.DateTime, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_DateTimeOffset_parameter()
        {
            Assert.Equal(
                "'09/03/1973 00:00:00 -08:00'",
                new DbParameterLogData(
                    "@param", new DateTimeOffset(new DateTime(1973, 9, 3), new TimeSpan(-8, 0, 0)),
                    true, ParameterDirection.Input, DbType.DateTimeOffset, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_DateTimeOffset_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'09/03/1973 00:00:00 -08:00' (DbType = Date)",
                new DbParameterLogData(
                    "@param", new DateTimeOffset(new DateTime(1973, 9, 3), new TimeSpan(-8, 0, 0)),
                    true, ParameterDirection.Input, DbType.Date, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_TimeSpan_parameter()
        {
            Assert.Equal(
                "'-08:00:00'",
                new DbParameterLogData(
                    "@param", new TimeSpan(-8, 0, 0), true, ParameterDirection.Input, DbType.Time, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void Formats_TimeSpan_parameter_with_unusual_type()
        {
            Assert.Equal(
                "'-08:00:00' (DbType = DateTime)",
                new DbParameterLogData(
                    "@param", new TimeSpan(-8, 0, 0), true, ParameterDirection.Input, DbType.DateTime, false, 0, 0, 0).FormatParameter());
        }

        [Fact]
        public void DbCommandLogData_implements_enumerable_of_key_value_pair()
        {
            var parameter = new DbParameterLogData(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.String, false, 0, 0, 0);

#pragma warning disable 618
            var logData = new DbCommandLogData(
#pragma warning restore 618
                "FakeCommand",
                CommandType.Text,
                30,
                new List<DbParameterLogData> { parameter },
                1000);

            var keyValuePairs = logData.ToList();

            Assert.Equal("FakeCommand", keyValuePairs.Single(kvp => kvp.Key == "CommandText").Value);
            Assert.Equal(CommandType.Text, keyValuePairs.Single(kvp => kvp.Key == "CommandType").Value);
            Assert.Equal(30, keyValuePairs.Single(kvp => kvp.Key == "CommandTimeout").Value);
            Assert.Equal(parameter, ((List<DbParameterLogData>)keyValuePairs.Single(kvp => kvp.Key == "Parameters").Value).Single());
            Assert.Equal(1000L, keyValuePairs.Single(kvp => kvp.Key == "ElapsedMilliseconds").Value);
        }
    }
}
