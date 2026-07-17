// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Projections of numbers in cosmos can result in double precision floating point numbers,
///     and thus have to be read as doubles to prevent reader exceptions
/// </remarks>
public sealed class CosmosJsonNumberProjectionReaderWriter<T> : JsonValueReaderWriter<T>
    where T : INumber<T>
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonNumberProjectionReaderWriter<T>).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static CosmosJsonNumberProjectionReaderWriter<T> Instance { get; } = new();

    /// <inheritdoc/>
    public override T FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.TryGetDouble(out var d)
         ? T.CreateChecked(d) // #38138
         : throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(manager.CurrentReader.TokenType));

    /// <inheritdoc/>
    public override void ToJsonTyped(Utf8JsonWriter writer, T value)
    {
        if (typeof(T) == typeof(int)
            || typeof(T) == typeof(short)
            || typeof(T) == typeof(sbyte)
            || typeof(T) == typeof(byte)
            || typeof(T) == typeof(ushort))
        {
            writer.WriteNumberValue(int.CreateChecked(value));
        }
        else if (typeof(T) == typeof(uint))
        {
            writer.WriteNumberValue(uint.CreateChecked(value));
        }
        else if (typeof(T) == typeof(long))
        {
            writer.WriteNumberValue(long.CreateChecked(value));
        }
        else if (typeof(T) == typeof(ulong))
        {
            writer.WriteNumberValue(ulong.CreateChecked(value));
        }
        else if (typeof(T) == typeof(float))
        {
            writer.WriteNumberValue(float.CreateChecked(value));
        }
        else if (typeof(T) == typeof(double))
        {
            writer.WriteNumberValue(double.CreateChecked(value));
        }
        else if (typeof(T) == typeof(decimal))
        {
            writer.WriteNumberValue(decimal.CreateChecked(value));
        }
        else
        {
            throw new UnreachableException($"Unsupported numeric type '{typeof(T)}' for JSON number projection.");
        }
    }

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
