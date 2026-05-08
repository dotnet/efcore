// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Query;

#pragma warning disable EF1001 // Internal EF Core API usage

public partial class RelationalMaterializerFactory
{
    /// <summary>
    ///     Builds a materializer for a standalone JSON structural type projection (e.g. <c>Select(x => x.Address)</c>).
    ///     Reads the JSON column from the DbDataReader, parses it, and materializes the structural type.
    /// </summary>
    private Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?> BuildTopLevelJsonMaterializer<T>(
        ITypeBase structuralType,
        JsonProjectionInfo jsonProjectionInfo,
        bool isTracking,
        bool nullable)
    {
        // Disallow tracking queries to project owned entities (but not complex types)
        if (structuralType is IEntityType && isTracking)
        {
            throw new InvalidOperationException(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner);
        }

        var jsonMaterializer = BuildJsonStructuralTypeMaterializer(structuralType, isTracking, nullable);

        // Find the JSON column type mapping for correct reading
        var jsonColumnName = structuralType.GetContainerColumnName()!;
        var containingEntityType = structuralType is IEntityType et ? et : structuralType.ContainingEntityType;
        var jsonColumn = containingEntityType.GetViewOrTableMappings()
            .Select(m => m.Table.FindColumn(jsonColumnName))
            .FirstOrDefault(c => c is not null)
            ?? throw new UnreachableException(
                $"Could not find JSON container column '{jsonColumnName}' for type '{structuralType.DisplayName()}'.");

        var jsonColumnTypeMapping = jsonColumn.StoreTypeMapping;
        var jsonStreamReader = BuildJsonColumnReader(jsonColumnTypeMapping, jsonProjectionInfo.JsonColumnIndex);

        return (queryContext, dataReader, rc, coord) =>
        {
            var jsonReaderData = ReadJsonColumn(
                dataReader, jsonProjectionInfo.JsonColumnIndex, jsonStreamReader, queryContext);
            if (jsonReaderData is null)
            {
                return nullable
                    ? default
                    : throw new InvalidOperationException(
                        RelationalStrings.JsonRequiredEntityWithNullJson(structuralType.ClrType.ShortDisplayName()));
            }

            var keyValues = ExtractJsonKeyValues(
                dataReader, jsonProjectionInfo, structuralType, isCollection: false);
            return (T?)jsonMaterializer.Materialize(queryContext, jsonReaderData, keyValues, out _);
        };
    }

    /// <summary>
    ///     Builds a materializer for a standalone JSON collection projection (e.g. <c>Select(x => x.Associates)</c>).
    ///     Reads the JSON column from the DbDataReader, parses the array, and materializes each element.
    /// </summary>
    private Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?> BuildJsonCollectionProjectionMaterializer<T>(
        IPropertyBase structuralProperty,
        JsonProjectionInfo jsonProjectionInfo,
        bool isTracking)
    {
        var (elementStructuralType, isNullable) = structuralProperty switch
        {
            INavigation n => ((ITypeBase)n.TargetEntityType, !n.ForeignKey.IsRequiredDependent),
            IComplexProperty cp => (cp.ComplexType, cp.IsNullable),
            _ => throw new UnreachableException()
        };

        // Disallow tracking queries to project owned entities (but not complex types)
        if (elementStructuralType is IEntityType && isTracking)
        {
            throw new InvalidOperationException(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner);
        }

        var elementMaterializer = BuildJsonStructuralTypeMaterializer(elementStructuralType, isTracking, isNullable);

        var jsonColumnName = elementStructuralType.GetContainerColumnName()!;
        var containingEntityType = elementStructuralType.ContainingEntityType;
        var jsonColumn = containingEntityType.GetViewOrTableMappings()
            .Select(m => m.Table.FindColumn(jsonColumnName))
            .FirstOrDefault(c => c is not null)
            ?? throw new UnreachableException(
                $"Could not find JSON container column '{jsonColumnName}' for type '{elementStructuralType.DisplayName()}'.");

        var jsonColumnTypeMapping = jsonColumn.StoreTypeMapping;
        var jsonStreamReader = BuildJsonColumnReader(jsonColumnTypeMapping, jsonProjectionInfo.JsonColumnIndex);

        return (queryContext, dataReader, rc, coord) =>
        {
            var jsonReaderData = ReadJsonColumn(
                dataReader, jsonProjectionInfo.JsonColumnIndex, jsonStreamReader, queryContext);
            if (jsonReaderData is null)
            {
                return default;
            }

            var keyValues = ExtractJsonKeyValues(
                dataReader, jsonProjectionInfo, elementStructuralType, isCollection: true);
            return (T?)RelationalJsonStructuralTypeMaterializer.MaterializeCollection(
                queryContext, jsonReaderData, keyValues, elementMaterializer, structuralProperty);
        };
    }

