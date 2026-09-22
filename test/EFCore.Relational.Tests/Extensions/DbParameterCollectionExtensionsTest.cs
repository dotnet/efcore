// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public class DbParameterCollectionExtensionsTest
{
    [ConditionalFact]
    public void Formats_string_parameter()
        => Assert.Equal(
            "@param='Muffin'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.String, true, 0, 0, 0));

    [ConditionalFact]
    public void Format_parameter_with_direction()
        => Assert.Equal(
            "@param='Muffin' (Direction = Output)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Output, DbType.String, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_non_nullable_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (Nullable = false)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.String, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_non_unicode_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (DbType = AnsiString)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.AnsiString, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_non_unicode_non_nullable_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (Nullable = false) (DbType = AnsiString)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.AnsiString, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_non_unicode_non_nullable_sized_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (Nullable = false) (Size = 100) (DbType = AnsiString)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.AnsiString, false, 100, 0, 0));

    [ConditionalFact]
    public void Formats_null_string_parameter()
        => Assert.Equal(
            "@param=NULL",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", null, true, ParameterDirection.Input, DbType.String, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_null_non_unicode_string_parameter()
        => Assert.Equal(
            "@param=NULL (DbType = AnsiString)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", null, true, ParameterDirection.Input, DbType.AnsiString, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_fixed_length_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (DbType = StringFixedLength)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.StringFixedLength, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_fixed_length_non_nullable_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (Nullable = false) (DbType = StringFixedLength)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.StringFixedLength, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_fixed_length_non_nullable_sized_string_parameter()
        => Assert.Equal(
            "@param='Muffin' (Nullable = false) (Size = 100) (DbType = StringFixedLength)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "Muffin", true, ParameterDirection.Input, DbType.StringFixedLength, false, 100, 0, 0));

    [ConditionalFact]
    public void Formats_null_fixed_length_string_parameter()
        => Assert.Equal(
            "@param=NULL (DbType = StringFixedLength)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", null, true, ParameterDirection.Input, DbType.StringFixedLength, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_sensitive_string_parameter()
        => Assert.Equal(
            "@param='?'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "?", false, ParameterDirection.Input, DbType.String, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_sensitive_non_nullable_string_parameter()
        => Assert.Equal(
            "@param='?'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "?", false, ParameterDirection.Input, DbType.String, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_int_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", 777, true, ParameterDirection.Input, DbType.Int32, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_nullable_int_parameter()
        => Assert.Equal(
            "@param='777' (Nullable = true)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", 777, true, ParameterDirection.Input, DbType.Int32, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_int_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", 777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_int_parameter_with_no_type()
        => Assert.Equal(
            "@param='777' (DbType = AnsiString)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", 777, true, ParameterDirection.Input, 0, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_null_nullable_int_parameter()
        => Assert.Equal(
            "@param=NULL (DbType = Int32)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", null, true, ParameterDirection.Input, DbType.Int32, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_sensitive_int_parameter()
        => Assert.Equal(
            "@param='?' (DbType = Int32)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "?", false, ParameterDirection.Input, DbType.Int32, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_sensitive_nullable_int_parameter()
        => Assert.Equal(
            "@param='?' (DbType = Int32)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", "?", false, ParameterDirection.Input, DbType.Int32, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_short_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (short)777, true, ParameterDirection.Input, DbType.Int16, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_short_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (short)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_long_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (long)777, true, ParameterDirection.Input, DbType.Int64, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_long_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (long)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_byte_parameter()
        => Assert.Equal(
            "@param='77'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (byte)77, true, ParameterDirection.Input, DbType.Byte, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_byte_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='77' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (byte)77, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_uint_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (uint)777, true, ParameterDirection.Input, DbType.UInt32, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_uint_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (uint)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_ushort_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (ushort)777, true, ParameterDirection.Input, DbType.UInt16, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_ushort_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (ushort)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_ulong_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (ulong)777, true, ParameterDirection.Input, DbType.UInt64, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_ulong_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (ulong)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_sbyte_parameter()
        => Assert.Equal(
            "@param='77'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (sbyte)77, true, ParameterDirection.Input, DbType.SByte, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_sbyte_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='77' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (sbyte)77, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_binary_parameter()
        => Assert.Equal(
            "@param='0x0102'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new byte[] { 1, 2 }, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_binary_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='0x0102' (DbType = Object)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new byte[] { 1, 2 }, true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_bool_parameter()
        => Assert.Equal(
            "@param='True'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", true, true, ParameterDirection.Input, DbType.Boolean, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_bool_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='True' (DbType = Int32)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", true, true, ParameterDirection.Input, DbType.Int32, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_decimal_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (decimal)777, true, ParameterDirection.Input, DbType.Decimal, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_decimal_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (decimal)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_decimal_parameter_with_precision()
        => Assert.Equal(
            "@param='77.7' (Precision = 18)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (decimal)77.7, true, ParameterDirection.Input, DbType.Decimal, false, 0, 18, 0));

    [ConditionalFact]
    public void Formats_decimal_parameter_with_scale()
        => Assert.Equal(
            "@param='77.7' (Scale = 2)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (decimal)77.7, true, ParameterDirection.Input, DbType.Decimal, false, 0, 0, 2));

    [ConditionalFact]
    public void Formats_decimal_parameter_with_precision_and_scale()
        => Assert.Equal(
            "@param='77.7' (Precision = 18) (Scale = 2)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (decimal)77.7, true, ParameterDirection.Input, DbType.Decimal, false, 0, 18, 2));

    [ConditionalFact]
    public void Formats_double_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (double)777, true, ParameterDirection.Input, DbType.Double, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_double_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (double)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_float_parameter()
        => Assert.Equal(
            "@param='777'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (float)777, true, ParameterDirection.Input, DbType.Single, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_float_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='777' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", (float)777, true, ParameterDirection.Input, DbType.VarNumeric, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_Guid_parameter()
        => Assert.Equal(
            "@param='304afb2a-8b8c-49ac-996e-8561f7559a3f'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", Guid.Parse("304afb2a-8b8c-49ac-996e-8561f7559a3f"),
                true, ParameterDirection.Input, DbType.Guid, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_Guid_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='304afb2a-8b8c-49ac-996e-8561f7559a3f' (DbType = Binary)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", Guid.Parse("304afb2a-8b8c-49ac-996e-8561f7559a3f"),
                true, ParameterDirection.Input, DbType.Binary, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_object_parameter()
        => Assert.Equal(
            "@param='System.Object'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new object(), true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_object_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='System.Object' (DbType = VarNumeric)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new object(), true, ParameterDirection.Input, DbType.VarNumeric, true, 0, 0, 0));

    [ConditionalFact]
    public void Formats_DateTime_parameter()
        => Assert.Equal(
            "@param='1973-09-03T00:00:00.0000000'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new DateTime(1973, 9, 3), true, ParameterDirection.Input, DbType.DateTime2, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_DateTime_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='1973-09-03T00:00:00.0000000' (DbType = DateTime)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new DateTime(1973, 9, 3), true, ParameterDirection.Input, DbType.DateTime, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_DateTimeOffset_parameter()
        => Assert.Equal(
            "@param='1973-09-03T00:00:00.0000000-08:00'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new DateTimeOffset(new DateTime(1973, 9, 3), new TimeSpan(-8, 0, 0)),
                true, ParameterDirection.Input, DbType.DateTimeOffset, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_DateTimeOffset_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='1973-09-03T00:00:00.0000000-08:00' (DbType = Date)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new DateTimeOffset(new DateTime(1973, 9, 3), new TimeSpan(-8, 0, 0)),
                true, ParameterDirection.Input, DbType.Date, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_TimeSpan_parameter()
        => Assert.Equal(
            "@param='-08:00:00'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new TimeSpan(-8, 0, 0), true, ParameterDirection.Input, DbType.Time, false, 0, 0, 0));

    [ConditionalFact]
    public void Formats_TimeSpan_parameter_with_unusual_type()
        => Assert.Equal(
            "@param='-08:00:00' (DbType = DateTime)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", new TimeSpan(-8, 0, 0), true, ParameterDirection.Input, DbType.DateTime, false, 0, 0, 0));

    [ConditionalFact]
    public void Short_byte_arrays_are_not_truncated()
    {
        var shortArray = new Guid("21EC2020-3AEA-4069-A2DD-08002B30309D").ToByteArray();
        var longerShortArray = shortArray.Concat(shortArray).ToArray();

        Assert.Equal(
            "@param='0x2020EC21EA3A6940A2DD08002B30309D'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", shortArray, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0));

        Assert.Equal(
            "@param='0x2020EC21EA3A6940A2DD08002B30309D2020EC21EA3A6940A2DD08002B30309D'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", longerShortArray, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0));
    }

    [ConditionalFact]
    public void Long_byte_arrays_are_truncated()
    {
        var shortArray = new Guid("21EC2020-3AEA-4069-A2DD-08002B30309D").ToByteArray();
        var longArray = shortArray.Concat(shortArray).Concat(shortArray).ToArray();

        Assert.Equal(
            "@param='0x2020EC21EA3A6940A2DD08002B30309D2020EC21EA3A6940A2DD08002B30309D...'",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", longArray, true, ParameterDirection.Input, DbType.Binary, true, 0, 0, 0));
    }

    [ConditionalFact]
    public void Short_arrays_are_not_truncated()
    {
        var array = new[] { 1, 2, 3, 4, 5 };

        Assert.Equal(
            "@param={ '1', '2', '3', '4', '5' } (DbType = Object)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", array, true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0));
    }

    [ConditionalFact]
    public void Long_arrays_are_truncated()
    {
        var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        Assert.Equal(
            "@param={ '1', '2', '3', '4', '5', ... } (DbType = Object)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", array, true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0));
    }

    [ConditionalFact]
    public void Short_generic_lists_are_not_truncated()
    {
        var array = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };

        Assert.Equal(
            "@param={ '1', '2', '3', '4', '5' } (DbType = Object)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", array, true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0));
    }

    [ConditionalFact]
    public void Long_generic_lists_are_truncated()
    {
        var array = new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10
        };

        Assert.Equal(
            "@param={ '1', '2', '3', '4', '5', ... } (DbType = Object)",
            DbParameterCollectionExtensions.FormatParameter(
                "@param", array, true, ParameterDirection.Input, DbType.Object, true, 0, 0, 0));
    }
}
