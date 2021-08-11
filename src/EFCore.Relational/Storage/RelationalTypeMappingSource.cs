// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable 1574, CS0419 // Ambiguous reference in cref attribute
namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The base class for relational type mapping source. Relational providers
    ///         should derive from this class and override <see cref="RelationalTypeMappingSource.FindMapping" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public abstract class RelationalTypeMappingSource : TypeMappingSourceBase, IRelationalTypeMappingSource
    {
        private readonly ConcurrentDictionary<(RelationalTypeMappingInfo, Type?, ValueConverter?), RelationalTypeMapping?> _explicitMappings
            = new();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational-specific dependencies for this service. </param>
        protected RelationalTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     <para>
        ///         Overridden by relational database providers to find a type mapping for the given info.
        ///     </para>
        ///     <para>
        ///         The mapping info is populated with as much information about the required type mapping as
        ///         is available. Use all the information necessary to create the best mapping. Return <see langword="null" />
        ///         if no mapping is available.
        ///     </para>
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none could be found. </returns>
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
        ///     Dependencies used to create this <see cref="RelationalTypeMappingSource" />
        /// </summary>
        protected virtual RelationalTypeMappingSourceDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Call <see cref="RelationalTypeMappingSource.FindMapping" /> instead
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none could be found. </returns>
        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
            => throw new InvalidOperationException(
                RelationalStrings.NoneRelationalTypeMappingOnARelationalTypeMappingSource);

        private RelationalTypeMapping? FindMappingWithConversion(
            in RelationalTypeMappingInfo mappingInfo,
            IReadOnlyList<IProperty>? principals)
        {
            Type? providerClrType = null;
            ValueConverter? customConverter = null;
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
                }
            }

            var resolvedMapping = FindMappingWithConversion(mappingInfo, providerClrType, customConverter);

            ValidateMapping(resolvedMapping, principals?[0]);

            return resolvedMapping;
        }

        private RelationalTypeMapping? FindMappingWithConversion(RelationalTypeMappingInfo mappingInfo, Type? providerClrType, ValueConverter? customConverter)
            => _explicitMappings.GetOrAdd(
                (mappingInfo, providerClrType, customConverter),
                k =>
                {
                    var (info, providerType, converter) = k;
                    var mapping = providerType == null
                        || providerType == info.ClrType
                            ? FindMapping(info)
                            : null;

                    if (mapping == null)
                    {
                        var sourceType = info.ClrType;

                        if (sourceType != null)
                        {
                            foreach (var converterInfo in Dependencies
                                .ValueConverterSelector
                                .Select(sourceType, providerType))
                            {
                                var mappingInfoUsed = info.WithConverter(converterInfo);
                                mapping = FindMapping(mappingInfoUsed);

                                if (mapping == null
                                    && providerType != null)
                                {
                                    foreach (var secondConverterInfo in Dependencies
                                        .ValueConverterSelector
                                        .Select(providerType))
                                    {
                                        mapping = FindMapping(mappingInfoUsed.WithConverter(secondConverterInfo));

                                        if (mapping != null)
                                        {
                                            mapping = (RelationalTypeMapping)mapping.Clone(secondConverterInfo.Create());
                                            break;
                                        }
                                    }
                                }

                                if (mapping != null)
                                {
                                    mapping = (RelationalTypeMapping)mapping.Clone(converterInfo.Create());
                                    break;
                                }
                            }
                        }
                    }

                    if (mapping != null
                        && converter != null)
                    {
                        mapping = (RelationalTypeMapping)mapping.Clone(converter);
                    }

                    return mapping;
                });

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="IProperty" />.
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
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

                if (isFixedLength == null)
                {
                    isFixedLength = principal.IsFixedLength();
                }
            }

            var storeTypeNameBase = ParseStoreTypeName(storeTypeName, out var unicode, out var size, out var precision, out var scale);

            return FindMappingWithConversion(
                new RelationalTypeMappingInfo(principals, storeTypeName, storeTypeNameBase, unicode, isFixedLength, size, precision, scale),
                principals);
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" />.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" />
        ///         or <see cref="IModel" /> available, otherwise call <see cref="FindMapping(IProperty)" />
        ///         or <see cref="FindMapping(Type, IModel)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public override CoreTypeMapping? FindMapping(Type type)
            => FindMappingWithConversion(new RelationalTypeMappingInfo(type), null);

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" />, taking pre-convention configuration into the account.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" />,
        ///         otherwise call <see cref="FindMapping(IProperty)" />.
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <param name="model"> The model. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public override CoreTypeMapping? FindMapping(Type type, IModel model)
        {
            var typeConfigurations = model.FindPropertyTypeConfigurations(type);
            var mappingInfo = new RelationalTypeMappingInfo(type);
            Type? providerClrType = null;
            ValueConverter? customConverter = null;
            if (typeConfigurations != null)
            {
                foreach (var typeConfiguration in typeConfigurations)
                {
                    if (providerClrType == null)
                    {
                        var providerType = typeConfiguration.GetProviderClrType();
                        if (providerType != null)
                        {
                            providerClrType = providerType.UnwrapNullableType();
                        }
                    }

                    var isUnicode = typeConfiguration.IsUnicode();
                    var scale = typeConfiguration.GetScale();
                    var precision = typeConfiguration.GetPrecision();
                    var size = typeConfiguration.GetMaxLength();

                    if (mappingInfo.StoreTypeName == null)
                    {
                        var storeTypeName = (string?)typeConfiguration[RelationalAnnotationNames.ColumnType];
                        if (storeTypeName != null)
                        {
                            var storeTypeBaseName = ParseStoreTypeName(
                                storeTypeName, out var parsedUnicode, out var parsedSize, out var parsedPrecision, out var parsedScale);

                            mappingInfo = mappingInfo with { StoreTypeName = storeTypeName };

                            if (mappingInfo.StoreTypeNameBase == null)
                            {
                                mappingInfo = mappingInfo with { StoreTypeNameBase = storeTypeBaseName };
                            }

                            if (size == null)
                            {
                                size = parsedSize;
                            }

                            if (precision == null)
                            {
                                precision = parsedPrecision;
                            }

                            if (scale == null)
                            {
                                scale = parsedScale;
                            }

                            if (isUnicode == null)
                            {
                                isUnicode = parsedUnicode;
                            }
                        }
                    }

                    if (mappingInfo.IsUnicode == null
                        && isUnicode != null)
                    {
                        mappingInfo = mappingInfo with { IsUnicode = isUnicode };
                    }

                    if (mappingInfo.Scale == null
                        && scale != null)
                    {
                        mappingInfo = mappingInfo with { Scale = scale };
                    }

                    if (mappingInfo.Precision == null
                        && precision != null)
                    {
                        mappingInfo = mappingInfo with { Precision = precision };
                    }

                    if (mappingInfo.Size == null
                        && size != null)
                    {
                        mappingInfo = mappingInfo with { Size = size };
                    }

                    if (mappingInfo.IsFixedLength == null)
                    {
                        var isFixedLength = (bool?)typeConfiguration[RelationalAnnotationNames.IsFixedLength];
                        if (isFixedLength != null)
                        {
                            mappingInfo = mappingInfo with { IsFixedLength = isFixedLength };
                        }
                    }
                }

                var firstConfiguration = typeConfigurations.FirstOrDefault();
                customConverter = firstConfiguration?.ClrType == type
                    ? firstConfiguration.GetValueConverter()
                    : null;
            }

            return FindMappingWithConversion(mappingInfo, providerClrType, customConverter);
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="MemberInfo" /> representing
        ///         a field or a property of a CLR type.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="member"> The field or property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public override CoreTypeMapping? FindMapping(MemberInfo member)
        {
            if (member.GetCustomAttribute<ColumnAttribute>(true) is ColumnAttribute attribute)
            {
                var storeTypeName = attribute.TypeName;
                var storeTypeNameBase = ParseStoreTypeName(
                    attribute.TypeName, out var unicode, out var size, out var precision, out var scale);

                return FindMappingWithConversion(
                    new RelationalTypeMappingInfo(member, storeTypeName, storeTypeNameBase, unicode, size, precision, scale), null);
            }

            return FindMappingWithConversion(new RelationalTypeMappingInfo(member), null);
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given database type name.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public virtual RelationalTypeMapping? FindMapping(string storeTypeName)
        {
            var storeTypeBaseName = ParseStoreTypeName(storeTypeName, out var unicode, out var size, out var precision, out var scale);

            return FindMappingWithConversion(
                new RelationalTypeMappingInfo(storeTypeName, storeTypeBaseName, unicode, size, precision, scale), null);
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" /> and additional facets.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <param name="keyOrIndex"> If <see langword="true" />, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <see langword="null" /> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <see langword="null" /> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <see langword="null" /> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <see langword="null" /> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <see langword="null" /> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <see langword="null" /> for default. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
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
                storeTypeBaseName = ParseStoreTypeName(
                    storeTypeName, out var parsedUnicode, out var parsedSize, out var parsedPrecision, out var parsedScale);
                if (size == null)
                {
                    size = parsedSize;
                }

                if (precision == null)
                {
                    precision = parsedPrecision;
                }

                if (scale == null)
                {
                    scale = parsedScale;
                }

                if (unicode == null)
                {
                    unicode = parsedUnicode;
                }
            }

            return FindMappingWithConversion(
                new RelationalTypeMappingInfo(
                    type, storeTypeName, storeTypeBaseName, keyOrIndex, unicode, size, rowVersion, fixedLength, precision, scale), null);
        }

        /// <inheritdoc />
        RelationalTypeMapping? IRelationalTypeMappingSource.FindMapping(IProperty property)
            => (RelationalTypeMapping?)FindMapping(property);

        /// <inheritdoc />
        RelationalTypeMapping? IRelationalTypeMappingSource.FindMapping(Type type)
            => (RelationalTypeMapping?)FindMapping(type);

        /// <inheritdoc />
        RelationalTypeMapping? IRelationalTypeMappingSource.FindMapping(Type type, IModel model)
            => (RelationalTypeMapping?)FindMapping(type);

        /// <inheritdoc />
        RelationalTypeMapping? IRelationalTypeMappingSource.FindMapping(MemberInfo member)
            => (RelationalTypeMapping?)FindMapping(member);

        /// <summary>
        ///     <para>
        ///         Parses a provider-specific store type name, extracting the standard facets
        ///         (e.g. size, precision) and returns the base store type name (without any facets).
        ///     </para>
        ///     <para>
        ///         The default implementation supports sometype(size), sometype(precision) and
        ///         sometype(precision, scale). Providers can override this to provide their own
        ///         logic.
        ///     </para>
        /// </summary>
        /// <param name="storeTypeName"> A provider-specific relational type name, including facets. </param>
        /// <param name="unicode"> The Unicode or ANSI setting parsed from the type name, or <see langword="null" /> if none was specified. </param>
        /// <param name="size"> The size parsed from the type name, or <see langword="null" /> if none was specified. </param>
        /// <param name="precision"> The precision parsed from the type name, or <see langword="null" /> if none was specified. </param>
        /// <param name="scale"> The scale parsed from the type name, or <see langword="null" /> if none was specified. </param>
        /// <returns> The provider-specific relational type name, with any facets removed. </returns>
        [return: NotNullIfNotNull("storeTypeName")]
        protected virtual string? ParseStoreTypeName(
            string? storeTypeName,
            out bool? unicode,
            out int? size,
            out int? precision,
            out int? scale)
        {
            unicode = null;
            size = null;
            precision = null;
            scale = null;

            if (storeTypeName != null)
            {
                var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);
                if (openParen > 0)
                {
                    var storeTypeNameBase = storeTypeName.Substring(0, openParen).Trim();
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
}