    /// <summary>
    ///     Builds a <see cref="RelationalJsonStructuralTypeMaterializer" /> for the given structural type (entity or complex type)
    ///     that is mapped to JSON. The materializer reads a JSON stream via <see cref="Utf8JsonReaderManager" /> and
    ///     produces instances of the structural type with all properties and nested children populated.
    /// </summary>
    /// <remarks>
    ///     This is the non-generated equivalent of
    ///     <see cref="RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.CreateJsonShapers" />
    ///     and <c>JsonEntityMaterializerRewriter.GenerateJsonPropertyReadLoop</c>.
    /// </remarks>
    internal static RelationalJsonStructuralTypeMaterializer BuildJsonStructuralTypeMaterializer(
        ITypeBase structuralType,
        bool isTracking,
        bool nullable)
    {
        // Determine constructor binding and which properties are consumed by constructor parameters
        var constructorBinding = structuralType.ConstructorBinding;
        Dictionary<IProperty, int>? constructorParameterMap = null;
        var constructorParameterCount = 0;

        switch (constructorBinding)
        {
            // Parameterless constructor or value type default
            case null or ConstructorBinding { ParameterBindings.Count: 0 } or DefaultValueBinding:
                break;

            // Constructor with property-bound parameters
            case ConstructorBinding { ParameterBindings: { Count: > 0 } paramBindings }:
            {
                constructorParameterCount = paramBindings.Count;
                constructorParameterMap = new Dictionary<IProperty, int>(paramBindings.Count);

                for (var p = 0; p < paramBindings.Count; p++)
                {
                    switch (paramBindings[p])
                    {
                        case PropertyParameterBinding { ConsumedProperties: [IProperty property] }:
                            constructorParameterMap[property] = p;
                            break;

                        default:
                            throw new NotImplementedException(
                                $"The non-generated materializer does not yet support constructor parameter "
                                + $"binding type '{paramBindings[p].GetType().Name}' on '{structuralType.DisplayName()}'.");
                    }
                }

                break;
            }

            default:
                throw new UnreachableException(
                    $"Unexpected instantiation binding type '{constructorBinding.GetType().Name}' on '{structuralType.DisplayName()}'.");
        }

        // Build property handlers — a single unified list of scalar properties and nested structural types.
        var properties = new List<RelationalJsonStructuralTypeMaterializer.JsonPropertyHandler>();

        foreach (var property in structuralType.GetProperties())
        {
            if (property.IsKey())
            {
                continue;
            }

            var jsonPropertyName = property.GetJsonPropertyName();
            if (jsonPropertyName is null)
            {
                continue;
            }

            var jsonReaderWriter = property.GetJsonValueReaderWriter()
                ?? property.GetTypeMapping().JsonValueReaderWriter;
            if (jsonReaderWriter is null)
            {
                continue;
            }

            if (property.IsShadowProperty())
            {
                // Shadow properties have no CLR member — values go into the snapshot for tracking
                properties.Add(new RelationalJsonStructuralTypeMaterializer.JsonPropertyHandler(
                    Encoding.UTF8.GetBytes(jsonPropertyName),
                    jsonReaderWriter,
                    memberInfo: null!,
                    property.IsNullable,
                    shadowIndex: property.GetShadowIndex()));
                continue;
            }

            var constructorParamIndex = constructorParameterMap is not null
                && constructorParameterMap.TryGetValue(property, out var idx) ? idx : -1;

            var memberInfo = constructorParamIndex < 0
                ? property.GetMemberInfo(forMaterialization: true, forSet: true)
                : null!; // Constructor-consumed properties don't need a setter MemberInfo

            properties.Add(new RelationalJsonStructuralTypeMaterializer.JsonPropertyHandler(
                Encoding.UTF8.GetBytes(jsonPropertyName),
                jsonReaderWriter,
                memberInfo,
                property.IsNullable,
                constructorParameterIndex: constructorParamIndex));
        }

        // Add nested structural type handlers (complex properties and owned navigations mapped to JSON)
        IEnumerable<IPropertyBase> nestedStructuralProperties = structuralType.GetComplexProperties();

        if (structuralType is IEntityType entityType)
        {
            nestedStructuralProperties = nestedStructuralProperties.Concat(
                entityType.GetNavigations()
                    .Where(n => n.TargetEntityType.IsMappedToJson()
                        && n.ForeignKey.IsOwnership
                        && n == n.ForeignKey.PrincipalToDependent));
        }

        foreach (var nestedProperty in nestedStructuralProperties)
        {
            var (nestedStructuralType, isStructuralPropertyNullable) = nestedProperty switch
            {
                INavigation n => ((ITypeBase)n.TargetEntityType, !n.ForeignKey.IsRequiredDependent),
                IComplexProperty cp => (cp.ComplexType, cp.IsNullable),
                _ => throw new UnreachableException()
            };

            var jsonPropertyName = nestedStructuralType.GetJsonPropertyName();
            if (jsonPropertyName is null)
            {
                continue;
            }

            var innerMaterializer = BuildJsonStructuralTypeMaterializer(
                nestedStructuralType,
                isTracking,
                nullable || isStructuralPropertyNullable);

            var nestedMemberInfo = nestedProperty.GetMemberInfo(forMaterialization: true, forSet: true);

            properties.Add(new RelationalJsonStructuralTypeMaterializer.JsonPropertyHandler(
                Encoding.UTF8.GetBytes(jsonPropertyName),
                innerMaterializer,
                nestedMemberInfo,
                nestedProperty.IsCollection,
                nestedProperty));
        }

        // Build key property members for entity types (needed for identity resolution and FK fixup)
        MemberInfo[]? keyPropertyMembers = null;
        if (structuralType is IEntityType et)
        {
            var primaryKey = et.FindPrimaryKey();
            if (primaryKey is not null)
            {
                keyPropertyMembers = new MemberInfo[primaryKey.Properties.Count];
                for (var i = 0; i < primaryKey.Properties.Count; i++)
                {
                    var prop = primaryKey.Properties[i];
                    // Shadow properties don't have CLR members — skip them.
                    // The key values are still passed for identity resolution.
                    if (!prop.IsShadowProperty())
                    {
                        keyPropertyMembers[i] = prop.GetMemberInfo(forMaterialization: true, forSet: true);
                    }
                }
            }
        }

        ConstructorInvoker? constructorInvoker = null;
        switch (constructorBinding)
        {
            // Parameterless or value type default — use the CLR default constructor
            case null or DefaultValueBinding:
                var ctor = structuralType.ClrType.GetConstructor(Type.EmptyTypes);
                constructorInvoker = ctor is not null ? ConstructorInvoker.Create(ctor) : null;
                break;

            // Constructor binding (parameterless or with parameters)
            case ConstructorBinding { Constructor: var ctorInfo }:
                constructorInvoker = ConstructorInvoker.Create(ctorInfo);
                break;

            default:
                throw new UnreachableException(
                    $"Unexpected instantiation binding type '{constructorBinding.GetType().Name}' on '{structuralType.DisplayName()}'.");
        }

        return new RelationalJsonStructuralTypeMaterializer(
            structuralType,
            properties.ToArray(),
            keyPropertyMembers,
            constructorInvoker,
            constructorParameterCount,
            isTracking,
            nullable);
    }

