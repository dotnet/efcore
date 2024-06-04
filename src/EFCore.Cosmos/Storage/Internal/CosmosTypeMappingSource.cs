// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosTypeMappingSource : TypeMappingSource
{
    private readonly Dictionary<Type, CosmosTypeMapping> _clrTypeMappings;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosTypeMappingSource(TypeMappingSourceDependencies dependencies)
        : base(dependencies)
    {
        _clrTypeMappings
            = new Dictionary<Type, CosmosTypeMapping>
            {
                {
                    typeof(JObject), new CosmosTypeMapping(
                        typeof(JObject), jsonValueReaderWriter: dependencies.JsonValueReaderWriterSource.FindReaderWriter(typeof(JObject)))
                }
            };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        Check.DebugAssert(clrType != null, "ClrType is null");

        return _clrTypeMappings.TryGetValue(clrType, out var mapping)
            ? mapping
            : (FindPrimitiveMapping(mappingInfo)
                ?? FindCollectionMapping(mappingInfo)
                ?? base.FindMapping(mappingInfo));
    }

    private CoreTypeMapping? FindPrimitiveMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType!;
        if ((clrType.IsValueType
                && clrType != typeof(Guid)
                && !clrType.IsEnum)
            || clrType == typeof(string))
        {
            return new CosmosTypeMapping(
                clrType, jsonValueReaderWriter: Dependencies.JsonValueReaderWriterSource.FindReaderWriter(clrType));
        }

        return null;
    }

    private CoreTypeMapping? FindCollectionMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType!;
        var elementMapping = mappingInfo.ElementTypeMapping;

        // Special case for byte[], to allow it to be treated as a scalar (i.e. base64 encoding) rather than as a collection
        if (clrType == typeof(byte[]) && elementMapping is null)
        {
            return null;
        }

        // First attempt to resolve this as a primitive collection (e.g. List<int>). This does not handle Dictionary.
        if (TryFindJsonCollectionMapping(
                mappingInfo, clrType, providerClrType: null, ref elementMapping, out var elementComparer,
                out var collectionReaderWriter)
            && elementMapping is not null)
        {
            return new CosmosTypeMapping(
                clrType, elementComparer, elementMapping: elementMapping, jsonValueReaderWriter: collectionReaderWriter);
        }

        // Next, attempt to resolve this as a dictionary (e.g. Dictionary<string, int>).
        if (elementMapping is not null)
        {
            return null;
        }

        var elementType = clrType.TryGetSequenceType();
        if (elementType == null)
        {
            return null;
        }

        var jsonValueReaderWriter = Dependencies.JsonValueReaderWriterSource.FindReaderWriter(clrType);

        if (clrType is { IsGenericType: true, IsGenericTypeDefinition: false })
        {
            var genericTypeDefinition = clrType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Dictionary<,>)
                || genericTypeDefinition == typeof(IDictionary<,>)
                || genericTypeDefinition == typeof(IReadOnlyDictionary<,>))
            {
                var genericArguments = clrType.GenericTypeArguments;
                if (genericArguments[0] != typeof(string))
                {
                    return null;
                }

                elementType = genericArguments[1];
                var elementMappingInfo = new TypeMappingInfo(elementType);
                elementMapping = FindPrimitiveMapping(elementMappingInfo)
                    ?? FindCollectionMapping(elementMappingInfo);
                return elementMapping == null
                    ? null
                    : new CosmosTypeMapping(
                        clrType, CreateStringDictionaryComparer(elementMapping, elementType, clrType),
                        jsonValueReaderWriter: jsonValueReaderWriter);
            }
        }

        return null;
    }

    private static ValueComparer CreateStringDictionaryComparer(
        CoreTypeMapping elementMapping,
        Type elementType,
        Type dictType,
        bool readOnly = false)
    {
        var unwrappedType = elementType.UnwrapNullableType();

        return (ValueComparer)Activator.CreateInstance(
            elementType == unwrappedType
                ? typeof(StringDictionaryComparer<,>).MakeGenericType(elementType, dictType)
                : typeof(NullableStringDictionaryComparer<,>).MakeGenericType(unwrappedType, dictType),
            elementMapping.Comparer,
            readOnly)!;
    }
}
