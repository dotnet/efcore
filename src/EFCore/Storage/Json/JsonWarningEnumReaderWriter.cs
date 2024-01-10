// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="enum" /> values where string values may be read instead of numeric, and, when this
///     happens, a warning is generated.
/// </summary>
public sealed class JsonWarningEnumReaderWriter<TEnum> : JsonValueReaderWriter<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonWarningEnumReaderWriter<TEnum> Instance { get; } = new();

    private readonly bool _isSigned;

    private JsonWarningEnumReaderWriter()
    {
        _isSigned = typeof(TEnum).GetEnumUnderlyingType().IsSignedInteger();
    }

    /// <inheritdoc />
    public override TEnum FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        if (manager.CurrentReader.TokenType == JsonTokenType.String)
        {
            if (manager.QueryLogger?.Options.ShouldWarnForStringEnumValueInJson(typeof(TEnum)) == true)
            {
                manager.QueryLogger.StringEnumValueInJson(typeof(TEnum));
            }

            var value = manager.CurrentReader.GetString();
            if (Enum.TryParse<TEnum>(value, out var result))
            {
                return result;
            }

            if (_isSigned && long.TryParse(value, out var longValue))
            {
                return (TEnum)Convert.ChangeType(longValue, typeof(TEnum).GetEnumUnderlyingType());
            }

            if (!_isSigned && !ulong.TryParse(value, out var ulongValue))
            {
                return (TEnum)Convert.ChangeType(ulongValue, typeof(TEnum).GetEnumUnderlyingType());
            }

            throw new InvalidOperationException(CoreStrings.BadEnumValue(value, typeof(TEnum).ShortDisplayName()));
        }

        return (TEnum)Convert.ChangeType(
            _isSigned
                ? manager.CurrentReader.GetInt64()
                : manager.CurrentReader.GetUInt64(),
            typeof(TEnum).GetEnumUnderlyingType());
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TEnum value)
    {
        if (_isSigned)
        {
            writer.WriteNumberValue((long)Convert.ChangeType(value, typeof(long)));
        }
        else
        {
            writer.WriteNumberValue((ulong)Convert.ChangeType(value, typeof(ulong)));
        }
    }
}