    /// <summary>
    ///     Reads a JSON column from a <see cref="DbDataReader" />, creates a <see cref="JsonReaderData" />,
    ///     and initializes a <see cref="Utf8JsonReaderManager" /> positioned on the first token.
    ///     Returns <see langword="null"/> if the column is null (DBNull).
    /// </summary>
    /// <remarks>
    ///     This is the non-generated equivalent of <c>GenerateJsonReader</c> in the generated shaper.
    /// </remarks>
    internal static JsonReaderData? ReadJsonColumn(
        DbDataReader dataReader,
        int jsonColumnIndex,
        Func<DbDataReader, MemoryStream?> jsonStreamReader,
        QueryContext queryContext)
    {
        if (dataReader.IsDBNull(jsonColumnIndex))
        {
            return null;
        }

        var stream = jsonStreamReader(dataReader);
        if (stream is null)
        {
            return null;
        }

        var jsonReaderData = new JsonReaderData(stream);
        var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
        manager.MoveNext();
        manager.CaptureState();

        return jsonReaderData;
    }

    /// <summary>
    ///     Builds a compiled delegate that reads a JSON column from a <see cref="DbDataReader" />
    ///     and returns a <see cref="MemoryStream" /> (applying provider-specific customization like
    ///     SQLite's string→UTF8 stream conversion).
    /// </summary>
    internal static Func<DbDataReader, MemoryStream?> BuildJsonColumnReader(
        RelationalTypeMapping jsonColumnTypeMapping,
        int columnIndex)
    {
        var dataReaderParam = Expression.Parameter(typeof(DbDataReader), "reader");

        var getMethod = jsonColumnTypeMapping.GetDataReaderMethod();
        Expression readExpression = Expression.Call(
            getMethod.DeclaringType != typeof(DbDataReader)
                ? Expression.Convert(dataReaderParam, getMethod.DeclaringType!)
                : dataReaderParam,
            getMethod,
            Expression.Constant(columnIndex));

        readExpression = jsonColumnTypeMapping.CustomizeDataReaderExpression(readExpression);

        if (readExpression.Type != typeof(MemoryStream))
        {
            readExpression = Expression.Convert(readExpression, typeof(MemoryStream));
        }

        return Expression.Lambda<Func<DbDataReader, MemoryStream?>>(readExpression, dataReaderParam).Compile();
    }

