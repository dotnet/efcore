// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        = Create(typeof(byte[]), new CosmosVectorType(DistanceFunction.Cosine, 0));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosVectorTypeMapping Create(Type clrType, CosmosVectorType vectorType)
    {
        var collectionType = clrType;
        var isRom = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<>);
        if (isRom)
        {
            collectionType = clrType.GetGenericArguments()[0].MakeArrayType();
        }

        var elementType = collectionType.GetElementType()!;

        JsonValueReaderWriter? jsonValueReaderWriter = collectionType switch
        {
            Type t when t == typeof(byte[]) => new JsonCollectionOfStructsReaderWriter<byte[], byte>(JsonByteReaderWriter.Instance),
            Type t when t == typeof(sbyte[]) => new JsonCollectionOfStructsReaderWriter<sbyte[], sbyte>(JsonSByteReaderWriter.Instance),
            Type t when t == typeof(float[]) => new JsonCollectionOfStructsReaderWriter<float[], float>(JsonFloatReaderWriter.Instance),
            _ => null
        };

        var parameters = new CoreTypeMappingParameters(
            clrType,
            null,
            null,
            null,
            null,
            jsonValueReaderWriter: jsonValueReaderWriter);

        if (isRom)
        {
            parameters = parameters.WithComposedConverter(
                    (ValueConverter)Activator.CreateInstance(typeof(ReadOnlyMemoryConverter<>).MakeGenericType(elementType))!,
                    (ValueComparer)Activator.CreateInstance(typeof(ReadOnlyMemoryComparer<>).MakeGenericType(elementType))!,
                    null,
                    null,
                    null);
        }

        return new CosmosVectorTypeMapping(parameters, vectorType);
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
