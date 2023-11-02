// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The base class for relational type mapping source. Relational providers
///         should derive from this class and override <see cref="FindMapping(in RelationalTypeMappingInfo)" />
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
public abstract class RelationalTypeMappingSource : TypeMappingSourceBase, IRelationalTypeMappingSource
{
    private readonly ConcurrentDictionary<(RelationalTypeMappingInfo, Type?, ValueConverter?, CoreTypeMapping?), RelationalTypeMapping?>
        _explicitMappings = new();

    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="relationalDependencies">Parameter object containing relational-specific dependencies for this service.</param>
    protected RelationalTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Overridden by relational database providers to find a type mapping for the given info.
    /// </summary>
    /// <remarks>
    ///     The mapping info is populated with as much information about the required type mapping as
    ///     is available. Use all the information necessary to create the best mapping. Return <see langword="null" />
    ///     if no mapping is available.
    /// </remarks>
    /// <param name="mappingInfo">The mapping info to use to create the mapping.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none could be found.</returns>
    protected virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        foreach (var plugin in RelationalDependencies.Plugins)
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
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalTypeMappingSourceDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Call <see cref="FindMapping(in RelationalTypeMappingInfo)" /> instead
    /// </summary>
    /// <param name="mappingInfo">The mapping info to use to create the mapping.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none could be found.</returns>
    protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
        => throw new InvalidOperationException(
            RelationalStrings.NoneRelationalTypeMappingOnARelationalTypeMappingSource);

