// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class CosmosJsonDecimalReaderWriter : JsonValueReaderWriter<decimal> // TODO: Cosmos does not support decimals: #38137
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonDecimalReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static CosmosJsonDecimalReaderWriter Instance { get; } = new();

    private CosmosJsonDecimalReaderWriter()
    {
    }

    /// <inheritdoc />
    public override decimal FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        // Mimics the behaviour of NewtonsoftJson's JToken.Parse().GetValue<decimal>() (old way of deserializing)
        var reader = manager.CurrentReader;
        var span = reader.ValueSpan;

        var isFloatToken = reader.HasValueSequence
            ? ContainsAny(reader.ValueSequence)
            : reader.ValueSpan.ContainsAny((byte)'.', (byte)'e', (byte)'E');

        static bool ContainsAny(ReadOnlySequence<byte> sequence)
        {
            foreach (var segment in sequence)
            {
                if (segment.Span.ContainsAny((byte)'.', (byte)'e', (byte)'E'))
                {
                    return true;
                }
            }
            return false;
        }

        return isFloatToken
            ? DoubleToDecimal(reader.GetDouble())
            : reader.GetDecimal();

        // Cosmos stores all numbers as IEEE-754 doubles, so a decimal is materialized from a double here. Historically this went
        // through Convert.ToDecimal(double), which rounded to 15 significant digits. .NET 11 made floating-point/decimal conversions
        // correctly-rounded (dotnet/runtime#130566), so Convert.ToDecimal now yields the full binary expansion (e.g. 21.35 =>
        // 21.350000000000001421085471520). Round-trip through the shortest 15-significant-digit representation to preserve the
        // original behavior.
        static decimal DoubleToDecimal(double value)
            => decimal.Parse(
                value.ToString("G15", CultureInfo.InvariantCulture),
                NumberStyles.Float,
                CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, decimal value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
