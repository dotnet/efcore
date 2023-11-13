// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The base class for non-relational type mapping source. Non-relational providers
///         should derive from this class and override <see cref="O:TypeMappingSourceBase.FindMapping" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public abstract class TypeMappingSourceBase : ITypeMappingSource
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected TypeMappingSourceBase(TypeMappingSourceDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual TypeMappingSourceDependencies Dependencies { get; }

    /// <summary>
    ///     Overridden by database providers to find a type mapping for the given info.
    /// </summary>
    /// <remarks>
    ///     The mapping info is populated with as much information about the required type mapping as
    ///     is available. Use all the information necessary to create the best mapping. Return <see langword="null" />
    ///     if no mapping is available.
    /// </remarks>
    /// <param name="mappingInfo">The mapping info to use to create the mapping.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none could be found.</returns>
    protected virtual CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
    {
        foreach (var plugin in Dependencies.Plugins)
        {
            var typeMapping = plugin.FindMapping(mappingInfo);
            if (typeMapping != null)
            {
                return typeMapping;
            }
        }

        return null;
    }

    /// <summary>
    ///     Called after a mapping has been found so that it can be validated for the given property.
    /// </summary>
    /// <param name="mapping">The mapping, if any.</param>
    /// <param name="property">The property, if any.</param>
    protected virtual void ValidateMapping(
        CoreTypeMapping? mapping,
        IProperty? property)
    {
    }

    /// <summary>
    ///     Finds the type mapping for a given <see cref="IProperty" />.
    /// </summary>
    /// <remarks>
    ///     Note: providers should typically not need to override this method.
    /// </remarks>
    /// <param name="property">The property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(IProperty property);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="IElementType" />.
    /// </summary>
    /// <param name="elementType">The collection element.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(IElementType elementType);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" />
    ///     or <see cref="IModel" /> available, otherwise call <see cref="FindMapping(IProperty)" />
    ///     or <see cref="FindMapping(Type, IModel, CoreTypeMapping)" />
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(Type type);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />, taking pre-convention configuration into the account.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" />,
    ///     otherwise call <see cref="FindMapping(IProperty)" />.
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <param name="model">The model.</param>
    /// <param name="elementMapping">The element mapping to use, if known.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(Type type, IModel model, CoreTypeMapping? elementMapping = null);

    /// <inheritdoc/>
    public abstract CoreTypeMapping? FindMapping(MemberInfo member);

    /// <inheritdoc/>
    public virtual CoreTypeMapping? FindMapping(MemberInfo member, IModel model, bool useAttributes)
        => FindMapping(member);

    /// <summary>
    ///     Attempts to find a JSON-based type mapping for a collection of primitive types.
    /// </summary>
    /// <param name="mappingInfo">The mapping info being used.</param>
    /// <param name="modelClrType">The model CLR type.</param>
    /// <param name="providerClrType">The provider CLR type.</param>
    /// <param name="elementMapping">The type mapping for elements of the collection.</param>
    /// <param name="elementComparer">The element comparer.</param>
    /// <param name="collectionReaderWriter">The reader/writer for the collection.</param>
    /// <returns><see langword="true" /> if a collection mapping was found; <see langword="false" /> otherwise.</returns>
    protected virtual bool TryFindJsonCollectionMapping(
        TypeMappingInfo mappingInfo,
        Type modelClrType,
        Type? providerClrType,
        ref CoreTypeMapping? elementMapping,
        out ValueComparer? elementComparer,
        out JsonValueReaderWriter? collectionReaderWriter)
    {
        if ((providerClrType == null || providerClrType == typeof(string))
            && modelClrType.TryGetElementType(typeof(IEnumerable<>)) is { } elementType
            && elementType != modelClrType
            && !modelClrType.GetGenericTypeImplementations(typeof(IDictionary<,>)).Any())
        {
            elementMapping ??= FindMapping(elementType);

            if (elementMapping is { ElementTypeMapping: null, JsonValueReaderWriter: not null })
            {
                var elementReader = elementMapping.JsonValueReaderWriter!;

                if (elementReader.ValueType.IsNullableValueType()
                    || !elementReader.ValueType.IsAssignableFrom(elementType.UnwrapNullableType()))
                {
                    elementReader = (JsonValueReaderWriter)Activator.CreateInstance(
                        typeof(JsonCastValueReaderWriter<>).MakeGenericType(elementType.UnwrapNullableType()), elementReader)!;
                }

                var typeToInstantiate = FindTypeToInstantiate();

                collectionReaderWriter = mappingInfo.JsonValueReaderWriter
                    ?? (JsonValueReaderWriter?)Activator.CreateInstance(
                        (elementType.IsNullableValueType()
                            ? typeof(JsonNullableStructCollectionReaderWriter<,,>)
                            : typeof(JsonCollectionReaderWriter<,,>))
                        .MakeGenericType(modelClrType, typeToInstantiate, elementType.UnwrapNullableType()),
                        elementReader);

                elementComparer = (ValueComparer?)Activator.CreateInstance(
                    elementType.IsNullableValueType()
                        ? typeof(NullableValueTypeListComparer<>).MakeGenericType(elementType.UnwrapNullableType())
                        : elementMapping.Comparer.Type.IsAssignableFrom(elementType)
                            ? typeof(ListComparer<>).MakeGenericType(elementType)
                            : typeof(ObjectListComparer<>).MakeGenericType(elementType),
                    elementMapping.Comparer.ToNullableComparer(elementType)!);

                return true;

                Type FindTypeToInstantiate()
                {
                    if (modelClrType.IsArray)
                    {
                        return modelClrType;
                    }

                    var listOfT = typeof(List<>).MakeGenericType(elementType);

                    if (modelClrType.IsAssignableFrom(listOfT))
                    {
                        if (!modelClrType.IsAbstract)
                        {
                            var constructor = modelClrType.GetDeclaredConstructor(null);
                            if (constructor?.IsPublic == true)
                            {
                                return modelClrType;
                            }
                        }

                        return listOfT;
                    }

                    return modelClrType;
                }
            }
        }

        elementMapping = null;
        elementComparer = null;
        collectionReaderWriter = null;
        return false;
    }
}
