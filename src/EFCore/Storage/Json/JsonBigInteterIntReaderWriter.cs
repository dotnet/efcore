// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="BigInteger" /> values.
/// </summary>
public sealed class JsonBigIntegerReaderWriter : JsonValueReaderWriter<BigInteger>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonBigIntegerReaderWriter Instance { get; } = new();

    private JsonBigIntegerReaderWriter()
    {
    }

    /// <inheritdoc />
    public override BigInteger FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        if (!BigInteger.TryParse(manager.CurrentReader.GetString(), out BigInteger result))
        {
            throw new FormatException("BigInteger");
        }
        return result;
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, BigInteger value)
        => writer.WriteStringValue(value.ToString("R", CultureInfo.InvariantCulture));
}
