// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     A value converter that converts a .NET primitive collection into a JSON string.
/// </summary>
// TODO: This currently just calls JsonSerialize.Serialize/Deserialize. It should go through the element type mapping's APIs for
// serializing/deserializing JSON instead, when those APIs are introduced.
// TODO: Nulls? Mapping hints? Customizable JsonSerializerOptions?
public class CollectionToJsonStringConverter : ValueConverter
{
    private readonly CoreTypeMapping _elementTypeMapping;

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public CollectionToJsonStringConverter(Type modelClrType, CoreTypeMapping elementTypeMapping)
        : base(
            (Expression<Func<object, string>>)(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null)),
            (Expression<Func<string, object>>)(s => JsonSerializer.Deserialize(s, modelClrType, (JsonSerializerOptions?)null)!)) // TODO: Nullability
    {
        ModelClrType = modelClrType;
        _elementTypeMapping = elementTypeMapping;

        // TODO: Value converters on the element type mapping should be supported
        // TODO: Full sanitization/nullability
        ConvertToProvider = x => x is null ? "[]" : JsonSerializer.Serialize(x);
        ConvertFromProvider = o
            => o is string s
                ? JsonSerializer.Deserialize(s, modelClrType)!
                : throw new ArgumentException(); // TODO
    }

    /// <inheritdoc />
    public override Func<object?, object?> ConvertToProvider { get; }

    /// <inheritdoc />
    public override Func<object?, object?> ConvertFromProvider { get; }

    /// <inheritdoc />
    public override Type ModelClrType { get; }

    /// <inheritdoc />
    public override Type ProviderClrType
        => typeof(string);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj) || (obj is CollectionToJsonStringConverter other && Equals(other));

    private bool Equals(CollectionToJsonStringConverter other)
        => ModelClrType == other.ModelClrType && _elementTypeMapping.Equals(other._elementTypeMapping);

    /// <inheritdoc />
    public override int GetHashCode()
        => ModelClrType.GetHashCode();
}
