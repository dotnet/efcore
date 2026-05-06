// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage

/// <summary>
///     Runtime materializer for a structural type (entity or complex type) mapped to JSON.
///     Reads a JSON stream via <see cref="Utf8JsonReaderManager" /> and produces instances
///     with all scalar properties and nested children populated.
/// </summary>
/// <remarks>
///     This is the non-generated runtime equivalent of the compiled expression tree produced by
///     <c>JsonEntityMaterializerRewriter.GenerateJsonPropertyReadLoop</c> in the generated shaper.
/// </remarks>
internal sealed class RelationalJsonStructuralTypeMaterializer(
    ITypeBase structuralType,
    RelationalJsonStructuralTypeMaterializer.JsonPropertyHandler[] properties,
    MemberInfo[]? keyPropertyMembers,
    bool isTracking,
    bool nullable)
{
    private readonly ITypeBase _structuralType = structuralType;
    private readonly JsonPropertyHandler[] _properties = properties;
    private readonly MemberInfo[]? _keyPropertyMembers = keyPropertyMembers;
    private readonly bool _isTracking = isTracking;
    private readonly bool _nullable = nullable;

    /// <summary>
    ///     Materializes a single instance of the structural type from the current position in the JSON stream.
    ///     The reader must be positioned on a <see cref="JsonTokenType.StartObject" /> token.
    ///     After this method returns, the reader is positioned on the corresponding <see cref="JsonTokenType.EndObject" />.
    /// </summary>
    public object? Materialize(QueryContext queryContext, JsonReaderData jsonReaderData, object[]? keyValues)
    {
        var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
        var tokenType = manager.CurrentReader.TokenType;

        switch (tokenType)
        {
            case JsonTokenType.Null:
                return _nullable
                    ? null
                    : throw new InvalidOperationException(
                        RelationalStrings.JsonRequiredEntityWithNullJson(_structuralType.DisplayName()));

            case not JsonTokenType.StartObject:
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
        }

        // Create the entity/complex type instance
        var clrType = _structuralType.ClrType;

        // For tracked entity types: check identity map first
        if (_isTracking && _structuralType is IEntityType entityType && keyValues is not null)
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is not null && !primaryKey.Properties.Any(p => p.IsShadowProperty()))
            {
                var entry = queryContext.TryGetEntry(primaryKey, keyValues, throwOnNullKey: true, out _);
                if (entry is not null)
                {
                    // Entity already tracked — skip JSON reading, advance past the JSON object
                    SkipJsonObject(ref manager);
                    return entry.Entity;
                }
            }
        }

        var instance = Activator.CreateInstance(clrType)!;

        // Set key properties from keyValues (needed for change tracker fixup)
        if (_keyPropertyMembers is not null && keyValues is not null)
        {
            for (var i = 0; i < _keyPropertyMembers.Length && i < keyValues.Length; i++)
            {
                if (_keyPropertyMembers[i] is not null)
                {
                    SetMemberValue(instance, _keyPropertyMembers[i], keyValues[i]);
                }
            }
        }

        // Allocate slots for nested property results that must be applied after the loop
        // (nested types need all their JSON to be read before being assigned to the parent)
        object?[]? nestedResults = null;

        // JSON property read loop — mirrors GenerateJsonPropertyReadLoop in the generated shaper.
        // Uses a single unified array of property handlers (scalar + nested objects).
        // Optimized for the common case where properties arrive in model declaration order
        // (i.e. JSON serialized by EF): we speculatively check _properties[nextExpectedIndex]
        // first, which gives O(1) per property when the order matches. On miss, we fall back
        // to a full linear scan.
        var nextExpectedIndex = 0;
        tokenType = manager.MoveNext();
        while (tokenType != JsonTokenType.EndObject)
        {
            switch (tokenType)
            {
                case JsonTokenType.PropertyName:
                {
                    var matched = false;

                    // Fast path: check the next expected property first
                    if (nextExpectedIndex < _properties.Length
                        && manager.CurrentReader.ValueTextEquals(_properties[nextExpectedIndex].JsonNameUtf8))
                    {
                        matched = true;
                        ProcessMatchedProperty(
                            ref manager, queryContext, jsonReaderData, keyValues,
                            nextExpectedIndex, instance, ref nestedResults);
                        nextExpectedIndex++;
                    }
                    else
                    {
                        // Slow path: scan all properties
                        for (var i = 0; i < _properties.Length; i++)
                        {
                            if (!manager.CurrentReader.ValueTextEquals(_properties[i].JsonNameUtf8))
                            {
                                continue;
                            }

                            matched = true;
                            ProcessMatchedProperty(
                                ref manager, queryContext, jsonReaderData, keyValues,
                                i, instance, ref nestedResults);

                            // Advance expected index past this match so subsequent
                            // properties can still hit the fast path
                            nextExpectedIndex = i + 1;
                            break;
                        }
                    }

                    if (!matched)
                    {
                        // Unknown property — skip its value
                        manager.MoveNext();
                        manager.Skip();
                    }

                    break;
                }

                case JsonTokenType.EndObject:
                    break;

                default:
                    manager.Skip();
                    break;
            }

            tokenType = manager.MoveNext();
        }

        // Apply nested results (navigations/complex properties) via setters
        if (nestedResults is not null)
        {
            for (var i = 0; i < _properties.Length; i++)
            {
                if (nestedResults[i] is not null)
                {
                    SetMemberValue(instance, _properties[i].MemberInfo, nestedResults[i]);
                }
            }
        }

        manager.CaptureState();

        // For tracked entity types: start tracking (skip for owned types with shadow keys — those
        // are tracked by the change tracker fixup when the owner is tracked)
        if (_isTracking && _structuralType is IEntityType trackedEntityType)
        {
            var primaryKey = trackedEntityType.FindPrimaryKey();
            if (primaryKey is null || !primaryKey.Properties.Any(p => p.IsShadowProperty()))
            {
                queryContext.StartTracking(trackedEntityType, instance, Snapshot.Empty);
            }
        }

        return instance;
    }

    /// <summary>
    ///     Processes a matched JSON property — either reads a scalar value or materializes a nested type.
    /// </summary>
    private void ProcessMatchedProperty(
        ref Utf8JsonReaderManager manager,
        QueryContext queryContext,
        JsonReaderData jsonReaderData,
        object[]? keyValues,
        int propertyIndex,
        object instance,
        ref object?[]? nestedResults)
    {
        ref readonly var handler = ref _properties[propertyIndex];
        manager.MoveNext();

        if (handler.JsonReaderWriter is not null)
        {
            // Scalar property
            ReadScalarProperty(ref manager, handler, instance);
        }
        else
        {
            // Nested structural type (complex property or owned navigation)
            manager.CaptureState();

            var nestedValue = handler.IsCollection
                ? MaterializeNestedCollection(queryContext, jsonReaderData, keyValues, handler)
                : handler.NestedMaterializer!.Materialize(queryContext, jsonReaderData, keyValues);

            nestedResults ??= new object?[_properties.Length];
            nestedResults[propertyIndex] = nestedValue;

            // Recreate manager — the inner materializer advanced the reader state
            manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
        }
    }

    /// <summary>
    ///     Materializes a JSON array into a collection. This is the entry point for top-level collection
    ///     complex properties on entities (called from <see cref="RelationalEntityMaterializer{TEntity}" />).
    /// </summary>
    public static object? MaterializeCollection(
        QueryContext queryContext,
        JsonReaderData jsonReaderData,
        object[]? keyValues,
        RelationalJsonStructuralTypeMaterializer elementMaterializer,
        IPropertyBase structuralProperty)
    {
        var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
        var tokenType = manager.CurrentReader.TokenType;

        switch (tokenType)
        {
            case JsonTokenType.Null:
                return null;

            case not JsonTokenType.StartArray:
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
        }

        var collectionAccessor = structuralProperty.GetCollectionAccessor()!;
        var collection = collectionAccessor.Create();

        object[]? childKeyValues = null;
        if (keyValues is not null)
        {
            childKeyValues = new object[keyValues.Length + 1];
            Array.Copy(keyValues, childKeyValues, keyValues.Length);
        }

        tokenType = manager.MoveNext();
        var index = 0;

        while (tokenType != JsonTokenType.EndArray)
        {
            childKeyValues?[^1] = ++index;

            if (tokenType is not JsonTokenType.StartObject)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            manager.CaptureState();
            var element = elementMaterializer.Materialize(queryContext, jsonReaderData, childKeyValues);

            if (element is not null)
            {
                collectionAccessor.AddStandalone(collection, element);
            }

            manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);

            if (manager.CurrentReader.TokenType != JsonTokenType.EndObject)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(manager.CurrentReader.TokenType.ToString()));
            }

            tokenType = manager.MoveNext();
        }

        manager.CaptureState();
        return collection;
    }

    private static void ReadScalarProperty(
        ref Utf8JsonReaderManager manager,
        in JsonPropertyHandler handler,
        object instance)
    {
        if (handler.IsNullable && manager.CurrentReader.TokenType == JsonTokenType.Null)
        {
            // Leave the property at its default value
            return;
        }

        var value = handler.JsonReaderWriter!.FromJson(ref manager, null);
        SetMemberValue(instance, handler.MemberInfo, value);
    }

    /// <summary>
    ///     Sets a property/field value on an instance using MemberInfo. This works correctly for
    ///     both reference types and boxed value types (structs).
    /// </summary>
    private static void SetMemberValue(object instance, MemberInfo memberInfo, object? value)
    {
        if (memberInfo is FieldInfo fieldInfo)
        {
            fieldInfo.SetValue(instance, value);
        }
        else
        {
            ((PropertyInfo)memberInfo).SetValue(instance, value);
        }
    }

    /// <summary>
    ///     Advances the reader past the current JSON object without materializing its contents.
    ///     Used when the entity is already tracked (from the identity map).
    /// </summary>
    private static void SkipJsonObject(ref Utf8JsonReaderManager manager)
    {
        // Skip all tokens until we reach the matching EndObject
        var depth = 1;
        while (depth > 0)
        {
            var tokenType = manager.MoveNext();
            switch (tokenType)
            {
                case JsonTokenType.StartObject or JsonTokenType.StartArray:
                    depth++;
                    break;
                case JsonTokenType.EndObject or JsonTokenType.EndArray:
                    depth--;
                    break;
            }
        }

        manager.CaptureState();
    }

    private static object? MaterializeNestedCollection(
        QueryContext queryContext,
        JsonReaderData jsonReaderData,
        object[]? keyValues,
        in JsonPropertyHandler handler)
    {
        var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
        var tokenType = manager.CurrentReader.TokenType;

        switch (tokenType)
        {
            case JsonTokenType.Null:
                return null;

            case not JsonTokenType.StartArray:
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
        }

        var collectionAccessor = handler.StructuralProperty!.GetCollectionAccessor()!;
        var collection = collectionAccessor.Create();

        // Extend key values with one more slot for the array index
        object[]? childKeyValues = null;
        if (keyValues is not null)
        {
            childKeyValues = new object[keyValues.Length + 1];
            Array.Copy(keyValues, childKeyValues, keyValues.Length);
        }

        tokenType = manager.MoveNext();
        var index = 0;

        while (tokenType != JsonTokenType.EndArray)
        {
            if (childKeyValues is not null)
            {
                childKeyValues[^1] = ++index;
            }

            if (tokenType == JsonTokenType.StartObject)
            {
                manager.CaptureState();
                var element = handler.NestedMaterializer!.Materialize(queryContext, jsonReaderData, childKeyValues);

                if (element is not null)
                {
                    collectionAccessor.AddStandalone(collection, element);
                }

                manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);

                if (manager.CurrentReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(manager.CurrentReader.TokenType.ToString()));
                }

                tokenType = manager.MoveNext();
            }
            else
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }
        }

        manager.CaptureState();
        return collection;
    }

    /// <summary>
    ///     Describes a JSON property handler — either a scalar property (with <see cref="JsonReaderWriter" />)
    ///     or a nested structural type (with <see cref="NestedMaterializer" />).
    /// </summary>
    internal readonly struct JsonPropertyHandler
    {
        /// <summary>The JSON property name as pre-encoded UTF-8 bytes.</summary>
        public byte[] JsonNameUtf8 { get; }

        /// <summary>The MemberInfo (field or property) to set on the instance.</summary>
        public MemberInfo MemberInfo { get; }

        /// <summary>
        ///     For scalar properties: the reader/writer for JSON deserialization.
        ///     Null for nested structural types.
        /// </summary>
        public JsonValueReaderWriter? JsonReaderWriter { get; }

        /// <summary>Whether the scalar property is nullable.</summary>
        public bool IsNullable { get; }

        /// <summary>
        ///     For nested structural types: the materializer for the nested type.
        ///     Null for scalar properties.
        /// </summary>
        public RelationalJsonStructuralTypeMaterializer? NestedMaterializer { get; }

        /// <summary>Whether the nested property is a collection.</summary>
        public bool IsCollection { get; }

        /// <summary>
        ///     For nested collection properties: the structural property metadata (needed for collection accessor).
        ///     Null for scalar properties and non-collection nested types.
        /// </summary>
        public IPropertyBase? StructuralProperty { get; }

        /// <summary>Creates a handler for a scalar property.</summary>
        public JsonPropertyHandler(
            byte[] jsonNameUtf8,
            JsonValueReaderWriter jsonReaderWriter,
            MemberInfo memberInfo,
            bool isNullable)
        {
            JsonNameUtf8 = jsonNameUtf8;
            MemberInfo = memberInfo;
            JsonReaderWriter = jsonReaderWriter;
            IsNullable = isNullable;
        }

        /// <summary>Creates a handler for a nested structural type.</summary>
        public JsonPropertyHandler(
            byte[] jsonNameUtf8,
            RelationalJsonStructuralTypeMaterializer nestedMaterializer,
            MemberInfo memberInfo,
            bool isCollection,
            IPropertyBase? structuralProperty)
        {
            JsonNameUtf8 = jsonNameUtf8;
            MemberInfo = memberInfo;
            NestedMaterializer = nestedMaterializer;
            IsCollection = isCollection;
            StructuralProperty = structuralProperty;
        }
    }
}