    /// <summary>
    ///     Extracts key values from a <see cref="DbDataReader" /> using <see cref="JsonProjectionInfo.KeyAccessInfo" />.
    ///     Returns <see langword="null"/> if key values are not needed (complex types).
    /// </summary>
    /// <remarks>
    ///     This is the non-generated equivalent of the key extraction in <c>JsonShapingPreProcess</c>.
    /// </remarks>
    internal static object[]? ExtractJsonKeyValues(
        DbDataReader dataReader,
        JsonProjectionInfo jsonProjectionInfo,
        ITypeBase structuralType,
        bool isCollection)
    {
        if (structuralType is not IEntityType entityType)
        {
            return null;
        }

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey is null)
        {
            return null;
        }

        var expectedKeyCount = primaryKey.Properties.Count - (isCollection ? 1 : 0);
        var keyValues = new object[primaryKey.Properties.Count];

        for (var i = 0; i < jsonProjectionInfo.KeyAccessInfo.Count && i < expectedKeyCount; i++)
        {
            var (keyProperty, constantKeyValue, keyProjectionIndex) = jsonProjectionInfo.KeyAccessInfo[i];

            if (constantKeyValue is not null)
            {
                // Constant array index — use 1-based indexing
                keyValues[i] = constantKeyValue.Value + 1;
            }
            else if (keyProjectionIndex is not null)
            {
                var value = dataReader.GetFieldValue<object>(keyProjectionIndex.Value);
                if (keyProperty is not null && value.GetType() != keyProperty.ClrType)
                {
                    value = Convert.ChangeType(value, keyProperty.ClrType);
                }
                else
                {
                    // Non-constant array index — add 1 for 1-based indexing
                    value = Convert.ToInt32(value) + 1;
                }

                keyValues[i] = value;
            }
        }

        // Fill remaining slots with default (1 for synthesized keys)
        for (var i = jsonProjectionInfo.KeyAccessInfo.Count; i < expectedKeyCount; i++)
        {
            keyValues[i] = 1;
        }

        return keyValues;
    }
}
