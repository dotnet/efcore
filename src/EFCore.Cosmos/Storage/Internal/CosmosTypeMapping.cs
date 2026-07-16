// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosTypeMapping : CoreTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosTypeMapping Default { get; } = new(typeof(object));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosTypeMapping(
        Type clrType,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        : base(
            new CoreTypeMappingParameters(
                clrType,
                converter: null,
                comparer,
                keyComparer,
                elementMapping: elementMapping,
                jsonValueReaderWriter: jsonValueReaderWriter))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CosmosTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping WithComposedConverter(
        ValueConverter? converter,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        => new CosmosTypeMapping(Parameters.WithComposedConverter(converter, comparer, keyComparer, elementMapping, jsonValueReaderWriter));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
        => new CosmosTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlParameter CreateParameter(string name, object? value)
        => new SqlValueParameter(name, ConvertToProviderValue(value));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? ConvertToProviderValue(object? value)
    {
        value = NormalizeValue(value);
        if (Converter != null && (value is not null || Converter.ConvertsNulls))
        {
            value = Converter.ConvertToProvider(value);
        }
        return value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateSqlLiteral(object? value)
    {
        value = NormalizeValue(value);
        return value is not null || JsonValueReaderWriter!.HandlesNullWrites
                ? JsonValueReaderWriter!.ToJsonString(value)
                : "null";
    }

    private object? NormalizeValue(object? value)
    {
        if (value is null)
        {
            return value;
        }

        var type = value.GetType();

        // When Enum column is compared to constant the C# compiler put a constant of integer there
        // In some unknown cases for parameter we also see integer value.
        // So if CLR type is enum we need to convert integer value to enum value
        if (type.IsInteger() == true
            && ClrType.UnwrapNullableType().IsEnum)
        {
            return Enum.ToObject(ClrType.UnwrapNullableType(), value);
        }

        // When Enum is cast manually our logic of removing implicit convert gives us enum value here
        // So if CLR type is integer we need to convert enum value to integer value
        if (type.IsEnum == true
            && ClrType.UnwrapNullableType().IsInteger())
        {
            return Convert.ChangeType(value, ClrType);
        }

        // Handle implicit conversions here to ensure the boxed value has the type expected by JsonValueReaderWriter. Otherwise, unboxing it will throw if the boxed type does not match (for example, value = (char)boxedInt).
        if (type != ClrType.UnwrapNullableType() && value is IConvertible)
        {
            return Convert.ChangeType(value, ClrType);
        }

        return value;
    }
}
