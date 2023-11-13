// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The base class for non-relational type mapping. Non-relational providers
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
public abstract class TypeMappingSource : TypeMappingSourceBase
{
    private readonly ConcurrentDictionary<(TypeMappingInfo, Type?, ValueConverter?, CoreTypeMapping?), CoreTypeMapping?>
        _explicitMappings = new();

    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected TypeMappingSource(TypeMappingSourceDependencies dependencies)
        : base(dependencies)
    {
    }

    private CoreTypeMapping? FindMappingWithConversion(
        TypeMappingInfo mappingInfo,
        IReadOnlyList<IProperty>? principals)
    {
        Type? providerClrType = null;
        ValueConverter? customConverter = null;
        CoreTypeMapping? elementMapping = null;
        if (principals != null)
        {
            for (var i = 0; i < principals.Count; i++)
            {
                var principal = principals[i];
                if (providerClrType == null)
                {
                    var providerType = principal.GetProviderClrType();
                    if (providerType != null)
                    {
                        providerClrType = providerType.UnwrapNullableType();
                    }
                }

                if (customConverter == null)
                {
                    var converter = principal.GetValueConverter();
                    if (converter != null)
                    {
                        customConverter = converter;
                    }
                }

                if (elementMapping == null)
                {
                    var element = principal.GetElementType();
                    if (element != null)
                    {
                        elementMapping = FindMapping(element);
                        mappingInfo = mappingInfo with { ElementTypeMapping = elementMapping };
                    }
                }
            }
        }

        var resolvedMapping = FindMappingWithConversion(mappingInfo, providerClrType, customConverter);

        ValidateMapping(resolvedMapping, principals?[0]);

        return resolvedMapping;
    }

    private CoreTypeMapping? FindMappingWithConversion(
        TypeMappingInfo mappingInfo,
        Type? providerClrType,
        ValueConverter? customConverter)
        => _explicitMappings.GetOrAdd(
            (mappingInfo, providerClrType, customConverter, mappingInfo.ElementTypeMapping),
            static (k, self) =>
            {
                var (mappingInfo, providerClrType, customConverter, elementMapping) = k;

                var sourceType = mappingInfo.ClrType;
                var mapping = providerClrType == null
                    || providerClrType == mappingInfo.ClrType
                        ? self.FindMapping(mappingInfo)
                        : null;

                if (mapping == null)
                {
                    if (elementMapping == null
                        || customConverter != null)
                    {
                        if (sourceType != null)
                        {
                            foreach (var converterInfo in self.Dependencies
                                         .ValueConverterSelector
                                         .Select(sourceType, providerClrType))
                            {
                                var mappingInfoUsed = mappingInfo.WithConverter(converterInfo);
                                mapping = self.FindMapping(mappingInfoUsed);

                                if (mapping == null
                                    && providerClrType != null)
                                {
                                    foreach (var secondConverterInfo in self.Dependencies
                                                 .ValueConverterSelector
                                                 .Select(providerClrType))
                                    {
                                        mapping = self.FindMapping(mappingInfoUsed.WithConverter(secondConverterInfo));

                                        if (mapping != null)
                                        {
                                            mapping = mapping.WithComposedConverter(
                                                secondConverterInfo.Create(),
                                                jsonValueReaderWriter: mappingInfoUsed.JsonValueReaderWriter);
                                            break;
                                        }
                                    }
                                }

                                if (mapping != null)
                                {
                                    mapping = mapping.WithComposedConverter(
                                        converterInfo.Create(),
                                        jsonValueReaderWriter: mappingInfo.JsonValueReaderWriter);
                                    break;
                                }
                            }

                            mapping ??= self.FindCollectionMapping(mappingInfo, sourceType, providerClrType, elementMapping);
                        }
                    }
                    else if (sourceType != null)
                    {
                        mapping = self.FindCollectionMapping(mappingInfo, sourceType, providerClrType, elementMapping);
                    }
                }

                if (mapping != null
                    && customConverter != null)
                {
                    mapping = mapping.WithComposedConverter(
                        customConverter,
                        jsonValueReaderWriter: mappingInfo.JsonValueReaderWriter);
                }

                return mapping;
            },
            this);

