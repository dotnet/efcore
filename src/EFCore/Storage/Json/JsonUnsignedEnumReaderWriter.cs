// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="enum" /> values backed by an unsigned integer.
/// </summary>
public sealed class JsonUnsignedEnumReaderWriter<TEnum> : JsonValueReaderWriter<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonUnsignedEnumReaderWriter<TEnum> Instance { get; } = new();

    private JsonUnsignedEnumReaderWriter()
    {
    }

    /// <inheritdoc />
    public override TEnum FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => (TEnum)Convert.ChangeType(manager.CurrentReader.GetUInt64(), typeof(TEnum).GetEnumUnderlyingType());

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TEnum value)
        => writer.WriteNumberValue((ulong)Convert.ChangeType(value, typeof(ulong))!);
}
