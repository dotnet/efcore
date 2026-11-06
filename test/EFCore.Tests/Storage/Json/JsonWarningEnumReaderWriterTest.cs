// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.Json;

public class JsonWarningEnumReaderWriterTest
{
    [Fact]
    public void Can_read_signed_enum_from_name_or_numeric_string()
    {
        var readerWriter = JsonWarningEnumReaderWriter<SignedEnum>.Instance;

        Assert.Equal(SignedEnum.Two, readerWriter.FromJsonString("\"Two\""));
        Assert.Equal(SignedEnum.Two, readerWriter.FromJsonString("\"2\""));
        Assert.Equal((SignedEnum)(-1), readerWriter.FromJsonString("\"-1\""));
        Assert.Equal(SignedEnum.Two, readerWriter.FromJsonString("2"));
    }

    [Fact]
    public void Can_read_unsigned_enum_from_name_or_numeric_string()
    {
        var readerWriter = JsonWarningEnumReaderWriter<UnsignedEnum>.Instance;

        Assert.Equal(UnsignedEnum.Two, readerWriter.FromJsonString("\"Two\""));
        Assert.Equal(UnsignedEnum.Two, readerWriter.FromJsonString("\"2\""));
        Assert.Equal((UnsignedEnum)uint.MaxValue, readerWriter.FromJsonString("\"4294967295\""));
        Assert.Equal(UnsignedEnum.Two, readerWriter.FromJsonString("2"));
    }

    [Fact]
    public void Throws_for_invalid_string_value_of_signed_enum()
        => Assert.Equal(
            CoreStrings.BadEnumValue("Three", typeof(SignedEnum).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => JsonWarningEnumReaderWriter<SignedEnum>.Instance.FromJsonString("\"Three\"")).Message);

    [Fact]
    public void Throws_for_invalid_string_value_of_unsigned_enum()
        => Assert.Equal(
            CoreStrings.BadEnumValue("Three", typeof(UnsignedEnum).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => JsonWarningEnumReaderWriter<UnsignedEnum>.Instance.FromJsonString("\"Three\"")).Message);

    [Fact]
    public void Throws_for_negative_string_value_of_unsigned_enum()
        => Assert.Equal(
            CoreStrings.BadEnumValue("-1", typeof(UnsignedEnum).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => JsonWarningEnumReaderWriter<UnsignedEnum>.Instance.FromJsonString("\"-1\"")).Message);

    private enum SignedEnum
    {
        One = 1,
        Two = 2
    }

    private enum UnsignedEnum : uint
    {
        One = 1,
        Two = 2
    }
}
