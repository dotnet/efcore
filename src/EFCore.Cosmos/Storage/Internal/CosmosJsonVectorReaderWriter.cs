// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class CosmosJsonVectorReaderWriter : JsonValueReaderWriter
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonVectorReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosJsonVectorReaderWriter Instance { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object FromJson(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        var tokenType = manager.CurrentReader.TokenType;
        if (tokenType != JsonTokenType.StartArray)
        {
            throw new InvalidOperationException(
                CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
        }

        var result = new List<byte>();

        while (tokenType != JsonTokenType.EndArray)
        {
            manager.MoveNext();
            tokenType = manager.CurrentReader.TokenType;

            if (tokenType != JsonTokenType.Number || !manager.CurrentReader.TryGetInt32(out var intValue))
            {
                throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            result.Add((byte)intValue);
        }

        return result.ToArray();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void ToJson(Utf8JsonWriter writer, object value)
    {
        writer.WriteStartArray();

        switch (value)
        {
            case IEnumerable<byte> bytes:
                foreach (var item in bytes)
                {
                    writer.WriteNumberValue(item);
                }
                break;
            case ReadOnlyMemory<byte> rom:
                foreach (var item in rom.Span)
                {
                    writer.WriteNumberValue(item);
                }
                break;
            case IEnumerable<sbyte> bytes:
                foreach (var item in bytes)
                {
                    writer.WriteNumberValue(item);
                }
                break;
            case ReadOnlyMemory<sbyte> rom:
                foreach (var item in rom.Span)
                {
                    writer.WriteNumberValue(item);
                }
                break;
            case IEnumerable<float> bytes:
                foreach (var item in bytes)
                {
                    writer.WriteNumberValue(item);
                }
                break;
            case ReadOnlyMemory<float> rom:
                foreach (var item in rom.Span)
                {
                    writer.WriteNumberValue(item);
                }
                break;
            default:
                throw new InvalidOperationException();
        }

        writer.WriteEndArray();
    }

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);

    /// <inheritdoc />
    public override Type ValueType { get; } = typeof(byte[]);
}
