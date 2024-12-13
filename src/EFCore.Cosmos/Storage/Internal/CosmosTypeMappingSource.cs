// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
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
        => _clrTypeMappings
            = new Dictionary<Type, CosmosTypeMapping>
            {
                {
                    typeof(JObject), new CosmosTypeMapping(
                        typeof(JObject), jsonValueReaderWriter: dependencies.JsonValueReaderWriterSource.FindReaderWriter(typeof(JObject)))
                }
            };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping? FindMapping(IProperty property)
        // A provider should typically not override this because using the property directly causes problems with Migrations where
        // the property does not exist. However, since the Cosmos provider doesn't have Migrations, it should be okay to use the property
        // directly.
        => base.FindMapping(property) switch
        {
            CosmosTypeMapping mapping when property.FindAnnotation(CosmosAnnotationNames.VectorType)?.Value is CosmosVectorType vectorType
                => new CosmosVectorTypeMapping(mapping, vectorType),
            var other => other
        };

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
            : (base.FindMapping(mappingInfo) // This will find a mapping from plugins, and so must happen first. See #34041.
                ?? FindPrimitiveMapping(mappingInfo)
                ?? FindCollectionMapping(mappingInfo));
    }

    private CoreTypeMapping? FindPrimitiveMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType!;

        var memoryType = clrType.TryGetElementType(typeof(ReadOnlyMemory<>));
        if (memoryType != null)
        {
            return new CosmosTypeMapping(clrType)
                .WithComposedConverter(
                    (ValueConverter)Activator.CreateInstance(typeof(ReadOnlyMemoryConverter<>).MakeGenericType(memoryType))!,
                    (ValueComparer)Activator.CreateInstance(typeof(ReadOnlyMemoryComparer<>).MakeGenericType(memoryType))!);
        }

        return clrType.IsNumeric()
            || clrType == typeof(bool)
            || clrType == typeof(DateOnly)
            || clrType == typeof(TimeOnly)
            || clrType == typeof(DateTime)
            || clrType == typeof(DateTimeOffset)
            || clrType == typeof(TimeSpan)
            || clrType == typeof(string)
                ? new CosmosTypeMapping(
                    clrType, jsonValueReaderWriter: Dependencies.JsonValueReaderWriterSource.FindReaderWriter(clrType))
                : null;
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
                mappingInfo,
                clrType,
                providerClrType: null,
                ref elementMapping,
                out var elementComparer,
                out var collectionReaderWriter)
            && elementMapping is not null)
        {
            return new CosmosTypeMapping(
                clrType,
                elementComparer,
                elementMapping: elementMapping,
                jsonValueReaderWriter: collectionReaderWriter);
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

        if (clrType is { IsGenericType: true, IsGenericTypeDefinition: false })
        {
            var genericTypeDefinition = clrType.GetGenericTypeDefinition();

            // This is legacy type mapping support for dictionaries in Cosmos. This needs to be consolidated with the relational
            // support, but for now this is being added back in to avoid a regression in EF9.
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

                if (elementMapping != null)
                {
                    var jsonValueReaderWriter = Dependencies.JsonValueReaderWriterSource.FindReaderWriter(clrType);
                    if (jsonValueReaderWriter == null
                        && elementMapping.JsonValueReaderWriter != null)
                    {
                        jsonValueReaderWriter = (JsonValueReaderWriter?)Activator.CreateInstance(
                            typeof(PlaceholderJsonStringKeyedDictionaryReaderWriter<>)
                                .MakeGenericType(elementMapping.JsonValueReaderWriter.ValueType),
                            elementMapping.JsonValueReaderWriter);
                    }

                    return new CosmosTypeMapping(
                        clrType,
                        CreateStringDictionaryComparer(elementMapping, elementType, clrType),
                        jsonValueReaderWriter: jsonValueReaderWriter);
                }
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
            typeof(StringDictionaryComparer<,>).MakeGenericType(dictType, elementType),
#pragma warning disable EF1001 // Internal EF Core API usage.
            elementMapping.Comparer.ComposeConversion(elementType))!;
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    // This ensures that the element reader/writers are not null when using Cosmos dictionary type mappings, but
    // is never actually used because Cosmos does not (yet) read and write JSON using this mechanism.
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
#pragma warning disable EF1001
    public sealed class PlaceholderJsonStringKeyedDictionaryReaderWriter<TElement>(JsonValueReaderWriter elementReaderWriter)
        : JsonValueReaderWriter<IEnumerable<KeyValuePair<string, TElement>>>, ICompositeJsonValueReaderWriter
#pragma warning restore EF1001
    {
        private readonly JsonValueReaderWriter<TElement> _elementReaderWriter = (JsonValueReaderWriter<TElement>)elementReaderWriter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<KeyValuePair<string, TElement>> FromJsonTyped(
            ref Utf8JsonReaderManager manager,
            object? existingObject = null)
            => throw new NotImplementedException("JsonValueReaderWriter infrastructure is not supported on Cosmos.");

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ToJsonTyped(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, TElement>> value)
            => throw new NotImplementedException("JsonValueReaderWriter infrastructure is not supported on Cosmos.");

        JsonValueReaderWriter ICompositeJsonValueReaderWriter.InnerReaderWriter
            => _elementReaderWriter;

        private readonly ConstructorInfo _constructorInfo
            = typeof(PlaceholderJsonStringKeyedDictionaryReaderWriter<TElement>)
                .GetConstructor([typeof(JsonValueReaderWriter<TElement>)])!;

        /// <inheritdoc />
        public override Expression ConstructorExpression
#pragma warning disable EF9100
#pragma warning disable EF1001
            => Expression.New(_constructorInfo, ((ICompositeJsonValueReaderWriter)this).InnerReaderWriter.ConstructorExpression);
#pragma warning restore EF1001
#pragma warning restore EF9100
    }
}
