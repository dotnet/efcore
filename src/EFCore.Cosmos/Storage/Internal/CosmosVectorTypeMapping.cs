// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[Experimental(EFDiagnostics.CosmosVectorSearchExperimental)]
public class CosmosVectorTypeMapping : CosmosTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new CosmosVectorTypeMapping Default { get; }
    // Note that this default is not valid because dimensions cannot be zero. But since there is no reasonable
    // default dimensions size for a vector type, this is intentionally not valid rather than just being wrong.
    // The fundamental problem here is that type mappings are "required" to have some default now.
        = new(typeof(byte[]), new CosmosVectorType(DistanceFunction.Cosine, 0));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosVectorTypeMapping(
        Type clrType,
        CosmosVectorType vectorType,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        : this(
            new CoreTypeMappingParameters(
                clrType,
                converter: null,
                comparer,
                keyComparer,
                elementMapping: elementMapping,
                jsonValueReaderWriter: jsonValueReaderWriter),
            vectorType)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosVectorTypeMapping(CosmosTypeMapping mapping, CosmosVectorType vectorType)
        : this(
            new CoreTypeMappingParameters(
                mapping.ClrType,
                // This is a hack to allow both arrays and ROM types without different function overloads or type mappings.
                converter: mapping.Converter?.GetType() == typeof(BytesToStringConverter) ? null : mapping.Converter,
                mapping.Comparer,
                mapping.KeyComparer,
                elementMapping: mapping.ElementTypeMapping,
                jsonValueReaderWriter: mapping.JsonValueReaderWriter),
            vectorType)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CosmosVectorTypeMapping(CoreTypeMappingParameters parameters, CosmosVectorType vectorType)
        : base(parameters)
        => VectorType = vectorType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosVectorType VectorType { get; }

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
        => new CosmosVectorTypeMapping(
            Parameters.WithComposedConverter(converter, comparer, keyComparer, elementMapping, jsonValueReaderWriter),
            VectorType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
        => new CosmosVectorTypeMapping(parameters, VectorType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override JToken? GenerateJToken(object? value)
    {
        // This is a hack to allow both arrays and ROM types without different function overloads or type mappings.
        var type = value?.GetType();
        if (type?.IsArray is false)
        {
            if (type == typeof(ReadOnlyMemory<byte>))
            {
                value = ((ReadOnlyMemory<byte>)value!).ToArray();
            }
            else if (type == typeof(ReadOnlyMemory<sbyte>))
            {
                value = ((ReadOnlyMemory<sbyte>)value!).ToArray();
            }
            else if (type == typeof(ReadOnlyMemory<float>))
            {
                value = ((ReadOnlyMemory<float>)value!).ToArray();
            }
        }

        return value == null
            ? null
            : JToken.FromObject(value, CosmosClientWrapper.Serializer);
    }
}
