// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable 1574, CS0419 // Ambiguous reference in cref attribute
namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The base class for relational type mapping starting with version 2.1. Relational providers
    ///         should derive from this class and override <see cref="RelationalTypeMappingSource.FindMapping" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public abstract class RelationalTypeMappingSource : TypeMappingSourceBase, IRelationalTypeMappingSource
    {
        private readonly ConcurrentDictionary<(RelationalTypeMappingInfo, Type, ValueConverter), RelationalTypeMapping> _explicitMappings
            = new ConcurrentDictionary<(RelationalTypeMappingInfo, Type, ValueConverter), RelationalTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational-specific dependencies for this service. </param>
        protected RelationalTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
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
        ///         is available. Use all the information necessary to create the best mapping. Return <c>null</c>
        ///         if no mapping is available.
        ///     </para>
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <c>null</c> if none could be found. </returns>
        protected virtual RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
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
        /// <returns> The type mapping, or <c>null</c> if none could be found. </returns>
        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
            => throw new InvalidOperationException("FindMapping on a 'RelationalTypeMappingSource' with a non-relational 'TypeMappingInfo'.");

        private RelationalTypeMapping FindMappingWithConversion(
            in RelationalTypeMappingInfo mappingInfo,
            [CanBeNull] IReadOnlyList<IProperty> principals)
        {
            Type providerClrType = null;
            ValueConverter customConverter = null;
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

            var resolvedMapping = _explicitMappings.GetOrAdd(
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

            ValidateMapping(resolvedMapping, principals?[0]);

            return resolvedMapping;
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="IProperty" />.
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public override CoreTypeMapping FindMapping(IProperty property)
        {
            var mapping = property.FindRelationalMapping();
            if (mapping != null)
            {
                return mapping;
            }

            var principals = property.FindPrincipals();

            string storeTypeName = null;
            bool? isFixedLength = null;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < principals.Count; i++)
            {
                var principal = principals[i];
                if (storeTypeName == null)
                {
                    var columnType = (string)principal[RelationalAnnotationNames.ColumnType];
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
                new RelationalTypeMappingInfo(principals, storeTypeName, storeTypeNameBase, unicode, isFixedLength, size, precision, scale), principals);
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" />.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" />
        ///         or <see cref="MemberInfo" /> available, otherwise call <see cref="FindMapping(IProperty)" />
        ///         or <see cref="FindMapping(MemberInfo)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public override CoreTypeMapping FindMapping(Type type)
            => FindMappingWithConversion(new RelationalTypeMappingInfo(type), null);

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
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public override CoreTypeMapping FindMapping(MemberInfo member)
        {
            if (member.GetCustomAttribute<ColumnAttribute>(true) is ColumnAttribute attribute)
            {
                var storeTypeName = attribute.TypeName;
                var storeTypeNameBase = ParseStoreTypeName(attribute.TypeName, out var unicode, out var size, out var precision, out var scale);

                return FindMappingWithConversion(
                    new RelationalTypeMappingInfo(member, storeTypeName, storeTypeNameBase, unicode, size, precision, scale), null);
            }
            else
            {
                return FindMappingWithConversion(new RelationalTypeMappingInfo(member), null);
            }
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
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public virtual RelationalTypeMapping FindMapping(string storeTypeName)
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
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public virtual RelationalTypeMapping FindMapping(
            Type type,
            string storeTypeName,
            bool keyOrIndex = false,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
        {
            string storeTypeBaseName = null;

            if (storeTypeName != null)
            {
                storeTypeBaseName = ParseStoreTypeName(storeTypeName, out var parsedUnicode, out var parsedSize, out var parsedPrecision, out var parsedScale);
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
        RelationalTypeMapping IRelationalTypeMappingSource.FindMapping(IProperty property)
            => (RelationalTypeMapping)FindMapping(property);

        /// <inheritdoc />
        RelationalTypeMapping IRelationalTypeMappingSource.FindMapping(Type type)
            => (RelationalTypeMapping)FindMapping(type);

        /// <inheritdoc />
        RelationalTypeMapping IRelationalTypeMappingSource.FindMapping(MemberInfo member)
            => (RelationalTypeMapping)FindMapping(member);

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
        /// <param name="unicode"> The Unicode or ANSI setting parsed from the type name, or <c>null</c> if none was specified. </param>
        /// <param name="size"> The size parsed from the type name, or <c>null</c> if none was specified. </param>
        /// <param name="precision"> The precision parsed from the type name, or <c>null</c> if none was specified. </param>
        /// <param name="scale"> The scale parsed from the type name, or <c>null</c> if none was specified. </param>
        /// <returns> The provider-specific relational type name, with any facets removed. </returns>
        protected virtual string ParseStoreTypeName(
            [CanBeNull] string storeTypeName,
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
                    var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
                    if (closeParen > openParen)
                    {
                        var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
                        if (comma > openParen && comma < closeParen)
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
                        else if (int.TryParse(storeTypeName.Substring(openParen + 1, closeParen - openParen - 1).Trim(), out var parsedSize))
                        {
                            size = parsedSize;
                            precision = parsedSize;
                        }

                        return storeTypeName.Substring(0, openParen).Trim();
                    }
                }
            }

            return storeTypeName;
        }
    }
}
