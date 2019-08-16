// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable 1574, CS0419 // Ambiguous reference in cref attribute
namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The base class for non-relational type mapping starting with version 2.1. Non-relational providers
    ///         should derive from this class and override <see cref="TypeMappingSourceBase.FindMapping" />
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
    public abstract class TypeMappingSource : TypeMappingSourceBase
    {
        private readonly ConcurrentDictionary<(TypeMappingInfo, Type, ValueConverter), CoreTypeMapping> _explicitMappings
            = new ConcurrentDictionary<(TypeMappingInfo, Type, ValueConverter), CoreTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected TypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
        }

        private CoreTypeMapping FindMappingWithConversion(
            in TypeMappingInfo mappingInfo,
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
                                            mapping = mapping.Clone(secondConverterInfo.Create());
                                            break;
                                        }
                                    }
                                }

                                if (mapping != null)
                                {
                                    mapping = mapping.Clone(converterInfo.Create());
                                    break;
                                }
                            }
                        }
                    }

                    if (mapping != null
                        && converter != null)
                    {
                        mapping = mapping.Clone(converter);
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
            var mapping = property.FindTypeMapping();
            if (mapping != null)
            {
                return mapping;
            }

            var principals = property.FindPrincipals();
            return FindMappingWithConversion(new TypeMappingInfo(principals), principals);
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
            => FindMappingWithConversion(new TypeMappingInfo(type), null);

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
            => FindMappingWithConversion(new TypeMappingInfo(member), null);
    }
}