    /// <summary>
    ///     Attempts to find a type mapping for a collection of primitive types.
    /// </summary>
    /// <param name="info">The mapping info being used.</param>
    /// <param name="modelType">The model type.</param>
    /// <param name="providerType">The provider type.</param>
    /// <param name="elementMapping">The element mapping, if known.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    [EntityFrameworkInternal]
    protected virtual CoreTypeMapping? FindCollectionMapping(
        TypeMappingInfo info,
        Type modelType,
        Type? providerType,
        CoreTypeMapping? elementMapping)
        => TryFindJsonCollectionMapping(
            info, modelType, providerType, ref elementMapping, out var comparer, out var collectionReaderWriter)
            ? FindMapping(
                    info.WithConverter(
                        // Note that the converter info is only used temporarily here and never creates an instance.
                        new ValueConverterInfo(modelType, typeof(string), _ => null!)))!
                .WithComposedConverter(
                    (ValueConverter)Activator.CreateInstance(
                        typeof(CollectionToJsonStringConverter<>).MakeGenericType(modelType.TryGetElementType(typeof(IEnumerable<>))!),
                        collectionReaderWriter!)!,
                    comparer,
                    comparer,
                    elementMapping,
                    collectionReaderWriter)
            : null;

    /// <summary>
    ///     Finds the type mapping for a given <see cref="IProperty" />.
    /// </summary>
    /// <remarks>
    ///     Note: providers should typically not need to override this method.
    /// </remarks>
    /// <param name="property">The property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public override CoreTypeMapping? FindMapping(IProperty property)
    {
        var principals = property.GetPrincipals();
        return FindMappingWithConversion(new TypeMappingInfo(principals), principals);
    }

    /// <summary>
    ///     Finds the type mapping for a given <see cref="IElementType" />.
    /// </summary>
    /// <remarks>
    ///     Note: providers should typically not need to override this method.
    /// </remarks>
    /// <param name="elementType">The property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public override CoreTypeMapping? FindMapping(IElementType elementType)
    {
        var resolvedMapping = FindMappingWithConversion(
            new TypeMappingInfo(elementType),
            providerClrType: elementType.GetProviderClrType(),
            customConverter: elementType.GetValueConverter());

        ValidateMapping(resolvedMapping, null);

        return resolvedMapping;
    }

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note: Only call this method if there is no <see cref="IProperty" />
    ///         or <see cref="IModel" /> available, otherwise call <see cref="FindMapping(IProperty)" />
    ///         or <see cref="FindMapping(Type, IModel, CoreTypeMapping)" />
    ///     </para>
    ///     <para>
    ///         Note: providers should typically not need to override this method.
    ///     </para>
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public override CoreTypeMapping? FindMapping(Type type)
        => FindMappingWithConversion(new TypeMappingInfo(type), null);

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
    public override CoreTypeMapping? FindMapping(Type type, IModel model, CoreTypeMapping? elementMapping = null)
    {
        type = type.UnwrapNullableType();
        var typeConfiguration = model.FindTypeMappingConfiguration(type);
        if (typeConfiguration != null)
        {
            var mappingInfo = new TypeMappingInfo(type, typeConfiguration, elementMapping);
            var providerClrType = typeConfiguration.GetProviderClrType()?.UnwrapNullableType();
            return FindMappingWithConversion(mappingInfo, providerClrType, customConverter: typeConfiguration.GetValueConverter());
        }

        return FindMappingWithConversion(
            new TypeMappingInfo(type, elementMapping),
            providerClrType: null,
            customConverter: null);
    }

    /// <inheritdoc/>
    public override CoreTypeMapping? FindMapping(MemberInfo member)
        => FindMappingWithConversion(new TypeMappingInfo(member), null, null);

    /// <inheritdoc/>
    public override CoreTypeMapping? FindMapping(MemberInfo member, IModel model, bool useAttributes)
        => FindMappingWithConversion(new TypeMappingInfo(member), null, null);
}
