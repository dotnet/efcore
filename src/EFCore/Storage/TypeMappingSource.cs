// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The base class for non-relational type mapping starting with version 2.1. Non-relational providers
    ///         should derive from this class and override <see cref="FindMapping(TypeMappingInfo)" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class TypeMappingSource : ITypeMappingSource
    {
        private readonly ConcurrentDictionary<TypeMappingInfo, CoreTypeMapping> _explicitMappings
            = new ConcurrentDictionary<TypeMappingInfo, CoreTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected TypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create this <see cref="TypeMappingSource" />
        /// </summary>
        protected virtual TypeMappingSourceDependencies Dependencies { get; }

        /// <summary>
        ///     <para>
        ///         Overridden by database providers to find a type mapping for the given info.
        ///     </para>
        ///     <para>
        ///         The mapping info is populated with as much information about the required type mapping as
        ///         is available. Use all the information necessary to create the best mapping. Return <c>null</c>
        ///         if no mapping is available.
        ///     </para>
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <c>null</c> if none could be found. </returns>
        protected abstract CoreTypeMapping FindMapping([NotNull] TypeMappingInfo mappingInfo);

        /// <summary>
        ///     <para>
        ///         Uses any available <see cref="ValueConverter" /> to help find a mapping that works.
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="mappingInfo"> The mapping info. </param>
        /// <returns> The type mapping with conversions applied, or <c>null</c> if none could be found. </returns>
        protected virtual CoreTypeMapping FindMappingWithConversion([NotNull] TypeMappingInfo mappingInfo)
        {
            Check.NotNull(mappingInfo, nameof(mappingInfo));

            return _explicitMappings.GetOrAdd(
                mappingInfo,
                k =>
                {
                    var mappingInfoUsed = mappingInfo;

                    var mapping = mappingInfoUsed.ConfiguredProviderClrType == null
                                  || mappingInfoUsed.ConfiguredProviderClrType == mappingInfoUsed.ModelClrType
                        ? FindMapping(mappingInfoUsed)
                        : null;

                    if (mapping == null)
                    {
                        var sourceType = mappingInfo.ValueConverterInfo?.ProviderClrType ?? mappingInfo.ModelClrType;

                        if (sourceType != null)
                        {
                            foreach (var converterInfo in Dependencies
                                .ValueConverterSelector
                                .Select(sourceType, mappingInfo.ConfiguredProviderClrType))
                            {
                                mappingInfoUsed = mappingInfo.WithBuiltInConverter(converterInfo);
                                mapping = FindMapping(mappingInfoUsed);

                                if (mapping == null
                                    && mappingInfo.ConfiguredProviderClrType != null)
                                {
                                    foreach (var secondConverterInfo in Dependencies
                                        .ValueConverterSelector
                                        .Select(mappingInfo.ConfiguredProviderClrType))
                                    {
                                        var secondMappingInfoUsed = mappingInfoUsed.WithBuiltInConverter(secondConverterInfo);
                                        mapping = FindMapping(secondMappingInfoUsed);

                                        if (mapping != null)
                                        {
                                            mapping = mapping.Clone(secondMappingInfoUsed.ValueConverterInfo?.Create());
                                            break;
                                        }
                                    }
                                }

                                if (mapping != null)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (mapping != null
                        && mappingInfoUsed.ValueConverterInfo != null)
                    {
                        mapping = mapping.Clone(mappingInfoUsed.ValueConverterInfo?.Create());
                    }

                    return mapping;
                });
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
        public virtual CoreTypeMapping FindMapping(IProperty property)
            => property.FindMapping()
               ?? FindMappingWithConversion(new ConcreteTypeMappingInfo(property));

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
        public virtual CoreTypeMapping FindMapping(Type type)
            => FindMappingWithConversion(new ConcreteTypeMappingInfo(type));

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
        public virtual CoreTypeMapping FindMapping(MemberInfo member)
            => FindMappingWithConversion(new ConcreteTypeMappingInfo(member));

        private sealed class ConcreteTypeMappingInfo : TypeMappingInfo
        {
            public ConcreteTypeMappingInfo([NotNull] IProperty property)
                : base(property)
            {
            }

            public ConcreteTypeMappingInfo([NotNull] Type type)
                : base(type)
            {
            }

            public ConcreteTypeMappingInfo([NotNull] MemberInfo member)
                : base(member)
            {
            }

            private ConcreteTypeMappingInfo(ConcreteTypeMappingInfo source, ValueConverterInfo builtInConverter)
                : base(source, builtInConverter)
            {
            }

            public override TypeMappingInfo WithBuiltInConverter(ValueConverterInfo converterInfo)
                => new ConcreteTypeMappingInfo(this, converterInfo);
        }
    }
}