    private RelationalTypeMapping? FindMappingWithConversion(
        RelationalTypeMappingInfo mappingInfo,
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
                        mappingInfo = mappingInfo with { ElementTypeMapping = (RelationalTypeMapping?)elementMapping };
                    }
                }
            }
        }

        var resolvedMapping = FindMappingWithConversion(mappingInfo, providerClrType, customConverter);

        ValidateMapping(resolvedMapping, principals?[0]);

        return resolvedMapping;
    }

    private RelationalTypeMapping? FindMappingWithConversion(
        RelationalTypeMappingInfo mappingInfo,
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
                                            mapping = (RelationalTypeMapping)mapping.WithComposedConverter(
                                                secondConverterInfo.Create(),
                                                jsonValueReaderWriter: mappingInfoUsed.JsonValueReaderWriter);
                                            break;
                                        }
                                    }
                                }

                                if (mapping != null)
                                {
                                    mapping = (RelationalTypeMapping)mapping.WithComposedConverter(
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
                    mapping = (RelationalTypeMapping)mapping.WithComposedConverter(
                        customConverter,
                        jsonValueReaderWriter: mappingInfo.JsonValueReaderWriter);
                }

                return mapping;
            },
            this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual RelationalTypeMapping? FindCollectionMapping(
        RelationalTypeMappingInfo info,
        Type modelType,
        Type? providerType,
        CoreTypeMapping? elementMapping)
        => TryFindJsonCollectionMapping(
            info.CoreTypeMappingInfo, modelType, providerType, ref elementMapping, out var comparer, out var collectionReaderWriter)
            ? (RelationalTypeMapping)FindMapping(
                    info.WithConverter(
                        // Note that the converter info is only used temporarily here and never creates an instance.
                        new ValueConverterInfo(modelType, typeof(string), _ => null!)))!
                .WithComposedConverter(
                    (ValueConverter)Activator.CreateInstance(
                        typeof(CollectionToJsonStringConverter<>).MakeGenericType(
                            modelType.TryGetElementType(typeof(IEnumerable<>))!), collectionReaderWriter!)!,
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

        string? storeTypeName = null;
        bool? isFixedLength = null;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < principals.Count; i++)
        {
            var principal = principals[i];
            if (storeTypeName == null)
            {
                var columnType = (string?)principal[RelationalAnnotationNames.ColumnType];
                if (columnType != null)
                {
                    storeTypeName = columnType;
                }
            }

            isFixedLength ??= principal.IsFixedLength();
        }

        bool? unicode = null;
        int? size = null;
        int? precision = null;
        int? scale = null;
        var storeTypeNameBase = ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

        return FindMappingWithConversion(
            new RelationalTypeMappingInfo(principals, storeTypeName, storeTypeNameBase, unicode, isFixedLength, size, precision, scale),
            principals);
    }

    /// <summary>
    ///     Finds the type mapping for the given <see cref="IElementType" />.
    /// </summary>
    /// <remarks>
    ///     Note: providers should typically not need to override this method.
    /// </remarks>
    /// <param name="elementType">The collection element.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public override CoreTypeMapping? FindMapping(IElementType elementType)
    {
        var storeTypeName = (string?)elementType[RelationalAnnotationNames.StoreType];
        var isFixedLength = elementType.IsFixedLength();
        bool? unicode = null;
        int? size = null;
        int? precision = null;
        int? scale = null;
        var storeTypeNameBase = ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);
        var providerClrType = elementType.GetProviderClrType();
        var customConverter = elementType.GetValueConverter();

        var resolvedMapping = FindMappingWithConversion(
            new RelationalTypeMappingInfo(elementType, storeTypeName, storeTypeNameBase, unicode, isFixedLength, size, precision, scale),
            providerClrType, customConverter);

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
    ///         or <see cref="FindMapping(Type, IModel, CoreTypeMapping?)" />
    ///     </para>
    ///     <para>
    ///         Note: providers should typically not need to override this method.
    ///     </para>
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public override RelationalTypeMapping? FindMapping(Type type)
        => FindMappingWithConversion(new RelationalTypeMappingInfo(type), null);

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
    public override RelationalTypeMapping? FindMapping(Type type, IModel model, CoreTypeMapping? elementMapping = null)
    {
        type = type.UnwrapNullableType();
        var typeConfiguration = model.FindTypeMappingConfiguration(type);
        if (typeConfiguration != null)
        {
            bool? unicode = null;
            int? scale = null;
            int? precision = null;
            int? size = null;
            string? storeTypeNameBase = null;
            var storeTypeName = (string?)typeConfiguration[RelationalAnnotationNames.ColumnType];
            if (storeTypeName != null)
            {
                storeTypeNameBase = ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);
            }

            var mappingInfo = new RelationalTypeMappingInfo(type, typeConfiguration, (RelationalTypeMapping?)elementMapping,
                storeTypeName, storeTypeNameBase, unicode, size, precision, scale);
            var providerClrType = typeConfiguration.GetProviderClrType()?.UnwrapNullableType();
            return FindMappingWithConversion(mappingInfo, providerClrType, customConverter: typeConfiguration.GetValueConverter());
        }

        return FindMappingWithConversion(
            new RelationalTypeMappingInfo(type, (RelationalTypeMapping?)elementMapping),
            providerClrType: null,
            customConverter: null);
    }

    /// <inheritdoc/>
    public override RelationalTypeMapping? FindMapping(MemberInfo member)
    {
        if (member.GetCustomAttribute<ColumnAttribute>(true) is ColumnAttribute attribute)
        {
            var storeTypeName = attribute.TypeName;
            bool? unicode = null;
            int? size = null;
            int? precision = null;
            int? scale = null;
            var storeTypeNameBase = ParseStoreTypeName(
                attribute.TypeName, ref unicode, ref size, ref precision, ref scale);

            return FindMappingWithConversion(
                new RelationalTypeMappingInfo(member, null, storeTypeName, storeTypeNameBase, unicode, size, precision, scale), null);
        }

        return FindMappingWithConversion(new RelationalTypeMappingInfo(member), null, null);
    }

    /// <inheritdoc/>
    public override RelationalTypeMapping? FindMapping(MemberInfo member, IModel model, bool useAttributes)
    {
        if (useAttributes
            && member.GetCustomAttribute<ColumnAttribute>(true) is ColumnAttribute attribute)
        {
            var storeTypeName = attribute.TypeName;
            bool? unicode = null;
            int? size = null;
            int? precision = null;
            int? scale = null;
            var storeTypeNameBase = ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

            return FindMappingWithConversion(
                new RelationalTypeMappingInfo(member, null, storeTypeName, storeTypeNameBase, unicode, size, precision, scale), null);
        }

        return FindMappingWithConversion(new RelationalTypeMappingInfo(member), null, null);
    }

    /// <summary>
    ///     Finds the type mapping for a given database type name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
    ///         call <see cref="FindMapping(IProperty)" />
    ///     </para>
    ///     <para>
    ///         Note: providers should typically not need to override this method.
    ///     </para>
    /// </remarks>
    /// <param name="storeTypeName">The database type name.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public virtual RelationalTypeMapping? FindMapping(string storeTypeName)
    {
        bool? unicode = null;
        int? size = null;
        int? precision = null;
        int? scale = null;
        var storeTypeBaseName = ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

        return FindMappingWithConversion(
            new RelationalTypeMappingInfo(storeTypeName, storeTypeBaseName, unicode, size, precision, scale), null);
    }

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" /> and additional facets.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
    ///         call <see cref="FindMapping(IProperty)" />
    ///     </para>
    ///     <para>
    ///         Note: providers should typically not need to override this method.
    ///     </para>
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <param name="storeTypeName">The database type name.</param>
    /// <param name="keyOrIndex">If <see langword="true" />, then a special mapping for a key or index may be returned.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="rowVersion">Specifies a row-version, or <see langword="null" /> for default.</param>
    /// <param name="fixedLength">Specifies a fixed length mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public virtual RelationalTypeMapping? FindMapping(
        Type type,
        string? storeTypeName,
        bool keyOrIndex = false,
        bool? unicode = null,
        int? size = null,
        bool? rowVersion = null,
        bool? fixedLength = null,
        int? precision = null,
        int? scale = null)
    {
        string? storeTypeBaseName = null;

        if (storeTypeName != null)
        {
            storeTypeBaseName = ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);
        }

        return FindMappingWithConversion(
            new RelationalTypeMappingInfo(
                type, null, storeTypeName, storeTypeBaseName, keyOrIndex, unicode, size, rowVersion, fixedLength, precision, scale), null);
    }

    /// <inheritdoc />
    RelationalTypeMapping? IRelationalTypeMappingSource.FindMapping(IProperty property)
        => (RelationalTypeMapping?)FindMapping(property);

    /// <summary>
    ///     Parses a provider-specific store type name, extracting the standard facets
    ///     (e.g. size, precision) and returns the base store type name (without any facets).
    /// </summary>
    /// <remarks>
    ///     The default implementation supports sometype(size), sometype(precision) and
    ///     sometype(precision, scale). Providers can override this to provide their own
    ///     logic.
    /// </remarks>
    /// <param name="storeTypeName">A provider-specific relational type name, including facets.</param>
    /// <param name="unicode">The Unicode or ANSI setting parsed from the type name, or <see langword="null" /> if none was specified.</param>
    /// <param name="size">The size parsed from the type name, or <see langword="null" /> if none was specified.</param>
    /// <param name="precision">The precision parsed from the type name, or <see langword="null" /> if none was specified.</param>
    /// <param name="scale">The scale parsed from the type name, or <see langword="null" /> if none was specified.</param>
    /// <returns>The provider-specific relational type name, with any facets removed.</returns>
    [return: NotNullIfNotNull("storeTypeName")]
    protected virtual string? ParseStoreTypeName(
        string? storeTypeName,
        ref bool? unicode,
        ref int? size,
        ref int? precision,
        ref int? scale)
    {
        if (storeTypeName != null)
        {
            var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                var storeTypeNameBase = storeTypeName[..openParen].Trim();
                var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
                if (closeParen > openParen)
                {
                    var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
                    if (comma > openParen
                        && comma < closeParen)
                    {
                        if (int.TryParse(storeTypeName.Substring(openParen + 1, comma - openParen - 1), out var parsedPrecision))
                        {
                            precision = parsedPrecision;
                        }

                        if (int.TryParse(storeTypeName.Substring(comma + 1, closeParen - comma - 1), out var parsedScale))
                        {
                            scale = parsedScale;
                        }
                    }
                    else if (int.TryParse(
                                 storeTypeName.Substring(openParen + 1, closeParen - openParen - 1).Trim(), out var parsedSize))
                    {
                        size = parsedSize;
                    }

                    return storeTypeNameBase;
                }
            }
        }

        return storeTypeName;
    }
}
